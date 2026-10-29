################################################################################
# General Informations
################################################################################

cmake_minimum_required(VERSION 3.1)
project(ElasticRods)

# CMP0063: Honor visibility properties for all target types.
if(POLICY CMP0063)
    cmake_policy(SET CMP0063 NEW)
endif()

################################################################################

set(THIRD_PARTY_DIR ${CMAKE_CURRENT_SOURCE_DIR}/3rdparty)
list(APPEND CMAKE_MODULE_PATH ${CMAKE_CURRENT_SOURCE_DIR}/cmake)

if(NOT CMAKE_CONFIGURATION_TYPES AND NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE Release CACHE STRING "Build configuration" FORCE)
endif()

# Export compile flags(used for autocompletion of the C++ code)
set(CMAKE_EXPORT_COMPILE_COMMANDS 1)

# CMake plugin for vscode
include(CMakeToolsHelpers OPTIONAL)

# Enable more warnings
if(NOT MSVC)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -pedantic -Wno-comment")
endif()

# We need -fPIC when compiling our libraries and our dependencies for
# the python bindings to link.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

################################################################################
# Common libraries
################################################################################
if(NOT TARGET MeshFEM)
    option(MESHFEM_ENABLE_BENCHMARKING "" ON)
    add_subdirectory(${THIRD_PARTY_DIR}/MeshFEM)
endif()

if(NOT TARGET rotation_optimization)
    add_subdirectory(${THIRD_PARTY_DIR}/RotationOptimization)
endif()

add_subdirectory(${THIRD_PARTY_DIR}/visvalingam_simplify)

# libigl
option(LIBIGL_WITH_OPENGL            "Use OpenGL"         ON)
option(LIBIGL_WITH_OPENGL_GLFW       "Use GLFW"           ON)
option(LIBIGL_WITH_OPENGL_GLFW_IMGUI "Use IMGUI"          ON)
option(LIBIGL_WITH_VIEWER            "Use OpenGL viewer"  ON)
option(LIBIGL_USE_STATIC_LIBRARY     "Use static library" ON)

find_package(LIBIGL QUIET)
################################################################################
# Elastic Rod Library
################################################################################
add_library(ElasticRods ElasticRod.cc ElasticRodOutput.cc RodMaterial.cc CrossSection.cc eigensolver.cc)
target_include_directories(ElasticRods SYSTEM PUBLIC ${SPECTRA_INCLUDE_DIR})
target_link_libraries(ElasticRods PUBLIC MeshFEM VisvalingamSimplify)
set_target_properties(ElasticRods PROPERTIES CXX_STANDARD 14)
set_target_properties(ElasticRods PROPERTIES CXX_STANDARD_REQUIRED ON)
target_include_directories(ElasticRods SYSTEM PUBLIC .)

add_library(RodLinkages RodLinkage.cc linkage_deformation_analysis.cc TargetSurfaceFitter.cc infer_target_surface.cc)
target_link_libraries(RodLinkages PUBLIC ElasticRods rotation_optimization igl::core)
set_target_properties(RodLinkages PROPERTIES CXX_STANDARD 14)
set_target_properties(RodLinkages PROPERTIES CXX_STANDARD_REQUIRED ON)
target_include_directories(RodLinkages SYSTEM PUBLIC .)

################################################################################
# Third Party Libraries
################################################################################
# Knitro

option(DISABLE_Knitro  "Whether to disable the Knitro parts even if some version of Knitro is available" OFF)

if (NOT DISABLE_Knitro)
    find_package(Knitro QUIET)
    if(Knitro_FOUND)
        target_link_libraries(ElasticRods PUBLIC knitro::knitro)
        target_link_libraries(RodLinkages PUBLIC knitro::knitro)
    else()
        message(STATUS "Knitro not found; knitro solvers and dependent binaries disabled")
    endif()
else()
    message(STATUS "Knitro was manually disabled")
endif()

################################################################################
# Binaries
################################################################################
# if(KNITRO_FOUND)
#     add_subdirectory(xshell_editor)
#     add_subdirectory(weaver_editor)
# endif()

################################################################################
# Python Bindings
################################################################################
if(NOT DEFINED EROD_BUILD_PYTHON OR EROD_BUILD_PYTHON)
add_subdirectory(python_bindings)
endif()

################################################################################
# More binaries
################################################################################
#add_subdirectory(tests)
#add_subdirectory(tools)

# Import only packages built by scripts/bootstrap-windows.ps1.
# Avoid FindBoost/FindCHOLMOD consulting machine-specific environment variables.
set(_prefix "${EROD_DEPS_PREFIX}")
# Configuration-specific dependency prefixes must not reuse package locations
# remembered from a previous configure (e.g. switching Release to Debug).
get_cmake_property(_erod_cache_variables CACHE_VARIABLES)
foreach(_key IN LISTS _erod_cache_variables)
    if(_key MATCHES "^(Boost_DIR|boost_.*_DIR|LAPACK_DIR|SuiteSparse_DIR|_erod_suitesparse_include)$")
        unset(${_key} CACHE)
    endif()
endforeach()
find_package(Boost 1.86.0 EXACT CONFIG REQUIRED COMPONENTS filesystem system program_options property_tree
    PATHS "${_prefix}/lib/cmake/Boost-1.86.0" NO_DEFAULT_PATH)
find_package(LAPACK CONFIG REQUIRED PATHS "${_prefix}/lib/cmake/lapack" NO_DEFAULT_PATH)
find_package(SuiteSparse CONFIG REQUIRED PATHS "${_prefix}" NO_DEFAULT_PATH)
find_path(_erod_suitesparse_include cholmod.h
    PATHS "${_prefix}/include" "${_prefix}/include/suitesparse" NO_DEFAULT_PATH REQUIRED)
foreach(_solver cholmod umfpack)
    add_library(${_solver}::${_solver} INTERFACE IMPORTED)
    set_target_properties(${_solver}::${_solver} PROPERTIES
        INTERFACE_LINK_LIBRARIES "SuiteSparse::${_solver}"
        INTERFACE_INCLUDE_DIRECTORIES "${_erod_suitesparse_include}")
endforeach()
message(STATUS "Using checkout-built dependencies: ${_prefix}")

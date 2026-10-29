# Import only packages built by the macOS/Windows dependency bootstraps.
# Avoid FindBoost/FindCHOLMOD consulting machine-specific environment variables.
set(_prefix "${EROD_DEPS_PREFIX}")
# Configuration-specific dependency prefixes must not reuse package locations
# remembered from a previous configure (e.g. switching Release to Debug).
get_cmake_property(_erod_cache_variables CACHE_VARIABLES)
foreach(_key IN LISTS _erod_cache_variables)
    if(_key MATCHES "^(Boost_DIR|boost_.*_DIR|LAPACK_DIR|SuiteSparse_DIR|SuiteSparse_config_DIR|AMD_DIR|CAMD_DIR|CCOLAMD_DIR|COLAMD_DIR|CHOLMOD_DIR|UMFPACK_DIR|_erod_suitesparse_include)$")
        unset(${_key} CACHE)
    endif()
endforeach()
find_package(Boost 1.86.0 EXACT CONFIG REQUIRED COMPONENTS filesystem system program_options property_tree
    PATHS "${_prefix}/lib/cmake/Boost-1.86.0" NO_DEFAULT_PATH)
# Load dependencies in order and restrict every package to our versioned prefix.
# SuiteSparse 7 exports individual packages rather than SuiteSparseConfig.cmake.
foreach(_package SuiteSparse_config AMD CAMD CCOLAMD COLAMD CHOLMOD UMFPACK)
    find_package(${_package} CONFIG REQUIRED PATHS "${_prefix}" NO_DEFAULT_PATH)
endforeach()
foreach(_solver cholmod umfpack)
    string(TOUPPER "${_solver}" _package)
    add_library(${_solver}::${_solver} INTERFACE IMPORTED)
    set_target_properties(${_solver}::${_solver} PROPERTIES
        INTERFACE_LINK_LIBRARIES "SuiteSparse::${_package}")
endforeach()
message(STATUS "Using checkout-built dependencies: ${_prefix}")

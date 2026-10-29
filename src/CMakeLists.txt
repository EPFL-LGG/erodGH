set(EROD_SDK_DIRECTORY "${CMAKE_BINARY_DIR}/sdk" CACHE PATH "Native developer import-library output directory")
add_library(erod SHARED liberod/erod.cc)
target_link_libraries(erod PRIVATE RodLinkages MeshFEM)
target_compile_features(erod PRIVATE cxx_std_14)
set_target_properties(erod PROPERTIES
    ARCHIVE_OUTPUT_NAME "erod-abi${EROD_NATIVE_ABI}"
    ARCHIVE_OUTPUT_DIRECTORY "${EROD_SDK_DIRECTORY}"
    RUNTIME_OUTPUT_DIRECTORY "${EROD_OUTPUT_DIRECTORY}"
    LIBRARY_OUTPUT_DIRECTORY "${EROD_OUTPUT_DIRECTORY}"
    INSTALL_RPATH "@loader_path"
    MACOSX_RPATH ON)
foreach(_configuration RELEASE DEBUG RELWITHDEBINFO MINSIZEREL)
    set_target_properties(erod PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY_${_configuration} "${EROD_OUTPUT_DIRECTORY}"
        LIBRARY_OUTPUT_DIRECTORY_${_configuration} "${EROD_OUTPUT_DIRECTORY}")
endforeach()
if(WIN32)
    target_compile_definitions(erod PRIVATE EROD_DLL EROD_DLL_EXPORT)
endif()
add_custom_command(TARGET erod POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        $<TARGET_FILE:tbb> $<TARGET_FILE:tbbmalloc> $<TARGET_FILE_DIR:erod>)

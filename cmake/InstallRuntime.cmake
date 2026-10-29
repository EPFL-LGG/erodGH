include(GNUInstallDirs)
install(TARGETS tbb tbbmalloc
    RUNTIME DESTINATION . COMPONENT Runtime
    LIBRARY DESTINATION . COMPONENT Runtime)
install(TARGETS erod
    RUNTIME DESTINATION . COMPONENT Runtime
    LIBRARY DESTINATION . COMPONENT Runtime
    ARCHIVE DESTINATION sdk COMPONENT Development)
install(FILES "${PROJECT_SOURCE_DIR}/src/liberod/erod_runtime.h" DESTINATION sdk COMPONENT Development)
# Dependency scanning runs after installing the root library. Platform system
# libraries are deliberately excluded; macOS install names are relocated by the
# package tool before its final smoke test and archive checksums.
get_filename_component(_erod_linker_dir "${CMAKE_LINKER}" DIRECTORY)
if(WIN32)
    set(_erod_dumpbin "${_erod_linker_dir}/dumpbin.exe")
endif()
set(EROD_RUNTIME_SEARCH_DIRECTORIES "" CACHE STRING "Additional runtime dependency directories")
configure_file("${PROJECT_SOURCE_DIR}/cmake/InstallRuntimeDependencies.cmake.in"
               "${CMAKE_CURRENT_BINARY_DIR}/InstallRuntimeDependencies.cmake" @ONLY)
install(SCRIPT "${CMAKE_CURRENT_BINARY_DIR}/InstallRuntimeDependencies.cmake" COMPONENT Runtime)
if(APPLE)
    set(_erod_os macos)
    if(CMAKE_OSX_ARCHITECTURES)
        set(_erod_arch "${CMAKE_OSX_ARCHITECTURES}")
    else()
        set(_erod_arch "${CMAKE_SYSTEM_PROCESSOR}")
    endif()
else()
    set(_erod_os windows)
    set(_erod_arch x64)
endif()
if(_erod_arch STREQUAL "x86_64")
    set(_erod_arch x64)
endif()
file(GENERATE OUTPUT "${CMAKE_CURRENT_BINARY_DIR}/metadata/$<CONFIG>/native-build.json"
    CONTENT "{\n  \"format\": 1,\n  \"nativeAbi\": 2,\n  \"os\": \"${_erod_os}\",\n  \"architecture\": \"${_erod_arch}\",\n  \"configuration\": \"$<CONFIG>\",\n  \"fastBuild\": $<IF:$<BOOL:${EROD_FAST_BUILD}>,true,false>,\n  \"library\": \"$<TARGET_FILE_NAME:erod>\"\n}\n")
install(FILES "${CMAKE_CURRENT_BINARY_DIR}/metadata/$<CONFIG>/native-build.json" DESTINATION . COMPONENT Runtime)

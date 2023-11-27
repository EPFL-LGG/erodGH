file(GLOB SRCFILES *.cpp)

add_library(erod SHARED erod.cc)
target_link_libraries(erod RodLinkages MeshFEM)
set_target_properties(erod PROPERTIES CXX_STANDARD 14)
set_target_properties(erod PROPERTIES CXX_STANDARD_REQUIRED ON)
set_target_properties(erod PROPERTIES LIBRARY_OUTPUT_DIRECTORY ${PROJECT_SOURCE_DIR}/erodGH/bin)
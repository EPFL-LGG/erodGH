#pragma once
#include <stdexcept>
#include <string>

namespace erod_runtime {
[[noreturn]] inline void fail(const char *condition, const char *file, int line) {
    throw std::runtime_error(std::string("Native precondition failed: ") + condition +
                             " (" + file + ":" + std::to_string(line) + ")");
}
}
// Explicitly used in recoverable core paths, never a global override of assert.
// Evaluate once and retain the check when NDEBUG is defined.
#define EROD_REQUIRE(condition) ((condition) ? static_cast<void>(0) : \
    ::erod_runtime::fail(#condition, __FILE__, __LINE__))

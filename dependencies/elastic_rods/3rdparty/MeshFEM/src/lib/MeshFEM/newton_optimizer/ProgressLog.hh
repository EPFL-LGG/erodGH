#pragma once
namespace erod_progress {
using Callback = void (*)(const char *, void *);
// Registration is local to the calling solver thread. Callback text is borrowed
// for the duration of the call; consumers must copy it and must not throw.
void setCallback(Callback callback, void *context) noexcept;
}

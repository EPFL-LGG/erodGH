#ifndef EROD_RUNTIME_H
#define EROD_RUNTIME_H
#include <stdint.h>
/* Runtime ABI introspection. Full model bindings are in the matching ERod source. */
#ifdef __cplusplus
extern "C" {
#endif
/* Optional progress extension: thread-local callback, borrowed text, no throwing.
   Clear the callback before releasing its managed delegate or context. */
void erodSetSolverLogCallback(void (*callback)(const char *, void *), void *context);
int erodGetApiVersion(void);
void erodFree(void *buffer);
/* ABI 2: only owned object handles use this destructor. Joint/segment views are borrowed. */
void erodDestroyObject(void *object);
uint64_t erodGetBufferSize(const void *buffer);
/* Copy immediately; borrowed thread-local storage changes on the next operation. */
const char *erodGetLastError(void);
uint64_t erodGetOutstandingObjectCount(void);
uint64_t erodGetOutstandingBufferCount(void);
#ifdef __cplusplus
}
#endif
#endif

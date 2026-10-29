#pragma once
#include <algorithm>
#include <cstdint>
#include <cstring>
#include <limits>
#include <memory>
#include <mutex>
#include <stdexcept>
#include <type_traits>
#include <unordered_map>
#include <vector>

namespace ElasticRodsGH { namespace interop {
// Error storage never refers to exception-owned memory. Copy it before the next
// operation on this thread. Managed callers copy it immediately into an exception.
inline char *error_storage() { static thread_local char text[2048] = {}; return text; }
inline void clear_error() noexcept { error_storage()[0] = 0; }
inline const char *record_error(const char *message) noexcept {
    auto out=error_storage(); std::strncpy(out,message ? message : "Unknown native error",2047); out[2047]=0; return out;
}
inline bool has_error() noexcept { return error_storage()[0] != 0; }
enum class Kind { Rod, Periodic, Linkage, Attracted, Material, Joint, Segment };
template<class T> struct TypeKind;
template<> struct TypeKind<ElasticRod> { static constexpr Kind value=Kind::Rod; };
template<> struct TypeKind<PeriodicRod> { static constexpr Kind value=Kind::Periodic; };
template<> struct TypeKind<RodLinkage> { static constexpr Kind value=Kind::Linkage; };
template<> struct TypeKind<SurfaceAttractedLinkage> { static constexpr Kind value=Kind::Attracted; };
template<> struct TypeKind<RodMaterial> { static constexpr Kind value=Kind::Material; };
template<> struct TypeKind<RodLinkage::Joint> { static constexpr Kind value=Kind::Joint; };
template<> struct TypeKind<RodLinkage::RodSegment> { static constexpr Kind value=Kind::Segment; };
struct Object { Kind kind; void (*destroy)(void *); const void *parent; };
struct Registry {
    std::mutex mutex;
    std::unordered_map<const void *,Object> objects;
    std::unordered_map<const void *,size_t> buffers;
};
inline Registry &registry() { static Registry instance; return instance; }
template<class T> T *own(T *pointer) {
    std::unique_ptr<T> guard(pointer);
    auto &r=registry(); std::lock_guard<std::mutex> lock(r.mutex);
    auto inserted=r.objects.emplace(pointer,Object{TypeKind<T>::value,[](void *p){ delete static_cast<T *>(p); },nullptr});
    if (!inserted.second) throw std::logic_error("Native object already registered");
    return guard.release();
}
template<class T,class Parent> T *borrow(T *pointer,Parent *parent) {
    using Plain=typename std::remove_const<T>::type;
    auto &r=registry(); std::lock_guard<std::mutex> lock(r.mutex);
    if (!r.objects.count(parent)) throw std::invalid_argument("Invalid parent handle");
    r.objects[pointer]=Object{TypeKind<Plain>::value,nullptr,parent}; return pointer;
}
template<class T> void require(T *pointer) {
    using Plain=typename std::remove_const<T>::type;
    if (!pointer) throw std::invalid_argument("Native handle is null");
    auto &r=registry(); std::lock_guard<std::mutex> lock(r.mutex);
    auto found=r.objects.find(pointer);
    if (found==r.objects.end()) throw std::invalid_argument("Native handle is invalid or released");
    auto expected=TypeKind<Plain>::value, actual=found->second.kind;
    if (actual!=expected && !(expected==Kind::Linkage && actual==Kind::Attracted))
        throw std::invalid_argument("Native handle has the wrong object type");
}
inline void release_buffer(void *pointer) noexcept {
    if (!pointer) return;
    try {
        auto &r=registry(); std::lock_guard<std::mutex> lock(r.mutex);
        auto found=r.buffers.find(pointer);
        if (found==r.buffers.end()) return; // Never pass foreign/already-freed pointers to free.
        r.buffers.erase(found); std::free(pointer);
    } catch (...) { record_error("Failed to release native buffer"); }
}
class Call {
    std::vector<void *> allocations;
    Call *previous;
    bool committed=false;
    static Call *&current() { static thread_local Call *value=nullptr; return value; }
public:
    Call() : previous(current()) { current()=this; }
    ~Call() noexcept { current()=previous; if (!committed) for (auto p:allocations) release_buffer(p); }
    void commit() {
        if (previous) previous->allocations.insert(previous->allocations.end(),allocations.begin(),allocations.end());
        committed=true;
    }
    static void track(void *p) { if (current()) current()->allocations.push_back(p); }
};
inline void *allocate(size_t bytes) {
    if (!bytes) return nullptr;
    auto pointer=std::malloc(bytes); if (!pointer) throw std::bad_alloc();
    try {
        auto &r=registry();
        { std::lock_guard<std::mutex> lock(r.mutex); r.buffers.emplace(pointer,bytes); }
        Call::track(pointer);
    } catch (...) {
        auto &r=registry(); { std::lock_guard<std::mutex> lock(r.mutex); r.buffers.erase(pointer); }
        std::free(pointer); throw;
    }
    return pointer;
}
inline void copy(void *destination,const void *source,size_t bytes) {
    if (!bytes) return;
    if (!destination || !source) throw std::invalid_argument("Null buffer");
    std::memcpy(destination,source,bytes);
}
inline int checked_count(size_t count) {
    if (count>static_cast<size_t>((std::numeric_limits<int>::max)())) throw std::overflow_error("Native result exceeds managed array limits");
    return static_cast<int>(count);
}
inline void index(int value,size_t count) {
    if (value<0 || static_cast<size_t>(value)>=count) throw std::out_of_range("Native index out of range");
}
inline void destroy(void *pointer) noexcept {
    if (!pointer) return;
    try {
        void (*deleter)(void *)=nullptr;
        auto &r=registry();
        {
            std::lock_guard<std::mutex> lock(r.mutex);
            auto found=r.objects.find(pointer);
            if (found==r.objects.end() || !found->second.destroy) { record_error("Not an owned native object"); return; }
            deleter=found->second.destroy;
            for (auto it=r.objects.begin();it!=r.objects.end();) {
                if (it->first==pointer || it->second.parent==pointer) it=r.objects.erase(it); else ++it;
            }
        }
        deleter(pointer);
    } catch (...) { record_error("Failed to destroy native object"); }
}
} }

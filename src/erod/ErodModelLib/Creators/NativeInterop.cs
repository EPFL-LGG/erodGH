using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ErodModelLib.Creators
{
    /// <summary>Owned native objects and borrowed views with an explicit owning root.</summary>
    public sealed class NativeHandle : SafeHandle
    {
        private NativeHandle parent;
        private bool owned;
        internal int Kind { get; private set; }
        internal readonly object Sync = new object();
        internal readonly long Id = Interlocked.Increment(ref nextId);
        private static long nextId;
        internal NativeHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid => handle == IntPtr.Zero;
        internal NativeHandle Root => parent == null ? this : parent.Root;
        public bool IsAlive => !IsClosed && !IsInvalid && (parent == null || parent.IsAlive);
        internal void Initialize(IntPtr pointer, NativeHandle owner = null, int kind = 0)
        {
            parent = owner == null ? null : owner.Root;
            owned = owner == null;
            Kind = owner == null ? kind : owner.Kind;
            SetHandle(pointer);
        }
        internal NativeHandle RetainView()
        {
            using (NativeCall.Enter(this))
            {
                var view = new NativeHandle();
                view.Initialize(DangerousGetHandle(), Root);
                return view;
            }
        }
        protected override bool ReleaseHandle()
        {
            try
            {
                if (owned) { lock (Sync) NativeApi.Destroy(handle); }
                parent = null;
                return true;
            }
            catch { return false; } // Exceptions must never escape SafeHandle finalization.
        }
    }

    public sealed class NativeInteropException : InvalidOperationException
    {
        public string Operation { get; }
        internal NativeInteropException(string operation, string message) : base(operation + ": " + message) { Operation = operation; }
    }

    internal static class NativeApi
    {
        internal const int Abi = 2;
        private static readonly object versionLock = new object();
        private static bool checkedVersion;
        [DllImport(Kernel.NativeLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodGetApiVersion")]
        private static extern int Version();
        [DllImport(Kernel.NativeLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodGetLastError")]
        private static extern IntPtr Error();
        [DllImport(Kernel.NativeLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodDestroyObject")]
        internal static extern void Destroy(IntPtr pointer);
        [DllImport(Kernel.NativeLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodFree")]
        internal static extern void Free(IntPtr pointer);
        [DllImport(Kernel.NativeLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodGetBufferSize")]
        internal static extern ulong BufferSize(IntPtr pointer);
        internal static void EnsureVersion()
        {
            lock (versionLock)
            {
                if (checkedVersion) return;
                var actual = Version();
                if (actual != Abi) throw new InvalidOperationException("ERod native ABI " + actual + " does not match managed ABI " + Abi + ". Install matching native and managed binaries.");
                checkedVersion = true;
            }
        }
        internal static void Check(string operation)
        {
            var text = Marshal.PtrToStringAnsi(Error());
            if (!String.IsNullOrEmpty(text)) throw new NativeInteropException(operation, text);
        }
    }

    // The callback remains rooted until native registration has been cleared.
    // Nested synchronous solves restore the previous registration on this thread.
    internal sealed class NativeSolverLog : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Callback(IntPtr text, IntPtr context);
        [DllImport(Kernel.NativeLibrary, CallingConvention=CallingConvention.Cdecl, EntryPoint="erodSetSolverLogCallback")]
        private static extern void SetCallback(Callback callback, IntPtr context);
        [ThreadStatic] private static NativeSolverLog current;
        private readonly NativeSolverLog previous;
        private readonly Callback callback;
        private readonly bool registered;
        internal NativeSolverLog(Action<string> log)
        {
            previous = current;
            if (log == null) return;
            NativeApi.EnsureVersion();
            callback = (text, context) => {
                try { log(Marshal.PtrToStringAnsi(text) ?? String.Empty); }
                catch { } // A log sink must never unwind across an unmanaged frame.
            };
            try { SetCallback(callback, IntPtr.Zero); }
            catch (EntryPointNotFoundException) {
                throw new InvalidOperationException("Live reports require updated ERod native binaries. Install the matching complete plugin package.");
            }
            registered = true;
            current = this;
        }
        public void Dispose()
        {
            if (!registered) return;
            SetCallback(previous == null ? null : previous.callback, IntPtr.Zero);
            current = previous;
            GC.KeepAlive(callback);
        }
    }

    // Root locks serialize mutation/read operations on the same model. AddRef
    // protects the owning native allocation even when Dispose races with a call.
    internal sealed class NativeCall : IDisposable
    {
        private readonly List<NativeHandle> locked = new List<NativeHandle>();
        private readonly List<NativeHandle> referenced = new List<NativeHandle>();
        private NativeCall(NativeHandle[] handles)
        {
            NativeApi.EnsureVersion();
            try
            {
                foreach (var h in handles) if (h == null) throw new ArgumentNullException(nameof(handles));
                var roots = handles.Select(h => h.Root).Distinct().OrderBy(h => h.Id).ToArray();
                locked.Capacity = roots.Length;
                referenced.Capacity = roots.Length;
                foreach (var root in roots)
                {
                    Monitor.Enter(root.Sync); locked.Add(root);
                }
                foreach (var h in handles) if (!h.IsAlive) throw new ObjectDisposedException("ERod native handle");
                foreach (var root in locked)
                {
                    bool success = false;
                    root.DangerousAddRef(ref success);
                    if (success) referenced.Add(root);
                }
            }
            catch { Dispose(); throw; }
        }
        internal static NativeCall Enter(params NativeHandle[] handles) => new NativeCall(handles);
        public void Dispose()
        {
            for (int i = referenced.Count - 1; i >= 0; --i) referenced[i].DangerousRelease();
            referenced.Clear();
            for (int i = locked.Count - 1; i >= 0; --i) Monitor.Exit(locked[i].Sync);
            locked.Clear();
        }
    }

    internal sealed class NativeBufferScope : IDisposable
    {
        [ThreadStatic] private static NativeBufferScope current;
        private readonly NativeBufferScope previous;
        private readonly List<IntPtr> pointers = new List<IntPtr>();
        internal NativeBufferScope() { previous = current; current = this; }
        internal static void Track(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero) return;
            if (NativeApi.BufferSize(pointer) == 0) throw new InvalidOperationException("Native result is not an owned array buffer");
            if (current != null) current.pointers.Add(pointer);
        }
        internal static void Free(IntPtr pointer)
        {
            for (var scope = current; scope != null; scope = scope.previous)
                while (scope.pointers.Remove(pointer)) { }
            NativeApi.Free(pointer);
        }
        internal static int Length(IntPtr pointer, int elementSize)
        {
            var bytes = NativeApi.BufferSize(pointer);
            if (bytes % (ulong)elementSize != 0) throw new InvalidOperationException("Invalid native buffer length");
            return checked((int)(bytes / (ulong)elementSize));
        }
        private static void Validate(IntPtr source, Array destination, int offset, int count, int size)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (offset < 0 || count < 0 || offset > destination.Length - count) throw new ArgumentOutOfRangeException(nameof(count));
            if (count > 0 && (source == IntPtr.Zero || checked((ulong)count * (ulong)size) > NativeApi.BufferSize(source)))
                throw new InvalidOperationException("Native buffer is shorter than its reported element count");
        }
        internal static void Copy(IntPtr source, double[] destination, int offset, int count) { Validate(source,destination,offset,count,8); if(count>0) Marshal.Copy(source,destination,offset,count); }
        internal static void Copy(IntPtr source, long[] destination, int offset, int count) { Validate(source,destination,offset,count,8); if(count>0) Marshal.Copy(source,destination,offset,count); }
        internal static void Copy(IntPtr source, int[] destination, int offset, int count) { Validate(source,destination,offset,count,4); if(count>0) Marshal.Copy(source,destination,offset,count); }
        public void Dispose()
        {
            current = previous;
            foreach (var pointer in pointers) NativeApi.Free(pointer);
            pointers.Clear();
        }
    }

    internal static class NativeChecks
    {
        internal static double[] BoxCoordinates(Rhino.Geometry.Box[] boxes)
        {
            if (boxes == null) throw new ArgumentNullException(nameof(boxes));
            var coordinates=new double[Product(boxes.Length,24)];
            int index=0;
            // Rhino's GetCorners also validates through rhcommon_c. Packing the
            // value-type plane/interval coordinates needs no native Rhino runtime.
            int[] cx={0,1,1,0,0,1,1,0}, cy={0,0,1,1,0,0,1,1}, cz={0,0,0,0,1,1,1,1};
            foreach(var box in boxes)
            {
                if(!(box.X.T1>box.X.T0) || !(box.Y.T1>box.Y.T0) || !(box.Z.T1>box.Z.T0))
                    throw new ArgumentException("Box intervals must have positive length",nameof(boxes));
                var plane=box.Plane; var origin=plane.Origin;
                var x=plane.XAxis; var y=plane.YAxis; var z=plane.ZAxis;
                double determinant=x.X*(y.Y*z.Z-y.Z*z.Y)-x.Y*(y.X*z.Z-y.Z*z.X)+x.Z*(y.X*z.Y-y.Y*z.X);
                if(Double.IsNaN(determinant) || Double.IsInfinity(determinant) || Math.Abs(determinant)<1e-12)
                    throw new ArgumentException("Box plane is degenerate",nameof(boxes));
                for(int corner=0;corner<8;++corner)
                {
                    double u=cx[corner]==0?box.X.T0:box.X.T1;
                    double v=cy[corner]==0?box.Y.T0:box.Y.T1;
                    double w=cz[corner]==0?box.Z.T0:box.Z.T1;
                    coordinates[index++]=origin.X+x.X*u+y.X*v+z.X*w;
                    coordinates[index++]=origin.Y+x.Y*u+y.Y*v+z.Y*w;
                    coordinates[index++]=origin.Z+x.Z*u+y.Z*v+z.Z*w;
                }
            }
            Finite(coordinates,nameof(boxes));
            return coordinates;
        }
        internal static int Product(int count, int stride) { if (count < 0) throw new ArgumentOutOfRangeException(nameof(count)); return checked(count * stride); }
        internal static void Array<T>(T[] data, int minimum, string name)
        {
            if (data == null) throw new ArgumentNullException(name);
            if (minimum < 0 || data.Length < minimum) throw new ArgumentException("Array is shorter than its native count", name);
        }
        internal static int Sum(int[] counts) { int total=0; foreach (var count in counts) { if(count<0)throw new ArgumentOutOfRangeException(nameof(counts)); total=checked(total+count); } return total; }
        internal static void Finite(double[] values, string name) { foreach (var value in values) if(Double.IsNaN(value)||Double.IsInfinity(value)) throw new ArgumentException("Array contains a non-finite value",name); }
        internal static void Indices(int[] indices, int count, string name) { foreach(var value in indices) if(value<0 || value>=count) throw new ArgumentOutOfRangeException(name); }
    }
}

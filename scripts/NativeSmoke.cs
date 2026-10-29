using System;
using System.Runtime.InteropServices;
public static class ErodNativeSmoke {
    const string Lib = "EROD_LIBRARY_PATH";
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    public static extern bool SetDllDirectory(string path);
    [StructLayout(LayoutKind.Sequential)]
    struct Count { public int Value; public int Canary; }
    [DllImport(Lib, CallingConvention=CallingConvention.Cdecl)]
    static extern IntPtr erodElasticRodBuild(int n, double[] points, out IntPtr message);
    [DllImport(Lib, CallingConvention=CallingConvention.Cdecl)]
    static extern int erodElasticRodGetDoFCount(IntPtr rod);
    [DllImport(Lib, CallingConvention=CallingConvention.Cdecl)]
    static extern void erodElasticRodGetDoFs(IntPtr rod, out IntPtr values, ref Count count);
    [DllImport(Lib, CallingConvention=CallingConvention.Cdecl)]
    static extern void erodElasticRodSetMaterial(IntPtr rod, int type, double E, double nu, double[] parameters, int count, int axis);
    [DllImport(Lib, CallingConvention=CallingConvention.Cdecl)]
    static extern void erodElasticRodGetMeshData(IntPtr rod, out IntPtr points, out IntPtr quads, ref Count np, ref Count nq);
    [DllImport(Lib, CallingConvention=CallingConvention.Cdecl)]
    static extern void erodFree(IntPtr buffer);
    [DllImport(Lib, CallingConvention=CallingConvention.Cdecl)]
    static extern int erodGetApiVersion();
    [DllImport(Lib, CallingConvention=CallingConvention.Cdecl)]
    static extern void erodDestroyObject(IntPtr pointer);
    public static string Run() {
        if(erodGetApiVersion() != 2) throw new Exception("Incompatible ERod native ABI");
        IntPtr message;
        var rod = erodElasticRodBuild(3, new double[]{0,0,0, 1,0,0, 2,0,0}, out message);
        if (rod == IntPtr.Zero) throw new Exception(Marshal.PtrToStringAnsi(message));
        try {
        int ndof = erodElasticRodGetDoFCount(rod);
        var count = new Count { Canary=0x13579bdf };
        IntPtr data;
        erodElasticRodGetDoFs(rod, out data, ref count);
        try {
            if(count.Value != 11 || ndof != 11 || count.Canary != 0x13579bdf)
                throw new Exception("Invalid DOF count or output buffer overwrite");
            var values = new double[count.Value];
            Marshal.Copy(data, values, 0, values.Length);
            if(values[0] != 0 || values[3] != 1 || values[6] != 2) throw new Exception("DOF roundtrip failed");
        } finally { erodFree(data); }
        erodElasticRodSetMaterial(rod, 0, 200000, 0.3, new double[]{0.02,0.01}, 2, 0);
        var np = new Count { Canary=0x13579bdf };
        var nq = new Count { Canary=0x13579bdf };
        IntPtr points, quads;
        erodElasticRodGetMeshData(rod, out points, out quads, ref np, ref nq);
        try {
            if(np.Value <= 0 || nq.Value <= 0 || np.Canary != 0x13579bdf || nq.Canary != 0x13579bdf)
                throw new Exception("Mesh generation or output count ABI failed");
        } finally { erodFree(points); erodFree(quads); }
        return String.Format("PASS: rod construction, {0} DOFs, material solve, mesh ({1} coordinates, {2} indices), buffer release and count ABI", ndof, np.Value, nq.Value);
        } finally { erodDestroyObject(rod); }
    }
}

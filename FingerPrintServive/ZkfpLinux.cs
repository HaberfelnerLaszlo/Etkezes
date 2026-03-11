using System;
using System.Runtime.InteropServices;

public static class ZkfpLinux
{
    private const string LibName = "zkfp";

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_Init();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_Terminate();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_GetDeviceCount();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ZKFPM_OpenDevice(int index);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_CloseDevice(IntPtr device);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_AcquireFingerprint(
        IntPtr device,
        byte[] imageBuffer,
        uint imageBufferLen,
        byte[] templateBuffer,
        ref int templateLen
    );
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ZKFPM_DBInit();
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_GetParameters(
        IntPtr device,
        int paramType,
        byte[] paramValue,
        ref int paramValueLen
    );
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_DBAdd(IntPtr db,int fid ,byte[] template, int templateLen);
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_DBMatch(IntPtr db, byte[] template1, int templateLen1, byte[] template2, int templateLen2, ref int matchedIndex);
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_DBIdentify(IntPtr db, byte[] template, ref int fid, ref int score);
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_ByteArray2Int(byte[] paramValue, ref int result);
                
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ZKFPM_DBFree(IntPtr db);
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ZKFPM_DBMerge(IntPtr db, byte[] template1, byte[] template2, byte[] template3, byte[] mergedTemplate, ref int mergedTemplateLen);
}
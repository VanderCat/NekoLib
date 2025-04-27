using System.Runtime.InteropServices;

namespace NekoLib.Archive;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Header() {
    public const ulong ExpectedSignature = 0x48_43_52_41__4F_4B_45_4E; //NEKOARCH
    [FieldOffset(0)]
    public ulong Signature = ExpectedSignature; 
    [FieldOffset(8)]
    public uint Version = 1;
    [FieldOffset(12)]
    public ulong TreeSize;
    [FieldOffset(20)]
    public fixed byte CompressionId[4];
    [FieldOffset(24)]
    public ulong CompressionDataSize;
}
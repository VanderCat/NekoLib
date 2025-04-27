using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace NekoLib.Archive;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Entry() {
    public fixed byte Md5[16];
    public ushort ArchiveIndex; //0 is main archive
    public ulong Offset;
    public ulong Size;
    public ushort Terminator = 0xffff;

    public unsafe string Md5Str {
        get {
            fixed (byte* md5 = Md5)
                return Convert.ToHexString(new Span<byte>(md5, 16));
        }
    }
}
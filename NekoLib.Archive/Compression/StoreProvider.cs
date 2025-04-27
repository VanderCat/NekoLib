namespace NekoLib.Archive;

[CompressionId("NONE")]
public class StoreProvider : IDecompressor, ICompressor {
    
    public void LoadData(ReadOnlySpan<byte> data) {
        if (data.Length > 0) throw new Exception("StoreProvider does not have extra data!");
    }
    private static void Copy(ReadOnlySpan<byte> data, Span<byte> dest) => data.CopyTo(dest);

    private static Span<byte> CopyReturn(ReadOnlySpan<byte> data) {
        var array = new byte[data.Length];
        Copy(data, array);
        return array;
    }

    public Span<byte> Decompress(ReadOnlySpan<byte> data, int maxsize = Int32.MaxValue) 
        => CopyReturn(data);
    
    public void Decompress(ReadOnlySpan<byte> data, Span<byte> dest) 
        => Copy(data, dest);
    
    public Span<byte> Compress(ReadOnlySpan<byte> data)
        => CopyReturn(data);
        
    public void Compress(ReadOnlySpan<byte> data, Span<byte> dest)
        => Copy(data, dest);
}
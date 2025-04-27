namespace NekoLib.Archive;

public interface IDecompressor {
    public void LoadData(ReadOnlySpan<byte> data);
    public Span<byte> Decompress(ReadOnlySpan<byte> data, int maxsize = int.MaxValue);
    public void Decompress(ReadOnlySpan<byte> data, Span<byte> dest);

    public bool SupportsStreaming => false;
    public Stream GetDecompressionStream(Stream compressedStream) => throw new NotSupportedException();
}
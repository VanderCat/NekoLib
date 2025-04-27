using ZstdSharp;
using ZstdSharp.Unsafe;

namespace NekoLib.Archive;

[CompressionId("ZSTD")]
public class ZstdProvider() : IDecompressor, ICompressor, IDisposable {
    private Decompressor Decompressor = new();
    private Compressor Compressor = new();
    public int DictCapacity = 112640;
    public byte[]? Dict;
    private int _compressionLevel = Compressor.DefaultCompressionLevel;
    
    
    public ZstdProvider(int compressionLevel) : this() {
        CompressionLevel = compressionLevel;
    }
    public int CompressionLevel {
        get => _compressionLevel;
        set {
            if (value > Compressor.MaxCompressionLevel || value < Compressor.MinCompressionLevel)
                value = Compressor.DefaultCompressionLevel;
            _compressionLevel = value;
            Compressor.SetParameter(ZSTD_cParameter.ZSTD_c_compressionLevel, CompressionLevel);
        }
    }

    public void LoadData(ReadOnlySpan<byte> data) {
        Decompressor.LoadDictionary(data);
        Compressor.LoadDictionary(data);
    }

    public Span<byte> Decompress(ReadOnlySpan<byte> data, int maxsize) {
        return Decompressor.Unwrap(data, maxsize);
    }
    
    public void Decompress(ReadOnlySpan<byte> data, Span<byte> dest) {
        Decompressor.Unwrap(data, dest);
    }
    
    public Span<byte> Compress(ReadOnlySpan<byte> data) {
        return Compressor.Wrap(data);
    }
    
    public void Compress(ReadOnlySpan<byte> data, Span<byte> dest) {
        Compressor.Wrap(data, dest);
    }

    public void Dispose() {
        Decompressor.Dispose();
        Compressor.Dispose();
    }

    public bool SupportsTraining => true;

    public void Train(IEnumerable<byte[]> data) {
        var span = DictBuilder.TrainFromBufferFastCover(data, _compressionLevel, DictCapacity);
        Dict = span.ToArray();
        LoadData(Dict);
    }

    public byte[] GetTrainData() {
        if (Dict is null)
            throw new NullReferenceException("Train first");
        return Dict;
    }

    public bool SupportsStreaming => true;

    public Stream GetDecompressionStream(Stream stream) {
        return new DecompressionStream(stream, Decompressor);
    }
}
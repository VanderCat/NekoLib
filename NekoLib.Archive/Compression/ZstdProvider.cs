using ZstdSharp;
using ZstdSharp.Unsafe;

namespace NekoLib.Archive;

[CompressionId("ZSTD")]
public class ZstdProvider() : IDecompressor, ICompressor, IDisposable {
    private Decompressor _decompressor = new();
    private Compressor _compressor = new();
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
            _compressor.SetParameter(ZSTD_cParameter.ZSTD_c_compressionLevel, CompressionLevel);
        }
    }

    public void LoadData(ReadOnlySpan<byte> data) {
        if (data.Length <= 0) return;
        _decompressor.LoadDictionary(data);
        _compressor.LoadDictionary(data);
    }

    public Span<byte> Decompress(ReadOnlySpan<byte> data, int maxsize) {
        return _decompressor.Unwrap(data, maxsize);
    }
    
    public void Decompress(ReadOnlySpan<byte> data, Span<byte> dest) {
        _decompressor.Unwrap(data, dest);
    }
    
    public Span<byte> Compress(ReadOnlySpan<byte> data) {
        return _compressor.Wrap(data);
    }
    
    public void Compress(ReadOnlySpan<byte> data, Span<byte> dest) {
        _compressor.Wrap(data, dest);
    }

    public void Dispose() {
        _decompressor.Dispose();
        _compressor.Dispose();
    }

    public bool SupportsTraining => DictCapacity > 0;

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
        return new DecompressionStream(stream, _decompressor);
    }
}
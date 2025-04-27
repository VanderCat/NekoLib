namespace NekoLib.Archive;

public interface ICompressor {
    public Span<byte> Compress(ReadOnlySpan<byte> data);
    public void Compress(ReadOnlySpan<byte> data, Span<byte> dest);
    public bool SupportsTraining => false;
    public void Train(IEnumerable<byte[]> data) => throw new NotSupportedException();
    public byte[] GetTrainData() => throw new NotSupportedException();
}
namespace NekoLib.Filesystem; 

public interface IFile : IMounted {
    public string Read();
    public byte[] ReadBinary();
    public void Write(string data);
    public void Write(byte[] data);

    public Task<string> ReadAsync();
    public Task<byte[]> ReadBinaryAsync();
    public void WriteAsync(string data);
    public void WriteBinaryAsync(byte[] data);

    public void Lock();
    public void Unlock();

    public bool Exists();
    
    public Stream GetStream();
}
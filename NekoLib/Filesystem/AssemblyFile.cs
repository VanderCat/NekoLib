using System.Data;
using NekoLib.Filesystem;

namespace Lamoon.Filesystem; 

public class AssemblyFile : IFile {
    private readonly AssemblyFilesystem _filesystem;
    public IMountable Filesystem => _filesystem;
    public string Path { get; }
    public string AssemblyPath;
    public bool ReadOnly => true;

    public AssemblyFile(string path, string virtualPath, AssemblyFilesystem fs) {
        _filesystem = fs;
        Path = virtualPath;
        AssemblyPath = path;
    }
    
    public string Read() {
        using var stream = GetStream();
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }

    public byte[] ReadBinary() {
        using var stream = GetStream();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public void Write(string data) {
        throw new ReadOnlyException();
    }

    public void Write(byte[] data) {
        throw new ReadOnlyException();
    }

    public async Task<string> ReadAsync() {
        await using var stream = GetStream();
        using var streamReader = new StreamReader(stream);
        return await streamReader.ReadToEndAsync();
    }

    public async Task<byte[]> ReadBinaryAsync() {
        await using var stream = GetStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public void WriteAsync(string data) {
        throw new ReadOnlyException();
    }

    public void WriteBinaryAsync(byte[] data) {
        throw new ReadOnlyException();
    }

    public void Lock() {
        throw new NotSupportedException();
    }

    public void Unlock() {
        throw new NotSupportedException();
    }

    public bool Exists() {
        using var stream = _filesystem.Assembly.GetManifestResourceStream(AssemblyPath);
        return stream is not null;
    }

    public Stream GetStream() {
        return _filesystem.Assembly.GetManifestResourceStream(AssemblyPath);
    }
}
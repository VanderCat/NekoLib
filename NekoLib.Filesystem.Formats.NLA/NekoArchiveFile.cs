using System.Text;
using NekoLib.Archive;

namespace NekoLib.Filesystem.Formats.NLA;

public class NekoArchiveFile : IFile {
    public IMountable Filesystem => _nekoArchiveFilesystem;
    public string Path { get; }
    public bool ReadOnly => true;
    private NekoArchiveFilesystem _nekoArchiveFilesystem;
    public NekoArchiveFile(NekoArchiveFilesystem archive, string path) {
        _nekoArchiveFilesystem = archive;
        Path = path;
    }
    
    public string Read() {
        var b = ReadBinary();
        return Encoding.UTF8.GetString(b);
    }
    public byte[] ReadBinary() {
        return _nekoArchiveFilesystem.Archive.ReadFile(Path);
    }
    public void Write(string data) {
        throw new NotSupportedException();
    }
    public void Write(byte[] data) {
        throw new NotSupportedException();
    }
    public Task<string> ReadAsync() {
        throw new NotImplementedException();
    }
    public Task<byte[]> ReadBinaryAsync() {
        throw new NotImplementedException();
    }
    public void WriteAsync(string data) {
        throw new NotSupportedException();
    }
    public void WriteBinaryAsync(byte[] data) {
        throw new NotSupportedException();
    }
    public void Lock() {
        throw new NotSupportedException();
    }
    public void Unlock() {
        throw new NotSupportedException();
    }
    public bool Exists() {
        return Filesystem.FileExists(Path);
    }
    public Stream GetStream() {
        return _nekoArchiveFilesystem.Archive.GetStream(Path);
    }
}
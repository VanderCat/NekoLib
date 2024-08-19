using NekoLib.Logging;

namespace NekoLib.Filesystem; 

public class FolderFile : IFile {
    private FolderFilesystem _filesystem;
    
    public IMountable Filesystem => _filesystem;

    public FileInfo FileInfo;
    public string FolderPath { get; }
    public string Path { get; }
    
    public bool ReadOnly { get; }

    public FolderFile(string path, string realPath) {
        Path = path;
        FolderPath = realPath;
        FileInfo = new FileInfo(realPath);
    }
    
    public string Read() {
        return File.ReadAllText(FolderPath);
    }

    public byte[] ReadBinary() {
        return File.ReadAllBytes(FolderPath);
    }

    public void Write(string data) {
        File.WriteAllText(FolderPath, data);
    }

    public void Write(byte[] data) {
        File.WriteAllBytes(FolderPath, data);
    }

    public async Task<string> ReadAsync() {
        return await File.ReadAllTextAsync(FolderPath);
    }

    public async Task<byte[]> ReadBinaryAsync() {
        return await File.ReadAllBytesAsync(FolderPath);
    }

    public async void WriteAsync(string data) {
        await File.WriteAllTextAsync(FolderPath, data);
    }

    public async void WriteBinaryAsync(byte[] data) {
        await File.WriteAllBytesAsync(FolderPath, data);
    }

    public void Lock() {
        using var stream = FileInfo.Open(FileMode.Open);

        stream.Lock(0,stream.Length); //TODO: FIX
    }

    public void Unlock() {
        using var stream = FileInfo.Open(FileMode.Open);

        stream.Unlock(0,stream.Length); //TODO: FIX
    }

    public bool Exists() => FileInfo.Exists;
    
    public Stream GetStream() {
        ILogger.Trace("Opened stream");
        return FileInfo.Open(FileMode.Open);
    }
}
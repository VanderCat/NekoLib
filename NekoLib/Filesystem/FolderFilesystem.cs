using NekoLib.Logging;

namespace NekoLib.Filesystem; 

public class FolderFilesystem : IMountable {
    
    public void Dispose() {
        
    }

    public FolderFilesystem(string path, bool allowCreation = false, bool readOnly = false) {
        PhysicalPath = path;
        DirectoryInfo = new DirectoryInfo(path);
        
        if (!DirectoryInfo.Exists)
            if (!allowCreation)
                throw new DirectoryNotFoundException();
            else 
                DirectoryInfo.Create();
        ForceReadOnly = readOnly;
    }

    public readonly DirectoryInfo DirectoryInfo;

    public string PhysicalPath { get; }

    public bool ForceReadOnly;
    public bool IsReadOnly => DirectoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
    public void OnMount() {
        ILogger.Debug("Mounted {0}", PhysicalPath);
    }
    

    public IFile GetFile(string path) {
        return new FolderFile(path, Path.Combine(PhysicalPath, path));
    }

    public IFile CreateFile(string path) {
        var dirName = Path.GetDirectoryName(Path.Combine(PhysicalPath, path));
        if (dirName is not null) Directory.CreateDirectory(dirName);
        using var stream = File.Create(Path.Combine(PhysicalPath, path));
        return GetFile(path);
    }

    public IEnumerable<string> ListFiles(string path) {
        var prev = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(PhysicalPath);
        var files = Directory.EnumerateFiles(path);
        Directory.SetCurrentDirectory(prev);
        return files;
    }

    public IEnumerable<string> ListDirectories(string path) {
        var prev = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(PhysicalPath);
        var files = Directory.EnumerateDirectories(path);
        Directory.SetCurrentDirectory(prev);
        return files;
    }
    
    public bool IsDirectory(string path) {
        return File.GetAttributes(Path.Combine(PhysicalPath, path)).HasFlag(FileAttributes.Directory);
    }

    public bool FileExists(string path) {
        return File.Exists(Path.Combine(PhysicalPath, path));
    }
}
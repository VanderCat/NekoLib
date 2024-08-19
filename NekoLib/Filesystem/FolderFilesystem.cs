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
        var dirName = Path.GetDirectoryName(path);
        if (dirName is not null) Directory.CreateDirectory(dirName);
        File.Create(path);
        return GetFile(path);
    }

    public string[] ListDirectory(string path) {
        throw new NotImplementedException();
    }

    public bool FileExists(string path) {
        return GetFile(path).Exists();
    }
}
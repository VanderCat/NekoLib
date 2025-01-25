namespace NekoLib.Filesystem; 

public interface IMountable : IDisposable {
    //public string MountPoint { get; }
    public string PhysicalPath { get; }
    public bool IsReadOnly { get; }
    
    public IFile GetFile(string path);
    public IFile CreateFile(string path);
    public IEnumerable<string> ListFiles(string path);
    public IEnumerable<string> ListDirectories(string path);
    public bool IsDirectory(string path);
    public bool FileExists(string path);
}
namespace NekoLib.Filesystem; 

public interface IMountable : IDisposable {
    //public string MountPoint { get; }
    public string PhysicalPath { get; }
    public bool IsReadOnly { get; }
    
    public void OnMount();
    
    public IFile GetFile(string path);
    public IFile CreateFile(string path);
    public string[] ListDirectory(string path);
    public bool FileExists(string path);
}
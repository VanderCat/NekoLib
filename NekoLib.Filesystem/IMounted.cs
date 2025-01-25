namespace NekoLib.Filesystem; 

public interface IMounted {
    public IMountable Filesystem { get; }
    public string Path { get; }
    public bool ReadOnly { get; }
}
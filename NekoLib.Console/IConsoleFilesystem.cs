namespace NekoLib.Extra;

public interface IConsoleFilesystem {
    public bool Exists(string path);
    public string Read(string path);
}
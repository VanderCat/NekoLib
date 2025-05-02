namespace NekoLib.Archive;

public record struct EntryPath {
    public string Extension;
    public string Directory;
    public string Name;
    
    public override string ToString() {
        if (Directory == "__ROOT__")
            return $"{Name}{Extension}";
        return $"{Directory}/{Name}{Extension}";
    }

    public static EntryPath FromString(string str) {
        //var split = str.Split("/", 2);
        var dir = Path.GetDirectoryName(str)??"__NULL__";
        if (dir == "") dir = "__ROOT__";
        return new EntryPath {
            Directory = dir,
            Name = Path.GetFileNameWithoutExtension(str),
            Extension = Path.GetExtension(str),
        };
    }
}
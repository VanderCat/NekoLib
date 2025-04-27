namespace NekoLib.Archive;

[AttributeUsage(AttributeTargets.Class)]
public class CompressionIdAttribute : Attribute {
    public CompressionIdAttribute(string id) {
        if (id.Length != 4) throw new Exception();
        Id = id;
    }

    public string Id;
}
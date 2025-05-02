using NekoLib.Archive;

namespace NekoLib.Filesystem.Formats.NLA;

public class NekoArchiveFilesystem : IMountable {
    public NekoArchive Archive;

    public NekoArchiveFilesystem(Stream stream, IEnumerable<Stream> additionalStreams, string path = "UNKNOWN") {
        Archive = NekoArchive.Load(stream, additionalStreams);
        PhysicalPath = path;
    }
    
    public NekoArchiveFilesystem(IFile file, IEnumerable<IFile> additionalArchives) {
        Archive = NekoArchive.Load(file.GetStream(), additionalArchives.Select(file1 => file1.GetStream()));
        PhysicalPath = file.Path;
    }
    
    public NekoArchiveFilesystem(string path) {
        Archive = NekoArchive.Load(path);
        PhysicalPath = path;
    }
    
    public void Dispose() {
        Archive.Dispose();
    }
    public string PhysicalPath { get; }
    public bool IsReadOnly => true;
    public IFile GetFile(string path) {
        return new NekoArchiveFile(this, path);
    }
    public IFile CreateFile(string path) {
        throw new NotSupportedException();
    }
    public IEnumerable<string> ListFiles(string path) {
        if (path.EndsWith('/')) path = path[..^2];
        if (path == "") path = "__ROOT__";
        return Archive.Entries.Keys.Where(key => key.Directory == path).Select(entryPath => entryPath.ToString());
    }
    public IEnumerable<string> ListDirectories(string path) {
        if (!path.EndsWith('/')) path += "/";
        var set = new HashSet<string>();
        foreach (var key in Archive.Entries.Keys) {
            if (key.Directory.StartsWith(path)) {
                set.Add(key.Directory);
            }
        }
        return set;
    }
    public bool IsDirectory(string path) {
        if (path.EndsWith('/')) path = path[..^2];
        if (path == "") path = "__ROOT__";
        return Archive.Entries.Keys.Any(key => key.Directory == path);
    }
    public bool FileExists(string path) {
        return Archive.Entries.ContainsKey(EntryPath.FromString(path));
    }
}
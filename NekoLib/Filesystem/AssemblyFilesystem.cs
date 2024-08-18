using System.Reflection;
using Lamoon.Filesystem;
using NekoLib.Logging;

namespace NekoLib.Filesystem; 

public class AssemblyFilesystem : IMountable {
    public void Dispose() {
        
    }

    public string MountPoint { get; private set; }
    public string PhysicalPath => Assembly.Location;

    public string RootNamespace = "";
    public bool IsReadOnly => true;

    public Assembly Assembly;

    public AssemblyFilesystem(Assembly assembly, string rootNamespace) {
        Assembly = assembly;
        RootNamespace = rootNamespace;
    }

    public AssemblyFilesystem(Assembly assembly) : this(assembly, assembly.GetName().Name) { }

    public AssemblyFilesystem() : this(Assembly.GetEntryAssembly()) { }

    public void OnMount() {
        ILogger.Debug("Mounted assembly fs for {0}", Assembly.FullName);
    }

    private string TransfromPath(string path) => RootNamespace+"."+path.Replace("/", ".");

    public IFile GetFile(string path) {
        if (!FileExists(path))
            throw new FileNotFoundException();
        var assemblyPath = TransfromPath(path);
        return new AssemblyFile(assemblyPath, path, this);
    }

    public IFile CreateFile(string path) {
        throw new NotSupportedException();
    }

    public string[] ListDirectory(string path) {
        var assemblyNames = Assembly.GetManifestResourceNames();
        var fsNames = new List<string>();
        foreach (var name in assemblyNames) {
            if (!name.StartsWith(RootNamespace)) continue;
            var newName = TransfromPath(path);
            var lastIdx = newName.LastIndexOf("/", StringComparison.Ordinal);
            newName = newName.Remove(lastIdx).Insert(lastIdx, ".");
            fsNames.Add(newName);
        }
        return fsNames.ToArray();
    }

    public bool FileExists(string path) {
        var assemblyPath = TransfromPath(path);
        //Log.Verbose(assemblyPath);
        using var stream = Assembly.GetManifestResourceStream(assemblyPath);
        return stream is not null;
    }
}
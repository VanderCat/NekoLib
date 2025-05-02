using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ZstdSharp;
using ZstdSharp.Unsafe;

namespace NekoLib.Archive;

public class NekoArchiveCompressor {
    public List<string> DirectoriesPath = [];
    public string OutputDir = "";
    public int MaxSize = 0;
    public string ArchiveName = "";
    public ICompressor Compressor;
    public bool Force;
    public bool Verbose;

    internal class ExtensionNode(string ext) {
        public string Extension = ext;
        public Dictionary<string,DirectoryNode> DirectoryNodes = new();
    }
    
    internal class DirectoryNode(string dir) {
        public string Directory = dir;
        public Dictionary<string,FileNode> FileNodes = new();
    }
    
    internal class FileNode(string name) {
        public string Name = name;
        public Entry Entry = new();
        public byte[] Data;
    }
    
    public NekoArchiveCompressor AddDirectoryPath(string path) {
        DirectoriesPath.Add(path);
        if (ArchiveName == "") ArchiveName = Path.GetFileName(Path.GetDirectoryName(path))??"data";
        if (OutputDir == "") OutputDir = Path.Combine(path, "..");
        return this;
    }

    protected void Log(string str) {
        LoggingFunc(str);
        if (Verbose)
            LoggingFunc(str);
    }

    public Action<string> LoggingFunc = Console.WriteLine;

    public NekoArchiveCompressor SetMaxSize(int count) {
        MaxSize = count;
        return this;
    }

    public NekoArchiveCompressor SetOutputDir(string path) {
        OutputDir = path;
        return this;
    }

    public NekoArchiveCompressor SetArchiveName(string name) {
        ArchiveName = name;
        return this;
    }

    public NekoArchiveCompressor SetCompressor(ICompressor compressor) {
        Compressor = compressor;
        return this;
    }

    public NekoArchiveCompressor SetForce(bool force) {
        Force = force;
        return this;
    }

    private FileStream OpenArchive(int archiveIndex) {
        Directory.Exists(DirectoriesPath[0]);
        var archivePath = 
            Path.Join(OutputDir, ArchiveName + (archiveIndex > 0 ? $".{archiveIndex}" : ".root") + ".nla");
        Log($"Writing to {archivePath}");
        if (Force && File.Exists(archivePath))
            File.Delete(archivePath);
        
        return File.Open(archivePath, FileMode.CreateNew, FileAccess.Write) ?? throw new FileLoadException();
    }

    private void Train() {
        Log("Starting Training");
        var uncompressed = new List<byte[]>();
        foreach (var path in DirectoriesPath) {
            Log($"Adding {path} to train list");
            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)) {
                var f = File.ReadAllBytes(file);
                uncompressed.Add(f);
            }
        }
        Log("Training...");
        Compressor.Train(uncompressed);
        Log("Training finished!");
    }

    private unsafe Dictionary<string, ExtensionNode> GetFileTree() {
        Log("Creating file tree");
        var fileTree = new Dictionary<string, ExtensionNode>();
        foreach (var file in Directory.GetFiles(DirectoriesPath[0], "*.*", SearchOption.AllDirectories)) {
            var rlPath = Path.GetRelativePath(DirectoriesPath[0], file);
            var ep = EntryPath.FromString(rlPath);
            if (!fileTree.TryGetValue(ep.Extension, out var node))
                node = fileTree[ep.Extension] = new ExtensionNode(ep.Extension);
            if (!node.DirectoryNodes.TryGetValue(ep.Directory, out var dirNode))
                dirNode = node.DirectoryNodes[ep.Directory] = new DirectoryNode(ep.Directory);
            if (!dirNode.FileNodes.TryGetValue(ep.Name, out var fileNode))
                fileNode = dirNode.FileNodes[ep.Name] = new FileNode(ep.Name);
            fileNode.Entry.ArchiveIndex = 0;
            Log($"Compressing {rlPath} as {ep}");
            var data = File.ReadAllBytes(file);
            var compressed = Compressor.Compress(data);
            var ratio = (float)data.Length / compressed.Length;
            Log($"Compressed, Size: {compressed.Length}, Ratio: {ratio:F} ({-(1 - (float)compressed.Length / data.Length)*100:F}%)");
            fileNode.Entry.Size = (ulong)compressed.Length;
            fileNode.Data = compressed.ToArray();
            
            fixed (byte* ptr = fileNode.Entry.Md5)
                MD5.HashData(compressed, new Span<byte>(ptr, 16));
        }

        return fileTree;
    }

    private ulong WriteHeader(BinaryWriter br) {
        var compressionId = Compressor.GetType().GetCustomAttribute<CompressionIdAttribute>()?.Id;
        if (compressionId is null) {
            throw new Exception("Compression id is missing please add");
        }
        var header = new Header();
        br.Write(header.Signature);
        br.Write(header.Version);
        br.Write(header.TreeSize);
        br.Write(Encoding.ASCII.GetBytes(compressionId));
        if (Compressor.SupportsTraining) {
            var data = Compressor.GetTrainData();
            br.Write((ulong)data.Length);
            br.Write(data);
        }
        else {
            br.Write((ulong)0);
        }
        return (ulong)br.BaseStream.Position;
    }

    private void WriteNodes(BinaryWriter br, Dictionary<string, ExtensionNode> fileTree) {
        foreach (var (_, ext) in fileTree) {
            WriteExtensionNode(br, ext);
        }
        br.Write(char.MinValue);
    }
    private void WriteExtensionNode(BinaryWriter br, ExtensionNode ext) {
        br.Write(Encoding.UTF8.GetBytes(ext.Extension));
        br.Write(char.MinValue);
        foreach (var (_, pathNode) in ext.DirectoryNodes) {
            WritePathNode(br, pathNode);
        }
        br.Write(char.MinValue);
    }

    private void WritePathNode(BinaryWriter br, DirectoryNode dir) {
        br.Write(Encoding.UTF8.GetBytes(dir.Directory));
        br.Write(char.MinValue);
        foreach (var (_, pathNode) in dir.FileNodes) {
            WriteFileNode(br, pathNode);
        }
        br.Write(char.MinValue);
    }
    private Dictionary<int, List<byte[]>> _dataBlob = [];
    private ulong _offset = 0;
    private ushort _archiveIndex = 0;
    private unsafe void WriteFileNode(BinaryWriter br, FileNode file) {
        br.Write(Encoding.UTF8.GetBytes(file.Name));
        br.Write(char.MinValue);
        file.Entry.Offset = _offset;
        _offset += file.Entry.Size;
        if (_offset + (_archiveIndex == 0 ? _headerSize : 0) > (ulong)MaxSize && MaxSize > 0) {
            _offset = file.Entry.Size;
            file.Entry.Offset = 0;
            _archiveIndex++;
        }
        if (!_dataBlob.TryGetValue(_archiveIndex, out var list))
            list = _dataBlob[_archiveIndex] = [];
        list.Add(file.Data);
        for (int i = 0; i < 16; i++) {
            br.Write(file.Entry.Md5[i]);
        }
        br.Write(_archiveIndex);
        br.Write(file.Entry.Offset);
        br.Write(file.Entry.Size);
        br.Write(file.Entry.Terminator);

        br.Flush();
    }

    private ulong _headerSize = 0;
    private void WriteArchives(Dictionary<string, ExtensionNode> fileTree) {
        using var fileStream = OpenArchive(0);
        using var br = new BinaryWriter(fileStream, Encoding.UTF8, true);
        _headerSize = WriteHeader(br);
        WriteNodes(br, fileTree);
        var treeSize = (ulong)fileStream.Position-_headerSize;
        var pos = br.BaseStream.Position;
        br.Seek(12, SeekOrigin.Begin);
        br.Write(treeSize);
        br.BaseStream.Seek(pos, SeekOrigin.Begin);
        foreach (var i in _dataBlob.Keys) {
            if (i != 0) {
                using var br1 = new BinaryWriter(OpenArchive(i), Encoding.UTF8, false);
                WriteArchive(br1, i);
                continue;
            }
            WriteArchive(br, 0);
        }
    }

    private void WriteArchive(BinaryWriter br, int i) {
        foreach (var arr in _dataBlob[i]) {
            br.Write(arr);
        }
    }
    
    //TODO: support multiple dirs DirectoriesPath[0]
    public unsafe void Compress() {
        Log("Compressing started");
        if (Compressor.SupportsTraining)
            Train();
        _dataBlob = [];
        _offset = 0;
        var fileTree = GetFileTree();
        WriteArchives(fileTree);
    }

    public NekoArchiveCompressor SetVerbose(bool verbose) {
        Verbose = verbose;
        return this;
    }
}
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
    public int ArchiveCount = 1;
    public string ArchiveName = "";
    public ICompressor Compressor;
    public bool Force;

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

    public NekoArchiveCompressor SetArchiveCount(int count) {
        ArchiveCount = 1;
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
    
    //TODO: support multiple dirs DirectoriesPath[0]
    public unsafe void Compress() {
        Console.WriteLine("Compressing started");
        Directory.Exists(DirectoriesPath[0]);
        var infos = new List<FileInfo>();
        var fileTree = new Dictionary<string, ExtensionNode>();
        ulong offset = 0;
        var archivePath = Path.Join(OutputDir, ArchiveName + ".nla");
        Console.WriteLine("Compressing to "+archivePath);
        if (Force && File.Exists(archivePath)) {
            File.Delete(archivePath);
        }
        using var fileStream = File.Open(archivePath, FileMode.CreateNew, FileAccess.Write);
        using var br = new BinaryWriter(fileStream);
        var datablob = Array.Empty<byte[]>();
        if (Compressor.SupportsTraining) {
            var uncomp = new List<byte[]>();
            foreach (var file in Directory.GetFiles(DirectoriesPath[0], "*.*", SearchOption.AllDirectories)) {
                var f = File.ReadAllBytes(file);
                uncomp.Add(f);
            }
            Compressor.Train(uncomp);
        }
        foreach (var file in Directory.GetFiles(DirectoriesPath[0], "*.*", SearchOption.AllDirectories)) {
            var rlPath = Path.GetRelativePath(DirectoriesPath[0], file);
            var ep = EntryPath.FromString(rlPath);
            var fileInfo = new FileInfo(file);
            if (!fileTree.TryGetValue(ep.Extension, out var node))
                node = fileTree[ep.Extension] = new ExtensionNode(ep.Extension);
            if (!node.DirectoryNodes.TryGetValue(ep.Directory, out var dirNode))
                dirNode = node.DirectoryNodes[ep.Directory] = new DirectoryNode(ep.Directory);
            if (!dirNode.FileNodes.TryGetValue(ep.Name, out var fileNode))
                fileNode = dirNode.FileNodes[ep.Name] = new FileNode(ep.Name);
            fileNode.Entry.ArchiveIndex = 0;
            var compressed = Compressor.Compress(File.ReadAllBytes(file));
            fileNode.Entry.Size = (ulong)compressed.Length;
            fileNode.Data = compressed.ToArray();
            fixed (byte* ptr = fileNode.Entry.Md5)
                MD5.HashData(compressed, new Span<byte>(ptr, 16));
        }

        var compid = Compressor.GetType().GetCustomAttribute<CompressionIdAttribute>();
        if (compid is null) {
            throw new Exception("Compression id is missing please add");
        }

        var header = new Header();
        br.Write(header.Signature);
        br.Write(header.Version);
        br.Write(header.TreeSize);
        br.Write(Encoding.ASCII.GetBytes(compid.Id));
        if (Compressor.SupportsTraining) {
            var data = Compressor.GetTrainData();
            br.Write((ulong)data.Length);
            br.Write(data);
        }
        else {
            br.Write((ulong)0);
        }
        header.TreeSize = (ulong)fileStream.Position;
        foreach (var (ext, extNode) in fileTree) {
            br.Write(Encoding.UTF8.GetBytes(ext));
            br.Write(char.MinValue);
            foreach (var (path, pathNode) in extNode.DirectoryNodes) {
                br.Write(Encoding.UTF8.GetBytes(path));
                br.Write(char.MinValue);
                foreach (var (name, fileNode) in pathNode.FileNodes) {
                    br.Write(Encoding.UTF8.GetBytes(name));
                    br.Write(char.MinValue);
                    Array.Resize(ref datablob, datablob.Length+1);
                    datablob[^1] = fileNode.Data;
                    fileNode.Entry.Offset = offset;
                    offset += fileNode.Entry.Size;
                    for (int i = 0; i < 16; i++) {
                        br.Write(fileNode.Entry.Md5[i]);
                    }
                    br.Write(fileNode.Entry.ArchiveIndex);
                    br.Write(fileNode.Entry.Offset);
                    br.Write(fileNode.Entry.Size);
                    br.Write(fileNode.Entry.Terminator);
                    

                    br.Flush();
                }
                br.Write(char.MinValue);
            }
            br.Write(char.MinValue);
        }
        br.Write(char.MinValue);
        header.TreeSize = (ulong)fileStream.Position-header.TreeSize;
        foreach (var arr in datablob) {
            br.Write(arr);
        }
        br.Seek(12, SeekOrigin.Begin);
        br.Write(header.TreeSize);
        br.Close();
        fileStream.Close();
    }
}
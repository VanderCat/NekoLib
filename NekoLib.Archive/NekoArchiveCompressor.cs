using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ZstdSharp;
using ZstdSharp.Unsafe;

namespace NekoLib.Archive;

public class NekoArchiveCompressor {
    public string DirectoryPath = "";
    public string OutputDir = "";
    public int ArchiveCount = 1;
    public string ArchiveName = "";
    public int CompressionLevel = 15;

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
    
    public NekoArchiveCompressor SetDirectoryPath(string path) {
        DirectoryPath = path;
        if (ArchiveName == "") ArchiveName = Path.GetFileName(Path.GetDirectoryName(path))??"data";
        if (OutputDir == "") OutputDir = Path.Combine(DirectoryPath, "..");
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
    
    public unsafe void Compress() {
        Directory.Exists(DirectoryPath);
        var infos = new List<FileInfo>();
        var fileTree = new Dictionary<string, ExtensionNode>();
        ulong offset = 0;
        var compressor = new Compressor();
        using var fileStream = File.Open(Path.Join(OutputDir, ArchiveName+".nla"), FileMode.CreateNew, FileAccess.Write);
        using var br = new BinaryWriter(fileStream);
        var datablob = Array.Empty<byte[]>();
        compressor.SetParameter(ZSTD_cParameter.ZSTD_c_compressionLevel, CompressionLevel);
        foreach (var file in Directory.GetFiles(DirectoryPath, "*.*", SearchOption.AllDirectories)) {
            var rlPath = Path.GetRelativePath(DirectoryPath, file);
            var ep = EntryPath.FromString(rlPath);
            var fileInfo = new FileInfo(file);
            if (!fileTree.TryGetValue(ep.Extension, out var node))
                node = fileTree[ep.Extension] = new ExtensionNode(ep.Extension);
            if (!node.DirectoryNodes.TryGetValue(ep.Directory, out var dirNode))
                dirNode = node.DirectoryNodes[ep.Directory] = new DirectoryNode(ep.Directory);
            if (!dirNode.FileNodes.TryGetValue(ep.Name, out var fileNode))
                fileNode = dirNode.FileNodes[ep.Name] = new FileNode(ep.Name);
            fileNode.Entry.ArchiveIndex = 0;
            fileNode.Entry.CompressionType = CompressionType.Zstd;
            var compressed = compressor.Wrap(File.ReadAllBytes(file));
            fileNode.Entry.Size = (ulong)compressed.Length;
            fileNode.Data = compressed.ToArray();
            fixed (byte* ptr = fileNode.Entry.Md5)
                MD5.HashData(compressed, new Span<byte>(ptr, 16));
        }

        var header = new Header();
        br.Write(header.Signature);
        br.Write(header.Version);
        br.Write(header.TreeSize);
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
                    br.Write((uint)fileNode.Entry.CompressionType);
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
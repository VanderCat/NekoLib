using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using ZstdSharp;

namespace NekoLib.Archive;

public class NekoArchive : IDisposable, IAsyncDisposable {
    private Stream ArchiveInfoStream;
    public Dictionary<EntryPath, Entry> Entries;
    public Header Header;
    private ulong DataOffset;
    public List<Stream> Streams;
    public IDecompressor Decompressor;
    
    internal class ArchiveComparer : IComparer<string> {
        public int Compare(string x, string y) {
            // Parse the extension from the file name.
            x = Path.GetFileNameWithoutExtension(x);
            y = Path.GetFileNameWithoutExtension(y);
            var idx = ushort.Parse(x[(x.LastIndexOf('.') + 1)..]);
            var idy = ushort.Parse(y[(y.LastIndexOf('.') + 1)..]);
            return idx - idy;
        }
    }

    
    private static Dictionary<string, IDecompressor>  _decompressors = new();
    public static void Register<T>() where T : IDecompressor, new() {
        var attr = typeof(T).GetCustomAttribute<CompressionIdAttribute>();
        if (attr is null) {
            throw new Exception("Missing CompressionIdAttribute");
        }

        if (_decompressors.ContainsKey(attr.Id)) {
            throw new Exception("decompressor with this id have been already registered");
        }
        
        _decompressors[attr.Id] = new T();
    }
    
    public static void UnregisterDecompressor(string id) {
        //BUG: not disposed if disposable
        _decompressors.Remove(id);
    }
    
    public static NekoArchive Load(string path) {
        var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        var additionalStreamPaths = new SortedSet<string>(new ArchiveComparer());
        var name = Path.GetFileNameWithoutExtension(path);
        if (name.EndsWith(".root")) {
            name = name.Replace(".root", "");
            foreach (var archive in  Directory.EnumerateFiles(Path.GetDirectoryName(Path.GetFullPath(path)), name+".*.nla",
                         SearchOption.TopDirectoryOnly)) {
                if (archive.Contains(".root.nla")) continue;
                additionalStreamPaths.Add(archive);
            }
        }

        var streams = new Stream[additionalStreamPaths.Count];
        var i = 0;
        foreach (var streamPath in additionalStreamPaths) {
            streams[i++] = File.Open(streamPath, FileMode.Open, FileAccess.Read);
        }
        return Load(stream, streams);
    }
    
    public static NekoArchive Load(Stream archive, IEnumerable<Stream>? additionalArchives) {
        var header = ReadHeader(archive);
        if (header.Version != 1) throw new NotImplementedException();
        //var decomp = new DecompressionStream(stream, 0, true, false);
        return new NekoArchive(archive, header, additionalArchives?.ToList()??[]);
    }
    
    internal SubStream GetCompressedStream(string path) {
        var ep = EntryPath.FromString(path);
        var entry = Entries[ep];
        Stream stream;
        ulong offset = 0;
        if (entry.ArchiveIndex == 0) {
            stream = ArchiveInfoStream;
            offset = (ulong)Marshal.SizeOf<Header>()+Header.TreeSize+Header.CompressionDataSize;
        }
        else {
            stream = Streams[entry.ArchiveIndex - 1];
        }
        offset += entry.Offset;
        return new SubStream(stream, (long)offset, (long)entry.Size);
    }

    public Stream GetStream(string path) {
        if (Decompressor.SupportsStreaming) {
            return Decompressor.GetDecompressionStream(GetCompressedStream(path));
        }
        throw new NotSupportedException();
    }

    public byte[] ReadFile(string path) {
        var compressed = GetCompressedStream(path);
        using var ms = new MemoryStream();
        compressed.CopyTo(ms);
        return Decompressor.Decompress(ms.ToArray()).ToArray();
    }

    private static unsafe Header ReadHeader(Stream stream) {
        using var br = new BinaryReader(stream, Encoding.Default, true);
        if (br.ReadUInt64() != Header.ExpectedSignature) throw new InvalidDataException();
        var header = new Header();
        header.Version = br.ReadUInt32();
        header.TreeSize = br.ReadUInt64();
        header.CompressionId[0] = br.ReadByte();
        header.CompressionId[1] = br.ReadByte();
        header.CompressionId[2] = br.ReadByte();
        header.CompressionId[3] = br.ReadByte();
        header.CompressionDataSize = br.ReadUInt64();
        return header;
    }

    private unsafe NekoArchive(Stream stream, Header header, List<Stream> streams) {
        Header = header;
        ArchiveInfoStream = stream;
        using var br = new BinaryReader(stream, Encoding.Default, true);
        Streams = streams;
        var text = Encoding.ASCII.GetString(header.CompressionId, 4);
        Decompressor = _decompressors[text];
        Decompressor.LoadData(br.ReadBytes((int)header.CompressionDataSize));
        Entries = ReadEntries(br);
    }

    public void Dispose() {
        ArchiveInfoStream.Dispose();
        foreach (var stream in Streams) {
            stream.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await ArchiveInfoStream.DisposeAsync();
        foreach (var stream in Streams) {
            await stream.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }

    private static string ReadStringTerminated(BinaryReader sr) {
        var sb = new StringBuilder();
        var ch = sr.Read(); 
        while (ch > 0) {
            sb.Append((char)ch);
            ch = sr.Read();
        }
        return sb.ToString();
    }
    
    private static Dictionary<EntryPath, Entry> ReadEntries(BinaryReader sr) {
        var dict = new Dictionary<EntryPath, Entry>();
        while (true) {
            var extension = ReadStringTerminated(sr);
            if (extension == "") break;
            while (true) {
                var path = ReadStringTerminated(sr);
                if (path == "") break;
                while (true) {
                    var filename = ReadStringTerminated(sr);
                    if (filename == "") break;
                    var a = ReadFileInformationAndPreloadData(sr);
                    dict.Add(new EntryPath{Directory = path, Extension = extension, Name = filename}, a);
                }
            }
        }
        return dict;
    }

    private static unsafe Entry ReadFileInformationAndPreloadData(BinaryReader sr) {
        var entry = new Entry();
        var md5 = new Span<byte>(entry.Md5, 16);
        for (int i = 0; i < 16; i++) {
            md5[i] = sr.ReadByte();
        }
        entry.ArchiveIndex = sr.ReadUInt16();
        entry.Offset = sr.ReadUInt64();
        entry.Size = sr.ReadUInt64();
        if (sr.ReadUInt16() != 0xffff) {
            throw new Exception();
        }
        return entry;
    }
}
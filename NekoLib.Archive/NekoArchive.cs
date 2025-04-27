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
        var AdditionalStreams = new List<Stream>();
        foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(path))) {
            if (file.StartsWith(Path.GetDirectoryName(path))) {
                if (file == path) continue;
                AdditionalStreams.Add(File.Open(file, FileMode.Open, FileAccess.Read));
            }
        }
        return Load(stream, AdditionalStreams);
    }
    
    public static NekoArchive Load(Stream archive, List<Stream>? additionalArchives) {
        var header = ReadHeader(archive);
        if (header.Version != 1) throw new NotImplementedException();
        //var decomp = new DecompressionStream(stream, 0, true, false);
        return new NekoArchive(archive, header, additionalArchives??[]);
    }

    public Stream GetStream(string path) {
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
        if (Decompressor.SupportsStreaming) {
            return Decompressor.GetDecompressionStream(new SubStream(stream, (long)offset, (long)entry.Size));
        }
        throw new NotSupportedException();
    }

    public byte[] ReadFile(string path) {
        using var stream = GetStream(path);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
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
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await ArchiveInfoStream.DisposeAsync();
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
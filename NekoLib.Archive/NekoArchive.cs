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
            offset = (ulong)Marshal.SizeOf<Header>()+Header.TreeSize-4;
        }
        else {
            stream = Streams[entry.ArchiveIndex - 1];
        }
        offset += entry.Offset;
        if (entry.CompressionType == CompressionType.Zstd) {
            return new DecompressionStream(new SubStream(stream, (long)offset, (long)entry.Size));
        }
        throw new NotImplementedException();
    }

    public byte[] ReadFile(string path) {
        using var stream = GetStream(path);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static Header ReadHeader(Stream stream) {
        using var br = new BinaryReader(stream, Encoding.Default, true);
        if (br.ReadUInt64() != Header.ExpectedSignature) throw new InvalidDataException();
        var header = new Header();
        header.Version = br.ReadUInt32();
        header.TreeSize = br.ReadUInt64();
        return header;
    }

    private NekoArchive(Stream stream, Header header, List<Stream> streams) {
        Header = header;
        ArchiveInfoStream = stream;
        using var br = new BinaryReader(stream, Encoding.Default, true);
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
        entry.CompressionType = (CompressionType)sr.ReadUInt32();
        if (sr.ReadUInt16() != 0xffff) {
            throw new Exception();
        }
        return entry;
    }
}
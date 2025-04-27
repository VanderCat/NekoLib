using NekoLib.Archive;
using ZstdSharp;

namespace Test;

class Program {
    static void Main(string[] args) {

        var comp = new NekoArchiveCompressor()
            .SetDirectoryPath(args[0])
            .SetOutputDir(args[1])
            .SetCompressor(new StoreProvider());
            //.SetCompressor(new ZstdProvider(Compressor.MaxCompressionLevel));
        Console.WriteLine($"Compressing with level {Compressor.MaxCompressionLevel}");
        var path = Path.Combine(comp.OutputDir, comp.ArchiveName + ".nla");
        if (!File.Exists(path)) comp.Compress();
        GC.Collect();
        //Thread.Sleep(10000);
        NekoArchive.Register<StoreProvider>();
        NekoArchive.Register<ZstdProvider>();
        var archive = NekoArchive.Load(path);
        foreach (var (key, value) in archive.Entries) {
            Console.WriteLine($"{key}:\t{value.Md5Str}\t{value.Size/1024} KB\t(Offset: {value.Offset})");
        }
        Console.WriteLine(new StreamReader(archive.GetStream("texture/megumitamura.json")).ReadToEnd());
    }
}
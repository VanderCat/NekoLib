namespace NekoLib.Archive;

public class Program {

    class ListArgs {
        [MiniCli.MiniName("i")] [MiniCli.Help("Path to root (first) archive")]
        public string Input = "";
    }

    class DecompressArgs {
        [MiniCli.MiniName("i")]
        [MiniCli.Help("Path to root (first) archive")]
        public string Input = "";
        
        [MiniCli.MiniName("o")]
        [MiniCli.Help("Output directory")]
        public string Output = "";
        
        [MiniCli.MiniName("f")]
        [MiniCli.Help("Overwrite files")]
        public bool Force = false;
        
        [MiniCli.MiniName("s")]
        [MiniCli.Help("Skip existing files")]
        public bool Skip = false;
        
        [MiniCli.MiniName("v")]
        [MiniCli.Help("Print what's going on")]
        public bool Verbose = false;
        
        // [MiniCli.MiniName("D")]
        // public string[] LoadDlls;
    }
    
    class CompressArgs {
        [MiniCli.MiniName("i")]
        [MiniCli.Help("Input directories to compress")]
        public string[] Input = [];
        [MiniCli.MiniName("o")]
        [MiniCli.Help("Output directory")]
        public string Output = "";
        [MiniCli.MiniName("r")]
        [MiniCli.Help("use directory as root")]
        public bool Root = false;
        [MiniCli.MiniName("m")]
        [MiniCli.Help("Max size of 1 archive data (in bytes)")]
        public int? MaxSize = null;
        [MiniCli.MiniName("n")]
        [MiniCli.Help("Name of the archives")]
        public string Name = "";
        [MiniCli.MiniName("t")]
        [MiniCli.Help("Compression type (ZSTD or NONE)")]
        public string CompressionType = "ZSTD";
        [MiniCli.MiniName("f")]
        [MiniCli.Help("Remove archives if exists")]
        public bool Force = false;
        [MiniCli.MiniName("v")]
        [MiniCli.Help("Print what's going on")]
        public bool Verbose = false;
        // [MiniCli.MiniName("D")]
        // public string[] LoadDlls;
    }

    class ZstdCompressArgs {
        [MiniCli.MiniName("l")]
        [MiniCli.Help("Compression level")]
        public int CompressionLevel = 3;
        [MiniCli.MiniName("s")] 
        [MiniCli.Help("Dictionary size (recommended to be ~100x less than data), 0 to disable")]
        public int DictionarySize = 112640;
    }
    
    public static void PrintHelp() 
    {
        Console.WriteLine("Usage: dotnet NekoLib.Archive.dll -- command [ARGS]");
        Console.WriteLine("Available commands: decompress, compress");
        Console.WriteLine("decompress ARGS:");
        Console.WriteLine(MiniCli.GetHelpFor<DecompressArgs>("\t"));
        Console.WriteLine("compress ARGS:");
        Console.WriteLine(MiniCli.GetHelpFor<CompressArgs>("\t"));
        Console.WriteLine("list ARGS:");
        Console.WriteLine(MiniCli.GetHelpFor<ListArgs>("\t"));
        Console.WriteLine("\tZSTD compression extra:");
        Console.WriteLine(MiniCli.GetHelpFor<ZstdCompressArgs>("\t\t"));
    }
    
    public static void Main(string[] args) {
        if (args.Length < 1) {
            PrintHelp();
            return;
        }
        var arglist = args.ToList();
        int i = 0;
        var subcommand = arglist[0];
        arglist.RemoveAt(0);

        if (subcommand == "compress") {
            var a = MiniCli.Parse<CompressArgs>(arglist);
            var comp = new NekoArchiveCompressor();

            switch (a.Input.Length) {
                case <= 0:
                    throw new ArgumentException("No input folders provided");
                case > 1:
                    throw new NotImplementedException();
            }

            foreach (var path in a.Input) {
                comp.AddDirectoryPath(path);
            }
            if (a.Output != "")
                comp.SetOutputDir(a.Output);
            if (a.Name != "")
                comp.SetArchiveName(a.Name);
            if (a.CompressionType.Equals("zstd", StringComparison.CurrentCultureIgnoreCase)) {
                var zstd = new ZstdProvider();
                var b = MiniCli.Parse<ZstdCompressArgs>(arglist);
                zstd.CompressionLevel = b.CompressionLevel;
                zstd.DictCapacity = b.DictionarySize;
                comp.SetCompressor(zstd);
            }
            else {
                comp.SetCompressor(new StoreProvider());
            }
            if (a.Force) {
                comp.SetForce(true);
            }
            if (a.MaxSize is not null) {
                comp.SetMaxSize(a.MaxSize??0);
            }

            comp.SetVerbose(a.Verbose);
            comp.Compress();
            //.SetCompressor(new ZstdProvider(Compressor.MaxCompressionLevel));
            return;
        }
        NekoArchive.Register<StoreProvider>();
        NekoArchive.Register<ZstdProvider>();

        if (subcommand == "decompress") {
            var a = MiniCli.Parse<DecompressArgs>(arglist);
            var archive = NekoArchive.Load(a.Input);
            foreach (var path in archive.Entries.Keys) {
                var pathStr = Path.Combine(a.Output, path.ToString());
                var shouldOverwrite = a.Force;
                FileStream stream;
                if (File.Exists(pathStr)) {
                    if (a.Skip) continue;
                    if (!shouldOverwrite) {
                        Console.Write($"File {pathStr} exists, overwrite? (Y/n): ");
                        var k = Console.ReadKey(false);
                        if (k.Key is ConsoleKey.Y or ConsoleKey.Enter)
                            shouldOverwrite = true;
                        Console.WriteLine();
                    }
                    if (shouldOverwrite) {
                        stream = File.Open(pathStr, FileMode.Create, FileAccess.Write);
                    }
                    else 
                        continue;
                }
                else {
                    var dir = Path.GetDirectoryName(pathStr);
                    if (dir is not null)
                        Directory.CreateDirectory(dir);
                    stream = File.Open(pathStr, FileMode.CreateNew, FileAccess.Write);
                }

                try {
                    stream.Write(archive.ReadFile(path.ToString()));
                    if (a.Verbose) Console.WriteLine($"Decompressed {pathStr}");
                }
                catch (Exception e) {
                    Console.WriteLine("Failed to decompress "+pathStr);
                    Console.WriteLine(e);
                }
            }
            return;
        }

        if (subcommand == "list") {
            var a = MiniCli.Parse<DecompressArgs>(arglist);
            var archive = NekoArchive.Load(a.Input);
            foreach (var (key, value) in archive.Entries) {
                Console.WriteLine($"{key}:\t{value.Md5Str}\t{value.Size/1024} KB\t(Offset: {value.Offset})");
            }
            return;
        }
        Console.WriteLine("ERROR: Unknown verb "+subcommand);
        PrintHelp();
        return;
    }
}
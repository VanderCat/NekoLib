namespace NekoLib.Archive;

public class Program {
    class DecompressArgs {
        [MiniCli.MiniName("i")]
        [MiniCli.Help("Path to root (first) archive")]
        public required string Input = "";
        
        [MiniCli.MiniName("o")]
        [MiniCli.Help("Output directory")]
        public required string Output = "";
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
        Console.WriteLine("Compress ARGS:");
        Console.WriteLine(MiniCli.GetHelpFor<CompressArgs>("\t"));
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
        if (subcommand != "compress" && subcommand != "decompress") {
            Console.WriteLine("ERROR: Unknown verb "+subcommand);
            PrintHelp();
            return;
        }

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
            comp.Compress();
            //.SetCompressor(new ZstdProvider(Compressor.MaxCompressionLevel));
        }
    }
}
using NekoLib.Archive;
using NekoLib.Core;
using NekoLib.Extra;
using NekoLib.QueuedActions;
using NekoLib.Scenes;
using ZstdSharp;
using Timer = NekoLib.Core.Timer;

namespace Test;

class Program {
    class stuff : Component {
        private bool _shouldRun = true;
        public void Awake() {
            Console.WriteLine("Awake");
            if (_shouldRun)
                GameObject.AddComponent<stuff>()._shouldRun = false;
        }

        public void Start() {
            Console.WriteLine("Start");
        }
        
    }

    class TestScene : BaseScene {
        public override void Initialize() {
            var g = new GameObject("stuff");
            g.AddComponent<stuff>();
        }
    }
    
    static void Main(string[] args) {
        SceneManager.LoadScene(new TestScene());
        while (true) {
            Timer.Global.Update(0.166f);
            ActionDispatcher.ExecuteQueuedActions();
            SceneManager.Update();
            Thread.Sleep(16);
        }

        //for (int i = 0; i < UPPER; i++) {
        //    
        //}
        // var comp = new NekoArchiveCompressor()
        //     .AddDirectoryPath(args[0])
        //     .SetOutputDir(args[1])
        //     .SetCompressor(new StoreProvider());
        //     //.SetCompressor(new ZstdProvider(Compressor.MaxCompressionLevel));
        // Console.WriteLine($"Compressing with level {Compressor.MaxCompressionLevel}");
        // var path = Path.Combine(comp.OutputDir, comp.ArchiveName + ".nla");
        // if (!File.Exists(path)) comp.Compress();
        // GC.Collect();
        // //Thread.Sleep(10000);
        // NekoArchive.Register<StoreProvider>();
        // NekoArchive.Register<ZstdProvider>();
        // var archive = NekoArchive.Load(path);
        // foreach (var (key, value) in archive.Entries) {
        //     Console.WriteLine($"{key}:\t{value.Md5Str}\t{value.Size/1024} KB\t(Offset: {value.Offset})");
        // }
        // Console.WriteLine(new StreamReader(archive.GetStream("texture/megumitamura.json")).ReadToEnd());
    }
}
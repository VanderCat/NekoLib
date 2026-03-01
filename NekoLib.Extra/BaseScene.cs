using NekoLib.Core;
using NekoLib.Scenes;

namespace NekoLib.Extra; 

public abstract class BaseScene : IScene {
    public virtual string Name => GetType().Name;
    public bool DestroyOnLoad { get; protected set; } = true;
    public int Index { get; set; }
    public List<GameObject> GameObjects { get; } = new();

    //private ILogger _logger;

    //protected BaseScene() {
    //    _logger = Logging.GetFor(GetType().ToString());
    //}

    public abstract void Initialize();

    public virtual void Update() {
        foreach (var gameObject in GameObjects) {
            gameObject.Update();
        }
    }

    public virtual void Draw() {
        foreach (var gameObject in GameObjects) {
            gameObject.SendMessage("Draw");
        }
        foreach (var gameObject in GameObjects) {
            gameObject.SendMessage("LateDraw");
        }
    }

    public virtual void OnWindowResize() {
        foreach (var gameObject in GameObjects) {
            gameObject.SendMessage("OnWindowResize");
        }
    }

    public virtual void FixedUpdate() {
        foreach (var gameObject in GameObjects) {
            gameObject.SendMessage("FixedUpdate");
        }
    }

    public virtual void DrawGui() {
        foreach (var gameObject in GameObjects) {
            gameObject.SendMessage("DrawGui");
        }
    }

    public virtual void Dispose() {
        GC.SuppressFinalize(this);
        foreach (var gameObject in GameObjects) {
            gameObject.Dispose();
        }
    }
}
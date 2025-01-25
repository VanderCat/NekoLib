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
    
    public virtual void Initialize() {
        var currentGameObjects = new GameObject[GameObjects.Count];
        GameObjects.CopyTo(currentGameObjects);
        foreach (var gameObject in currentGameObjects) {
            gameObject.Initialize();
        }
    }

    public virtual void Update() {
        var currentGameObjects = new GameObject[GameObjects.Count];
        GameObjects.CopyTo(currentGameObjects);
        foreach (var gameObject in currentGameObjects) {
            gameObject.Update();
        }
    }

    public virtual void Draw() {
        var currentGameObjects = new GameObject[GameObjects.Count];
        GameObjects.CopyTo(currentGameObjects);
        foreach (var gameObject in currentGameObjects) {
            gameObject.SendMessage("Draw");
        }
        foreach (var gameObject in currentGameObjects) {
            gameObject.SendMessage("LateDraw");
        }
    }

    public virtual void OnWindowResize() {
        var currentGameObjects = new GameObject[GameObjects.Count];
        GameObjects.CopyTo(currentGameObjects);
        //_logger.Trace("Window resized");
        foreach (var gameObject in currentGameObjects) {
            gameObject.SendMessage("OnWindowResize");
        }
    }

    public virtual void FixedUpdate() {
        var currentGameObjects = new GameObject[GameObjects.Count];
        GameObjects.CopyTo(currentGameObjects);
        foreach (var gameObject in currentGameObjects) {
            gameObject.SendMessage("FixedUpdate");
        }
    }

    public virtual void DrawGui() {
        var currentGameObjects = new GameObject[GameObjects.Count];
        GameObjects.CopyTo(currentGameObjects);
        foreach (var gameObject in currentGameObjects) {
            gameObject.SendMessage("DrawGui");
        }
    }

    public virtual void Dispose() {
        GC.SuppressFinalize(this);
        var currentGameObjects = new GameObject[GameObjects.Count];
        GameObjects.CopyTo(currentGameObjects);
        foreach (var gameObject in currentGameObjects) {
            gameObject.Dispose();
        }
    }
}
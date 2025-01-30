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
    private GameObject[] _currentGameObjects = [];
    public virtual void Initialize() {
        UpdateCurrentGameObjects();
        foreach (var gameObject in _currentGameObjects) {
            gameObject.Initialize();
        }
    }

    public virtual void Update() {
        UpdateCurrentGameObjects();
        foreach (var gameObject in _currentGameObjects) {
            gameObject.Update();
        }
    }

    public virtual void Draw() {
        UpdateCurrentGameObjects();
        foreach (var gameObject in _currentGameObjects) {
            gameObject.SendMessage("Draw");
        }
        foreach (var gameObject in _currentGameObjects) {
            gameObject.SendMessage("LateDraw");
        }
    }

    public virtual void OnWindowResize() {
        UpdateCurrentGameObjects();
        //_logger.Trace("Window resized");
        foreach (var gameObject in _currentGameObjects) {
            gameObject.SendMessage("OnWindowResize");
        }
    }

    public virtual void FixedUpdate() {
        UpdateCurrentGameObjects();
        foreach (var gameObject in _currentGameObjects) {
            gameObject.SendMessage("FixedUpdate");
        }
    }

    public virtual void DrawGui() {
        UpdateCurrentGameObjects();
        foreach (var gameObject in _currentGameObjects) {
            gameObject.SendMessage("DrawGui");
        }
    }

    public virtual void Dispose() {
        GC.SuppressFinalize(this);
        UpdateCurrentGameObjects();
        foreach (var gameObject in _currentGameObjects) {
            gameObject.Dispose();
        }
    }

    private void UpdateCurrentGameObjects() {
        if (GameObjects.Count > _currentGameObjects.Length) {
            Array.Resize(ref _currentGameObjects, GameObjects.Count);
        }
        GameObjects.CopyTo(_currentGameObjects);
    }
}
using NekoLib.Scenes;

namespace NekoLib.Core; 

public class GameObject : Object {
    public bool ActiveSelf = true;
    
    public bool Active {
        get => ActiveSelf;
        set => ActiveSelf = value;
    }

    public int Layer = 0;

    public IScene Scene { get; protected set; } = SceneManager.ActiveScene;

    public string Tag = "";

    public Transform Transform;

    private List<Component> _components = new();
    public GameObject() {
        Transform = new Transform(){GameObject = this};
        Scene.GameObjects.Add(this);
    }

    public T AddComponent<T>() where T : Component, new() {
        var component = new T {
            GameObject = this
        };
        _components.Add(component);
        return component;
    }

    public T[] GetComponents<T>() where T : Component, new() {
        return _components.OfType<T>().ToArray();
    }
    
    public T GetComponent<T>() where T : Component, new() {
        return GetComponents<T>()[0];
    }
    
    public bool HasComponent<T>() where T : Component, new() {
        return _components.OfType<T>().Any();
    }

    public void SendMessage(string methodName) {
        foreach (var component in _components) {
            component.Invoke(methodName);
        }
    }

    public void Broadcast(string methodName) {
        foreach (var child in Transform) {
            child.Broadcast(methodName);
        }
        SendMessage(methodName);
    }

    public void Update() {
        SendMessage("Update");// FIXME: I think reflection will be slow but whatever/will work fine for now
        SendMessage("LateUpdate");
    }
    
    public void Draw() {
        SendMessage("Draw");
        SendMessage("DrawGui");
    }

    public void Initialize() {
        SendMessage("Awake");
    }
}
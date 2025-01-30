using System.Text.RegularExpressions;
using NekoLib.Scenes;

namespace NekoLib.Core; 
/// <summary>
/// The main building block of this lib.
///
/// <para>
/// An object with placement somewhere in the Scene with possibility to attach a component with any custom
/// behaviour.
/// </para>
/// </summary>
public class GameObject : Object {
    public bool Initialized { get; private set; } = false;

    /// <summary>
    /// Is this <c>GameObject</c> is active or not.
    /// </summary>
    public bool ActiveSelf = true;
    
    /// <summary>
    /// Is this <c>GameObject</c> AND it's parent is active. If it is not it will not update <c>Behaviour</c> derived components.
    /// </summary>
    public bool Active {
        get => (Transform.Parent is null || Transform.Parent.GameObject.Active)&&ActiveSelf;
        set => ActiveSelf = value;
    }

    /// <summary>
    /// The layer the GameObject is in.
    /// <para>
    /// Use this to implement selective rendering/raycast etc.
    /// </para>
    /// </summary>
    public int Layer = 0;

    /// <summary>
    /// Current Scene this GameObject in.
    ///
    /// <para>
    /// When GameObject is created it is automatically placed in active scene.
    /// Keep in mind if there is no ActiveScene it will throw NullReferenceException!
    /// </para>
    /// </summary>
    /// <remarks>
    /// Subject to change
    /// </remarks>
    public IScene Scene { get; protected set; } = SceneManager.ActiveScene;

    /// <summary>
    /// Tags of this GameObject.
    ///
    /// <para>
    /// You can use this to differentiate this GameObject from anything
    /// </para>
    /// </summary>
    public HashSet<string> Tags = new();

    /// <summary>
    /// All tags from all parent tree
    /// </summary>
    public HashSet<string> AllTags {
        get {
            if (Transform.Parent is not null) {
                return Transform.Parent.AllTags.Concat(Tags).ToHashSet();
            }

            return Tags;
        }
    }

    /// <summary>
    /// Transform of the GameObject
    ///
    /// </summary>
    /// <see>
    /// Refer to <c>Transform</c> for more info
    /// </see>
    public Transform Transform;
    
    internal List<Component> _components = new();
    
    /// <summary>
    /// Create a GameObject and add it to scene
    /// </summary>
    /// <exception cref="NullReferenceException">
    /// Throws when there is no active scene
    /// </exception>
    /// <todo>
    /// maybe allow for scene index specification?
    /// </todo>
    public GameObject(string name = "GameObject") {
        Transform = new Transform(){GameObject = this};
        Scene.GameObjects.Add(this);
        Name = name;
    }

    /// <summary>
    /// Add component on this GameObject
    /// </summary>
    /// <typeparam name="TComponent">A Component type</typeparam>
    /// <returns>Component instance of a given type</returns>
    public TComponent AddComponent<TComponent>() where TComponent : Component, new() {
        var component = new TComponent {
            GameObject = this
        };
        _components.Add(component);
        if (Initialized)  {
            component.Invoke("Awake");
            component._awoke = true;
        }
        return component;
    }

    /// <summary>
    /// Get all component instances of a given type on GameObject
    /// </summary>
    /// <typeparam name="TComponent">A Component type</typeparam>
    /// <returns>An array of component instances of a given type</returns>
    public TComponent[] GetComponents<TComponent>() where TComponent : Component, new() {
        return _components.OfType<TComponent>().ToArray();
    }

    public Component[] GetComponents() {
        return _components.ToArray();
    }
    
    /// <summary>
    /// Get first component instance of a given type on GameObject
    /// </summary>
    /// <typeparam name="TComponent">A Component type</typeparam>
    /// <returns>Component instance of a given type</returns>
    public TComponent GetComponent<TComponent>() where TComponent : Component, new() {
        return GetComponents<TComponent>()[0];
    }
    
    /// <summary>
    /// Get first component instance of a given type on GameObject
    /// </summary>
    /// <param name="type">A Component type</param>
    /// <returns>Component instance of a given type</returns>
    public Component? GetComponent(Type type) {
        return _components.FirstOrDefault(t => t.GetType()==type);
    }
    
    /// <summary>
    /// Get first component instance of a given type on GameObject
    /// </summary>
    /// <param name="id">A Component id</param>
    /// <returns>Component instance with a given id, if found</returns>
    public Component? GetComponentById(Guid id) {
        return _components.FirstOrDefault(t => t.Id==id);
    }
    
    /// <summary>
    /// Check if GameObject tree have a component of a given type
    /// </summary>
    /// <typeparam name="T">A Component type</typeparam>
    public bool HasComponentInChildren<TComponent>() where TComponent : Component, new() {
        return GetComponentsInChildren<TComponent>().Any();
    }

    /// <summary>
    /// Get all component instances of a given type in GameObject tree
    /// </summary>
    /// <typeparam name="TComponent">A Component type</typeparam>
    /// <returns>An array of component instances of a given type</returns>
    public TComponent[] GetComponentsInChildren<TComponent>() where TComponent : Component, new() {
        var a = _components.OfType<TComponent>();
        foreach (var child in Transform) {
            a = a.Concat(child.GameObject._components.OfType<TComponent>());
        }

        return a.ToArray();
    }

    public Component[] GetComponentsInChildren() {
        var a = _components.AsEnumerable();
        foreach (var child in Transform) {
            a = a.Concat(child.GameObject._components);
        }

        return a.ToArray();
    }
    
    /// <summary>
    /// Get first component instance of a given type in GameObject tree
    /// </summary>
    /// <typeparam name="TComponent">A Component type</typeparam>
    /// <returns>Component instance of a given type</returns>
    public TComponent GetComponentInChildren<TComponent>() where TComponent : Component, new() {
        return GetComponentsInChildren<TComponent>()[0];
    }
    
    /// <summary>
    /// Check if GameObject have a component of a given type
    /// </summary>
    /// <typeparam name="T">A Component type</typeparam>
    public bool HasComponent<T>() where T : Component, new() {
        return _components.OfType<T>().Any();
    }
    /// <summary>
    /// Check if GameObject have a component of a given type
    /// </summary>
    /// <param name="type">A Component type</param>
    public bool HasComponent(Type type) {
        return _components.Any(t => t.GetType() == type);
    }

    /// <summary>
    /// Calls the method on every Component in this GameObject.
    /// </summary>
    /// <param name="methodName">Name of the method to call</param>
    /// <param name="o">Addition argument to call</param>
    public void SendMessage(string methodName, object? o = null) {
        _SendMessage(_componentsThisFrame, methodName, o);
    }

    private Component[] _componentsThisFrame = [];
    
    private static void _SendMessage(Component[] components, string methodName, object? o = null) {
        foreach (var component in components) {
            component.Invoke(methodName, o);
        }
    }

    /// <summary>
    /// Calls the method on every Component in this GameObject and its children
    /// </summary>
    /// <param name="methodName">Name of the method to call</param>
    /// <param name="o">Addition argument to broadcast</param>
    public void Broadcast(string methodName, object? o = null) {
        foreach (var child in Transform) {
            child.Broadcast(methodName, o);
        }
        _SendMessage(_componentsThisFrame, methodName, o);
    }

    public virtual void Update() {
        if (_components.Count > _componentsThisFrame.Length) {
            Array.Resize(ref _componentsThisFrame, _components.Count);
        }
        _components.CopyTo(_componentsThisFrame);
        _SendMessage(_componentsThisFrame, "StartIfNeeded");
        _SendMessage(_componentsThisFrame, "Update");
        _SendMessage(_componentsThisFrame, "LateUpdate");
    }

    public virtual void Draw() {
        _SendMessage(_componentsThisFrame, "Draw");
        _SendMessage(_componentsThisFrame, "DrawGui");
    }

    public virtual void Initialize() {
        Initialized = true;
        _SendMessage(_components.ToArray(),"Awake"); //TODO: Automatically initialize components added while Awake
        foreach (var component in _components) {
            component._awoke = true;
        }

        foreach (var behaviour in _components.OfType<Behaviour>()) {
            if (behaviour is null) continue;
            if (behaviour.Enabled) behaviour.Enabled = behaviour.Enabled; // Auto run hook
        }
    }

    public override void Dispose() {
        foreach (var component in _components) {
            Destroy(component);
        }
        base.Dispose();
        Scene.GameObjects.Remove(this);
    }

    public static GameObject Find(string path) {
        throw new NotImplementedException();
        var stuff = Regex.Split(path, @"(?<!\\)\/").ToList();
        if (Path.IsPathRooted(path)) {
            foreach (var scene in SceneManager.Scenes) {
                for (var i = 0; i < scene.RootGameObjects.Length; i++) {
                    
                }
            }
        }
    }
}
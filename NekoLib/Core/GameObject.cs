using System.Collections.Immutable;
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
    /// <remarks> Not Yet Implemented </remarks>
    public bool Active {
        get => ActiveSelf;
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
    /// The tag of this GameObject.
    ///
    /// <para>
    /// You can use this to differentiate this GameObject from anything
    /// </para>
    /// </summary>
    public string Tag = "";

    /// <summary>
    /// Transform of the GameObject
    ///
    /// </summary>
    /// <see>
    /// Refer to <c>Transform</c> for more info
    /// </see>
    public Transform Transform;
    
    private List<Component> _components = new();
    
    /// <summary>
    /// Create a GameObject and add it to scene
    /// </summary>
    /// <exception cref="NullReferenceException">
    /// Throws when there is no active scene
    /// </exception>
    /// <todo>
    /// maybe allow for scene index specification?
    /// </todo>
    public GameObject() {
        Transform = new Transform(){GameObject = this};
        Scene.GameObjects.Add(this);
        Name = "GameObject";
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
    /// Check if GameObject have a component of a given type
    /// </summary>
    /// <typeparam name="T">A Component type</typeparam>
    public bool HasComponent<T>() where T : Component, new() {
        return _components.OfType<T>().Any();
    }

    /// <summary>
    /// Calls the method on every Component in this GameObject.
    /// </summary>
    /// <param name="methodName">Name of the method to call</param>
    /// <param name="o">Addition argument to call</param>
    public void SendMessage(string methodName, object? o = null) {
        foreach (var component in _components) {
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
        SendMessage(methodName, o);
    }

    public void Update() {
        SendMessage("StartIfNeeded");
        SendMessage("Update");// FIXME: I think reflection will be slow but whatever/will work fine for now
        SendMessage("LateUpdate");
    }

    public void Draw() {
        SendMessage("Draw");
        SendMessage("DrawGui");
    }

    public void Initialize() {
        Initialized = true;
        SendMessage("Awake");
        foreach (var component in _components) {
            component._awoke = true;
        }
    }
}
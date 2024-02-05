using NekoLib.Core;

namespace NekoLib.Scenes;

/// <summary>
/// Implement this in your scene
/// </summary>
public interface IScene
{
    /// <summary>
    /// Name of the Scene
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Should we unload this scene when another scene is loaded with <c>SceneLoadMode.Exclusive</c>
    /// </summary>
    public bool DestroyOnLoad { get; }
    
    /// <summary>
    /// Index of this scene, please do not override it unless you want bad thing to happen
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// A list of GameObjects in this scene
    /// </summary>
    public List<GameObject> GameObjects { get; }
    
    /// <summary>
    /// Run on Scene Loading
    /// </summary>
    public void Initialize();
    
    /// <summary>
    /// Run every frame
    /// </summary>
    public void Update();
    
    /// <summary>
    /// Run every frame to draw
    /// </summary>
    public void Draw();
}
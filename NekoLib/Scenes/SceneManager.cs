using System.Collections.Immutable;
using System.Reflection;

namespace NekoLib.Scenes;

/// <summary>
/// Manager of all scenes
/// </summary>
public static class SceneManager
{
    private static List<IScene> _scenes = new();
    
    /// <summary>
    /// List of all loaded scenes
    /// </summary>
    public static ImmutableList<IScene> Scenes => _scenes.ToImmutableList();

    /// <summary>
    /// Current active scene index
    /// </summary>
    public static int ActiveSceneIndex { get; private set; } = -1;
    
    /// <summary>
    /// Current active scene
    /// </summary>
    public static IScene ActiveScene => _scenes[ActiveSceneIndex];

    private static InvalidScene _invalidScene = new InvalidScene();

    
    /// <summary>
    /// Get loaded scene by it's name
    /// </summary>
    /// <param name="name">Name of the scene</param>
    /// <returns>Scene instance</returns>
    public static IScene GetSceneByName(string name)
    {
        var searchResult = _scenes.Where(scene => scene.Name == name).ToList();
        return searchResult.Count>0?searchResult[0]:_invalidScene;
    }
    
    /// <summary>
    /// Get loaded scenes by type
    /// </summary>
    /// <typeparam name="TScene">Type : IScene</typeparam>
    /// <returns>List of found scenes of a given type</returns>
    public static List<TScene> GetScenes<TScene>() where TScene : IScene
    {
        return _scenes.Where(scene => scene.GetType() == typeof(TScene)).Cast<TScene>().ToList();
    }

    /// <summary>
    /// Get Scene by it's type
    /// </summary>
    /// <typeparam name="TScene">Type of scene</typeparam>
    /// <returns>Scene of the given type</returns>
    /// <exception cref="ArgumentException">Throws if there is more than one instance of scene of give type</exception>
    public static TScene GetScene<TScene>() where TScene : IScene
    {
        var searchResult = GetScenes<TScene>();
        if (searchResult.Count > 1)
            throw new ArgumentException(); //maybe not?
        return searchResult[0];
    }

    /// <summary>
    /// Load scene instance
    /// </summary>
    /// <param name="scene">Instance of a scene</param>
    /// <param name="loadMode">Scene load mode</param>
    /// <exception cref="InvalidSceneException">You tried to load an instance of <c>InvalidScene</c></exception>
    public static void LoadScene(IScene scene, SceneLoadMode loadMode = SceneLoadMode.Exclusive)
    {
        if (scene.GetType() == typeof(InvalidScene))
            throw new InvalidSceneException();
        if (loadMode == SceneLoadMode.Exclusive) UnloadAllScenes();
        _scenes.Add(scene);
        scene.Index = _scenes.Count - 1;
        ActiveSceneIndex = scene.Index;
        scene.Initialize();
        scene.Update(); //FIXME: Dirty hack: must we unload scene on a next frame?
    }
    
    /// <summary>
    /// Unload scene instance
    /// </summary>
    /// <param name="scene">Scene instance</param>
    /// <exception cref="InvalidSceneException">You tried to unload an instance of <c>InvalidScene</c></exception>
    public static void UnloadScene(IScene scene)
    {
        if (scene.GetType() == typeof(InvalidScene))
            throw new  InvalidSceneException();
        _scenes.Remove(scene);
        RebuildIndexes();
        ActiveSceneIndex = _scenes.Count - 1;
    }
    
    private static void RebuildIndexes()
    {
        for (var index = 0; index < _scenes.Count; index++)
        {
            _scenes[index].Index = index;
        }
    }
    
    /// <summary>
    /// Unload all scenes except scenes marked to not unload
    /// </summary>
    /// <param name="forced">Unload all anyway</param>
    public static void UnloadAllScenes(bool forced = false) {
        for (var index = 0; index < _scenes.Count; index++) {
            var scene = _scenes[index];
            if (scene.DestroyOnLoad || forced)
                UnloadScene(scene);
        }
    }

    public static void Update() {
        for (var index = 0; index < _scenes.Count; index++) {
            var scene = _scenes[index];
            scene.Update();
        }
    }
    
    public static void Draw() {
        for (var index = 0; index < _scenes.Count; index++) {
            var scene = _scenes[index];
            scene.Draw();
        }
    }

    public static void InvokeScene(string name, object? payload = null) {
        for (var index = 0; index < _scenes.Count; index++) {
            var scene = _scenes[index];
            scene.GetType()
                .GetMethod(name,
                    BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.Invoke(scene, payload is null ? null : new[] {payload});
        }
    }
}
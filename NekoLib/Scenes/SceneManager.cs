using System.Collections.Immutable;

namespace NekoLib.Scenes;

public static class SceneManager
{
    private static List<IScene> _scenes = new();
    public static ImmutableList<IScene> Scenes => _scenes.ToImmutableList();

    public static int ActiveSceneIndex { get; private set; } = -1;
    public static IScene ActiveScene => _scenes[ActiveSceneIndex];

    private static InvalidScene _invalidScene = new InvalidScene();

    public static IScene GetSceneByName(string name)
    {
        var searchResult = _scenes.Where(scene => scene.Name == name).ToList();
        return searchResult.Count>0?searchResult[0]:_invalidScene;
    }
    
    public static List<T> GetScenes<T>() where T : IScene
    {
        return _scenes.Where(scene => scene.GetType() == typeof(T)).Cast<T>().ToList();
    }

    public static T GetScene<T>() where T : IScene
    {
        var searchResult = GetScenes<T>();
        if (searchResult.Count > 1)
            throw new ArgumentException(); //maybe not?
        return searchResult[0];
    }

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
}
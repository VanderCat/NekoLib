# NekoLib
[![NuGet ECS](https://img.shields.io/nuget/vpre/VanderCat.NekoLib?label=VanderCat.NekoLib)](https://www.nuget.org/packages/VanderCat.NekoLib)
[![NuGet Console](https://img.shields.io/nuget/vpre/VanderCat.NekoLib.Console?label=VanderCat.NekoLib.Console)](https://www.nuget.org/packages/VanderCat.NekoLib.Console)
[![NuGet Extra](https://img.shields.io/nuget/vpre/VanderCat.NekoLib.Extra?label=VanderCat.NekoLib.Extra)](https://www.nuget.org/packages/VanderCat.NekoLib.Extra)
[![NuGet Filesystem](https://img.shields.io/nuget/vpre/VanderCat.NekoLib.Filesystem?label=VanderCat.NekoLib.Filesystem)](https://www.nuget.org/packages/VanderCat.NekoLib.Filesystem)
[![NuGet Tools](https://img.shields.io/nuget/vpre/VanderCat.NekoLib.Tools?label=VanderCat.NekoLib.Tools)](https://www.nuget.org/packages/VanderCat.NekoLib.Tools)

A collection of library agnostic gamedev-like stuff

> [!WARNING]  
> Beware of bugs: everything's made for personal use, and may be undocumented

## Features
1. Unity like GameObjects + Components
2. Scene Management (implement serializing/etc. yourself)  
   just implement `IScene` and you created a scene! Next you can load it by using `SceneManagement.LoadScene(new Scene)`
3. Transform and Object graph
4. Extendable Virtual Filesystem (NekoLib.Filesystem)
5. ImGui Unity-Like in-game editor (NekoLib.Tools)
6. Source-like console system (NekoLib.Console)
7. Extra stuff like AttachMode, HotReloadService, Damping, base Scene implementation (NekoLib.Extra)

## Example:
```csharp
using NekoLib.Core;
using NekiLib.Scenes;

class Scene : IScene {
    public string Name => "Name"
    public bool DestroyOnLoad { get; set; } = true;
    public int Index { get; set; }
   
   
    private List<GameObject> _gameObjects;
    public List<GameObject> GameObjects => _gameObjects;
    
    public void Initialize() {
        var gameObject = new GameObject();
        gameObject.AddComponent<TestComponent>();
        // Do stuff before first frame
    }
    
    public void Update() {
        // Do stuff every frame
    }
    public void Draw() {
        // Draw stuff on frame
    }
}

class TestComponent : Behaviour {
    void Awake() {
        Console.WriteLine("Just like in Unity!")
    }
}

static class Program {

    public static void Main() {
        var scene = new Scene();
        SceneManagement.LoadScene(scene);
    
        while (true) {
            SceneManagement.Update();
            SceneManagement.Draw();
        }
    }
}
```
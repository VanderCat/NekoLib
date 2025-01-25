using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using NekoLib.Extra;
using NekoLib.Scenes;

namespace NekoLib.Tools; 

public class Inspect : ToolBehaviour {
    private static Inspect? _instance;

    public static Inspect Instance {
        get {
            if (_instance is null) Toggle();
            return _instance;
        }
    }

    public static void Toggle() => _instance = ToolsShared.ToggleTool<Inspect>();

    [ConCommand("inspect")]
    [ConDescription("inspect SceneName/Path/To/GameObject(.ComponentType)")]
    [ConTags("cheat")]
    public static void OpenInspect(params string[] pathArgs) {
        if (pathArgs.Length < 1) {
            Toggle();
            return;
        }
        var path = string.Join(" ", pathArgs);
        var stuff = Regex.Split(path, "(?<!\\\\)\\/").ToList();
        if (stuff.Count < 2) {
            throw new ArgumentException("the path is wrong");
        }
        var scene = SceneManager.GetSceneByName(stuff[0]);
        stuff.RemoveAt(0);
        throw new NotImplementedException();
        //scene.Get
    }
    
    private object? _selectedObject;
    private Inspector? _inspector;
    public object? SelectedObject {
        get => _selectedObject;
        set {
            if (!Enabled) Enabled = true;
            _selectedObject = value;
            try {
                _inspector = Inspector.GetInspectorFor(value);
            }
            catch (Exception e) {
                _lastException = e;
                Log.LogError(e, "Could not create inspector");
            }
        }
    }

    private Exception _lastException;
    
    void DrawGui() {
        if (ImGui.Begin("Inspector")) {
            if (_selectedObject is null ) return;
            if (_inspector is null) DrawFail(_lastException);
            try {
                _inspector.DrawGui();
            }
            catch (Exception e) {
                ImGui.EndDisabled();
                DrawFail(e);
            }
        }
        ImGui.End();
    }

    void DrawFail(Exception e) {
        ImGui.TextColored(Vector4.UnitX, "Failed to render inspector!");
        if (ImGui.CollapsingHeader(e.GetType().FullName??"Unknown error")) {
            ImGui.TextWrapped($"{e}");
        }
    }
}
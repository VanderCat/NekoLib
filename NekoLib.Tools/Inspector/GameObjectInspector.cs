using System.Reflection;
using ImGuiNET;
using Neko;
using NekoLib.Core;

namespace NekoLib.Tools; 

[CustomInspector(typeof(GameObject))]
public class GameObjectInspector : Inspector {

    private Inspector? _transformInspector;

    public override void Initialize() {
        if (Target is not GameObject target)
            throw new InvalidOperationException($"Attempt to use gameobject inspector on {Target.GetType()}");
        _transformInspector = GetInspectorFor(target.Transform);
    }

    private readonly Dictionary<Guid, Inspector> _cache = new(); 

    public override void DrawGui() {
        var target = ((GameObject) Target);
        ImGui.TextDisabled($"ID:{target.Id}");
        ImGui.InputText("Name", ref target.Name, 256);
        ImGui.Checkbox("Enabled", ref target.ActiveSelf);
        if (ImGui.CollapsingHeader(MaterialIcons.Control_camera + "Transform")) {
            _transformInspector?.DrawGui();
        }
        foreach (var component in target.GetComponents()) {
            DrawComponent(component);
        }
    }

    private void DrawComponent(Component component) {
        var iconAttribute = component.GetType().GetCustomAttribute<ToolsIconAttribute>();
        var icon = iconAttribute?.Icon ?? MaterialIcons.Insert_drive_file;
        ImGui.PushID(component.Id.ToString());
        if (ImGui.CollapsingHeader(icon + component.GetType().Name)) {
            ImGui.TextDisabled($"ID: {component.Id}");
            if (component is Behaviour behaviour) {
                var enabled = behaviour.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                    behaviour.Enabled = enabled;
            }
            if (!_cache.TryGetValue(component.Id, out var inspector)) {
                inspector = GetInspectorFor(component);
                _cache[component.Id] = inspector ?? throw new NullReferenceException($"Could not find inspector for {component.GetType()} ({component.Id})");
            }
            inspector.DrawGui();
        }
        ImGui.PopID();
    }
}
using System.Reflection;
using ImGuiNET;

namespace NekoLib.Tools;
[CustomDrawer(typeof(object))]
public class ObjectDrawer : Drawer {
    public override void DrawGui(MemberInfo info, object? obj) {
        base.DrawGui(info, obj);
        if (info.GetCustomAttribute<InlineAttribute>() is null) {
            DrawSelect(info, obj);
            return;
        }
        DrawInline(info, obj);
    }

    private void DrawSelect(MemberInfo info, object? obj) {
        var type = info.GetUnderlyingType()?.ToString()??"Unknown Type";
        ImGui.BeginDisabled();
        ImGui.InputText(info.Name, ref type, (uint)type.Length);
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (!ImGui.Button("i##"+info.Name)) return;
        if (info.MemberType == MemberTypes.Property) {
            if (((PropertyInfo)info).GetMethod is not null)
                Inspect.Instance.SelectedObject = ((PropertyInfo)info).GetValue(obj);
            return;
        }

        if (info.MemberType == MemberTypes.Field) {
            Inspect.Instance.SelectedObject = ((FieldInfo)info).GetValue(obj);
        }
    }

    //TODO: how do we clear it?
    private static Dictionary<MemberInfo, Inspector> _inspectorCache = new();

    private void DrawInline(MemberInfo info, object? obj) {
        if (ImGui.TreeNode(info.Name)) {
            if (!_inspectorCache.TryGetValue(info, out var inspector)) {
                inspector = Inspector.GetInspectorFor(obj);
                _inspectorCache[info] = inspector;
            }
            inspector.DrawGui();
        }
    }
}
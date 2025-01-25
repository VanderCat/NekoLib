using ImGuiNET;

namespace NekoLib.Tools;

[CustomDrawer(typeof(bool))]
public class BoolDrawer : SimpleDrawer<bool> {
    protected override bool DrawInput(string label, ref bool value) => 
        ImGui.Checkbox(label, ref value);
}
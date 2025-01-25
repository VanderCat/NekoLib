using ImGuiNET;
using NekoLib.Extra;

namespace NekoLib.Tools; 

public class ImguiDemoWindow : ToolBehaviour {
    void DrawGui() {
        ImGui.ShowDemoWindow(ref _enabled);
    }

    [ConCommand("imgui_demo_toggle")]
    public static void ToggleImGuiDemoWindow() => ToolsShared.ToggleTool<ImguiDemoWindow>();
}
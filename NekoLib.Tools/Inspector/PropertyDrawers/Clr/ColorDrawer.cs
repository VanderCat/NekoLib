using System.Drawing;
using System.Numerics;
using ImGuiNET;

namespace NekoLib.Tools;

[CustomDrawer(typeof(Color))]
public class ColorDrawer : SimpleDrawer<Color> {
    protected override bool DrawInput(string label, ref Color value) {
        var color = value;
        var color1 = new Vector4(value.R, value.G, value.B, value.A);
        if (ImGui.ColorEdit4(label, ref color1)) {
            value = Color.FromArgb((int)color1.W, (int)color1.X, (int)color1.Y, (int)color1.Z);
            return true;
        }

        return false;
    }
}
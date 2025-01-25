using System.Numerics;
using ImGuiNET;
using NekoLib.Core;

namespace NekoLib.Tools;

[CustomInspector(typeof(Transform))]
public class TransformInspector : Inspector {
    public override void DrawGui() {
        if (Target is not Transform target) 
            throw new ArgumentException($"The target of type {Target.GetType()} is not assignable to type {typeof(Transform)}");
        var pos = target.LocalPosition;
        if (ImGui.DragFloat3("Position", ref pos))
            if (pos != target.LocalPosition) target.LocalPosition = pos;
        var scale = target.LocalScale;
        if (ImGui.DragFloat3("Scale", ref scale))
            if (scale != target.LocalScale) target.LocalScale = scale;
        var rot = target.LocalRotation;
        var rotvec = rot.GetEulerAngles();
        var rotPretty = new Vector3(
            float.RadiansToDegrees(rotvec.X),
            float.RadiansToDegrees(rotvec.Y),
            float.RadiansToDegrees(rotvec.Z)
        );
        if (ImGui.SliderFloat3("Rotation", ref rotPretty, 0, 360f)) {
            rot = Quaternion.CreateFromYawPitchRoll(
                float.RadiansToDegrees(rotvec.X), 
                float.RadiansToDegrees(rotvec.Y), 
                float.RadiansToDegrees(rotvec.Z)
                );
            target.LocalRotation = rot;
        }
    }
}
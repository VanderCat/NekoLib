using System.Numerics;
using NekoLib.Core;
using NekoLib.Scenes;

namespace NekoLib; 

public static class Extensions {
    /// <remarks>From: LEI-Hongfann: https://github.com/dotnet/runtime/issues/38567#issuecomment-655567603</remarks>
    public static Vector3 GetEulerAngles(this Quaternion r) {
        Vector3 angles;

        angles.X = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
        angles.Y = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
        angles.Z = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
        
        return angles;
    }

    public static GameObject? GetGameObjectByName(this IScene scene, string name) {
        return scene.GameObjects.FirstOrDefault(o => o.Name == name);
    }
    public static GameObject? GetGameObjectById(this IScene scene, Guid id) {
        return scene.GameObjects.FirstOrDefault(o => o.Id == id);
    }
    
    public static Component? GetComponentById(this IScene scene, Guid id) {
        foreach (var gameObject in scene.GameObjects) {
            var component = gameObject.GetComponentById(id);
            if (component is not null)
                return component;
        }
        return null;
    }

    public static GameObject? GetChildByName(this GameObject gameObject, string name) {
        return gameObject.Transform.First(o => o.GameObject.Name == name).GameObject;
    }
}
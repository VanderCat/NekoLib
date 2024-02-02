using System.Numerics;

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
}
using System.Drawing;
using System.Numerics;

namespace NekoLib.Extra; 

public static class NekoMath {
    public static float Damp(float from, float to, ref float velocity, float smoothTime, float dt) {
        var omega = 2f / smoothTime;
        var x = omega * dt;
        var exp = 1f / (1f + x + .48f * x * x + .235f * x * x * x);
        var change = from - to;
        var temp = (velocity * omega * change)*dt;
        velocity = exp * (velocity - omega * temp);
        return to + exp* (change + temp);
    }

    public static Vector2 Damp(Vector2 from, Vector2 to, ref Vector2 velocity, float smoothTime, float dt) {
        return new Vector2(Damp(from.X, to.X, ref velocity.X, smoothTime, dt),
            Damp(from.Y, to.Y, ref velocity.Y, smoothTime, dt));
    }

    public static Vector3 Damp(Vector3 from, Vector3 to, ref Vector3 velocity, float smoothTime, float dt) {
        return new Vector3(Damp(from.X, to.X, ref velocity.X, smoothTime, dt),
            Damp(from.Y, to.Y, ref velocity.Y, smoothTime, dt), Damp(from.Z, to.Z, ref velocity.Z, smoothTime, dt));
    }
    public static Vector4 Damp(Vector4 from, Vector4 to, ref Vector4 velocity, float smoothTime, float dt) {
        return new Vector4(
            Damp(from.X, to.X, ref velocity.X, smoothTime, dt),
            Damp(from.Y, to.Y, ref velocity.Y, smoothTime, dt), 
            Damp(from.Z, to.Z, ref velocity.Z, smoothTime, dt), 
            Damp(from.W, to.W, ref velocity.W, smoothTime, dt));
    }
    public static Quaternion Damp (Quaternion from, Quaternion to, ref Vector4 velocity, float smoothTime, float dt) {
        return new Quaternion(
            Damp(from.X, to.X, ref velocity.X, smoothTime, dt),
            Damp(from.Y, to.Y, ref velocity.Y, smoothTime, dt), 
            Damp(from.Z, to.Z, ref velocity.Z, smoothTime, dt), 
            Damp(from.W, to.W, ref velocity.W, smoothTime, dt));
    }

    public static bool CheckPointInRect(RectangleF rectangle, Vector2 position) {
        var min = rectangle.Location;
        var max = rectangle.Location + rectangle.Size;
        return (position.X < max.X && position.Y < max.Y) && (position.X > min.X && position.Y > min.Y);
    }
}
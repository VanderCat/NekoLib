namespace NekoLib.Extra;

internal static class Extensions {
    public static bool IsNullable(this Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
}
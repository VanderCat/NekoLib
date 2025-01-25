using System.Reflection.Metadata;

[assembly: MetadataUpdateHandler(typeof(NekoLib.Extra.ConsoleHotReloadService))]

namespace NekoLib.Extra;
internal delegate void ConsoleHotReloadCallback(Type[]? updatedTypes);
internal static class ConsoleHotReloadService {
    public static event ConsoleHotReloadCallback? OnClearCache;
    public static event ConsoleHotReloadCallback? OnUpdateApplication;

    private static void ClearCache(Type[]? updatedTypes) =>
        OnClearCache?.Invoke(updatedTypes);

    private static void UpdateApplication(Type[]? updatedTypes) =>
        OnUpdateApplication?.Invoke(updatedTypes);
}
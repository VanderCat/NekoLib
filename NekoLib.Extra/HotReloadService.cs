#if NET6_0_OR_GREATER
using System.Reflection.Metadata;
using Microsoft.Extensions.Logging;

[assembly: MetadataUpdateHandler(typeof(NekoLib.Extra.HotReloadService))]

namespace NekoLib.Extra;
public delegate void HotReloadCallback(Type[]? updatedTypes);
public static class HotReloadService {
    public static event HotReloadCallback? OnClearCache;
    public static event HotReloadCallback? OnUpdateApplication;

    public static readonly ILogger Logger = Logging.GetFor("Hot Reload");

    private static void ClearCache(Type[]? updatedTypes) =>
        OnClearCache?.Invoke(updatedTypes);

    private static void UpdateApplication(Type[]? updatedTypes) =>
        OnUpdateApplication?.Invoke(updatedTypes);

    static HotReloadService() {
        OnUpdateApplication += types => {
            Logger.LogTrace("The application was hot reloaded! Affected types: {Types}", [types]);
        };
    }
}
#endif
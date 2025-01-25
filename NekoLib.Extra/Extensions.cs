using NekoLib.Scenes;

namespace NekoLib.Extra;

public static class Extensions {
    public static AttachMode UseTemporarily(this IScene scene) {
        var prev = SceneManager.ActiveScene;
        SceneManager.SetSceneActive(scene);
        return new AttachMode(() => {
            SceneManager.SetSceneActive(prev);
        });
    }
}
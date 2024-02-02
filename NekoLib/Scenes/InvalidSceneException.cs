namespace NekoLib.Scenes;

public class InvalidSceneException : Exception
{
    public InvalidSceneException()
    {
    }

    public InvalidSceneException(string message)
        : base(message)
    {
    }

    public InvalidSceneException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
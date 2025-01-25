using System.Reflection;

namespace NekoLib.Filesystem; 

public static class Extensions {
    public static void Mount(this IMountable mountPoint) => Files.Mount(mountPoint);
    public static void Unmount(this IMountable mountPoint) => Files.Unmount(mountPoint);
    
    public static DirectoryInfo GetExecutingDirectory(this Assembly assembly)
    {
        var location = new Uri(assembly.GetName().CodeBase);
        return new FileInfo(location.AbsolutePath).Directory;
    }
}
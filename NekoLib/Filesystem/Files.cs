﻿using NekoLib.Filesystem.Exceptions;
using NekoLib.Logging;

namespace NekoLib.Filesystem;

public static class Files {
    private static List<IMountable> _mounted = new();
    
    public static void Mount(IMountable mountPoint) {
        mountPoint.OnMount();
        _mounted.Add(mountPoint);
    }

    public static void UnmountAll() {
        foreach (var fs in _mounted) {
            fs.Dispose();
        }
        _mounted.Clear();
    }

    public static void Unmount(IMountable mountPoint) {
        if (!_mounted.Contains(mountPoint))
            throw new ArgumentException("Provided mount point is not mounted!");
        mountPoint.Dispose();
        _mounted.Remove(mountPoint);
    }

    public static IFile GetFile(string path) {
        if (_mounted.Count < 1)
            throw new NoFilesystemException();
        for (int i = _mounted.Count - 1; i >= 0; i--) {
            if (!_mounted[i].FileExists(path)) continue;
            return _mounted[i].GetFile(path);
        }

        throw new FileNotFoundException();
    }

    public static bool FileExists(string path) {
        return _mounted.Any(fs => fs.FileExists(path));
    }

    public static IMountable GetWritableFilesystem() {
        if (_mounted.Any(x => !x.IsReadOnly))
            return _mounted.First(x => !x.IsReadOnly);
        throw new Exception("There is no writable filesystem mounted");
    }

    public static IEnumerable<string> ListDirectories(string path) {
        if (_mounted.Count < 1)
            throw new NoFilesystemException();
        //TODO: add check if folder exists
        HashSet<string> allFiles = new();
        for (int i = _mounted.Count - 1; i >= 0; i--) {
            try {
                allFiles.UnionWith(_mounted[i].ListDirectories(path));
            }
            catch (Exception e) {
                ILogger.Debug("listing failed with error: {0}", e);
            }
        }

        return allFiles;
    }
    
    public static IEnumerable<string> ListFiles(string path) {
        if (_mounted.Count < 1)
            throw new NoFilesystemException();
        //TODO: add check if folder exists
        HashSet<string> allFiles = new();
        for (int i = _mounted.Count - 1; i >= 0; i--) {
            try {
                allFiles.UnionWith(_mounted[i].ListFiles(path));
            }
            catch (Exception e) {
                ILogger.Debug("listing failed with error: {0}", e);
            }
        }

        return allFiles;
    }

    /// <summary>
    /// Untested
    /// </summary>
    public static bool IsDirectory(string path) {
        for (int i = _mounted.Count - 1; i >= 0; i--) {
            try {
                if (_mounted[i].IsDirectory(path)) return true;
            }
            catch (Exception e) {
                ILogger.Debug("Is direcotory failed with error: {0}", e);
            }
        }
        return false;
    }
}
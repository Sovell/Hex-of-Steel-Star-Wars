using System.IO;
using UnityEngine;

public static class PrototypePaths
{
    // Resolve from the executable's data directory, never from the original user profile.
    public static string Root => Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
    public static string DataPath
    {
        get
        {
            string path = Path.Combine(Root, "Profile");
            Directory.CreateDirectory(path);
            return path;
        }
    }
}

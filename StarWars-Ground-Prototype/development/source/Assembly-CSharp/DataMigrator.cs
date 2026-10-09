using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class DataMigrator
{
	private static readonly string[] OldCompanyNames = new string[10] { "Valentin's studios", "Valentin's Studios", "ValentinsStudios", "Valentinsstudio", "Valentinsstudios", "valentinsstudios", "valentinstudio", "Valentinstudio", "valentinstudios", "Valentin_s studios" };

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void MigrateLegacyInstallLocation()
	{
		if (PrototypeBootstrap.IsIsolatedProfile || Application.isEditor)
		{
			return;
		}
		string persistentDataPath = PrototypePaths.DataPath;
		string fullName = Directory.GetParent(persistentDataPath).FullName;
		string fullName2 = Directory.GetParent(fullName).FullName;
		string fileName = Path.GetFileName(persistentDataPath);
		string text = Path.Combine(persistentDataPath, ".dataMigration");
		if (File.Exists(text))
		{
			return;
		}
		Debug.Log(persistentDataPath);
		Debug.Log(fullName);
		Debug.Log(fullName2);
		Debug.Log(fileName);
		Debug.Log(text);
		Debug.Log("------");
		foreach (string item in Directory.EnumerateDirectories(fullName2))
		{
			string companyFolderName = Path.GetFileName(item);
			if (!OldCompanyNames.Any((string old) => string.Equals(old, companyFolderName, StringComparison.OrdinalIgnoreCase)))
			{
				continue;
			}
			string text2 = Path.Combine(item, fileName);
			if (Directory.Exists(text2))
			{
				try
				{
					CopyRecursive(text2, persistentDataPath);
					Debug.Log("Copyied install data.");
					File.WriteAllText(text, "Migration complete.");
					Debug.Log("Migrated from " + text2 + " → " + persistentDataPath);
					break;
				}
				catch (Exception arg)
				{
					Debug.LogError($"Moving install data failed: {arg}");
				}
			}
		}
	}

	private static void CopyRecursive(string sourceDir, string destDir)
	{
		string[] directories = Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories);
		for (int i = 0; i < directories.Length; i++)
		{
			Directory.CreateDirectory(directories[i].Replace(sourceDir, destDir));
		}
		directories = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
		foreach (string obj in directories)
		{
			string text = obj.Replace(sourceDir, destDir);
			Directory.CreateDirectory(Path.GetDirectoryName(text));
			File.Copy(obj, text, overwrite: true);
		}
	}
}

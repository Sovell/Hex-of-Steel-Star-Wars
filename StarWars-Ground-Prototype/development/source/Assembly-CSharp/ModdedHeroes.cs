using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class ModdedHeroes
{
	public const string FILENAME = "Custom heroes.txt";

	public List<Hero> heroes { get; set; } = new List<Hero>();

	public ModdedHeroes()
	{
		heroes = new List<Hero>();
	}

	public void Save(string p_fullPath)
	{
		if (string.IsNullOrEmpty(p_fullPath))
		{
			return;
		}
		string directoryName = Path.GetDirectoryName(p_fullPath);
		if (string.IsNullOrEmpty(directoryName))
		{
			return;
		}
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		using (FileStream serializationStream = new FileStream(p_fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			new BinaryFormatter().Serialize(serializationStream, this);
		}
	}

	public void Load(string p_fullPath)
	{
		if (!string.IsNullOrEmpty(p_fullPath) && File.Exists(p_fullPath))
		{
			ModdedHeroes moddedHeroes = (ModdedHeroes)Utils.ConvertByteArrayToObject(File.ReadAllBytes(p_fullPath));
			heroes = moddedHeroes.heroes;
			CheckInvalidData();
		}
	}

	public void CheckInvalidData()
	{
		if (heroes == null)
		{
			heroes = new List<Hero>();
		}
	}

	public void Reset()
	{
		heroes = new List<Hero>();
	}
}

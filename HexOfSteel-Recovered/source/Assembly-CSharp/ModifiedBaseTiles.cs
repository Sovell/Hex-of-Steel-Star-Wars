using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class ModifiedBaseTiles
{
	public const string FILENAME = "Modified base tiles.txt";

	public Dictionary<string, Tile> tiles { get; protected set; } = new Dictionary<string, Tile>();

	public ModifiedBaseTiles()
	{
		tiles = new Dictionary<string, Tile>();
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
			ModifiedBaseTiles modifiedBaseTiles = (ModifiedBaseTiles)Utils.ConvertByteArrayToObject(File.ReadAllBytes(p_fullPath));
			tiles = modifiedBaseTiles.tiles;
			CheckInvalidData();
		}
	}

	public void CheckInvalidData()
	{
		if (tiles == null)
		{
			tiles = new Dictionary<string, Tile>();
		}
	}

	public void Reset()
	{
		tiles = new Dictionary<string, Tile>();
	}
}

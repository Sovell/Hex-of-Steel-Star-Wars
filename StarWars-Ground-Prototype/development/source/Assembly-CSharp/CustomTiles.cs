using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class CustomTiles
{
	public const string FILENAME = "Custom tiles.txt";

	public List<Tile> ListOfCustomTiles { get; protected set; } = new List<Tile>();

	public CustomTiles()
	{
		ListOfCustomTiles = new List<Tile>();
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
			CustomTiles customTiles = (CustomTiles)Utils.ConvertByteArrayToObject(File.ReadAllBytes(p_fullPath));
			ListOfCustomTiles = customTiles.ListOfCustomTiles;
			CheckInvalidData();
		}
	}

	public void CheckInvalidData()
	{
		if (ListOfCustomTiles == null)
		{
			ListOfCustomTiles = new List<Tile>();
		}
	}

	public void Reset()
	{
		ListOfCustomTiles = new List<Tile>();
	}
}

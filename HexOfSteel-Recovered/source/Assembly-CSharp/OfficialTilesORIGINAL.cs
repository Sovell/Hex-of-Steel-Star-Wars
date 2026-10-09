using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class OfficialTilesORIGINAL
{
	public static string filePath = Application.streamingAssetsPath + "/Official tiles ORIGINAL.txt";

	public static OfficialTilesORIGINAL _officialTilesOriginal = null;

	public static readonly object _myLock = new object();

	public List<Tile> Tiles { get; set; }

	public static OfficialTilesORIGINAL GetInstance()
	{
		if (_officialTilesOriginal == null)
		{
			lock (_myLock)
			{
				if (_officialTilesOriginal == null)
				{
					_officialTilesOriginal = new OfficialTilesORIGINAL();
				}
			}
		}
		return _officialTilesOriginal;
	}

	public OfficialTilesORIGINAL()
	{
		Tiles = new List<Tile>();
	}

	public static void Save()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (FileStream serializationStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			binaryFormatter.Serialize(serializationStream, GetInstance());
		}
	}

	public static void Load()
	{
		if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
		{
			throw new FileNotFoundException("File not found.", filePath);
		}
		OfficialTiles officialTiles;
		try
		{
			using (FileStream serializationStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				officialTiles = (OfficialTiles)new BinaryFormatter().Deserialize(serializationStream);
			}
		}
		catch (SerializationException ex)
		{
			Debug.LogError("Save file is corrupted or incompatible: " + ex.Message);
			throw;
		}
		catch (IOException ex2)
		{
			Debug.LogError("Could not read save file: " + ex2.Message);
			throw;
		}
		GetInstance().Tiles = officialTiles.Tiles;
	}
}

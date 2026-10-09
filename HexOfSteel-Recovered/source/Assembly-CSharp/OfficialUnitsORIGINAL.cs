using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class OfficialUnitsORIGINAL
{
	public static string filePath = Application.streamingAssetsPath + "/Official units ORIGINAL.txt";

	public static OfficialUnitsORIGINAL _officialUnitsOriginal = null;

	public static readonly object _myLock = new object();

	public List<Unit> Units { get; set; }

	public static OfficialUnitsORIGINAL GetInstance()
	{
		if (_officialUnitsOriginal == null)
		{
			lock (_myLock)
			{
				if (_officialUnitsOriginal == null)
				{
					_officialUnitsOriginal = new OfficialUnitsORIGINAL();
				}
			}
		}
		return _officialUnitsOriginal;
	}

	public OfficialUnitsORIGINAL()
	{
		Units = new List<Unit>();
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
		OfficialUnits officialUnits;
		try
		{
			using (FileStream serializationStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				officialUnits = (OfficialUnits)new BinaryFormatter().Deserialize(serializationStream);
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
		GetInstance().Units = officialUnits.Units;
	}
}

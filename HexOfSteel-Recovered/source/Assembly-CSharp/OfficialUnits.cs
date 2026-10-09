using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class OfficialUnits
{
	public static string filePath = Application.streamingAssetsPath + "/Official units.txt";

	public static OfficialUnits _officialUnits = null;

	public static readonly object _myLock = new object();

	private static readonly HashSet<(string Country, string Name)> _unitKeys = new HashSet<(string, string)>();

	public List<Unit> Units { get; set; }

	public static OfficialUnits GetInstance()
	{
		if (_officialUnits == null)
		{
			lock (_myLock)
			{
				if (_officialUnits == null)
				{
					_officialUnits = new OfficialUnits();
				}
			}
		}
		return _officialUnits;
	}

	public OfficialUnits()
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
		_unitKeys.Clear();
		foreach (Unit unit in officialUnits.Units)
		{
			_unitKeys.Add((unit.Country, unit.Name));
		}
	}

	public static bool IsOfficialUnit(Unit p_unit)
	{
		if (p_unit == null)
		{
			return false;
		}
		return _unitKeys.Contains((p_unit.Country, p_unit.Name));
	}
}

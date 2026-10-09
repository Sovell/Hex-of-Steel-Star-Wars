using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class ModifiedBaseUnits
{
	public const string FILENAME = "Modified base units.txt";

	public List<Unit> Units { get; protected set; } = new List<Unit>();

	[Obsolete]
	public Dictionary<string, Unit> units { get; protected set; }

	public ModifiedBaseUnits()
	{
		Units = new List<Unit>();
		units = new Dictionary<string, Unit>();
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
			ModifiedBaseUnits modifiedBaseUnits = (ModifiedBaseUnits)Utils.ConvertByteArrayToObject(File.ReadAllBytes(p_fullPath));
			units = modifiedBaseUnits.units;
			Units = modifiedBaseUnits.Units;
			CheckInvalidData();
		}
	}

	public void CheckInvalidData()
	{
		if (Units == null)
		{
			Units = new List<Unit>();
		}
		if (units == null)
		{
			units = new Dictionary<string, Unit>();
		}
		RemoveDuplicates();
		ConvertToNewSystem();
		CheckInvalidAllowedRotations();
	}

	public void Reset()
	{
		Units = new List<Unit>();
		units = new Dictionary<string, Unit>();
	}

	private void RemoveDuplicates()
	{
		HashSet<Unit> hashSet = new HashSet<Unit>();
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Unit> unit in units)
		{
			Unit value = unit.Value;
			if (hashSet.Contains(value))
			{
				list.Add(unit.Key);
			}
			else
			{
				hashSet.Add(value);
			}
		}
		foreach (string item in list)
		{
			units.Remove(item);
		}
	}

	private void ConvertToNewSystem()
	{
		if (Units == null)
		{
			Units = new List<Unit>();
		}
		foreach (KeyValuePair<string, Unit> unit in units)
		{
			if (!Units.Contains(unit.Value))
			{
				Units.Add(unit.Value);
			}
		}
	}

	private void CheckInvalidAllowedRotations()
	{
		if (Units == null)
		{
			Units = new List<Unit>();
		}
		foreach (Unit unit in Units)
		{
			unit.SetDefaultAuthorizedRotation();
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class CustomUnits
{
	public const string FILENAME = "Custom units.txt";

	public List<Unit> ListOfCustomUnits { get; protected set; } = new List<Unit>();

	public CustomUnits()
	{
		ListOfCustomUnits = new List<Unit>();
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
			CustomUnits customUnits = (CustomUnits)Utils.ConvertByteArrayToObject(File.ReadAllBytes(p_fullPath));
			ListOfCustomUnits = customUnits.ListOfCustomUnits;
			CheckInvalidData();
		}
	}

	public void CheckInvalidData()
	{
		if (ListOfCustomUnits == null)
		{
			ListOfCustomUnits = new List<Unit>();
		}
		foreach (Unit listOfCustomUnit in ListOfCustomUnits)
		{
			listOfCustomUnit.SetDefaultAuthorizedRotation();
		}
		foreach (Unit listOfCustomUnit2 in ListOfCustomUnits)
		{
			if (listOfCustomUnit2.IsCommander && listOfCustomUnit2.CommanderTargets == null)
			{
				if (listOfCustomUnit2.FilterType == "FilterBoat")
				{
					listOfCustomUnit2.CommanderTargets = "FilterBoat";
				}
				else if (listOfCustomUnit2.FilterType == "FilterInfantry")
				{
					listOfCustomUnit2.CommanderTargets = "FilterInfantryFilterArtilleryFilterAntitank";
				}
				else if (listOfCustomUnit2.FilterType == "FilterTank")
				{
					listOfCustomUnit2.CommanderTargets = "FilterTankFilterArtilleryFilterAntitank";
				}
				else if (listOfCustomUnit2.Type == "Plane")
				{
					listOfCustomUnit2.CommanderTargets = "FilterAAFilterFighterFilterCASFilterBomber";
				}
			}
		}
	}

	public void Reset()
	{
		ListOfCustomUnits = new List<Unit>();
	}
}

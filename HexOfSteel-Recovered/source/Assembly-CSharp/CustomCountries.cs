using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class CustomCountries
{
	public const string FILENAME = "Custom countries.txt";

	public List<string> ListOfCustomCountries { get; protected set; } = new List<string>();

	public CustomCountries()
	{
		ListOfCustomCountries = new List<string>();
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
			CustomCountries customCountries = (CustomCountries)Utils.ConvertByteArrayToObject(File.ReadAllBytes(p_fullPath));
			ListOfCustomCountries = customCountries.ListOfCustomCountries;
			CheckInvalidData();
		}
	}

	public void CheckInvalidData()
	{
		if (ListOfCustomCountries == null)
		{
			ListOfCustomCountries = new List<string>();
		}
	}

	public void Reset()
	{
		ListOfCustomCountries = new List<string>();
	}
}

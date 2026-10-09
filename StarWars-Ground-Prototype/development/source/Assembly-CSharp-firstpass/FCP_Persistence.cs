using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(FlexibleColorPicker))]
public class FCP_Persistence : MonoBehaviour
{
	public enum SaveStrategy
	{
		SessionOnly,
		File,
		PlayerPrefs
	}

	public string saveName = GenerateID();

	public SaveStrategy saveStrategy;

	private FlexibleColorPicker fcp;

	private static Dictionary<string, Color> savedColors;

	private static string saveFilePath;

	private static bool saveFileLoaded;

	private static bool saveFileOutdated;

	private void Awake()
	{
		// Keep color-picker preferences in this prototype's profile, not the shared registry.
		saveStrategy = SaveStrategy.File;
		fcp = GetComponent<FlexibleColorPicker>();
		InitStatic();
	}

	private void InitStatic()
	{
		if (saveFilePath == null)
		{
			saveFilePath = Path.Combine(PrototypePaths.DataPath, "FCP_SavedColors.txt");
		}
		if (savedColors == null)
		{
			savedColors = new Dictionary<string, Color>();
		}
		if (!saveFileLoaded & (saveStrategy == SaveStrategy.File))
		{
			LoadDataFile();
			saveFileLoaded = true;
		}
	}

	private void OnDestroy()
	{
		if (saveFileOutdated & (saveStrategy == SaveStrategy.File))
		{
			SaveDataFile();
			saveFileOutdated = false;
		}
	}

	private void OnEnable()
	{
		if (savedColors == null)
		{
			InitStatic();
		}
		if (LoadColor(out var c))
		{
			fcp.color = c;
		}
	}

	private void OnDisable()
	{
		SaveColor(fcp.color);
	}

	private void LoadDataFile()
	{
		string[] array = File.ReadAllLines(saveFilePath);
		foreach (string text in array)
		{
			int num = text.LastIndexOf('#');
			if (num >= 0 && ColorUtility.TryParseHtmlString(text.Substring(num, text.Length - num), out var color))
			{
				savedColors.Add(text.Substring(0, num), color);
			}
		}
	}

	private void SaveDataFile()
	{
		string[] array = new string[savedColors.Count];
		int num = 0;
		foreach (KeyValuePair<string, Color> savedColor in savedColors)
		{
			array[num++] = savedColor.Key + "#" + ColorUtility.ToHtmlStringRGBA(savedColor.Value);
		}
		File.WriteAllText(saveFilePath, string.Join("\r\n", array));
	}

	public void SaveColor(Color c)
	{
		if (saveStrategy == SaveStrategy.PlayerPrefs)
		{
			PlayerPrefs.SetString("FCP_sc_" + saveName, "#" + ColorUtility.ToHtmlStringRGBA(c));
		}
		else if (savedColors.ContainsKey(saveName))
		{
			saveFileOutdated |= savedColors[saveName] != c;
			savedColors[saveName] = c;
		}
		else
		{
			savedColors.Add(saveName, c);
			saveFileOutdated = true;
		}
	}

	public bool LoadColor(out Color c)
	{
		c = Color.black;
		if (saveStrategy == SaveStrategy.PlayerPrefs)
		{
			string key = "FCP_sc_" + saveName;
			if (!PlayerPrefs.HasKey(key))
			{
				return false;
			}
			if (!ColorUtility.TryParseHtmlString(PlayerPrefs.GetString(key), out c))
			{
				return false;
			}
		}
		else
		{
			if (!savedColors.ContainsKey(saveName))
			{
				return false;
			}
			c = savedColors[saveName];
		}
		return true;
	}

	private static string GenerateID()
	{
		return Convert.ToBase64String(BitConverter.GetBytes(DateTime.Now.Ticks));
	}
}

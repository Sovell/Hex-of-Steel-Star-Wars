using System;
using UnityEngine;

[Serializable]
public class ModManifest
{
	public string modName = "None";

	public string modVersion = "None";

	public string modAuthor = "None";

	public string supportedGameVersion = "Unknown";

	public string dependencies = "None";

	public string modDescription = "None";

	public ModManifest()
	{
	}

	public ModManifest(string p_modName)
	{
		modName = p_modName;
		modVersion = "0.0.1";
		supportedGameVersion = Application.version + "+";
		modAuthor = PlayerSettings.Instance.Username ?? "None";
	}
}

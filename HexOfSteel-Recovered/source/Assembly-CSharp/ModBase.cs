using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class ModBase
{
	public string Name { get; protected set; }

	public string InstallLocation { get; protected set; }

	public ModPropertyBag ModDataBag { get; } = new ModPropertyBag();

	public CustomUnits CustomUnits { get; protected set; } = new CustomUnits();

	public CustomTiles CustomTiles { get; protected set; } = new CustomTiles();

	public CustomCountries CustomCountries { get; protected set; } = new CustomCountries();

	public ModdedHeroes ModdedHeroes { get; protected set; } = new ModdedHeroes();

	public ModifiedBaseUnits ModifiedBaseUnits { get; protected set; } = new ModifiedBaseUnits();

	public ModifiedBaseTiles ModifiedBaseTiles { get; protected set; } = new ModifiedBaseTiles();

	public List<Assembly> Assemblies { get; protected set; } = new List<Assembly>();

	public List<GameModification> GameModifications { get; protected set; } = new List<GameModification>();

	public abstract void LoadAssemblies();

	public abstract void UnloadMod();

	public virtual void Log(string p_content, LogType p_logType = LogType.Log)
	{
		string message = "[MOD:" + Name + "] " + p_content;
		switch (p_logType)
		{
		default:
			Debug.Log(message);
			break;
		case LogType.Warning:
			Debug.LogWarning(message);
			break;
		case LogType.Error:
			Debug.LogError(message);
			break;
		}
	}
}

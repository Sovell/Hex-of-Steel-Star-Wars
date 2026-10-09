using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Mod : ModBase
{
	public Mod(string p_modName, string p_installLocation)
	{
		Name = p_modName;
		InstallLocation = p_installLocation;
		CustomUnits = new CustomUnits();
		CustomTiles = new CustomTiles();
		CustomCountries = new CustomCountries();
		ModifiedBaseUnits = new ModifiedBaseUnits();
		ModifiedBaseTiles = new ModifiedBaseTiles();
		ModdedHeroes = new ModdedHeroes();
		Assemblies = new List<Assembly>();
		GameModifications = new List<GameModification>();
	}

	public bool HasGameplayImpact()
	{
		if (GameModifications.Count <= 0 && CustomUnits.ListOfCustomUnits.Count <= 0 && CustomTiles.ListOfCustomTiles.Count <= 0 && ModifiedBaseUnits.Units.Count <= 0)
		{
			return ModifiedBaseTiles.tiles.Count > 0;
		}
		return true;
	}

	public override void LoadAssemblies()
	{
		GameModifications = new List<GameModification>();
		Assemblies = Assemblies?.OrderBy((Assembly a) => a?.FullName, StringComparer.OrdinalIgnoreCase).Where((Assembly a) => a != null).ToList() ?? new List<Assembly>();
		List<string> list = new List<string>();
		foreach (Assembly assembly in Assemblies)
		{
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition || !typeof(GameModification).IsAssignableFrom(type))
				{
					continue;
				}
				try
				{
					GameModification gameModification = (GameModification)Activator.CreateInstance(type, this);
					if (gameModification != null)
					{
						GameModifications.Add(gameModification);
					}
				}
				catch (Exception arg)
				{
					string text = $"Failed to create '{type.FullName}' from '{assembly.GetName().Name}': {arg}";
					Debug.LogError(text);
					list.Add(text);
				}
			}
		}
		foreach (GameModification gameModification2 in GameModifications)
		{
			try
			{
				gameModification2.OnModInitialization(this);
			}
			catch (Exception arg2)
			{
				string text2 = $"Error during mod initialization '{gameModification2.GetType().FullName}' in '{Name}': {arg2}";
				Debug.LogError(text2);
				UIManager.ShowMessage(text2);
				list.Add(text2);
			}
		}
		if (list.Count > 0)
		{
			UIManager.ShowMessage("Some mods failed to load/initialize for library '" + Name + "'.\n\n" + string.Join("\n", list));
		}
	}

	public override void UnloadMod()
	{
		foreach (GameModification gameModification in GameModifications)
		{
			try
			{
				gameModification.OnModUnloaded();
			}
			catch (Exception ex)
			{
				string text = "Undoing game modification from mod: " + Name + " failed: " + ex.Message;
				Debug.LogError(text);
				UIManager.ShowMessage(text);
			}
		}
	}
}

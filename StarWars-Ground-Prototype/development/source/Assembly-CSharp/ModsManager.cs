using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class ModsManager : MonoBehaviour
{
	public static ModsManager instance;

	public const string LIBRARIES = "Libraries";

	public const string MAPS = "Maps";

	public const string SOUNDS = "Sounds";

	public const string MUSICS = "Musics";

	public const string VIDEOS = "Videos";

	public const string CUSTOM_HEROES = "Custom heroes";

	public const string CUSTOM_UNITS = "Custom units";

	public const string CUSTOM_COUNTRIES = "Custom countries";

	public const string CUSTOM_TILES = "Custom tiles";

	public const string WALLPAPERS = "Wallpapers";

	public const string GRAPHICS = "Graphics";

	public const string ATTACK = "Attack";

	public const string TILES = "Tiles";

	public const string UNITS = "Units";

	public const string FLAGS = "Flags";

	public const string HEROES = "Heroes";

	public const string MOVEMENT = "Movement";

	public const string MODIFIED_BASE_UNITS = "Modified base units";

	public const string MODIFIED_BASE_TILES = "Modified base tiles";

	public const int MODDED_UNITS_SIZE = 512;

	public const int MODDED_TILES_SIZE = 512;

	public const int MODDED_HEROES_SIZE = 512;

	public static Dictionary<string, string> workshopMods;

	public static Dictionary<string, string> workshopModsTitles;

	public static readonly string[] ModHierarchy = new string[20]
	{
		"Libraries", "Maps", "Videos", "Wallpapers", "Musics", "Graphics", "Graphics/UI", "Graphics/Tiles", "Graphics/Units", "Graphics/Heroes",
		"Graphics/Flags", "Custom tiles", "Custom units", "Modified base units", "Modified base tiles", "Custom countries", "Custom heroes", "Sounds", "Sounds/Attack", "Sounds/Movement"
	};

	public static Mod selectedMod;

	public static List<Mod> activeMods = new List<Mod>();

	public static List<FileInfo> moddedMaps = new List<FileInfo>();

	public static List<AudioClip> moddedMusics = new List<AudioClip>();

	public static List<AudioClip> moddedMovementSounds = new List<AudioClip>();

	public static List<AudioClip> moddedAttackSounds = new List<AudioClip>();

	public static List<Sprite> moddedUnitsGraphics = new List<Sprite>();

	public static List<Sprite> moddedFlagsGraphics = new List<Sprite>();

	public static List<Sprite> moddedHeroesGraphics = new List<Sprite>();

	public static List<Sprite> moddedTilesGraphics = new List<Sprite>();

	public static List<FileInfo> moddedWallpapers = new List<FileInfo>();

	public static List<FileInfo> moddedVideos = new List<FileInfo>();

	private static bool _hasLoadedAllMods;

	[Header("Setup")]
	[SerializeField]
	private GameObject _loadingContent_window;

	[SerializeField]
	private TextMeshProUGUI _modBeingLoaded_text;

	[SerializeField]
	private TextMeshProUGUI _fileLoaded_text;

	[SerializeField]
	private Slider _contentLoading_slider;

	[SerializeField]
	private GameObject _video_panel;

	[SerializeField]
	private VideoPlayer _videoPlayer;

	[SerializeField]
	private Image _wallpaper_Image;

	private Sprite _defaultWallpaper;

	private AudioClip _defaultSoundtrack;

	private int _numberOfFilesToLoad;

	private int _numberOfFilesLoaded;

	private bool _isContentBeingLoaded;

	private string _modBeingLoaded;

	private string _fileBeingLoaded;

	private readonly List<string> _authorizedExtensionsForModding = new List<string> { ".jpg", ".png", ".jpeg", ".wav", ".ogg", ".sav", ".txt", ".mp3", ".mp4", ".dll" };

	public static int ModdedCountriesCount
	{
		get
		{
			if (activeMods == null)
			{
				return 0;
			}
			int num = 0;
			foreach (Mod activeMod in activeMods)
			{
				if (activeMod.CustomCountries != null && activeMod.CustomCountries.ListOfCustomCountries != null)
				{
					num += activeMod.CustomCountries.ListOfCustomCountries.Count;
				}
			}
			return num;
		}
	}

	public static List<Unit> GetModdedUnitsChanges()
	{
		List<Unit> list = new List<Unit>();
		foreach (Mod activeMod in activeMods)
		{
			if (activeMod.ModifiedBaseUnits != null && activeMod.ModifiedBaseUnits.Units != null)
			{
				list.AddRange(activeMod.ModifiedBaseUnits.Units);
			}
		}
		return list;
	}

	public static List<Tile> GetModdedTilesChanges()
	{
		List<Tile> list = new List<Tile>();
		foreach (Mod activeMod in activeMods)
		{
			if (activeMod.ModifiedBaseTiles != null && activeMod.ModifiedBaseTiles.tiles != null)
			{
				list.AddRange(activeMod.ModifiedBaseTiles.tiles.Values.ToList());
			}
		}
		return list;
	}

	public static List<string> GetModdedCountries(bool p_sort)
	{
		List<string> list = new List<string>();
		foreach (Mod activeMod in activeMods)
		{
			if (activeMod.CustomCountries != null && activeMod.CustomCountries.ListOfCustomCountries != null)
			{
				list.AddRange(activeMod.CustomCountries.ListOfCustomCountries);
			}
		}
		if (p_sort)
		{
			list.Sort(StringComparer.Ordinal);
		}
		return list;
	}

	public static void SaveHeroesAllMods()
	{
		foreach (Mod activeMod in activeMods)
		{
			activeMod.ModdedHeroes.Save(Path.Combine(activeMod.InstallLocation, "Custom heroes", "Custom heroes.txt"));
		}
	}

	public static void SaveUnitsAllMods()
	{
		foreach (Mod activeMod in activeMods)
		{
			activeMod.CustomUnits.Save(Path.Combine(activeMod.InstallLocation, "Custom units", "Custom units.txt"));
			activeMod.ModifiedBaseUnits.Save(Path.Combine(activeMod.InstallLocation, "Modified base units", "Modified base units.txt"));
		}
	}

	public static void SaveTilesAllMods()
	{
		foreach (Mod activeMod in activeMods)
		{
			activeMod.CustomTiles.Save(Path.Combine(activeMod.InstallLocation, "Custom tiles", "Custom tiles.txt"));
			activeMod.ModifiedBaseTiles.Save(Path.Combine(activeMod.InstallLocation, "Modified base tiles", "Modified base tiles.txt"));
		}
	}

	public static void SaveCountriesAllMods()
	{
		foreach (Mod activeMod in activeMods)
		{
			activeMod.CustomCountries.Save(Path.Combine(activeMod.InstallLocation, "Custom countries", "Custom countries.txt"));
		}
	}

	public static Mod FindModByName(string p_modName)
	{
		foreach (Mod activeMod in activeMods)
		{
			if (activeMod.Name == p_modName)
			{
				return activeMod;
			}
		}
		return null;
	}

	public static void ApplyModdedUnitsChanges()
	{
		OfficialUnitsORIGINAL.Load();
		OfficialUnits.GetInstance().Units = OfficialUnitsORIGINAL.GetInstance().Units;
		List<Unit> moddedUnitsChanges = GetModdedUnitsChanges();
		if (moddedUnitsChanges.Count == 0)
		{
			return;
		}
		foreach (Unit unit in OfficialUnits.GetInstance().Units)
		{
			foreach (Unit item in moddedUnitsChanges)
			{
				if (unit.Country == item.Country && unit.Name == item.Name)
				{
					unit.ApplyModdedUnitModifications(item);
				}
			}
		}
	}

	public static void ApplyModdedTilesChanges()
	{
		OfficialTilesORIGINAL.Load();
		OfficialTiles.GetInstance().Tiles = OfficialTilesORIGINAL.GetInstance().Tiles;
		List<Tile> moddedTilesChanges = GetModdedTilesChanges();
		if (moddedTilesChanges.Count == 0)
		{
			return;
		}
		for (int i = 0; i < OfficialTiles.GetInstance().Tiles.Count; i++)
		{
			for (int j = 0; j < moddedTilesChanges.Count; j++)
			{
				if (OfficialTiles.GetInstance().Tiles[i].Name == moddedTilesChanges[j].Name && OfficialTiles.GetInstance().Tiles[i].Season == moddedTilesChanges[j].Season)
				{
					OfficialTiles.GetInstance().Tiles[i].Name = moddedTilesChanges[j].Name;
					OfficialTiles.GetInstance().Tiles[i].Season = moddedTilesChanges[j].Season;
					OfficialTiles.GetInstance().Tiles[i].Type = moddedTilesChanges[j].Type;
					OfficialTiles.GetInstance().Tiles[i].FilterType = moddedTilesChanges[j].FilterType;
					OfficialTiles.GetInstance().Tiles[i].MoneyReward = moddedTilesChanges[j].MoneyReward;
					OfficialTiles.GetInstance().Tiles[i].MoneyProductionPerTurn = moddedTilesChanges[j].MoneyProductionPerTurn;
					OfficialTiles.GetInstance().Tiles[i].OccupiedMoneyProductionPerTurn = moddedTilesChanges[j].OccupiedMoneyProductionPerTurn;
					OfficialTiles.GetInstance().Tiles[i].Manpower = moddedTilesChanges[j].Manpower;
					OfficialTiles.GetInstance().Tiles[i].OccupiedManpower = moddedTilesChanges[j].OccupiedManpower;
					OfficialTiles.GetInstance().Tiles[i].MPCost = moddedTilesChanges[j].MPCost;
					OfficialTiles.GetInstance().Tiles[i].DamageModificator = moddedTilesChanges[j].DamageModificator;
					OfficialTiles.GetInstance().Tiles[i].Bridge = moddedTilesChanges[j].Bridge;
					OfficialTiles.GetInstance().Tiles[i].Railroad = moddedTilesChanges[j].Railroad;
					OfficialTiles.GetInstance().Tiles[i].Capturable = moddedTilesChanges[j].Capturable;
					OfficialTiles.GetInstance().Tiles[i].IsReinforcementTile = moddedTilesChanges[j].IsReinforcementTile;
				}
			}
		}
	}

	public static void GetUserSubscribedWorkshopMods()
	{
		workshopMods = new Dictionary<string, string>();
		workshopModsTitles = new Dictionary<string, string>();
		if (MainMenu.isDemo || !SteamManager.IsSteamReady)
		{
			return;
		}
		PublishedFileId_t[] array = new PublishedFileId_t[SteamUGC.GetNumSubscribedItems()];
		SteamUGC.GetSubscribedItems(array, (uint)array.Length);
		PublishedFileId_t[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			PublishedFileId_t nPublishedFileID = array2[i];
			SteamUGC.GetItemInstallInfo(nPublishedFileID, out var _, out var pchFolder, 1024u, out var _);
			Dictionary<string, string> dictionary = workshopMods;
			ulong publishedFileId = nPublishedFileID.m_PublishedFileId;
			dictionary.Add(publishedFileId.ToString(), pchFolder);
			Dictionary<string, string> dictionary2 = workshopModsTitles;
			publishedFileId = nPublishedFileID.m_PublishedFileId;
			string key = publishedFileId.ToString();
			publishedFileId = nPublishedFileID.m_PublishedFileId;
			dictionary2[key] = publishedFileId.ToString();
		}
		if (workshopMods.Count > 0)
		{
			SteamManager.UnlockAchievement("Subscribe mod");
		}
		foreach (string IDString in workshopMods.Keys)
		{
			SteamAPICall_t hAPICall = SteamUGC.RequestUGCDetails(new PublishedFileId_t(ulong.Parse(IDString)), 0u);
			CallResult<SteamUGCRequestUGCDetailsResult_t>.Create().Set(hAPICall, (SteamUGCRequestUGCDetailsResult_t callback, bool ioFailure) =>
			{
				if (!ioFailure && callback.m_details.m_eResult == EResult.k_EResultOK)
				{
					workshopModsTitles[IDString] = callback.m_details.m_rgchTitle;
				}
			});
		}
	}

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		selectedMod = null;
		_defaultWallpaper = _wallpaper_Image.sprite;
		_defaultSoundtrack = SoundManager.instance.music_Source.clip;
		if (_hasLoadedAllMods)
		{
			ChangeWallpaperVideo();
			StartCoroutine(CR_ChangeWallpaper());
			PlayRandomModdedMusic(p_fromVideoPlayer: false);
		}
		else
		{
			LoadAllMods();
		}
	}

	private void Update()
	{
		_loadingContent_window.SetActive(_isContentBeingLoaded);
		if (_isContentBeingLoaded)
		{
			if (_numberOfFilesLoaded != 0 && _numberOfFilesToLoad != 0)
			{
				_modBeingLoaded_text.text = LocalizationManager.Translate("txt-modLoadingName") + _modBeingLoaded;
				_fileLoaded_text.text = _fileBeingLoaded;
				_contentLoading_slider.value = (float)_numberOfFilesLoaded / (float)_numberOfFilesToLoad;
			}
			else
			{
				_contentLoading_slider.value = 0f;
			}
		}
		else
		{
			_contentLoading_slider.value = 0f;
		}
	}

	public void LoadAllMods()
	{
		StopAllCoroutines();
		foreach (Mod activeMod in activeMods)
		{
			activeMod.UnloadMod();
		}
		foreach (Sprite moddedUnitsGraphic in moddedUnitsGraphics)
		{
			UnityEngine.Object.Destroy(moddedUnitsGraphic);
		}
		foreach (Sprite moddedFlagsGraphic in moddedFlagsGraphics)
		{
			UnityEngine.Object.Destroy(moddedFlagsGraphic);
		}
		foreach (Sprite moddedTilesGraphic in moddedTilesGraphics)
		{
			UnityEngine.Object.Destroy(moddedTilesGraphic);
		}
		foreach (Sprite moddedHeroesGraphic in moddedHeroesGraphics)
		{
			UnityEngine.Object.Destroy(moddedHeroesGraphic);
		}
		foreach (AudioClip moddedMusic in moddedMusics)
		{
			UnityEngine.Object.Destroy(moddedMusic);
		}
		foreach (AudioClip moddedAttackSound in moddedAttackSounds)
		{
			UnityEngine.Object.Destroy(moddedAttackSound);
		}
		foreach (AudioClip moddedMovementSound in moddedMovementSounds)
		{
			UnityEngine.Object.Destroy(moddedMovementSound);
		}
		activeMods.Clear();
		moddedMaps.Clear();
		moddedVideos.Clear();
		moddedMusics.Clear();
		moddedAttackSounds.Clear();
		moddedMovementSounds.Clear();
		moddedTilesGraphics.Clear();
		moddedUnitsGraphics.Clear();
		moddedFlagsGraphics.Clear();
		moddedHeroesGraphics.Clear();
		moddedWallpapers.Clear();
		GC.Collect();
		GetUserSubscribedWorkshopMods();
		StartCoroutine(CR_LoadContent());
	}

	private IEnumerator CR_LoadContent()
	{
		_numberOfFilesLoaded = 0;
		_numberOfFilesToLoad = 0;
		yield return Resources.UnloadUnusedAssets();
		Dictionary<string, string> modPaths = new Dictionary<string, string>();
		foreach (string activeMod in PlayerSettings.Instance.ActiveMods)
		{
			string modPath = Path.Combine(PrototypePaths.DataPath, "MODS", activeMod);
			if (Directory.Exists(modPath))
			{
				modPaths.Add(activeMod, modPath);
				_numberOfFilesToLoad += (from path3 in Directory.GetFiles(modPath, "*", SearchOption.AllDirectories)
					where _authorizedExtensionsForModding.Contains(Path.GetExtension(path3)) && Path.GetDirectoryName(path3) != modPath
					select path3).Count();
			}
		}
		foreach (KeyValuePair<string, string> workshopMod in workshopMods)
		{
			string key = workshopMod.Key;
			string path = workshopMod.Value;
			if (path != null && PlayerSettings.Instance.ActiveMods.Contains(key))
			{
				modPaths.Add(key, path);
				_numberOfFilesToLoad += (from path3 in Directory.GetFiles(path, "*", SearchOption.AllDirectories)
					where _authorizedExtensionsForModding.Contains(Path.GetExtension(path3)) && Path.GetDirectoryName(path3) != path
					select path3).Count();
			}
		}
		_isContentBeingLoaded = _numberOfFilesToLoad > 0;
		Vector2 centerPivot = new Vector2(0.5f, 0.5f);
		foreach (string activeMod2 in PlayerSettings.Instance.ActiveMods)
		{
			if (!modPaths.TryGetValue(activeMod2, out var value))
			{
				continue;
			}
			_modBeingLoaded = activeMod2;
			if (workshopModsTitles.ContainsKey(activeMod2))
			{
				_modBeingLoaded = workshopModsTitles[activeMod2];
			}
			Mod mod = new Mod(_modBeingLoaded, value);
			DirectoryInfo directoryInfo = new DirectoryInfo(value);
			DirectoryInfo[] modDirectories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo activeDirectory in modDirectories)
			{
				FileInfo[] subDirectoryFiles = activeDirectory.GetFiles();
				switch (activeDirectory.Name)
				{
				case "Libraries":
				{
					string[] files = Directory.GetFiles(activeDirectory.FullName, "*dll", SearchOption.AllDirectories);
					foreach (string obj5 in files)
					{
						FileInfo fileInfo2 = new FileInfo(obj5);
						_fileBeingLoaded = fileInfo2.Name;
						Assembly item = Assembly.LoadFrom(obj5);
						mod.Assemblies.Add(item);
						_numberOfFilesLoaded++;
						yield return null;
					}
					break;
				}
				case "Maps":
				{
					string[] files = Directory.GetFiles(activeDirectory.FullName, "*", SearchOption.AllDirectories);
					for (int num = 0; num < files.Length; num++)
					{
						FileInfo fileInfo = new FileInfo(files[num]);
						if (!(fileInfo.Extension != ".sav"))
						{
							FileInfo activeFile = fileInfo;
							if (!(activeFile.Extension.ToLower() != ".sav"))
							{
								_fileBeingLoaded = activeFile.Name;
								moddedMaps.Add(activeFile);
								_numberOfFilesLoaded++;
								yield return null;
							}
						}
					}
					break;
				}
				case "Wallpapers":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						if (!(activeFile.Extension.ToLower() != ".png") || !(activeFile.Extension.ToLower() != ".jpg") || !(activeFile.Extension.ToLower() != ".jpeg"))
						{
							_fileBeingLoaded = activeFile.Name;
							moddedWallpapers.Add(activeFile);
							_numberOfFilesLoaded++;
							yield return null;
						}
					}
					break;
				case "Videos":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						if (!(activeFile.Extension.ToLower() != ".mp4"))
						{
							_fileBeingLoaded = activeFile.Name;
							moddedVideos.Add(activeFile);
							_numberOfFilesLoaded++;
							yield return null;
						}
					}
					break;
				case "Sounds":
				{
					DirectoryInfo[] rootSubDirectories = activeDirectory.GetDirectories();
					for (int num = 0; num < rootSubDirectories.Length; num++)
					{
						if (rootSubDirectories[num].Name == "Movement")
						{
							string[] filesFullPath = Directory.GetFiles(rootSubDirectories[num].FullName, "*", SearchOption.AllDirectories);
							for (int f = 0; f < filesFullPath.Length; f++)
							{
								FileInfo activeFile = new FileInfo(filesFullPath[f]);
								string text7 = activeFile.Extension.ToLowerInvariant();
								if (text7 != ".ogg" && text7 != ".wav" && text7 != ".mp3")
								{
									if (activeFile.Name != ".DS_Store")
									{
										UIManager.ShowMessage("Format error, needs to be .mp3 or .ogg or .wav : " + activeFile.Name);
									}
									continue;
								}
								_fileBeingLoaded = activeFile.Name;
								string absoluteUri2 = new Uri(activeFile.FullName).AbsoluteUri;
								StringBuilder stringBuilder2 = new StringBuilder(absoluteUri2);
								stringBuilder2.Replace("+", "%2B");
								absoluteUri2 = stringBuilder2.ToString();
								AudioType audioType2 = Utils.GetAudioType(text7);
								using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(absoluteUri2, audioType2))
								{
									yield return www.SendWebRequest();
									if (www.result == UnityWebRequest.Result.Success)
									{
										AudioClip content2 = DownloadHandlerAudioClip.GetContent(www);
										content2.name = Path.GetFileNameWithoutExtension(activeFile.FullName);
										moddedMovementSounds.Add(content2);
										_numberOfFilesLoaded++;
									}
									else
									{
										Debug.LogError("Error loading audio file " + activeFile.Name + ": " + www.error);
									}
								}
							}
						}
						else
						{
							if (!(rootSubDirectories[num].Name == "Attack"))
							{
								continue;
							}
							string[] filesFullPath = Directory.GetFiles(rootSubDirectories[num].FullName, "*", SearchOption.AllDirectories);
							for (int f = 0; f < filesFullPath.Length; f++)
							{
								FileInfo activeFile = new FileInfo(filesFullPath[f]);
								string text8 = activeFile.Extension.ToLowerInvariant();
								if (text8 != ".ogg" && text8 != ".wav" && text8 != ".mp3")
								{
									if (activeFile.Name != ".DS_Store")
									{
										UIManager.ShowMessage("Format error, needs to be .mp3 or .ogg or .wav : " + activeFile.Name);
									}
									continue;
								}
								_fileBeingLoaded = activeFile.Name;
								string absoluteUri3 = new Uri(activeFile.FullName).AbsoluteUri;
								StringBuilder stringBuilder3 = new StringBuilder(absoluteUri3);
								stringBuilder3.Replace("+", "%2B");
								absoluteUri3 = stringBuilder3.ToString();
								AudioType audioType3 = Utils.GetAudioType(text8);
								using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(absoluteUri3, audioType3))
								{
									yield return www.SendWebRequest();
									if (www.result == UnityWebRequest.Result.Success)
									{
										AudioClip content3 = DownloadHandlerAudioClip.GetContent(www);
										content3.name = Path.GetFileNameWithoutExtension(activeFile.FullName);
										moddedAttackSounds.Add(content3);
										_numberOfFilesLoaded++;
									}
									else
									{
										Debug.LogError("Error loading audio file " + activeFile.Name + ": " + www.error);
									}
								}
							}
						}
					}
					break;
				}
				case "Musics":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						string text6 = activeFile.Extension.ToLowerInvariant();
						if (text6 != ".ogg" && text6 != ".wav" && text6 != ".mp3")
						{
							if (activeFile.Name != ".DS_Store")
							{
								UIManager.ShowMessage("Format error, needs to be .mp3 or .ogg or .wav : " + activeFile.Name);
							}
							continue;
						}
						_fileBeingLoaded = activeFile.Name;
						string absoluteUri = new Uri(activeFile.FullName).AbsoluteUri;
						StringBuilder stringBuilder = new StringBuilder(absoluteUri);
						stringBuilder.Replace("+", "%2B");
						absoluteUri = stringBuilder.ToString();
						AudioType audioType = Utils.GetAudioType(text6);
						using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(absoluteUri, audioType))
						{
							((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
							yield return www.SendWebRequest();
							if (www.result == UnityWebRequest.Result.Success)
							{
								AudioClip content = DownloadHandlerAudioClip.GetContent(www);
								content.name = Path.GetFileNameWithoutExtension(activeFile.FullName);
								moddedMusics.Add(content);
								_numberOfFilesLoaded++;
							}
							else
							{
								Debug.LogError("Error loading audio file " + activeFile.Name + ": " + www.error);
							}
						}
					}
					break;
				case "Custom heroes":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						if (!(activeFile.Extension.ToLower() != ".txt"))
						{
							_fileBeingLoaded = activeFile.Name;
							try
							{
								mod.ModdedHeroes.Load(activeFile.FullName);
							}
							catch
							{
								UIManager.ShowMessage("Couldn't load: " + activeFile.Name + " from " + activeDirectory.Name + " folder.");
							}
							_numberOfFilesLoaded++;
							yield return null;
						}
					}
					break;
				case "Custom countries":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						if (!(activeFile.Extension.ToLower() != ".txt"))
						{
							_fileBeingLoaded = activeFile.Name;
							try
							{
								mod.CustomCountries.Load(activeFile.FullName);
							}
							catch
							{
								UIManager.ShowMessage("Couldn't load: " + activeFile.Name + " from " + activeDirectory.Name + " folder.");
							}
							_numberOfFilesLoaded++;
							yield return null;
						}
					}
					break;
				case "Custom units":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						if (!(activeFile.Extension.ToLower() != ".txt"))
						{
							_fileBeingLoaded = activeFile.Name;
							try
							{
								mod.CustomUnits.Load(activeFile.FullName);
							}
							catch
							{
								UIManager.ShowMessage("Couldn't load: " + activeFile.Name + " from " + activeDirectory.Name + " folder.");
							}
							_numberOfFilesLoaded++;
							yield return null;
						}
					}
					break;
				case "Custom tiles":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						if (!(activeFile.Extension.ToLower() != ".txt"))
						{
							_fileBeingLoaded = activeFile.Name;
							try
							{
								mod.CustomTiles.Load(activeFile.FullName);
							}
							catch
							{
								UIManager.ShowMessage("Couldn't load: " + activeFile.Name + " from " + activeDirectory.Name + " folder.");
							}
							_numberOfFilesLoaded++;
							yield return null;
						}
					}
					break;
				case "Modified base units":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						if (!(activeFile.Extension.ToLower() != ".txt"))
						{
							_fileBeingLoaded = activeFile.Name;
							try
							{
								mod.ModifiedBaseUnits.Load(activeFile.FullName);
							}
							catch
							{
								UIManager.ShowMessage("Couldn't load: " + activeFile.Name + " from " + activeDirectory.Name + " folder.");
							}
							_numberOfFilesLoaded++;
							yield return null;
						}
					}
					break;
				case "Modified base tiles":
					foreach (FileInfo activeFile in subDirectoryFiles)
					{
						if (!(activeFile.Extension.ToLower() != ".txt"))
						{
							_fileBeingLoaded = activeFile.Name;
							try
							{
								mod.ModifiedBaseTiles.Load(activeFile.FullName);
							}
							catch
							{
								UIManager.ShowMessage("Couldn't load: " + activeFile.Name + " from " + activeDirectory.Name + " folder.");
							}
							_numberOfFilesLoaded++;
							yield return null;
						}
					}
					break;
				case "Graphics":
				{
					DirectoryInfo[] rootSubDirectories = activeDirectory.GetDirectories();
					for (int num = 0; num < rootSubDirectories.Length; num++)
					{
						DirectoryInfo[] subSubDirectories = rootSubDirectories[num].GetDirectories();
						if (rootSubDirectories[num].Name == "Units")
						{
							for (int f = 0; f < subSubDirectories.Length; f++)
							{
								string[] filesFullPath = Directory.GetFiles(subSubDirectories[f].FullName, "*", SearchOption.AllDirectories);
								for (int f2 = 0; f2 < filesFullPath.Length; f2++)
								{
									FileInfo activeFile = new FileInfo(filesFullPath[f2]);
									if (activeFile.Extension.ToLower() != ".jpg" && activeFile.Extension.ToLower() != ".jpeg" && activeFile.Extension.ToLower() != ".png")
									{
										continue;
									}
									_fileBeingLoaded = activeFile.Name;
									byte[] data = File.ReadAllBytes(activeFile.FullName);
									int num2 = 512;
									Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true, linear: false);
									texture2D.LoadImage(data, markNonReadable: false);
									texture2D.Apply(updateMipmaps: true, makeNoLongerReadable: true);
									int origW = texture2D.width;
									int height = texture2D.height;
									int w = num2;
									int h = Mathf.RoundToInt((float)height * ((float)num2 / (float)origW));
									RenderTexture temporary = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
									temporary.filterMode = FilterMode.Bilinear;
									Graphics.Blit(texture2D, temporary);
									Texture2D dst = new Texture2D(w, h, TextureFormat.RGBA32, mipChain: true, linear: false)
									{
										filterMode = FilterMode.Bilinear,
										wrapMode = TextureWrapMode.Clamp
									};
									RenderTexture active = RenderTexture.active;
									RenderTexture.active = temporary;
									dst.ReadPixels(new Rect(0f, 0f, w, h), 0, 0);
									bool flag = activeFile.Name.StartsWith("Counter");
									if (!flag)
									{
										try
										{
											dst.Compress(highQuality: true);
										}
										catch (Exception arg)
										{
											Debug.LogError($"Could not compress {activeFile.Name}: {arg}");
										}
									}
									bool makeNoLongerReadable = !flag || !activeFile.Name.Contains("full square");
									dst.Apply(updateMipmaps: true, makeNoLongerReadable);
									RenderTexture.active = active;
									RenderTexture.ReleaseTemporary(temporary);
									UnityEngine.Object.Destroy(texture2D);
									yield return null;
									float num3 = (float)w / (float)origW;
									float pixelsPerUnit = 100f * num3;
									Sprite sprite = Sprite.Create(dst, new Rect(0f, 0f, w, h), centerPivot, pixelsPerUnit, 0u, SpriteMeshType.FullRect);
									if (Utils.GetFileNameWithoutExtension(activeFile) == "Flag")
									{
										sprite.name = activeFile.Directory.Name;
										moddedFlagsGraphics.Add(sprite);
										string path2 = activeFile.Directory.Name;
										string text = Path.Combine(mod.InstallLocation, "Graphics");
										string text2 = Path.Combine(text, "Flags");
										Directory.CreateDirectory(text);
										Directory.CreateDirectory(text2);
										string text3 = Path.Combine(text2, path2);
										Directory.CreateDirectory(text3);
										string text4 = Path.Combine(text3, Path.GetFileName(activeFile.FullName));
										if (!Path.GetFullPath(activeFile.FullName).Equals(Path.GetFullPath(text4), StringComparison.OrdinalIgnoreCase))
										{
											if (File.Exists(text4))
											{
												File.Delete(text4);
											}
											File.Copy(activeFile.FullName, text4);
											File.Delete(activeFile.FullName);
										}
									}
									else
									{
										sprite.name = activeFile.Directory.Name + " " + Utils.GetFileNameWithoutExtension(activeFile);
										moddedUnitsGraphics.Add(sprite);
									}
									_numberOfFilesLoaded++;
								}
							}
						}
						else if (rootSubDirectories[num].Name == "Flags")
						{
							for (int num4 = 0; num4 < subSubDirectories.Length; num4++)
							{
								string[] filesFullPath = Directory.GetFiles(subSubDirectories[num4].FullName, "*", SearchOption.AllDirectories);
								for (int num5 = 0; num5 < filesFullPath.Length; num5++)
								{
									FileInfo activeFile = new FileInfo(filesFullPath[num5]);
									string text5 = activeFile.Extension.ToLowerInvariant();
									if (text5 != ".jpg" && text5 != ".jpeg" && text5 != ".png")
									{
										continue;
									}
									_fileBeingLoaded = activeFile.Name;
									byte[] data2 = File.ReadAllBytes(activeFile.FullName);
									Texture2D texture2D2 = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true, linear: false);
									if (!texture2D2.LoadImage(data2, markNonReadable: false))
									{
										UnityEngine.Object.Destroy(texture2D2);
										continue;
									}
									texture2D2.Apply(updateMipmaps: true, makeNoLongerReadable: true);
									int width = texture2D2.width;
									int height2 = texture2D2.height;
									int num6 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Log((float)Mathf.Max(width, height2) / 128f, 2f)), 0, texture2D2.mipmapCount - 1);
									int num7 = 1 << num6;
									int num8 = Mathf.Max(1, width / num7);
									int num9 = Mathf.Max(1, height2 / num7);
									Texture2D texture;
									if (num6 == 0)
									{
										texture2D2.filterMode = FilterMode.Bilinear;
										texture2D2.wrapMode = TextureWrapMode.Clamp;
										texture = texture2D2;
									}
									else
									{
										RenderTexture temporary2 = RenderTexture.GetTemporary(num8, num9, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
										texture2D2.filterMode = FilterMode.Bilinear;
										texture2D2.wrapMode = TextureWrapMode.Clamp;
										temporary2.filterMode = FilterMode.Bilinear;
										Graphics.Blit(texture2D2, temporary2);
										RenderTexture active2 = RenderTexture.active;
										RenderTexture.active = temporary2;
										Texture2D texture2D3 = new Texture2D(num8, num9, TextureFormat.RGBA32, mipChain: false, linear: false);
										texture2D3.filterMode = FilterMode.Bilinear;
										texture2D3.wrapMode = TextureWrapMode.Clamp;
										texture2D3.ReadPixels(new Rect(0f, 0f, num8, num9), 0, 0);
										texture2D3.Apply(updateMipmaps: false, makeNoLongerReadable: true);
										RenderTexture.active = active2;
										RenderTexture.ReleaseTemporary(temporary2);
										UnityEngine.Object.Destroy(texture2D2);
										texture = texture2D3;
									}
									float pixelsPerUnit2 = 100f * ((float)num8 / (float)width);
									Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, num8, num9), new Vector2(0.5f, 0.5f), pixelsPerUnit2, 0u, SpriteMeshType.FullRect);
									sprite.name = activeFile.Directory.Name;
									moddedFlagsGraphics.Add(sprite);
									_numberOfFilesLoaded++;
								}
							}
						}
						else if (rootSubDirectories[num].Name == "Tiles")
						{
							for (int f = 0; f < subSubDirectories.Length; f++)
							{
								string[] filesFullPath = Directory.GetFiles(subSubDirectories[f].FullName, "*", SearchOption.AllDirectories);
								for (int f2 = 0; f2 < filesFullPath.Length; f2++)
								{
									FileInfo activeFile = new FileInfo(filesFullPath[f2]);
									if (!(activeFile.Extension.ToLower() != ".jpg") || !(activeFile.Extension.ToLower() != ".jpeg") || !(activeFile.Extension.ToLower() != ".png"))
									{
										_fileBeingLoaded = activeFile.Name;
										byte[] data3 = File.ReadAllBytes(activeFile.FullName);
										int num10 = 512;
										Texture2D texture2D4 = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true, linear: false);
										texture2D4.LoadImage(data3, markNonReadable: false);
										texture2D4.Apply(updateMipmaps: true, makeNoLongerReadable: true);
										int h = texture2D4.width;
										int height3 = texture2D4.height;
										int w = num10;
										int origW = Mathf.RoundToInt((float)height3 * ((float)num10 / (float)h));
										RenderTexture temporary3 = RenderTexture.GetTemporary(w, origW, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
										temporary3.filterMode = FilterMode.Bilinear;
										Graphics.Blit(texture2D4, temporary3);
										Texture2D dst = new Texture2D(w, origW, TextureFormat.RGBA32, mipChain: false, linear: false)
										{
											filterMode = FilterMode.Bilinear,
											wrapMode = TextureWrapMode.Clamp
										};
										RenderTexture active3 = RenderTexture.active;
										RenderTexture.active = temporary3;
										dst.ReadPixels(new Rect(0f, 0f, w, origW), 0, 0);
										try
										{
											dst.Compress(highQuality: true);
										}
										catch (Exception arg2)
										{
											Debug.LogError($"Could not compress {activeFile.Name}: {arg2}");
										}
										dst.Apply(updateMipmaps: false, makeNoLongerReadable: true);
										RenderTexture.active = active3;
										RenderTexture.ReleaseTemporary(temporary3);
										UnityEngine.Object.Destroy(texture2D4);
										yield return null;
										float num11 = (float)w / (float)h;
										float pixelsPerUnit3 = 330f * num11;
										Sprite sprite = Sprite.Create(dst, new Rect(0f, 0f, w, origW), centerPivot, pixelsPerUnit3, 0u, SpriteMeshType.FullRect);
										sprite.name = activeFile.Directory.Name + " " + Utils.GetFileNameWithoutExtension(activeFile);
										moddedTilesGraphics.Add(sprite);
										_numberOfFilesLoaded++;
									}
								}
							}
						}
						else
						{
							if (!(rootSubDirectories[num].Name == "Heroes"))
							{
								continue;
							}
							for (int f = 0; f < subSubDirectories.Length; f++)
							{
								string[] filesFullPath = Directory.GetFiles(subSubDirectories[f].FullName, "*", SearchOption.AllDirectories);
								for (int f2 = 0; f2 < filesFullPath.Length; f2++)
								{
									FileInfo activeFile = new FileInfo(filesFullPath[f2]);
									if (!(activeFile.Extension.ToLower() != ".jpg") || !(activeFile.Extension.ToLower() != ".jpeg") || !(activeFile.Extension.ToLower() != ".png"))
									{
										_fileBeingLoaded = activeFile.Name;
										byte[] data4 = File.ReadAllBytes(activeFile.FullName);
										int num12 = 512;
										Texture2D texture2D5 = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true, linear: false);
										texture2D5.LoadImage(data4, markNonReadable: false);
										texture2D5.Apply(updateMipmaps: true, makeNoLongerReadable: true);
										int origW = texture2D5.width;
										int height4 = texture2D5.height;
										int w = num12;
										int h = Mathf.RoundToInt((float)height4 * ((float)num12 / (float)origW));
										RenderTexture temporary4 = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
										temporary4.filterMode = FilterMode.Bilinear;
										Graphics.Blit(texture2D5, temporary4);
										Texture2D dst = new Texture2D(w, h, TextureFormat.RGBA32, mipChain: false, linear: false)
										{
											filterMode = FilterMode.Bilinear,
											wrapMode = TextureWrapMode.Clamp
										};
										RenderTexture active4 = RenderTexture.active;
										RenderTexture.active = temporary4;
										dst.ReadPixels(new Rect(0f, 0f, w, h), 0, 0);
										dst.Apply(updateMipmaps: false, makeNoLongerReadable: true);
										RenderTexture.active = active4;
										RenderTexture.ReleaseTemporary(temporary4);
										UnityEngine.Object.Destroy(texture2D5);
										yield return null;
										float num13 = (float)w / (float)origW;
										float pixelsPerUnit4 = 100f * num13;
										Sprite sprite = Sprite.Create(dst, new Rect(0f, 0f, w, h), centerPivot, pixelsPerUnit4, 0u, SpriteMeshType.FullRect);
										sprite.name = activeFile.Directory.Name + " " + Utils.GetFileNameWithoutExtension(activeFile);
										moddedHeroesGraphics.Add(sprite);
										_numberOfFilesLoaded++;
									}
								}
							}
						}
					}
					break;
				}
				}
			}
			activeMods.Add(mod);
		}
		yield return null;
		GC.Collect();
		yield return null;
		foreach (Mod activeMod3 in activeMods)
		{
			activeMod3.LoadAssemblies();
		}
		yield return null;
		_hasLoadedAllMods = true;
		_isContentBeingLoaded = false;
		_fileBeingLoaded = string.Empty;
		_modBeingLoaded = string.Empty;
		yield return null;
		PlayRandomModdedMusic(p_fromVideoPlayer: false);
		ChangeWallpaperVideo();
		StartCoroutine(CR_ChangeWallpaper());
		ApplyModdedUnitsChanges();
		ApplyModdedTilesChanges();
		yield return null;
		PlayerSettings.Instance.SetLanguage(PlayerSettings.Instance.Language);
	}

	private void PlayRandomModdedMusic(bool p_fromVideoPlayer)
	{
		if (moddedMusics.Count == 0)
		{
			if (SoundManager.instance.music_Source.clip != _defaultSoundtrack)
			{
				SoundManager.instance.music_Source.clip = _defaultSoundtrack;
				SoundManager.instance.music_Source.Play();
			}
		}
		else if (p_fromVideoPlayer || moddedVideos.Count <= 0)
		{
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			SoundManager.instance.music_Source.clip = moddedMusics[UnityEngine.Random.Range(0, moddedMusics.Count)];
			SoundManager.instance.music_Source.Play();
			_videoPlayer.SetDirectAudioMute(0, mute: true);
		}
	}

	private IEnumerator CR_ChangeWallpaper()
	{
		if (moddedWallpapers.Count == 0)
		{
			_wallpaper_Image.sprite = _defaultWallpaper;
			yield break;
		}
		FileInfo fileInfo = moddedWallpapers[UnityEngine.Random.Range(0, moddedWallpapers.Count)];
		UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + fileInfo.FullName);
		yield return request.SendWebRequest();
		_wallpaper_Image.sprite = Sprite.Create(DownloadHandlerTexture.GetContent(request), new Rect(0f, 0f, DownloadHandlerTexture.GetContent(request).width, DownloadHandlerTexture.GetContent(request).height), new Vector2(0.5f, 0.5f));
	}

	private void ChangeWallpaperVideo()
	{
		if (moddedVideos.Count == 0)
		{
			_video_panel.SetActive(value: false);
			return;
		}
		_video_panel.SetActive(value: true);
		FileInfo fileInfo = moddedVideos[UnityEngine.Random.Range(0, moddedVideos.Count)];
		_videoPlayer.source = VideoSource.Url;
		_videoPlayer.url = fileInfo.FullName;
		_videoPlayer.isLooping = true;
		_videoPlayer.skipOnDrop = true;
		_videoPlayer.waitForFirstFrame = true;
		_videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
		_videoPlayer.EnableAudioTrack(0, enabled: true);
		_videoPlayer.SetDirectAudioVolume(0, PlayerSettings.Instance.MusicVolume);
		_videoPlayer.errorReceived += (VideoPlayer vp, string msg) =>
		{
			Debug.LogWarning("Video error: " + msg);
		};
		_videoPlayer.prepareCompleted += (VideoPlayer vp) =>
		{
			if (vp.audioTrackCount > 0)
			{
				SoundManager.instance.music_Source.Stop();
			}
			else
			{
				PlayRandomModdedMusic(p_fromVideoPlayer: true);
			}
			vp.Play();
		};
		_videoPlayer.Prepare();
	}
}

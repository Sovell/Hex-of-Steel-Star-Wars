using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModPanelGO : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Transform _availableModsContainer;

	[SerializeField]
	private Transform _activeModsContainer;

	[SerializeField]
	private ModSlotGO _availableModSlot_prefab;

	[SerializeField]
	private ModSlotGO _activeModSlot_prefab;

	[SerializeField]
	private GameObject _modInformationWindow;

	[SerializeField]
	private TextMeshProUGUI _modAuthor_text;

	[SerializeField]
	private TextMeshProUGUI _modVersion_text;

	[SerializeField]
	private TextMeshProUGUI _modDependencies_text;

	[SerializeField]
	private TextMeshProUGUI _modSupportedVersion_text;

	[SerializeField]
	private TextMeshProUGUI _modDescription_text;

	[SerializeField]
	private SteamWorkshopMenu _steamWorkshopMenu;

	[SerializeField]
	private Button _create_button;

	[SerializeField]
	private Image _thumbnail_image;

	[SerializeField]
	private TMP_InputField _modName_inputfield;

	private byte[] _filesCheckSum;

	private string _modsFolderLocation;

	private void Awake()
	{
		_modsFolderLocation = Path.Combine(Application.persistentDataPath, "MODS");
	}

	private void OnEnable()
	{
		StartCoroutine(CR_CheckFiles());
	}

	private void Update()
	{
		_create_button.interactable = !string.IsNullOrEmpty(_modName_inputfield.text);
	}

	public void ApplyChanges()
	{
		SoundManager.instance.PlayUIClickSound();
		ModsManager.instance.LoadAllMods();
		_modInformationWindow.SetActive(value: false);
	}

	public void OpenTutorialYoutube()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://www.youtube.com/watch?v=sGADvjll8pU");
	}

	public void OpenModsFolder()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start(Application.persistentDataPath + "/MODS");
	}

	public void OpenWorkshopPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!SteamManager.IsSteamReady)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-steam not connected"));
			return;
		}
		_steamWorkshopMenu.gameObject.SetActive(value: true);
		_steamWorkshopMenu.RefreshListOfUserMadeMods();
	}

	public void CreateMod()
	{
		SoundManager.instance.PlayUIClickSound();
		string text = _modName_inputfield.text.Trim();
		string text2 = Path.Combine(_modsFolderLocation, text);
		if (Directory.Exists(text2))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-mod already exist"));
			return;
		}
		try
		{
			Directory.CreateDirectory(text2);
			ModManifest obj = new ModManifest(text);
			File.WriteAllText(Path.Combine(text2, "Manifest.json"), JsonUtility.ToJson(obj, prettyPrint: true));
			string[] modHierarchy = ModsManager.ModHierarchy;
			foreach (string path in modHierarchy)
			{
				Directory.CreateDirectory(Path.Combine(text2, path));
			}
			foreach (string country in Database.Countries)
			{
				if (country != "None")
				{
					Directory.CreateDirectory(Path.Combine(text2, "Graphics/Flags", country));
					Directory.CreateDirectory(Path.Combine(text2, "Graphics/Units", country));
					Directory.CreateDirectory(Path.Combine(text2, "Graphics/Heroes", country));
				}
			}
			foreach (string season in Database.seasons)
			{
				Directory.CreateDirectory(Path.Combine(text2, "Graphics/Tiles", season));
			}
			new CustomTiles().Save(Path.Combine(text2, "Custom tiles", "Custom tiles.txt"));
			new CustomUnits().Save(Path.Combine(text2, "Custom units", "Custom units.txt"));
			new CustomCountries().Save(Path.Combine(text2, "Custom countries", "Custom countries.txt"));
			new ModdedHeroes().Save(Path.Combine(text2, "Custom heroes", "Custom heroes.txt"));
			new ModifiedBaseTiles().Save(Path.Combine(text2, "Modified base tiles", "Modified base tiles.txt"));
			new ModifiedBaseUnits().Save(Path.Combine(text2, "Modified base units", "Modified base units.txt"));
			CopyIfExists("Modding/Thumbnail.jpg", Path.Combine(text2, "Thumbnail.jpg"));
			CopyIfExists("Modding/README.rtf", Path.Combine(text2, "README.rtf"));
			CopyIfExists("Modding/NATO template.jpg", Path.Combine(text2, "NATO template.jpg"));
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Failed to create mod {text}: {arg}");
			UIManager.ShowMessage(LocalizationManager.Translate("error-mod creation failed"));
			return;
		}
		Process.Start(text2);
		_modName_inputfield.text = string.Empty;
		RefreshMods();
	}

	private void CopyIfExists(string p_sourceRelative, string p_destAbsolute)
	{
		string text = Path.Combine(Application.streamingAssetsPath, p_sourceRelative);
		if (File.Exists(text))
		{
			File.Copy(text, p_destAbsolute, overwrite: false);
		}
	}

	private void PurgeUI()
	{
		ModSlotGO[] componentsInChildren = _availableModsContainer.GetComponentsInChildren<ModSlotGO>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
		componentsInChildren = _activeModsContainer.GetComponentsInChildren<ModSlotGO>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	private void RefreshMods()
	{
		PurgeUI();
		List<ModItem> list = new List<ModItem>();
		string[] directories = Directory.GetDirectories(_modsFolderLocation);
		for (int i = 0; i < directories.Length; i++)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(directories[i]);
			if (Directory.Exists(directoryInfo.FullName))
			{
				list.Add(new ModItem(directoryInfo));
			}
		}
		if (SteamManager.IsSteamReady)
		{
			uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
			PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
			SteamUGC.GetSubscribedItems(array, numSubscribedItems);
			PublishedFileId_t[] array2 = array;
			foreach (PublishedFileId_t publishedFileId_t in array2)
			{
				if (SteamUGC.GetItemInstallInfo(publishedFileId_t, out var _, out var pchFolder, 1024u, out var _))
				{
					DirectoryInfo directoryInfo2 = new DirectoryInfo(pchFolder);
					if (Directory.Exists(directoryInfo2.FullName))
					{
						list.Add(new ModItem(directoryInfo2, publishedFileId_t));
					}
				}
				else
				{
					UnityEngine.Debug.LogError($"Could not retrieve item install info: {publishedFileId_t}");
				}
			}
		}
		List<string> list2 = new List<string>();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			ModItem modItem = list[num];
			try
			{
				EnsureManifest(modItem);
			}
			catch (Exception arg)
			{
				list.RemoveAt(num);
				list2.Add(modItem.name);
				UnityEngine.Debug.LogError($"Skipping mod '{modItem.name}' at '{modItem.installPath}': failed to prepare its manifest. {arg}");
			}
		}
		foreach (string modName in PlayerSettings.Instance.ActiveMods)
		{
			ModItem modItem2 = list.FirstOrDefault((ModItem m) => m.name == modName);
			if (modItem2 != null)
			{
				InstantiateSlot(_activeModSlot_prefab, _activeModsContainer, modItem2);
				list.Remove(modItem2);
			}
		}
		foreach (ModItem item in list.OrderBy((ModItem m) => m.name, StringComparer.OrdinalIgnoreCase))
		{
			InstantiateSlot(_availableModSlot_prefab, _availableModsContainer, item);
		}
		_modInformationWindow.SetActive(value: false);
		if (list2.Count > 0)
		{
			UIManager.ShowMessage(string.Format(LocalizationManager.Translate("error-mod manifest failed"), string.Join("\n", list2)));
		}
		if (!MainMenu.isDemo && list.Any((ModItem m) => m.isSteam))
		{
			SteamManager.UnlockAchievement("Subscribe mod");
		}
	}

	private void EnsureManifest(ModItem p_modItem)
	{
		if (Directory.Exists(p_modItem.installPath))
		{
			string path = Path.Combine(p_modItem.installPath, "Manifest.json");
			if (!File.Exists(path))
			{
				string contents = JsonUtility.ToJson(new ModManifest(), prettyPrint: true);
				File.WriteAllText(path, contents);
			}
		}
	}

	private void InstantiateSlot(ModSlotGO p_prefab, Transform p_parent, ModItem p_modItem)
	{
		if (Directory.Exists(p_modItem.installPath))
		{
			ModSlotGO modSlotGO = UnityEngine.Object.Instantiate(p_prefab, p_parent);
			modSlotGO.name = p_modItem.name;
			modSlotGO.modItem = p_modItem;
			modSlotGO.UpdateDisplayName();
			modSlotGO.mod_button.onClick.AddListener(() =>
			{
				ClickOnMod(p_modItem);
			});
			modSlotGO.deleteMod_button.onClick.AddListener(() =>
			{
				DeleteMod(p_modItem);
			});
			modSlotGO.openMod_button.onClick.AddListener(() =>
			{
				OpenModFolder(p_modItem);
			});
			modSlotGO.manageMod_button.onClick.AddListener(() =>
			{
				ToggleMod(p_modItem);
			});
			if (p_modItem.isSteam)
			{
				RetrieveModInfos(p_modItem.steamItemID);
			}
		}
	}

	private void OpenModFolder(ModItem p_modItem)
	{
		Process.Start(p_modItem.installPath);
	}

	private void DeleteMod(ModItem p_modItem)
	{
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("confirm-mod deletion"), p_modItem, ConfirmDeleteMod);
	}

	private void ConfirmDeleteMod(ModItem p_modItem)
	{
		if (p_modItem.isSteam)
		{
			SteamManager.instance.UnsubscribeFromWorkshopItem(p_modItem.steamItemID);
		}
		Directory.Delete(p_modItem.installPath, recursive: true);
		RefreshMods();
		PlayerSettings.Instance.ActiveMods.Remove(p_modItem.name);
	}

	private void ToggleMod(ModItem p_modItem)
	{
		if (PlayerSettings.Instance.ActiveMods.Contains(p_modItem.name))
		{
			PlayerSettings.Instance.ActiveMods.Remove(p_modItem.name);
		}
		else
		{
			PlayerSettings.Instance.ActiveMods.Add(p_modItem.name);
		}
		RefreshMods();
	}

	private void RetrieveModInfos(PublishedFileId_t p_publishedFileID)
	{
		SteamAPICall_t hAPICall = SteamUGC.RequestUGCDetails(p_publishedFileID, 30u);
		CallResult<SteamUGCRequestUGCDetailsResult_t>.Create().Set(hAPICall, OnUGCDetailsResult);
	}

	private void OnUGCDetailsResult(SteamUGCRequestUGCDetailsResult_t p_result, bool p_IOFailure)
	{
		if (p_IOFailure || p_result.m_details.m_eResult != EResult.k_EResultOK)
		{
			UnityEngine.Debug.LogError("Could not retrieve workshop item metadata");
			return;
		}
		ModSlotGO[] componentsInChildren = transform.GetComponentsInChildren<ModSlotGO>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].name == p_result.m_details.m_nPublishedFileId.ToString())
			{
				componentsInChildren[i].modName_text.text = $"{p_result.m_details.m_rgchTitle} - {componentsInChildren[i].modItem.installSize} MB";
			}
		}
	}

	private void ClickOnMod(ModItem p_modItem)
	{
		SoundManager.instance.PlayUIClickSound();
		_modInformationWindow.SetActive(value: true);
		StartCoroutine(CR_LoadThumbnail(p_modItem.installPath + "/Thumbnail.jpg"));
		string path = p_modItem.installPath + "/Manifest.json";
		ModManifest modManifest;
		if (File.Exists(path))
		{
			modManifest = JsonUtility.FromJson<ModManifest>(File.ReadAllText(path));
		}
		else
		{
			modManifest = new ModManifest();
			string contents = JsonUtility.ToJson(modManifest, prettyPrint: true);
			File.WriteAllText(path, contents);
		}
		_modAuthor_text.text = LocalizationManager.Translate("txt-mod author") + modManifest.modAuthor;
		_modVersion_text.text = LocalizationManager.Translate("txt-mod version") + modManifest.modVersion;
		_modDependencies_text.text = LocalizationManager.Translate("txt-mod dependencies") + modManifest.dependencies;
		_modSupportedVersion_text.text = LocalizationManager.Translate("txt-target version") + modManifest.supportedGameVersion;
		_modDescription_text.text = LocalizationManager.Translate("txt-mod descr") + modManifest.modDescription;
		_modInformationWindow.SetActive(value: true);
	}

	private IEnumerator CR_LoadThumbnail(string p_thumbnailPath)
	{
		if (!File.Exists(p_thumbnailPath))
		{
			ClearImage();
			yield break;
		}
		byte[] data = File.ReadAllBytes(p_thumbnailPath);
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGB24, mipChain: false);
		if (texture2D.LoadImage(data))
		{
			Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
			_thumbnail_image.sprite = sprite;
			_thumbnail_image.color = Color.white;
		}
		else
		{
			UnityEngine.Debug.LogError("Failed to decode thumbnail at " + p_thumbnailPath);
			ClearImage();
		}
		yield return null;
	}

	private void ClearImage()
	{
		_thumbnail_image.sprite = null;
		_thumbnail_image.color = new Color(0f, 0f, 0f, 0f);
	}

	private IEnumerator CR_CheckFiles()
	{
		_filesCheckSum = new byte[0];
		WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.1f);
		while (true)
		{
			byte[] array = Utils.ConvertObjectToByteArray(Directory.GetFiles(_modsFolderLocation, "*", SearchOption.AllDirectories));
			if (array.Length != _filesCheckSum.Length)
			{
				_filesCheckSum = array;
				RefreshMods();
			}
			yield return waitForSecondsRealtime;
		}
	}
}

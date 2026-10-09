using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamWorkshopMenu : MonoBehaviour
{
	private const string MOD_FOLDER_PATH_LABEL = "/War Frogs Studio/Hex Of Steel/MODS/";

	private const string MODS_DIR_NAME = "MODS";

	private const string THUMBNAIL_FILE = "Thumbnail.jpg";

	private const long MAX_THUMBNAIL_BYTES = 1000000L;

	[Header("Prefabs & Containers")]
	[SerializeField]
	private GameObject modSlotPrefab;

	[SerializeField]
	private Transform containerForAllMods;

	[Header("Panels / Buttons")]
	[SerializeField]
	private GameObject createModPanel;

	[SerializeField]
	private GameObject createTemplatesTilesButton;

	[SerializeField]
	private GameObject createTemplatesUnitsButton;

	[SerializeField]
	private GameObject invalidUploadSmall_Button;

	[SerializeField]
	private GameObject invalidUploadBig_Button;

	[SerializeField]
	private GameObject submitMod_Button;

	[SerializeField]
	private GameObject updateMod_Button;

	[SerializeField]
	private GameObject deleteMod_Button;

	[Header("Inputs / Toggles")]
	[SerializeField]
	private List<Toggle> tags_toggles = new List<Toggle>();

	[SerializeField]
	private TextMeshProUGUI modPath_Text;

	[SerializeField]
	private TMP_InputField modName_Inputfield;

	[SerializeField]
	private TMP_InputField modDescription_InputField;

	[SerializeField]
	private TMP_InputField changeLog_InputField;

	[SerializeField]
	private Toggle publicPrivate_Toggle;

	[Header("Upload progress")]
	[SerializeField]
	private GameObject upload_Panel;

	[SerializeField]
	private Slider upload_Slider;

	[SerializeField]
	private Image uploadProgress_image;

	[SerializeField]
	private float progressSmoothTime = 0.25f;

	private float _displayedProgress01;

	private float _targetProgress01;

	private float _progressVelocity;

	private GameObject _activeCheckmark;

	private HashSet<string> _selectedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private PublishedFileId_t _publishedFileID = PublishedFileId_t.Invalid;

	private UGCUpdateHandle_t _updateHandle = UGCUpdateHandle_t.Invalid;

	private UGCQueryHandle_t _userModsQueryHandle = UGCQueryHandle_t.Invalid;

	private CallResult<SteamUGCQueryCompleted_t> _userModsQueryResult;

	private CallResult<SteamUGCQueryCompleted_t> _singleModQueryResult;

	private CallResult<CreateItemResult_t> _createItemResult;

	private CallResult<SubmitItemUpdateResult_t> _submitItemUpdateResult;

	private CallResult<DeleteItemResult_t> _deleteItemResult;

	private string SafeModName => modName_Inputfield.text.Trim();

	private string CurrentModFolderPath => Path.Combine(PrototypePaths.DataPath, "MODS", SafeModName);

	private void Awake()
	{
		_userModsQueryResult = CallResult<SteamUGCQueryCompleted_t>.Create();
		_singleModQueryResult = CallResult<SteamUGCQueryCompleted_t>.Create();
		_createItemResult = CallResult<CreateItemResult_t>.Create();
		_submitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create();
		_deleteItemResult = CallResult<DeleteItemResult_t>.Create();
		SetupUIListeners();
		createModPanel.SetActive(value: false);
		SetUploadUI(visible: false);
		UpdateFormState();
		UpdateModPathLabel();
	}

	private void OnDisable()
	{
		createModPanel.SetActive(value: false);
	}

	private void Update()
	{
		if (_updateHandle == UGCUpdateHandle_t.Invalid)
		{
			_targetProgress01 = 0f;
			SetUploadUI(visible: false);
		}
		else
		{
			EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(_updateHandle, out var punBytesProcessed, out var punBytesTotal);
			_targetProgress01 = ComputeNormalizedTarget(itemUpdateProgress, punBytesProcessed, punBytesTotal);
			SetUploadUI(visible: true);
		}
		_displayedProgress01 = Mathf.SmoothDamp(_displayedProgress01, _targetProgress01, ref _progressVelocity, progressSmoothTime);
		upload_Slider.minValue = 0f;
		upload_Slider.maxValue = 1f;
		upload_Slider.value = _displayedProgress01;
		uploadProgress_image.fillAmount = _displayedProgress01;
	}

	private void SetupUIListeners()
	{
		modName_Inputfield.onValueChanged.AddListener((string _) =>
		{
			UpdateModPathLabel();
			UpdateFormState();
		});
		modDescription_InputField.onValueChanged.AddListener((string _) =>
		{
			UpdateFormState();
		});
		changeLog_InputField.onValueChanged.AddListener((string _) =>
		{
			UpdateFormState();
		});
		foreach (Toggle tags_toggle in tags_toggles)
		{
			Toggle localToggle = tags_toggle;
			localToggle.onValueChanged.AddListener((bool isOn) =>
			{
				OnTagToggleChanged(localToggle, isOn);
				UpdateFormState();
			});
		}
	}

	private void UpdateModPathLabel()
	{
		if (!(modPath_Text == null))
		{
			modPath_Text.text = LocalizationManager.Translate("txt-mod path") + "/War Frogs Studio/Hex Of Steel/MODS/" + SafeModName;
		}
	}

	private void OnTagToggleChanged(Toggle toggle, bool isOn)
	{
		if (toggle == null)
		{
			return;
		}
		string text = toggle.name;
		if (!string.IsNullOrWhiteSpace(text))
		{
			if (isOn)
			{
				_selectedTags.Add(text);
			}
			else
			{
				_selectedTags.Remove(text);
			}
		}
	}

	private void SetUploadUI(bool visible)
	{
		upload_Panel.SetActive(visible);
	}

	private void UpdateFormState()
	{
		bool flag = !string.IsNullOrWhiteSpace(SafeModName);
		bool flag2 = !string.IsNullOrWhiteSpace(modDescription_InputField.text);
		bool flag3 = !string.IsNullOrWhiteSpace(changeLog_InputField.text);
		bool flag4 = _selectedTags.Count > 0;
		bool flag5 = Directory.Exists(CurrentModFolderPath);
		bool flag6 = flag & flag2 & flag3 & flag4 & flag5;
		submitMod_Button.SetActive(flag6);
		createTemplatesTilesButton.SetActive(flag5 & flag);
		createTemplatesUnitsButton.SetActive(flag5 & flag);
		invalidUploadBig_Button.SetActive(!flag6 && !createTemplatesTilesButton.activeSelf && !createTemplatesUnitsButton.activeSelf);
		invalidUploadSmall_Button.SetActive(!flag6 && !invalidUploadBig_Button.activeSelf);
	}

	private void ClearModsListUI()
	{
		if (containerForAllMods == null)
		{
			return;
		}
		for (int num = containerForAllMods.childCount - 1; num >= 0; num--)
		{
			Transform child = containerForAllMods.GetChild(num);
			if (!(child == null) && !(child.gameObject == containerForAllMods.gameObject))
			{
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}
	}

	private void SetAllTagToggles(bool isOn)
	{
		foreach (Toggle tags_toggle in tags_toggles)
		{
			tags_toggle.SetIsOnWithoutNotify(isOn);
		}
		if (!isOn)
		{
			_selectedTags.Clear();
		}
	}

	private float ComputeNormalizedTarget(EItemUpdateStatus status, ulong processed, ulong total)
	{
		float num = 0f;
		float num2 = 0f;
		switch (status)
		{
		case EItemUpdateStatus.k_EItemUpdateStatusInvalid:
			num = 0f;
			num2 = 0f;
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
			num = 1f;
			num2 = 0f;
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
			num = 2f;
			num2 = 0f;
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
			num = 2f;
			num2 = ((total != 0) ? Mathf.Clamp01((float)processed / (float)total) : 0.5f);
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
			num = 3f;
			num2 = ((total != 0) ? Mathf.Clamp01((float)processed / (float)total) : 0.5f);
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
			return 1f;
		}
		return Mathf.Clamp01((num + num2) / 5f);
	}

	public void ClosePanel_Button()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(value: false);
	}

	public void ModifyModName()
	{
		UpdateModPathLabel();
	}

	public void CreateMod()
	{
		SoundManager.instance.PlayUIClickSound();
		createModPanel.SetActive(value: true);
		updateMod_Button.SetActive(value: false);
		deleteMod_Button.SetActive(value: false);
		EmptyForm();
		if (_activeCheckmark != null)
		{
			_activeCheckmark.SetActive(value: false);
		}
	}

	public void RefreshListOfUserMadeMods()
	{
		ClearModsListUI();
		SetAllTagToggles(isOn: false);
		publicPrivate_Toggle.SetIsOnWithoutNotify(value: false);
		_userModsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_All, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, SteamUtils.GetAppID(), SteamUtils.GetAppID(), 1u);
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(_userModsQueryHandle);
		_userModsQueryResult.Set(hAPICall, OnUserModsQueryCompleted);
	}

	public void DeleteMod()
	{
		SoundManager.instance.PlayUIClickSound();
		try
		{
			SteamAPICall_t hAPICall = SteamUGC.DeleteItem(_publishedFileID);
			_deleteItemResult.Set(hAPICall, OnDeleteItemResult);
		}
		catch (Exception)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot delete steam mod"));
		}
	}

	public void UploadMod()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!ValidateModFolderExists() || !ValidateThumbnail())
		{
			return;
		}
		try
		{
			SteamAPICall_t hAPICall = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeFirst);
			_createItemResult.Set(hAPICall, OnCreateItemResult);
		}
		catch
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot create steam mod"));
		}
	}

	public void UpdateItem()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!ValidateModFolderExists() || !ValidateThumbnail())
		{
			return;
		}
		_updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), _publishedFileID);
		SteamUGC.SetItemTitle(_updateHandle, SafeModName);
		SteamUGC.SetItemDescription(_updateHandle, modDescription_InputField.text);
		SteamUGC.SetItemTags(_updateHandle, _selectedTags.ToList());
		if (!SteamUGC.SetItemContent(_updateHandle, CurrentModFolderPath))
		{
			UIManager.ShowMessage("Content folder not found: " + CurrentModFolderPath);
			return;
		}
		string text = Path.Combine(CurrentModFolderPath, "Thumbnail.jpg");
		if (File.Exists(text) && !SteamUGC.SetItemPreview(_updateHandle, text))
		{
			UIManager.ShowMessage("Preview image not found: " + text);
			return;
		}
		SteamUGC.SetItemVisibility(_updateHandle, publicPrivate_Toggle.isOn ? ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate : ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(_updateHandle, changeLog_InputField.text);
		_submitItemUpdateResult.Set(hAPICall, OnSubmitItemUpdateResult);
	}

	public void CreateModFolder()
	{
		SoundManager.instance.PlayUIClickSound();
		if (Directory.Exists(CurrentModFolderPath))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-mod already exist"));
			return;
		}
		try
		{
			Directory.CreateDirectory(CurrentModFolderPath);
			string contents = JsonUtility.ToJson(new ModManifest(SafeModName), prettyPrint: true);
			File.WriteAllText(Path.Combine(CurrentModFolderPath, "Manifest.json"), contents);
			string[] modHierarchy = ModsManager.ModHierarchy;
			foreach (string path in modHierarchy)
			{
				Directory.CreateDirectory(Path.Combine(CurrentModFolderPath, path));
			}
			foreach (string country in Database.Countries)
			{
				if (country != "None")
				{
					Directory.CreateDirectory(Path.Combine(CurrentModFolderPath, "Graphics/Flags", country));
					Directory.CreateDirectory(Path.Combine(CurrentModFolderPath, "Graphics/Units", country));
					Directory.CreateDirectory(Path.Combine(CurrentModFolderPath, "Graphics/Heroes", country));
				}
			}
			foreach (string season in Database.seasons)
			{
				Directory.CreateDirectory(Path.Combine(CurrentModFolderPath, "Graphics/Tiles", season));
			}
			new CustomTiles().Save(Path.Combine(CurrentModFolderPath, "Custom tiles", "Custom tiles.txt"));
			new CustomUnits().Save(Path.Combine(CurrentModFolderPath, "Custom units", "Custom units.txt"));
			new CustomCountries().Save(Path.Combine(CurrentModFolderPath, "Custom countries", "Custom countries.txt"));
			new ModdedHeroes().Save(Path.Combine(CurrentModFolderPath, "Custom heroes", "Custom heroes.txt"));
			new ModifiedBaseTiles().Save(Path.Combine(CurrentModFolderPath, "Modified base tiles", "Modified base tiles.txt"));
			new ModifiedBaseUnits().Save(Path.Combine(CurrentModFolderPath, "Modified base units", "Modified base units.txt"));
			TryCopyFromStreamingAssets("Modding/Thumbnail.jpg", Path.Combine(CurrentModFolderPath, "Thumbnail.jpg"));
			TryCopyFromStreamingAssets("Modding/READ ME.rtf", Path.Combine(CurrentModFolderPath, "READ ME.rtf"));
			TryCopyFromStreamingAssets("Modding/NATO template.jpg", Path.Combine(CurrentModFolderPath, "NATO template.jpg"));
			Process.Start(CurrentModFolderPath);
			RefreshListOfUserMadeMods();
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"CreateModFolder failed: {arg}");
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot create steam mod"));
		}
		UpdateFormState();
	}

	public void CreateUnitsTemplates()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!ValidateModFolderExists())
		{
			return;
		}
		string text = Path.Combine(CurrentModFolderPath, "Graphics/Units");
		try
		{
			foreach (string country in Database.Countries)
			{
				string text2 = Path.Combine(text, country);
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
					CopyUnitTemplate("Droppable.png", text2);
					CopyUnitTemplate("Train.png", text2);
					CopyUnitTemplate("Landing craft.png", text2);
					CopyUnitTemplate("Infantry Landing craft.png", text2);
				}
			}
			foreach (Unit unit in OfficialUnits.GetInstance().Units)
			{
				string text3 = Path.Combine(text, unit.Country, unit.Name + ".png");
				if (!File.Exists(text3))
				{
					TryCopyFromStreamingAssets("Modding/Unit template.png", text3);
				}
			}
			foreach (Mod activeMod in ModsManager.activeMods)
			{
				foreach (Unit listOfCustomUnit in activeMod.CustomUnits.ListOfCustomUnits)
				{
					string text4 = Path.Combine(text, listOfCustomUnit.Country, listOfCustomUnit.Name + ".png");
					if (!File.Exists(text4))
					{
						TryCopyFromStreamingAssets("Modding/Unit template.png", text4);
					}
				}
			}
			Process.Start(text);
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"CreateUnitsTemplates failed: {arg}");
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot create unit templates"));
		}
	}

	public void CreateTilesTemplates()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!ValidateModFolderExists())
		{
			return;
		}
		string text = Path.Combine(CurrentModFolderPath, "Graphics/Tiles");
		try
		{
			foreach (Tile tile in OfficialTiles.GetInstance().Tiles)
			{
				string text2 = Path.Combine(text, tile.Season, tile.Name + ".png");
				if (!File.Exists(text2))
				{
					TryCopyFromStreamingAssets("Modding/Tile template.png", text2);
				}
			}
			foreach (Mod activeMod in ModsManager.activeMods)
			{
				foreach (Tile listOfCustomTile in activeMod.CustomTiles.ListOfCustomTiles)
				{
					string text3 = Path.Combine(text, listOfCustomTile.Season, listOfCustomTile.Name + ".png");
					if (!File.Exists(text3))
					{
						TryCopyFromStreamingAssets("Modding/Tile template.png", text3);
					}
				}
			}
			Process.Start(text);
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"CreateTilesTemplates failed: {arg}");
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot create tile templates"));
		}
	}

	private void OnUserModsQueryCompleted(SteamUGCQueryCompleted_t result, bool ioFailure)
	{
		if (ioFailure || result.m_eResult != EResult.k_EResultOK)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot retrieve client mods") + ": " + result.m_eResult);
			return;
		}
		try
		{
			for (uint num = 0u; num < result.m_unNumResultsReturned; num++)
			{
				if (SteamUGC.GetQueryUGCResult(_userModsQueryHandle, num, out var pDetails) && SteamUser.GetSteamID().m_SteamID == pDetails.m_ulSteamIDOwner)
				{
					CreateModSlot(pDetails);
				}
			}
		}
		finally
		{
			SteamUGC.ReleaseQueryUGCRequest(_userModsQueryHandle);
		}
	}

	private void CreateModSlot(SteamUGCDetails_t details)
	{
		if (modSlotPrefab == null || containerForAllMods == null)
		{
			return;
		}
		GameObject slot = UnityEngine.Object.Instantiate(modSlotPrefab, containerForAllMods, worldPositionStays: false);
		slot.name = details.m_nPublishedFileId.ToString();
		TextMeshProUGUI componentInChildren = slot.GetComponentInChildren<TextMeshProUGUI>();
		if (componentInChildren != null)
		{
			componentInChildren.text = details.m_rgchTitle;
			componentInChildren.fontSize = 20f;
		}
		Button component = slot.GetComponent<Button>();
		if (component != null)
		{
			PublishedFileId_t fileId = details.m_nPublishedFileId;
			component.onClick.AddListener(() =>
			{
				SelectModSlot(slot, fileId);
			});
		}
	}

	private void SelectModSlot(GameObject slotGO, PublishedFileId_t fileId)
	{
		SoundManager.instance.PlayUIClickSound();
		GameObject gameObject = slotGO.transform.Find("Checkmark")?.gameObject;
		if (gameObject != null)
		{
			gameObject.SetActive(value: true);
			if (_activeCheckmark != null && _activeCheckmark != gameObject)
			{
				_activeCheckmark.SetActive(value: false);
			}
			_activeCheckmark = gameObject;
		}
		createModPanel.SetActive(value: true);
		_publishedFileID = fileId;
		_userModsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_All, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, SteamUtils.GetAppID(), SteamUtils.GetAppID(), 1u);
		SteamUGC.SetReturnLongDescription(_userModsQueryHandle, bReturnLongDescription: true);
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(_userModsQueryHandle);
		_singleModQueryResult.Set(hAPICall, OnSingleModQueryCompleted);
	}

	private void OnSingleModQueryCompleted(SteamUGCQueryCompleted_t result, bool ioFailure)
	{
		if (ioFailure || result.m_eResult != EResult.k_EResultOK)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot retrieve mod info") + ": " + result.m_eResult);
			return;
		}
		try
		{
			for (uint num = 0u; num < result.m_unNumResultsReturned; num++)
			{
				if (!SteamUGC.GetQueryUGCResult(_userModsQueryHandle, num, out var pDetails) || _publishedFileID.m_PublishedFileId != pDetails.m_nPublishedFileId.m_PublishedFileId)
				{
					continue;
				}
				modName_Inputfield.text = pDetails.m_rgchTitle ?? string.Empty;
				modDescription_InputField.SetTextWithoutNotify(pDetails.m_rgchDescription ?? string.Empty);
				changeLog_InputField.SetTextWithoutNotify(string.Empty);
				publicPrivate_Toggle.SetIsOnWithoutNotify(pDetails.m_eVisibility != ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
				UpdateModPathLabel();
				_selectedTags.Clear();
				IEnumerable<string> source = from t in (pDetails.m_rgchTags ?? string.Empty).Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					select t.Trim();
				foreach (Toggle toggle in tags_toggles)
				{
					bool flag = source.Any((string tag) => string.Equals(tag, toggle.name, StringComparison.OrdinalIgnoreCase));
					toggle.SetIsOnWithoutNotify(flag);
					if (flag)
					{
						_selectedTags.Add(toggle.name);
					}
				}
				UpdateFormState();
				deleteMod_Button.SetActive(value: true);
				updateMod_Button.SetActive(Directory.Exists(CurrentModFolderPath));
				break;
			}
		}
		finally
		{
			SteamUGC.ReleaseQueryUGCRequest(_userModsQueryHandle);
		}
	}

	private void OnCreateItemResult(CreateItemResult_t p_result, bool p_IOfailure)
	{
		if (p_IOfailure || p_result.m_eResult != EResult.k_EResultOK)
		{
			if (p_IOfailure)
			{
				UnityEngine.Debug.Log("IO failure at workshop item creation: " + p_IOfailure);
			}
			else
			{
				UnityEngine.Debug.Log("Could not create workshop item: " + p_result.m_eResult);
			}
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot create steam mod"));
			return;
		}
		if (p_result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement");
		}
		_publishedFileID = p_result.m_nPublishedFileId;
		UpdateItem();
	}

	private void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t p_result, bool p_IOfailure)
	{
		if (p_IOfailure || p_result.m_eResult != EResult.k_EResultOK)
		{
			if (p_IOfailure)
			{
				UnityEngine.Debug.Log("IO failure at workshop item creation: " + p_IOfailure);
			}
			else
			{
				UnityEngine.Debug.Log("Could not update workshop item: " + p_result.m_eResult);
			}
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot submit steam mod") + ": " + p_result.m_eResult);
		}
		else
		{
			_updateHandle = UGCUpdateHandle_t.Invalid;
			UIManager.ShowMessage(LocalizationManager.Translate("info-successful steam mod upload"));
			createModPanel.SetActive(value: false);
			updateMod_Button.SetActive(value: false);
			deleteMod_Button.SetActive(value: false);
			EmptyForm();
			RefreshListOfUserMadeMods();
		}
	}

	private void OnDeleteItemResult(DeleteItemResult_t p_result, bool p_IOfailure)
	{
		if (p_IOfailure || p_result.m_eResult != EResult.k_EResultOK)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-cannot delete steam mod"));
			return;
		}
		RefreshListOfUserMadeMods();
		createModPanel.SetActive(value: false);
		updateMod_Button.SetActive(value: false);
		deleteMod_Button.SetActive(value: false);
	}

	private bool ValidateModFolderExists()
	{
		if (!Directory.Exists(CurrentModFolderPath))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-mod missing at location"));
			return false;
		}
		return true;
	}

	private bool ValidateThumbnail()
	{
		string text = Path.Combine(CurrentModFolderPath, "Thumbnail.jpg");
		if (!File.Exists(text))
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-missing thumbnail"));
			return false;
		}
		try
		{
			if (new FileInfo(text).Length > 1000000)
			{
				UIManager.ShowMessage(LocalizationManager.Translate("error-thumbnail too big"));
				return false;
			}
		}
		catch
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-missing thumbnail"));
			return false;
		}
		return true;
	}

	private void EmptyForm()
	{
		modName_Inputfield.text = string.Empty;
		modDescription_InputField.text = string.Empty;
		changeLog_InputField.text = string.Empty;
		modPath_Text.text = LocalizationManager.Translate("txt-mod path") + "/War Frogs Studio/Hex Of Steel/MODS/";
		SetAllTagToggles(isOn: false);
		UpdateFormState();
	}

	private void TryCopyFromStreamingAssets(string relativeSource, string destination)
	{
		try
		{
			string text = Path.Combine(Application.streamingAssetsPath, relativeSource);
			if (File.Exists(text))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(destination));
				File.Copy(text, destination, overwrite: false);
			}
		}
		catch
		{
			UnityEngine.Debug.Log("Couldn't copy files.");
		}
	}

	private void CopyUnitTemplate(string fileName, string toFolder)
	{
		string destination = Path.Combine(toFolder, fileName);
		TryCopyFromStreamingAssets("Modding/Unit template.png", destination);
	}
}

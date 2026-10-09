using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StandaloneScenariosMenu : MonoBehaviour
{
	[Header("Setup")]
	public Transform parent;

	public Button startButton;

	public Toggle FFAToggle;

	public Toggle lockFileToggle;

	public Toggle offlineMultiplayerToggle;

	public Transform playerInfoContainer_Transform;

	public GameObject mapNameSlot;

	public GameObject playerQuickGameSlot;

	public TextMeshProUGUI mapSizeText;

	public TMP_InputField mapSearch_inputfield;

	public GameObject achievement;

	public GameObject YESironman;

	public GameObject NOironman;

	public GameObject loadingScenarioPanel;

	private byte[] _filesCheckSum;

	private string _filesPath;

	private Coroutine _CR_loadingMap;

	private void Awake()
	{
		_filesPath = Application.persistentDataPath + "/Maps";
	}

	private void OnEnable()
	{
		startButton.gameObject.SetActive(value: false);
		achievement.SetActive(value: false);
		mapSizeText.gameObject.SetActive(value: false);
		FFAToggle.gameObject.SetActive(value: false);
		lockFileToggle.gameObject.SetActive(value: false);
		offlineMultiplayerToggle.gameObject.SetActive(value: false);
		mapSearch_inputfield.SetTextWithoutNotify("");
		StartCoroutine(CR_CheckFiles());
	}

	public void Browse()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start(_filesPath);
	}

	public void StartTutorial()
	{
		SoundManager.instance.PlayUIClickSound();
		RefreshListOfMaps();
		foreach (FileInfo item in MainMenu.listOfAllMapsQuickGame)
		{
			if (item.Extension == ".sav" && item.Name.Contains("Tutorial"))
			{
				loadingScenarioPanel.SetActive(value: true);
				MainMenu.selectedMap = item;
				GameData.Instance.Load(item.FullName);
				GameData.Instance.listOfPlayers[0].IsComputer = false;
				GameData.Instance.listOfPlayers[0].IsPlayingFirst = true;
				RefreshIronMan();
				Start2PlayersGame(p_ignoreConfirmationWindow: true);
				break;
			}
		}
	}

	public void RefreshListOfMaps()
	{
		startButton.gameObject.SetActive(value: false);
		achievement.SetActive(value: false);
		mapSizeText.gameObject.SetActive(value: false);
		FFAToggle.gameObject.SetActive(value: false);
		lockFileToggle.gameObject.SetActive(value: false);
		offlineMultiplayerToggle.gameObject.SetActive(value: false);
		MainMenu.GetAllCustomUserMadeMaps(p_includeCampaigns: false);
		foreach (Transform item in parent)
		{
			if (item != parent.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		foreach (Transform item2 in playerInfoContainer_Transform)
		{
			if (item2 != playerInfoContainer_Transform)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		Dictionary<FileInfo, string> dictionary = new Dictionary<FileInfo, string>();
		foreach (FileInfo item3 in MainMenu.listOfAllMapsQuickGame)
		{
			if (item3.Extension == ".sav")
			{
				dictionary.Add(item3, LocalizationManager.GetLocalizedScenario(item3));
			}
		}
		string text = null;
		bool flag = false;
		foreach (KeyValuePair<FileInfo, string> files in dictionary)
		{
			if (!string.IsNullOrEmpty(mapSearch_inputfield.text) && !files.Value.ToLower().Contains(mapSearch_inputfield.text.ToLower()))
			{
				continue;
			}
			GameObject gameObject2;
			if (files.Key.Directory.Name != text)
			{
				if (files.Key.Directory.Name != "Maps")
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(mapNameSlot, parent);
					gameObject.GetComponentInChildren<TextMeshProUGUI>().text = files.Key.Directory.Name;
					gameObject.GetComponentInChildren<TextMeshProUGUI>().color = MainMenu.yellowColor;
					gameObject.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.UpperCase;
					gameObject.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Midline;
					UnityEngine.Object.Destroy(gameObject.GetComponentInChildren<Button>());
					gameObject.transform.localScale = Vector3.one;
					text = files.Key.Directory.Name;
				}
				else if (!flag)
				{
					flag = true;
					gameObject2 = UnityEngine.Object.Instantiate(mapNameSlot, parent);
					gameObject2.name = "STANDALONE SCENARIOS";
					UnityEngine.Object.Destroy(gameObject2.GetComponentInChildren<Button>());
					TextMeshProUGUI componentInChildren = gameObject2.GetComponentInChildren<TextMeshProUGUI>();
					componentInChildren.text = LocalizationManager.Translate("txt-standalone scenarios");
					componentInChildren.fontStyle = FontStyles.UpperCase;
					componentInChildren.alignment = TextAlignmentOptions.Midline;
					componentInChildren.color = MainMenu.yellowColor;
					gameObject2.transform.localScale = Vector3.one;
				}
			}
			gameObject2 = UnityEngine.Object.Instantiate(mapNameSlot, parent);
			gameObject2.name = files.Key.Name;
			TextMeshProUGUI componentInChildren2 = gameObject2.GetComponentInChildren<TextMeshProUGUI>();
			componentInChildren2.text = Path.GetFileNameWithoutExtension(files.Value);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files.Key.Name);
			if (PlayerSettings.Instance.CompletedScenarioz.ContainsKey(fileNameWithoutExtension))
			{
				if (PlayerSettings.Instance.CompletedScenarioz[fileNameWithoutExtension].Contains("Allies"))
				{
					gameObject2.transform.Find("Won allies").gameObject.SetActive(value: true);
				}
				if (PlayerSettings.Instance.CompletedScenarioz[fileNameWithoutExtension].Contains("Axis"))
				{
					gameObject2.transform.Find("Won axis").gameObject.SetActive(value: true);
				}
			}
			componentInChildren2.text = componentInChildren2.text.Replace(".", string.Empty);
			componentInChildren2.text = componentInChildren2.text.Replace("_", string.Empty);
			gameObject2.GetComponentInChildren<Button>().onClick.AddListener(() =>
			{
				MapChoice(files.Key);
			});
			gameObject2.transform.localScale = Vector3.one;
		}
		if (MainMenu.listOfAllCampaigns.Count > 0)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(mapNameSlot, parent);
			gameObject2.name = "CAMPAIGNS";
			UnityEngine.Object.Destroy(gameObject2.GetComponentInChildren<Button>());
			TextMeshProUGUI componentInChildren3 = gameObject2.GetComponentInChildren<TextMeshProUGUI>();
			componentInChildren3.text = LocalizationManager.Translate("txt-campaign scenarios");
			componentInChildren3.fontStyle = FontStyles.UpperCase;
			componentInChildren3.alignment = TextAlignmentOptions.Midline;
			componentInChildren3.color = MainMenu.yellowColor;
			gameObject2.transform.localScale = Vector3.one;
		}
		MainMenu.listOfAllCampaigns = MainMenu.listOfAllCampaigns.OrderBy((FileInfo x) => x.Name).ToList();
		dictionary.Clear();
		foreach (FileInfo listOfAllCampaign in MainMenu.listOfAllCampaigns)
		{
			if (listOfAllCampaign.Extension == ".sav")
			{
				dictionary.Add(listOfAllCampaign, LocalizationManager.GetLocalizedScenario(listOfAllCampaign));
			}
		}
		foreach (KeyValuePair<FileInfo, string> files2 in dictionary)
		{
			if (!string.IsNullOrEmpty(mapSearch_inputfield.text) && !files2.Value.ToLower().Contains(mapSearch_inputfield.text.ToLower()))
			{
				continue;
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(mapNameSlot, parent);
			gameObject2.name = files2.Key.Name;
			TextMeshProUGUI componentInChildren4 = gameObject2.GetComponentInChildren<TextMeshProUGUI>();
			componentInChildren4.text = Path.GetFileNameWithoutExtension(files2.Value);
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(files2.Key.Name);
			if (PlayerSettings.Instance.CompletedScenarioz.ContainsKey(fileNameWithoutExtension2))
			{
				if (PlayerSettings.Instance.CompletedScenarioz[fileNameWithoutExtension2].Contains("Allies"))
				{
					gameObject2.transform.Find("Won allies").gameObject.SetActive(value: true);
				}
				if (PlayerSettings.Instance.CompletedScenarioz[fileNameWithoutExtension2].Contains("Axis"))
				{
					gameObject2.transform.Find("Won axis").gameObject.SetActive(value: true);
				}
			}
			componentInChildren4.text = componentInChildren4.text.Replace(".", string.Empty);
			componentInChildren4.text = componentInChildren4.text.Replace("_", string.Empty);
			componentInChildren4.text = componentInChildren4.text + " - " + LocalizationManager.GetLocalizedCampaignSide(new DirectoryInfo(Directory.GetParent(Directory.GetParent(Directory.GetParent(files2.Key.FullName).ToString()).ToString()).ToString()).Name);
			gameObject2.GetComponentInChildren<Button>().onClick.AddListener(() =>
			{
				MapChoice(files2.Key);
			});
			gameObject2.transform.localScale = Vector3.one;
		}
		RefreshIronMan();
	}

	public void MapChoice(FileInfo p_fileInfo)
	{
		SoundManager.instance.PlayUIClickSound();
		if (_CR_loadingMap != null)
		{
			StopCoroutine(_CR_loadingMap);
		}
		_CR_loadingMap = StartCoroutine(CR_LoadMap(p_fileInfo));
	}

	public void ChangePlayerIncome(Player p_player, Slider p_slider)
	{
		SoundManager.instance.PlayUIClickSound();
		p_player.IncomeModifier = (byte)p_slider.value;
		p_slider.GetComponentInChildren<TextMeshProUGUI>().text = p_player.IncomeModifier * 10 + "%";
		RefreshIronMan();
	}

	public void ChangeConfiguration(Player p_player, TMP_Dropdown p_dropdown, Slider p_incomeSlider)
	{
		SoundManager.instance.PlayUIClickSound();
		p_player.IsComputer = p_dropdown.captionText.text == p_dropdown.options[0].text;
		p_player.SetDefaultIncomeModifier();
		p_incomeSlider.value = (int)p_player.IncomeModifier;
		RefreshIronMan();
	}

	public void SetPlaysFirst(Player p_player, Toggle p_toggle)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.IsPlayingFirst = false;
		}
		p_player.IsPlayingFirst = true;
		Toggle[] componentsInChildren = playerInfoContainer_Transform.GetComponentsInChildren<Toggle>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetIsOnWithoutNotify(value: false);
		}
		p_toggle.SetIsOnWithoutNotify(value: true);
		startButton.gameObject.SetActive(value: true);
		startButton.GetComponent<Button>().interactable = true;
		achievement.SetActive(value: true);
		FFAToggle.gameObject.SetActive(value: true);
		lockFileToggle.gameObject.SetActive(value: true);
		offlineMultiplayerToggle.gameObject.SetActive(value: true);
		RefreshIronMan();
	}

	public void Start2PlayersGame(bool p_ignoreConfirmationWindow)
	{
		SoundManager.instance.PlayUIClickSound();
		MainMenu.instance.loading_Panel.SetActive(value: true);
		if (GameData.Instance.map.IsCampaign)
		{
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (!listOfPlayer.IsComputer)
				{
					GameData.Instance.map.OriginalCampaignCountry = listOfPlayer.Country;
					break;
				}
			}
		}
		if (FFAToggle.isOn)
		{
			foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
			{
				listOfPlayer2.Faction = listOfPlayer2.Country;
			}
		}
		GameData.Instance.offlinePBEM = offlineMultiplayerToggle.isOn;
		GameData.Instance.GenerateGameSettings(lockFileToggle.isOn);
		GameData.Instance.SetIronManMode(GameData.Instance.IsIronManModeON(out var _));
		if (GameData.Instance.map.IsOfficial)
		{
			GameData.Instance.map.IsCampaign = false;
		}
		MainMenu.isLoadedGame = false;
		foreach (Transform item in parent)
		{
			if (item != parent.transform)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		foreach (Transform item2 in playerInfoContainer_Transform)
		{
			if (item2 != playerInfoContainer_Transform)
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		SceneManager.LoadScene("Game");
	}

	private IEnumerator CR_LoadMap(FileInfo p_fileInfo)
	{
		MainMenu.selectedMap = null;
		RefreshIronMan();
		startButton.gameObject.SetActive(value: false);
		achievement.SetActive(value: false);
		mapSizeText.gameObject.SetActive(value: false);
		FFAToggle.gameObject.SetActive(value: false);
		lockFileToggle.gameObject.SetActive(value: false);
		offlineMultiplayerToggle.gameObject.SetActive(value: false);
		loadingScenarioPanel.SetActive(value: true);
		mapSizeText.text = LocalizationManager.Translate("txt-map size") + LocalizationManager.Translate("txt-loading");
		yield return null;
		try
		{
			GameData.Instance.Load(p_fileInfo.FullName);
			mapSizeText.text = LocalizationManager.Translate("txt-map size") + GameData.Instance.map.SizeX + " x " + GameData.Instance.map.SizeY;
			if (GameData.Instance.map.SizeX * GameData.Instance.map.SizeY <= 1000)
			{
				TextMeshProUGUI textMeshProUGUI = mapSizeText;
				textMeshProUGUI.text = textMeshProUGUI.text + " - " + LocalizationManager.Translate("txt-size small");
			}
			else if (GameData.Instance.map.SizeX * GameData.Instance.map.SizeY <= 10000)
			{
				TextMeshProUGUI textMeshProUGUI2 = mapSizeText;
				textMeshProUGUI2.text = textMeshProUGUI2.text + " - " + LocalizationManager.Translate("txt-size medium");
			}
			else if (GameData.Instance.map.SizeX * GameData.Instance.map.SizeY <= 30000)
			{
				TextMeshProUGUI textMeshProUGUI3 = mapSizeText;
				textMeshProUGUI3.text = textMeshProUGUI3.text + " - " + LocalizationManager.Translate("txt-size large");
			}
			else
			{
				TextMeshProUGUI textMeshProUGUI4 = mapSizeText;
				textMeshProUGUI4.text = textMeshProUGUI4.text + " - " + LocalizationManager.Translate("txt-size huge");
			}
			achievement.SetActive(value: true);
			mapSizeText.gameObject.SetActive(value: true);
			startButton.gameObject.SetActive(value: true);
			startButton.GetComponent<Button>().interactable = false;
		}
		catch (Exception ex)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-loading map"));
			UnityEngine.Debug.LogError("Could not load scenario: " + ex.ToString());
			UnityEngine.Debug.LogError("File info: " + p_fileInfo);
			if (p_fileInfo != null)
			{
				UnityEngine.Debug.LogError("File path: " + p_fileInfo.FullName);
			}
			yield break;
		}
		MainMenu.selectedMap = p_fileInfo;
		Transform[] componentsInChildren = playerInfoContainer_Transform.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != playerInfoContainer_Transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		foreach (Player player in GameData.Instance.listOfPlayers)
		{
			player.IsPlayingFirst = player == GameData.Instance.listOfPlayers[0];
			player.IsComputer = true;
			player.SetDefaultIncomeModifier();
			GameObject playerSlotInfo = UnityEngine.Object.Instantiate(playerQuickGameSlot, playerInfoContainer_Transform);
			playerSlotInfo.name = player.Name;
			componentsInChildren = playerSlotInfo.GetComponentsInChildren<Transform>();
			foreach (Transform transform2 in componentsInChildren)
			{
				if (transform2.gameObject.name == "Player country image")
				{
					transform2.transform.Find("Flag image").GetComponent<Image>().sprite = Utils.GetFlagSprite(player.Country);
				}
				if (transform2.gameObject.TryGetComponent<Toggle>(out var toggle))
				{
					toggle.onValueChanged.AddListener(delegate
					{
						SetPlaysFirst(player, toggle);
					});
					toggle.SetIsOnWithoutNotify(player.IsPlayingFirst);
				}
				if (transform2.gameObject.TryGetComponent<TMP_Dropdown>(out var dropdown))
				{
					dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("drpdwn-IA")));
					dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("drpdwn-human")));
					dropdown.RefreshShownValue();
					dropdown.onValueChanged.AddListener(delegate
					{
						ChangeConfiguration(player, dropdown, playerSlotInfo.GetComponentInChildren<Slider>());
					});
				}
				if (transform2.gameObject.TryGetComponent<Slider>(out var slider))
				{
					slider.value = (int)player.IncomeModifier;
					slider.GetComponentInChildren<TextMeshProUGUI>().text = player.IncomeModifier * 10 + "%";
					slider.onValueChanged.AddListener(delegate
					{
						ChangePlayerIncome(player, slider);
					});
				}
			}
		}
		startButton.GetComponent<Button>().interactable = true;
		FFAToggle.gameObject.SetActive(value: true);
		lockFileToggle.gameObject.SetActive(value: true);
		offlineMultiplayerToggle.gameObject.SetActive(value: true);
		loadingScenarioPanel.SetActive(value: false);
		RefreshIronMan();
	}

	private void RefreshIronMan()
	{
		bool flag = GameData.Instance.IsIronManModeON(out var _);
		YESironman.SetActive(flag);
		NOironman.SetActive(!flag);
	}

	private IEnumerator CR_CheckFiles()
	{
		_filesCheckSum = new byte[0];
		while (true)
		{
			byte[] array = Utils.ConvertObjectToByteArray(Directory.GetFiles(_filesPath, "*", SearchOption.AllDirectories));
			if (array.Length != _filesCheckSum.Length)
			{
				_filesCheckSum = array;
				RefreshListOfMaps();
			}
			yield return new WaitForSecondsRealtime(0.3f);
		}
	}
}

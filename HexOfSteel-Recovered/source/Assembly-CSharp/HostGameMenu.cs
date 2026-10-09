using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostGameMenu : MonoBehaviour, IMenu
{
	[SerializeField]
	private MultiplayerGamesMenu _multiplayerGamesMenu;

	[SerializeField]
	private RealtimeMultiplayerMenu _realtimeMultiplayerMenu;

	[SerializeField]
	private GameSessionContentMenu _gameSessionContentMenu;

	[Space]
	[SerializeField]
	private GameObject _scenarioSlotPrefab;

	[SerializeField]
	private GameObject _playerSlotPrefab;

	[Space]
	[SerializeField]
	private Button _hostPBEMgame_button;

	[SerializeField]
	private Button _hostRealtimeGame_button;

	[Space]
	[SerializeField]
	private TMP_Text _mapSizeText;

	[Space]
	[SerializeField]
	private TMP_InputField _search_inputfield;

	[SerializeField]
	private TMP_InputField _description_inputfield;

	[SerializeField]
	private TMP_InputField _password_inputfield;

	[Space]
	[SerializeField]
	private Transform _scenarioListContainer;

	[SerializeField]
	private Transform _playersListContainer;

	[Space]
	[SerializeField]
	private GameObject _scenarioLoadingIndicator;

	[Space]
	[SerializeField]
	private GameObject _loadingIndicator;

	private bool _isHostingGame;

	private string _scenariosDirectoryPath;

	private Coroutine _CR_loadingMap;

	private CancellationTokenSource _cts;

	private void Awake()
	{
		_scenariosDirectoryPath = Application.persistentDataPath + "/Maps";
	}

	private void OnEnable()
	{
		_isHostingGame = false;
		_loadingIndicator.SetActive(value: false);
		GameData.Instance.ResetData();
		_cts = new CancellationTokenSource();
		PrepareForm();
		RefreshScenariosList();
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public async void OnClick_HostRealtimeGame()
	{
		if (!_isHostingGame)
		{
			SoundManager.instance.PlayUIClickSound();
			string text = _description_inputfield.text;
			string text2 = _password_inputfield.text;
			_multiplayerGamesMenu.ShowRealtimeMultiplayerMenu();
			_isHostingGame = true;
			_loadingIndicator.SetActive(value: true);
			await _realtimeMultiplayerMenu.CreateRoom(GameData.Instance, text, text2);
			_loadingIndicator.SetActive(value: false);
			_isHostingGame = false;
		}
	}

	public void OnClick_HostPBEMGame()
	{
		SoundManager.instance.PlayUIClickSound();
		UploadGameData(GameData.Instance);
	}

	public void OnClick_Browse()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start(_scenariosDirectoryPath);
	}

	public void OnEdit_RefreshScenarios()
	{
		SoundManager.instance.PlayUIClickSound();
		RefreshScenariosList();
	}

	private async void UploadGameData(GameData p_gameData)
	{
		if (!_isHostingGame)
		{
			_isHostingGame = true;
			p_gameData.isPBEM = true;
			p_gameData.GenerateGameSettings(p_lockSaveFile: true);
			_loadingIndicator.SetActive(value: true);
			var (flag, p_gameSessionId) = await Server.Instance.GameService.HostGame(p_gameData, _description_inputfield.text, _password_inputfield.text, _cts.Token);
			if (flag)
			{
				_multiplayerGamesMenu.ShowJoinGameMenu();
				_gameSessionContentMenu.OpenWithSessionId(p_gameSessionId, null, null);
			}
			_isHostingGame = false;
			_loadingIndicator.SetActive(value: false);
		}
	}

	private void PrepareForm()
	{
		_search_inputfield.SetTextWithoutNotify(string.Empty);
		_description_inputfield.SetTextWithoutNotify(string.Empty);
		_password_inputfield.SetTextWithoutNotify(string.Empty);
		_hostPBEMgame_button.interactable = false;
		_hostRealtimeGame_button.interactable = false;
		_mapSizeText.text = LocalizationManager.Translate("txt-map size");
		_scenarioLoadingIndicator.SetActive(value: false);
		foreach (Transform item in _playersListContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	private void RefreshScenariosList()
	{
		MainMenu.GetAllCustomUserMadeMaps(p_includeCampaigns: false);
		foreach (Transform item in _scenarioListContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in _playersListContainer)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
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
			if (!string.IsNullOrEmpty(_search_inputfield.text) && !files.Value.ToLower().Contains(_search_inputfield.text.ToLower()))
			{
				continue;
			}
			if (files.Key.Directory.Name != text)
			{
				if (files.Key.Directory.Name != "Maps")
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(_scenarioSlotPrefab, _scenarioListContainer);
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
					GameObject gameObject2 = UnityEngine.Object.Instantiate(_scenarioSlotPrefab, _scenarioListContainer);
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
			GameObject gameObject3 = UnityEngine.Object.Instantiate(_scenarioSlotPrefab, _scenarioListContainer);
			gameObject3.name = files.Key.Name;
			TextMeshProUGUI componentInChildren2 = gameObject3.GetComponentInChildren<TextMeshProUGUI>();
			componentInChildren2.text = Path.GetFileNameWithoutExtension(files.Value);
			Path.GetFileNameWithoutExtension(files.Key.Name);
			componentInChildren2.text = componentInChildren2.text.Replace(".", string.Empty);
			componentInChildren2.text = componentInChildren2.text.Replace("_", string.Empty);
			gameObject3.GetComponentInChildren<Button>().onClick.AddListener(() =>
			{
				SelectScenario(files.Key);
			});
			gameObject3.transform.localScale = Vector3.one;
		}
		if (MainMenu.listOfAllCampaigns.Count > 0)
		{
			GameObject gameObject4 = UnityEngine.Object.Instantiate(_scenarioSlotPrefab, _scenarioListContainer);
			gameObject4.name = "CAMPAIGNS";
			UnityEngine.Object.Destroy(gameObject4.GetComponentInChildren<Button>());
			TextMeshProUGUI componentInChildren3 = gameObject4.GetComponentInChildren<TextMeshProUGUI>();
			componentInChildren3.text = LocalizationManager.Translate("txt-campaign scenarios");
			componentInChildren3.fontStyle = FontStyles.UpperCase;
			componentInChildren3.alignment = TextAlignmentOptions.Midline;
			componentInChildren3.color = MainMenu.yellowColor;
			gameObject4.transform.localScale = Vector3.one;
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
			if (string.IsNullOrEmpty(_search_inputfield.text) || files2.Value.ToLower().Contains(_search_inputfield.text.ToLower()))
			{
				GameObject gameObject5 = UnityEngine.Object.Instantiate(_scenarioSlotPrefab, _scenarioListContainer);
				gameObject5.name = files2.Key.Name;
				TextMeshProUGUI componentInChildren4 = gameObject5.GetComponentInChildren<TextMeshProUGUI>();
				componentInChildren4.text = Path.GetFileNameWithoutExtension(files2.Value);
				Path.GetFileNameWithoutExtension(files2.Key.Name);
				componentInChildren4.text = componentInChildren4.text.Replace(".", string.Empty);
				componentInChildren4.text = componentInChildren4.text.Replace("_", string.Empty);
				componentInChildren4.text = componentInChildren4.text + " - " + LocalizationManager.GetLocalizedCampaignSide(new DirectoryInfo(Directory.GetParent(Directory.GetParent(Directory.GetParent(files2.Key.FullName).ToString()).ToString()).ToString()).Name);
				gameObject5.GetComponentInChildren<Button>().onClick.AddListener(() =>
				{
					SelectScenario(files2.Key);
				});
				gameObject5.transform.localScale = Vector3.one;
			}
		}
		List<FileInfo> list = new List<FileInfo>();
		string[] files3 = Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Saves"), "*", SearchOption.AllDirectories);
		for (int num = 0; num < files3.Length; num++)
		{
			FileInfo fileInfo = new FileInfo(files3[num]);
			if (fileInfo.Extension == ".sav")
			{
				list.Add(fileInfo);
			}
		}
		list = (from x in list
			orderby x.Directory.Name == "Saves" descending, x.Directory.Name
			select x).ThenByDescending((FileInfo f) => f.LastWriteTime).ToList();
		text = null;
		foreach (FileInfo fileInfo2 in list)
		{
			if (fileInfo2.Directory.Name != text)
			{
				GameObject gameObject6 = UnityEngine.Object.Instantiate(_scenarioSlotPrefab, _scenarioListContainer);
				gameObject6.name = fileInfo2.Directory.Name;
				UnityEngine.Object.Destroy(gameObject6.GetComponentInChildren<Button>());
				TextMeshProUGUI componentInChildren5 = gameObject6.GetComponentInChildren<TextMeshProUGUI>();
				componentInChildren5.text = fileInfo2.Directory.Name;
				componentInChildren5.fontStyle = FontStyles.UpperCase;
				componentInChildren5.alignment = TextAlignmentOptions.Midline;
				componentInChildren5.color = MainMenu.yellowColor;
				gameObject6.transform.localScale = new Vector3(1f, 1f, 1f);
				text = fileInfo2.Directory.Name;
			}
			if (string.IsNullOrEmpty(_search_inputfield.text) || fileInfo2.Name.Contains(_search_inputfield.text))
			{
				GameObject gameObject7 = UnityEngine.Object.Instantiate(_scenarioSlotPrefab, _scenarioListContainer);
				gameObject7.name = fileInfo2.FullName;
				gameObject7.GetComponentInChildren<TextMeshProUGUI>().text = Utils.GetFileNameWithoutExtension(fileInfo2);
				gameObject7.GetComponentInChildren<Button>().onClick.AddListener(() =>
				{
					SelectScenario(fileInfo2);
				});
				gameObject7.transform.localScale = new Vector3(1f, 1f, 1f);
			}
		}
	}

	private void SelectScenario(FileInfo p_fileInfo)
	{
		SoundManager.instance.PlayUIClickSound();
		if (_CR_loadingMap != null)
		{
			StopCoroutine(_CR_loadingMap);
		}
		_CR_loadingMap = StartCoroutine(CR_LoadMap(p_fileInfo));
	}

	private IEnumerator CR_LoadMap(FileInfo p_fileInfo)
	{
		if (p_fileInfo == null)
		{
			yield break;
		}
		MainMenu.selectedMap = null;
		_hostPBEMgame_button.interactable = false;
		_hostRealtimeGame_button.interactable = false;
		_scenarioLoadingIndicator.SetActive(value: true);
		_mapSizeText.text = LocalizationManager.Translate("txt-map size") + LocalizationManager.Translate("txt-loading");
		yield return null;
		try
		{
			GameData.Instance.Load(p_fileInfo.FullName);
			_mapSizeText.text = LocalizationManager.Translate("txt-map size") + GameData.Instance.map.SizeX + " x " + GameData.Instance.map.SizeY;
			if (GameData.Instance.map.SizeX * GameData.Instance.map.SizeY <= 1000)
			{
				TMP_Text mapSizeText = _mapSizeText;
				mapSizeText.text = mapSizeText.text + " (" + LocalizationManager.Translate("txt-size small") + ")";
			}
			else if (GameData.Instance.map.SizeX * GameData.Instance.map.SizeY <= 10000)
			{
				TMP_Text mapSizeText2 = _mapSizeText;
				mapSizeText2.text = mapSizeText2.text + " (" + LocalizationManager.Translate("txt-size medium") + ")";
			}
			else if (GameData.Instance.map.SizeX * GameData.Instance.map.SizeY <= 30000)
			{
				TMP_Text mapSizeText3 = _mapSizeText;
				mapSizeText3.text = mapSizeText3.text + " (" + LocalizationManager.Translate("txt-size large") + ")";
			}
			else
			{
				TMP_Text mapSizeText4 = _mapSizeText;
				mapSizeText4.text = mapSizeText4.text + " (" + LocalizationManager.Translate("txt-size huge") + ")";
			}
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Could not load scenario {p_fileInfo.Name}: {arg}");
			UIManager.ShowMessage(LocalizationManager.Translate("error-loading map"));
			yield break;
		}
		MainMenu.selectedMap = p_fileInfo;
		foreach (Transform item in _playersListContainer)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		bool flag = GameData.Instance.map.timePlayed > 0f;
		foreach (Player player in GameData.Instance.listOfPlayers)
		{
			if (!flag)
			{
				player.UserId = null;
				player.LocalPassword = null;
				player.IsComputer = false;
				player.IsPlaying = false;
				player.IsPlayingFirst = false;
				player.SetDefaultIncomeModifier();
			}
			GameObject playerSlotInfo = UnityEngine.Object.Instantiate(_playerSlotPrefab, _playersListContainer);
			playerSlotInfo.name = player.Name;
			foreach (Transform item2 in playerSlotInfo.transform)
			{
				if (item2.gameObject.name == "Player country image")
				{
					item2.transform.Find("Flag image").GetComponent<Image>().sprite = Utils.GetFlagSprite(player.Country);
				}
				if (item2.gameObject.TryGetComponent<Toggle>(out var toggle))
				{
					toggle.SetIsOnWithoutNotify(player.IsPlayingFirst);
					toggle.onValueChanged.AddListener(delegate
					{
						SetPlaysFirst(player, toggle);
					});
				}
				if (item2.gameObject.TryGetComponent<TMP_Dropdown>(out var dropdown))
				{
					dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("drpdwn-IA")));
					dropdown.options.Add(new TMP_Dropdown.OptionData(LocalizationManager.Translate("drpdwn-human")));
					dropdown.SetValueWithoutNotify((!player.IsComputer) ? 1 : 0);
					dropdown.RefreshShownValue();
					dropdown.onValueChanged.AddListener(delegate
					{
						ChangeConfiguration(player, dropdown, playerSlotInfo.GetComponentInChildren<Slider>());
					});
				}
				if (item2.gameObject.TryGetComponent<Slider>(out var slider))
				{
					slider.value = (int)player.IncomeModifier;
					slider.GetComponentInChildren<TextMeshProUGUI>().text = player.IncomeModifier * 10 + "%";
					slider.onValueChanged.AddListener(delegate
					{
						SetIncomeModifier(player, slider);
					});
				}
			}
		}
		_scenarioLoadingIndicator.SetActive(value: false);
		RefreshStartGameButtonState();
	}

	private void SetIncomeModifier(Player p_player, Slider p_slider)
	{
		SoundManager.instance.PlayUIClickSound();
		p_player.IncomeModifier = (byte)p_slider.value;
		p_slider.GetComponentInChildren<TextMeshProUGUI>().text = p_player.IncomeModifier * 10 + "%";
	}

	private void ChangeConfiguration(Player p_player, TMP_Dropdown p_dropdown, Slider p_incomeSlider)
	{
		SoundManager.instance.PlayUIClickSound();
		p_player.IsComputer = p_dropdown.value == 0;
		if (p_player.IsComputer)
		{
			p_player.UserId = null;
			p_player.LocalPassword = null;
		}
		p_player.SetDefaultIncomeModifier();
		p_incomeSlider.value = (int)p_player.IncomeModifier;
		RefreshStartGameButtonState();
	}

	private void SetPlaysFirst(Player p_player, Toggle p_toggle)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
		{
			listOfPlayer.IsPlaying = false;
			listOfPlayer.IsPlayingFirst = false;
		}
		p_player.IsPlaying = true;
		p_player.IsPlayingFirst = true;
		Toggle[] componentsInChildren = _playersListContainer.GetComponentsInChildren<Toggle>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetIsOnWithoutNotify(value: false);
		}
		p_toggle.SetIsOnWithoutNotify(value: true);
		RefreshStartGameButtonState();
	}

	private void RefreshStartGameButtonState()
	{
		bool flag = GameData.Instance.listOfPlayers.Count((Player p) => !p.IsComputer) >= 2;
		bool flag2 = GameData.Instance.listOfPlayers.Count((Player p) => p.IsPlayingFirst) == 1;
		bool interactable = flag & flag2;
		_hostPBEMgame_button.interactable = interactable;
		_hostRealtimeGame_button.interactable = interactable;
	}

	private void OnDisable()
	{
		_cts?.Cancel();
	}

	private void OnDestroy()
	{
		_cts?.Dispose();
		_cts = null;
	}
}

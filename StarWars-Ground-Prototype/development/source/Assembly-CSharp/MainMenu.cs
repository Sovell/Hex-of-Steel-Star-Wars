using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
	public static MainMenu instance;

	public static bool isDemo;

	public static bool isEditor;

	public static bool isMainMenu;

	public static bool isTablet = false;

	public static bool openMultiplayerMenuAfterGame = false;

	public static GameSettings gameSettingsBackup;

	public static Stopwatch stopwatchAttackMovement;

	public Canvas mainCanvas;

	public Transform UI_parent;

	public GameObject continue_button;

	public GameObject menu_section;

	public GameObject play_section;

	public GameObject editors_section;

	public GameObject help_section;

	public GameObject community_section;

	public MultiplayerMenuManager multiplayerMenuManager;

	[Header("DEMO")]
	[SerializeField]
	private bool _isDemo;

	public GameObject demo_text;

	public TextMeshProUGUI gameVersion_text;

	public Image wallpaper_Image;

	[SerializeField]
	private VideoPlayer _videoPlayer;

	public GameObject newCampaignPanel;

	public GameObject twoPlayers_Panel;

	public GameObject mapEditor_Panel;

	public GameObject countryEditor_Panel;

	public GameObject unitEditor_Panel;

	public GameObject tileEditor_Panel;

	public GameObject heroesEditor_Panel;

	public GameObject modManager_Panel;

	public GameObject settings_Panel;

	public GameObject loading_Panel;

	public GameObject saveLoad_Panel;

	public GameObject mods_Panel;

	public GameObject mapgGenerator_Panel;

	public GameObject leaderboardGO_panel;

	public static bool isLoadedGame;

	public static FileInfo selectedMap;

	public static List<FileInfo> listOfAllCampaigns;

	public static List<FileInfo> listOfAllMapsQuickGame;

	public static Color fogOfWarColor = new Color(0.5f, 0.5f, 0.5f);

	public static Color redColor = new Color(0.6f, 0.13f, 0.13f);

	public static Color greenColor = new Color(0.124f, 0.641f, 0.124f);

	public static Color yellowColor = new Color(0.8705882f, 186f / 255f, 0.1529412f);

	public static Color greyColor = new Color(0.6f, 0.6f, 0.62f);

	public static Color blackColor = new Color(22f / 255f, 22f / 255f, 0.09411766f);

	public static List<Player> listOfPlayersToBeSavedBetweenCampaign;

	public LoadMenu loadGO;

	public StandaloneScenariosMenu twoPlayers;

	public MapEditorMenu mapEditorMenu;

	public Sprite baseWallapper_sprite;

	public GameObject tooltip;

	private void Awake()
	{
		instance = this;
		isDemo = _isDemo;
		gameVersion_text.text = "v " + Application.version;
		demo_text.SetActive(_isDemo);
		stopwatchAttackMovement = new Stopwatch();
		stopwatchAttackMovement.Start();
		PlayerSettings.Load();
		isLoadedGame = false;
		selectedMap = null;
		UIManager.isNATO = false;
		TurnManager.humanPlayer = null;
		TurnManager.currPlayer = null;
		MapGO.IsAIOnly = false;
		isEditor = false;
		isMainMenu = true;
		listOfPlayersToBeSavedBetweenCampaign = null;
		GameData.Instance.map = null;
		Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
		QualitySettings.vSyncCount = PlayerSettings.Instance.VsyncValue;
		GetAllOfficialCampaignMaps();
		CreateAllDirectories();
		OfficialUnits.Load();
		OfficialUnitsORIGINAL.Load();
		OfficialUnits.GetInstance().Units = OfficialUnitsORIGINAL.GetInstance().Units;
		ModsManager.ApplyModdedUnitsChanges();
		OfficialTiles.Load();
		OfficialTilesORIGINAL.Load();
		OfficialTiles.GetInstance().Tiles = OfficialTilesORIGINAL.GetInstance().Tiles;
		ModsManager.ApplyModdedTilesChanges();
	}

	private void Start()
	{
		PrototypeBootstrap.Begin();
		Application.targetFrameRate = PlayerSettings.Instance.MaxFramerate;
		CheckContinueButton();
		if (gameSettingsBackup.isBackup)
		{
			gameSettingsBackup.LoadBackup();
		}
		if (openMultiplayerMenuAfterGame)
		{
			openMultiplayerMenuAfterGame = false;
			multiplayerMenuManager.OpenFromGame();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CloseAllPanel();
		}
	}

	private void CheckContinueButton()
	{
		FileInfo mostRecentSaveFile = Utils.GetMostRecentSaveFile();
		continue_button.SetActive(mostRecentSaveFile != null);
		if (mostRecentSaveFile != null && continue_button.TryGetComponent<TooltipGO>(out var component))
		{
			component.localizationKey = Utils.GetFileNameWithoutExtension(mostRecentSaveFile);
		}
	}

	public void ContinueLastSave()
	{
		SoundManager.instance.PlayUIClickSound();
		try
		{
			FileInfo mostRecentSaveFile = Utils.GetMostRecentSaveFile();
			if (mostRecentSaveFile != null)
			{
				loading_Panel.SetActive(value: true);
				GameData.Instance.Load(mostRecentSaveFile.FullName);
				if (isDemo && GameData.Instance.map.turnCount > 20)
				{
					loading_Panel.SetActive(value: false);
					UIManager.ShowMessage(LocalizationManager.Translate("info-demo no loading"));
					return;
				}
				isLoadedGame = true;
				selectedMap = mostRecentSaveFile;
				UnityEngine.Object.Destroy(continue_button.GetComponent<TooltipGO>());
				tooltip.SetActive(value: false);
				SceneManager.LoadScene("Game");
			}
		}
		catch (Exception ex)
		{
			UIManager.ShowMessage(ex.Message);
		}
	}

	private void CreateAllDirectories()
	{
		if (!Directory.Exists(PrototypePaths.DataPath + "/Maps"))
		{
			Directory.CreateDirectory(PrototypePaths.DataPath + "/Maps");
		}
		if (!Directory.Exists(PrototypePaths.DataPath + "/Saves"))
		{
			Directory.CreateDirectory(PrototypePaths.DataPath + "/Saves");
		}
		if (!Directory.Exists(PrototypePaths.DataPath + "/MODS"))
		{
			Directory.CreateDirectory(PrototypePaths.DataPath + "/MODS");
		}
		if (!Directory.Exists(PrototypePaths.DataPath + "/Screenshots"))
		{
			Directory.CreateDirectory(PrototypePaths.DataPath + "/Screenshots");
		}
	}

	private void GetAllOfficialCampaignMaps()
	{
		listOfAllCampaigns = new List<FileInfo>();
		string[] files = Directory.GetFiles(Application.streamingAssetsPath + "/Campaign", "*", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			FileInfo fileInfo = new FileInfo(files[i]);
			if (fileInfo.Extension == ".sav" && !Utils.IsHiddenOrAppleDoubleFile(fileInfo))
			{
				listOfAllCampaigns.Add(fileInfo);
			}
		}
	}

	public static void GetAllCustomUserMadeMaps(bool p_includeCampaigns)
	{
		listOfAllMapsQuickGame = new List<FileInfo>();
		FileInfo[] files = new DirectoryInfo(Application.streamingAssetsPath + "/Maps").GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			if (fileInfo.Extension == ".sav" && !Utils.IsHiddenOrAppleDoubleFile(fileInfo))
			{
				listOfAllMapsQuickGame.Add(fileInfo);
			}
		}
		string[] files2 = Directory.GetFiles(PrototypePaths.DataPath + "/Maps", "*", SearchOption.AllDirectories);
		for (int i = 0; i < files2.Length; i++)
		{
			FileInfo fileInfo2 = new FileInfo(files2[i]);
			if (fileInfo2.Extension == ".sav" && !Utils.IsHiddenOrAppleDoubleFile(fileInfo2))
			{
				listOfAllMapsQuickGame.Add(fileInfo2);
			}
		}
		foreach (FileInfo moddedMap in ModsManager.moddedMaps)
		{
			listOfAllMapsQuickGame.Add(moddedMap);
		}
		listOfAllMapsQuickGame = (from x in listOfAllMapsQuickGame
			orderby x.Directory.Name != "Maps" descending, x.Directory.Name, x.Name
			select x).ToList();
		if (!p_includeCampaigns)
		{
			return;
		}
		List<FileInfo> list = new List<FileInfo>();
		files2 = Directory.GetFiles(Application.streamingAssetsPath + "/Campaign", "*", SearchOption.AllDirectories);
		for (int i = 0; i < files2.Length; i++)
		{
			FileInfo fileInfo3 = new FileInfo(files2[i]);
			if (fileInfo3.Extension == ".sav" && !Utils.IsHiddenOrAppleDoubleFile(fileInfo3))
			{
				list.Add(fileInfo3);
			}
		}
		list = list.OrderBy((FileInfo x) => x.Name).ToList();
		listOfAllMapsQuickGame = listOfAllMapsQuickGame.Concat(list).ToList();
	}

	public void OpenCampaignPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		newCampaignPanel.SetActive(value: true);
		selectedMap = null;
		SteamManager.SetRichPresenceStatus(SteamStatuses.STARTING_CAMPAIGN);
	}

	public void OpenQuickGamePanel()
	{
		SoundManager.instance.PlayUIClickSound();
		selectedMap = null;
		twoPlayers_Panel.SetActive(value: true);
		SteamManager.SetRichPresenceStatus(SteamStatuses.STARTING_QUICKGAME);
	}

	public void OpenMapEditorPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		mapEditorMenu.RefreshListOfMaps();
		mapEditor_Panel.SetActive(value: true);
	}

	public void OpenMapGeneratorPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		mapgGenerator_Panel.SetActive(value: true);
	}

	public void OpenCountryEditorPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		countryEditor_Panel.SetActive(value: true);
	}

	public void OpenUnitEditorPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		unitEditor_Panel.SetActive(value: true);
	}

	public void OpenTileEditorPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		tileEditor_Panel.SetActive(value: true);
	}

	public void OpenHeroesEditorPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		heroesEditor_Panel.SetActive(value: true);
	}

	public void OpenLeaderboardPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!SteamManager.IsSteamReady)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-steam not connected"));
		}
		else
		{
			leaderboardGO_panel.SetActive(value: true);
		}
	}

	public void OpenModManagerPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		mods_Panel.SetActive(value: true);
	}

	public void CloseAllPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		_videoPlayer.SetDirectAudioVolume(0, PlayerSettings.Instance.MusicVolume);
		newCampaignPanel.SetActive(value: false);
		twoPlayers_Panel.SetActive(value: false);
		if (!SettingsMenu.listeningForNewKeybind)
		{
			settings_Panel.SetActive(value: false);
		}
		unitEditor_Panel.SetActive(value: false);
		countryEditor_Panel.SetActive(value: false);
		tileEditor_Panel.SetActive(value: false);
		heroesEditor_Panel.SetActive(value: false);
		tooltip.SetActive(value: false);
		modManager_Panel.SetActive(value: false);
		mods_Panel.SetActive(value: false);
		mapgGenerator_Panel.SetActive(value: false);
		mapEditor_Panel.SetActive(value: false);
		leaderboardGO_panel.SetActive(value: false);
		saveLoad_Panel.SetActive(value: false);
		selectedMap = null;
		PlayerSettings.Save();
		CheckContinueButton();
		SteamManager.SetRichPresenceStatus(SteamStatuses.MAIN_MENU);
	}

	public void PickSection(string p_value)
	{
		SoundManager.instance.PlayUIClickSound();
		menu_section.SetActive(value: false);
		play_section.SetActive(value: false);
		editors_section.SetActive(value: false);
		help_section.SetActive(value: false);
		community_section.SetActive(value: false);
		switch (p_value)
		{
		case "Play":
			play_section.SetActive(value: true);
			break;
		case "Editors":
			editors_section.SetActive(value: true);
			break;
		case "Community":
			community_section.SetActive(value: true);
			break;
		case "Help":
			help_section.SetActive(value: true);
			break;
		case "Back":
			menu_section.SetActive(value: true);
			break;
		}
		UIManager.HideToolTip();
	}

	public void OpenSettings()
	{
		SoundManager.instance.PlayUIClickSound();
		settings_Panel.SetActive(value: true);
		SteamManager.SetRichPresenceStatus(SteamStatuses.SETTINGS);
	}

	public void OpenManual()
	{
		SoundManager.instance.PlayUIClickSound();
		string text = Path.Combine(Application.streamingAssetsPath, "Manual.pdf");
		if (File.Exists(text))
		{
			Process.Start(text);
		}
		else
		{
			Process.Start("https://hex-of-steel.fr/manual");
		}
	}

	public void Discord()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://discord.com/invite/Tn63mrwJyH");
	}

	public void Youtube()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://www.youtube.com/channel/UCMMq2he5LtC5Sg5qcfy8y9g");
	}

	public void BeastsOfSteel()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://store.steampowered.com/app/1871460?utm_source=HOSbutton1");
	}

	public void Instagram()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://www.instagram.com/hex_of_steel/");
	}

	public void Facebook()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://www.facebook.com/hexofsteel");
	}

	public void Reddit()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://www.reddit.com/r/hexofsteel/");
	}

	public void Donate()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://www.paypal.com/donate?hosted_button_id=26S5Q3PPC8TKY");
	}

	public void PlayStore()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://play.google.com/store/apps/details?id=com.valentinstudio.OperationCitadel");
	}

	public void AppStore()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start("https://apps.apple.com/us/app/hex-of-steel/id1532682161");
	}

	public void ContactSupport()
	{
		SoundManager.instance.PlayUIClickSound();
		string text = "support@hex-of-steel.fr";
		StringBuilder stringBuilder = new StringBuilder();
		string s = "Support request for " + Application.productName;
		stringBuilder.AppendLine("Description of your problem:");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("=========================");
		stringBuilder.AppendLine("Generated automatically");
		stringBuilder.AppendLine("=========================");
		stringBuilder.AppendLine("Game: " + Application.productName + " v" + Application.version);
		stringBuilder.AppendLine($"Language: {Application.systemLanguage}");
		stringBuilder.AppendLine($"Platform: {Application.platform}");
		stringBuilder.AppendLine("OS: " + SystemInfo.operatingSystem);
		stringBuilder.AppendLine("Device: " + SystemInfo.deviceModel);
		stringBuilder.AppendLine("Graphics: " + SystemInfo.graphicsDeviceName);
		stringBuilder.AppendLine("=========================");
		stringBuilder.AppendLine("Please attach your Player.log file to this email.");
		string url = "mailto:" + text + "?subject=" + MailToEncode(s) + "&body=" + MailToEncode(stringBuilder.ToString());
		try
		{
			GUIUtility.systemCopyBuffer = Application.consoleLogPath;
			try
			{
				string consoleLogPath = Application.consoleLogPath;
				Path.GetDirectoryName(consoleLogPath);
				Process.Start("explorer.exe", "/select," + consoleLogPath.Replace("/", "\\"));
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning("Could not open log folder: " + ex.Message);
			}
			Application.OpenURL(url);
		}
		catch
		{
			GUIUtility.systemCopyBuffer = text;
			UnityEngine.Debug.LogWarning("Couldn't open mail app. Support email copied to clipboard.");
			UIManager.ShowMessage("Couldn't open mail app. Support email copied to clipboard.");
		}
	}

	private string MailToEncode(string s)
	{
		return Uri.EscapeDataString(s).Replace("%0A", "%0D%0A");
	}

	public void Quit()
	{
		SoundManager.instance.PlayUIClickSound();
		Application.Quit();
	}

	private void OnApplicationQuit()
	{
		PlayerSettings.Save();
	}
}

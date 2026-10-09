using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class PlayerSettings
{
	private static readonly string SAVE_PATH = Application.persistentDataPath + "/playerSettings.sav";

	private static PlayerSettings _instance;

	private ModPropertyBag _modDataBag;

	public static PlayerSettings Instance => _instance ?? (_instance = new PlayerSettings());

	public string GameVersion { get; set; }

	public GameLanguages Language { get; set; }

	public float UnitVolume { get; set; }

	public float WeatherVolume { get; set; }

	public float MusicVolume { get; set; }

	public float UIVolume { get; set; }

	public float UnitSize { get; set; }

	public bool UnitsAlwaysBlink { get; set; }

	public bool InvertedMouseSroll { get; set; }

	public int DifficultyLevel { get; set; }

	public int VsyncValue { get; set; }

	public int WinterDamageValue { get; set; }

	public int MaxFramerate { get; set; }

	public int RandomDamageModifier { get; set; }

	public int AutosavesAmount { get; set; }

	public int AutosavesCurrent { get; set; }

	public int AutosavesFrequency { get; set; }

	public bool FollowAIMoves { get; set; }

	public bool MoveCameraWithMouseOnEdge { get; set; }

	public bool ShowTooltips { get; set; }

	public bool ShowInfoTile { get; set; }

	public bool IsAISlowMode { get; set; }

	public bool ComplexSupplies { get; set; }

	public bool IsMorale { get; set; }

	public bool IsQuickMovement { get; set; }

	public bool IsFogOfWar { get; set; }

	public bool IsInfiniteAmmoAndFuel { get; set; }

	public bool ShowClouds { get; set; }

	public bool Fullscreen { get; set; }

	public bool IsWeather { get; set; }

	public bool ISHistoricalFocus { get; set; }

	public bool IsWeatherParticles { get; set; }

	public bool ShowBorders { get; set; }

	public bool OneMoreTilePlanes { get; set; }

	public bool BlinkInactiveUnits { get; set; }

	public bool Overrun { get; set; }

	public bool Policies { get; set; }

	public bool Manpower { get; set; }

	public bool ZoneOfControl { get; set; }

	public bool ColorBlindMode { get; set; }

	public bool PublicChatOFF { get; set; }

	public bool FastAllies { get; set; }

	public bool HideBattlePreview { get; set; }

	public bool DisableVanillaSoundtracks { get; set; }

	public bool DisplayUnitsLevelOnCounter { get; set; }

	public bool DamageReductionRepeatedAttacks { get; set; }

	public bool ShowUnitMovePath { get; set; }

	public bool SkipEndTurnSummary { get; set; }

	public bool ShowShipNames { get; set; }

	public bool IsMovementInertia { get; set; }

	public bool BattleAnimations { get; set; }

	public float UIScale { get; set; }

	public float ZoomFactor { get; set; }

	public int[,] Resolution { get; set; }

	public bool MoveCameraToMousePosWhenZooming { get; set; }

	public bool MoreContrastSelectedUnit { get; set; }

	public bool AutomaticallyHidePopups { get; set; }

	public bool Quick2RespondOption { get; set; }

	public bool UnitSuffix { get; set; }

	public bool TechTree { get; set; }

	public bool RealisticPlanes { get; set; }

	public bool Partisans { get; set; }

	public bool HideUnitsStats { get; set; }

	public bool ProductionTakesTime { get; set; }

	public bool UnitIsDoneIndicator { get; set; }

	public bool MoveCameraWithLeftClick { get; set; }

	public bool FastAI { get; set; }

	public bool ShowUnitsNumbersOnCounters { get; set; }

	public bool HideVisualCueCoreUnits { get; set; }

	public bool InstantSeasonChange { get; set; }

	public bool Upkeep { get; set; }

	public bool CensoredGermany { get; set; }

	public bool FiniteManpower { get; set; }

	public bool PrototypesDisabled { get; set; }

	public bool NoAAsupportDuringAirFight { get; set; }

	public bool UseNATObyDefault { get; set; }

	public bool StricterFogOfWar { get; set; }

	public bool HideEnemyHP { get; set; }

	public bool ShuffleMusics { get; set; }

	public string CameraBackgroundColor { get; set; }

	public string ChatChannel { get; set; }

	public string DefaultServerRegion { get; set; }

	public ScenarioGeneratorData ScenarioGeneratorData { get; set; }

	public Guid UserId { get; set; }

	public string Username { get; set; }

	public string Email { get; set; }

	public string Password { get; set; }

	public byte[] AvatarData { get; set; }

	public KeyCode MoveCameraUp { get; set; }

	public KeyCode MoveCameraDown { get; set; }

	public KeyCode MoveCameraLeft { get; set; }

	public KeyCode MoveCameraRight { get; set; }

	public KeyCode ZoomInCamera { get; set; }

	public KeyCode ZoomOutCamera { get; set; }

	public KeyCode MoveOverlayUp { get; set; }

	public KeyCode MoveOverlayDown { get; set; }

	public KeyCode MoveOverlayLeft { get; set; }

	public KeyCode MoveOverlayRight { get; set; }

	public KeyCode ScaleOverlayUp { get; set; }

	public KeyCode ScaleOverlayDown { get; set; }

	public KeyCode IncreaseOverlayOpacity { get; set; }

	public KeyCode DecreaseOverlayOpacity { get; set; }

	public KeyCode NextUnit { get; set; }

	public KeyCode PreviousUnit { get; set; }

	public KeyCode NextUnitAir { get; set; }

	public KeyCode NextUnitNaval { get; set; }

	public KeyCode NextUnitGround { get; set; }

	public KeyCode FocusOnSelectedUnit { get; set; }

	public KeyCode SwitchNATOGraphics { get; set; }

	public KeyCode ShowBuilding { get; set; }

	public KeyCode Quicksave { get; set; }

	public KeyCode Quickload { get; set; }

	public KeyCode ManageMinimap { get; set; }

	public KeyCode ShowHideGroundAirUnit { get; set; }

	public KeyCode ChangeParatroopersState { get; set; }

	public KeyCode ChangeSubmarineState { get; set; }

	public KeyCode SetDefensive { get; set; }

	public KeyCode SetUnitAsGoal { get; set; }

	public KeyCode MoveUnit { get; set; }

	public KeyCode AttackUnit { get; set; }

	public KeyCode ShowUnitHistory { get; set; }

	public KeyCode EyedropperTile { get; set; }

	public KeyCode HideAllUnits { get; set; }

	public KeyCode PutUnitToSleep { get; set; }

	public KeyCode ResupplyManpower { get; set; }

	public KeyCode ResupplyAmmo { get; set; }

	public KeyCode ResupplyFuel { get; set; }

	public KeyCode ResupplyAmmoAndFuel { get; set; }

	public KeyCode Disband { get; set; }

	public KeyCode SkipUnit { get; set; }

	public KeyCode UnselectUnit { get; set; }

	public KeyCode Waypoints { get; set; }

	public KeyCode Disengage { get; set; }

	public KeyCode Undo { get; set; }

	public KeyCode Objectives { get; set; }

	public KeyCode ToggleOverlay { get; set; }

	public KeyCode ToggleOrderOfBattle { get; set; }

	public KeyCode PauseGame { get; set; }

	public KeyCode JumpLeftSideMap { get; set; }

	public KeyCode JumpRightSideMap { get; set; }

	public KeyCode JumpUpSideMap { get; set; }

	public KeyCode JumpDownSideMap { get; set; }

	public KeyCode TogglePoliticalMap { get; set; }

	public KeyCode ShowHideUI { get; set; }

	public KeyCode EndTurn { get; set; }

	public KeyCode SelectPen { get; set; }

	public KeyCode SelectEraser { get; set; }

	public KeyCode ToggleAISpeed { get; set; }

	public KeyCode ToggleStore { get; set; }

	public KeyCode ToggleReserve { get; set; }

	public KeyCode TimeSpeedUp { get; set; }

	public KeyCode TimeSlowDown { get; set; }

	public KeyCode ToggleSuppliesMap { get; set; }

	public List<string> Friends { get; set; }

	public DateTime LastChatMentionTime { get; set; }

	public HashSet<string> ChatMentions { get; set; }

	public List<string> ActiveMods { get; set; }

	public Dictionary<string, string> CompletedScenarioz { get; set; }

	public ModPropertyBag ModDataBag => _modDataBag ?? (_modDataBag = new ModPropertyBag());

	public void SetLanguage(GameLanguages p_gameLanguage)
	{
		Language = p_gameLanguage;
		Save();
		LocalizationManager.SetLanguage(Language);
		SceneTranslator.instance.TranslateScene();
	}

	public void ClearCredentials()
	{
		UserId = Guid.Empty;
		Username = string.Empty;
		Email = string.Empty;
		Password = string.Empty;
	}

	public static void Save()
	{
		using (FileStream serializationStream = new FileStream(SAVE_PATH, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			new BinaryFormatter().Serialize(serializationStream, Instance);
		}
	}

	public static void ApplyGameSettings(GameSettings p_gameSettings)
	{
		Instance.ISHistoricalFocus = p_gameSettings.historicalFocusON;
		Instance.IsFogOfWar = p_gameSettings.fogOfWarON;
		Instance.TechTree = p_gameSettings.researchON;
		Instance.Manpower = p_gameSettings.manpowerON;
		Instance.Policies = p_gameSettings.policiesON;
		Instance.IsWeather = p_gameSettings.weatherON;
		Instance.Partisans = p_gameSettings.partisansON;
		Instance.RealisticPlanes = p_gameSettings.realisticPlanesON;
		Instance.Overrun = p_gameSettings.overrunON;
		Instance.IsMorale = p_gameSettings.moraleON;
		Instance.ProductionTakesTime = p_gameSettings.productionTakesTimeON;
		Instance.IsInfiniteAmmoAndFuel = p_gameSettings.infiniteAmmoAndFuelON;
		Instance.ComplexSupplies = p_gameSettings.complexSuppliesON;
		Instance.OneMoreTilePlanes = p_gameSettings.planeMoveAfterAttackON;
		Instance.DifficultyLevel = p_gameSettings.difficultyValue;
		Instance.WinterDamageValue = p_gameSettings.winterDamageValue;
		Instance.RandomDamageModifier = p_gameSettings.randomDamageValue;
		Instance.Upkeep = p_gameSettings.upkeepON;
		Instance.ZoneOfControl = p_gameSettings.zoneOfControlON;
		Instance.DamageReductionRepeatedAttacks = p_gameSettings.damageReductionRepeatedAttacks;
		Instance.FiniteManpower = p_gameSettings.finiteManpower;
		Instance.NoAAsupportDuringAirFight = p_gameSettings.noAAsupportDuringAirFight;
	}

	public static void Load()
	{
		if (File.Exists(SAVE_PATH))
		{
			byte[] p_bytes = File.ReadAllBytes(SAVE_PATH);
			PlayerSettings playerSettings;
			try
			{
				playerSettings = (PlayerSettings)Utils.ConvertByteArrayToObject(p_bytes);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Player settings are incompatible with the current version and will be reset. " + ex.Message);
				Reset();
				Save();
				return;
			}
			playerSettings.HandleGameVersionUpdate();
			Instance.GameVersion = playerSettings.GameVersion;
			Instance.UnitVolume = playerSettings.UnitVolume;
			Instance.WeatherVolume = playerSettings.WeatherVolume;
			Instance.MusicVolume = playerSettings.MusicVolume;
			Instance.UIVolume = playerSettings.UIVolume;
			Instance.DifficultyLevel = playerSettings.DifficultyLevel;
			Instance.VsyncValue = playerSettings.VsyncValue;
			Instance.FollowAIMoves = playerSettings.FollowAIMoves;
			Instance.FastAllies = playerSettings.FastAllies;
			Instance.RandomDamageModifier = playerSettings.RandomDamageModifier;
			Instance.MoveCameraWithMouseOnEdge = playerSettings.MoveCameraWithMouseOnEdge;
			Instance.ShowTooltips = playerSettings.ShowTooltips;
			Instance.ShowInfoTile = playerSettings.ShowInfoTile;
			Instance.IsAISlowMode = playerSettings.IsAISlowMode;
			Instance.WinterDamageValue = playerSettings.WinterDamageValue;
			Instance.ComplexSupplies = playerSettings.ComplexSupplies;
			Instance.IsMorale = playerSettings.IsMorale;
			Instance.IsQuickMovement = playerSettings.IsQuickMovement;
			Instance.ShowClouds = playerSettings.ShowClouds;
			Instance.ZoomFactor = playerSettings.ZoomFactor;
			Instance.UIScale = playerSettings.UIScale;
			Instance.IsInfiniteAmmoAndFuel = playerSettings.IsInfiniteAmmoAndFuel;
			Instance.IsFogOfWar = playerSettings.IsFogOfWar;
			Instance.Fullscreen = playerSettings.Fullscreen;
			Instance.MaxFramerate = Mathf.Clamp(playerSettings.MaxFramerate, 30, 240);
			Instance.IsWeather = playerSettings.IsWeather;
			Instance.ISHistoricalFocus = playerSettings.ISHistoricalFocus;
			Instance.IsWeatherParticles = playerSettings.IsWeatherParticles;
			Instance.UserId = playerSettings.UserId;
			Instance.Username = playerSettings.Username;
			Instance.Email = playerSettings.Email;
			Instance.Password = playerSettings.Password;
			Instance.UnitSize = playerSettings.UnitSize;
			Instance.MoreContrastSelectedUnit = playerSettings.MoreContrastSelectedUnit;
			Instance.AutomaticallyHidePopups = playerSettings.AutomaticallyHidePopups;
			Instance.RealisticPlanes = playerSettings.RealisticPlanes;
			Instance.Partisans = playerSettings.Partisans;
			Instance.HideUnitsStats = playerSettings.HideUnitsStats;
			Instance.ProductionTakesTime = playerSettings.ProductionTakesTime;
			Instance.UnitIsDoneIndicator = playerSettings.UnitIsDoneIndicator;
			Instance.MoveCameraWithLeftClick = playerSettings.MoveCameraWithLeftClick;
			Instance.ShowBorders = playerSettings.ShowBorders;
			Instance.OneMoreTilePlanes = playerSettings.OneMoreTilePlanes;
			Instance.BlinkInactiveUnits = playerSettings.BlinkInactiveUnits;
			Instance.Overrun = playerSettings.Overrun;
			Instance.Policies = playerSettings.Policies;
			Instance.Manpower = playerSettings.Manpower;
			Instance.ZoneOfControl = playerSettings.ZoneOfControl;
			Instance.ColorBlindMode = playerSettings.ColorBlindMode;
			Instance.HideBattlePreview = playerSettings.HideBattlePreview;
			Instance.UnitsAlwaysBlink = playerSettings.UnitsAlwaysBlink;
			Instance.InvertedMouseSroll = playerSettings.InvertedMouseSroll;
			Instance.ShowUnitsNumbersOnCounters = playerSettings.ShowUnitsNumbersOnCounters;
			Instance.DisableVanillaSoundtracks = playerSettings.DisableVanillaSoundtracks;
			Instance.CameraBackgroundColor = playerSettings.CameraBackgroundColor;
			Instance.HideVisualCueCoreUnits = playerSettings.HideVisualCueCoreUnits;
			Instance.InstantSeasonChange = playerSettings.InstantSeasonChange;
			Instance.Upkeep = playerSettings.Upkeep;
			Instance.ChatChannel = playerSettings.ChatChannel;
			Instance.DefaultServerRegion = playerSettings.DefaultServerRegion;
			Instance.AutosavesAmount = playerSettings.AutosavesAmount;
			Instance.AutosavesCurrent = playerSettings.AutosavesCurrent;
			Instance.AutosavesFrequency = playerSettings.AutosavesFrequency;
			Instance.DisplayUnitsLevelOnCounter = playerSettings.DisplayUnitsLevelOnCounter;
			Instance.DamageReductionRepeatedAttacks = playerSettings.DamageReductionRepeatedAttacks;
			Instance.ShowUnitMovePath = playerSettings.ShowUnitMovePath;
			Instance.SkipEndTurnSummary = playerSettings.SkipEndTurnSummary;
			Instance.FiniteManpower = playerSettings.FiniteManpower;
			Instance.PrototypesDisabled = playerSettings.PrototypesDisabled;
			Instance.NoAAsupportDuringAirFight = playerSettings.NoAAsupportDuringAirFight;
			Instance.UseNATObyDefault = playerSettings.UseNATObyDefault;
			Instance.StricterFogOfWar = playerSettings.StricterFogOfWar;
			Instance.HideEnemyHP = playerSettings.HideEnemyHP;
			Instance.ShuffleMusics = playerSettings.ShuffleMusics;
			Instance.AvatarData = playerSettings.AvatarData;
			Instance.ScenarioGeneratorData = playerSettings.ScenarioGeneratorData;
			Instance.MoveCameraUp = playerSettings.MoveCameraUp;
			Instance.MoveCameraDown = playerSettings.MoveCameraDown;
			Instance.MoveCameraLeft = playerSettings.MoveCameraLeft;
			Instance.MoveCameraRight = playerSettings.MoveCameraRight;
			Instance.ZoomInCamera = playerSettings.ZoomInCamera;
			Instance.ZoomOutCamera = playerSettings.ZoomOutCamera;
			Instance.MoveOverlayUp = playerSettings.MoveOverlayUp;
			Instance.MoveOverlayDown = playerSettings.MoveOverlayDown;
			Instance.MoveOverlayLeft = playerSettings.MoveOverlayLeft;
			Instance.MoveOverlayRight = playerSettings.MoveOverlayRight;
			Instance.ScaleOverlayUp = playerSettings.ScaleOverlayUp;
			Instance.ScaleOverlayDown = playerSettings.ScaleOverlayDown;
			Instance.IncreaseOverlayOpacity = playerSettings.IncreaseOverlayOpacity;
			Instance.DecreaseOverlayOpacity = playerSettings.DecreaseOverlayOpacity;
			Instance.NextUnit = playerSettings.NextUnit;
			Instance.PreviousUnit = playerSettings.PreviousUnit;
			Instance.NextUnitAir = playerSettings.NextUnitAir;
			Instance.NextUnitNaval = playerSettings.NextUnitNaval;
			Instance.NextUnitGround = playerSettings.NextUnitGround;
			Instance.FocusOnSelectedUnit = playerSettings.FocusOnSelectedUnit;
			Instance.SwitchNATOGraphics = playerSettings.SwitchNATOGraphics;
			Instance.Quicksave = playerSettings.Quicksave;
			Instance.Quickload = playerSettings.Quickload;
			Instance.ManageMinimap = playerSettings.ManageMinimap;
			Instance.ShowHideGroundAirUnit = playerSettings.ShowHideGroundAirUnit;
			Instance.MoveCameraToMousePosWhenZooming = playerSettings.MoveCameraToMousePosWhenZooming;
			Instance.EyedropperTile = playerSettings.EyedropperTile;
			Instance.HideAllUnits = playerSettings.HideAllUnits;
			Instance.PutUnitToSleep = playerSettings.PutUnitToSleep;
			Instance.Disband = playerSettings.Disband;
			Instance.UnselectUnit = playerSettings.UnselectUnit;
			Instance.SkipUnit = playerSettings.SkipUnit;
			Instance.Waypoints = playerSettings.Waypoints;
			Instance.Disengage = playerSettings.Disengage;
			Instance.JumpLeftSideMap = playerSettings.JumpLeftSideMap;
			Instance.JumpRightSideMap = playerSettings.JumpRightSideMap;
			Instance.JumpDownSideMap = playerSettings.JumpDownSideMap;
			Instance.JumpUpSideMap = playerSettings.JumpUpSideMap;
			Instance.TogglePoliticalMap = playerSettings.TogglePoliticalMap;
			Instance.MoveUnit = playerSettings.MoveUnit;
			Instance.AttackUnit = playerSettings.AttackUnit;
			Instance.ShowUnitHistory = playerSettings.ShowUnitHistory;
			Instance.ShowBuilding = playerSettings.ShowBuilding;
			Instance.ShowHideUI = playerSettings.ShowHideUI;
			Instance.EndTurn = playerSettings.EndTurn;
			Instance.SelectPen = playerSettings.SelectPen;
			Instance.SelectEraser = playerSettings.SelectEraser;
			Instance.ChangeParatroopersState = playerSettings.ChangeParatroopersState;
			Instance.ChangeSubmarineState = playerSettings.ChangeSubmarineState;
			Instance.SetDefensive = playerSettings.SetDefensive;
			Instance.SetUnitAsGoal = playerSettings.SetUnitAsGoal;
			Instance.ActiveMods = playerSettings.ActiveMods;
			Instance.ResupplyManpower = playerSettings.ResupplyManpower;
			Instance.ResupplyAmmo = playerSettings.ResupplyAmmo;
			Instance.ResupplyFuel = playerSettings.ResupplyFuel;
			Instance.ResupplyAmmoAndFuel = playerSettings.ResupplyAmmoAndFuel;
			Instance.Quick2RespondOption = playerSettings.Quick2RespondOption;
			Instance.UnitSuffix = playerSettings.UnitSuffix;
			Instance.TechTree = playerSettings.TechTree;
			Instance.FastAI = playerSettings.FastAI;
			Instance.Friends = playerSettings.Friends;
			Instance.PublicChatOFF = playerSettings.PublicChatOFF;
			Instance.CompletedScenarioz = playerSettings.CompletedScenarioz;
			Instance.Undo = playerSettings.Undo;
			Instance.ToggleAISpeed = playerSettings.ToggleAISpeed;
			Instance.ToggleStore = playerSettings.ToggleStore;
			Instance.ToggleReserve = playerSettings.ToggleReserve;
			Instance.TimeSpeedUp = playerSettings.TimeSpeedUp;
			Instance.TimeSlowDown = playerSettings.TimeSlowDown;
			Instance.ToggleSuppliesMap = playerSettings.ToggleSuppliesMap;
			Instance.Objectives = playerSettings.Objectives;
			Instance.ToggleOverlay = playerSettings.ToggleOverlay;
			Instance.ToggleOrderOfBattle = playerSettings.ToggleOrderOfBattle;
			Instance.PauseGame = playerSettings.PauseGame;
			Instance.Language = playerSettings.Language;
			Instance.ChatMentions = playerSettings.ChatMentions;
			Instance.LastChatMentionTime = playerSettings.LastChatMentionTime;
			Instance.CensoredGermany = playerSettings.CensoredGermany;
			Instance.ShowShipNames = playerSettings.ShowShipNames;
			Instance.IsMovementInertia = playerSettings.IsMovementInertia;
			Instance.BattleAnimations = playerSettings.BattleAnimations;
			Instance._modDataBag = playerSettings._modDataBag;
			if (Instance.Resolution == null)
			{
				Instance.Resolution = new int[2, 2];
				Instance.Resolution[0, 0] = Screen.width;
				Instance.Resolution[1, 0] = Screen.height;
			}
			else
			{
				Instance.Resolution[0, 0] = playerSettings.Resolution[0, 0];
				Instance.Resolution[1, 0] = playerSettings.Resolution[1, 0];
			}
			if (Instance.DifficultyLevel == 0)
			{
				Instance.DifficultyLevel = 1;
			}
			if (Instance.UnitSize == 0f)
			{
				Instance.UnitSize = 0.425f;
			}
			if (Instance.Friends == null)
			{
				Instance.Friends = new List<string>();
			}
			if (Instance.CompletedScenarioz == null)
			{
				Instance.CompletedScenarioz = new Dictionary<string, string>();
			}
			if (Instance.WinterDamageValue > 5)
			{
				Instance.WinterDamageValue = 1;
			}
			if (string.IsNullOrEmpty(Instance.ChatChannel) || Instance.ChatChannel == "language.EN")
			{
				Instance.ChatChannel = "English";
			}
			if (string.IsNullOrEmpty(Instance.DefaultServerRegion))
			{
				Instance.DefaultServerRegion = "EU";
			}
			if (Instance.ScenarioGeneratorData == null)
			{
				Instance.ScenarioGeneratorData = ScenarioGeneratorData.CreateDefault();
			}
		}
		else
		{
			Reset();
			Save();
		}
		if (Instance.MaxFramerate < 30)
		{
			Instance.MaxFramerate = 30;
		}
		if (Instance.PauseGame == KeyCode.None)
		{
			Instance.PauseGame = KeyCode.P;
		}
		if (Instance.Objectives == KeyCode.None)
		{
			Instance.Objectives = KeyCode.O;
		}
		if (Instance.Disengage == KeyCode.None)
		{
			Instance.Disengage = KeyCode.LeftAlt;
		}
		if (Instance.MoveUnit == KeyCode.None)
		{
			Instance.MoveUnit = KeyCode.Mouse0;
		}
		if (Instance.AttackUnit == KeyCode.None)
		{
			Instance.AttackUnit = KeyCode.Mouse0;
		}
		if (Instance.ShowUnitHistory == KeyCode.None)
		{
			Instance.ShowUnitHistory = KeyCode.Mouse1;
		}
		if (Instance.UnselectUnit == KeyCode.None)
		{
			Instance.UnselectUnit = KeyCode.Mouse1;
		}
		if (Instance.Waypoints == KeyCode.None)
		{
			Instance.Waypoints = KeyCode.LeftControl;
		}
		if (Instance.Undo == KeyCode.None)
		{
			Instance.Undo = KeyCode.U;
		}
		if (Instance.ActiveMods == null)
		{
			Instance.ActiveMods = new List<string>();
		}
		if (Instance.Friends == null)
		{
			Instance.Friends = new List<string>();
		}
		if (Instance.CompletedScenarioz == null)
		{
			Instance.CompletedScenarioz = new Dictionary<string, string>();
		}
		if (Instance.Fullscreen)
		{
			Screen.SetResolution(Instance.Resolution[0, 0], Instance.Resolution[1, 0], FullScreenMode.ExclusiveFullScreen);
		}
		else
		{
			Screen.SetResolution(Instance.Resolution[0, 0], Instance.Resolution[1, 0], FullScreenMode.Windowed);
		}
		if (Instance.Resolution[0, 0] <= 10 || Instance.Resolution[1, 0] <= 10)
		{
			Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
		}
		if (Instance.Fullscreen)
		{
			Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
		}
		else
		{
			Screen.fullScreenMode = FullScreenMode.Windowed;
		}
		if (Instance.ChatMentions == null)
		{
			Instance.ChatMentions = new HashSet<string>();
			Instance.LastChatMentionTime = DateTime.Now;
		}
		if (Instance.ZoomFactor == 0f)
		{
			Instance.ZoomFactor = 1f;
		}
		if (Instance.UIScale == 0f)
		{
			Instance.UIScale = Mathf.Clamp((float)Screen.width / 1920f, 0.5f, 2f);
		}
		else if (Instance.UIScale <= 0.1f || Instance.UIScale > 2f)
		{
			Instance.UIScale = 1f;
		}
		SoundManager.instance.UI_Source.volume = Instance.UIVolume;
		SoundManager.instance.music_Source.volume = Instance.MusicVolume;
		LocalizationManager.SetLanguage(Instance.Language);
	}

	public static void Reset()
	{
		Instance.UnitVolume = 0.5f;
		Instance.WeatherVolume = 0.5f;
		Instance.MusicVolume = 0.5f;
		Instance.UIVolume = 0.5f;
		Instance.UnitSize = 0.37f;
		Instance.DifficultyLevel = 2;
		Instance.ZoomFactor = 2f;
		Instance.UIScale = Mathf.Clamp((float)Screen.width / 1920f, 0.5f, 2f);
		Instance.VsyncValue = 0;
		Instance.RandomDamageModifier = 0;
		Instance.FollowAIMoves = true;
		Instance.MoveCameraWithMouseOnEdge = true;
		Instance.ShowTooltips = true;
		Instance.ShowInfoTile = true;
		Instance.FastAllies = false;
		Instance.IsAISlowMode = true;
		Instance.WinterDamageValue = 1;
		Instance.ComplexSupplies = true;
		Instance.IsMorale = false;
		Instance.IsQuickMovement = false;
		Instance.IsFogOfWar = true;
		Instance.IsInfiniteAmmoAndFuel = false;
		Instance.ShowClouds = true;
		Instance.Fullscreen = false;
		Instance.InvertedMouseSroll = false;
		Instance.IsWeather = true;
		Instance.ISHistoricalFocus = true;
		Instance.IsWeatherParticles = true;
		Instance.UnitsAlwaysBlink = false;
		Instance.MoveCameraToMousePosWhenZooming = false;
		Instance.MoreContrastSelectedUnit = false;
		Instance.AutomaticallyHidePopups = true;
		Instance.Quick2RespondOption = false;
		Instance.UnitSuffix = true;
		Instance.TechTree = true;
		Instance.RealisticPlanes = false;
		Instance.Partisans = true;
		Instance.MaxFramerate = Mathf.Clamp((int)Screen.currentResolution.refreshRateRatio.value, 30, 240);
		Instance.HideUnitsStats = false;
		Instance.ProductionTakesTime = true;
		Instance.UnitIsDoneIndicator = true;
		Instance.MoveCameraWithLeftClick = false;
		Instance.ShowBorders = true;
		Instance.FastAI = false;
		Instance.OneMoreTilePlanes = false;
		Instance.BlinkInactiveUnits = true;
		Instance.Overrun = true;
		Instance.Policies = true;
		Instance.Manpower = true;
		Instance.FiniteManpower = true;
		Instance.ColorBlindMode = false;
		Instance.PublicChatOFF = false;
		Instance.HideBattlePreview = false;
		Instance.ShowUnitsNumbersOnCounters = false;
		Instance.DisableVanillaSoundtracks = false;
		Instance.HideVisualCueCoreUnits = false;
		Instance.InstantSeasonChange = false;
		Instance.ZoneOfControl = true;
		Instance.Upkeep = true;
		Instance.ChatChannel = "English";
		Instance.DefaultServerRegion = "EU";
		Instance.Friends = new List<string>();
		Instance.AutosavesAmount = 5;
		Instance.AutosavesCurrent = 0;
		Instance.AutosavesFrequency = 1;
		Instance.DisplayUnitsLevelOnCounter = true;
		Instance.DamageReductionRepeatedAttacks = false;
		Instance.ShowUnitMovePath = true;
		Instance.SkipEndTurnSummary = false;
		Instance.NoAAsupportDuringAirFight = false;
		Instance.UseNATObyDefault = false;
		Instance.StricterFogOfWar = false;
		Instance.HideEnemyHP = false;
		Instance.ShuffleMusics = true;
		Instance.ShowShipNames = true;
		Instance.IsMovementInertia = true;
		Instance.BattleAnimations = true;
		Instance.ChatMentions = new HashSet<string>();
		Instance.LastChatMentionTime = DateTime.Now;
		Instance.ScenarioGeneratorData = ScenarioGeneratorData.CreateDefault();
		Instance.Resolution = new int[2, 2];
		Instance.Resolution[0, 0] = Screen.width;
		Instance.Resolution[1, 0] = Screen.height;
		Instance.ZoomInCamera = KeyCode.KeypadPlus;
		Instance.ZoomOutCamera = KeyCode.KeypadMinus;
		Instance.MoveOverlayUp = KeyCode.UpArrow;
		Instance.MoveOverlayDown = KeyCode.DownArrow;
		Instance.MoveOverlayLeft = KeyCode.LeftArrow;
		Instance.MoveOverlayRight = KeyCode.RightArrow;
		Instance.ScaleOverlayUp = KeyCode.PageUp;
		Instance.ScaleOverlayDown = KeyCode.PageDown;
		Instance.IncreaseOverlayOpacity = KeyCode.E;
		Instance.DecreaseOverlayOpacity = KeyCode.Q;
		Instance.NextUnit = KeyCode.RightBracket;
		Instance.PreviousUnit = KeyCode.LeftBracket;
		Instance.NextUnitGround = KeyCode.Alpha1;
		Instance.NextUnitAir = KeyCode.Alpha2;
		Instance.NextUnitNaval = KeyCode.Alpha3;
		Instance.FocusOnSelectedUnit = KeyCode.F;
		Instance.SwitchNATOGraphics = KeyCode.N;
		Instance.Quicksave = KeyCode.F5;
		Instance.Quickload = KeyCode.F6;
		Instance.ManageMinimap = KeyCode.M;
		Instance.ShowHideGroundAirUnit = KeyCode.Tab;
		Instance.ShowBuilding = KeyCode.Slash;
		Instance.ChangeParatroopersState = KeyCode.P;
		Instance.ChangeSubmarineState = KeyCode.S;
		Instance.SetDefensive = KeyCode.D;
		Instance.SetUnitAsGoal = KeyCode.G;
		Instance.MoveUnit = KeyCode.Mouse0;
		Instance.AttackUnit = KeyCode.Mouse0;
		Instance.ShowUnitHistory = KeyCode.Mouse1;
		Instance.EyedropperTile = KeyCode.I;
		Instance.HideAllUnits = KeyCode.H;
		Instance.PutUnitToSleep = KeyCode.Z;
		Instance.ResupplyManpower = KeyCode.Keypad0;
		Instance.ResupplyAmmo = KeyCode.Keypad1;
		Instance.ResupplyFuel = KeyCode.Keypad2;
		Instance.ResupplyAmmoAndFuel = KeyCode.Keypad3;
		Instance.Disband = KeyCode.Delete;
		Instance.UnselectUnit = KeyCode.Mouse1;
		Instance.SkipUnit = KeyCode.Space;
		Instance.Waypoints = KeyCode.LeftControl;
		Instance.Disengage = KeyCode.LeftAlt;
		Instance.Undo = KeyCode.U;
		Instance.Objectives = KeyCode.O;
		Instance.ToggleOverlay = KeyCode.None;
		Instance.ToggleOrderOfBattle = KeyCode.L;
		Instance.PauseGame = KeyCode.P;
		Instance.TogglePoliticalMap = KeyCode.M;
		Instance.ShowHideUI = KeyCode.F10;
		Instance.SelectPen = KeyCode.None;
		Instance.SelectEraser = KeyCode.None;
		Instance.EndTurn = KeyCode.Backspace;
		Instance.ToggleStore = KeyCode.None;
		Instance.ToggleReserve = KeyCode.None;
		Instance.TimeSpeedUp = KeyCode.Plus;
		Instance.TimeSlowDown = KeyCode.Minus;
		Instance.ToggleSuppliesMap = KeyCode.F9;
		Instance.CameraBackgroundColor = "#000000";
		Instance.Language = LocalizationManager.GetSupportedLanguageCode();
		Instance.CensoredGermany = SteamManager.ShouldApplyCensor();
		if (Instance.Language == GameLanguages.FR)
		{
			Instance.MoveCameraUp = KeyCode.Z;
			Instance.MoveCameraDown = KeyCode.S;
			Instance.MoveCameraLeft = KeyCode.Q;
			Instance.MoveCameraRight = KeyCode.D;
		}
		else
		{
			Instance.MoveCameraUp = KeyCode.W;
			Instance.MoveCameraDown = KeyCode.S;
			Instance.MoveCameraLeft = KeyCode.A;
			Instance.MoveCameraRight = KeyCode.D;
		}
		Instance.ZoomFactor = 5f;
	}

	public int GetNextAutosaveNumber()
	{
		AutosavesCurrent++;
		if (AutosavesCurrent > AutosavesAmount)
		{
			AutosavesCurrent = 1;
		}
		return AutosavesCurrent;
	}

	public string GetLocalizedDifficulty(int p_difficultyValue)
	{
		switch (p_difficultyValue)
		{
		case 1:
			return LocalizationManager.Translate("txt-easy");
		default:
			return LocalizationManager.Translate("txt-normal");
		case 3:
			return LocalizationManager.Translate("txt-hard");
		case 4:
			return LocalizationManager.Translate("txt-hardcore");
		}
	}

	public void CheckNeedRefreshChatMentions()
	{
		if ((DateTime.Now - LastChatMentionTime).Days >= 3)
		{
			ChatMentions.Clear();
		}
	}

	private void HandleGameVersionUpdate()
	{
		string version = Application.version;
		if (version != GameVersion)
		{
			if (IsPriorVersion("8.3.0"))
			{
				ShowShipNames = true;
				ToggleSuppliesMap = KeyCode.F9;
			}
			if (IsPriorVersion("8.3.9"))
			{
				IsMovementInertia = true;
			}
			if (IsPriorVersion("8.3.11"))
			{
				BattleAnimations = true;
			}
		}
		GameVersion = version;
	}

	private bool IsPriorVersion(string p_versionToCompare)
	{
		if (string.IsNullOrEmpty(GameVersion))
		{
			return true;
		}
		try
		{
			Version version = new Version(GameVersion);
			Version value = new Version(p_versionToCompare);
			return version.CompareTo(value) < 0;
		}
		catch (FormatException)
		{
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
	public static bool listeningForNewKeybind;

	[Header("Key Binding")]
	public string keyToChangeName;

	public GameObject keyBindingPanel;

	public List<Button> keysButtonsElement;

	[SerializeField]
	private List<GameObject> _settingsCategoriesPanels;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI UIscale_text;

	[SerializeField]
	private TextMeshProUGUI difficulty_Text;

	[SerializeField]
	private TextMeshProUGUI winterDamage_Text;

	[SerializeField]
	private TextMeshProUGUI randomDamageModifier_text;

	[SerializeField]
	private TextMeshProUGUI vsync_Text;

	[SerializeField]
	private TextMeshProUGUI zoomSpeed_text;

	[SerializeField]
	private TextMeshProUGUI unitsSize_text;

	[SerializeField]
	private TextMeshProUGUI autosavesAmount_text;

	[SerializeField]
	private TextMeshProUGUI autosavesFrequency_text;

	[SerializeField]
	private TextMeshProUGUI framerate_text;

	[SerializeField]
	private TextMeshProUGUI masterVolume_text;

	[SerializeField]
	private TextMeshProUGUI musicVolume_text;

	[SerializeField]
	private TextMeshProUGUI unitsVolume_text;

	[SerializeField]
	private TextMeshProUGUI weatherVolume_text;

	[SerializeField]
	private TextMeshProUGUI UIvolume_text;

	[Header("Inputfields")]
	public InputField cameraBackgroundColor_inputfield;

	[Header("Sliders")]
	public Slider unitVolume_slider;

	public Slider weatherVolume_slider;

	public Slider UIVolume_slider;

	public Slider musicVolume_slider;

	public Slider zoom_Slider;

	public Slider antiAliasing_Slider;

	public Slider vsync_Slider;

	public Slider unitSize_Slider;

	public Slider winterDamage_Slider;

	public Slider randomDamageModifier_slider;

	public Slider autosavesAmount_slider;

	public Slider autosavesFrequency_slider;

	public Slider framerate_slider;

	public Slider UIscale_slider;

	[Header("Dropdowns")]
	public TMP_Dropdown language_dropdown;

	public TMP_Dropdown resolution_dropdown;

	[Header("Toggles")]
	public Toggle showTooltips_Toggle;

	public Toggle moveCameraWithEdge_Toggle;

	public Toggle cameraFollowAI_Toggle;

	public Toggle showTileInfo_Toggle;

	public Toggle slowAIMode_Toggle;

	public Toggle complexSupplies_Toggle;

	public Toggle morale_Toggle;

	public Toggle quickMovement_Toggle;

	public Toggle fogOfWar_Toggle;

	public Toggle infiniteFuelAmmo_Toggle;

	public Toggle showClouds_Toggle;

	public Toggle fullscreen_Toggle;

	public Toggle weather_Toggle;

	public Toggle historicalFocus_Toggle;

	public Toggle weatherParticles_toggle;

	public Toggle moveCameraToMousePosZooming_toggle;

	public Toggle moreContrast_toggle;

	public Toggle automaticallyHidePopups_Toggle;

	public Toggle quick2Respond_Toggle;

	public Toggle unitSuffix_Toggle;

	public Toggle techTree_Toggle;

	public Toggle realisticPlanes_Toggle;

	public Toggle partisans_Toggle;

	public Toggle showUnitsStats_Toggle;

	public Toggle productionTakesTime_Toggle;

	public Toggle unitIsDoneIndicator_toggle;

	public Toggle moveCameraWithLeftClick_toggle;

	public Toggle borders_toggle;

	public Toggle fastAI_toggle;

	public Toggle OneMoreTilePlanes_toggle;

	public Toggle blinkInactiveUnits_toggle;

	public Toggle overrun_toggle;

	public Toggle policies_toggle;

	public Toggle manpower_toggle;

	public Toggle finiteManpower_toggle;

	public Toggle colorBlind_toggle;

	public Toggle connectPublicChat_toggle;

	public Toggle fastAIAllyOnly_toggle;

	public Toggle hideBattlePreview_toggle;

	public Toggle unitsAlwaysBlink_toggle;

	public Toggle invertedMouseScroll_toggle;

	public Toggle showUnitsNumberOnCounter_toggle;

	public Toggle disableVanillaSoundtracks_toggle;

	public Toggle hideVisualCueForCoreUnits_toggle;

	public Toggle instantWinter_toggle;

	public Toggle zoneOfControl_toggle;

	public Toggle upkeep_toggle;

	public Toggle displayUnitLevelOnCounter_toggle;

	public Toggle damageReductionAttackFrequency_toggle;

	public Toggle censoredGermany_toggle;

	public Toggle showUnitMovePath_toggle;

	public Toggle prototypesDisabled_toggle;

	public Toggle noAAsupportInAirBattles_toggle;

	public Toggle useNATOiconsByDefault_toggle;

	public Toggle stricterFogOfWar_toggle;

	public Toggle hideEnemyHP_toggle;

	public Toggle skipEndTurnSummary_toggle;

	public Toggle showShipsNames_toggle;

	public Toggle movementInertia_toggle;

	public Toggle battleAnimations_toggle;

	private void Awake()
	{
		RefreshSettings();
	}

	private void Update()
	{
		if (listeningForNewKeybind)
		{
			ListenForNewKey();
		}
	}

	public void ResetSettingsToDefault()
	{
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-are you sure war"), ConfirmResetSettings);
	}

	private void ConfirmResetSettings()
	{
		PlayerSettings.Reset();
		RefreshSettings();
	}

	public void SelectSettingsCategory(GameObject p_settingsCategory)
	{
		SoundManager.instance.PlayUIClickSound();
		foreach (GameObject settingsCategoriesPanel in _settingsCategoriesPanels)
		{
			settingsCategoriesPanel.SetActive(settingsCategoriesPanel == p_settingsCategory);
		}
	}

	public void RefreshUIScaleSliderValue()
	{
		UIscale_text.text = LocalizationManager.Translate("txt-UI scale") + ": " + Utils.Truncate(UIscale_slider.value * 100f, 0) + "%";
	}

	private void RefreshSettings()
	{
		SetUIScaleOnGameOpens();
		SetVsyncOnGameOpens();
		RefreshDifficultyValue();
		SetVolumeOnGameOpens();
		SetZoomSpeedOnGameOpens();
		SetWinterDamageOnGameOpens();
		SetRandomDamageModifierOnGameOpens();
		SetUnitSizeOnGameOpens();
		SetMaxFramerateOnGameOpens();
		SetKeyBindingsOnGameOpens();
		SetAutosavesAmountOnGameOpens();
		SetAutosavesFrequencyOnGameOpens();
		if (!string.IsNullOrEmpty(PlayerSettings.Instance.CameraBackgroundColor))
		{
			cameraBackgroundColor_inputfield.SetTextWithoutNotify(PlayerSettings.Instance.CameraBackgroundColor);
		}
		cameraFollowAI_Toggle.onValueChanged.RemoveAllListeners();
		cameraFollowAI_Toggle.isOn = PlayerSettings.Instance.FollowAIMoves;
		cameraFollowAI_Toggle.onValueChanged.AddListener(delegate
		{
			SetCameraMovementTracking();
		});
		morale_Toggle.onValueChanged.RemoveAllListeners();
		morale_Toggle.isOn = PlayerSettings.Instance.IsMorale;
		morale_Toggle.onValueChanged.AddListener(delegate
		{
			SetMorale();
		});
		complexSupplies_Toggle.onValueChanged.RemoveAllListeners();
		complexSupplies_Toggle.isOn = PlayerSettings.Instance.ComplexSupplies;
		complexSupplies_Toggle.onValueChanged.AddListener(delegate
		{
			SetComplexSupplies();
		});
		slowAIMode_Toggle.onValueChanged.RemoveAllListeners();
		slowAIMode_Toggle.isOn = PlayerSettings.Instance.IsAISlowMode;
		slowAIMode_Toggle.onValueChanged.AddListener(delegate
		{
			SetAISlowMode();
		});
		showTileInfo_Toggle.onValueChanged.RemoveAllListeners();
		showTileInfo_Toggle.isOn = PlayerSettings.Instance.ShowInfoTile;
		showTileInfo_Toggle.onValueChanged.AddListener(delegate
		{
			SetShowInfoTile();
		});
		showTooltips_Toggle.onValueChanged.RemoveAllListeners();
		showTooltips_Toggle.isOn = PlayerSettings.Instance.ShowTooltips;
		showTooltips_Toggle.onValueChanged.AddListener(delegate
		{
			SetShowTooltips();
		});
		moveCameraWithEdge_Toggle.onValueChanged.RemoveAllListeners();
		moveCameraWithEdge_Toggle.isOn = PlayerSettings.Instance.MoveCameraWithMouseOnEdge;
		moveCameraWithEdge_Toggle.onValueChanged.AddListener(delegate
		{
			SetMoveCameraWithMouseOnEdge();
		});
		quickMovement_Toggle.onValueChanged.RemoveAllListeners();
		quickMovement_Toggle.isOn = PlayerSettings.Instance.IsQuickMovement;
		quickMovement_Toggle.onValueChanged.AddListener(delegate
		{
			SetQuickMovement();
		});
		fogOfWar_Toggle.onValueChanged.RemoveAllListeners();
		fogOfWar_Toggle.isOn = PlayerSettings.Instance.IsFogOfWar;
		fogOfWar_Toggle.onValueChanged.AddListener(delegate
		{
			SetFogOfWar();
		});
		infiniteFuelAmmo_Toggle.onValueChanged.RemoveAllListeners();
		infiniteFuelAmmo_Toggle.isOn = PlayerSettings.Instance.IsInfiniteAmmoAndFuel;
		infiniteFuelAmmo_Toggle.onValueChanged.AddListener(delegate
		{
			SetInfiniteAmmoFuel();
		});
		showClouds_Toggle.onValueChanged.RemoveAllListeners();
		showClouds_Toggle.isOn = PlayerSettings.Instance.ShowClouds;
		showClouds_Toggle.onValueChanged.AddListener(delegate
		{
			ShowClouds();
		});
		fullscreen_Toggle.onValueChanged.RemoveAllListeners();
		fullscreen_Toggle.isOn = PlayerSettings.Instance.Fullscreen;
		fullscreen_Toggle.onValueChanged.AddListener(delegate
		{
			ManageFullscreen();
		});
		weather_Toggle.onValueChanged.RemoveAllListeners();
		weather_Toggle.isOn = PlayerSettings.Instance.IsWeather;
		weather_Toggle.onValueChanged.AddListener(delegate
		{
			SetWeather();
		});
		historicalFocus_Toggle.onValueChanged.RemoveAllListeners();
		historicalFocus_Toggle.isOn = PlayerSettings.Instance.ISHistoricalFocus;
		historicalFocus_Toggle.onValueChanged.AddListener(delegate
		{
			SetHistoricalFocuses();
		});
		weatherParticles_toggle.onValueChanged.RemoveAllListeners();
		weatherParticles_toggle.isOn = PlayerSettings.Instance.IsWeatherParticles;
		weatherParticles_toggle.onValueChanged.AddListener(delegate
		{
			SetWeatherParticles();
		});
		language_dropdown.onValueChanged.RemoveAllListeners();
		language_dropdown.ClearOptions();
		foreach (GameLanguages value in Enum.GetValues(typeof(GameLanguages)))
		{
			language_dropdown.options.Add(new TMP_Dropdown.OptionData
			{
				text = LocalizationManager.Translate("language." + value)
			});
		}
		language_dropdown.SetValueWithoutNotify(Utils.GetIndexByName(language_dropdown, LocalizationManager.Translate("language." + PlayerSettings.Instance.Language)));
		language_dropdown.RefreshShownValue();
		language_dropdown.onValueChanged.AddListener(delegate
		{
			SetLanguage();
		});
		resolution_dropdown.onValueChanged.RemoveAllListeners();
		resolution_dropdown.ClearOptions();
		Resolution[] array = (from res in Screen.resolutions
			orderby res.width, res.height, res.refreshRateRatio.value
			select res).ToArray();
		string text = "";
		Resolution[] array2 = array;
		for (int num = 0; num < array2.Length; num++)
		{
			Resolution resolution = array2[num];
			if (string.IsNullOrEmpty(text) || !(text == resolution.width + " x " + resolution.height))
			{
				text = resolution.width + " x " + resolution.height;
				resolution_dropdown.options.Add(new TMP_Dropdown.OptionData(text));
			}
		}
		SetResolutionOnGameOpens();
		resolution_dropdown.onValueChanged.AddListener(delegate
		{
			SetResolution();
		});
		moveCameraToMousePosZooming_toggle.onValueChanged.RemoveAllListeners();
		moveCameraToMousePosZooming_toggle.isOn = PlayerSettings.Instance.MoveCameraToMousePosWhenZooming;
		moveCameraToMousePosZooming_toggle.onValueChanged.AddListener(delegate
		{
			SetMoveCameraToMousePosWhenZooming();
		});
		moreContrast_toggle.onValueChanged.RemoveAllListeners();
		moreContrast_toggle.isOn = PlayerSettings.Instance.MoreContrastSelectedUnit;
		moreContrast_toggle.onValueChanged.AddListener(delegate
		{
			MoreContrast();
		});
		automaticallyHidePopups_Toggle.onValueChanged.RemoveAllListeners();
		automaticallyHidePopups_Toggle.isOn = PlayerSettings.Instance.AutomaticallyHidePopups;
		automaticallyHidePopups_Toggle.onValueChanged.AddListener(delegate
		{
			AutomaticallyHidePopups();
		});
		quick2Respond_Toggle.onValueChanged.RemoveAllListeners();
		quick2Respond_Toggle.isOn = PlayerSettings.Instance.Quick2RespondOption;
		quick2Respond_Toggle.onValueChanged.AddListener(delegate
		{
			SetQuick2RespondOption();
		});
		unitSuffix_Toggle.onValueChanged.RemoveAllListeners();
		unitSuffix_Toggle.isOn = PlayerSettings.Instance.UnitSuffix;
		unitSuffix_Toggle.onValueChanged.AddListener(delegate
		{
			SetUnitSuffixOption();
		});
		techTree_Toggle.onValueChanged.RemoveAllListeners();
		techTree_Toggle.isOn = PlayerSettings.Instance.TechTree;
		techTree_Toggle.onValueChanged.AddListener(delegate
		{
			SetTechTreeOption();
		});
		realisticPlanes_Toggle.onValueChanged.RemoveAllListeners();
		realisticPlanes_Toggle.isOn = PlayerSettings.Instance.RealisticPlanes;
		realisticPlanes_Toggle.onValueChanged.AddListener(delegate
		{
			SetRealisticPlanesOption();
		});
		partisans_Toggle.onValueChanged.RemoveAllListeners();
		partisans_Toggle.isOn = PlayerSettings.Instance.Partisans;
		partisans_Toggle.onValueChanged.AddListener(delegate
		{
			SetPartisans();
		});
		showUnitsStats_Toggle.onValueChanged.RemoveAllListeners();
		showUnitsStats_Toggle.isOn = PlayerSettings.Instance.HideUnitsStats;
		showUnitsStats_Toggle.onValueChanged.AddListener(delegate
		{
			SetShowUnitsStats();
		});
		productionTakesTime_Toggle.onValueChanged.RemoveAllListeners();
		productionTakesTime_Toggle.isOn = PlayerSettings.Instance.ProductionTakesTime;
		productionTakesTime_Toggle.onValueChanged.AddListener(delegate
		{
			SetProductionTakesTime();
		});
		unitIsDoneIndicator_toggle.onValueChanged.RemoveAllListeners();
		unitIsDoneIndicator_toggle.isOn = PlayerSettings.Instance.UnitIsDoneIndicator;
		unitIsDoneIndicator_toggle.onValueChanged.AddListener(delegate
		{
			SetUnitIsDoneIndicator();
		});
		moveCameraWithLeftClick_toggle.onValueChanged.RemoveAllListeners();
		moveCameraWithLeftClick_toggle.isOn = PlayerSettings.Instance.MoveCameraWithLeftClick;
		moveCameraWithLeftClick_toggle.onValueChanged.AddListener(delegate
		{
			SetMoveCameraWithLeftClick();
		});
		borders_toggle.onValueChanged.RemoveAllListeners();
		borders_toggle.isOn = PlayerSettings.Instance.ShowBorders;
		borders_toggle.onValueChanged.AddListener(delegate
		{
			SetShowBorders();
		});
		fastAI_toggle.onValueChanged.RemoveAllListeners();
		fastAI_toggle.isOn = PlayerSettings.Instance.FastAI;
		fastAI_toggle.onValueChanged.AddListener(delegate
		{
			SetFastAI();
		});
		blinkInactiveUnits_toggle.onValueChanged.RemoveAllListeners();
		blinkInactiveUnits_toggle.isOn = PlayerSettings.Instance.BlinkInactiveUnits;
		blinkInactiveUnits_toggle.onValueChanged.AddListener(delegate
		{
			SetBlinkInactiveUnits();
		});
		overrun_toggle.onValueChanged.RemoveAllListeners();
		overrun_toggle.isOn = PlayerSettings.Instance.Overrun;
		overrun_toggle.onValueChanged.AddListener(delegate
		{
			SetOverrun();
		});
		OneMoreTilePlanes_toggle.onValueChanged.RemoveAllListeners();
		OneMoreTilePlanes_toggle.isOn = PlayerSettings.Instance.OneMoreTilePlanes;
		OneMoreTilePlanes_toggle.onValueChanged.AddListener(delegate
		{
			SetOneMoreTilePlanes();
		});
		policies_toggle.onValueChanged.RemoveAllListeners();
		policies_toggle.isOn = PlayerSettings.Instance.Policies;
		policies_toggle.onValueChanged.AddListener(delegate
		{
			SetPolicies();
		});
		manpower_toggle.onValueChanged.RemoveAllListeners();
		manpower_toggle.isOn = PlayerSettings.Instance.Manpower;
		manpower_toggle.onValueChanged.AddListener(delegate
		{
			SetManpower();
		});
		finiteManpower_toggle.onValueChanged.RemoveAllListeners();
		finiteManpower_toggle.isOn = PlayerSettings.Instance.FiniteManpower;
		finiteManpower_toggle.onValueChanged.AddListener(delegate
		{
			SetFiniteManpower();
		});
		colorBlind_toggle.onValueChanged.RemoveAllListeners();
		colorBlind_toggle.isOn = PlayerSettings.Instance.ColorBlindMode;
		colorBlind_toggle.onValueChanged.AddListener(delegate
		{
			SetColorBlindMode();
		});
		connectPublicChat_toggle.onValueChanged.RemoveAllListeners();
		connectPublicChat_toggle.isOn = PlayerSettings.Instance.PublicChatOFF;
		connectPublicChat_toggle.onValueChanged.AddListener(delegate
		{
			SetConnectPublicChat();
		});
		fastAIAllyOnly_toggle.onValueChanged.RemoveAllListeners();
		fastAIAllyOnly_toggle.isOn = PlayerSettings.Instance.FastAllies;
		fastAIAllyOnly_toggle.onValueChanged.AddListener(delegate
		{
			SetFastAllies();
		});
		hideBattlePreview_toggle.onValueChanged.RemoveAllListeners();
		hideBattlePreview_toggle.isOn = PlayerSettings.Instance.HideBattlePreview;
		hideBattlePreview_toggle.onValueChanged.AddListener(delegate
		{
			SetHideBattlePreview();
		});
		unitsAlwaysBlink_toggle.onValueChanged.RemoveAllListeners();
		unitsAlwaysBlink_toggle.isOn = PlayerSettings.Instance.UnitsAlwaysBlink;
		unitsAlwaysBlink_toggle.onValueChanged.AddListener(delegate
		{
			SetUnitsAlwaysBlink();
		});
		invertedMouseScroll_toggle.onValueChanged.RemoveAllListeners();
		invertedMouseScroll_toggle.isOn = PlayerSettings.Instance.InvertedMouseSroll;
		invertedMouseScroll_toggle.onValueChanged.AddListener(delegate
		{
			SetInvertedMouseScroll();
		});
		showUnitsNumberOnCounter_toggle.onValueChanged.RemoveAllListeners();
		showUnitsNumberOnCounter_toggle.isOn = PlayerSettings.Instance.ShowUnitsNumbersOnCounters;
		showUnitsNumberOnCounter_toggle.onValueChanged.AddListener(delegate
		{
			SetShowUnitNumberOnCounters();
		});
		disableVanillaSoundtracks_toggle.onValueChanged.RemoveAllListeners();
		disableVanillaSoundtracks_toggle.isOn = PlayerSettings.Instance.DisableVanillaSoundtracks;
		disableVanillaSoundtracks_toggle.onValueChanged.AddListener(delegate
		{
			SetDisableVanillaSoundtracks();
		});
		hideVisualCueForCoreUnits_toggle.onValueChanged.RemoveAllListeners();
		hideVisualCueForCoreUnits_toggle.isOn = PlayerSettings.Instance.HideVisualCueCoreUnits;
		hideVisualCueForCoreUnits_toggle.onValueChanged.AddListener(delegate
		{
			SetHideVisualCueForCoreUnits();
		});
		instantWinter_toggle.onValueChanged.RemoveAllListeners();
		instantWinter_toggle.isOn = PlayerSettings.Instance.InstantSeasonChange;
		instantWinter_toggle.onValueChanged.AddListener(delegate
		{
			SetInstantWinter();
		});
		zoneOfControl_toggle.onValueChanged.RemoveAllListeners();
		zoneOfControl_toggle.isOn = PlayerSettings.Instance.ZoneOfControl;
		zoneOfControl_toggle.onValueChanged.AddListener(delegate
		{
			SetZoneOfControl();
		});
		upkeep_toggle.onValueChanged.RemoveAllListeners();
		upkeep_toggle.isOn = PlayerSettings.Instance.Upkeep;
		upkeep_toggle.onValueChanged.AddListener(delegate
		{
			SetUpkeep();
		});
		displayUnitLevelOnCounter_toggle.onValueChanged.RemoveAllListeners();
		displayUnitLevelOnCounter_toggle.isOn = PlayerSettings.Instance.DisplayUnitsLevelOnCounter;
		displayUnitLevelOnCounter_toggle.onValueChanged.AddListener(delegate
		{
			SetDisplayUnitsLevelOnCounter();
		});
		damageReductionAttackFrequency_toggle.onValueChanged.RemoveAllListeners();
		damageReductionAttackFrequency_toggle.isOn = PlayerSettings.Instance.DamageReductionRepeatedAttacks;
		damageReductionAttackFrequency_toggle.onValueChanged.AddListener(delegate
		{
			SetDamageReductionRepeatedAttacks();
		});
		censoredGermany_toggle.onValueChanged.RemoveAllListeners();
		censoredGermany_toggle.isOn = PlayerSettings.Instance.CensoredGermany;
		censoredGermany_toggle.onValueChanged.AddListener(delegate
		{
			SetCensoredGermany();
		});
		showUnitMovePath_toggle.onValueChanged.RemoveAllListeners();
		showUnitMovePath_toggle.isOn = PlayerSettings.Instance.ShowUnitMovePath;
		showUnitMovePath_toggle.onValueChanged.AddListener(delegate
		{
			SetShowUnitMovePath();
		});
		prototypesDisabled_toggle.onValueChanged.RemoveAllListeners();
		prototypesDisabled_toggle.isOn = PlayerSettings.Instance.PrototypesDisabled;
		prototypesDisabled_toggle.onValueChanged.AddListener(delegate
		{
			SetPrototypesDisabled();
		});
		noAAsupportInAirBattles_toggle.onValueChanged.RemoveAllListeners();
		noAAsupportInAirBattles_toggle.isOn = PlayerSettings.Instance.NoAAsupportDuringAirFight;
		noAAsupportInAirBattles_toggle.onValueChanged.AddListener(delegate
		{
			SetNoAASupportDuringAirFight();
		});
		useNATOiconsByDefault_toggle.onValueChanged.RemoveAllListeners();
		useNATOiconsByDefault_toggle.isOn = PlayerSettings.Instance.UseNATObyDefault;
		useNATOiconsByDefault_toggle.onValueChanged.AddListener(delegate
		{
			SetNATOiconsByDefault();
		});
		stricterFogOfWar_toggle.onValueChanged.RemoveAllListeners();
		stricterFogOfWar_toggle.isOn = PlayerSettings.Instance.StricterFogOfWar;
		stricterFogOfWar_toggle.onValueChanged.AddListener(delegate
		{
			SetStricterFogOfWar();
		});
		hideEnemyHP_toggle.onValueChanged.RemoveAllListeners();
		hideEnemyHP_toggle.isOn = PlayerSettings.Instance.HideEnemyHP;
		hideEnemyHP_toggle.onValueChanged.AddListener(delegate
		{
			SetHideEnemyHP();
		});
		skipEndTurnSummary_toggle.onValueChanged.RemoveAllListeners();
		skipEndTurnSummary_toggle.isOn = PlayerSettings.Instance.SkipEndTurnSummary;
		skipEndTurnSummary_toggle.onValueChanged.AddListener(delegate
		{
			SetSkipEndTurnSummary();
		});
		showShipsNames_toggle.onValueChanged.RemoveAllListeners();
		showShipsNames_toggle.isOn = PlayerSettings.Instance.ShowShipNames;
		showShipsNames_toggle.onValueChanged.AddListener(delegate
		{
			SetShowShipNames();
		});
		movementInertia_toggle.onValueChanged.RemoveAllListeners();
		movementInertia_toggle.isOn = PlayerSettings.Instance.IsMovementInertia;
		movementInertia_toggle.onValueChanged.AddListener(delegate
		{
			SetMovementInertia();
		});
		battleAnimations_toggle.onValueChanged.RemoveAllListeners();
		battleAnimations_toggle.isOn = PlayerSettings.Instance.BattleAnimations;
		battleAnimations_toggle.onValueChanged.AddListener(delegate
		{
			SetBattleAnimations();
		});
	}

	private void SetResolutionOnGameOpens()
	{
		if (PlayerSettings.Instance.Resolution == null)
		{
			return;
		}
		string text = PlayerSettings.Instance.Resolution[0, 0] + " x " + PlayerSettings.Instance.Resolution[1, 0];
		for (int i = 0; i < resolution_dropdown.options.Count; i++)
		{
			if (resolution_dropdown.options[i].text == text)
			{
				resolution_dropdown.value = i;
				break;
			}
		}
	}

	private void SetZoomSpeedOnGameOpens()
	{
		zoom_Slider.value = PlayerSettings.Instance.ZoomFactor;
		zoomSpeed_text.text = LocalizationManager.Translate("txt-zoom speed") + ": x" + Utils.Truncate(PlayerSettings.Instance.ZoomFactor, 1);
	}

	private void SetVsyncOnGameOpens()
	{
		if (PlayerSettings.Instance.VsyncValue == 0)
		{
			vsync_Text.text = "No Vsync";
		}
		if (PlayerSettings.Instance.VsyncValue == 1)
		{
			vsync_Text.text = "Every V blank";
		}
		if (PlayerSettings.Instance.VsyncValue == 2)
		{
			vsync_Text.text = "Every second V blank";
		}
		vsync_Slider.value = PlayerSettings.Instance.VsyncValue;
	}

	public void SetVSync()
	{
		PlayerSettings.Instance.VsyncValue = (int)vsync_Slider.value;
		QualitySettings.vSyncCount = PlayerSettings.Instance.VsyncValue;
		if (PlayerSettings.Instance.VsyncValue == 0)
		{
			vsync_Text.text = "No Vsync";
		}
		if (PlayerSettings.Instance.VsyncValue == 1)
		{
			vsync_Text.text = "Every V blank";
		}
		if (PlayerSettings.Instance.VsyncValue == 2)
		{
			vsync_Text.text = "Every second V blank";
		}
	}

	private void SetMaxFramerateOnGameOpens()
	{
		framerate_slider.SetValueWithoutNotify(PlayerSettings.Instance.MaxFramerate);
		framerate_text.text = LocalizationManager.Translate("txt-framerate") + ": " + PlayerSettings.Instance.MaxFramerate + " " + LocalizationManager.Translate("txt-fps");
	}

	private void SetUnitSizeOnGameOpens()
	{
		unitSize_Slider.SetValueWithoutNotify(PlayerSettings.Instance.UnitSize);
		unitsSize_text.text = LocalizationManager.Translate("txt-units size") + ": x" + Utils.Truncate(PlayerSettings.Instance.UnitSize, 2);
	}

	private void RefreshDifficultyValue()
	{
		difficulty_Text.text = PlayerSettings.Instance.GetLocalizedDifficulty(PlayerSettings.Instance.DifficultyLevel);
	}

	private void SetUIScaleOnGameOpens()
	{
		UIscale_slider.SetValueWithoutNotify(PlayerSettings.Instance.UIScale);
		UIscale_text.text = LocalizationManager.Translate("txt-UI scale") + ": " + Utils.Truncate(PlayerSettings.Instance.UIScale * 100f, 0) + "%";
	}

	private void SetVolumeOnGameOpens()
	{
		unitVolume_slider.SetValueWithoutNotify(PlayerSettings.Instance.UnitVolume);
		unitsVolume_text.text = LocalizationManager.Translate("txt-units volume setting") + ": " + Utils.Truncate(PlayerSettings.Instance.UnitVolume * 100f, 0) + "%";
		weatherVolume_slider.SetValueWithoutNotify(PlayerSettings.Instance.WeatherVolume);
		weatherVolume_text.text = LocalizationManager.Translate("txt-weather volume setting") + ": " + Utils.Truncate(PlayerSettings.Instance.WeatherVolume * 100f, 0) + "%";
		musicVolume_slider.SetValueWithoutNotify(PlayerSettings.Instance.MusicVolume);
		musicVolume_text.text = LocalizationManager.Translate("txt-music volume setting") + ": " + Utils.Truncate(PlayerSettings.Instance.MusicVolume * 100f, 0) + "%";
		UIVolume_slider.SetValueWithoutNotify(PlayerSettings.Instance.UIVolume);
		UIvolume_text.text = LocalizationManager.Translate("txt-UI volume setting") + ": " + Utils.Truncate(PlayerSettings.Instance.UIVolume * 100f, 0) + "%";
	}

	private void SetWinterDamageOnGameOpens()
	{
		winterDamage_Slider.SetValueWithoutNotify(PlayerSettings.Instance.WinterDamageValue);
		winterDamage_Text.text = LocalizationManager.Translate("txt-winter damage modifier") + ": x" + winterDamage_Slider.value;
	}

	public void ChangeWinterDamage()
	{
		PlayerSettings.Instance.WinterDamageValue = (int)winterDamage_Slider.value;
		winterDamage_Text.text = LocalizationManager.Translate("txt-winter damage modifier") + ": x" + PlayerSettings.Instance.WinterDamageValue;
	}

	public void SetCameraBackgroundColor()
	{
		PlayerSettings.Instance.CameraBackgroundColor = cameraBackgroundColor_inputfield.text;
	}

	private void SetRandomDamageModifierOnGameOpens()
	{
		randomDamageModifier_slider.SetValueWithoutNotify(PlayerSettings.Instance.RandomDamageModifier);
		randomDamageModifier_text.text = LocalizationManager.Translate("txt-random damage modifier") + ": " + PlayerSettings.Instance.RandomDamageModifier + "%";
	}

	public void SetRandomDamageModifier()
	{
		PlayerSettings.Instance.RandomDamageModifier = (int)randomDamageModifier_slider.value;
		randomDamageModifier_text.text = LocalizationManager.Translate("txt-random damage modifier") + ": " + PlayerSettings.Instance.RandomDamageModifier + "%";
	}

	private void SetAutosavesAmountOnGameOpens()
	{
		autosavesAmount_slider.SetValueWithoutNotify(PlayerSettings.Instance.AutosavesAmount);
		autosavesAmount_text.text = LocalizationManager.Translate("sldr-autosaves amount") + ": " + PlayerSettings.Instance.AutosavesAmount;
	}

	public void SetAutosavesAmount()
	{
		PlayerSettings.Instance.AutosavesAmount = (int)autosavesAmount_slider.value;
		autosavesAmount_text.text = LocalizationManager.Translate("sldr-autosaves amount") + ": " + PlayerSettings.Instance.AutosavesAmount;
	}

	private void SetAutosavesFrequencyOnGameOpens()
	{
		autosavesFrequency_slider.SetValueWithoutNotify(PlayerSettings.Instance.AutosavesFrequency);
		autosavesFrequency_text.text = LocalizationManager.Translate("sldr-autosaves frequency") + ": " + PlayerSettings.Instance.AutosavesFrequency;
	}

	public void SetAutosavesFrequency()
	{
		PlayerSettings.Instance.AutosavesFrequency = (int)autosavesFrequency_slider.value;
		autosavesFrequency_text.text = LocalizationManager.Translate("sldr-autosaves frequency") + ": " + PlayerSettings.Instance.AutosavesFrequency;
	}

	public void SetUnitSize()
	{
		PlayerSettings.Instance.UnitSize = unitSize_Slider.value;
		unitsSize_text.text = LocalizationManager.Translate("txt-units size") + ": x" + Utils.Truncate(PlayerSettings.Instance.UnitSize, 2);
	}

	public void SetInfiniteAmmoFuel()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.IsInfiniteAmmoAndFuel = !PlayerSettings.Instance.IsInfiniteAmmoAndFuel;
	}

	public void SetUnitSuffixOption()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.UnitSuffix = !PlayerSettings.Instance.UnitSuffix;
	}

	public void SetTechTreeOption()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.TechTree = !PlayerSettings.Instance.TechTree;
	}

	public void SetRealisticPlanesOption()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.RealisticPlanes = !PlayerSettings.Instance.RealisticPlanes;
	}

	public void SetPartisans()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.Partisans = !PlayerSettings.Instance.Partisans;
	}

	public void SetShowUnitsStats()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.HideUnitsStats = !PlayerSettings.Instance.HideUnitsStats;
	}

	public void SetProductionTakesTime()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ProductionTakesTime = !PlayerSettings.Instance.ProductionTakesTime;
	}

	public void SetMoveCameraWithLeftClick()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.MoveCameraWithLeftClick = !PlayerSettings.Instance.MoveCameraWithLeftClick;
	}

	public void SetShowBorders()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ShowBorders = !PlayerSettings.Instance.ShowBorders;
	}

	public void SetFastAI()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.FastAI = !PlayerSettings.Instance.FastAI;
		if (PlayerSettings.Instance.FastAI)
		{
			PlayerSettings.Instance.IsAISlowMode = false;
			slowAIMode_Toggle.SetIsOnWithoutNotify(value: false);
			PlayerSettings.Instance.FastAllies = false;
			fastAIAllyOnly_toggle.SetIsOnWithoutNotify(value: false);
		}
	}

	public void SetOneMoreTilePlanes()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.OneMoreTilePlanes = !PlayerSettings.Instance.OneMoreTilePlanes;
	}

	public void SetPolicies()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.Policies = !PlayerSettings.Instance.Policies;
	}

	public void SetManpower()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.Manpower = !PlayerSettings.Instance.Manpower;
	}

	public void SetFiniteManpower()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.FiniteManpower = !PlayerSettings.Instance.FiniteManpower;
	}

	public void SetColorBlindMode()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ColorBlindMode = !PlayerSettings.Instance.ColorBlindMode;
	}

	public void SetConnectPublicChat()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.PublicChatOFF = !PlayerSettings.Instance.PublicChatOFF;
	}

	public void SetFastAllies()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.FastAllies = !PlayerSettings.Instance.FastAllies;
		if (PlayerSettings.Instance.FastAllies)
		{
			PlayerSettings.Instance.FastAI = false;
			fastAI_toggle.SetIsOnWithoutNotify(value: false);
			PlayerSettings.Instance.IsAISlowMode = false;
			slowAIMode_Toggle.SetIsOnWithoutNotify(value: false);
		}
	}

	public void SetHideBattlePreview()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.HideBattlePreview = !PlayerSettings.Instance.HideBattlePreview;
	}

	public void SetUnitsAlwaysBlink()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.UnitsAlwaysBlink = !PlayerSettings.Instance.UnitsAlwaysBlink;
	}

	public void SetInvertedMouseScroll()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.InvertedMouseSroll = !PlayerSettings.Instance.InvertedMouseSroll;
	}

	public void SetShowUnitNumberOnCounters()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ShowUnitsNumbersOnCounters = !PlayerSettings.Instance.ShowUnitsNumbersOnCounters;
	}

	public void SetDisableVanillaSoundtracks()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.DisableVanillaSoundtracks = !PlayerSettings.Instance.DisableVanillaSoundtracks;
	}

	public void SetHideVisualCueForCoreUnits()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.HideVisualCueCoreUnits = !PlayerSettings.Instance.HideVisualCueCoreUnits;
	}

	public void SetInstantWinter()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.InstantSeasonChange = !PlayerSettings.Instance.InstantSeasonChange;
	}

	public void SetZoneOfControl()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ZoneOfControl = !PlayerSettings.Instance.ZoneOfControl;
	}

	public void SetUpkeep()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.Upkeep = !PlayerSettings.Instance.Upkeep;
	}

	public void SetShowUnitMovePath()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ShowUnitMovePath = !PlayerSettings.Instance.ShowUnitMovePath;
	}

	public void SetPrototypesDisabled()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.PrototypesDisabled = !PlayerSettings.Instance.PrototypesDisabled;
	}

	public void SetNoAASupportDuringAirFight()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.NoAAsupportDuringAirFight = !PlayerSettings.Instance.NoAAsupportDuringAirFight;
	}

	public void SetNATOiconsByDefault()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.UseNATObyDefault = !PlayerSettings.Instance.UseNATObyDefault;
	}

	public void SetStricterFogOfWar()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.StricterFogOfWar = !PlayerSettings.Instance.StricterFogOfWar;
	}

	public void SetHideEnemyHP()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.HideEnemyHP = !PlayerSettings.Instance.HideEnemyHP;
	}

	public void SetSkipEndTurnSummary()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.SkipEndTurnSummary = !PlayerSettings.Instance.SkipEndTurnSummary;
	}

	public void SetShowShipNames()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ShowShipNames = !PlayerSettings.Instance.ShowShipNames;
	}

	public void SetMovementInertia()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.IsMovementInertia = !PlayerSettings.Instance.IsMovementInertia;
	}

	public void SetBattleAnimations()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.BattleAnimations = !PlayerSettings.Instance.BattleAnimations;
	}

	public void SetDisplayUnitsLevelOnCounter()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.DisplayUnitsLevelOnCounter = !PlayerSettings.Instance.DisplayUnitsLevelOnCounter;
	}

	public void SetDamageReductionRepeatedAttacks()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.DamageReductionRepeatedAttacks = !PlayerSettings.Instance.DamageReductionRepeatedAttacks;
	}

	public void SetCensoredGermany()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.CensoredGermany = !PlayerSettings.Instance.CensoredGermany;
	}

	public void SetBlinkInactiveUnits()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.BlinkInactiveUnits = !PlayerSettings.Instance.BlinkInactiveUnits;
	}

	public void SetOverrun()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.Overrun = !PlayerSettings.Instance.Overrun;
	}

	public void SetUnitIsDoneIndicator()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.UnitIsDoneIndicator = !PlayerSettings.Instance.UnitIsDoneIndicator;
	}

	public void SetQuick2RespondOption()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.Quick2RespondOption = !PlayerSettings.Instance.Quick2RespondOption;
	}

	public void ShowClouds()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ShowClouds = !PlayerSettings.Instance.ShowClouds;
	}

	public void MoreContrast()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.MoreContrastSelectedUnit = !PlayerSettings.Instance.MoreContrastSelectedUnit;
	}

	public void AutomaticallyHidePopups()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.AutomaticallyHidePopups = !PlayerSettings.Instance.AutomaticallyHidePopups;
	}

	public void ManageFullscreen()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.Fullscreen = !PlayerSettings.Instance.Fullscreen;
		if (PlayerSettings.Instance.Fullscreen)
		{
			Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
		}
		else
		{
			Screen.fullScreenMode = FullScreenMode.Windowed;
		}
	}

	public void SetQuickMovement()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.IsQuickMovement = !PlayerSettings.Instance.IsQuickMovement;
	}

	public void SetHistoricalFocuses()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ISHistoricalFocus = !PlayerSettings.Instance.ISHistoricalFocus;
	}

	public void SetLanguage()
	{
		SoundManager.instance.PlayUIClickSound();
		GameLanguages value = (GameLanguages)language_dropdown.value;
		PlayerSettings.Instance.SetLanguage(value);
	}

	public void SetResolution()
	{
		SoundManager.instance.PlayUIClickSound();
		string text = "";
		string text2 = "";
		string text3 = resolution_dropdown.options[resolution_dropdown.value].text;
		string text4 = text3;
		for (int i = 0; i < text4.Length; i++)
		{
			char c = text4[i];
			text += c;
			if (c == ' ')
			{
				break;
			}
		}
		int num = 0;
		text4 = text3;
		for (int i = 0; i < text4.Length; i++)
		{
			char c2 = text4[i];
			if (c2 == ' ')
			{
				num++;
			}
			if (num == 2)
			{
				text2 += c2;
			}
		}
		PlayerSettings.Instance.Resolution = new int[2, 2];
		PlayerSettings.Instance.Resolution[0, 0] = int.Parse(text);
		PlayerSettings.Instance.Resolution[1, 0] = int.Parse(text2);
		if (PlayerSettings.Instance.Fullscreen)
		{
			Screen.SetResolution(int.Parse(text), int.Parse(text2), FullScreenMode.ExclusiveFullScreen);
		}
		else
		{
			Screen.SetResolution(int.Parse(text), int.Parse(text2), FullScreenMode.Windowed);
		}
	}

	public void SetAISlowMode()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.IsAISlowMode = !PlayerSettings.Instance.IsAISlowMode;
		if (PlayerSettings.Instance.IsAISlowMode)
		{
			PlayerSettings.Instance.FastAI = false;
			fastAI_toggle.SetIsOnWithoutNotify(value: false);
			PlayerSettings.Instance.FastAllies = false;
			fastAIAllyOnly_toggle.SetIsOnWithoutNotify(value: false);
		}
	}

	public void SetComplexSupplies()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ComplexSupplies = !PlayerSettings.Instance.ComplexSupplies;
	}

	public void SetFogOfWar()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.IsFogOfWar = !PlayerSettings.Instance.IsFogOfWar;
	}

	public void SetShowInfoTile()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ShowInfoTile = !PlayerSettings.Instance.ShowInfoTile;
	}

	public void SetMoveCameraWithMouseOnEdge()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.MoveCameraWithMouseOnEdge = !PlayerSettings.Instance.MoveCameraWithMouseOnEdge;
	}

	public void SetShowTooltips()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.ShowTooltips = !PlayerSettings.Instance.ShowTooltips;
	}

	public void SetMoveCameraToMousePosWhenZooming()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.MoveCameraToMousePosWhenZooming = !PlayerSettings.Instance.MoveCameraToMousePosWhenZooming;
	}

	public void SetCameraMovementTracking()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.FollowAIMoves = !PlayerSettings.Instance.FollowAIMoves;
	}

	public void SetWeather()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.IsWeather = !PlayerSettings.Instance.IsWeather;
	}

	public void SetWeatherParticles()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.IsWeatherParticles = !PlayerSettings.Instance.IsWeatherParticles;
	}

	public void SetMorale()
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.IsMorale = !PlayerSettings.Instance.IsMorale;
	}

	public void SetHarderDifficulty()
	{
		SoundManager.instance.PlayUIClickSound();
		if (PlayerSettings.Instance.DifficultyLevel + 1 <= 4)
		{
			PlayerSettings.Instance.DifficultyLevel++;
		}
		RefreshDifficultyValue();
	}

	public void SetEasierDifficulty()
	{
		SoundManager.instance.PlayUIClickSound();
		if (PlayerSettings.Instance.DifficultyLevel - 1 >= 1)
		{
			PlayerSettings.Instance.DifficultyLevel--;
		}
		RefreshDifficultyValue();
	}

	public void SetUIScale()
	{
		PlayerSettings.Instance.UIScale = UIscale_slider.value;
		UIscale_text.text = LocalizationManager.Translate("txt-UI scale") + ": " + Utils.Truncate(PlayerSettings.Instance.UIScale * 100f, 0) + "%";
		UnityEngine.Object.FindObjectOfType<CanvasRescaler>().RescaleCanvases();
	}

	public void ChangeMusicVolume()
	{
		PlayerSettings.Instance.MusicVolume = musicVolume_slider.value;
		musicVolume_text.text = LocalizationManager.Translate("txt-music volume setting") + ": " + Utils.Truncate(PlayerSettings.Instance.MusicVolume * 100f, 0) + "%";
		if ((bool)SoundManager.instance.music_Source)
		{
			SoundManager.instance.music_Source.volume = PlayerSettings.Instance.MusicVolume;
		}
	}

	public void ChangeUnitVolume()
	{
		PlayerSettings.Instance.UnitVolume = unitVolume_slider.value;
		unitsVolume_text.text = LocalizationManager.Translate("txt-units volume setting") + ": " + Utils.Truncate(PlayerSettings.Instance.UnitVolume * 100f, 0) + "%";
		if ((bool)SoundManager.instance.unit_Source)
		{
			SoundManager.instance.unit_Source.volume = PlayerSettings.Instance.UnitVolume;
		}
	}

	public void ChangeUIVolume_Slider()
	{
		PlayerSettings.Instance.UIVolume = UIVolume_slider.value;
		UIvolume_text.text = LocalizationManager.Translate("txt-UI volume setting") + ": " + Utils.Truncate(PlayerSettings.Instance.UIVolume * 100f, 0) + "%";
		if ((bool)SoundManager.instance.UI_Source)
		{
			SoundManager.instance.UI_Source.volume = PlayerSettings.Instance.UIVolume;
		}
	}

	public void ChangeWeatherVolume()
	{
		PlayerSettings.Instance.WeatherVolume = weatherVolume_slider.value;
		weatherVolume_text.text = LocalizationManager.Translate("txt-weather volume setting") + ": " + Utils.Truncate(PlayerSettings.Instance.WeatherVolume * 100f, 0) + "%";
		if ((bool)SoundManager.instance.weather_Source)
		{
			SoundManager.instance.weather_Source.volume = PlayerSettings.Instance.WeatherVolume;
		}
	}

	public void SetFramerate()
	{
		PlayerSettings.Instance.MaxFramerate = (int)framerate_slider.value;
		Application.targetFrameRate = PlayerSettings.Instance.MaxFramerate;
		framerate_text.text = LocalizationManager.Translate("txt-framerate") + ": " + PlayerSettings.Instance.MaxFramerate + " " + LocalizationManager.Translate("txt-fps");
	}

	public void ChangeMouseZoomSensivity()
	{
		PlayerSettings.Instance.ZoomFactor = zoom_Slider.value;
		zoomSpeed_text.text = LocalizationManager.Translate("txt-zoom speed") + ": x" + Utils.Truncate(PlayerSettings.Instance.ZoomFactor, 1);
	}

	public void OpenCustomMusicFolder()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start(Application.persistentDataPath + "/Custom musics");
	}

	public void OpenCustomUnitSounds()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start(Application.persistentDataPath + "/Custom unit sounds");
	}

	private void SetKeyBindingsOnGameOpens()
	{
		foreach (Button item in keysButtonsElement)
		{
			PropertyInfo[] properties = PlayerSettings.Instance.GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (item != null && item.gameObject.name == propertyInfo.Name)
				{
					item.GetComponentInChildren<TextMeshProUGUI>().text = propertyInfo.GetValue(PlayerSettings.Instance).ToString();
				}
			}
		}
	}

	public void SetNewKey()
	{
		SoundManager.instance.PlayUIClickSound();
		listeningForNewKeybind = true;
		keyToChangeName = EventSystem.current.currentSelectedGameObject.name;
		PropertyInfo[] properties = PlayerSettings.Instance.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.Name == keyToChangeName)
			{
				propertyInfo.SetValue(PlayerSettings.Instance, null);
				SetKeyBindingsOnGameOpens();
				break;
			}
		}
	}

	private void ListenForNewKey()
	{
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			if (!Input.GetKey(value))
			{
				continue;
			}
			SoundManager.instance.PlayUIClickSound();
			listeningForNewKeybind = false;
			PropertyInfo[] properties = PlayerSettings.Instance.GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.Name == keyToChangeName)
				{
					if (value == KeyCode.Escape)
					{
						propertyInfo.SetValue(PlayerSettings.Instance, null);
					}
					else
					{
						propertyInfo.SetValue(PlayerSettings.Instance, value);
					}
					SetKeyBindingsOnGameOpens();
					return;
				}
			}
		}
	}
}

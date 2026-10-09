using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapEditorMenu : MonoBehaviour
{
	public static string mapName;

	public static bool isLoadedMapEditor;

	public static int sizeX;

	public static int sizeY;

	public Transform container;

	[SerializeField]
	private Button newMap_button;

	[SerializeField]
	private Button modify_button;

	[SerializeField]
	private Button delete_Button;

	[SerializeField]
	private TMP_InputField mapName_inputfield;

	[SerializeField]
	private TMP_InputField inputX;

	[SerializeField]
	private TMP_InputField inputY;

	[SerializeField]
	private TMP_InputField searchInputfield;

	[SerializeField]
	private TextMeshProUGUI selectedMap_name;

	[SerializeField]
	private SimpleSlotUI mapSlot_Prefab;

	private int fileCount;

	private float lastClick;

	private string _mapsPath;

	private void Awake()
	{
		_mapsPath = Application.persistentDataPath + "/Maps";
	}

	private void Start()
	{
		mapName = null;
		fileCount = 0;
		UIManager.isUIOpen = false;
	}

	private void OnEnable()
	{
		mapName = null;
		MainMenu.selectedMap = null;
		StartCoroutine(CR_CheckFiles());
	}

	private IEnumerator CR_CheckFiles()
	{
		while (true)
		{
			if (new DirectoryInfo(_mapsPath).GetFiles().Length != fileCount)
			{
				RefreshListOfMaps();
				fileCount = new DirectoryInfo(_mapsPath).GetFiles().Length;
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void Update()
	{
		newMap_button.interactable = !string.IsNullOrEmpty(mapName_inputfield.text) && !string.IsNullOrEmpty(inputX.text) && !string.IsNullOrEmpty(inputY.text) && inputY.text != "0" && !inputX.text.Contains("-") && !inputY.text.Contains("-");
		modify_button.interactable = MainMenu.selectedMap != null;
		delete_Button.interactable = MainMenu.selectedMap != null;
	}

	public void BrowseMaps()
	{
		SoundManager.instance.PlayUIClickSound();
		Process.Start(_mapsPath);
	}

	public void RefreshListOfMaps()
	{
		selectedMap_name.text = "";
		MainMenu.GetAllCustomUserMadeMaps(p_includeCampaigns: true);
		SimpleSlotUI[] componentsInChildren = container.GetComponentsInChildren<SimpleSlotUI>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
		Dictionary<FileInfo, string> dictionary = new Dictionary<FileInfo, string>();
		foreach (FileInfo item in MainMenu.listOfAllMapsQuickGame)
		{
			if (!(item.Extension != ".sav"))
			{
				dictionary.Add(item, LocalizationManager.GetLocalizedScenario(item));
			}
		}
		foreach (KeyValuePair<FileInfo, string> files in dictionary)
		{
			if (string.IsNullOrEmpty(searchInputfield.text) || files.Value.ToLower().Contains(searchInputfield.text.ToLower()))
			{
				SimpleSlotUI simpleSlotUI = Object.Instantiate(mapSlot_Prefab, container);
				simpleSlotUI.name = files.Key.Name;
				simpleSlotUI.textComponent.text = Path.GetFileNameWithoutExtension(files.Value);
				simpleSlotUI.textComponent.text = simpleSlotUI.textComponent.text.Replace(".", string.Empty);
				simpleSlotUI.textComponent.text = simpleSlotUI.textComponent.text.Replace("_", string.Empty);
				simpleSlotUI.button.onClick.AddListener(() =>
				{
					MapChoice(files.Key);
				});
				if (!files.Key.FullName.Contains("Maps") && !files.Key.FullName.Contains("Standalone Scenarios"))
				{
					TextMeshProUGUI textComponent = simpleSlotUI.textComponent;
					textComponent.text = textComponent.text + " - " + LocalizationManager.GetLocalizedCampaignSide(new DirectoryInfo(Directory.GetParent(Directory.GetParent(Directory.GetParent(files.Key.FullName).ToString()).ToString()).ToString()).Name);
				}
			}
		}
	}

	public void DeleteMap()
	{
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-map deletion confirmation"), ConfirmDeleteMap);
	}

	public void ConfirmDeleteMap()
	{
		SoundManager.instance.PlayUIClickSound();
		ModsManager.moddedMaps.Remove(MainMenu.selectedMap);
		MainMenu.listOfAllCampaigns.Remove(MainMenu.selectedMap);
		MainMenu.listOfAllMapsQuickGame.Remove(MainMenu.selectedMap);
		MainMenu.selectedMap.Delete();
		RefreshListOfMaps();
		mapName = null;
		MainMenu.selectedMap = null;
		selectedMap_name.text = "";
	}

	private void MapChoice(FileInfo p_fileInfo)
	{
		SoundManager.instance.PlayUIClickSound();
		MainMenu.selectedMap = p_fileInfo;
		mapName = Path.GetFileNameWithoutExtension(p_fileInfo.FullName);
		selectedMap_name.text = LocalizationManager.Translate("txt-selected map") + LocalizationManager.GetLocalizedScenario(p_fileInfo);
		if (Time.time - lastClick < 0.15f)
		{
			ModifyMap();
		}
		else
		{
			lastClick = Time.time;
		}
	}

	public void Back()
	{
		SoundManager.instance.PlayUIClickSound();
		inputX.text = "";
		inputY.text = "";
		mapName_inputfield.text = "";
		mapName = null;
		MainMenu.selectedMap = null;
		gameObject.SetActive(value: false);
	}

	public void NewMap()
	{
		SoundManager.instance.PlayUIClickSound();
		isLoadedMapEditor = false;
		mapName = mapName_inputfield.text;
		MainMenu.instance.loading_Panel.SetActive(value: true);
		sizeX = int.Parse(inputX.text);
		sizeY = int.Parse(inputY.text);
		GameData.ResetForScenarioCreation(new List<Player>
		{
			new Player("Player1"),
			new Player("Player2")
		});
		GameData.Instance.map = new Map(sizeX, sizeY);
		MainMenu.isEditor = true;
		SceneManager.LoadScene("MapEditor");
	}

	public void ModifyMap()
	{
		SoundManager.instance.PlayUIClickSound();
		if (MainMenu.selectedMap != null)
		{
			MainMenu.isEditor = true;
			MainMenu.instance.loading_Panel.SetActive(value: true);
			SceneManager.LoadScene("MapEditor");
			isLoadedMapEditor = true;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadMenu : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Transform parent;

	[SerializeField]
	private GameObject loadButton;

	[SerializeField]
	private GameObject renameButton;

	[SerializeField]
	private GameObject deleteButton;

	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private GameObject saveSlot_Prefab;

	[SerializeField]
	private GameObject saveSlotCloud_Prefab;

	private FileInfo _selectedFileInfo;

	private string _savesPath;

	private float _lastClickTime;

	private byte[] _filesCheckSum;

	private void Awake()
	{
		loadButton.SetActive(value: false);
		renameButton.SetActive(value: false);
		deleteButton.SetActive(value: false);
		inputField.gameObject.SetActive(value: false);
		_savesPath = Application.persistentDataPath + "/Saves";
	}

	private void OnEnable()
	{
		_selectedFileInfo = null;
		StartCoroutine(CR_PollSaves());
	}

	private void Update()
	{
		if (Input.GetMouseButton(1))
		{
			_selectedFileInfo = null;
			loadButton.SetActive(value: false);
			renameButton.SetActive(value: false);
			deleteButton.SetActive(value: false);
			inputField.gameObject.SetActive(value: false);
		}
		if (Input.GetKeyDown(KeyCode.Delete) && _selectedFileInfo != null)
		{
			DeleteSave();
		}
	}

	public void CloseLoadPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(value: false);
	}

	public void Browse()
	{
		Process.Start(_savesPath);
	}

	private void RefreshSaveList()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != parent)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		List<FileInfo> list = new List<FileInfo>();
		string[] files = Directory.GetFiles(_savesPath, "*", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			FileInfo fileInfo = new FileInfo(files[i]);
			if (fileInfo.Extension == ".sav")
			{
				list.Add(fileInfo);
			}
		}
		list = (from x in list
			orderby x.Directory.Name == "Saves" descending, x.Directory.Name
			select x).ThenByDescending((FileInfo f) => f.LastWriteTime).ToList();
		List<string> steamCloudSaves = SteamManager.instance.GetSteamCloudSaves();
		string text = null;
		foreach (FileInfo fileInfo2 in list)
		{
			if (fileInfo2.Directory.Name != "Saves" && fileInfo2.Directory.Name != text)
			{
				GameObject gameObject = Object.Instantiate(saveSlot_Prefab, parent);
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = fileInfo2.Directory.Name;
				gameObject.GetComponentInChildren<TextMeshProUGUI>().color = MainMenu.yellowColor;
				gameObject.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.UpperCase;
				gameObject.GetComponentInChildren<TextMeshProUGUI>().fontSizeMax = 40f;
				gameObject.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Midline;
				gameObject.transform.localScale = Vector3.one;
				Object.Destroy(gameObject.GetComponentInChildren<Button>());
				text = fileInfo2.Directory.Name;
			}
			GameObject gameObject2 = ((!(fileInfo2.Directory.Name == "Steam Cloud") || !steamCloudSaves.Contains(Utils.GetFileNameWithoutExtension(fileInfo2))) ? Object.Instantiate(saveSlot_Prefab, parent) : Object.Instantiate(saveSlotCloud_Prefab, parent));
			gameObject2.GetComponentInChildren<TextMeshProUGUI>().text = Utils.GetFileNameWithoutExtension(fileInfo2);
			gameObject2.GetComponentInChildren<Button>().onClick.AddListener(() =>
			{
				SetSaveChoice(fileInfo2);
			});
			gameObject2.transform.localScale = Vector3.one;
		}
		if (_selectedFileInfo == null)
		{
			loadButton.SetActive(value: false);
			renameButton.SetActive(value: false);
			deleteButton.SetActive(value: false);
			inputField.gameObject.SetActive(value: false);
			inputField.text = string.Empty;
		}
	}

	public void OpenLoadMenu()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(value: true);
		RefreshSaveList();
	}

	public void SetSaveChoice(FileInfo p_fileInfo)
	{
		SoundManager.instance.PlayUIClickSound();
		_selectedFileInfo = p_fileInfo;
		MainMenu.selectedMap = p_fileInfo;
		inputField.text = Utils.GetFileNameWithoutExtension(p_fileInfo);
		loadButton.SetActive(value: true);
		renameButton.SetActive(value: true);
		deleteButton.SetActive(value: true);
		inputField.gameObject.SetActive(value: true);
		if (Time.unscaledTime - _lastClickTime < 0.15f)
		{
			Load();
		}
		else
		{
			_lastClickTime = Time.unscaledTime;
		}
	}

	public void Load()
	{
		SoundManager.instance.PlayUIClickSound();
		if (File.Exists(_selectedFileInfo.FullName))
		{
			MainMenu.instance.loading_Panel.SetActive(value: true);
			GameData.Instance.Load(_selectedFileInfo.FullName);
			GameData.Instance.SetIronManMode(GameData.Instance.IsIronManModeON(out var _));
			MainMenu.isLoadedGame = true;
			SceneManager.LoadScene("Game");
			UIManager.isUIOpen = false;
		}
	}

	public void DeleteSave()
	{
		SoundManager.instance.PlayUIClickSound();
		UIManager.ShowConfirmationWindow(LocalizationManager.Translate("txt-file deletion confirmation"), ConfirmSaveDeletion);
	}

	private void ConfirmSaveDeletion()
	{
		if (_selectedFileInfo != null)
		{
			File.Delete(_selectedFileInfo.FullName);
			if (_selectedFileInfo.Directory.Name == "Steam Cloud")
			{
				SteamManager.instance.DeleteSteamCloudFile(Utils.GetFileNameWithoutExtension(_selectedFileInfo));
			}
			RefreshSaveList();
			inputField.text = string.Empty;
			_selectedFileInfo = null;
		}
	}

	public void RenameSave()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!string.IsNullOrWhiteSpace(inputField.text))
		{
			if (_selectedFileInfo.Directory.Name == "Steam Cloud")
			{
				SteamManager.instance.RenameSteamCloudFile(Utils.GetFileNameWithoutExtension(_selectedFileInfo), inputField.text);
			}
			string destFileName = new DirectoryInfo(_selectedFileInfo.Directory.FullName).FullName + "/" + inputField.text + ".sav";
			File.Move(_selectedFileInfo.FullName, destFileName);
			RefreshSaveList();
			inputField.SetTextWithoutNotify(string.Empty);
		}
	}

	private IEnumerator CR_PollSaves()
	{
		_filesCheckSum = new byte[0];
		WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(0.3f);
		while (true)
		{
			byte[] array = Utils.ConvertObjectToByteArray(Directory.GetFiles(_savesPath, "*", SearchOption.AllDirectories));
			if (array.Length != _filesCheckSum.Length)
			{
				_filesCheckSum = array;
				RefreshSaveList();
			}
			yield return waitForSecondsRealtime;
		}
	}
}

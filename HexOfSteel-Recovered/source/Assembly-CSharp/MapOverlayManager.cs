using System.Collections;
using System.IO;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapOverlayManager : MonoBehaviour
{
	private const float DEFAULT_SCALE = 15f;

	public SpriteRenderer spriteRenderer;

	public string[] filePath;

	private bool _inputFieldSelected;

	private void Start()
	{
		if (!string.IsNullOrEmpty(GameData.Instance.map.OverlayPath))
		{
			filePath = new string[1];
			filePath[0] = GameData.Instance.map.OverlayPath;
			StartCoroutine(CR_LoadOverlay(p_isAutoLoad: true));
		}
	}

	private void Update()
	{
		_inputFieldSelected = EventSystem.current.currentSelectedGameObject != null && (EventSystem.current.currentSelectedGameObject.TryGetComponent<InputField>(out var _) || EventSystem.current.currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var _));
		if (_inputFieldSelected)
		{
			return;
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.ToggleOverlay))
		{
			spriteRenderer.enabled = !spriteRenderer.enabled;
		}
		if (Input.GetKey(PlayerSettings.Instance.ScaleOverlayUp))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * 1.05f, gameObject.transform.localScale.y * 1.05f, gameObject.transform.localScale.z);
			}
			else
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * 1.0001f, gameObject.transform.localScale.y * 1.0001f, gameObject.transform.localScale.z);
			}
			GameData.Instance.map.OverlayScale = gameObject.transform.localScale.x;
		}
		else if (Input.GetKey(PlayerSettings.Instance.ScaleOverlayDown))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * 0.95f, gameObject.transform.localScale.y * 0.95f, gameObject.transform.localScale.z);
			}
			else
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * 0.9999f, gameObject.transform.localScale.y * 0.9999f, gameObject.transform.localScale.z);
			}
			GameData.Instance.map.OverlayScale = gameObject.transform.localScale.x;
		}
		if (Input.GetKey(PlayerSettings.Instance.MoveOverlayUp))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 2.5f, gameObject.transform.position.z);
			}
			else
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 0.1f, gameObject.transform.position.z);
			}
			GameData.Instance.map.OverlayPosX = gameObject.transform.position.x;
			GameData.Instance.map.OverlayPosY = gameObject.transform.position.y;
		}
		if (Input.GetKey(PlayerSettings.Instance.MoveOverlayDown))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 2.5f, gameObject.transform.position.z);
			}
			else
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 0.1f, gameObject.transform.position.z);
			}
			GameData.Instance.map.OverlayPosX = gameObject.transform.position.x;
			GameData.Instance.map.OverlayPosY = gameObject.transform.position.y;
		}
		if (Input.GetKey(PlayerSettings.Instance.MoveOverlayLeft))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x - 2.5f, gameObject.transform.position.y, gameObject.transform.position.z);
			}
			else
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x - 0.1f, gameObject.transform.position.y, gameObject.transform.position.z);
			}
			GameData.Instance.map.OverlayPosX = gameObject.transform.position.x;
			GameData.Instance.map.OverlayPosY = gameObject.transform.position.y;
		}
		if (Input.GetKey(PlayerSettings.Instance.MoveOverlayRight))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x + 2.5f, gameObject.transform.position.y, gameObject.transform.position.z);
			}
			else
			{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x + 0.1f, gameObject.transform.position.y, gameObject.transform.position.z);
			}
			GameData.Instance.map.OverlayPosX = gameObject.transform.position.x;
			GameData.Instance.map.OverlayPosY = gameObject.transform.position.y;
		}
		if (Input.GetKeyDown(PlayerSettings.Instance.IncreaseOverlayOpacity) && spriteRenderer.color.a < 1f)
		{
			spriteRenderer.color = new Color(1f, 1f, 1f, spriteRenderer.color.a + 0.1f);
			GameData.Instance.map.OverlayOpacity = spriteRenderer.color.a;
		}
		else if (Input.GetKeyDown(PlayerSettings.Instance.DecreaseOverlayOpacity) && spriteRenderer.color.a > 0f)
		{
			spriteRenderer.color = new Color(1f, 1f, 1f, spriteRenderer.color.a - 0.1f);
			GameData.Instance.map.OverlayOpacity = spriteRenderer.color.a;
		}
	}

	public void OpenFileManager()
	{
		filePath = null;
		FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png", ".jpeg"));
		FileBrowser.SetDefaultFilter(".jpg");
		FileBrowser.ShowLoadDialog((string[] test) =>
		{
			Success(test);
		}, Cancel, FileBrowser.PickMode.Files, allowMultiSelection: false, Application.persistentDataPath + "/MODS/");
	}

	private void Success(string[] p_values)
	{
		filePath = p_values;
		if (p_values != null && p_values[0] != null)
		{
			StartCoroutine(CR_LoadOverlay(p_isAutoLoad: false));
		}
	}

	private void Cancel()
	{
	}

	private IEnumerator CR_LoadOverlay(bool p_isAutoLoad)
	{
		SoundManager.instance.PlayUIClickSound();
		if (p_isAutoLoad)
		{
			if (GameData.Instance.map.OverlayPath == null && filePath != null && filePath[0] != null)
			{
				GameData.Instance.map.OverlayPath = filePath[0];
			}
			if (GameData.Instance.map.OverlayPath.Contains("MODS") && GameData.Instance.map.OverlayPath.Contains("Overlays"))
			{
				foreach (Mod activeMod in ModsManager.activeMods)
				{
					if (!GameData.Instance.map.OverlayPath.Contains(activeMod.Name))
					{
						continue;
					}
					DirectoryInfo[] directories = new DirectoryInfo(activeMod.InstallLocation).GetDirectories();
					foreach (DirectoryInfo directoryInfo in directories)
					{
						if (!(directoryInfo.Name == "Overlays"))
						{
							continue;
						}
						FileInfo[] files = directoryInfo.GetFiles();
						foreach (FileInfo fileInfo in files)
						{
							if (GameData.Instance.map.OverlayPath.Contains(fileInfo.Name))
							{
								GameData.Instance.map.OverlayPath = fileInfo.FullName;
							}
						}
					}
				}
			}
		}
		else
		{
			if (string.IsNullOrEmpty(GameData.Instance.map.OverlayPath))
			{
				GameData.Instance.map.OverlayOpacity = 1f;
			}
			GameData.Instance.map.OverlayPath = filePath[0];
		}
		UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + filePath[0]);
		yield return request.SendWebRequest();
		try
		{
			spriteRenderer.sprite = Sprite.Create(DownloadHandlerTexture.GetContent(request), new Rect(0f, 0f, DownloadHandlerTexture.GetContent(request).width, DownloadHandlerTexture.GetContent(request).height), new Vector2(0f, 0f));
			if (GameData.Instance.map.OverlayPosX != gameObject.transform.position.x && GameData.Instance.map.OverlayPosY != gameObject.transform.position.y)
			{
				gameObject.transform.position = new Vector3(GameData.Instance.map.OverlayPosX, GameData.Instance.map.OverlayPosY, gameObject.transform.position.z);
			}
			if (GameData.Instance.map.OverlayScale == 0f)
			{
				GameData.Instance.map.OverlayScale = 15f;
			}
			gameObject.transform.localScale = Vector3.one * GameData.Instance.map.OverlayScale;
			spriteRenderer.color = new Color(1f, 1f, 1f, GameData.Instance.map.OverlayOpacity);
		}
		catch
		{
		}
	}
}

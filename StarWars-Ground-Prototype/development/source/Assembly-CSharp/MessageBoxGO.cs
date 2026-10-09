using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MessageBoxGO : MonoBehaviour
{
	public TextMeshProUGUI message_Text;

	private void Awake()
	{
		if (SceneManager.GetActiveScene().name == "Game")
		{
			gameObject.transform.SetParent(UIManager.instance.UI_parent);
		}
		else
		{
			gameObject.transform.SetParent(GameObject.Find("UI").transform);
		}
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);
	}

	public void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Object.Destroy(gameObject);
		}
	}

	public void CloseMessageBox()
	{
		Object.Destroy(gameObject);
	}
}

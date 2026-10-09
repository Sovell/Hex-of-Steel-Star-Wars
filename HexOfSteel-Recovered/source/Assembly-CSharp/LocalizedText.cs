using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
	[Header("Setup")]
	public bool updateFontOnAwake;

	public bool translateOnAwake;

	[Space]
	[Space]
	[Space]
	public string key;

	[Space]
	[Space]
	[Space]
	[Header("DEBUG / INFOS")]
	public bool hasTextComponentAssigned;

	public TextMeshProUGUI textMeshProUGUI;

	private void Start()
	{
		if (!TryGetComponent<TextMeshProUGUI>(out textMeshProUGUI))
		{
			Debug.LogError("No text component on this object : " + gameObject.name, gameObject);
			return;
		}
		hasTextComponentAssigned = true;
		if (updateFontOnAwake)
		{
			textMeshProUGUI.font = SceneTranslator.instance.GetActiveFontAsset();
		}
		if (translateOnAwake)
		{
			Translate();
		}
	}

	public void Translate()
	{
		if (key == null)
		{
			Debug.LogError("The key for this localized text is null : " + gameObject.name, gameObject);
			return;
		}
		if (!TryGetComponent<TextMeshProUGUI>(out textMeshProUGUI))
		{
			Debug.LogError("No text component on this object : " + gameObject.name, gameObject);
			return;
		}
		hasTextComponentAssigned = true;
		textMeshProUGUI.text = LocalizationManager.Translate(key);
	}

	public void Translate(TMP_FontAsset p_fontAsset)
	{
		if (key == null)
		{
			Debug.LogError("The key for this localized text is null : " + gameObject.name, gameObject);
			return;
		}
		if (!TryGetComponent<TextMeshProUGUI>(out textMeshProUGUI))
		{
			Debug.LogError("No text component on this object : " + gameObject.name, gameObject);
			return;
		}
		hasTextComponentAssigned = true;
		textMeshProUGUI.font = p_fontAsset;
		textMeshProUGUI.text = LocalizationManager.Translate(key);
	}
}

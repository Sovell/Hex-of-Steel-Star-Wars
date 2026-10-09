using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _languageSelectionMenu;

	[SerializeField]
	private Image _activeLocaleIcon_image;

	[SerializeField]
	private TextMeshProUGUI _translator_text;

	[SerializeField]
	private Transform _languages_container;

	[SerializeField]
	private LanguageSlotUI _languageOption_prefab;

	private void Start()
	{
		UpdateActiveLocaleInfos();
	}

	public void ManageLanguagesPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_languageSelectionMenu.activeSelf)
		{
			CloseLanguageMenu();
		}
		else
		{
			OpenLanguageMenu();
		}
	}

	private void UpdateActiveLocaleInfos()
	{
		_activeLocaleIcon_image.sprite = GetLocaleIcon(PlayerSettings.Instance.Language.ToString());
		_translator_text.text = LocalizationManager.GetActiveLocaleTranslator();
	}

	private void OpenLanguageMenu()
	{
		_languageSelectionMenu.SetActive(value: true);
		ClearSlots();
		GenerateSlots();
	}

	private void CloseLanguageMenu()
	{
		_languageSelectionMenu.SetActive(value: false);
	}

	private void ClearSlots()
	{
		LanguageSlotUI[] componentsInChildren = _languages_container.GetComponentsInChildren<LanguageSlotUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	private void GenerateSlots()
	{
		foreach (GameLanguages value in Enum.GetValues(typeof(GameLanguages)))
		{
			GenerateSlot(value.ToString());
		}
	}

	private void GenerateSlot(string p_ISOcode)
	{
		LanguageSlotUI languageSlotUI = UnityEngine.Object.Instantiate(_languageOption_prefab, _languages_container);
		languageSlotUI.name = p_ISOcode;
		languageSlotUI.image.sprite = GetLocaleIcon(p_ISOcode);
		languageSlotUI.button.onClick.AddListener(() =>
		{
			SelectLanguage(p_ISOcode);
		});
	}

	private void SelectLanguage(string p_languageISOcode)
	{
		SoundManager.instance.PlayUIClickSound();
		PlayerSettings.Instance.SetLanguage((GameLanguages)Enum.Parse(typeof(GameLanguages), p_languageISOcode));
		_translator_text.text = LocalizationManager.GetActiveLocaleTranslator();
		UpdateActiveLocaleInfos();
		CloseLanguageMenu();
	}

	private Sprite GetLocaleIcon(string p_ISOcode)
	{
		return Resources.Load<Sprite>("UI/Localization/Flags/" + p_ISOcode);
	}
}

using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class LanguageDropdown : MonoBehaviour
{
	private void Start()
	{
		GenerateLanguages();
	}

	private void GenerateLanguages()
	{
		TMP_Dropdown component = GetComponent<TMP_Dropdown>();
		component.options.Clear();
		foreach (GameLanguages value in Enum.GetValues(typeof(GameLanguages)))
		{
			component.options.Add(new TMP_Dropdown.OptionData
			{
				text = LocalizationManager.Translate("language." + value)
			});
		}
		component.SetValueWithoutNotify(Utils.GetIndexByName(component, LocalizationManager.Translate(PlayerSettings.Instance.ChatChannel)));
		component.RefreshShownValue();
	}
}

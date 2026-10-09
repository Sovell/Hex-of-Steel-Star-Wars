using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModSlotGO : MonoBehaviour
{
	public TextMeshProUGUI modName_text;

	public Button mod_button;

	public Button deleteMod_button;

	public Button openMod_button;

	public Button manageMod_button;

	public ModItem modItem;

	public void UpdateDisplayName()
	{
		modName_text.text = $"{modItem.name} - {modItem.installSize} MB";
	}
}

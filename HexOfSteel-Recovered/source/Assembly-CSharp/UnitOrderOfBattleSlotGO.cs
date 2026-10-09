using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitOrderOfBattleSlotGO : MonoBehaviour
{
	public Image backgroundForCoreUnit;

	public Image representation;

	public Image backgroundUnit;

	public Image coreUnitIcon;

	public Sprite squareCoreUnit_sprite;

	public Sprite roundCoreUnit_sprite;

	public TextMeshProUGUI unitName_text;

	public TextMeshProUGUI displayName_text;

	public TextMeshProUGUI unitXP_text;

	public TextMeshProUGUI unitRank_text;

	public TextMeshProUGUI unitHP_text;

	public TextMeshProUGUI heroName_text;

	public Toggle autoSupplies_toggle;

	public Button slot_button;

	public Button delete_button;

	public Button redeploy_button;

	public Button transfer_button;

	public Unit unit;

	public void ResetSlot()
	{
		autoSupplies_toggle.onValueChanged.RemoveAllListeners();
		autoSupplies_toggle.SetIsOnWithoutNotify(value: false);
		slot_button.onClick.RemoveAllListeners();
		delete_button.onClick.RemoveAllListeners();
		redeploy_button.onClick.RemoveAllListeners();
		transfer_button.onClick.RemoveAllListeners();
		redeploy_button.gameObject.SetActive(value: true);
		transfer_button.gameObject.SetActive(value: true);
	}

	public void SetUpSlot()
	{
		gameObject.name = unit.ID.ToString();
		unitName_text.text = unit.GetUnitName();
		displayName_text.text = unit.GetCounterDisplayName();
		unitXP_text.text = LocalizationManager.Translate("history-xp") + unit.XP;
		unitRank_text.text = LocalizationManager.Translate("history-rank") + unit.Lvl;
		unitHP_text.text = LocalizationManager.Translate("txt-health") + unit.CurrHP + "/" + unit.MaxHP;
		if (unit.Hero != null)
		{
			heroName_text.text = unit.Hero.Name;
		}
		else
		{
			heroName_text.text = string.Empty;
		}
		representation.sprite = unit.unitGO.unitSprite.sprite;
		backgroundUnit.sprite = unit.unitGO.backgroundSprite.sprite;
		coreUnitIcon.gameObject.SetActive(unit.IsCoreUnit);
		if (unit.IsCoreUnit)
		{
			coreUnitIcon.sprite = ((unit.Type == "Boat" || unit.Type == "Plane") ? roundCoreUnit_sprite : squareCoreUnit_sprite);
		}
		if (unit.AutoSupplies)
		{
			autoSupplies_toggle.SetIsOnWithoutNotify(value: true);
		}
		if (unit.MaxMP == 0)
		{
			redeploy_button.gameObject.SetActive(value: false);
		}
		if (unit.ActionPoints < 2)
		{
			transfer_button.gameObject.SetActive(value: false);
		}
		redeploy_button.interactable = unit.CanBeRedeployed();
		displayName_text.gameObject.SetActive(!string.IsNullOrEmpty(displayName_text.text));
	}
}

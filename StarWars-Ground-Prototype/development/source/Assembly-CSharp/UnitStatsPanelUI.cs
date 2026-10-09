using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsPanelUI : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Image _flagImage;

	[SerializeField]
	private TextMeshProUGUI _historyElement_prefab;

	[SerializeField]
	private TextMeshProUGUI _unitName_text;

	[SerializeField]
	private Transform _history_parent;

	[SerializeField]
	private TextMeshProUGUI _XP_text;

	[SerializeField]
	private TextMeshProUGUI _level_text;

	[SerializeField]
	private TextMeshProUGUI _infantryKills_text;

	[SerializeField]
	private TextMeshProUGUI _tankKills_text;

	[SerializeField]
	private TextMeshProUGUI _AAKills_text;

	[SerializeField]
	private TextMeshProUGUI _ATKills_text;

	[SerializeField]
	private TextMeshProUGUI _ArtilleryKills_text;

	[SerializeField]
	private TextMeshProUGUI _fighterKills_text;

	[SerializeField]
	private TextMeshProUGUI _lightBombersKills_text;

	[SerializeField]
	private TextMeshProUGUI _heavyBombersKills_text;

	[SerializeField]
	private TextMeshProUGUI _shipKills_text;

	[Header("Hero")]
	[SerializeField]
	private TextMeshProUGUI _heroName_text;

	[SerializeField]
	private Image _hero_image;

	public void Open(UnitGO p_unitGO)
	{
		gameObject.SetActive(value: true);
		Unit unit = p_unitGO.unit;
		_flagImage.sprite = Utils.GetFlagSprite(unit.OwnerPlayer.Country);
		_unitName_text.text = unit.Name;
		unit.GetLevelAndXPToNext(out var o_level, out var o_XPToNextLevel);
		int num = ((o_level >= Unit.MAX_LEVEL) ? unit.XP : (unit.XP + o_XPToNextLevel));
		_XP_text.text = LocalizationManager.Translate("history-xp") + unit.XP + ((o_XPToNextLevel > 0) ? ("/" + num) : string.Empty);
		_level_text.text = LocalizationManager.Translate("history-rank") + unit.Lvl + "/" + Unit.MAX_LEVEL;
		_infantryKills_text.text = unit.InfantryKill.ToString();
		_tankKills_text.text = unit.TankKill.ToString();
		_AAKills_text.text = unit.AAKill.ToString();
		_ArtilleryKills_text.text = unit.ArtilleryKill.ToString();
		_ATKills_text.text = unit.AntitankKill.ToString();
		_fighterKills_text.text = unit.FighterKill.ToString();
		_lightBombersKills_text.text = unit.CASKill.ToString();
		_heavyBombersKills_text.text = unit.HeavyBomberKill.ToString();
		_shipKills_text.text = unit.BoatKill.ToString();
		Transform[] componentsInChildren = _history_parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != _history_parent)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		foreach (string item in unit.StuffThatHappened)
		{
			TextMeshProUGUI textMeshProUGUI = Object.Instantiate(_historyElement_prefab, _history_parent);
			textMeshProUGUI.text = item;
			textMeshProUGUI.transform.localScale = new Vector3(1f, 1f, 1f);
			textMeshProUGUI.GetComponent<RectTransform>().ForceUpdateRectTransforms();
			Canvas.ForceUpdateCanvases();
		}
		_history_parent.GetComponent<RectTransform>().ForceUpdateRectTransforms();
		Canvas.ForceUpdateCanvases();
		if (unit.Hero != null)
		{
			_heroName_text.text = unit.Hero.Name;
			_hero_image.color = new Color(1f, 1f, 1f, 0.8f);
			Sprite sprite;
			if (unit.Type == "Plane")
			{
				sprite = Resources.Load<Sprite>("Heroes/" + unit.OwnerPlayer.Country + "/Air/" + unit.Hero.Name);
			}
			else
			{
				sprite = ((!(unit.FilterType == "FilterBoat")) ? Resources.Load<Sprite>("Heroes/" + unit.OwnerPlayer.Country + "/Ground/" + unit.Hero.Name) : Resources.Load<Sprite>("Heroes/" + unit.OwnerPlayer.Country + "/Boat/" + unit.Hero.Name));
			}
			if (!sprite)
			{
				sprite = Utils.GetModdedHeroSprite(unit.Hero);
			}
			if (!sprite)
			{
				sprite = Utils.GetHeroIconSprite(unit);
			}
			if (!sprite)
			{
				if (unit.Type == "Plane")
				{
					sprite = Resources.Load<Sprite>("Heroes/Generic/Air/Air generic");
				}
				else
				{
					sprite = ((!(unit.FilterType == "FilterBoat")) ? Resources.Load<Sprite>("Heroes/Generic/Ground/Ground generic") : Resources.Load<Sprite>("Heroes/Generic/Boat/Boat generic"));
				}
			}
			if (!sprite)
			{
				Debug.LogError("MISSING HERO IMAGE");
			}
			_hero_image.sprite = sprite;
		}
		else
		{
			_heroName_text.text = string.Empty;
			_hero_image.color = new Color(1f, 1f, 1f, 0f);
		}
		TutorialManager.PerformAction(TutorialActions.OPEN_UNIT_HISTORY);
	}
}

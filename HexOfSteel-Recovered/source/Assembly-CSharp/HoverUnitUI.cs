using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HoverUnitUI : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private TextMeshProUGUI _unitName_text;

	[SerializeField]
	private TextMeshProUGUI _HP_text;

	[SerializeField]
	private TextMeshProUGUI _armour_text;

	[SerializeField]
	private TextMeshProUGUI _softAttack_text;

	[SerializeField]
	private TextMeshProUGUI _hardAttack_text;

	[SerializeField]
	private TextMeshProUGUI _airAttack_text;

	[SerializeField]
	private TextMeshProUGUI _range_text;

	[SerializeField]
	private TextMeshProUGUI _entrenchment_text;

	[SerializeField]
	private TextMeshProUGUI _supplies_text;

	[SerializeField]
	private Image _rank_image;

	[SerializeField]
	private Image _unitTypeIcon_image;

	[SerializeField]
	private Image _flag_image;

	private Unit _prevUnit;

	public void Close()
	{
		gameObject.SetActive(value: false);
		_prevUnit = null;
	}

	public void Show(UnitGO p_unitGO)
	{
		Unit unit = p_unitGO.unit;
		float hPPercent = unit.GetHPPercent(p_forDamagePurposes: true);
		_unitName_text.text = unit.GetUnitName();
		if (PlayerSettings.Instance.HideEnemyHP && !MapGO.IsAIOnly && !unit.OwnerPlayer.isAlliedWithHumanPlayer)
		{
			_HP_text.text = "???/" + unit.MaxHP;
		}
		else
		{
			_HP_text.text = unit.CurrHP + "/" + unit.MaxHP;
		}
		if (unit.Type != "Plane")
		{
			_armour_text.text = unit.GetArmour().ToString();
		}
		else
		{
			_armour_text.text = unit.GetArmour().ToString();
		}
		if (unit.Hero == null)
		{
			_softAttack_text.text = Math.Round((float)unit.SoftDamage * hPPercent, MidpointRounding.AwayFromZero).ToString();
		}
		else
		{
			_softAttack_text.text = (Math.Round((float)unit.SoftDamage * hPPercent, MidpointRounding.AwayFromZero) + (double)unit.Hero.SoftDamageBonus).ToString();
		}
		if (unit.Hero == null)
		{
			_hardAttack_text.text = Math.Round((float)unit.HardDamage * hPPercent, MidpointRounding.AwayFromZero).ToString();
		}
		else
		{
			_hardAttack_text.text = (Math.Round((float)unit.HardDamage * hPPercent, MidpointRounding.AwayFromZero) + (double)unit.Hero.HardDamageBonus).ToString();
		}
		if (unit.Hero == null)
		{
			_airAttack_text.text = Math.Round((float)unit.AirDamage * hPPercent, MidpointRounding.AwayFromZero).ToString();
		}
		else
		{
			_airAttack_text.text = (Math.Round((float)unit.AirDamage * hPPercent, MidpointRounding.AwayFromZero) + (double)unit.Hero.AirDamageBonus).ToString();
		}
		_range_text.text = unit.Range.ToString();
		_entrenchment_text.text = unit.EntrenchmentLevel.ToString();
		_rank_image.sprite = Resources.Load<Sprite>("UI/Ranks/" + unit.Lvl);
		_flag_image.sprite = Utils.GetFlagSprite(unit.OwnerPlayer.Country);
		if (unit != _prevUnit)
		{
			p_unitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
		}
		_supplies_text.text = Utils.Truncate(unit.SupplyPercentage, 0) + "%";
		if (unit.BuiltByEngineers || unit.IsMine || (unit.MaxMP == 0 && unit.CoastalBattery))
		{
			if (unit.IsMine)
			{
				_unitTypeIcon_image.sprite = Resources.Load<Sprite>("Filter types/Mine");
			}
			else if (unit.FilterType == "FilterBuilding")
			{
				_unitTypeIcon_image.sprite = Resources.Load<Sprite>("Filter types/FilterBuilding");
			}
			else
			{
				_unitTypeIcon_image.sprite = Resources.Load<Sprite>("Filter types/FilterTank");
			}
		}
		else
		{
			_unitTypeIcon_image.sprite = Resources.Load<Sprite>("Filter types/" + unit.FilterType);
		}
		gameObject.SetActive(value: true);
		_prevUnit = unit;
	}
}

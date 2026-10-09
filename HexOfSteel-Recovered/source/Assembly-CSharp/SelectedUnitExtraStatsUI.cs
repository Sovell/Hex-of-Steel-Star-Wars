using System;
using TMPro;
using UnityEngine;

public class SelectedUnitExtraStatsUI : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private TextMeshProUGUI _softAttack_text;

	[SerializeField]
	private TextMeshProUGUI _hardAttack_text;

	[SerializeField]
	private TextMeshProUGUI _airAttack_text;

	[SerializeField]
	private TextMeshProUGUI _visibility_text;

	[SerializeField]
	private TextMeshProUGUI _range_text;

	[SerializeField]
	private TextMeshProUGUI _terrainModifier_text;

	private Unit _unit;

	private UnitGO _unitGO;

	public void Close()
	{
		_unit = null;
		_unitGO = null;
		gameObject.SetActive(value: false);
	}

	public void Show(UnitGO p_unitGO)
	{
		_unit = p_unitGO.unit;
		_unitGO = p_unitGO;
		RefreshInfos();
	}

	public void RefreshIfIsSelectedUnit(Unit p_unit)
	{
		if (IsUnitShown(p_unit))
		{
			RefreshInfos();
		}
	}

	public bool IsUnitShown(Unit p_unit)
	{
		if (_unit == null)
		{
			return false;
		}
		return _unit.ID == p_unit.ID;
	}

	public void RefreshInfos()
	{
		float hPPercent = _unit.GetHPPercent(p_forDamagePurposes: true);
		if (_unit.Hero == null)
		{
			_softAttack_text.text = Math.Round((float)_unit.SoftDamage * hPPercent, MidpointRounding.AwayFromZero).ToString();
		}
		else
		{
			_softAttack_text.text = (Math.Round((float)_unit.SoftDamage * hPPercent, MidpointRounding.AwayFromZero) + (double)_unit.Hero.SoftDamageBonus).ToString();
		}
		if (_unit.Hero == null)
		{
			_hardAttack_text.text = Math.Round((float)_unit.HardDamage * hPPercent, MidpointRounding.AwayFromZero).ToString();
		}
		else
		{
			_hardAttack_text.text = (Math.Round((float)_unit.HardDamage * hPPercent, MidpointRounding.AwayFromZero) + (double)_unit.Hero.HardDamageBonus).ToString();
		}
		if (_unit.Hero == null)
		{
			_airAttack_text.text = Math.Round((float)_unit.AirDamage * hPPercent, MidpointRounding.AwayFromZero).ToString();
		}
		else
		{
			_airAttack_text.text = (Math.Round((float)_unit.AirDamage * hPPercent, MidpointRounding.AwayFromZero) + (double)_unit.Hero.AirDamageBonus).ToString();
		}
		_visibility_text.text = _unit.Visibility.ToString();
		_range_text.text = _unit.Range.ToString();
		if (_unit.Type != "Plane" && _unitGO != null)
		{
			_terrainModifier_text.text = _unitGO.tileGO.tile.DamageModificator + "%";
		}
		else
		{
			_terrainModifier_text.text = "0%";
		}
	}
}

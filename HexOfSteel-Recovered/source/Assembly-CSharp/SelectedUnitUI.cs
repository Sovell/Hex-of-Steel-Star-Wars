using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnitUI : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private TMP_InputField _unitName_inputField;

	[SerializeField]
	private TextMeshProUGUI _HP_text;

	[SerializeField]
	private TextMeshProUGUI _armour_text;

	[SerializeField]
	private TextMeshProUGUI _MP_text;

	[SerializeField]
	private TextMeshProUGUI _fuel_text;

	[SerializeField]
	private TextMeshProUGUI _entrenchment_text;

	[SerializeField]
	private TextMeshProUGUI _ammo_text;

	[SerializeField]
	private TextMeshProUGUI _supplies_text;

	[SerializeField]
	private Image _rank_image;

	[SerializeField]
	private Image _unitTypeIcon_image;

	[SerializeField]
	private Image _flag_image;

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
		_unitGO = p_unitGO;
		_unit = p_unitGO.unit;
		RefreshInfos();
		gameObject.SetActive(value: true);
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
		_unitName_inputField.SetTextWithoutNotify(_unit.GetUnitName());
		_HP_text.text = _unit.CurrHP + "/" + _unit.MaxHP;
		if (_unit.Type != "Plane")
		{
			_armour_text.text = _unit.GetArmour().ToString();
		}
		else
		{
			_armour_text.text = _unit.GetArmour().ToString();
		}
		_fuel_text.text = _unit.CurrAutonomy + "/" + _unit.MaxAutonomy;
		_MP_text.text = _unit.CurrMP + "/" + _unit.MaxMP;
		_entrenchment_text.text = _unit.EntrenchmentLevel.ToString();
		_ammo_text.text = _unit.CurrAmmo + "/" + _unit.MaxAmmo;
		_unitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
		_supplies_text.text = Utils.Truncate(_unit.SupplyPercentage, 0) + "%";
		if (_unit.BuiltByEngineers || _unit.IsMine || (_unit.MaxMP == 0 && _unit.CoastalBattery))
		{
			if (_unit.IsMine)
			{
				_unitTypeIcon_image.sprite = Resources.Load<Sprite>("Filter types/Mine");
			}
			else
			{
				_unitTypeIcon_image.sprite = Resources.Load<Sprite>("Filter types/FilterTank");
			}
		}
		else
		{
			_unitTypeIcon_image.sprite = Resources.Load<Sprite>("Filter types/" + _unit.FilterType);
		}
		_rank_image.sprite = Resources.Load<Sprite>("UI/Ranks/" + _unit.Lvl);
		_flag_image.sprite = Utils.GetFlagSprite(_unit.OwnerPlayer.Country);
	}
}

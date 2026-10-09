using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResupplyCostGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Setup")]
	public GameObject resupplyCost_Panel;

	[SerializeField]
	private TextMeshProUGUI _HP_text;

	[SerializeField]
	private TextMeshProUGUI _moneyCost_text;

	[SerializeField]
	private TextMeshProUGUI _manpowerCost_text;

	private RectTransform _tooltipRectTransform;

	private RectTransform _canvas_RectTransform;

	private Vector3 _tooltipPosition;

	private void Awake()
	{
		_tooltipRectTransform = resupplyCost_Panel.GetComponent<RectTransform>();
		_canvas_RectTransform = resupplyCost_Panel.transform.parent.gameObject.GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (!resupplyCost_Panel.activeSelf)
		{
			resupplyCost_Panel.SetActive(value: false);
			return;
		}
		Vector2 pivot = new Vector2((Input.mousePosition.x > (float)Screen.width / 2f) ? 1f : 0f, (Input.mousePosition.y > (float)Screen.height / 2f) ? 1f : 0f);
		(resupplyCost_Panel.transform as RectTransform).pivot = pivot;
		Vector2 anchoredPosition = _tooltipRectTransform.anchoredPosition;
		if (anchoredPosition.x + _tooltipRectTransform.rect.width > _canvas_RectTransform.rect.width)
		{
			anchoredPosition.x = _canvas_RectTransform.rect.width - _tooltipRectTransform.rect.width;
			resupplyCost_Panel.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		}
		if (anchoredPosition.x + _tooltipRectTransform.rect.height > _canvas_RectTransform.rect.height)
		{
			anchoredPosition.x = _canvas_RectTransform.rect.height - _tooltipRectTransform.rect.height;
			resupplyCost_Panel.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		}
		resupplyCost_Panel.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		_tooltipPosition = UIManager.instance.tooltip.transform.position;
	}

	private void LateUpdate()
	{
		if (resupplyCost_Panel.activeSelf && UIManager.instance.tooltip.activeSelf)
		{
			UIManager.instance.tooltip.transform.position = new Vector3(_tooltipPosition.x, _tooltipPosition.y - UIManager.instance.tooltip.GetComponent<RectTransform>().rect.height, _tooltipPosition.z);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (MapGO.selectedUnitGO == null || MapGO.selectedUnitGO.isMoving)
		{
			if ((bool)UIManager.instance.supplyLine)
			{
				Object.Destroy(UIManager.instance.supplyLine);
			}
			resupplyCost_Panel.SetActive(value: false);
			return;
		}
		MapGO.selectedUnitGO.CalculateSupplyRoute(p_drawSupplyLine: true);
		float num = Utils.Truncate(MapGO.selectedUnitGO.unit.SupplyPercentage, 0);
		if (gameObject.name == "Send to reserve box")
		{
			_HP_text.gameObject.SetActive(value: false);
			_manpowerCost_text.gameObject.SetActive(value: false);
			_moneyCost_text.text = (-MapGO.selectedUnitGO.unit.GetRedeployCost()).ToString();
		}
		else if (gameObject.name == "Resupply HP Box")
		{
			_HP_text.gameObject.SetActive(value: true);
			_HP_text.text = "+" + MapGO.selectedUnitGO.unit.GetHealAmount();
			_moneyCost_text.text = -MapGO.selectedUnitGO.unit.GetHealCost() + " (" + num + "% " + LocalizationManager.Translate("tooltip-resupply rate") + ")";
			_manpowerCost_text.gameObject.SetActive(PlayerSettings.Instance.Manpower && PlayerSettings.Instance.FiniteManpower);
			if (_manpowerCost_text.gameObject.activeSelf)
			{
				_manpowerCost_text.text = -MapGO.selectedUnitGO.unit.GetResupplyManpowerCost(MapGO.selectedUnitGO.unit.GetHealAmount()) + " (" + num + "% " + LocalizationManager.Translate("tooltip-resupply rate") + ")";
			}
		}
		else
		{
			_HP_text.gameObject.SetActive(value: false);
			_manpowerCost_text.gameObject.SetActive(value: false);
			if (gameObject.name == "Resupply ammo Box")
			{
				_moneyCost_text.text = -MapGO.selectedUnitGO.unit.GetResupplyAmmoCost() + " (" + num + "% " + LocalizationManager.Translate("tooltip-resupply rate") + ")";
			}
			else if (gameObject.name == "Resupply fuel Box")
			{
				_moneyCost_text.text = -MapGO.selectedUnitGO.unit.GetResupplyFuelCost() + " (" + num + "% " + LocalizationManager.Translate("tooltip-resupply rate") + ")";
			}
			else if (gameObject.name == "Resupply ammo and fuel box")
			{
				_moneyCost_text.text = -MapGO.selectedUnitGO.unit.GetResupplyAmmoAndFuelCost() + " (" + num + "% " + LocalizationManager.Translate("tooltip-resupply rate") + ")";
			}
		}
		resupplyCost_Panel.SetActive(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if ((bool)UIManager.instance.supplyLine)
		{
			Object.Destroy(UIManager.instance.supplyLine);
		}
		resupplyCost_Panel.SetActive(value: false);
	}
}

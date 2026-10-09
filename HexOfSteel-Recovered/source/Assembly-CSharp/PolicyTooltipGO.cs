using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PolicyTooltipGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private GameObject _tooltip;

	private RectTransform _canvas_RectTransform;

	private TextMeshProUGUI _policyName_text;

	private TextMeshProUGUI _policyCost_text;

	private TextMeshProUGUI _policyDescription_text;

	private PolicySlotGO _policySlotGO;

	private void Start()
	{
		_tooltip = PoliciesMenu.instance.tooltip;
		_policySlotGO = GetComponent<PolicySlotGO>();
		_canvas_RectTransform = PoliciesMenu.instance.gameObject.transform.parent.transform.GetComponent<RectTransform>();
		_policyName_text = _tooltip.transform.Find("Policy name text").GetComponent<TextMeshProUGUI>();
		_policyCost_text = _tooltip.transform.Find("Policy cost text").GetComponent<TextMeshProUGUI>();
		_policyDescription_text = _tooltip.transform.Find("Description text").GetComponent<TextMeshProUGUI>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_tooltip.SetActive(value: true);
		_policyName_text.text = _policySlotGO.policyName_text.text;
		_policyCost_text.text = LocalizationManager.Translate("txt-hq cost") + _policySlotGO.policy.HighCommandPointsCost;
		_policyDescription_text.text = LocalizationManager.GetPolicyDescription(_policySlotGO.policy);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltip.GetComponent<RectTransform>());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_tooltip.SetActive(value: false);
	}

	private void OnDisable()
	{
		_tooltip.SetActive(value: false);
	}

	private void Update()
	{
		if (_tooltip.activeSelf)
		{
			Vector2 pivot = new Vector2((Input.mousePosition.x > (float)Screen.width / 2f) ? 1f : 0f, (Input.mousePosition.y > (float)Screen.height / 2f) ? 1f : 0f);
			(_tooltip.transform as RectTransform).pivot = pivot;
			Vector2 anchoredPosition = _tooltip.transform.GetComponent<RectTransform>().anchoredPosition;
			if (anchoredPosition.x + _tooltip.GetComponent<RectTransform>().rect.width > _canvas_RectTransform.rect.width)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.width - _tooltip.GetComponent<RectTransform>().rect.width;
				_tooltip.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			if (anchoredPosition.x + _tooltip.GetComponent<RectTransform>().rect.height > _canvas_RectTransform.rect.height)
			{
				anchoredPosition.x = _canvas_RectTransform.rect.height - _tooltip.GetComponent<RectTransform>().rect.height;
				_tooltip.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			_tooltip.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}
}

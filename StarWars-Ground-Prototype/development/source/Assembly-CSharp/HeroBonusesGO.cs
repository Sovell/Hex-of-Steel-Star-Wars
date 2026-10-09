using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroBonusesGO : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public GameObject HeroBonuses_Panel;

	public GameObject HeroBonus_Text;

	public RectTransform canvas_RectTransform;

	public static Hero selectedHero;

	public GameObject softDamageHero_Text;

	public GameObject hardDamageHero_Text;

	public GameObject airDamageHero_Text;

	public GameObject MPHero_Text;

	public GameObject visibilityHero_Text;

	private void Update()
	{
		if (HeroBonuses_Panel.activeSelf)
		{
			Vector2 pivot = new Vector2((Input.mousePosition.x > (float)Screen.width / 2f) ? 1f : 0f, (Input.mousePosition.y > (float)Screen.height / 2f) ? 1f : 0f);
			(HeroBonuses_Panel.transform as RectTransform).pivot = pivot;
			Vector2 anchoredPosition = HeroBonuses_Panel.transform.GetComponent<RectTransform>().anchoredPosition;
			if (anchoredPosition.x + HeroBonuses_Panel.GetComponent<RectTransform>().rect.width > canvas_RectTransform.rect.width)
			{
				anchoredPosition.x = canvas_RectTransform.rect.width - HeroBonuses_Panel.GetComponent<RectTransform>().rect.width;
				HeroBonuses_Panel.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			if (anchoredPosition.x + HeroBonuses_Panel.GetComponent<RectTransform>().rect.height > canvas_RectTransform.rect.height)
			{
				anchoredPosition.x = canvas_RectTransform.rect.height - HeroBonuses_Panel.GetComponent<RectTransform>().rect.height;
				HeroBonuses_Panel.transform.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			HeroBonuses_Panel.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (selectedHero != null)
		{
			HeroBonuses_Panel.SetActive(value: true);
			softDamageHero_Text.GetComponentInChildren<Text>().text = "+" + selectedHero.SoftDamageBonus;
			hardDamageHero_Text.GetComponentInChildren<Text>().text = "+" + selectedHero.HardDamageBonus;
			airDamageHero_Text.GetComponentInChildren<Text>().text = "+" + selectedHero.AirDamageBonus;
			MPHero_Text.GetComponentInChildren<Text>().text = "+" + selectedHero.MPBonus;
			visibilityHero_Text.GetComponentInChildren<Text>().text = "+" + selectedHero.VisibilityBonus;
			if (selectedHero.SoftDamageBonus > 0)
			{
				softDamageHero_Text.SetActive(value: true);
			}
			else
			{
				softDamageHero_Text.SetActive(value: false);
			}
			if (selectedHero.HardDamageBonus > 0)
			{
				hardDamageHero_Text.SetActive(value: true);
			}
			else
			{
				hardDamageHero_Text.SetActive(value: false);
			}
			if (selectedHero.AirDamageBonus > 0)
			{
				airDamageHero_Text.SetActive(value: true);
			}
			else
			{
				airDamageHero_Text.SetActive(value: false);
			}
			if (selectedHero.MPBonus > 0)
			{
				MPHero_Text.SetActive(value: true);
			}
			else
			{
				MPHero_Text.SetActive(value: false);
			}
			if (selectedHero.VisibilityBonus > 0)
			{
				visibilityHero_Text.SetActive(value: true);
			}
			else
			{
				visibilityHero_Text.SetActive(value: false);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HeroBonuses_Panel.SetActive(value: false);
	}
}

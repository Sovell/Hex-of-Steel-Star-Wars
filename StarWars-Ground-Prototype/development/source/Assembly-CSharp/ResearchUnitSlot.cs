using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchUnitSlot : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI unitName_text;

	public TextMeshProUGUI progress_text;

	public Button button;

	public Slider progress_slider;

	[SerializeField]
	private Image unitBackground_image;

	[SerializeField]
	private Image unitRepresentation_image;

	public Unit unit;

	public void Setup(Unit p_unit)
	{
		unit = p_unit;
		unitName_text.text = Unit.TryTranslateUnitName(unit.Name);
		unitBackground_image.sprite = Utils.GetUnitBackground(unit, null);
		unitRepresentation_image.sprite = Utils.GetUnitSprite(unit);
		if (progress_slider.value <= 0.45f)
		{
			progress_text.color = Color.white;
		}
		else
		{
			progress_text.color = Color.black;
		}
	}
}

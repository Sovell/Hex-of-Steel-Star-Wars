using UnityEngine;
using UnityEngine.UI;

public class AchievementElementUI : MonoBehaviour
{
	[SerializeField]
	private Image _base_image;

	[SerializeField]
	private Sprite _locked_sprite;

	[SerializeField]
	private Sprite _unlocked_sprite;

	[SerializeField]
	private Text _name_text;

	[SerializeField]
	private Text _description_text;

	public void Setup(bool p_unlocked, string p_name, string p_description)
	{
		_name_text.text = p_name;
		_description_text.text = p_description;
		if (p_unlocked)
		{
			_base_image.sprite = _unlocked_sprite;
		}
		else
		{
			_base_image.sprite = _locked_sprite;
		}
	}
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordInputfieldUI : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private TMP_InputField _inputfield;

	[SerializeField]
	private Image _visibility_image;

	[SerializeField]
	private Sprite _visible_sprite;

	[SerializeField]
	private Sprite _hidden_sprite;

	public void ChangePasswordVisibility()
	{
		if (_inputfield.contentType == TMP_InputField.ContentType.Password)
		{
			_inputfield.contentType = TMP_InputField.ContentType.Standard;
			_visibility_image.sprite = _hidden_sprite;
		}
		else
		{
			_inputfield.contentType = TMP_InputField.ContentType.Password;
			_visibility_image.sprite = _visible_sprite;
		}
		string text = _inputfield.text;
		_inputfield.text = string.Empty;
		_inputfield.text = text;
	}
}

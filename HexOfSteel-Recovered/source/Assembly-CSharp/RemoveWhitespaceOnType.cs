using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class RemoveWhitespaceOnType : MonoBehaviour
{
	private TMP_InputField _inputField;

	private void Awake()
	{
		_inputField = GetComponent<TMP_InputField>();
		_inputField.onValueChanged.AddListener(OnTextChanged);
	}

	private void OnDestroy()
	{
		_inputField.onValueChanged.RemoveListener(OnTextChanged);
	}

	private void OnTextChanged(string currentText)
	{
		string text = Regex.Replace(currentText, "\\s+", "");
		if (text != currentText)
		{
			_inputField.text = text;
		}
	}
}

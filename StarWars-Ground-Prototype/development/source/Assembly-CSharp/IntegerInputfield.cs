using System;
using TMPro;
using UnityEngine;

public class IntegerInputfield : MonoBehaviour
{
	private TMP_InputField _inputField;

	private void Start()
	{
		_inputField = GetComponent<TMP_InputField>();
		TMP_InputField inputField = _inputField;
		inputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new TMP_InputField.OnValidateInput(ValidateIntegerInput));
	}

	private char ValidateIntegerInput(string text, int charIndex, char addedChar)
	{
		if (char.IsDigit(addedChar) || (addedChar == '-' && charIndex == 0))
		{
			return addedChar;
		}
		return '\0';
	}

	private void OnDestroy()
	{
		if (_inputField != null)
		{
			TMP_InputField inputField = _inputField;
			inputField.onValidateInput = (TMP_InputField.OnValidateInput)Delegate.Remove(inputField.onValidateInput, new TMP_InputField.OnValidateInput(ValidateIntegerInput));
		}
	}
}

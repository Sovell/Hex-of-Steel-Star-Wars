using System.Collections;
using System.Reflection;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SteamDeckInputField : MonoBehaviour, ISelectHandler, IEventSystemHandler, IPointerClickHandler
{
	[SerializeField]
	private TMP_InputField _inputField;

	[Header("Steam Deck Virtual Keyboard")]
	private readonly float _reopenSuppressionSeconds = 0.5f;

	private readonly float _openDebounceSeconds = 0.25f;

	private bool _isKeyboardOpen;

	private float _suppressOpenUntil;

	private float _lastOpenAttemptTime = -999f;

	private Callback<GamepadTextInputDismissed_t> _textInputDismissed;

	private void Awake()
	{
		if (_inputField == null)
		{
			_inputField = GetComponent<TMP_InputField>();
		}
		_textInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnTextInputDismissed);
	}

	private void OnDisable()
	{
		if (_isKeyboardOpen)
		{
			ForceDismissKeyboard();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus && _isKeyboardOpen)
		{
			ForceDismissKeyboard();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		TryOpenKeyboard();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		TryOpenKeyboard();
	}

	private void TryOpenKeyboard()
	{
		if (_inputField == null)
		{
			return;
		}
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime < _suppressOpenUntil || unscaledTime - _lastOpenAttemptTime < _openDebounceSeconds)
		{
			return;
		}
		_lastOpenAttemptTime = unscaledTime;
		if (SteamManager.IsSteamReady && SteamUtils.IsSteamRunningOnSteamDeck() && !_isKeyboardOpen && _inputField.interactable && !_inputField.readOnly)
		{
			string pchExistingText = _inputField.text ?? string.Empty;
			EGamepadTextInputLineMode eLineInputMode = ((_inputField.lineType == TMP_InputField.LineType.MultiLineNewline || _inputField.lineType == TMP_InputField.LineType.MultiLineSubmit) ? EGamepadTextInputLineMode.k_EGamepadTextInputLineModeMultipleLines : EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine);
			uint unCharMax = ((_inputField.characterLimit > 0) ? ((uint)_inputField.characterLimit) : 255u);
			if (!(_isKeyboardOpen = SteamUtils.ShowGamepadTextInput(EGamepadTextInputMode.k_EGamepadTextInputModeNormal, eLineInputMode, "Enter text", unCharMax, pchExistingText)))
			{
				Debug.LogWarning("Failed to open Steam Deck keyboard");
			}
			else
			{
				_suppressOpenUntil = unscaledTime + 0.1f;
			}
		}
	}

	private void OnTextInputDismissed(GamepadTextInputDismissed_t cb)
	{
		if (!_isKeyboardOpen)
		{
			return;
		}
		_isKeyboardOpen = false;
		_suppressOpenUntil = Time.unscaledTime + _reopenSuppressionSeconds;
		if (_inputField == null)
		{
			return;
		}
		_inputField.DeactivateInputField();
		StartCoroutine(CR_UnselectNextFrame());
		if (cb.m_bSubmitted)
		{
			uint enteredGamepadTextLength = SteamUtils.GetEnteredGamepadTextLength();
			string text = string.Empty;
			if (enteredGamepadTextLength != 0 && SteamUtils.GetEnteredGamepadTextInput(out var pchText, enteredGamepadTextLength + 1))
			{
				text = pchText;
			}
			_inputField.text = text;
			_inputField.caretPosition = text.Length;
			_inputField.ForceLabelUpdate();
		}
	}

	private void ForceDismissKeyboard()
	{
		_isKeyboardOpen = false;
		_suppressOpenUntil = Time.unscaledTime + _reopenSuppressionSeconds;
		if (!SteamManager.IsSteamReady || !SteamUtils.IsSteamRunningOnSteamDeck())
		{
			return;
		}
		try
		{
			typeof(SteamUtils).GetMethod("DismissGamepadTextInput", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
		}
		catch
		{
		}
	}

	private IEnumerator CR_UnselectNextFrame()
	{
		yield return null;
		if (_inputField != null)
		{
			_inputField.ReleaseSelection();
			_inputField.caretPosition = _inputField.text?.Length ?? 0;
			_inputField.ForceLabelUpdate();
		}
		EventSystem current = EventSystem.current;
		if (current != null)
		{
			current.SetSelectedGameObject(null);
		}
	}

	private void OnDestroy()
	{
		if (_isKeyboardOpen)
		{
			ForceDismissKeyboard();
		}
		_textInputDismissed?.Dispose();
		_textInputDismissed = null;
	}
}

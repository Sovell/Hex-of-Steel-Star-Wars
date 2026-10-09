using UnityEngine;
using UnityEngine.UI;

public class ToggleLabelColor : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField]
	private Toggle _toggle;

	[Header("Colors")]
	[SerializeField]
	private Color _onColor = Color.white;

	[SerializeField]
	private Color _offColor = Color.black;

	private void Start()
	{
		UpdateToggleGraphicColor(_toggle.isOn);
	}

	public void UpdateToggleGraphicColor(bool p_isOn)
	{
		_toggle.targetGraphic.color = (p_isOn ? _onColor : _offColor);
	}
}

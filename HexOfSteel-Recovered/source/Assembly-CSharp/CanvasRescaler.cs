using UnityEngine;
using UnityEngine.UI;

public class CanvasRescaler : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler[] _canvasScalers;

	private void Start()
	{
		RescaleCanvases();
	}

	public void RescaleCanvases()
	{
		for (int i = 0; i < _canvasScalers.Length; i++)
		{
			_canvasScalers[i].referencePixelsPerUnit = 100f;
			_canvasScalers[i].uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			_canvasScalers[i].scaleFactor = PlayerSettings.Instance.UIScale;
		}
	}
}

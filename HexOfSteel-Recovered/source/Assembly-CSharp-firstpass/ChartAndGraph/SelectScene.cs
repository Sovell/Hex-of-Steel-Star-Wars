using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph
{
	public class SelectScene : MonoBehaviour
	{
		public GameObject EventSystem;

		public GameObject MainCamera;

		public Canvas MainCanvas;

		public Canvas BackCanvas;

		public Button ButtonPrefab;

		private string[] Buttons = new string[32]
		{
			"3D Bar 1", "Chart And Graph/Themes/3d/Bar/Theme1/Preset2", "3D Bar 2", "Chart And Graph/Themes/3d/Bar/Theme2/Preset1", "3D Pie 1", "Chart And Graph/Themes/3d/Pie/Theme 2/preset5", "3D Pie 2", "Chart And Graph/Themes/3d/Pie/Theme 6/preset 3", "3D Graph 1", "Chart And Graph/Themes/3d/Graph/Theme 1/preset 2",
			"3D Graph 2", "Chart And Graph/Themes/3d/Graph/Theme 2/preset 1", "3D Bubble", "Chart And Graph/Themes/3d/Bubble/preset 1", "3D Radar", "Chart And Graph/Themes/3d/Radar/Theme 1/preset 1", "2D Bar 1", "Chart And Graph/Themes/2d/Bar/preset 2", "2D Bar 2", "Chart And Graph/Themes/2d/Bar/preset 3",
			"2D Pie 1", "Chart And Graph/Themes/2d/Pie/preset1", "2D Pie 2", "Chart And Graph/Themes/2d/Pie/preset3", "2D Graph", "Chart And Graph/Themes/2d/Graph/preset 1", "2D Realtime Graph", "Chart And Graph/Themes/2d/Graph/preset 5", "2D Bubble", "Chart And Graph/Themes/2d/Bubble/preset 1",
			"2D Radar", "Chart And Graph/Themes/2d/Radar/preset 1"
		};

		private void Start()
		{
			for (int i = 0; i < Buttons.Length; i += 2)
			{
				string text = Buttons[i];
				string scene = Buttons[i + 1];
				Button button = Object.Instantiate(ButtonPrefab);
				button.GetComponentInChildren<Text>().text = text;
				button.onClick.AddListener(() =>
				{
					Select(scene);
				});
				button.transform.SetParent(MainCanvas.transform, worldPositionStays: false);
			}
		}

		private void ChangeCanvas()
		{
			EventSystem.SetActive(value: false);
			MainCamera.SetActive(value: false);
			MainCanvas.gameObject.SetActive(value: false);
			BackCanvas.gameObject.SetActive(value: true);
		}

		public void Select(string scene)
		{
			Application.LoadLevelAdditive(scene);
			ChangeCanvas();
		}

		public void SelectMain()
		{
			Application.LoadLevel("Chart And Graph/Themes/Demo");
			ChangeCanvas();
		}
	}
}

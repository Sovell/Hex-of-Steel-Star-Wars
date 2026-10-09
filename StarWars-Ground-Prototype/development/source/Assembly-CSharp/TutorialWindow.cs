using TMPro;
using UnityEngine;

public class TutorialWindow : UIWindowController
{
	[Header("Setup")]
	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private TextMeshProUGUI _bodyText;

	[SerializeField]
	private Transform _tasksParent;

	[SerializeField]
	private GameObject _content;

	[Header("Buttons")]
	public GameObject prevButton;

	public GameObject nextButton;

	public GameObject skipButton;

	protected override void Start()
	{
		base.Start();
		OnMinimized += Minimized;
		OnMaximized += Maximized;
	}

	public void SetTitle(string p_text)
	{
		_titleText.text = p_text;
	}

	public void SetBody(string p_text)
	{
		_bodyText.text = p_text;
	}

	public void AddTask(TutorialTaskUI p_tutorialTaskUI)
	{
		p_tutorialTaskUI.transform.SetParent(_tasksParent, worldPositionStays: false);
	}

	public void ClearTasks()
	{
		TutorialTaskUI[] componentsInChildren = _tasksParent.GetComponentsInChildren<TutorialTaskUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	private void Minimized()
	{
		_content.SetActive(value: false);
	}

	private void Maximized()
	{
		_content.SetActive(value: true);
	}
}

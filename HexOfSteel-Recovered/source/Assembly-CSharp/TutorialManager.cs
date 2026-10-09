using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	public static TutorialManager instance;

	private const float MIN_SKIP_TIME = 5f;

	[Header("UI")]
	[SerializeField]
	private GameObject _tutorialButton;

	[SerializeField]
	private TutorialWindow _tutorialWindow;

	[SerializeField]
	private TutorialTaskUI _taskItemPrefab;

	[Header("Data")]
	[SerializeField]
	private List<TutorialStepData> _tutorialSteps;

	[Header("DEBUG / INFOS")]
	[SerializeField]
	private int _currentStep;

	[SerializeField]
	private int _currentPage;

	[SerializeField]
	private bool _isTutorialActive;

	private HashSet<TutorialActions> _pendingActions = new HashSet<TutorialActions>();

	private Dictionary<TutorialActions, TutorialTaskUI> _taskWidgets = new Dictionary<TutorialActions, TutorialTaskUI>();

	private float _activeTaskTime;

	public event Action<TutorialStepData, int> OnPageStarted;

	public event Action<TutorialStepData, int> OnPageCompleted;

	public event Action<TutorialStepData> OnStepCompleted;

	public event Action OnTutorialCompleted;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		_currentPage = 0;
		_currentStep = GameData.Instance.tutorialStep;
		_isTutorialActive = GameData.Instance.map.IsTutorial;
		if (ShouldProcessTutorial())
		{
			StartPage(_currentStep, _currentPage);
		}
		else
		{
			_tutorialWindow.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		ProcessSkipButton();
	}

	public static bool HasAction(TutorialActions p_action)
	{
		if (instance == null)
		{
			return false;
		}
		return instance._pendingActions.Contains(p_action);
	}

	public static void PerformAction(TutorialActions p_action, bool p_useInstance = true)
	{
		if (!(instance == null))
		{
			instance.PerformAction(p_action);
		}
	}

	public void SkipStep()
	{
		Continue();
	}

	public void ClosedTutorialWindow()
	{
		_tutorialButton.SetActive(value: true);
	}

	public void OpenedTutorialWindow()
	{
		_tutorialButton.SetActive(value: false);
	}

	public void Continue()
	{
		if (ShouldProcessTutorial())
		{
			SoundManager.instance.PlayUIClickSound();
			if (_currentPage < GetCurrentStep().pages.Count - 1)
			{
				CompletePage();
				_currentPage++;
				StartPage(_currentStep, _currentPage);
			}
			else
			{
				CompleteStep();
			}
		}
	}

	public void GoBack()
	{
		if (!ShouldProcessTutorial())
		{
			return;
		}
		SoundManager.instance.PlayUIClickSound();
		if (_currentPage != 0 || _currentStep != 0)
		{
			if (_currentPage > 0)
			{
				StartPage(_currentStep, _currentPage - 1);
				return;
			}
			int num = _currentStep - 1;
			int pageIdx = GetStep(num).pages.Count - 1;
			StartPage(num, pageIdx);
		}
	}

	private void PerformAction(TutorialActions p_action)
	{
		if (ShouldProcessTutorial() && _pendingActions != null && _pendingActions.Remove(p_action))
		{
			if (_taskWidgets.TryGetValue(p_action, out var value))
			{
				value.UpdateTaskStatus(p_done: true);
			}
			if (_pendingActions.Count == 0)
			{
				AutoAdvanceIfAllowed();
			}
		}
	}

	private void ProcessSkipButton()
	{
		if (_tutorialWindow.nextButton.activeSelf)
		{
			_activeTaskTime = 0f;
			return;
		}
		_activeTaskTime += Time.deltaTime;
		if (_activeTaskTime >= 5f)
		{
			_tutorialWindow.skipButton.SetActive(value: true);
		}
	}

	private void StartPage(int stepIdx, int pageIdx)
	{
		_activeTaskTime = 0f;
		_tutorialWindow.skipButton.SetActive(value: false);
		_currentStep = Mathf.Clamp(stepIdx, 0, _tutorialSteps.Count - 1);
		TutorialStepData step = GetStep(_currentStep);
		if (step == null || step.pages == null || step.pages.Count == 0)
		{
			CompleteTutorial();
			return;
		}
		_currentPage = Mathf.Clamp(pageIdx, 0, step.pages.Count - 1);
		TutorialPage tutorialPage = step.pages[_currentPage];
		_pendingActions = new HashSet<TutorialActions>(tutorialPage.tasks.Select((TutorialTask t) => t.action));
		_tutorialWindow.gameObject.SetActive(value: true);
		_tutorialWindow.SetTitle(LocalizationManager.Translate(tutorialPage.titleLocalizationID));
		_tutorialWindow.SetBody(LocalizationManager.Translate(tutorialPage.bodyLocalizationID));
		_tutorialWindow.ClearTasks();
		_taskWidgets.Clear();
		foreach (TutorialTask task in tutorialPage.tasks)
		{
			TutorialTaskUI tutorialTaskUI = UnityEngine.Object.Instantiate(_taskItemPrefab);
			tutorialTaskUI.Setup(LocalizationManager.Translate(task.localizationID), p_done: false);
			_tutorialWindow.AddTask(tutorialTaskUI);
			_taskWidgets.Add(task.action, tutorialTaskUI);
		}
		_tutorialWindow.nextButton.SetActive(CanGoNextWithoutTasks() || _pendingActions.Count == 0);
		_tutorialWindow.prevButton.SetActive(_currentStep > 0 || _currentPage > 0);
		OnPageStarted?.Invoke(step, _currentPage);
	}

	private void CompletePage()
	{
		TutorialStepData currentStep = GetCurrentStep();
		OnPageCompleted?.Invoke(currentStep, _currentPage);
	}

	private void CompleteStep()
	{
		TutorialStepData currentStep = GetCurrentStep();
		OnStepCompleted?.Invoke(currentStep);
		_currentStep++;
		_currentPage = 0;
		GameData.Instance.tutorialStep = _currentStep;
		if (_currentStep >= _tutorialSteps.Count)
		{
			CompleteTutorial();
		}
		else
		{
			StartPage(_currentStep, _currentPage);
		}
	}

	private void CompleteTutorial()
	{
		if (ShouldProcessTutorial())
		{
			_isTutorialActive = false;
			OnTutorialCompleted?.Invoke();
			_tutorialWindow.gameObject.SetActive(value: false);
			SteamManager.UnlockAchievement("Tutorial");
		}
	}

	private void AutoAdvanceIfAllowed()
	{
		_tutorialWindow.nextButton.SetActive(value: true);
	}

	private bool ShouldProcessTutorial()
	{
		if (!_isTutorialActive)
		{
			return false;
		}
		if (_tutorialSteps == null || _tutorialSteps.Count == 0)
		{
			return false;
		}
		if (TurnManager.currPlayer == null || TurnManager.currPlayer.IsComputer)
		{
			return false;
		}
		return true;
	}

	private bool CanGoNextWithoutTasks()
	{
		return GetCurrentPage().tasks.Count == 0;
	}

	private TutorialStepData GetCurrentStep()
	{
		return GetStep(_currentStep);
	}

	private TutorialStepData GetStep(int idx)
	{
		if (idx < 0 || idx >= _tutorialSteps.Count)
		{
			return null;
		}
		return _tutorialSteps[idx];
	}

	private TutorialPage GetCurrentPage()
	{
		return GetCurrentStep().pages[_currentPage];
	}
}

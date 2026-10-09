using TMPro;
using UnityEngine;

public class SceneTranslator : MonoBehaviour
{
	public static SceneTranslator instance;

	[Header("Setup")]
	[SerializeField]
	private bool _translateSceneOnStart = true;

	[SerializeField]
	private Transform _UI_parent;

	[SerializeField]
	private TMP_FontAsset _EU_font;

	[SerializeField]
	private TMP_FontAsset _CN_font;

	[SerializeField]
	private TMP_FontAsset _JP_font;

	[SerializeField]
	private TMP_FontAsset _KR_font;

	private LocalizedText[] _localizedTexts;

	private TMP_FontAsset _activeFontAsset;

	private GameLanguages _previousGameLanguage;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		if (_translateSceneOnStart)
		{
			TranslateScene();
		}
	}

	public void TranslateScene()
	{
		GameLanguages language = PlayerSettings.Instance.Language;
		bool flag = false;
		if (_previousGameLanguage != language)
		{
			flag = language == GameLanguages.CNS || language == GameLanguages.CNT || language == GameLanguages.KR || language == GameLanguages.JP;
			if (!flag)
			{
				flag = _previousGameLanguage == GameLanguages.CNS || _previousGameLanguage == GameLanguages.CNT || _previousGameLanguage == GameLanguages.KR || _previousGameLanguage == GameLanguages.JP;
			}
		}
		if (flag)
		{
			_activeFontAsset = GetFontToUse(PlayerSettings.Instance.Language);
		}
		_localizedTexts = _UI_parent.GetComponentsInChildren<LocalizedText>(includeInactive: true);
		for (int i = 0; i < _localizedTexts.Length; i++)
		{
			if (flag)
			{
				_localizedTexts[i].Translate(_activeFontAsset);
			}
			else
			{
				_localizedTexts[i].Translate();
			}
		}
		_previousGameLanguage = PlayerSettings.Instance.Language;
	}

	public TMP_FontAsset GetActiveFontAsset()
	{
		return _activeFontAsset;
	}

	private TMP_FontAsset GetFontToUse(GameLanguages p_gameLanguage)
	{
		switch (p_gameLanguage)
		{
		default:
			return _EU_font;
		case GameLanguages.KR:
			return _KR_font;
		case GameLanguages.JP:
			return _JP_font;
		case GameLanguages.CNS:
		case GameLanguages.CNT:
			return _CN_font;
		}
	}
}

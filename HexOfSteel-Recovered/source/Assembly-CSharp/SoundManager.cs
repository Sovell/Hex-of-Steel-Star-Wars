using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
	public static SoundManager instance;

	[SerializeField]
	private AnimationCurve _soundVolumeByDistance;

	public AudioSource unit_Source;

	public AudioSource weather_Source;

	public AudioSource music_Source;

	public AudioSource UI_Source;

	public Slider unitVolume_Slider;

	public Slider weatherVolume_Slider;

	public Slider musicVolume_Slider;

	public Slider UIVolume_slider;

	public List<AudioClip> listOfAllMusics;

	public GameObject playlist;

	public GameObject musicSlotPrefab;

	public Transform container;

	public Toggle shuffle_toggle;

	private int _musicIndex;

	private int _lastMusicIndex = -1;

	private bool _isGameScene;

	private bool _isMainMenuScene;

	private bool _isMusicPaused;

	private bool _isAudioPaused;

	public static AudioClip GetMovementSound(string p_soundName)
	{
		if (ModsManager.moddedMovementSounds.Count > 0)
		{
			foreach (AudioClip moddedMovementSound in ModsManager.moddedMovementSounds)
			{
				if (moddedMovementSound.name == p_soundName)
				{
					return moddedMovementSound;
				}
			}
		}
		return Resources.Load<AudioClip>("Sounds/Moving/" + p_soundName);
	}

	public static AudioClip GetAttackSound(string p_soundName)
	{
		if (ModsManager.moddedAttackSounds.Count > 0)
		{
			foreach (AudioClip moddedAttackSound in ModsManager.moddedAttackSounds)
			{
				if (moddedAttackSound.name == p_soundName)
				{
					return moddedAttackSound;
				}
			}
		}
		return Resources.Load<AudioClip>("Sounds/Attacking/" + p_soundName);
	}

	public static AudioClip GetUISound(string p_soundName)
	{
		return Resources.Load<AudioClip>("Sounds/UI/" + p_soundName);
	}

	private void Awake()
	{
		instance = this;
		_isGameScene = SceneManager.GetActiveScene().buildIndex == 1;
		_isMainMenuScene = SceneManager.GetActiveScene().buildIndex == 0;
		if ((bool)UI_Source)
		{
			UI_Source.volume = PlayerSettings.Instance.UIVolume;
		}
		if ((bool)music_Source)
		{
			music_Source.volume = PlayerSettings.Instance.MusicVolume;
		}
		if ((bool)unit_Source)
		{
			unit_Source.volume = PlayerSettings.Instance.UnitVolume;
		}
		if ((bool)weather_Source)
		{
			weather_Source.volume = PlayerSettings.Instance.WeatherVolume;
		}
		if (MainMenu.isEditor || _isMainMenuScene)
		{
			if (_isMainMenuScene)
			{
				music_Source.Play();
			}
			return;
		}
		unitVolume_Slider.value = PlayerSettings.Instance.UnitVolume;
		weatherVolume_Slider.value = PlayerSettings.Instance.WeatherVolume;
		musicVolume_Slider.value = PlayerSettings.Instance.MusicVolume;
		UIVolume_slider.value = PlayerSettings.Instance.UIVolume;
		if (SteamManager.OwnsDLC(new AppId_t(1484260u)))
		{
			listOfAllMusics.Add(Resources.Load<AudioClip>("DLC/Musics/Tobruk"));
			listOfAllMusics.Add(Resources.Load<AudioClip>("DLC/Musics/Rhine March"));
			listOfAllMusics.Add(Resources.Load<AudioClip>("DLC/Musics/Kursk"));
			listOfAllMusics.Add(Resources.Load<AudioClip>("DLC/Musics/Operation Typhoon"));
			listOfAllMusics.Add(Resources.Load<AudioClip>("DLC/Musics/Peenemunde"));
			SteamManager.UnlockAchievement("Supporter");
		}
		if (PlayerSettings.Instance.DisableVanillaSoundtracks)
		{
			listOfAllMusics.Clear();
		}
		foreach (AudioClip moddedMusic in ModsManager.moddedMusics)
		{
			listOfAllMusics.Add(moddedMusic);
		}
		if (listOfAllMusics.Count > 0)
		{
			_musicIndex = Random.Range(0, listOfAllMusics.Count);
			_lastMusicIndex = _musicIndex;
			music_Source.clip = listOfAllMusics[_musicIndex];
			music_Source.Play();
		}
	}

	private void Start()
	{
		if (shuffle_toggle != null)
		{
			shuffle_toggle.SetIsOnWithoutNotify(PlayerSettings.Instance.ShuffleMusics);
		}
		foreach (AudioClip music in listOfAllMusics)
		{
			GameObject gameObject = Object.Instantiate(musicSlotPrefab, container);
			gameObject.name = music.name;
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = music.name;
			gameObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
			{
				PlayMusicByName(music.name, p_fromUI: true);
			});
		}
	}

	private void Update()
	{
		if (!music_Source.isPlaying && !_isMusicPaused && !_isAudioPaused)
		{
			if (PlayerSettings.Instance.ShuffleMusics)
			{
				PlayRandomMusic(p_fromUI: false);
			}
			else
			{
				PlayNextMusic(p_fromUI: false);
			}
		}
		if (_isGameScene)
		{
			unit_Source.volume = PlayerSettings.Instance.UnitVolume * _soundVolumeByDistance.Evaluate(CameraGO.instance.cam.orthographicSize);
		}
	}

	public void PlayUIClickSound()
	{
		UI_Source.PlayOneShot(GetUISound("Click"));
	}

	private void PlayMusicByName(string p_musicName, bool p_fromUI)
	{
		if (p_fromUI)
		{
			PlayUIClickSound();
		}
		PlayPause(p_fromUI);
		for (int i = 0; i < listOfAllMusics.Count; i++)
		{
			if (listOfAllMusics[i].name == p_musicName)
			{
				_musicIndex = i;
				music_Source.clip = listOfAllMusics[i];
				break;
			}
		}
		_lastMusicIndex = _musicIndex;
		PlayPause(p_fromUI);
	}

	public void OnClick_ShowHidePlaylist()
	{
		PlayUIClickSound();
		playlist.SetActive(!playlist.activeSelf);
		RefreshPlayingIcon();
	}

	public void OnClick_ChangeShuffleMode()
	{
		PlayUIClickSound();
		PlayerSettings.Instance.ShuffleMusics = !PlayerSettings.Instance.ShuffleMusics;
		_lastMusicIndex = _musicIndex;
	}

	public void PlayPause(bool p_fromUI)
	{
		if (p_fromUI)
		{
			PlayUIClickSound();
		}
		if (music_Source.isPlaying)
		{
			_isMusicPaused = true;
			music_Source.Pause();
		}
		else
		{
			_isMusicPaused = false;
			music_Source.Play();
		}
		RefreshPlayingIcon();
	}

	public void PlayRandomMusic(bool p_fromUI)
	{
		if (p_fromUI)
		{
			PlayUIClickSound();
		}
		if (listOfAllMusics.Count == 0)
		{
			return;
		}
		if (listOfAllMusics.Count > 1)
		{
			do
			{
				_musicIndex = Random.Range(0, listOfAllMusics.Count);
			}
			while (_musicIndex == _lastMusicIndex);
		}
		else
		{
			_musicIndex = 0;
		}
		_lastMusicIndex = _musicIndex;
		music_Source.clip = listOfAllMusics[_musicIndex];
		music_Source.Play();
		RefreshPlayingIcon();
	}

	public void PlayNextMusic(bool p_fromUI)
	{
		if (PlayerSettings.Instance.ShuffleMusics)
		{
			PlayRandomMusic(p_fromUI);
			return;
		}
		if (p_fromUI)
		{
			PlayUIClickSound();
		}
		if (listOfAllMusics.Count != 0)
		{
			_lastMusicIndex = _musicIndex;
			_musicIndex++;
			if (_musicIndex >= listOfAllMusics.Count)
			{
				_musicIndex = 0;
			}
			music_Source.clip = listOfAllMusics[_musicIndex];
			music_Source.Play();
			RefreshPlayingIcon();
		}
	}

	public void PlayPreviousMusic(bool p_fromUI)
	{
		if (p_fromUI)
		{
			PlayUIClickSound();
		}
		if (listOfAllMusics.Count != 0)
		{
			_lastMusicIndex = _musicIndex;
			_musicIndex--;
			if (_musicIndex < 0)
			{
				_musicIndex = listOfAllMusics.Count - 1;
			}
			music_Source.clip = listOfAllMusics[_musicIndex];
			music_Source.Play();
			RefreshPlayingIcon();
		}
	}

	public void ChangeUnitVolume()
	{
		PlayerSettings.Instance.UnitVolume = unitVolume_Slider.value;
		unit_Source.volume = unitVolume_Slider.value;
	}

	public void ChangeWeatherVolume()
	{
		PlayerSettings.Instance.WeatherVolume = weatherVolume_Slider.value;
		weather_Source.volume = weatherVolume_Slider.value;
	}

	public void ChangeMusicVolume()
	{
		PlayerSettings.Instance.MusicVolume = musicVolume_Slider.value;
		music_Source.volume = musicVolume_Slider.value;
	}

	public void ChangeUIVolume_Slider()
	{
		PlayerSettings.Instance.UIVolume = UIVolume_slider.value;
		UI_Source.volume = UIVolume_slider.value;
	}

	public void PlayDelayedUnitSound(float p_delay, string p_soundName)
	{
		StartCoroutine(CR_PlayDelayedUnitSound(p_delay, p_soundName));
	}

	public void SaveSoundSettings()
	{
		PlayerSettings.Save();
	}

	private void RefreshPlayingIcon()
	{
		string text = null;
		if (music_Source.clip != null)
		{
			text = music_Source.clip.name;
		}
		Image[] componentsInChildren = container.GetComponentsInChildren<Image>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].name == "Currently played image")
			{
				componentsInChildren[i].gameObject.SetActive(componentsInChildren[i].transform.parent.name == text);
			}
		}
	}

	private void OnApplicationFocus(bool p_focus)
	{
		if (p_focus)
		{
			_isAudioPaused = false;
			if (!_isMusicPaused)
			{
				music_Source.UnPause();
			}
			if (weather_Source != null)
			{
				weather_Source.UnPause();
			}
		}
		else
		{
			_isAudioPaused = true;
			music_Source.Pause();
			if (weather_Source != null)
			{
				weather_Source.Pause();
			}
		}
	}

	private IEnumerator CR_PlayDelayedUnitSound(float p_delay, string p_soundName)
	{
		yield return new WaitForSeconds(p_delay);
		unit_Source.PlayOneShot(GetAttackSound(p_soundName));
	}

	private void OnApplicationQuit()
	{
		SaveSoundSettings();
	}
}

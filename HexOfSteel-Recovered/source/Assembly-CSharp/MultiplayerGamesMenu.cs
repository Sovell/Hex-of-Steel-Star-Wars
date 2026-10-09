using UnityEngine;
using UnityEngine.UI;

public class MultiplayerGamesMenu : MonoBehaviour
{
	[Header("Sub-menus")]
	[SerializeField]
	private ContinueGameMenu _continueGameMenu;

	[SerializeField]
	private JoinGameMenu _joinGameMenu;

	[SerializeField]
	private HostGameMenu _hostGameMenu;

	[SerializeField]
	private RealtimeMultiplayerMenu _realtimeMultiplayerMenu;

	[SerializeField]
	private LobbyChatMenu _lobbyChatMenu;

	[SerializeField]
	private ArchivedGamesMenu _archivedGamesMenu;

	[SerializeField]
	private Toggle _continueGameTab_toggle;

	[SerializeField]
	private Toggle _hostGameTab_toggle;

	[SerializeField]
	private Toggle _joinGameTab_toggle;

	[SerializeField]
	private Toggle _archivedGamesTab_toggle;

	[SerializeField]
	private Toggle _realtimeMultiplayerTab_toggle;

	[SerializeField]
	private ToggleLabelColor _continueGameTab_toggleLabelColor;

	[SerializeField]
	private ToggleLabelColor _hostGameTab_toggleLabelColor;

	[SerializeField]
	private ToggleLabelColor _joinGameTab_toggleLabelColor;

	[SerializeField]
	private ToggleLabelColor _archivedGamesTab_toggleLabelColor;

	[SerializeField]
	private ToggleLabelColor _realtimeMultiplayerTab_toggleLabelColor;

	private void OnEnable()
	{
		ShowJoinGameMenu();
	}

	public void Open()
	{
		gameObject.SetActive(value: true);
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
	}

	public void OnClick_RefreshGamesListing()
	{
		SoundManager.instance.PlayUIClickSound();
		if (_joinGameMenu.gameObject.activeSelf)
		{
			_joinGameMenu.RefreshGamesListing();
		}
		else if (_continueGameMenu.gameObject.activeSelf)
		{
			_continueGameMenu.RefreshGamesListing();
		}
		else if (_archivedGamesMenu.gameObject.activeSelf)
		{
			_archivedGamesMenu.RefreshGamesListing();
		}
	}

	public void OnClick_ContinueGame()
	{
		SoundManager.instance.PlayUIClickSound();
		ShowContinueGameMenu();
	}

	public void OnClick_HostGame()
	{
		SoundManager.instance.PlayUIClickSound();
		ShowHostGameMenu();
	}

	public void OnClick_JoinGame()
	{
		SoundManager.instance.PlayUIClickSound();
		ShowJoinGameMenu();
	}

	public void OnClick_RealtimeMultiplayer()
	{
		SoundManager.instance.PlayUIClickSound();
		ShowRealtimeMultiplayerMenu();
	}

	public void OnClick_LobbyChat()
	{
		SoundManager.instance.PlayUIClickSound();
		ShowMenu(null, _lobbyChatMenu);
	}

	public void OnClick_ArchivedGames()
	{
		SoundManager.instance.PlayUIClickSound();
		ShowArchivedGamesMenu();
	}

	public void ShowContinueGameMenu()
	{
		ShowMenu(_continueGameTab_toggle, _continueGameMenu);
	}

	public void ShowHostGameMenu()
	{
		ShowMenu(_hostGameTab_toggle, _hostGameMenu);
	}

	public void ShowJoinGameMenu()
	{
		ShowMenu(_joinGameTab_toggle, _joinGameMenu);
	}

	public void ShowArchivedGamesMenu()
	{
		ShowMenu(_archivedGamesTab_toggle, _archivedGamesMenu);
	}

	public void ShowRealtimeMultiplayerMenu()
	{
		ShowMenu(_realtimeMultiplayerTab_toggle, _realtimeMultiplayerMenu);
	}

	private void ShowMenu(Toggle p_activeToggle, IMenu p_menuToOpen)
	{
		_continueGameTab_toggle.SetIsOnWithoutNotify(_continueGameTab_toggle == p_activeToggle);
		_hostGameTab_toggle.SetIsOnWithoutNotify(_hostGameTab_toggle == p_activeToggle);
		_joinGameTab_toggle.SetIsOnWithoutNotify(_joinGameTab_toggle == p_activeToggle);
		_archivedGamesTab_toggle.SetIsOnWithoutNotify(_archivedGamesTab_toggle == p_activeToggle);
		_realtimeMultiplayerTab_toggle.SetIsOnWithoutNotify(_realtimeMultiplayerTab_toggle == p_activeToggle);
		if (p_menuToOpen != _lobbyChatMenu && !((MonoBehaviour)p_menuToOpen).gameObject.activeSelf)
		{
			CloseAll();
		}
		else if (p_menuToOpen != _lobbyChatMenu)
		{
			_lobbyChatMenu.Close();
		}
		p_menuToOpen.Open();
		RefreshToggles();
	}

	private void RefreshToggles()
	{
		_continueGameTab_toggleLabelColor.UpdateToggleGraphicColor(_continueGameTab_toggle.isOn);
		_hostGameTab_toggleLabelColor.UpdateToggleGraphicColor(_hostGameTab_toggle.isOn);
		_joinGameTab_toggleLabelColor.UpdateToggleGraphicColor(_joinGameTab_toggle.isOn);
		_archivedGamesTab_toggleLabelColor.UpdateToggleGraphicColor(_archivedGamesTab_toggle.isOn);
		_realtimeMultiplayerTab_toggleLabelColor.UpdateToggleGraphicColor(_realtimeMultiplayerTab_toggle.isOn);
	}

	private void CloseAll()
	{
		_continueGameMenu.Close();
		_joinGameMenu.Close();
		_hostGameMenu.Close();
		_archivedGamesMenu.Close();
		_lobbyChatMenu.Close();
		_realtimeMultiplayerMenu.Close();
	}
}

using TMPro;
using UnityEngine;

public class LastTurnSummaryUI : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private TextMeshProUGUI _countryName_text;

	[SerializeField]
	private TextMeshProUGUI _unitsDestroyed_text;

	[SerializeField]
	private TextMeshProUGUI _unitsLost_text;

	[SerializeField]
	private TextMeshProUGUI _unitsBuilt_text;

	[SerializeField]
	private TextMeshProUGUI _moneySpent_text;

	[SerializeField]
	private TextMeshProUGUI _moneyEarned_text;

	[Header("DEBUG / INFOS")]
	public LastTurnSummaryData lastTurnSummaryData;

	public void Setup(Player p_player)
	{
		lastTurnSummaryData = default;
		_countryName_text.text = LocalizationManager.GetLocalizedCountry(p_player.Country).Replace(".", "");
		lastTurnSummaryData.countryName = _countryName_text.text;
		int turnUnitsDestroyedBackup = p_player.TurnUnitsDestroyedBackup;
		turnUnitsDestroyedBackup += p_player.TurnUnitsDestroyed;
		_unitsDestroyed_text.text = turnUnitsDestroyedBackup.ToString();
		if (turnUnitsDestroyedBackup > 0)
		{
			_unitsDestroyed_text.color = MainMenu.greenColor;
		}
		lastTurnSummaryData.unitsDestroyed = turnUnitsDestroyedBackup;
		turnUnitsDestroyedBackup = p_player.TurnUnitsLostBackup;
		turnUnitsDestroyedBackup += p_player.TurnUnitsLost;
		_unitsLost_text.text = turnUnitsDestroyedBackup.ToString();
		if (turnUnitsDestroyedBackup > 0)
		{
			_unitsLost_text.color = MainMenu.redColor;
		}
		lastTurnSummaryData.unitsLost = turnUnitsDestroyedBackup;
		turnUnitsDestroyedBackup = p_player.TurnUnitsBoughtBackup;
		turnUnitsDestroyedBackup += p_player.TurnUnitsBought;
		_unitsBuilt_text.text = turnUnitsDestroyedBackup.ToString();
		if (turnUnitsDestroyedBackup > 0)
		{
			_unitsBuilt_text.color = MainMenu.yellowColor;
		}
		lastTurnSummaryData.unitsBuilt = turnUnitsDestroyedBackup;
		turnUnitsDestroyedBackup = p_player.TurnMoneySpentBackup;
		turnUnitsDestroyedBackup += p_player.TurnMoneySpent;
		_moneySpent_text.text = turnUnitsDestroyedBackup.ToString();
		if (turnUnitsDestroyedBackup > 0)
		{
			_moneySpent_text.color = MainMenu.redColor;
		}
		else if (turnUnitsDestroyedBackup == 0)
		{
			_moneySpent_text.color = MainMenu.greyColor;
		}
		else if (turnUnitsDestroyedBackup < 0)
		{
			_moneySpent_text.text = "+" + -1 * turnUnitsDestroyedBackup;
			_moneySpent_text.color = MainMenu.greenColor;
		}
		lastTurnSummaryData.moneySpent = turnUnitsDestroyedBackup;
		turnUnitsDestroyedBackup = p_player.TurnMoneyEarnedBackup;
		if (p_player != TurnManager.currPlayer)
		{
			turnUnitsDestroyedBackup += p_player.TurnMoneyEarned;
		}
		_moneyEarned_text.text = turnUnitsDestroyedBackup.ToString();
		if (turnUnitsDestroyedBackup > 0)
		{
			_moneyEarned_text.color = MainMenu.greenColor;
		}
		else if (turnUnitsDestroyedBackup == 0)
		{
			_moneyEarned_text.color = MainMenu.greyColor;
		}
		else if (turnUnitsDestroyedBackup < 0)
		{
			_moneyEarned_text.color = MainMenu.redColor;
		}
		lastTurnSummaryData.moneyEarned = turnUnitsDestroyedBackup;
	}
}

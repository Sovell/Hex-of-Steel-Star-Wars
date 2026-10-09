using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Player
{
	public const float UNIT_LEND_LEASE_COST_MODIFIER = 0.35f;

	public const string DEFAULT_FACTION = "Neutral";

	[NonSerialized]
	public bool canAIattackPlayer;

	[NonSerialized]
	public float ratio;

	[NonSerialized]
	public int baseIncome;

	[NonSerialized]
	public int turnIncome;

	[NonSerialized]
	public int upkeepCost;

	[NonSerialized]
	public int tilesDestroyedIncomeLoss;

	[NonSerialized]
	public int ManpowerMax;

	[NonSerialized]
	public int ManpowerUsed;

	[NonSerialized]
	public int GroundForces;

	[NonSerialized]
	public int AirForces;

	[NonSerialized]
	public int NavalForces;

	[NonSerialized]
	public bool isAtWarWithCurrentPlayer;

	[NonSerialized]
	public bool isAlliedWithCurrentPlayer;

	[NonSerialized]
	public bool noRelationWithCurrentPlayer;

	[NonSerialized]
	public bool isAtWarWithHumanPlayer;

	[NonSerialized]
	public bool isAlliedWithHumanPlayer;

	[NonSerialized]
	public bool noRelationWithHumanPlayer;

	[NonSerialized]
	public bool isAtWarWithGivenPlayer;

	[NonSerialized]
	public bool isAlliedWithGivenPlayer;

	[NonSerialized]
	public bool noRelationWithGivenPlayer;

	public Guid? UserId { get; set; }

	public string Name { get; set; }

	public string LocalPassword { get; set; }

	public string Country { get; set; }

	public string Faction { get; set; }

	public string Ideology { get; set; }

	public int Rank { get; set; }

	public int XP { get; set; }

	public int XPNeededForNextRank { get; set; }

	public string ScenarioDescription { get; set; }

	public string ScenarioVictoryText { get; set; }

	public string ScenarioDefeatText { get; set; }

	public short HighCommandPoints { get; set; }

	public int MoneyAmount { get; set; }

	public int FixedIncome { get; set; }

	public byte IncomeModifier { get; set; }

	public int BonusIncome { get; set; }

	public int BonusManpower { get; set; }

	public short NumberOfAvailableResearchPoints { get; set; }

	public short NumberOfPartisansLost { get; set; }

	public int NumberOfVPEverConquered { get; set; }

	public bool IsPlaying { get; set; }

	public bool IsPlayingFirst { get; set; }

	public bool IsEndTurnPanel { get; set; }

	public bool IsComputer { get; set; }

	public bool OOBautoSupplies { get; set; }

	public GameOutcomes GameOutcome { get; set; }

	public List<Policy> ListActivePolicies { get; set; }

	public List<Unit> ListOfUnits { get; set; }

	public List<string> ListOfUnitsResearched { get; set; }

	public List<Tile.Coordinates> ClearedFOW { get; set; }

	public Dictionary<string, float> CurrentUnitsResearch { get; set; }

	public int TotalUnitsBought { get; set; }

	public int TotalUnitsLost { get; set; }

	public int TotalUnitsDestroyed { get; set; }

	public int TotalMoneySpent { get; set; }

	public int TotalMoneyEarned { get; set; }

	public int TurnUnitsBought { get; set; }

	public int TurnUnitsDestroyed { get; set; }

	public int TurnUnitsLost { get; set; }

	public int TurnMoneySpent { get; set; }

	public int TurnMoneyEarned { get; set; }

	public int SafariKillCount { get; set; }

	public float TotalDamageDealt { get; set; }

	public float TotalTilesBombed { get; set; }

	public int TurnUnitsBoughtBackup { get; set; }

	public int TurnUnitsDestroyedBackup { get; set; }

	public int TurnUnitsLostBackup { get; set; }

	public int TurnMoneySpentBackup { get; set; }

	public int TurnMoneyEarnedBackup { get; set; }

	public int EventMoneyToProcess { get; set; }

	public List<LineData> LineDatas { get; set; }

	public Dictionary<ushort, Statistic> Stats { get; set; }

	public HashSet<string> PlayersAtWar { get; set; }

	public HashSet<string> AlliedPlayers { get; set; }

	public Dictionary<string, int> Truces { get; set; }

	public Dictionary<string, int> PurchasedUnits { get; set; }

	public Dictionary<UnitCategories, HashSet<short>> UnitCategoriesQuantity { get; set; }

	public int ManpowerAvailable { get; private set; }

	public Player(string p_name)
	{
		Name = p_name;
		Faction = "Neutral";
		ListOfUnits = new List<Unit>();
		ListOfUnitsResearched = new List<string>();
		CurrentUnitsResearch = new Dictionary<string, float>();
		ListActivePolicies = new List<Policy>();
		PurchasedUnits = new Dictionary<string, int>();
		Truces = new Dictionary<string, int>();
		Stats = new Dictionary<ushort, Statistic>();
		LineDatas = new List<LineData>();
		ClearedFOW = new List<Tile.Coordinates>();
		CheckForMissingUnitsCategories();
		Rank = 1;
		XP = 0;
		IncomeModifier = 10;
	}

	public bool IsLocalClientPlayer()
	{
		if (UserId.HasValue && UserId != Guid.Empty)
		{
			Guid? userId = UserId;
			Guid userId2 = PlayerSettings.Instance.UserId;
			if (!userId.HasValue)
			{
				return false;
			}
			if (!userId.HasValue)
			{
				return true;
			}
			return userId.GetValueOrDefault() == userId2;
		}
		return false;
	}

	public void BackupTurnStatsAndReset()
	{
		TurnUnitsBoughtBackup = TurnUnitsBought;
		TurnUnitsDestroyedBackup = TurnUnitsDestroyed;
		TurnUnitsLostBackup = TurnUnitsLost;
		TurnMoneySpentBackup = TurnMoneySpent;
		TurnMoneyEarnedBackup = TurnMoneyEarned;
		TurnUnitsBought = 0;
		TurnUnitsLost = 0;
		TurnUnitsDestroyed = 0;
		TurnMoneySpent = 0;
		TurnMoneyEarned = 0;
	}

	public void AddTurnStats()
	{
		Statistic statistic = new Statistic();
		statistic.NumberOfUnits = (short)CountOurUnits();
		statistic.MoneyAmount = MoneyAmount;
		statistic.ManpowerAmount = GetFieldedManpower();
		statistic.RealisticManpowerAmount = ManpowerAvailable;
		statistic.IncomeAmount = baseIncome;
		statistic.HQpointsAMount = (byte)HighCommandPoints;
		statistic.VPsAmount = (short)CountOurVPs();
		CountUnitTypesForStatistic(statistic);
		if (!Stats.ContainsKey((ushort)GameData.Instance.map.turnCount))
		{
			Stats.Add((ushort)GameData.Instance.map.turnCount, statistic);
		}
	}

	public void CountUnitTypesForStatistic(Statistic p_stat)
	{
		foreach (Unit listOfUnit in ListOfUnits)
		{
			if (!listOfUnit.IsReserve)
			{
				if (listOfUnit.Type == "Tank" && listOfUnit.MaxMP > 0 && !listOfUnit.IsArmouredTrain)
				{
					p_stat.TanksNumber++;
				}
				if (listOfUnit.FilterType == "FilterBoat" && listOfUnit.MaxMP > 0)
				{
					p_stat.ShipsNumber++;
				}
				if (listOfUnit.Type == "Plane" && listOfUnit.MaxMP > 0)
				{
					p_stat.PlanesNumber++;
				}
				if (listOfUnit.FilterType == "FilterArtillery")
				{
					p_stat.ArtilleryNumber++;
				}
				if (listOfUnit.FilterType == "FilterInfantry")
				{
					p_stat.InfantryNumber++;
				}
			}
		}
	}

	public int CountOurVPs()
	{
		int num = 0;
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (victoryPoint.tile.OwnerName == Name)
			{
				num++;
			}
		}
		return num;
	}

	public int CountOurUnits()
	{
		int num = 0;
		for (int i = 0; i < ListOfUnits.Count; i++)
		{
			if (!ListOfUnits[i].IsReserve)
			{
				num++;
			}
		}
		return num;
	}

	public int CountCurrCoreUnits()
	{
		int num = 0;
		for (int i = 0; i < ListOfUnits.Count; i++)
		{
			if (ListOfUnits[i].IsCoreUnit)
			{
				num++;
			}
		}
		return num;
	}

	public int GetAvailableManpower()
	{
		if (PlayerSettings.Instance.FiniteManpower)
		{
			return ManpowerAvailable;
		}
		return Mathf.Clamp(CalculateMaxManpower() - GetFieldedManpower(), 0, int.MaxValue);
	}

	public int GetFieldedManpower()
	{
		int num = 0;
		GroundForces = 0;
		AirForces = 0;
		NavalForces = 0;
		foreach (Unit listOfUnit in ListOfUnits)
		{
			num += listOfUnit.manpowerCost;
			if (listOfUnit.FilterType == "FilterBoat")
			{
				NavalForces += listOfUnit.manpowerCost;
			}
			else if (listOfUnit.Type == "Plane")
			{
				AirForces += listOfUnit.manpowerCost;
			}
			else
			{
				GroundForces += listOfUnit.manpowerCost;
			}
		}
		ManpowerUsed = num;
		return num;
	}

	public int CalculateMaxManpower()
	{
		ManpowerMax = 0;
		bool flag = HasPolicy(Policies.Type.Early_mobilization);
		bool flag2 = HasPolicy(Policies.Type.Partial_mobilization);
		bool flag3 = HasPolicy(Policies.Type.War_economy);
		bool flag4 = HasPolicy(Policies.Type.Total_mobilization);
		bool flag5 = HasPolicy(Policies.Type.Balanced_Approach);
		bool flag6 = HasPolicy(Policies.Type.Aggressive_Approach);
		bool flag7 = HasPolicy(Policies.Type.Secret_Police);
		bool flag8 = HasPolicy(Policies.Type.Forced_labor);
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (!victoryPoint.tile.IsVictoryPoint || victoryPoint.tile.OwnerName != Name)
			{
				continue;
			}
			bool flag9 = victoryPoint.tile.OriginalCountryOwner != Country;
			int num;
			int num2;
			if (victoryPoint.tile.EditorOverwrite_manpower != 0)
			{
				num = victoryPoint.tile.EditorOverwrite_manpower;
				num2 = num;
				if (flag9)
				{
					num2 /= 5;
				}
			}
			else
			{
				num = victoryPoint.tile.Manpower;
				num2 = num;
				if (flag9)
				{
					num2 = victoryPoint.tile.OccupiedManpower;
				}
			}
			if (flag9)
			{
				if (flag5)
				{
					num2 += (int)Math.Round((float)num * 0.15f, MidpointRounding.ToEven);
				}
				else if (flag6)
				{
					num2 += (int)Math.Round((float)num * 0.08f, MidpointRounding.ToEven);
				}
				else if (flag7)
				{
					num2 += (int)Math.Round((float)num * 0.05f, MidpointRounding.ToEven);
				}
				else if (flag8)
				{
					num2 += (int)Math.Round((float)num * 0.05f, MidpointRounding.ToEven);
				}
			}
			if (victoryPoint.tile.enumType == TileTypes.CITY && victoryPoint.tile.Name == "Village")
			{
				if (flag)
				{
					num2 -= (int)Math.Round((float)num * 0.05f, MidpointRounding.ToEven);
				}
				else if (flag2)
				{
					num2 -= (int)Math.Round((float)num * 0.15f, MidpointRounding.ToEven);
				}
				else if (flag3)
				{
					num2 -= (int)Math.Round((float)num * 0.3f, MidpointRounding.ToEven);
				}
				else if (flag4)
				{
					num2 -= (int)Math.Round((float)num * 0.5f, MidpointRounding.ToEven);
				}
			}
			ManpowerMax += num2;
		}
		ManpowerMax += BonusManpower;
		if (GameData.Instance.map.NumberOfDaysThatGoesByPerTurn > 0)
		{
			float num3 = (float)GameData.Instance.map.NumberOfDaysThatGoesByPerTurn / 30f;
			int num4 = ((!Database.Minors.Contains(Country)) ? ((int)((float)ManpowerMax * 0.02f * num3)) : ((int)((float)ManpowerMax * 0.01f * num3)));
			num4 *= GameData.Instance.map.turnCount - 1;
			ManpowerMax += num4;
		}
		if (PlayersAtWar.Count == 0)
		{
			ManpowerMax /= 2;
		}
		if (HasPolicy(Policies.Type.Conscription1))
		{
			ManpowerMax = (int)Math.Round((float)ManpowerMax * 1.1f, MidpointRounding.ToEven);
		}
		else if (HasPolicy(Policies.Type.Conscription2))
		{
			ManpowerMax = (int)Math.Round((float)ManpowerMax * 1.2f, MidpointRounding.ToEven);
		}
		else if (HasPolicy(Policies.Type.Conscription3))
		{
			ManpowerMax = (int)Math.Round((float)ManpowerMax * 1.3f, MidpointRounding.ToEven);
		}
		return ManpowerMax;
	}

	public int CalculateTurnIncome()
	{
		turnIncome = 0;
		baseIncome = 0;
		tilesDestroyedIncomeLoss = 0;
		bool flag = HasPolicy(Policies.Type.Industrial_overtime);
		bool flag2 = HasPolicy(Policies.Type.Early_mobilization);
		bool flag3 = HasPolicy(Policies.Type.Partial_mobilization);
		bool flag4 = HasPolicy(Policies.Type.War_economy);
		bool flag5 = HasPolicy(Policies.Type.Total_mobilization);
		bool flag6 = HasPolicy(Policies.Type.Minimized_destruction);
		bool flag7 = HasPolicy(Policies.Type.Balanced_Approach);
		bool flag8 = HasPolicy(Policies.Type.Aggressive_Approach);
		bool flag9 = HasPolicy(Policies.Type.Secret_Police);
		bool flag10 = HasPolicy(Policies.Type.Forced_labor);
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (!victoryPoint.tile.IsVictoryPoint || victoryPoint.tile.OwnerName != Name)
			{
				continue;
			}
			bool flag11 = victoryPoint.tile.ownerPlayer.Country != victoryPoint.tile.OriginalCountryOwner;
			int num = ((victoryPoint.tile.EditorOverwrite_income == 0) ? victoryPoint.tile.MoneyProductionPerTurn : victoryPoint.tile.EditorOverwrite_income);
			int num2 = num;
			if (flag11)
			{
				num2 = victoryPoint.tile.OccupiedMoneyProductionPerTurn;
			}
			if (num > 0 && victoryPoint.tile.NumberOfTurnsAtLowProd > 0)
			{
				if (flag6)
				{
					num2 /= 2;
					tilesDestroyedIncomeLoss += num2;
				}
				else
				{
					tilesDestroyedIncomeLoss += num2;
					num2 = 0;
				}
			}
			if (num2 > 0)
			{
				if (victoryPoint.tile.NumberOfTurnsSinceVPGotCaptured <= 3 && GameData.Instance.TryFindPlayerByCountry(victoryPoint.tile.OriginalCountryOwner, out var o_player) && o_player.HasPolicy(Policies.Type.Scorched_earth))
				{
					num2 = 0;
				}
				else
				{
					if (flag)
					{
						if (victoryPoint.tile.enumType == TileTypes.FACTORY || victoryPoint.tile.enumType == TileTypes.REFINERY)
						{
							num2 += (int)Math.Round((float)num * 0.35f, MidpointRounding.ToEven);
						}
						else if (victoryPoint.tile.Name == "Village")
						{
							num2 -= (int)Math.Round((float)num * 0.4f, MidpointRounding.ToEven);
						}
					}
					if (flag11)
					{
						if (flag7)
						{
							num2 += (int)Math.Round((float)num * 0.15f, MidpointRounding.ToEven);
						}
						else if (flag8)
						{
							num2 += (int)Math.Round((float)num * 0.08f, MidpointRounding.ToEven);
						}
						else if (flag9)
						{
							num2 += (int)Math.Round((float)num * 0.05f, MidpointRounding.ToEven);
						}
						else if (flag10)
						{
							num2 += (int)Math.Round((float)num * 0.3f, MidpointRounding.ToEven);
						}
					}
					if (flag2)
					{
						num2 += (int)Math.Round((float)num * 0.05f, MidpointRounding.ToEven);
					}
					else if (flag3)
					{
						num2 += (int)Math.Round((float)num * 0.1f, MidpointRounding.ToEven);
					}
					else if (flag4)
					{
						num2 += (int)Math.Round((float)num * 0.15f, MidpointRounding.ToEven);
					}
					else if (flag5)
					{
						num2 += (int)Math.Round((float)num * 0.25f, MidpointRounding.ToEven);
					}
				}
			}
			turnIncome += num2;
		}
		turnIncome = (int)((float)turnIncome * ((float)(int)IncomeModifier / 10f));
		turnIncome += FixedIncome;
		turnIncome += BonusIncome;
		baseIncome = turnIncome + tilesDestroyedIncomeLoss;
		if (PlayersAtWar.Count == 0)
		{
			turnIncome /= 2;
		}
		if (PlayerSettings.Instance.Upkeep)
		{
			turnIncome += -1 * CalculateTotalUnitsUpkeep(p_applyReserveModifier: true);
		}
		if (IsComputer && Faction == "Neutral")
		{
			turnIncome /= 2;
		}
		return turnIncome;
	}

	public int CalculateTotalUnitsUpkeep(bool p_applyReserveModifier)
	{
		upkeepCost = 0;
		bool flag = PlayersAtWar.Count > 0;
		foreach (Unit listOfUnit in ListOfUnits)
		{
			upkeepCost += listOfUnit.GetUpkeepCost(p_applyReserveModifier & flag);
		}
		if (!flag)
		{
			upkeepCost /= 2;
		}
		return upkeepCost;
	}

	public void StartResearch(Unit p_unit, bool p_sendRPC)
	{
		if (p_unit != null)
		{
			NumberOfAvailableResearchPoints--;
			TurnManager.currPlayer.CurrentUnitsResearch.Add(p_unit.Name, 0f);
			if (p_sendRPC)
			{
				MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_RESEARCH, this);
			}
			UIManager.instance.RefreshTechTreeNotification();
			if (!IsComputer && GameData.Instance.ironManMode && !MultiplayerManager.IsRealtimeMultiplayer())
			{
				SteamManager.UnlockAchievement("Start first research");
				TutorialManager.PerformAction(TutorialActions.START_RESEARCH);
			}
		}
	}

	public void CancelResearch(Unit p_unit, bool p_sendRPC)
	{
		if (p_unit != null)
		{
			NumberOfAvailableResearchPoints++;
			TurnManager.currPlayer.CurrentUnitsResearch.Remove(p_unit.Name);
			if (p_sendRPC)
			{
				MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_RESEARCH, this);
			}
			UIManager.instance.RefreshTechTreeNotification();
		}
	}

	public bool HasPolicy(Policy p_policy)
	{
		if (p_policy == null)
		{
			return false;
		}
		if (!PlayerSettings.Instance.Policies)
		{
			return false;
		}
		return HasPolicy(p_policy.Type);
	}

	public bool HasPolicy(Policies.Type p_type)
	{
		if (!PlayerSettings.Instance.Policies)
		{
			return false;
		}
		return ListActivePolicies.Any((Policy p) => p.Type == p_type);
	}

	public bool HasPolicy(Policies.Categories p_category)
	{
		if (!PlayerSettings.Instance.Policies)
		{
			return false;
		}
		return ListActivePolicies.Any((Policy p) => p.Category == p_category);
	}

	public void PurchasePolicy(Policy p_policy, bool p_sendRPC)
	{
		if (p_policy != null && !HasPolicy(p_policy))
		{
			RemoveHQpoints(p_policy.HighCommandPointsCost, p_sendRPC: false);
			AddPolicy(p_policy, p_sendRPC: false);
			if (p_sendRPC)
			{
				MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_PURCHASE_POLICY, p_policy);
			}
		}
	}

	public void AddPolicy(Policy p_policy, bool p_sendRPC)
	{
		if (p_policy == null || HasPolicy(p_policy))
		{
			return;
		}
		foreach (Policy item in ListActivePolicies.Where((Policy p) => p.Category == p_policy.Category).ToList())
		{
			RemovePolicy(item, p_sendRPC: false);
		}
		ListActivePolicies.Add(p_policy);
		ApplyPolicyEffects(p_policy);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_ADD_POLICY, p_policy);
		}
		if (!IsComputer && (!MultiplayerManager.IsRealtimeMultiplayer() || IsLocalClientPlayer()))
		{
			if (p_policy.Type == Policies.Type.Forced_labor)
			{
				SteamManager.UnlockAchievement("Tyrant");
			}
			if (p_policy.Type == Policies.Type.Conscription3)
			{
				SteamManager.UnlockAchievement("Scraping the barrel");
			}
		}
	}

	public void RemovePolicy(Policy p_policy, bool p_sendRPC)
	{
		if (p_policy != null)
		{
			RemovePolicy(p_policy.Type, p_sendRPC);
		}
	}

	public void RemovePolicy(Policies.Type p_type, bool p_sendRPC)
	{
		int num = ListActivePolicies.FindIndex((Policy p) => p.Type == p_type);
		if (num >= 0)
		{
			Policy policy = ListActivePolicies[num];
			ListActivePolicies.RemoveAt(num);
			RemovePolicyEffects(policy);
			if (p_sendRPC)
			{
				MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_REMOVE_POLICY, policy);
			}
		}
	}

	private void ApplyPolicyEffects(Policy p_policy)
	{
		if (p_policy.Type == Policies.Type.Military_scientists_1)
		{
			NumberOfAvailableResearchPoints++;
		}
		else if (p_policy.Type == Policies.Type.Military_scientists_2)
		{
			NumberOfAvailableResearchPoints += 2;
		}
	}

	private void RemovePolicyEffects(Policy p_policy)
	{
		if (p_policy.Type == Policies.Type.Military_scientists_1)
		{
			NumberOfAvailableResearchPoints--;
		}
		else if (p_policy.Type == Policies.Type.Military_scientists_2)
		{
			NumberOfAvailableResearchPoints -= 2;
		}
		if (HasPolicy(Policies.Type.Strategic_Flexibility))
		{
			AddHQPoints((byte)((float)(int)p_policy.HighCommandPointsCost * 0.3f), p_sendRPC: false);
		}
	}

	public void SetAvailableManpower(int p_amount)
	{
		ManpowerAvailable = p_amount;
		if (ManpowerAvailable < 0)
		{
			ManpowerAvailable = 0;
		}
	}

	public void AddNewTurnManpower()
	{
		AddManpower(GetManpowerGrowth(), p_doMarginCheck: true, p_sendRPC: false);
	}

	public int GetManpowerGrowth()
	{
		int num = CalculateMaxManpower();
		float num2 = 0.03f;
		float num3 = (float)GameData.Instance.map.NumberOfDaysThatGoesByPerTurn / 30f;
		float num4 = 0f;
		if (HasPolicy(Policies.Type.Aggressive_propaganda))
		{
			num4 += 0.3f;
		}
		if (IsComputer)
		{
			switch (PlayerSettings.Instance.DifficultyLevel)
			{
			default:
				num2 *= 1f;
				break;
			case 2:
				num2 *= 2f;
				break;
			case 3:
				num2 *= 3f;
				break;
			case 4:
				num2 *= 4f;
				break;
			}
		}
		return (int)Math.Round((float)num * num2 * num3 * (1f + num4), MidpointRounding.ToEven);
	}

	public void AddManpower(int p_amount, bool p_doMarginCheck, bool p_sendRPC)
	{
		if (p_amount == 0)
		{
			return;
		}
		CalculateMaxManpower();
		GetFieldedManpower();
		if (p_doMarginCheck)
		{
			int num = ManpowerMax - ManpowerUsed - ManpowerAvailable;
			if (num <= 0)
			{
				return;
			}
			if (p_amount > num)
			{
				p_amount = num;
			}
		}
		ManpowerAvailable += p_amount;
		if (ManpowerAvailable > ManpowerMax)
		{
			ManpowerAvailable = ManpowerMax;
		}
		if (!MainMenu.isEditor && PlayerSettings.Instance.FiniteManpower)
		{
			UIManager.instance.RefreshManpower();
			if (p_sendRPC)
			{
				MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_ADD_MANPOWER, p_amount, p_doMarginCheck);
			}
		}
		if (!MainMenu.isEditor && ManpowerAvailable == 0 && !IsComputer && (IsLocalClientPlayer() || !MultiplayerManager.IsRealtimeMultiplayer()))
		{
			SteamManager.UnlockAchievement("Krieg ist verloren");
		}
	}

	public void RemoveManpower(int p_amount, bool p_sendRPC)
	{
		if (p_amount == 0)
		{
			return;
		}
		ManpowerAvailable -= p_amount;
		if (!MainMenu.isEditor && PlayerSettings.Instance.FiniteManpower)
		{
			UIManager.instance.RefreshManpower();
			if (p_sendRPC)
			{
				MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_REMOVE_MANPOWER, p_amount);
			}
		}
		if (!MainMenu.isEditor && ManpowerAvailable == 0 && !IsComputer && (IsLocalClientPlayer() || !MultiplayerManager.IsRealtimeMultiplayer()))
		{
			SteamManager.UnlockAchievement("Krieg ist verloren");
		}
	}

	public void AddMoney(int p_amount, bool p_sendRPC)
	{
		MoneyAmount += p_amount;
		if (MoneyAmount < 0)
		{
			MoneyAmount = 0;
		}
		if (MainMenu.isEditor)
		{
			return;
		}
		TotalMoneyEarned += p_amount;
		TurnMoneyEarned += p_amount;
		UIManager.instance.RefreshMoneyAmountUI();
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_ADD_MONEY, p_amount);
		}
		if (!IsComputer && GameData.Instance.ironManMode && !MultiplayerManager.IsRealtimeMultiplayer())
		{
			if (MoneyAmount >= 5000)
			{
				SteamManager.UnlockAchievement("MoneyIsNice_Bronze");
			}
			if (MoneyAmount >= 10000)
			{
				SteamManager.UnlockAchievement("MoneyIsNice_Silver");
			}
			if (MoneyAmount >= 30000)
			{
				SteamManager.UnlockAchievement("MoneyIsNice_Gold");
			}
		}
	}

	public void RemoveMoney(int p_amount, bool p_sendRPC)
	{
		MoneyAmount -= p_amount;
		if (MoneyAmount < 0)
		{
			MoneyAmount = 0;
		}
		if (MainMenu.isEditor)
		{
			return;
		}
		TotalMoneySpent += p_amount;
		TurnMoneySpent += p_amount;
		if (MoneyAmount < 0)
		{
			MoneyAmount = 0;
		}
		UIManager.instance.RefreshMoneyAmountUI();
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_REMOVE_MONEY, p_amount);
		}
		if (!IsComputer && GameData.Instance.ironManMode && !MultiplayerManager.IsRealtimeMultiplayer())
		{
			if (MoneyAmount >= 5000)
			{
				SteamManager.UnlockAchievement("MoneyIsNice_Bronze");
			}
			if (MoneyAmount >= 10000)
			{
				SteamManager.UnlockAchievement("MoneyIsNice_Silver");
			}
			if (MoneyAmount >= 30000)
			{
				SteamManager.UnlockAchievement("MoneyIsNice_Gold");
			}
		}
	}

	public void AddHQPoints(int p_amount, bool p_sendRPC)
	{
		if (HasPolicy(Policies.Type.Bigger_command_centers))
		{
			HighCommandPoints = (short)Mathf.Clamp(HighCommandPoints + p_amount, -32767, 200);
		}
		else
		{
			HighCommandPoints = (short)Mathf.Clamp(HighCommandPoints + p_amount, -32767, 100);
		}
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_ADD_HQ_POINTS, p_amount);
		}
	}

	public void RemoveHQpoints(int p_amount, bool p_sendRPC)
	{
		if (HasPolicy(Policies.Type.Bigger_command_centers))
		{
			HighCommandPoints = (short)Mathf.Clamp(HighCommandPoints - p_amount, -32767, 200);
		}
		else
		{
			HighCommandPoints = (short)Mathf.Clamp(HighCommandPoints - p_amount, -32767, 100);
		}
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_REMOVE_HQ_POINTS, p_amount);
		}
	}

	public void ChangeFaction(string p_newFactionName, bool p_getNewAllianceWars, bool p_sendRPC)
	{
		Faction = p_newFactionName;
		foreach (string item in AlliedPlayers.ToList())
		{
			if (GameData.Instance.TryFindPlayerByName(item, out var o_player))
			{
				RemoveAllianceWith(o_player, p_sendRPC: false);
			}
		}
		if (Faction == "Neutral")
		{
			foreach (string item2 in PlayersAtWar.ToList())
			{
				if (GameData.Instance.TryFindPlayerByName(item2, out var o_player2))
				{
					RemoveWarWith(o_player2, p_sendRPC: false);
				}
			}
		}
		else
		{
			PlayersAtWar = new HashSet<string>();
			AlliedPlayers = new HashSet<string>();
			foreach (Player listOfPlayer in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer.IsAtWarWith(this))
				{
					listOfPlayer.RemoveWarWith(this, p_sendRPC: false);
				}
			}
			foreach (Player listOfPlayer2 in GameData.Instance.listOfPlayers)
			{
				if (listOfPlayer2.Name == Name)
				{
					continue;
				}
				if (listOfPlayer2.Faction == Faction)
				{
					AddAllianceWith(listOfPlayer2, p_sendRPC: false);
				}
				if (!IsAlliedWith(listOfPlayer2))
				{
					continue;
				}
				foreach (string item3 in listOfPlayer2.PlayersAtWar.ToList())
				{
					Player o_player3;
					if (item3 == Name)
					{
						listOfPlayer2.RemoveWarWith(this, p_sendRPC: false);
					}
					else if (p_getNewAllianceWars && GameData.Instance.TryFindPlayerByName(item3, out o_player3))
					{
						AddWarWith(o_player3, p_actualDirectWarDeclaration: false, p_sendRPC: false);
					}
				}
			}
		}
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_FACTION_CHANGE, Faction, p_getNewAllianceWars);
		}
		if (this == TurnManager.currPlayer)
		{
			UIManager.instance.RefreshManpower();
		}
	}

	public bool IsAtWarWith(Player p_player)
	{
		if (p_player == null)
		{
			return false;
		}
		if (p_player.Name == Name)
		{
			return false;
		}
		return PlayersAtWar.Contains(p_player.Name);
	}

	public void AddWarWith(Player p_player, bool p_actualDirectWarDeclaration, bool p_sendRPC)
	{
		if (p_player == this)
		{
			return;
		}
		if (Country == "Soviet Union" && p_player.Country == "Nazi Germany")
		{
			AI.playerNeedsToRedeployItsUnitsNow = this;
		}
		bool flag = false;
		bool flag2 = false;
		if (!IsAtWarWith(p_player))
		{
			bool flag3 = PlayersAtWar.Count == 0;
			flag = true;
			PlayersAtWar.Add(p_player.Name);
			if (flag3)
			{
				AddManpower(Math.Clamp(CalculateMaxManpower() - GetFieldedManpower(), 0, CalculateMaxManpower()), p_doMarginCheck: true, p_sendRPC: true);
			}
		}
		if (!p_player.IsAtWarWith(this))
		{
			bool flag4 = p_player.PlayersAtWar.Count == 0;
			flag2 = true;
			p_player.PlayersAtWar.Add(Name);
			if (flag4)
			{
				p_player.AddManpower(Math.Clamp(p_player.CalculateMaxManpower() - p_player.GetFieldedManpower(), 0, p_player.CalculateMaxManpower()), p_doMarginCheck: true, p_sendRPC: true);
			}
		}
		if (p_actualDirectWarDeclaration)
		{
			foreach (Event @event in GameData.Instance.map.Events)
			{
				if (@event.CheckType == CheckTypes.WAR_DECLARATION && Name == @event.Player1Check && p_player.Name == @event.Player2Check)
				{
					TurnManager.instance.ProcessSingleEvent(@event);
				}
			}
		}
		if (IsAlliedWith(p_player))
		{
			RemoveAllianceWith(p_player, p_sendRPC: false);
		}
		if (p_player.IsAlliedWith(this))
		{
			p_player.RemoveAllianceWith(this, p_sendRPC: false);
		}
		if (HasTruceWith(p_player))
		{
			RemoveTruceWith(p_player, p_sendRPC: false);
		}
		if (p_player.HasTruceWith(this))
		{
			p_player.RemoveTruceWith(this, p_sendRPC: false);
		}
		if (p_sendRPC)
		{
			if (flag)
			{
				MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, this);
			}
			if (flag2)
			{
				MultiplayerManager.RPC("RPC_SyncPlayerData", p_player.Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, p_player);
			}
		}
	}

	public void RemoveWarWith(Player p_player, bool p_sendRPC)
	{
		PlayersAtWar.Remove(p_player.Name);
		p_player.PlayersAtWar.Remove(Name);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, this);
			MultiplayerManager.RPC("RPC_SyncPlayerData", p_player.Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, p_player);
		}
	}

	public bool IsAlliedWith(Player p_player)
	{
		if (p_player == null)
		{
			return false;
		}
		if (p_player.Name == Name)
		{
			return true;
		}
		return AlliedPlayers.Contains(p_player.Name);
	}

	public void AddAllianceWith(Player p_player, bool p_sendRPC)
	{
		if (!IsAlliedWith(p_player))
		{
			AlliedPlayers.Add(p_player.Name);
		}
		if (!p_player.IsAlliedWith(this))
		{
			p_player.AlliedPlayers.Add(Name);
		}
		if (IsAtWarWith(p_player))
		{
			RemoveWarWith(p_player, p_sendRPC: false);
		}
		foreach (Event @event in GameData.Instance.map.Events)
		{
			if (@event.CheckType == CheckTypes.ALLIANCE_MADE && Name == @event.Player1Check && p_player.Name == @event.Player2Check)
			{
				TurnManager.instance.ProcessSingleEvent(@event);
			}
		}
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, this);
			MultiplayerManager.RPC("RPC_SyncPlayerData", p_player.Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, p_player);
		}
	}

	public void RemoveAllianceWith(Player p_player, bool p_sendRPC)
	{
		AlliedPlayers.Remove(p_player.Name);
		p_player.AlliedPlayers.Remove(Name);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, this);
			MultiplayerManager.RPC("RPC_SyncPlayerData", p_player.Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, p_player);
		}
	}

	public bool HasTruceWith(Player p_player)
	{
		if (p_player.Name == Name)
		{
			return true;
		}
		return Truces.ContainsKey(p_player.Name);
	}

	public void AddTruceWith(Player p_player, bool p_sendRPC)
	{
		int value = 30;
		if (GameData.Instance.map.NumberOfDaysThatGoesByPerTurn > 0)
		{
			value = 182 / GameData.Instance.map.NumberOfDaysThatGoesByPerTurn;
		}
		if (!HasTruceWith(p_player))
		{
			Truces.Add(p_player.Name, value);
		}
		if (!p_player.HasTruceWith(this))
		{
			p_player.Truces.Add(Name, value);
		}
		if (IsAtWarWith(p_player))
		{
			RemoveWarWith(p_player, p_sendRPC: false);
		}
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, this);
			MultiplayerManager.RPC("RPC_SyncPlayerData", p_player.Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, p_player);
		}
	}

	public void RemoveTruceWith(Player p_player, bool p_sendRPC)
	{
		Truces.Remove(p_player.Name);
		p_player.Truces.Remove(Name);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, this);
			MultiplayerManager.RPC("RPC_SyncPlayerData", p_player.Name, MultiplayerManager.SyncActions.PLAYER_DIPLOMACY, p_player);
		}
	}

	public void PurchaseUnit(Unit p_unit, bool p_sendRPC)
	{
		if (PlayerSettings.Instance.ProductionTakesTime)
		{
			p_unit.SetDefaultProductionTime();
		}
		AddUnit(p_unit, p_sendRPC: false);
		if (PurchasedUnits.ContainsKey(p_unit.Name))
		{
			PurchasedUnits[p_unit.Name]++;
		}
		else
		{
			PurchasedUnits.Add(p_unit.Name, 1);
		}
		if (MainMenu.isEditor)
		{
			return;
		}
		RemoveManpower(p_unit.manpowerCost, p_sendRPC: false);
		if (p_unit.IsCommander && PlayerSettings.Instance.Policies)
		{
			RemoveHQpoints((short)p_unit.Cost, p_sendRPC: false);
		}
		else
		{
			RemoveMoney(p_unit.Cost, p_sendRPC: false);
		}
		if (p_unit.Country != Country && GameData.Instance.TryFindPlayerByCountry(p_unit.Country, out var o_player))
		{
			new Notification(NotificationTypes.UNIT_PURCHASE, o_player.Name, 0f, 0f, p_unit.Name, p_isImportant: true).SenderName = Name;
			o_player.AddMoney((int)((float)p_unit.Cost * 0.35f), p_sendRPC: false);
			if (!IsComputer && (IsLocalClientPlayer() || !MultiplayerManager.IsRealtimeMultiplayer()))
			{
				SteamManager.UnlockAchievement("Parasite");
			}
		}
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_PURCHASE_UNIT, p_unit, p_unit.Cost, p_unit.manpowerCost);
		}
	}

	public void AddUnit(Unit p_unit, bool p_sendRPC)
	{
		p_unit.OwnerName = Name;
		p_unit.OwnerPlayer = this;
		AssignUnitCategoryID(p_unit);
		p_unit.SetUnitCustomName();
		ListOfUnits.Add(p_unit);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_ADD_UNIT, p_unit);
		}
	}

	public void AssignUnitCategoryID(Unit p_unit)
	{
		UnitCategories unitCategory = p_unit.GetUnitCategory();
		HashSet<short> hashSet = UnitCategoriesQuantity[unitCategory];
		for (short num = 1; num < short.MaxValue; num++)
		{
			if (!hashSet.Contains(num))
			{
				p_unit.CategoryID = num;
				UnitCategoriesQuantity[unitCategory].Add(num);
				return;
			}
		}
		p_unit.CategoryID = 0;
	}

	public void RemoveUnit(Unit p_unit, bool p_sendRPC)
	{
		for (int i = 0; i < ListOfUnits.Count; i++)
		{
			if (ListOfUnits[i].ID == p_unit.ID && (!PlayerSettings.Instance.UnitSuffix || (PlayerSettings.Instance.UnitSuffix && p_unit.CategoryID == ListOfUnits[i].CategoryID)))
			{
				Unit unit = ListOfUnits[i];
				if (p_sendRPC)
				{
					MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_REMOVE_UNIT, unit);
				}
				RemoveUnitCategoryID(unit);
				ListOfUnits.RemoveAt(i);
				break;
			}
		}
	}

	public void RemoveUnitCategoryID(Unit p_unit)
	{
		UnitCategories unitCategory = p_unit.GetUnitCategory();
		UnitCategoriesQuantity[unitCategory].Remove(p_unit.CategoryID);
		p_unit.CategoryID = 0;
	}

	public void SetupIdeology()
	{
		if (string.IsNullOrEmpty(Ideology))
		{
			switch (Country)
			{
			case "USA":
			case "Great Britain":
			case "France":
			case "Canada":
			case "Belgium":
			case "Netherlands":
			case "Denmark":
			case "Norway":
			case "Greece":
			case "Poland":
			case "Yugoslavia":
			case "Australia":
			case "New Zealand":
			case "Luxembourg":
			case "Philippines":
			case "British Raj":
			case "Mexico":
			case "Ethiopia":
			case "Estonia":
			case "Latvia":
			case "Lithuania":
			case "Spanish Republic":
			case "Sweden":
			case "Ireland":
			case "China":
				Ideology = "Democracy";
				break;
			case "Nazi Germany":
				Ideology = "National Socialism";
				break;
			case "Italy":
			case "Japan":
			case "Finland":
			case "Manchukuo":
			case "Bulgaria":
			case "Romania":
			case "Slovakia":
			case "Croatia":
			case "Thailand":
			case "Hungary":
			case "Spain":
			case "Vichy France":
				Ideology = "Fascism";
				break;
			case "Switzerland":
			case "Portugal":
			case "Turkey":
			case "Iran":
			case "Iraq":
			case "Saudi Arabia":
			case "Iceland":
				Ideology = "Neutral";
				break;
			case "Soviet Union":
			case "Communist China":
			case "Cuba":
				Ideology = "Communism";
				break;
			default:
				Ideology = "None";
				break;
			}
		}
	}

	public bool IsNaturalAllyOf(Player p_player)
	{
		if ((Ideology == "National Socialism" || Ideology == "Fascism") && (p_player.Ideology == "Fascism" || p_player.Ideology == "National Socialism"))
		{
			return true;
		}
		return p_player.Ideology == Ideology;
	}

	public bool AreThereUnitsReadyForDeployment()
	{
		if (ListOfUnits.Count == 0)
		{
			return false;
		}
		bool productionTakesTime = PlayerSettings.Instance.ProductionTakesTime;
		foreach (Unit listOfUnit in ListOfUnits)
		{
			if (listOfUnit.IsReserve && (listOfUnit.TurnToProduce == 0 || !productionTakesTime))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasResearchedAllUnits()
	{
		if (!PlayerSettings.Instance.TechTree)
		{
			return true;
		}
		foreach (Unit allUnit in Utils.GetAllUnits())
		{
			if (!(Country == allUnit.Country) || allUnit.IsPrototype || ListOfUnitsResearched.Contains(allUnit.Name))
			{
				continue;
			}
			bool flag = false;
			foreach (string key in CurrentUnitsResearch.Keys)
			{
				if (key == allUnit.Name)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public void AddLineData(LineData p_lineData, bool p_sendRPC)
	{
		LineDatas.Add(p_lineData);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_DRAWINGS, LineDatas);
		}
	}

	public void RemoveLineData(LineData p_lineData, bool p_sendRPC)
	{
		LineDatas.Remove(p_lineData);
		UnityEngine.Object.Destroy(p_lineData.lineObject);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncPlayerData", Name, MultiplayerManager.SyncActions.PLAYER_DRAWINGS, LineDatas);
		}
	}

	public void CacheAllianceAndWarForGivenPlayer(Player p_player)
	{
		isAtWarWithGivenPlayer = IsAtWarWith(p_player);
		isAlliedWithGivenPlayer = IsAlliedWith(p_player);
		noRelationWithGivenPlayer = !isAtWarWithGivenPlayer && !isAlliedWithGivenPlayer;
	}

	public void SetDefaultIncomeModifier()
	{
		if (IsComputer)
		{
			if (PlayerSettings.Instance.DifficultyLevel <= 1)
			{
				IncomeModifier = 5;
			}
			else if (PlayerSettings.Instance.DifficultyLevel <= 2)
			{
				IncomeModifier = 10;
			}
			else if (PlayerSettings.Instance.DifficultyLevel == 3)
			{
				IncomeModifier = 20;
			}
			else if (PlayerSettings.Instance.DifficultyLevel >= 4)
			{
				IncomeModifier = 30;
			}
		}
		else
		{
			IncomeModifier = 10;
		}
	}

	public bool HasReachedMaxPurchaseLimit(Unit p_unit)
	{
		foreach (KeyValuePair<string, int> purchasedUnit in PurchasedUnits)
		{
			if (purchasedUnit.Key == p_unit.Name && purchasedUnit.Value >= p_unit.MaxPurchaseTime)
			{
				return true;
			}
		}
		return false;
	}

	public void SetDefaultResearchPointsAmount(List<Unit> p_gameUnits)
	{
		int num = 0;
		foreach (Unit p_gameUnit in p_gameUnits)
		{
			if (p_gameUnit.Country == Country)
			{
				num++;
				if (p_gameUnit.ProductionMonth == 0)
				{
					p_gameUnit.ProductionMonth = 1;
				}
				DateTime dateTime;
				try
				{
					dateTime = new DateTime(p_gameUnit.ProductionYear, p_gameUnit.ProductionMonth, 1);
				}
				catch
				{
					dateTime = new DateTime(GameData.Instance.map.Date.Year, 1, 1);
				}
				if (dateTime <= GameData.Instance.map.Date && !ListOfUnitsResearched.Contains(p_gameUnit.Name))
				{
					ListOfUnitsResearched.Add(p_gameUnit.Name);
				}
			}
		}
		if (num != 0)
		{
			NumberOfAvailableResearchPoints = (short)(num / 35);
		}
		if (NumberOfAvailableResearchPoints == 0)
		{
			NumberOfAvailableResearchPoints = 1;
		}
	}

	public void DestroyAndResetAllUnits(Player p_playerTakingOver)
	{
		foreach (Unit listOfUnit in ListOfUnits)
		{
			if (!(listOfUnit.unitGO != null))
			{
				continue;
			}
			if (listOfUnit.Type == "Plane")
			{
				listOfUnit.unitGO.tileGO.airUnitGO = null;
				listOfUnit.unitGO.tileGO.tile.AirUnit = null;
			}
			else
			{
				listOfUnit.unitGO.tileGO.groundUnitGO = null;
				listOfUnit.unitGO.tileGO.tile.GroundUnit = null;
			}
			UnityEngine.Object.Destroy(listOfUnit.unitGO.gameObject);
			RemoveUnitCategoryID(listOfUnit);
			if (p_playerTakingOver == null)
			{
				continue;
			}
			int num = -1;
			if (listOfUnit.MaxMP > 0)
			{
				if (listOfUnit.FilterType == "FilterFighter" || listOfUnit.FilterType == "FilterCAS" || listOfUnit.FilterType == "FilterBomber")
				{
					listOfUnit.TurnToProduce = 3;
					num = UnityEngine.Random.Range(0, 6);
				}
				else if (listOfUnit.Type == "Artillery" || listOfUnit.Type == "AA")
				{
					listOfUnit.TurnToProduce = 2;
					num = UnityEngine.Random.Range(0, 6);
				}
				else if (listOfUnit.Type == "Tank")
				{
					listOfUnit.TurnToProduce = 3;
					num = UnityEngine.Random.Range(0, 6);
				}
				else if (listOfUnit.FilterType == "FilterBoat")
				{
					listOfUnit.TurnToProduce = 5;
					num = UnityEngine.Random.Range(0, 6);
				}
			}
			if (num == 0)
			{
				listOfUnit.IsReserve = true;
				listOfUnit.CurrHP = listOfUnit.MaxHP / 4;
				listOfUnit.AddToHistory(LocalizationManager.Translate("history-surrendered"), p_sendRPC: false);
				p_playerTakingOver.AddUnit(listOfUnit, p_sendRPC: false);
			}
		}
		ListOfUnits = new List<Unit>();
		UnitCategoriesQuantity = null;
		CheckForMissingUnitsCategories();
		if (p_playerTakingOver != null)
		{
			ReservePanelGO.instance.RefreshDeployNotification();
		}
	}

	public void CheckForMissingUnitsCategories()
	{
		if (UnitCategoriesQuantity == null)
		{
			UnitCategoriesQuantity = new Dictionary<UnitCategories, HashSet<short>>();
		}
		foreach (UnitCategories value in Enum.GetValues(typeof(UnitCategories)))
		{
			if (!UnitCategoriesQuantity.ContainsKey(value))
			{
				UnitCategoriesQuantity.Add(value, new HashSet<short>());
			}
		}
	}

	public bool HasReachedUnitLimitCap(Unit p_unit)
	{
		if (p_unit.MaxPurchaseTime <= 0)
		{
			return false;
		}
		int num = 0;
		foreach (Unit listOfUnit in ListOfUnits)
		{
			if (listOfUnit.MaxPurchaseTime > 0 && listOfUnit.Name == p_unit.Name)
			{
				num++;
			}
		}
		return num >= p_unit.MaxPurchaseTime;
	}

	public bool IsMajor()
	{
		return Database.Majors.Contains(Country);
	}

	public bool IsMinor()
	{
		return Database.Minors.Contains(Country);
	}

	public bool IsNavalMajor()
	{
		return Database.NavalMajors.Contains(Country);
	}

	public bool IsNavalMinor()
	{
		return Database.NavalMinors.Contains(Country);
	}

	public int GetFactionChangeCost(string p_targetFaction)
	{
		if (Faction == "Neutral")
		{
			return baseIncome;
		}
		int num = 0;
		foreach (string item in PlayersAtWar)
		{
			if (GameData.Instance.TryFindPlayerByName(item, out var o_player) && GameData.Instance.map.IsPlayerOnTheMap(o_player))
			{
				num++;
			}
		}
		int num2 = 5;
		if (p_targetFaction == "Neutral")
		{
			num2 = 3;
		}
		return baseIncome * num2 + num * baseIncome;
	}

	public List<BorderData> GetBorders(Map p_map, bool p_sortedByLongestFirst = false)
	{
		Dictionary<Player, BorderData> dictionary = new Dictionary<Player, BorderData>(GameData.Instance.listOfPlayers.Count - 1);
		Tile[,] tilesTable = p_map.TilesTable;
		int sizeX = p_map.SizeX;
		int sizeY = p_map.SizeY;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				Tile tile = tilesTable[i, j];
				if (tile == null || tile.terrainOwner != this)
				{
					continue;
				}
				List<Tile> neighbours = tile.neighbours;
				if (neighbours == null || neighbours.Count == 0)
				{
					continue;
				}
				for (int k = 0; k < neighbours.Count; k++)
				{
					Tile tile2 = neighbours[k];
					if (tile2 == null)
					{
						continue;
					}
					Player terrainOwner = tile2.terrainOwner;
					if (terrainOwner != null && terrainOwner != this)
					{
						if (!dictionary.TryGetValue(terrainOwner, out var value))
						{
							value = new BorderData(terrainOwner);
							dictionary.Add(terrainOwner, value);
						}
						value.EdgeCount++;
						value.AddMy(tile);
						value.AddTheir(tile2);
					}
				}
			}
		}
		if (p_sortedByLongestFirst)
		{
			List<BorderData> list = dictionary.Values.ToList();
			list.Sort((BorderData a, BorderData b) => b.EdgeCount.CompareTo(a.EdgeCount));
			return list;
		}
		return new List<BorderData>(dictionary.Values);
	}

	public int GetTotalSuppliesCost(bool p_autoSuppliesOnly)
	{
		int num = 0;
		foreach (Unit listOfUnit in ListOfUnits)
		{
			if (!(listOfUnit.unitGO == null) && (!p_autoSuppliesOnly || listOfUnit.AutoSupplies))
			{
				bool flag = listOfUnit.CurrHP < listOfUnit.MaxHP;
				bool flag2 = listOfUnit.CurrAmmo < listOfUnit.MaxAmmo;
				bool flag3 = listOfUnit.MaxMP > 0 && listOfUnit.CurrAutonomy < listOfUnit.MaxAutonomy;
				if (flag || flag2 || flag3)
				{
					listOfUnit.unitGO.CalculateSupplyRoute(p_drawSupplyLine: false);
					num += listOfUnit.GetHealCost();
					num += listOfUnit.GetResupplyAmmoAndFuelCost();
				}
			}
		}
		return num;
	}

	public bool CanControlUnitsOf(Player p_player)
	{
		if (this == p_player)
		{
			return true;
		}
		if (IsComputer && !p_player.IsComputer)
		{
			return false;
		}
		if (p_player.IsComputer)
		{
			return false;
		}
		if (MultiplayerManager.IsRealtimeMultiplayer())
		{
			return false;
		}
		if (GameData.Instance.isPBEM)
		{
			return false;
		}
		_ = GameData.Instance.offlinePBEM;
		return false;
	}

	public bool ShouldGuardBorders()
	{
		if (Faction != "Neutral")
		{
			return false;
		}
		return true;
	}

	public int GetDangerLevel(Player p_otherPlayer)
	{
		int num = 0;
		if (!IsComputer)
		{
			num++;
		}
		if (p_otherPlayer.PlayersAtWar.Count > 0)
		{
			num++;
		}
		if (PlayerSettings.Instance.ISHistoricalFocus && !IsNaturalAllyOf(p_otherPlayer))
		{
			switch (Ideology)
			{
			case "National Socialism":
				num = 4;
				break;
			case "Fascism":
				num = 3;
				break;
			case "Communism":
				num = 2;
				break;
			case "Democracy":
				num = 1;
				break;
			case "Neutral":
				num = 0;
				break;
			default:
				num = 0;
				break;
			}
		}
		return num;
	}

	public int GetAllianceInvitationCost(Player p_playerToInvite)
	{
		if (p_playerToInvite == null)
		{
			return 0;
		}
		if (GameData.Instance.map == null)
		{
			return 0;
		}
		int num = FixedIncome + BonusIncome;
		int num2 = p_playerToInvite.FixedIncome + p_playerToInvite.BonusIncome;
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (victoryPoint.tile.OwnerName == Name)
			{
				num += victoryPoint.tile.MoneyProductionPerTurn;
			}
			else if (victoryPoint.tile.OwnerName == p_playerToInvite.Name)
			{
				num2 += victoryPoint.tile.MoneyProductionPerTurn;
			}
		}
		int num3 = num + num2;
		if (GameData.Instance.map.IsTutorial)
		{
			if (IsNaturalAllyOf(p_playerToInvite))
			{
				num3 /= 2;
			}
			else
			{
				num3 = ((!(p_playerToInvite.Ideology == "Neutral")) ? (num3 * 4) : (num3 * 2));
			}
		}
		else if (PlayersAtWar.Except(p_playerToInvite.PlayersAtWar).Any())
		{
			if (p_playerToInvite.IsNaturalAllyOf(this))
			{
				num3 *= 2;
			}
			else
			{
				num3 = ((!(p_playerToInvite.Ideology == "Neutral")) ? (num3 + num2 * 10) : (num3 + num2 * 5));
			}
		}
		else if (IsNaturalAllyOf(p_playerToInvite))
		{
			num3 /= 2;
		}
		else
		{
			num3 = ((!(p_playerToInvite.Ideology == "Neutral")) ? (num3 * 3) : (num3 * 2));
		}
		if (HasPolicy(Policies.Type.Good_diplomats))
		{
			num3 -= (int)((float)num3 * 0.3f);
		}
		return num3;
	}

	public bool IsDefeated()
	{
		if (GameData.Instance.map == null)
		{
			return false;
		}
		foreach (TileGO victoryPoint in MapGO.victoryPoints)
		{
			if (victoryPoint.tile.OwnerName == Name)
			{
				return false;
			}
		}
		return true;
	}
}

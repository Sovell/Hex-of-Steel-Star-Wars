using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

[Serializable]
public class Unit
{
	[NonSerialized]
	public static byte MAX_LEVEL = 5;

	[NonSerialized]
	public static byte NEW_HP_MAX_PER_LEVEL = 10;

	[NonSerialized]
	public static byte MORALE_ELITE = 100;

	[NonSerialized]
	public static byte MORALE_DEFAULT = 80;

	[NonSerialized]
	public static float RETREAT_THRESHOLD = 0.3f;

	[NonSerialized]
	public string BaseType;

	[NonSerialized]
	public short BaseArmour;

	[NonSerialized]
	public int BaseMaxHP;

	[NonSerialized]
	public byte BaseMaxMP;

	[NonSerialized]
	public short BaseSoftDamage;

	[NonSerialized]
	public short BaseHardDamage;

	[NonSerialized]
	public short BaseAirDamage;

	[NonSerialized]
	public byte BaseVisibility;

	[NonSerialized]
	public byte BaseRange;

	[NonSerialized]
	public short BaseMaxAmmo;

	[NonSerialized]
	public float UpkeepValue;

	[NonSerialized]
	public bool hasBeenAttackedThisTurn;

	[NonSerialized]
	public bool OthersNeedUsAsBridge;

	[NonSerialized]
	public bool OthersNeedUsAsBridgeNotArtificially;

	[NonSerialized]
	public int baseUnitCost;

	[NonSerialized]
	public int veterancyOptionCost;

	[NonSerialized]
	public int manpowerCost;

	[NonSerialized]
	public int swapeeUnitID;

	[NonSerialized]
	public bool shouldMoveOutNow;

	[NonSerialized]
	public bool shouldPerformDirectSwap;

	[NonSerialized]
	public bool willReplaceUnitMovingOut1;

	[NonSerialized]
	public bool willReplaceUnitMovingOut2;

	[NonSerialized]
	public string nameToLower;

	[NonSerialized]
	public int baseDamage;

	[NonSerialized]
	public int baseDamageMinusArmour;

	[NonSerialized]
	public int FinalDamage;

	[NonSerialized]
	public UnitGO unitGO;

	[NonSerialized]
	public Player OwnerPlayer;

	[NonSerialized]
	public DamageTypes DamageDealingTypeForCommander;

	[NonSerialized]
	public float SupplyPercentage;

	[NonSerialized]
	public int HPbreakdown;

	[NonSerialized]
	public int baseDamageBreakdown;

	[NonSerialized]
	public int veterancyBreakdown;

	[NonSerialized]
	public int moraleBreakdown;

	[NonSerialized]
	public int heroBreakdown;

	[NonSerialized]
	public int generalBreakdown;

	[NonSerialized]
	public int entrenchmentBreakdown;

	[NonSerialized]
	public int reconBreakdown;

	[NonSerialized]
	public int combinedArmsBreakdown;

	[NonSerialized]
	public int encirclementBreakdown;

	[NonSerialized]
	public int flamethrowerBreakdown;

	[NonSerialized]
	public int riverBreakdown;

	[NonSerialized]
	public int terrainBreakdown;

	[NonSerialized]
	public int hillBreakdown;

	[NonSerialized]
	public int landingBreakdown;

	[NonSerialized]
	public int mountaineerBreakdown;

	[NonSerialized]
	public int subVSlandingCraftBreakdown;

	[NonSerialized]
	public int shipVSgroundUnitsBreakdown;

	[NonSerialized]
	public int heavyBomberVSshipsWithoutTorpedoBreakdown;

	[NonSerialized]
	public int torpedoBreakdown;

	[NonSerialized]
	public int destroyerVSsubmergedSubBreakdown;

	[NonSerialized]
	public int armourBreakdown;

	[NonSerialized]
	public int terrainArmouredBreakdown;

	[NonSerialized]
	public int policyBreakdown;

	[NonSerialized]
	public int politicalUnitsBreakdown;

	[NonSerialized]
	public int biomeBreakdown;

	[NonSerialized]
	public int repeatedAttacksBreakdown;

	[NonSerialized]
	public int weatherBreakdown;

	[NonSerialized]
	public int othersBreakdown;

	public int ID { get; set; }

	public string Name { get; set; }

	public string Country { get; set; }

	public Hero Hero { get; set; }

	public string Type { get; set; }

	public string FilterType { get; set; }

	public string AttackSound { get; set; }

	public string MovementSound { get; set; }

	public short ProductionYear { get; set; }

	public byte ProductionMonth { get; set; }

	public short EndUseYear { get; set; }

	public short Armour { get; set; }

	public int MaxHP { get; set; }

	public int CurrHP { get; set; }

	public byte MaxMP { get; set; }

	public byte CurrMP { get; set; }

	public short SoftDamage { get; set; }

	public short HardDamage { get; set; }

	public short AirDamage { get; set; }

	public byte Visibility { get; set; }

	public byte Range { get; set; }

	public short MaxAmmo { get; set; }

	public short CurrAmmo { get; set; }

	public short MaxAutonomy { get; set; }

	public short CurrAutonomy { get; set; }

	public short BaseMaxAutonomy { get; set; }

	public int Cost { get; set; }

	public int CustomCost { get; set; }

	public int ProgressiveCost { get; set; }

	public short AIPurchaseChanceOverride { get; set; }

	public float CustomUpkeepValue { get; set; }

	public int CustomManpowerCost { get; set; }

	public byte CustomTurnProduction { get; set; }

	public short MaxPurchaseTime { get; set; }

	public byte? HPmodifierOnScenarioStart { get; set; }

	public byte? EntrenchmentLevelOnStart { get; set; }

	public FacingRotations FacingRotation { get; set; }

	public AllowedRotations AllowedRotation { get; set; }

	[Obsolete("This field is no longer used and will be removed in future versions. Please use the 'AvailabilityOption' property instead.")]
	public bool IsRemoved { get; set; }

	public AvailabilityOptions AvailabilityOption { get; set; }

	public bool IsCommander { get; set; }

	public bool IsParatrooper { get; set; }

	public bool IsEngineers { get; set; }

	public bool IsSubmarine { get; set; }

	public bool IsCatapultPlane { get; set; }

	public bool CanCatapultPlanes { get; set; }

	public bool IsCarrier { get; set; }

	public bool IsDestroyer { get; set; }

	public bool CanLayMines { get; set; }

	public bool IsSupplyShip { get; set; }

	public bool IsConvoy { get; set; }

	public bool IsRADAR { get; set; }

	public bool HasSonar { get; set; }

	public bool IsSupplyDepot { get; set; }

	public bool IsPartisans { get; set; }

	public bool IsMineFlail { get; set; }

	public bool IsPatrolBoat { get; set; }

	public bool HasFlamethrower { get; set; }

	public bool CanCarryRockets { get; set; }

	public bool HasRockets { get; set; }

	public bool CanCarryBombs { get; set; }

	public bool HasBombs { get; set; }

	public bool CanCarryDropTank { get; set; }

	public bool HasDropTank { get; set; }

	public bool CanCarryTorpedo { get; set; }

	public bool HasTorpedo { get; set; }

	public bool CanCarryABomb { get; set; }

	public bool HasABomb { get; set; }

	public bool CanBeRefueledOnCarrier { get; set; }

	public bool IsPrototype { get; set; }

	public bool IsArmouredTrain { get; set; }

	public bool IsRecon { get; set; }

	public bool IsMine { get; set; }

	public bool IsMissile { get; set; }

	public bool IsLaunchSite { get; set; }

	public bool IsCavalry { get; set; }

	public bool IsPoliticalUnit { get; set; }

	public bool NoAmmoTurn1 { get; set; }

	public bool NoEntrenchmentTurn1 { get; set; }

	public bool BuiltByEngineers { get; set; }

	public bool TileBuilding { get; set; }

	public bool CoastalBattery { get; set; }

	public bool IsPontoonBridge { get; set; }

	public bool IsArmourObstacle { get; set; }

	public bool IsAmphibious { get; set; }

	public bool NoLendLease { get; set; }

	public bool NoCapture { get; set; }

	public bool PermanentDeath { get; set; }

	public bool CanAttackShips { get; set; }

	public bool ImmuneToWinterDamage { get; set; }

	public bool MoveOrAttack { get; set; }

	public string CommanderTargets { get; set; }

	public byte Lvl { get; set; }

	public short XP { get; set; }

	public short CategoryID { get; set; }

	public byte Morale { get; set; }

	public byte TurnToProduce { get; set; }

	public byte EntrenchmentLevel { get; set; }

	public byte TampMaxMP { get; set; }

	public byte TampMP { get; set; }

	public short TampMaxAutonomy { get; set; }

	public short TampAutonomy { get; set; }

	public byte ActionPoints { get; set; }

	public bool HasMoved { get; set; }

	public bool HasAttacked { get; set; }

	public bool HasBuilt { get; set; }

	public bool HasResuppliedAmmo { get; set; }

	public bool HasResuppliedFuel { get; set; }

	public byte NumberOfTurnsUnderWater { get; set; }

	public byte NumberOfTilesMovedDuringCurrentTurn { get; set; }

	public bool IsSubmerged { get; set; }

	public bool HasOverrun { get; set; }

	public bool IsHorsed { get; set; }

	public bool IsMotorized { get; set; }

	public bool IsMechanized { get; set; }

	public bool IsEliteUnit { get; set; }

	public bool IsMountaineer { get; set; }

	public bool IsReserve { get; set; }

	public bool IsOnBoat { get; set; }

	public bool IsDropable { get; set; }

	public bool IsDead { get; set; }

	public bool IsTrain { get; set; }

	public bool IsDefensive { get; set; }

	public bool IsCoreUnit { get; set; }

	public bool IsWinterSpecialized { get; set; }

	public bool IsKillGoal { get; set; }

	public bool AutoSupplies { get; set; }

	public ushort TotalKill { get; set; }

	public ushort TotalDistance { get; set; }

	public short NumberOfTimesTheUnitMoved { get; set; }

	public ushort TankKill { get; set; }

	public ushort AntitankKill { get; set; }

	public ushort ArtilleryKill { get; set; }

	public ushort InfantryKill { get; set; }

	public ushort FighterKill { get; set; }

	public ushort CASKill { get; set; }

	public ushort HeavyBomberKill { get; set; }

	public ushort AAKill { get; set; }

	public ushort BoatKill { get; set; }

	public byte AttackedAmountThisTurn { get; set; }

	public AsleepStates AsleepState { get; set; }

	public string CustomName { get; set; }

	public string ForcedCustomName { get; set; }

	public string OwnerName { get; set; }

	public List<int[]> Waypoints { get; set; }

	public List<string> StuffThatHappened { get; set; }

	public bool IsModded => !OfficialUnits.IsOfficialUnit(this);

	public void ApplyModification(UnitModification p_modification)
	{
		p_modification.Apply(this);
	}

	public void RevertModification(UnitModification p_modification)
	{
		p_modification.Revert(this);
	}

	public string GetUnitName()
	{
		if (!string.IsNullOrEmpty(ForcedCustomName))
		{
			return ForcedCustomName;
		}
		if (!string.IsNullOrEmpty(CustomName))
		{
			return CustomName;
		}
		return Name;
	}

	public string GetNameToLower()
	{
		if (string.IsNullOrEmpty(nameToLower))
		{
			CacheUnitNameToLower();
		}
		else if (nameToLower.Length != Name.Length)
		{
			CacheUnitNameToLower();
		}
		return nameToLower;
	}

	public void CacheUnitNameToLower()
	{
		nameToLower = Name.ToLowerInvariant();
	}

	public void Heal()
	{
		if (ActionPoints < 2 || OwnerPlayer.MoneyAmount - GetHealCost() < 0 || IsDead || CurrHP == MaxHP || SupplyPercentage == 0f)
		{
			return;
		}
		int healAmount = GetHealAmount();
		if (OwnerPlayer.ManpowerAvailable - GetResupplyManpowerCost(healAmount) < 0 && PlayerSettings.Instance.FiniteManpower && PlayerSettings.Instance.Manpower)
		{
			return;
		}
		float num = 1f - (float)CurrHP / (float)MaxHP;
		if (num > 0.5f)
		{
			num = 0.5f;
		}
		short num2 = (short)Math.Round(num * (float)(MaxAmmo - CurrAmmo), MidpointRounding.AwayFromZero);
		CurrAmmo += num2;
		if (CurrAmmo > MaxAmmo)
		{
			CurrAmmo = MaxAmmo;
		}
		short num3 = (short)Math.Round(num * (float)(MaxAutonomy - CurrAutonomy), MidpointRounding.AwayFromZero);
		CurrAutonomy += num3;
		if (CurrAutonomy > MaxAutonomy)
		{
			CurrAutonomy = MaxAutonomy;
		}
		OwnerPlayer.RemoveMoney(GetHealCost(), p_sendRPC: true);
		OwnerPlayer.RemoveManpower(GetResupplyManpowerCost(healAmount), p_sendRPC: false);
		int num4 = 0;
		for (int i = CurrHP; i < MaxHP; i++)
		{
			if (num4 == healAmount)
			{
				break;
			}
			num4++;
			CurrHP++;
		}
		num4 = (int)Math.Round((float)num4 / 10f, MidpointRounding.AwayFromZero);
		for (int j = 0; j < num4; j++)
		{
			AddXP(-1, p_canAddHero: false, p_showXPUI: false);
		}
		ActionPoints = 0;
		if (CurrHP > MaxHP)
		{
			CurrHP = MaxHP;
		}
		AddMorale(10);
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.selectedUnitGO.canUndo = false;
			MapGO.selectedUnitGO.RemoveFogOfWarAroundUnit();
			MapGO.selectedUnitGO.DisplayXPGain((-1 * num4).ToString(), p_sendRPC: true);
		}
		MultiplayerManager.RPC("RPC_SyncUnit", this, false);
	}

	public void ResupplyAmmo()
	{
		if (ActionPoints < 2 || HasResuppliedAmmo || OwnerPlayer.MoneyAmount - GetResupplyAmmoCost() < 0 || IsDead || SupplyPercentage == 0f || CurrAmmo == MaxAmmo)
		{
			return;
		}
		float num = ((!PlayerSettings.Instance.ComplexSupplies) ? ((float)MaxAmmo) : ((float)(int)Math.Round((float)(MaxAmmo - CurrAmmo) * (SupplyPercentage / 100f), MidpointRounding.AwayFromZero)));
		OwnerPlayer.RemoveMoney(GetResupplyAmmoCost(), p_sendRPC: true);
		int num2 = 0;
		for (int i = CurrAmmo; i < MaxAmmo; i++)
		{
			if ((float)num2 == num)
			{
				break;
			}
			num2++;
			CurrAmmo++;
		}
		AddMorale(2);
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.selectedUnitGO.RemoveFogOfWarAroundUnit();
			MapGO.selectedUnitGO.canUndo = false;
		}
		HasResuppliedAmmo = true;
		MultiplayerManager.RPC("RPC_SyncUnit", this, false);
	}

	public void ResupplyFuel()
	{
		if (MaxMP == 0 || SupplyPercentage == 0f || CurrAutonomy == MaxAutonomy || ActionPoints < 2 || HasResuppliedFuel || OwnerPlayer.MoneyAmount - GetResupplyFuelCost() < 0 || IsDead)
		{
			return;
		}
		float num = ((!PlayerSettings.Instance.ComplexSupplies) ? ((float)MaxAutonomy) : ((float)(int)Math.Round((float)(MaxAutonomy - CurrAutonomy) * (SupplyPercentage / 100f), MidpointRounding.AwayFromZero)));
		OwnerPlayer.RemoveMoney(GetResupplyFuelCost(), p_sendRPC: true);
		int num2 = 0;
		for (int i = CurrAutonomy; i < MaxAutonomy; i++)
		{
			if ((float)num2 == num)
			{
				break;
			}
			num2++;
			CurrAutonomy++;
		}
		if (!HasMoved && CurrMP != MaxMP && (!TurnManager.isBadWeather || CurrMP < MaxMP - 1))
		{
			CurrMP = MaxMP;
		}
		AddMorale(2);
		if (MapGO.selectedUnitGO != null)
		{
			MapGO.selectedUnitGO.RemoveFogOfWarAroundUnit();
			MapGO.selectedUnitGO.canUndo = false;
		}
		HasResuppliedFuel = true;
		MultiplayerManager.RPC("RPC_SyncUnit", this, false);
	}

	public void ResupplyAmmoAndFuel()
	{
		ResupplyAmmo();
		ResupplyFuel();
	}

	public int GetHealCost()
	{
		int num = GetHealAmount();
		if (FilterType == "FilterBoat")
		{
			if (IsCarrier)
			{
				num *= 3;
			}
			else if (GetNameToLower().Contains("battle"))
			{
				num *= 3;
			}
			else if (!GetNameToLower().Contains("cruiser") && !GetNameToLower().Contains("dreadnought"))
			{
				num = ((!IsSubmarine && !IsDestroyer) ? ((int)Math.Round((float)num * 1.5f, MidpointRounding.ToEven)) : (num * 2));
			}
			else
			{
				num *= 2;
			}
		}
		else if (FilterType == "FilterFighter")
		{
			num = ((!IsRecon) ? (num * 2) : ((int)Math.Round((float)num * 1.5f, MidpointRounding.ToEven)));
		}
		else if (FilterType == "FilterCAS")
		{
			num = (int)Math.Round((float)num * 2.25f, MidpointRounding.ToEven);
		}
		else if (FilterType == "FilterBomber")
		{
			num = (int)Math.Round((float)num * 2.5f, MidpointRounding.ToEven);
		}
		else if (MaxMP == 0)
		{
			num *= 2;
		}
		else if (Armour >= 70)
		{
			num *= 3;
		}
		else if (Armour >= 50)
		{
			num = (int)Math.Round((float)num * 2.5f, MidpointRounding.ToEven);
		}
		else if (Armour >= 30)
		{
			num *= 2;
		}
		else if (Armour >= 10)
		{
			num = (int)Math.Round((float)num * 1.5f, MidpointRounding.ToEven);
		}
		if (IsMotorized)
		{
			num = (int)Math.Round((float)num * 1.5f, MidpointRounding.ToEven);
		}
		else if (IsMechanized)
		{
			num *= 2;
		}
		if (OwnerPlayer != null)
		{
			if (OwnerPlayer.HasPolicy(Policies.Type.Artillery_barrage) && FilterType == "FilterArtillery")
			{
				num *= 2;
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Human_wave) && FilterType == "FilterInfantry")
			{
				num *= 2;
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Fleet_in_being))
			{
				num += (int)Math.Round((float)num * 0.25f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Cheap_supplies))
			{
				num -= (int)Math.Round((float)num * 0.5f, MidpointRounding.ToEven);
			}
		}
		return num;
	}

	public int GetResupplyManpowerCost(int p_HPamount)
	{
		if (CurrHP >= MaxHP)
		{
			return 0;
		}
		float num = (float)Mathf.Clamp(p_HPamount, 0, MaxHP - CurrHP) / (float)MaxHP;
		float num2 = 4f;
		if (FilterType == "FilterBuilding")
		{
			num2 = 10f;
		}
		return (int)((float)manpowerCost * num / num2);
	}

	public int GetResupplyAmmoCost()
	{
		if (PlayerSettings.Instance.IsInfiniteAmmoAndFuel)
		{
			return 0;
		}
		int num = 0;
		float num2 = MaxAmmo - CurrAmmo;
		if (PlayerSettings.Instance.ComplexSupplies)
		{
			num2 = (int)Math.Round(num2 * (SupplyPercentage / 100f), MidpointRounding.AwayFromZero);
		}
		if (HasABomb)
		{
			num = 5000;
		}
		else
		{
			bool flag = OwnerPlayer != null && IsBlitzkriegDoctrineApplicable();
			int num3 = ((OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Artillery_barrage) && FilterType == "FilterArtillery") ? 3 : ((!flag) ? 1 : 3));
			num = (int)num2 * num3;
			if (HasBombs || HasRockets || HasTorpedo)
			{
				num *= 2;
			}
			else if (FilterType == "FilterBomber" || FilterType == "FilterCAS")
			{
				num *= 2;
			}
			if (OwnerPlayer != null)
			{
				if (FilterType == "FilterAntitank" && OwnerPlayer.HasPolicy(Policies.Type.Improved_AP_shells))
				{
					num *= 3;
				}
				if (OwnerPlayer.HasPolicy(Policies.Type.Cheap_supplies))
				{
					num -= (int)Math.Round((float)num * 0.5f, MidpointRounding.ToEven);
				}
			}
		}
		return num;
	}

	public int GetResupplyFuelCost()
	{
		if (PlayerSettings.Instance.IsInfiniteAmmoAndFuel)
		{
			return 0;
		}
		int num = 0;
		float num2 = MaxAutonomy - CurrAutonomy;
		if (PlayerSettings.Instance.ComplexSupplies)
		{
			num2 = (int)Math.Round(num2 * (SupplyPercentage / 100f), MidpointRounding.AwayFromZero);
		}
		int num3 = ((!IsBlitzkriegDoctrineApplicable()) ? 1 : 3);
		num = (int)num2 * num3;
		if (Type == "Boat")
		{
			if (num > 0 && num < 3)
			{
				num = 3;
			}
			num = (int)Math.Round((float)num / 3f, MidpointRounding.AwayFromZero);
		}
		else if (IsMotorized || IsMechanized)
		{
			num *= 2;
		}
		else if (Type == "Tank")
		{
			num *= 2;
		}
		else
		{
			if (num > 0 && num < 2)
			{
				num = 2;
			}
			num = (int)Math.Round((float)num / 2f, MidpointRounding.AwayFromZero);
		}
		if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Cheap_supplies))
		{
			num -= (int)Math.Round((float)num * 0.5f, MidpointRounding.ToEven);
		}
		return num;
	}

	public int GetResupplyAmmoAndFuelCost()
	{
		return GetResupplyFuelCost() + GetResupplyAmmoCost();
	}

	public int GetHealAmount()
	{
		float num = 0.35f;
		if (FilterType == "FilterBoat")
		{
			if (IsCarrier)
			{
				num = 0.2f;
			}
			else if (Name.Contains("battle"))
			{
				num = 0.2f;
			}
			else
			{
				num = ((!Name.Contains("cruiser") && !Name.Contains("dreadnought") && !Name.Contains("coastal")) ? 0.3f : 0.25f);
			}
		}
		else if (FilterType == "FilterBuilding" && (Name == "Bunker" || GetNameToLower().Contains("coastal battery")))
		{
			num = 0.2f;
		}
		if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Improved_hospitals))
		{
			num *= 2f;
		}
		int num2 = ((!PlayerSettings.Instance.ComplexSupplies) ? ((int)Math.Round((float)MaxHP * num)) : ((int)Math.Round((float)MaxHP * num * (SupplyPercentage / 100f), MidpointRounding.ToEven)));
		if (num2 > MaxHP - CurrHP)
		{
			num2 = MaxHP - CurrHP;
		}
		return num2;
	}

	public void EmbarkDisembarkTrain()
	{
		if (IsTrain)
		{
			IsTrain = false;
			CurrMP = MaxMP;
			MaxMP = TampMaxMP;
			CurrMP = TampMP;
			MaxAutonomy = TampMaxAutonomy;
			CurrAutonomy = TampAutonomy;
			if (NumberOfTimesTheUnitMoved >= 1)
			{
				HasMoved = true;
			}
		}
		else
		{
			IsTrain = true;
			TampMaxMP = MaxMP;
			TampMP = CurrMP;
			TampMaxAutonomy = MaxAutonomy;
			TampAutonomy = CurrAutonomy;
			ApplyModification(Database.trainTransportOption);
			CurrMP = MaxMP;
			CurrAutonomy = MaxAutonomy;
			if (HasMoved && NumberOfTimesTheUnitMoved == 1)
			{
				HasMoved = false;
				if (CurrMP > 0)
				{
					CurrMP = 10;
				}
			}
		}
		EntrenchmentLevel = 0;
		unitGO.RemoveFogOfWarAroundUnit();
		unitGO.ShowPotentialPaths();
	}

	public void EmbarkDisembarkBoat()
	{
		if (IsOnBoat)
		{
			IsOnBoat = false;
			if (FilterType == "FilterInfantry")
			{
				Type = "Infantry";
			}
			else if (FilterType == "FilterArtillery" || FilterType == "FilterAntitank" || FilterType == "FilterAA")
			{
				if (Armour > 0 && !IsMechanized)
				{
					Type = "Tank";
				}
				else if (FilterType == "FilterAA")
				{
					Type = "AA";
				}
				else
				{
					Type = "Artillery";
				}
			}
			else if (FilterType == "FilterTank")
			{
				Type = "Tank";
			}
			else if (FilterType == "FilterBoat")
			{
				Type = "Boat";
			}
			CurrMP = MaxMP;
			MaxMP = TampMaxMP;
			CurrMP = TampMP;
			MaxAutonomy = TampMaxAutonomy;
			CurrAutonomy = TampAutonomy;
			if (NumberOfTimesTheUnitMoved >= 1)
			{
				HasMoved = true;
			}
		}
		else
		{
			IsOnBoat = true;
			TampMaxMP = MaxMP;
			TampMP = CurrMP;
			TampMaxAutonomy = MaxAutonomy;
			TampAutonomy = CurrAutonomy;
			ApplyModification(Database.shipTransportOption);
			CurrMP = MaxMP;
			CurrAutonomy = MaxAutonomy;
			if (HasMoved && NumberOfTimesTheUnitMoved == 1)
			{
				HasMoved = false;
				if (CurrMP > 0)
				{
					CurrMP = 2;
				}
			}
		}
		IsTrain = false;
		EntrenchmentLevel = 0;
	}

	public void EmbarkDisembarkPlane()
	{
		if (IsDropable)
		{
			IsDropable = false;
			if (FilterType == "FilterInfantry")
			{
				Type = "Infantry";
			}
			else if (FilterType == "FilterArtillery" || FilterType == "FilterAntitank" || FilterType == "FilterAA")
			{
				if (Armour > 0 && !IsMechanized)
				{
					Type = "Tank";
				}
				else if (FilterType == "FilterAA")
				{
					Type = "AA";
				}
				else
				{
					Type = "Artillery";
				}
			}
			else if (FilterType == "FilterTank")
			{
				Type = "Tank";
			}
			else if (FilterType == "FilterBoat")
			{
				Type = "Boat";
			}
			CurrMP = MaxMP;
			MaxMP = BaseMaxMP;
			CurrMP = TampMP;
			MaxAutonomy = TampMaxAutonomy;
			CurrAutonomy = TampAutonomy;
			if (NumberOfTimesTheUnitMoved >= 1)
			{
				HasMoved = true;
			}
		}
		else
		{
			IsDropable = true;
			TampMaxMP = MaxMP;
			TampMP = CurrMP;
			TampMaxAutonomy = MaxAutonomy;
			TampAutonomy = CurrAutonomy;
			ApplyModification(Database.planeTransportOption);
			CurrMP = MaxMP;
			CurrAutonomy = MaxAutonomy;
			if (HasMoved && NumberOfTimesTheUnitMoved == 1)
			{
				HasMoved = false;
				if (CurrMP > 0)
				{
					CurrMP = 1;
				}
			}
		}
		EntrenchmentLevel = 0;
	}

	public void NextScenario()
	{
		if (IsOnBoat)
		{
			IsOnBoat = false;
			if (FilterType == "FilterInfantry")
			{
				Type = "Infantry";
			}
			else if (FilterType == "FilterArtillery" || FilterType == "FilterAntitank" || FilterType == "FilterAA")
			{
				if (Armour > 0 && !IsMechanized)
				{
					Type = "Tank";
				}
				else if (FilterType == "FilterAA")
				{
					Type = "AA";
				}
				else
				{
					Type = "Artillery";
				}
			}
			else if (FilterType == "FilterTank")
			{
				Type = "Tank";
			}
			else if (FilterType == "FilterBoat")
			{
				Type = "Boat";
			}
		}
		CurrMP = MaxMP;
		CurrHP = MaxHP;
		CurrAmmo = MaxAmmo;
		CurrAutonomy = MaxAutonomy;
		HasAttacked = false;
		HasOverrun = false;
		HasBuilt = false;
		HasResuppliedFuel = false;
		HasResuppliedAmmo = false;
		HasMoved = false;
		NumberOfTimesTheUnitMoved = 0;
		IsTrain = false;
		Morale = MORALE_DEFAULT;
		ActionPoints = 2;
		AsleepState = AsleepStates.NONE;
		NumberOfTurnsUnderWater = 0;
		EntrenchmentLevel = 0;
		AttackedAmountThisTurn = 0;
		TurnToProduce = 0;
		Waypoints = new List<int[]>();
		if (IsParatrooper)
		{
			IsDropable = true;
			Type = "Plane";
			TampMaxMP = MaxMP;
			TampMP = CurrMP;
			TampMaxAutonomy = MaxAutonomy;
			TampAutonomy = CurrAutonomy;
			MaxMP = 8;
			CurrMP = 8;
			MaxAutonomy = 40;
			CurrAutonomy = MaxAutonomy;
		}
	}

	public void AddRemoveActionPoints(int p_value)
	{
		ActionPoints = (byte)Math.Clamp(ActionPoints + p_value, 0, 2);
	}

	public void AddMorale(int p_amount)
	{
		byte max = 100;
		if (IsPoliticalUnit)
		{
			p_amount = ((p_amount <= 0) ? (p_amount / 2) : (p_amount * 2));
		}
		else
		{
			max = (byte)((float)CurrHP / (float)MaxHP * 100f);
		}
		Morale = (byte)Math.Clamp(Morale + p_amount, 0, max);
	}

	public void NextTurn()
	{
		if (CanGetEntrenched() && (CurrMP == MaxMP || (TurnManager.isBadWeather && CurrMP == MaxMP - 1)))
		{
			IncrementEntrenchment();
		}
		byte b = CurrMP;
		while (b < MaxMP && CurrMP != MaxMP && CurrAutonomy != 0 && CurrMP != CurrAutonomy)
		{
			CurrMP++;
			if (CurrAutonomy < 0)
			{
				CurrAutonomy = 0;
			}
			b++;
		}
		if (CurrMP < 0)
		{
			CurrMP = 0;
		}
		if (AsleepState == AsleepStates.SKIP)
		{
			AsleepState = AsleepStates.NONE;
		}
		if (IsSubmerged)
		{
			NumberOfTurnsUnderWater++;
		}
		else
		{
			NumberOfTurnsUnderWater = 0;
		}
		bool flag = NumberOfTimesTheUnitMoved == short.MaxValue;
		ActionPoints = 2;
		AttackedAmountThisTurn = 0;
		NumberOfTimesTheUnitMoved = 0;
		NumberOfTilesMovedDuringCurrentTurn = 0;
		HasBuilt = false;
		HasAttacked = false;
		HasMoved = false;
		HasOverrun = false;
		HasResuppliedAmmo = false;
		HasResuppliedFuel = false;
		hasBeenAttackedThisTurn = false;
		swapeeUnitID = 0;
		shouldMoveOutNow = false;
		shouldPerformDirectSwap = false;
		willReplaceUnitMovingOut1 = false;
		willReplaceUnitMovingOut2 = false;
		AddMorale(5);
		if (flag)
		{
			ActionPoints = 1;
			HasAttacked = true;
		}
	}

	public bool CanGetEntrenched()
	{
		if (unitGO == null || unitGO.tileGO == null)
		{
			return false;
		}
		if (Type == "Boat" || Type == "Plane" || IsOnBoat || IsTrain || IsArmouredTrain || FilterType == "FilterBuilding")
		{
			return false;
		}
		return true;
	}

	public void IncrementEntrenchment()
	{
		byte maxEntrenchmentLevel = GetMaxEntrenchmentLevel();
		if (OwnerPlayer == null)
		{
			EntrenchmentLevel++;
		}
		else if (OwnerPlayer.HasPolicy(Policies.Type.Organized_defenses) || OwnerPlayer.HasPolicy(Policies.Type.Dug_deep))
		{
			if (GameData.Instance.map.turnCount % 2 == 0)
			{
				EntrenchmentLevel++;
			}
		}
		else
		{
			EntrenchmentLevel++;
		}
		EntrenchmentLevel = (byte)Mathf.Clamp(EntrenchmentLevel, 0, maxEntrenchmentLevel);
	}

	public byte GetMaxEntrenchmentLevel()
	{
		byte result = Database.maxEntrenchmentLevel;
		if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Dug_deep))
		{
			result = 5;
		}
		return result;
	}

	public void AddToHistory(string p_text, bool p_sendRPC)
	{
		if (StuffThatHappened == null)
		{
			StuffThatHappened = new List<string>();
		}
		StuffThatHappened.Add(LocalizationManager.GetLocalizedDate(GameData.Instance.map.Date) + p_text);
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_SyncUnit", this, false);
		}
	}

	public void AddToKillCount(Unit p_unitKilled)
	{
		if (p_unitKilled.IsOnBoat || p_unitKilled.Type == "Boat")
		{
			BoatKill++;
		}
		else if (p_unitKilled.Type == "Plane" || p_unitKilled.IsDropable)
		{
			if (p_unitKilled.FilterType == "FilterFighter")
			{
				FighterKill++;
			}
			else if (p_unitKilled.FilterType == "FilterCAS")
			{
				CASKill++;
			}
			else if (p_unitKilled.FilterType == "FilterBomber" || p_unitKilled.IsDropable)
			{
				HeavyBomberKill++;
			}
		}
		else if (p_unitKilled.Type == "Tank")
		{
			TankKill++;
		}
		else if (p_unitKilled.FilterType == "FilterAntitank")
		{
			AntitankKill++;
		}
		else if (p_unitKilled.FilterType == "FilterArtillery")
		{
			ArtilleryKill++;
		}
		else if (p_unitKilled.FilterType == "FilterInfantry" || p_unitKilled.IsParatrooper)
		{
			InfantryKill++;
		}
		else if (p_unitKilled.Type == "AA")
		{
			AAKill++;
		}
		TotalKill++;
		MultiplayerManager.RPC("RPC_SyncUnit", this, false);
	}

	public void AddXP(int p_XPamount, bool p_canAddHero, bool p_showXPUI)
	{
		int num = 0;
		if (p_XPamount > 0 && OwnerPlayer != null)
		{
			if (OwnerPlayer.IsComputer)
			{
				if (PlayerSettings.Instance.DifficultyLevel == 3)
				{
					p_XPamount++;
				}
				else if (PlayerSettings.Instance.DifficultyLevel == 4)
				{
					p_XPamount += 2;
				}
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Lessons_of_war))
			{
				num++;
			}
		}
		if (FilterType == "FilterBoat" || Type == "Artillery")
		{
			float num2 = Mathf.Clamp(Mathf.Abs((float)p_XPamount / 2f), 1f, Mathf.Abs(p_XPamount));
			p_XPamount = (int)(Mathf.Sign(p_XPamount) * num2);
		}
		p_XPamount += num;
		XP += (short)p_XPamount;
		if (p_showXPUI && unitGO != null)
		{
			if (p_XPamount > 0)
			{
				unitGO.DisplayXPGain("+" + p_XPamount, p_sendRPC: true);
			}
			else
			{
				unitGO.DisplayXPGain(p_XPamount.ToString(), p_sendRPC: true);
			}
		}
		int lvl = Lvl;
		bool flag = CurrHP == MaxHP;
		GetLevelAndXPToNext(out var o_level, out var _);
		Lvl = (byte)o_level;
		if (XP < 0)
		{
			XP = 0;
		}
		if (Lvl > lvl && p_showXPUI && unitGO != null)
		{
			unitGO.DisplayLevelUp();
		}
		MaxHP = BaseMaxHP + NEW_HP_MAX_PER_LEVEL * (Lvl - 1);
		if (MaxHP == 0)
		{
			Debug.Log("-------", unitGO);
			Debug.Log("Max HP is 0 for " + Name, unitGO);
			Debug.Log("Base max HP=" + BaseMaxHP, unitGO);
			Debug.Log("Is the unit dead? " + IsDead, unitGO);
		}
		if (CurrHP > MaxHP)
		{
			CurrHP = MaxHP;
		}
		if (flag)
		{
			CurrHP = MaxHP;
		}
		if (p_canAddHero && Hero == null && !IsCommander && (Lvl >= MAX_LEVEL || (Lvl >= MAX_LEVEL - 1 && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.War_experience))))
		{
			AddHero();
		}
		CheckAchievements();
	}

	private void AddHero()
	{
		if (FilterType == "FilterBuilding")
		{
			return;
		}
		List<string> list = new List<string>();
		if (Type == "Plane" && !IsDropable)
		{
			list = AirHeroes.GetHeroes(OwnerPlayer.Country);
		}
		else
		{
			list = ((!(Type == "Boat") || IsOnBoat) ? GroundHeroes.GetHeroes(OwnerPlayer.Country) : NavalHeroes.GetHeroes(OwnerPlayer.Country));
		}
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			foreach (Hero hero in activeMod.ModdedHeroes.heroes)
			{
				if (!(hero.Country != OwnerPlayer.Country) && GameData.Instance.map.Date.Year >= hero.YearStartAvailability && GameData.Instance.map.Date.Year <= hero.YearStopAvailability && (!(Type == "Plane") || IsDropable || !(hero.Type != "Air")) && (!(Type == "Boat") || IsOnBoat || !(hero.Type != "Naval")) && !(hero.Type != "Ground"))
				{
					list.Add(hero.Name);
				}
			}
		}
		for (int i = 0; i < OwnerPlayer.ListOfUnits.Count; i++)
		{
			if (OwnerPlayer.ListOfUnits[i].Hero == null)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (OwnerPlayer.ListOfUnits[i].Hero.Name == list[j])
				{
					list.RemoveAt(j);
				}
			}
		}
		if (list.Count != 0)
		{
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
			int index = UnityEngine.Random.Range(0, list.Count);
			Hero = new Hero();
			Hero.Name = list[index];
			Hero.Country = OwnerPlayer.Country;
			GenerateHerostats();
			SoftDamage += Hero.SoftDamageBonus;
			BaseSoftDamage += Hero.SoftDamageBonus;
			HardDamage += Hero.HardDamageBonus;
			BaseHardDamage += Hero.HardDamageBonus;
			AirDamage += Hero.AirDamageBonus;
			BaseAirDamage += Hero.AirDamageBonus;
			MaxMP += Hero.MPBonus;
			Visibility += Hero.VisibilityBonus;
			AddToHistory(" : " + Hero.Name + LocalizationManager.Translate("history-got hero"), p_sendRPC: false);
			if (OwnerPlayer != null && OwnerPlayer == TurnManager.humanPlayer)
			{
				UIManager.ReceivedNewHero(unitGO);
			}
			CheckAchievements();
			if ((bool)unitGO)
			{
				unitGO.SetSprite();
			}
			MultiplayerManager.RPC("RPC_SyncUnit", this, true);
		}
	}

	public void GetLevelAndXPToNext(out int o_level, out int o_XPToNextLevel)
	{
		int num = 1;
		int i;
		for (i = 15; XP >= i; i += 15 + (num - 1) * 10)
		{
			if (num >= MAX_LEVEL)
			{
				break;
			}
			num++;
		}
		o_level = Mathf.Clamp(num, 1, MAX_LEVEL);
		if (o_level >= MAX_LEVEL)
		{
			o_XPToNextLevel = 0;
		}
		else
		{
			o_XPToNextLevel = Mathf.Max(0, i - XP);
		}
	}

	private void GenerateHerostats()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		if (FilterType == "FilterBoat")
		{
			num4 = UnityEngine.Random.Range(0, 2);
			num2 = UnityEngine.Random.Range(5, 16);
			if (IsCarrier || Armour > 30)
			{
				num3 = UnityEngine.Random.Range(5, 16);
			}
			else
			{
				num3 = ((!IsSubmarine) ? UnityEngine.Random.Range(3, 11) : UnityEngine.Random.Range(0, 6));
			}
			num5 = ((!IsSubmarine) ? UnityEngine.Random.Range(0, 2) : UnityEngine.Random.Range(0, 3));
		}
		else if (Type == "Plane" && !IsDropable)
		{
			if (FilterType == "FilterBomber")
			{
				num = UnityEngine.Random.Range(0, 6);
				num2 = UnityEngine.Random.Range(0, 6);
				num3 = UnityEngine.Random.Range(0, 11);
				num5 = UnityEngine.Random.Range(0, 2);
			}
			else if (FilterType == "FilterCAS")
			{
				num = UnityEngine.Random.Range(3, 11);
				num2 = UnityEngine.Random.Range(3, 11);
				num3 = UnityEngine.Random.Range(0, 11);
				num5 = UnityEngine.Random.Range(0, 3);
			}
			else
			{
				num = UnityEngine.Random.Range(0, 6);
				num2 = UnityEngine.Random.Range(0, 6);
				num3 = UnityEngine.Random.Range(1, 6);
				num5 = UnityEngine.Random.Range(0, 3);
			}
		}
		else
		{
			if (Type == "AA")
			{
				num3 = UnityEngine.Random.Range(3, 11);
			}
			else if (BaseAirDamage > 0)
			{
				num3 = UnityEngine.Random.Range(0, 6);
			}
			if (FilterType == "FilterAntitank")
			{
				num = UnityEngine.Random.Range(0, 11);
				num2 = UnityEngine.Random.Range(5, 21);
				num5 = UnityEngine.Random.Range(0, 2);
			}
			else if (FilterType == "FilterTank")
			{
				num = UnityEngine.Random.Range(3, 11);
				num2 = UnityEngine.Random.Range(3, 11);
				num5 = UnityEngine.Random.Range(0, 2);
			}
			else if (FilterType == "FilterArtillery")
			{
				num = UnityEngine.Random.Range(1, 6);
				num2 = UnityEngine.Random.Range(1, 6);
			}
			else
			{
				num = UnityEngine.Random.Range(1, 6);
				num2 = UnityEngine.Random.Range(1, 6);
				num5 = UnityEngine.Random.Range(0, 2);
			}
		}
		Hero.SoftDamageBonus = (short)num;
		Hero.HardDamageBonus = (short)num2;
		Hero.AirDamageBonus = (short)num3;
		Hero.VisibilityBonus = (byte)num4;
		Hero.MPBonus = (byte)num5;
	}

	public int GetPotentialDamage(UnitGO p_targetUnitGO, bool p_isRetaliation, bool p_isSupportiveFire)
	{
		baseDamage = 0;
		baseDamageMinusArmour = 0;
		FinalDamage = 0;
		ResetBreakdownValues();
		if (!CanAttackUnit(p_targetUnitGO, p_isRetaliation, p_isSupportiveFire))
		{
			return 0;
		}
		Unit unit = p_targetUnitGO.unit;
		DamageTypes p_damageType = (DamageDealingTypeForCommander = GetDamageTypeAgainstUnit(unit));
		SetBaseDamage(p_damageType, out var o_isEarlyReturn);
		if (o_isEarlyReturn)
		{
			return FinalDamage;
		}
		BattleContext p_context = new BattleContext(this, unit, p_damageType, p_isRetaliation, p_isSupportiveFire);
		ApplyHealthDamageModifier(p_context);
		ApplyArmourDamageModifier(p_context);
		ApplyMechanizedDamageModifier(p_context);
		ApplyTrainDamageModifier(p_context);
		ApplyFrozenTerrainDamageModifier(p_context);
		if (p_isSupportiveFire)
		{
			ApplyShipsSupportingFireDamageModifier(p_context);
		}
		ApplyCASvsShipsDamageModifier(p_context);
		ApplySubmarineVsLandingCraftDamageModifier(p_context);
		ApplyArtilleryVsLandingCraftDamageModifier(p_context);
		ApplyGunboatsVsShipsDamageModifier(p_context);
		ApplyDestroyersVsSubmarinesDamageModifier(p_context);
		ApplyHeroDamageModifier(p_context);
		ApplyPoliciesDamageModifier(p_context);
		ApplyPoliticalUnitDamageModifier(p_context);
		ApplyBadWeatherDamageModifier(p_context);
		ApplyMoraleDamageModifier(p_context);
		ApplyTorpedoVsBoatsDamageModifier(p_context);
		ApplyFlamethrowerDamageModifier(p_context);
		ApplyMountaineerTraitDamageModifier(p_context);
		ApplyHillsDamageModifier(p_context);
		ApplyUnfavorableEnvironmentDamageModifier(p_context);
		ApplyLandingDamageModifier(p_context);
		ApplyCombinedArmsDamageModifier(p_context);
		ApplyReconDamageModifier(p_context);
		ApplyCommanderDamageModifier(p_context);
		ApplyEngineersDamageModifier(p_context);
		ApplyEntrenchmentDamageModifier(p_context);
		FinalDamage = Mathf.Clamp(FinalDamage, 0, int.MaxValue);
		ApplyEncirclementDamageModifier(p_context);
		FinalDamage = Mathf.Clamp(FinalDamage, 0, int.MaxValue);
		ApplyShipsVsLandUnitsDamageModifier(p_context);
		FinalDamage = Mathf.Clamp(FinalDamage, 0, int.MaxValue);
		ApplyTerrainDamageModifier(p_context);
		FinalDamage = Mathf.Clamp(FinalDamage, 0, int.MaxValue);
		ApplyRepeatedAttacksDamageModifier(p_context);
		FinalDamage = Mathf.Clamp(FinalDamage, 0, int.MaxValue);
		return FinalDamage;
	}

	private void ResetBreakdownValues()
	{
		HPbreakdown = 0;
		baseDamageBreakdown = 0;
		veterancyBreakdown = 0;
		moraleBreakdown = 0;
		heroBreakdown = 0;
		entrenchmentBreakdown = 0;
		reconBreakdown = 0;
		encirclementBreakdown = 0;
		flamethrowerBreakdown = 0;
		riverBreakdown = 0;
		terrainBreakdown = 0;
		hillBreakdown = 0;
		landingBreakdown = 0;
		mountaineerBreakdown = 0;
		subVSlandingCraftBreakdown = 0;
		shipVSgroundUnitsBreakdown = 0;
		heavyBomberVSshipsWithoutTorpedoBreakdown = 0;
		torpedoBreakdown = 0;
		destroyerVSsubmergedSubBreakdown = 0;
		armourBreakdown = 0;
		terrainArmouredBreakdown = 0;
		generalBreakdown = 0;
		policyBreakdown = 0;
		politicalUnitsBreakdown = 0;
		biomeBreakdown = 0;
		repeatedAttacksBreakdown = 0;
		combinedArmsBreakdown = 0;
		weatherBreakdown = 0;
		othersBreakdown = 0;
	}

	public bool CanAttackUnit(UnitGO p_targetUnitGO, bool p_isRetaliation, bool p_isSupportiveFire)
	{
		if (p_targetUnitGO == null)
		{
			return false;
		}
		if (unitGO == null)
		{
			return false;
		}
		if (TurnManager.isBadWeather && Type == "Plane" && !IsMissile && !p_isRetaliation && (OwnerPlayer == null || !OwnerPlayer.HasPolicy(Policies.Type.All_weather)))
		{
			return false;
		}
		if (IsOnBoat && (p_targetUnitGO.unit.Type == "Boat" || p_targetUnitGO.unit.Type == "Plane"))
		{
			return false;
		}
		if (!p_isRetaliation && p_targetUnitGO.unit.IsOnBoat && p_targetUnitGO.tileGO.tile.enumType != TileTypes.HARBOUR && !CanAttackShips && (Type == "Infantry" || IsEngineers || FilterType == "FilterAntitank" || FilterType == "FilterTank" || FilterType == "FilterAA"))
		{
			return false;
		}
		if (p_isRetaliation && IsOnBoat && unitGO.tileGO.tile.enumType != TileTypes.HARBOUR)
		{
			return false;
		}
		if (IsOnBoat && FilterType == "FilterArtillery")
		{
			return false;
		}
		if (IsSubmarine && !p_targetUnitGO.unit.IsOnBoat && p_targetUnitGO.unit.Type != "Boat")
		{
			if (!p_isRetaliation || (p_isRetaliation && p_targetUnitGO.unit.Type != "Plane"))
			{
				return false;
			}
		}
		else if (FilterType == "FilterFighter" && p_targetUnitGO.unit.IsSubmarine && !HasRockets && !HasBombs && !HasABomb && !HasTorpedo)
		{
			return false;
		}
		if (p_isRetaliation)
		{
			if (p_targetUnitGO.unit.FilterType == "FilterAA" && Type == "Plane")
			{
				return false;
			}
			if (IsSubmerged)
			{
				return false;
			}
		}
		if (!IsDestroyer && p_targetUnitGO.unit.IsSubmerged)
		{
			return false;
		}
		if (IsDropable)
		{
			return false;
		}
		if (p_targetUnitGO.unit.FilterType == "FilterBoat" && !CanAttackShips)
		{
			return false;
		}
		if (this == p_targetUnitGO.unit && Type == "Plane" && AirDamage <= 0)
		{
			return false;
		}
		if (AirDamage <= 0 && p_targetUnitGO.unit.Type == "Plane")
		{
			return false;
		}
		if (!p_isRetaliation && p_targetUnitGO.unit.Type == "Plane" && (FilterType == "FilterCAS" || FilterType == "FilterBomber"))
		{
			return false;
		}
		if (p_isRetaliation && (p_targetUnitGO.unit.FilterType == "FilterArtillery" || p_targetUnitGO.unit.CoastalBattery))
		{
			return false;
		}
		if (p_isRetaliation && FilterType == "FilterArtillery" && Type != "Tank" && !CoastalBattery)
		{
			return false;
		}
		if (p_isRetaliation && p_targetUnitGO.unit.FilterType == "FilterBoat" && FilterType != "FilterBoat" && !CoastalBattery)
		{
			return false;
		}
		if (p_isRetaliation && p_targetUnitGO.unit.IsSubmarine && !IsDestroyer)
		{
			return false;
		}
		if (p_isRetaliation && IsSubmarine && p_targetUnitGO.unit.Type != "Plane")
		{
			return false;
		}
		if (IsTrain && !IsArmouredTrain)
		{
			return false;
		}
		if (CurrAmmo == 0 && (!p_isRetaliation || (!(FilterType == "FilterBomber") && !(FilterType == "FilterCAS"))))
		{
			return false;
		}
		return true;
	}

	public float GetHPPercent(bool p_forDamagePurposes)
	{
		if (p_forDamagePurposes)
		{
			if (BaseMaxHP == 0)
			{
				return 0f;
			}
			float num = Mathf.Clamp01((float)CurrHP / (float)BaseMaxHP);
			return num + 0.05f * (float)(Lvl - 1);
		}
		if (MaxHP == 0)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)CurrHP / (float)MaxHP);
	}

	public DamageTypes GetDamageTypeAgainstUnit(Unit p_targetUnit)
	{
		if (p_targetUnit == null)
		{
			return DamageTypes.NONE;
		}
		if (p_targetUnit.Type == "Plane")
		{
			return DamageTypes.AIR;
		}
		if (FilterType == "FilterBoat" || p_targetUnit.Type == "Tank" || p_targetUnit.IsTrain || p_targetUnit.FilterType == "FilterBuilding" || p_targetUnit.FilterType == "FilterBoat")
		{
			return DamageTypes.HARD;
		}
		if (p_targetUnit.Type == "Infantry" || p_targetUnit.Type == "AA" || p_targetUnit.Type == "Artillery")
		{
			return DamageTypes.SOFT;
		}
		if (p_targetUnit.IsOnBoat)
		{
			if (p_targetUnit.Armour == 0)
			{
				return DamageTypes.SOFT;
			}
			if (p_targetUnit.IsHorsed || p_targetUnit.IsMotorized || p_targetUnit.IsMechanized)
			{
				return DamageTypes.SOFT;
			}
			return DamageTypes.HARD;
		}
		Debug.LogWarning("Damage type not found for unit " + Name + " against target unit " + p_targetUnit.Name + ". Defaulting to NONE.");
		return DamageTypes.NONE;
	}

	private void SetBaseDamage(DamageTypes p_damageType, out bool o_isEarlyReturn)
	{
		o_isEarlyReturn = false;
		int finalDamage = 0;
		switch (p_damageType)
		{
		case DamageTypes.AIR:
			finalDamage = AirDamage;
			break;
		case DamageTypes.HARD:
			if (HasABomb)
			{
				o_isEarlyReturn = true;
				finalDamage = 300;
			}
			else
			{
				finalDamage = HardDamage;
			}
			break;
		case DamageTypes.SOFT:
			if (HasABomb)
			{
				o_isEarlyReturn = true;
				finalDamage = 300;
			}
			else
			{
				finalDamage = SoftDamage;
			}
			break;
		}
		baseDamage = finalDamage;
		baseDamageMinusArmour = finalDamage;
		FinalDamage = finalDamage;
		baseDamageBreakdown = finalDamage;
	}

	private void ApplyHealthDamageModifier(BattleContext p_context)
	{
		float hPPercent = GetHPPercent(p_forDamagePurposes: true);
		int num = 0;
		if (hPPercent < 1f)
		{
			num = -(int)Math.Round((float)baseDamage * (1f - hPPercent), MidpointRounding.ToEven);
			HPbreakdown += num;
		}
		else if (hPPercent > 1f)
		{
			num = (int)Math.Round((float)baseDamage * (hPPercent - 1f), MidpointRounding.ToEven);
			veterancyBreakdown += num;
		}
		FinalDamage += num;
	}

	public int GetArmour()
	{
		return Armour;
	}

	private void ApplyArmourDamageModifier(BattleContext p_context)
	{
		if (p_context == null)
		{
			return;
		}
		Unit targetUnit = p_context.TargetUnit;
		UnitGO unitGO = targetUnit.unitGO;
		if (targetUnit == null || unitGO == null)
		{
			return;
		}
		float num = 0f;
		if (Type == "Infantry" && targetUnit.Type == "Infantry")
		{
			num = 1f;
		}
		else if (FilterType == "FilterInfantry")
		{
			num = 0.1f;
			if (unitGO.tileGO.tile.enumType == TileTypes.CITY || unitGO.tileGO.tile.enumType == TileTypes.FOREST || unitGO.tileGO.tile.enumType == TileTypes.MARSH || unitGO.tileGO.tile.enumType == TileTypes.HARBOUR || unitGO.tileGO.tile.enumType == TileTypes.FACTORY || unitGO.tileGO.tile.enumType == TileTypes.REFINERY)
			{
				num = 0.05f;
			}
		}
		else if (FilterType == "FilterArtillery")
		{
			num = ((!(targetUnit.FilterType == "FilterBoat")) ? 0.2f : 0.3f);
		}
		else if (!IsSubmarine && FilterType == "FilterBoat" && targetUnit.FilterType == "FilterBoat")
		{
			if (IsDestroyer)
			{
				num = 0.25f;
			}
			else
			{
				num = ((!IsPatrolBoat) ? 0.75f : 1f);
			}
		}
		else if (FilterType == "FilterBoat" && targetUnit.FilterType != "FilterBoat")
		{
			num = 0.15f;
		}
		else if (Type == "Plane")
		{
			if (FilterType == "FilterFighter")
			{
				num = 0.5f;
			}
		}
		else if (GetNameToLower().Contains("bunker"))
		{
			num = 0.15f;
		}
		else if (!targetUnit.IsDropable && (FilterType == "FilterAntitank" || FilterType == "FilterTank" || FilterType == "FilterAA"))
		{
			num = ((!HasFlamethrower || !targetUnit.GetNameToLower().Contains("bunker")) ? 1f : 0.15f);
		}
		int num2 = (int)Math.Round((float)targetUnit.GetArmour() * num, MidpointRounding.ToEven);
		FinalDamage -= num2;
		armourBreakdown -= num2;
		baseDamageMinusArmour = Mathf.Clamp(baseDamage - num2, 0, int.MaxValue);
	}

	private void ApplyMechanizedDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && p_context.TargetUnit.IsMechanized && (FilterType == "FilterAntitank" || FilterType == "FilterTank" || FilterType == "FilterAA") && Database.mechanizedOption.TryGetOperation("Armour", typeof(UnitModification.AddOp), out var op))
		{
			FinalDamage += (int)((UnitModification.AddOp)op).Delta;
			othersBreakdown += (int)((UnitModification.AddOp)op).Delta;
		}
	}

	private void ApplyTrainDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && p_context.TargetUnit.IsTrain && !p_context.TargetUnit.IsArmouredTrain)
		{
			FinalDamage += baseDamage;
			othersBreakdown += baseDamage;
		}
	}

	private void ApplyFrozenTerrainDamageModifier(BattleContext p_context)
	{
		if (p_context == null || p_context.TargetUnit == null)
		{
			return;
		}
		Unit targetUnit = p_context.TargetUnit;
		if (!(targetUnit.unitGO == null) && targetUnit.unitGO.tileGO.tile.isFrozen && !(targetUnit.Type == "Plane"))
		{
			if (targetUnit.Type == "Tank")
			{
				FinalDamage += baseDamage;
				terrainArmouredBreakdown += baseDamage;
			}
			else if (targetUnit.unitGO.tileGO.tile.enumType == TileTypes.RIVER && Type != "Infantry")
			{
				FinalDamage += baseDamage;
				othersBreakdown += baseDamage;
			}
		}
	}

	private void ApplyShipsSupportingFireDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && FilterType == "FilterBoat" && p_context.TargetUnit.Type == "Plane")
		{
			int num = (int)Math.Round((float)FinalDamage * 0.5f, MidpointRounding.ToEven);
			FinalDamage -= num;
			othersBreakdown -= num;
		}
	}

	private void ApplyCASvsShipsDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && FilterType == "FilterCAS" && p_context.TargetUnit.Type == "Boat")
		{
			FinalDamage += baseDamage;
			othersBreakdown += baseDamage;
			baseDamage *= 2;
		}
	}

	private void ApplySubmarineVsLandingCraftDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && IsSubmarine && (p_context.TargetUnit.IsOnBoat || p_context.TargetUnit.IsPatrolBoat))
		{
			FinalDamage += baseDamage;
			subVSlandingCraftBreakdown += baseDamage;
		}
	}

	private void ApplyArtilleryVsLandingCraftDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && FilterType == "FilterArtillery" && p_context.TargetUnit.IsOnBoat)
		{
			FinalDamage -= (int)Math.Round((float)baseDamage * 0.3f, MidpointRounding.ToEven);
			othersBreakdown -= (int)Math.Round((float)baseDamage * 0.3f, MidpointRounding.ToEven);
		}
	}

	private void ApplyGunboatsVsShipsDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && IsPatrolBoat && FilterType == "FilterBoat" && p_context.TargetUnit.FilterType == "FilterBoat")
		{
			FinalDamage -= (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
			othersBreakdown -= (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
		}
	}

	private void ApplyDestroyersVsSubmarinesDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && IsDestroyer && p_context.TargetUnit.IsSubmerged)
		{
			FinalDamage -= (int)Math.Round((float)baseDamage * 0.3f, MidpointRounding.ToEven);
			destroyerVSsubmergedSubBreakdown -= (int)Math.Round((float)baseDamage * 0.3f, MidpointRounding.ToEven);
		}
	}

	private void ApplyHeroDamageModifier(BattleContext p_context)
	{
		if (Hero != null && p_context != null)
		{
			int num = 0;
			if (p_context.DamageType == DamageTypes.AIR)
			{
				num = Hero.AirDamageBonus;
			}
			else if (p_context.DamageType == DamageTypes.HARD)
			{
				num = Hero.HardDamageBonus;
			}
			else if (p_context.DamageType == DamageTypes.SOFT)
			{
				num = Hero.SoftDamageBonus;
			}
			heroBreakdown += num;
			baseDamageBreakdown -= num;
		}
	}

	private void ApplyPoliciesDamageModifier(BattleContext p_context)
	{
		if (this.unitGO == null || p_context == null || p_context.TargetUnit == null)
		{
			return;
		}
		Unit targetUnit = p_context.TargetUnit;
		UnitGO unitGO = targetUnit.unitGO;
		if (!(unitGO == null))
		{
			float hPPercent = GetHPPercent(p_forDamagePurposes: true);
			if (IsBlitzkriegDoctrineApplicable() && !p_context.IsRetaliation && !p_context.IsSupportiveFire)
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.2f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.2f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (Type == "Boat" && !targetUnit.IsOnBoat && targetUnit.Type != "Plane" && targetUnit.Type != "Boat" && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Shore_bombardment))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.2f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.2f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (IsSubmarine && targetUnit.Type != "Plane" && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Wolfpack))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (Type == "Plane" && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Air_supremacy))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.2f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.2f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (FilterType == "FilterArtillery" && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Artillery_barrage))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.2f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.2f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (!p_context.IsRetaliation && targetUnit.OwnerPlayer != null && targetUnit.OwnerPlayer.HasPolicy(Policies.Type.Organized_defenses))
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.2f, MidpointRounding.ToEven);
				policyBreakdown -= (int)Math.Round((float)baseDamage * 0.2f, MidpointRounding.ToEven);
			}
			if (FilterType == "FilterAA" && targetUnit.Type == "Plane" && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Superior_bombing_protection))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f, MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Urban_tactics) && Type != "Boat" && Type != "Plane" && targetUnit.Type != "Plane" && targetUnit.Type != "Boat" && ((p_context.IsRetaliation && (this.unitGO.tileGO.tile.enumType == TileTypes.CITY || this.unitGO.tileGO.tile.enumType == TileTypes.FACTORY || this.unitGO.tileGO.tile.enumType == TileTypes.HARBOUR || this.unitGO.tileGO.tile.enumType == TileTypes.REFINERY)) || (!p_context.IsRetaliation && (unitGO.tileGO.tile.enumType == TileTypes.CITY || unitGO.tileGO.tile.enumType == TileTypes.FACTORY || unitGO.tileGO.tile.enumType == TileTypes.HARBOUR || unitGO.tileGO.tile.enumType == TileTypes.REFINERY))))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Forest_tactics) && Type != "Boat" && Type != "Plane" && targetUnit.Type != "Plane" && targetUnit.Type != "Boat" && ((p_context.IsRetaliation && (this.unitGO.tileGO.tile.enumType == TileTypes.FOREST || this.unitGO.tileGO.tile.Name.Contains("Bocage") || this.unitGO.tileGO.tile.enumType == TileTypes.MARSH)) || (!p_context.IsRetaliation && (unitGO.tileGO.tile.enumType == TileTypes.FOREST || unitGO.tileGO.tile.Name.Contains("Bocage") || unitGO.tileGO.tile.enumType == TileTypes.MARSH))))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Winter_soldiers) && Type != "Boat" && Type != "Plane" && targetUnit.Type != "Plane" && targetUnit.Type != "Boat" && ((p_context.IsRetaliation && this.unitGO.tileGO.tile.enumSeason == TileSeason.WINTER) || (!p_context.IsRetaliation && unitGO.tileGO.tile.enumSeason == TileSeason.WINTER)))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Desert_rats) && Type != "Boat" && Type != "Plane" && targetUnit.Type != "Plane" && targetUnit.Type != "Boat" && ((p_context.IsRetaliation && this.unitGO.tileGO.tile.Season == "Desert") || (!p_context.IsRetaliation && unitGO.tileGO.tile.Season == "Desert")))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Human_wave) && FilterType == "FilterInfantry" && MaxMP > 0 && Lvl < 3 && hPPercent > 0.5f)
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (!p_context.IsRetaliation && targetUnit.OwnerPlayer != null && targetUnit.OwnerPlayer.HasPolicy(Policies.Type.Camouflage))
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.1f, MidpointRounding.ToEven);
				policyBreakdown -= (int)Math.Round((float)baseDamage * 0.1f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Magnetic_grenades) && FilterType == "FilterInfantry" && MaxMP > 0 && targetUnit.Type == "Tank")
			{
				FinalDamage += (int)Math.Round(Math.Abs((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f)), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round(Math.Abs((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f)), MidpointRounding.ToEven);
			}
			if (targetUnit.OwnerPlayer != null && targetUnit.OwnerPlayer.HasPolicy(Policies.Type.Armoured_skirts) && FilterType == "FilterInfantry" && MaxMP > 0 && targetUnit.Type == "Tank")
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.2f, MidpointRounding.ToEven);
				policyBreakdown -= (int)Math.Round((float)baseDamage * 0.2f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Landing_expert) && IsOnBoat)
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Highlands_experts) && Type != "Boat" && Type != "Plane" && targetUnit.Type != "Plane" && targetUnit.Type != "Boat" && ((p_context.IsRetaliation && (this.unitGO.tileGO.tile.enumType == TileTypes.HILL || this.unitGO.tileGO.tile.enumType == TileTypes.MOUNTAIN)) || (!p_context.IsRetaliation && (unitGO.tileGO.tile.enumType == TileTypes.HILL || unitGO.tileGO.tile.enumType == TileTypes.MOUNTAIN))))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.3f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Conscription1) && Lvl <= 1)
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.05f, MidpointRounding.ToEven);
				policyBreakdown -= (int)Math.Round((float)baseDamage * 0.05f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Conscription2) && Lvl <= 1)
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.1f, MidpointRounding.ToEven);
				policyBreakdown -= (int)Math.Round((float)baseDamage * 0.1f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Conscription3) && Lvl <= 1)
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.2f, MidpointRounding.ToEven);
				policyBreakdown -= (int)Math.Round((float)baseDamage * 0.2f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Improved_AP_shells) && FilterType == "FilterAntitank" && targetUnit.Type == "Tank")
			{
				FinalDamage += (int)Math.Round((float)baseDamageBreakdown * 0.3f, MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamageBreakdown * 0.3f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Fleet_in_being) && FilterType == "FilterBoat")
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.25f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
				policyBreakdown += (int)Math.Round((float)baseDamage * 0.25f * Math.Clamp(hPPercent, 0f, 1f), MidpointRounding.ToEven);
			}
			if (TurnManager.isBadWeather && !p_context.IsRetaliation && !IsMissile && Type == "Plane" && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.All_weather))
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
				policyBreakdown -= (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
			}
		}
	}

	private void ApplyPoliticalUnitDamageModifier(BattleContext p_context)
	{
		if (IsPoliticalUnit && p_context != null && p_context.TargetUnit != null && OwnerPlayer != null && p_context.TargetUnit.OwnerPlayer != null && p_context.TargetUnit.OwnerPlayer.Ideology != OwnerPlayer.Ideology)
		{
			FinalDamage += (int)Math.Round((float)baseDamage * 0.15f, MidpointRounding.ToEven);
			politicalUnitsBreakdown += (int)Math.Round((float)baseDamage * 0.15f, MidpointRounding.ToEven);
		}
	}

	private void ApplyRepeatedAttacksDamageModifier(BattleContext p_context)
	{
		if (p_context != null && !p_context.IsRetaliation && p_context.TargetUnit.AttackedAmountThisTurn != 0 && PlayerSettings.Instance.DamageReductionRepeatedAttacks && p_context.TargetUnit.Type != "Boat" && p_context.TargetUnit.Type != "Plane" && (Type == "Plane" || FilterType == "FilterArtillery" || FilterType == "FilterBoat"))
		{
			float num = 0.25f * (float)(int)p_context.TargetUnit.AttackedAmountThisTurn;
			int num2 = (int)Math.Round((float)FinalDamage * num, MidpointRounding.ToEven);
			FinalDamage -= num2;
			repeatedAttacksBreakdown -= num2;
		}
	}

	private void ApplyBadWeatherDamageModifier(BattleContext p_context)
	{
		if (TurnManager.isBadWeather)
		{
			if (Type == "Plane" && !IsMissile && p_context.IsRetaliation)
			{
				int num = (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
				FinalDamage -= num;
				weatherBreakdown -= num;
			}
			else if (Type != "Plane" || (OwnerPlayer != null && !OwnerPlayer.HasPolicy(Policies.Type.All_weather)))
			{
				int num2 = (int)Math.Round((float)baseDamage * 0.1f, MidpointRounding.ToEven);
				FinalDamage -= num2;
				weatherBreakdown -= num2;
			}
		}
	}

	private void ApplyMoraleDamageModifier(BattleContext p_context)
	{
		if (MaxMP > 0 && PlayerSettings.Instance.IsMorale)
		{
			float num = (int)Morale;
			float num2;
			if (num <= 20f)
			{
				num2 = -0.15f;
			}
			else if (num <= 40f)
			{
				num2 = Mathf.Lerp(-0.1f, -0.05f, (num - 20f) / 20f);
			}
			else if (num <= 60f)
			{
				num2 = Mathf.Lerp(-0.05f, 0.05f, (num - 40f) / 20f);
			}
			else
			{
				num2 = ((!(num <= 85f)) ? 0.1f : Mathf.Lerp(0.05f, 0.1f, (num - 60f) / 25f));
			}
			int num3 = (int)Math.Round((float)baseDamage * num2, MidpointRounding.ToEven);
			FinalDamage += num3;
			moraleBreakdown += num3;
		}
	}

	private void ApplyTorpedoVsBoatsDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && HasTorpedo && p_context.TargetUnit.Type == "Boat")
		{
			FinalDamage += 35;
			torpedoBreakdown = 35;
		}
	}

	private void ApplyFlamethrowerDamageModifier(BattleContext p_context)
	{
		if (HasFlamethrower && p_context != null && p_context.TargetUnit != null && !(p_context.TargetUnit.unitGO == null))
		{
			Unit targetUnit = p_context.TargetUnit;
			UnitGO unitGO = targetUnit.unitGO;
			float hPPercent = GetHPPercent(p_forDamagePurposes: true);
			if (targetUnit.FilterType == "FilterBuilding")
			{
				FinalDamage += baseDamage;
				flamethrowerBreakdown = baseDamage;
			}
			else if (unitGO.tileGO.tile.enumType == TileTypes.TRENCH || unitGO.tileGO.tile.enumType == TileTypes.CITY || unitGO.tileGO.tile.enumType == TileTypes.FACTORY || unitGO.tileGO.tile.enumType == TileTypes.REFINERY || unitGO.tileGO.tile.enumType == TileTypes.HARBOUR)
			{
				flamethrowerBreakdown = (int)Math.Round((float)baseDamage * Mathf.Clamp01(hPPercent) * 0.35f, MidpointRounding.ToEven);
				FinalDamage += (int)Math.Round((float)baseDamage * Mathf.Clamp01(hPPercent) * 0.35f, MidpointRounding.ToEven);
			}
		}
	}

	private void ApplyMountaineerTraitDamageModifier(BattleContext p_context)
	{
		if (IsMountaineer && p_context != null && p_context.TargetUnit != null && !(p_context.TargetUnit.unitGO == null) && ((p_context.IsRetaliation && (unitGO.tileGO.tile.enumType == TileTypes.HILL || unitGO.tileGO.tile.enumType == TileTypes.MOUNTAIN)) || (!p_context.IsRetaliation && (p_context.TargetUnit.unitGO.tileGO.tile.enumType == TileTypes.HILL || p_context.TargetUnit.unitGO.tileGO.tile.enumType == TileTypes.MOUNTAIN))))
		{
			float num = (p_context.IsRetaliation ? ((float)unitGO.tileGO.tile.DamageModificator / 100f) : ((float)p_context.TargetUnit.unitGO.tileGO.tile.DamageModificator / 100f));
			FinalDamage += (int)Math.Round((float)baseDamage * num, MidpointRounding.ToEven);
			mountaineerBreakdown += (int)Math.Round((float)baseDamage * num, MidpointRounding.ToEven);
		}
	}

	private void ApplyHillsDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && !(p_context.TargetUnit.unitGO == null) && Type != "Plane" && p_context.IsRetaliation && (unitGO.tileGO.tile.enumType == TileTypes.HILL || unitGO.tileGO.tile.enumType == TileTypes.MOUNTAIN))
		{
			FinalDamage += (int)Math.Round((float)baseDamage * 0.15f, MidpointRounding.ToEven);
			hillBreakdown += (int)Math.Round((float)baseDamage * 0.15f, MidpointRounding.ToEven);
		}
	}

	private void ApplyUnfavorableEnvironmentDamageModifier(BattleContext p_context)
	{
		if (unitGO == null || p_context == null || p_context.TargetUnit == null || p_context.TargetUnit.unitGO == null || !(FilterType == "FilterAA") || p_context.DamageType == DamageTypes.AIR)
		{
			return;
		}
		if (p_context.IsRetaliation)
		{
			if (unitGO.tileGO.tile.enumType == TileTypes.CITY || unitGO.tileGO.tile.enumType == TileTypes.FACTORY || unitGO.tileGO.tile.enumType == TileTypes.HARBOUR || unitGO.tileGO.tile.enumType == TileTypes.FOREST)
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
				othersBreakdown -= (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
			}
		}
		else if (p_context.TargetUnit.unitGO.tileGO.tile.enumType == TileTypes.CITY || p_context.TargetUnit.unitGO.tileGO.tile.enumType == TileTypes.FACTORY || p_context.TargetUnit.unitGO.tileGO.tile.enumType == TileTypes.HARBOUR || p_context.TargetUnit.unitGO.tileGO.tile.enumType == TileTypes.FOREST)
		{
			FinalDamage -= (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
			othersBreakdown -= (int)Math.Round((float)baseDamage * 0.5f, MidpointRounding.ToEven);
		}
	}

	private void ApplyLandingDamageModifier(BattleContext p_context)
	{
		if (IsOnBoat && p_context != null && p_context.TargetUnit != null && !(p_context.TargetUnit.unitGO == null))
		{
			if (GetNameToLower().Contains("marines"))
			{
				FinalDamage += (int)Math.Round((float)baseDamage * 0.15f, MidpointRounding.ToEven);
				landingBreakdown += (int)Math.Round((float)baseDamage * 0.15f, MidpointRounding.ToEven);
			}
			else if (!GetNameToLower().Contains("commando") && Name != "Rangers" && Name != "Imperial Guards")
			{
				FinalDamage -= (int)Math.Round((float)baseDamage * 0.15f, MidpointRounding.ToEven);
				landingBreakdown -= (int)Math.Round((float)baseDamage * 0.15f, MidpointRounding.ToEven);
			}
		}
	}

	private void ApplyCombinedArmsDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && !(p_context.TargetUnit.unitGO == null) && !p_context.IsRetaliation)
		{
			float num = unitGO.ApplyCombinedArmsBonus(p_context.TargetUnit.unitGO.tileGO);
			FinalDamage += (int)Math.Round((float)baseDamage * num, MidpointRounding.ToEven);
			combinedArmsBreakdown += (int)Math.Round((float)baseDamage * num, MidpointRounding.ToEven);
		}
	}

	private void ApplyReconDamageModifier(BattleContext p_context)
	{
		if (!(unitGO == null) && p_context != null && p_context.TargetUnit != null && !(p_context.TargetUnit.unitGO == null))
		{
			float num = unitGO.ApplyReconAttackBonus(p_context.TargetUnit.unitGO.tileGO);
			float hPPercent = GetHPPercent(p_forDamagePurposes: true);
			FinalDamage += (int)Math.Round((float)baseDamage * num * hPPercent, MidpointRounding.ToEven);
			reconBreakdown += (int)Math.Round((float)baseDamage * num * hPPercent, MidpointRounding.ToEven);
		}
	}

	private void ApplyCommanderDamageModifier(BattleContext p_context)
	{
		if (!(unitGO == null) && p_context != null)
		{
			float num = unitGO.ApplyCommandersBonuses();
			float hPPercent = GetHPPercent(p_forDamagePurposes: true);
			FinalDamage += (int)Math.Round((float)baseDamage * num * hPPercent, MidpointRounding.ToEven);
			generalBreakdown += (int)Math.Round((float)baseDamage * num * hPPercent, MidpointRounding.ToEven);
		}
	}

	private void ApplyEngineersDamageModifier(BattleContext p_context)
	{
		if (IsEngineers && p_context != null && p_context.TargetUnit != null && !(p_context.TargetUnit.unitGO == null) && p_context.TargetUnit.IsArmourObstacle)
		{
			FinalDamage = p_context.TargetUnit.MaxHP;
			othersBreakdown = 0;
		}
	}

	private void ApplyEntrenchmentDamageModifier(BattleContext p_context)
	{
		if (!p_context.IsRetaliation && p_context != null && p_context.TargetUnit != null && p_context.TargetUnit.EntrenchmentLevel > 0 && !IsPartisans && !IsPatrolBoat && !HasFlamethrower)
		{
			FinalDamage -= (int)Math.Round((float)baseDamageMinusArmour * (Database.entrenchmentDamageModifier * (float)(int)p_context.TargetUnit.EntrenchmentLevel), MidpointRounding.ToEven);
			entrenchmentBreakdown -= (int)Math.Round((float)baseDamageMinusArmour * (Database.entrenchmentDamageModifier * (float)(int)p_context.TargetUnit.EntrenchmentLevel), MidpointRounding.ToEven);
		}
	}

	private void ApplyEncirclementDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && MaxMP > 0 && !IsArmouredTrain && !IsMissile && Type != "Boat")
		{
			int num = unitGO.CalculateEncirclement();
			if (num >= 2)
			{
				FinalDamage -= (int)Math.Round((float)FinalDamage * ((float)(num - 1) * 0.15f), MidpointRounding.ToEven);
				encirclementBreakdown -= (int)Math.Round((float)FinalDamage * ((float)(num - 1) * 0.15f), MidpointRounding.ToEven);
			}
		}
	}

	private void ApplyShipsVsLandUnitsDamageModifier(BattleContext p_context)
	{
		if (p_context != null && p_context.TargetUnit != null && !(p_context.TargetUnit.unitGO == null) && FilterType == "FilterBoat" && !p_context.TargetUnit.IsOnBoat && p_context.TargetUnit.FilterType != "FilterBoat" && p_context.TargetUnit.Type != "Plane")
		{
			if (IsDestroyer)
			{
				othersBreakdown -= (int)Math.Round((float)FinalDamage * 0.6f, MidpointRounding.ToEven);
				FinalDamage -= (int)Math.Round((float)FinalDamage * 0.6f, MidpointRounding.ToEven);
			}
			else if (GetNameToLower().Contains("light"))
			{
				othersBreakdown -= (int)Math.Round((float)FinalDamage * 0.65f, MidpointRounding.ToEven);
				FinalDamage -= (int)Math.Round((float)FinalDamage * 0.65f, MidpointRounding.ToEven);
			}
			else if (GetNameToLower().Contains("heavy"))
			{
				othersBreakdown -= (int)Math.Round((float)FinalDamage * 0.7f, MidpointRounding.ToEven);
				FinalDamage -= (int)Math.Round((float)FinalDamage * 0.7f, MidpointRounding.ToEven);
			}
			else if (GetNameToLower().Contains("battle"))
			{
				othersBreakdown -= (int)Math.Round((float)FinalDamage * 0.7f, MidpointRounding.ToEven);
				FinalDamage -= (int)Math.Round((float)FinalDamage * 0.7f, MidpointRounding.ToEven);
			}
			else
			{
				othersBreakdown -= (int)Math.Round((float)FinalDamage * 0.6f, MidpointRounding.ToEven);
				FinalDamage -= (int)Math.Round((float)FinalDamage * 0.6f, MidpointRounding.ToEven);
			}
			float num = Utils.GetHeuristicDistance(unitGO.tileGO.tile, p_context.TargetUnit.unitGO.tileGO.tile);
			if (num >= 3f)
			{
				othersBreakdown -= (int)Math.Round((float)FinalDamage * 0.3f, MidpointRounding.ToEven);
				FinalDamage -= (int)Math.Round((float)FinalDamage * 0.3f, MidpointRounding.ToEven);
			}
			else if (num >= 2f)
			{
				othersBreakdown -= (int)Math.Round((float)FinalDamage * 0.15f, MidpointRounding.ToEven);
				FinalDamage -= (int)Math.Round((float)FinalDamage * 0.15f, MidpointRounding.ToEven);
			}
		}
	}

	private void ApplyTerrainDamageModifier(BattleContext p_context)
	{
		if (unitGO == null || unitGO.tileGO == null || p_context == null || p_context.TargetUnit == null || p_context.TargetUnit.unitGO == null)
		{
			return;
		}
		TileGO tileGO = p_context.TargetUnit.unitGO.tileGO;
		if (tileGO == null || IsPartisans || !(p_context.TargetUnit.Type != "Plane") || ((!(Type == "Infantry") || !(p_context.TargetUnit.Type != "Tank")) && !(Type != "Infantry")))
		{
			return;
		}
		int num = ((!(FilterType == "FilterArtillery")) ? FinalDamage : baseDamage);
		if (!p_context.IsRetaliation)
		{
			float num2 = (float)num * ((float)tileGO.tile.DamageModificator / 100f);
			if (num2 > 0f && num2 < 1f)
			{
				num2 = 1f;
			}
			FinalDamage -= (int)Math.Round(num2, MidpointRounding.ToEven);
			terrainBreakdown -= (int)Math.Round(num2, MidpointRounding.ToEven);
		}
		if (!IsCavalry && (!(Type == "Tank") || (!(FilterType == "FilterTank") && !(FilterType == "FilterAntitank"))))
		{
			return;
		}
		Tile tile = ((!p_context.IsRetaliation) ? tileGO.tile : unitGO.tileGO.tile);
		if (tile.enumType == TileTypes.CITY || tile.enumType == TileTypes.FACTORY || tile.Type == "Bocage" || tile.enumType == TileTypes.MARSH || tile.enumType == TileTypes.REFINERY || tile.enumType == TileTypes.HARBOUR || tile.enumType == TileTypes.FOREST || tile.enumType == TileTypes.MOUNTAIN)
		{
			FinalDamage -= (int)Math.Round((float)num * ((float)tile.DamageModificator / 2f / 100f), MidpointRounding.ToEven);
			if (IsCavalry)
			{
				othersBreakdown -= (int)Math.Round((float)num * ((float)tile.DamageModificator / 2f / 100f), MidpointRounding.ToEven);
			}
			else
			{
				terrainArmouredBreakdown -= (int)Math.Round((float)num * ((float)tile.DamageModificator / 2f / 100f), MidpointRounding.ToEven);
			}
		}
	}

	public short GetAmmoAttrition(Unit p_targetUnit)
	{
		if (p_targetUnit == null)
		{
			return 0;
		}
		short num = Math.Max(SoftDamage, HardDamage);
		float num2;
		float num3;
		if (num <= 10)
		{
			num2 = 0.05f;
			num3 = 0.1f;
		}
		else if (num <= 20)
		{
			num2 = 0.1f;
			num3 = 0.15f;
		}
		else if (num <= 30)
		{
			num2 = 0.15f;
			num3 = 0.2f;
		}
		else
		{
			num2 = 0.2f;
			num3 = 0.25f;
		}
		if (p_targetUnit.EntrenchmentLevel > 0)
		{
			float num4 = (float)(int)p_targetUnit.EntrenchmentLevel * 0.03f;
			num2 -= num4;
			num3 -= num4;
		}
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		if (num3 < 0.01f)
		{
			num3 = 0.01f;
		}
		short num5 = (short)Math.Round((float)p_targetUnit.MaxAmmo * UnityEngine.Random.Range(num2, num3), MidpointRounding.ToEven);
		if (FilterType == "FilterArtillery" && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Artillery_barrage))
		{
			num5 += (short)Math.Round((float)num5 * 0.2f, MidpointRounding.ToEven);
		}
		short num6 = (short)Math.Max(1, p_targetUnit.MaxAmmo / 5);
		if (p_targetUnit.CurrAmmo - num5 < num6)
		{
			num5 = (short)Math.Max(0, p_targetUnit.CurrAmmo - num6);
		}
		return num5;
	}

	public short GetFuelAttrition(Unit p_targetUnit)
	{
		if (p_targetUnit == null)
		{
			return 0;
		}
		short num = Math.Max(SoftDamage, HardDamage);
		float num2;
		float num3;
		if (num <= 10)
		{
			num2 = 0.03f;
			num3 = 0.06f;
		}
		else if (num <= 20)
		{
			num2 = 0.05f;
			num3 = 0.08f;
		}
		else if (num <= 30)
		{
			num2 = 0.06f;
			num3 = 0.1f;
		}
		else
		{
			num2 = 0.08f;
			num3 = 0.12f;
		}
		if (p_targetUnit.EntrenchmentLevel > 0)
		{
			float num4 = (float)(int)p_targetUnit.EntrenchmentLevel * 0.03f;
			num2 -= num4;
			num3 -= num4;
		}
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		if (num3 < 0.01f)
		{
			num3 = 0.01f;
		}
		short num5 = (short)Math.Round((float)p_targetUnit.MaxAutonomy * UnityEngine.Random.Range(num2, num3), MidpointRounding.ToEven);
		if (FilterType == "FilterArtillery" && OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Artillery_barrage))
		{
			num5 += (short)Math.Round((float)num5 * 0.2f, MidpointRounding.ToEven);
		}
		short num6 = (short)Math.Max(1, p_targetUnit.MaxAutonomy / 5);
		if (p_targetUnit.CurrAutonomy - num5 < num6)
		{
			num5 = (short)Math.Max(0, p_targetUnit.CurrAutonomy - num6);
		}
		return num5;
	}

	public void CalculateAndSetUnitPrice()
	{
		string text = (string.IsNullOrEmpty(BaseType) ? Type : BaseType);
		if (IsCommander && PlayerSettings.Instance.Policies)
		{
			Cost = (SoftDamage + HardDamage + AirDamage + Utils.RoundUp(Range)) * 2;
			Cost += Armour;
			Cost = Utils.RoundUp(Cost);
			if (Cost > 100)
			{
				Cost = 100;
			}
			return;
		}
		if (CustomCost != 0)
		{
			baseUnitCost = CustomCost;
			Cost = CustomCost;
		}
		else
		{
			int num = ((!(FilterType == "FilterBoat")) ? (BaseMaxMP * 8 + BaseArmour * 3 + BaseAirDamage * 2 + BaseMaxAutonomy / 2 + BaseRange * 40 + BaseVisibility * 10) : (BaseMaxMP * 10 + BaseArmour * 5 + BaseAirDamage * 2 + BaseMaxAutonomy / 5 + BaseRange * 30 + BaseVisibility * 30));
			int num2 = num;
			if (FilterType == "FilterArtillery")
			{
				if (BaseSoftDamage >= 35)
				{
					num2 += BaseSoftDamage * 5 + BaseHardDamage * 5 + 15;
				}
				else if (BaseSoftDamage >= 30)
				{
					num2 += BaseSoftDamage * 4 + BaseHardDamage * 4 + 15;
				}
				else if (BaseSoftDamage >= 25)
				{
					num2 += BaseSoftDamage * 3 + BaseHardDamage * 3 + 15;
				}
				else if (BaseSoftDamage >= 20)
				{
					num2 += BaseSoftDamage * 2 + BaseHardDamage * 2 + 15;
				}
				else
				{
					num2 = ((BaseSoftDamage < 15) ? (num2 + (BaseSoftDamage + BaseHardDamage)) : (num2 + (BaseSoftDamage * 2 + BaseHardDamage)));
				}
				if (!IsArmouredTrain && BaseMaxMP > 1 && text == "Tank")
				{
					num2 += (int)((float)num2 * 0.5f);
				}
			}
			else if (FilterType == "FilterBomber")
			{
				if (BaseSoftDamage >= 35)
				{
					num2 += BaseSoftDamage + BaseHardDamage + BaseAirDamage + 50;
				}
				else if (BaseSoftDamage >= 30)
				{
					num2 += BaseSoftDamage + BaseHardDamage + BaseAirDamage + 40;
				}
				else if (BaseSoftDamage >= 25)
				{
					num2 += BaseSoftDamage + BaseHardDamage + BaseAirDamage + 30;
				}
				else if (BaseSoftDamage >= 20)
				{
					num2 += BaseSoftDamage + BaseHardDamage + BaseAirDamage + 20;
				}
				else
				{
					num2 = ((BaseSoftDamage < 15) ? (num2 + (BaseSoftDamage + BaseHardDamage + BaseAirDamage)) : (num2 + (BaseSoftDamage + BaseHardDamage + BaseAirDamage + 10)));
				}
			}
			else if (FilterType == "FilterBoat")
			{
				num2 += BaseHardDamage * 2;
				if (IsSubmarine)
				{
					if (IsRADAR)
					{
						num2 += 50;
					}
					if (CanCatapultPlanes)
					{
						num2 += 50;
					}
				}
				else
				{
					if (HasSonar)
					{
						num2 += 50;
					}
					if (IsRADAR)
					{
						num2 += 100;
					}
					if (CanCatapultPlanes)
					{
						num2 += 50;
					}
					if (IsPatrolBoat)
					{
						num2 = (int)((float)num2 * 1f);
					}
					else if (IsDestroyer || GetNameToLower().Contains("destroyer"))
					{
						num2 = (int)((float)num2 * 0.8f);
					}
					else if (GetNameToLower().Contains("coastal"))
					{
						num2 += (int)((float)num * 0.3f);
					}
					else if (GetNameToLower().Contains("battle"))
					{
						num2 += (int)((float)num * 0.7f);
					}
				}
			}
			else
			{
				num2 += BaseSoftDamage + BaseHardDamage;
				if (HasSonar)
				{
					num2 += 50;
				}
			}
			if (IsCommander)
			{
				num2 += 800;
			}
			if (IsCarrier)
			{
				num2 += (int)((float)num * 1.2f);
			}
			if (IsParatrooper)
			{
				num2 += 50;
			}
			if (HasFlamethrower)
			{
				num2 += 50;
			}
			if (IsMineFlail)
			{
				num2 += 50;
			}
			if (IsMissile)
			{
				num2 += 150;
			}
			if (CanBeRefueledOnCarrier)
			{
				num2 += 50;
			}
			if (text == "Plane")
			{
				num2 += 100;
				if (IsRecon)
				{
					num2 -= 50;
				}
			}
			if (IsPrototype)
			{
				num2 += (int)((float)num2 * 0.3f);
			}
			Cost = Utils.RoundUp(num2);
			baseUnitCost = Cost;
		}
		if (IsCoreUnit)
		{
			Cost += Database.coreUnitCost;
		}
		if (IsMountaineer)
		{
			Cost += Database.mountaineerCost;
		}
		if (IsPoliticalUnit)
		{
			Cost += Database.politicalUnitCost;
		}
		if (IsHorsed)
		{
			Cost += Database.horseCost;
		}
		if (IsMotorized)
		{
			Cost += Database.motorizeCost;
		}
		if (IsMechanized)
		{
			Cost += Database.mechanizeCost;
		}
		if (HasABomb)
		{
			Cost += Database.AtomBombCost;
		}
		if (HasDropTank)
		{
			Cost += Database.dropTankCost;
		}
		if (HasBombs)
		{
			Cost += Database.bombCost;
		}
		if (HasRockets)
		{
			Cost += Database.rocketsCost;
		}
		if (HasTorpedo)
		{
			Cost += Database.torpedoCost;
		}
		if (OwnerPlayer != null && (!IsCommander || !PlayerSettings.Instance.Policies))
		{
			int cost = Cost;
			if (OwnerPlayer.HasPolicy(Policies.Type.Army_training))
			{
				Cost += (int)Math.Round((float)cost * 0.1f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Armoured_skirts) && text == "Tank")
			{
				Cost += (int)Math.Round((float)cost * 0.05f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Construction_engineering) && FilterType == "FilterBuilding")
			{
				Cost += (int)Math.Round((float)cost * 0.05f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Camouflage) && text != "Boat" && text != "Plane" && FilterType != "FilterBuilding")
			{
				Cost += (int)Math.Round((float)cost * 0.05f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Winter_trained) && text != "Boat" && text != "Plane" && FilterType != "FilterBuilding" && !IsCommander)
			{
				Cost += (int)Math.Round((float)cost * 0.05f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Infantry_focus) && FilterType == "FilterInfantry")
			{
				Cost -= (int)Math.Round((float)cost * 0.25f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Armoured_focus) && text == "Tank")
			{
				Cost -= (int)Math.Round((float)cost * 0.25f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Air_focus) && text == "Plane")
			{
				Cost -= (int)Math.Round((float)cost * 0.25f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Guns_focus) && (text == "Artillery" || text == "AA"))
			{
				Cost -= (int)Math.Round((float)cost * 0.25f, MidpointRounding.ToEven);
			}
			if (OwnerPlayer.HasPolicy(Policies.Type.Naval_focus) && text == "Boat")
			{
				Cost -= (int)Math.Round((float)cost * 0.25f, MidpointRounding.ToEven);
			}
		}
	}

	public void CalculateAndSetManpowerCost()
	{
		if (CustomManpowerCost != 0)
		{
			manpowerCost = Mathf.Clamp(CustomManpowerCost, 0, int.MaxValue);
			return;
		}
		manpowerCost = 0;
		if (Type == "Plane" && !IsParatrooper)
		{
			if (FilterType == "FilterBomber")
			{
				if (Name.Contains("alloon"))
				{
					manpowerCost = 500;
				}
				else
				{
					manpowerCost = 3000;
				}
			}
			else if (FilterType == "FilterCAS")
			{
				manpowerCost = 2000;
			}
			else if (FilterType == "FilterFighter")
			{
				manpowerCost = 1000;
			}
		}
		else if (FilterType == "FilterInfantry")
		{
			if (IsMine)
			{
				manpowerCost = 0;
			}
			else if (IsPartisans)
			{
				manpowerCost = 0;
			}
			else if (MaxMP == 0)
			{
				manpowerCost = 500;
			}
			else if (IsCommander)
			{
				manpowerCost = 100;
			}
			else if (IsCavalry)
			{
				manpowerCost = 5000;
			}
			else if (Name.Contains("Militia"))
			{
				manpowerCost = 5000;
			}
			else if (Name.Contains("Conscript"))
			{
				manpowerCost = 5000;
			}
			else if (GetNameToLower().Contains("garrison"))
			{
				manpowerCost = 2500;
			}
			else if (GetNameToLower().Contains("light infantry"))
			{
				manpowerCost = 13000;
			}
			else if (Name.Contains("AT infantry"))
			{
				manpowerCost = 5000;
			}
			else
			{
				manpowerCost += 15000;
			}
		}
		else if (FilterType == "FilterTank")
		{
			if (BaseMaxHP == 50)
			{
				manpowerCost = 2500;
			}
			else if (BaseArmour <= 30)
			{
				manpowerCost = 5000;
			}
			else if (BaseArmour <= 50)
			{
				manpowerCost = 7500;
			}
			else if (BaseArmour > 50)
			{
				manpowerCost = 10000;
			}
		}
		else if (FilterType == "FilterBoat")
		{
			if (IsDestroyer)
			{
				manpowerCost = 3000;
			}
			else if (IsSubmarine)
			{
				manpowerCost = 1000;
			}
			else if (IsPatrolBoat)
			{
				manpowerCost = 1000;
			}
			else if (IsCarrier)
			{
				manpowerCost = 10000;
			}
			else if (Name.Contains("light cruiser"))
			{
				manpowerCost = 5000;
			}
			else if (Name.Contains("heavy cruiser"))
			{
				manpowerCost = 7500;
			}
			else if (Name.Contains("attle"))
			{
				manpowerCost = 10000;
			}
			else
			{
				manpowerCost = 5000;
			}
			if (GameData.Instance != null && GameData.Instance.map != null && GameData.Instance.map.Date.Year >= 1939 && GameData.Instance.map.Date.Year <= 1945 && (Country == "USA" || Country == "Great Britain"))
			{
				manpowerCost /= 2;
			}
		}
		else if (FilterType == "FilterAA")
		{
			if (MaxMP > 1)
			{
				manpowerCost = 5000;
			}
			else
			{
				manpowerCost = 3000;
			}
		}
		else if (FilterType == "FilterArtillery")
		{
			if (MaxMP > 1)
			{
				manpowerCost = 5000;
			}
			else
			{
				manpowerCost = 3000;
			}
		}
		else if (FilterType == "FilterAntitank")
		{
			if (MaxMP > 1)
			{
				manpowerCost = 5000;
			}
			else
			{
				manpowerCost = 3000;
			}
		}
	}

	public byte GetUpgradeProductionTime(Unit p_fromUnit, byte p_currentProductionTime)
	{
		if (TotalDistance > 0 || TotalKill > 0 || (StuffThatHappened != null && StuffThatHappened.Count > 0))
		{
			float num = (float)Cost / (float)p_fromUnit.Cost;
			byte b = 0;
			if (FilterType == "FilterBoat")
			{
				if (IsCarrier)
				{
					b = 13;
				}
				else if (GetNameToLower().Contains("battleship"))
				{
					b = 10;
				}
				else if (GetNameToLower().Contains("heavy cruiser"))
				{
					b = 5;
				}
				else
				{
					b = (byte)((!GetNameToLower().Contains("light cruiser")) ? 3 : 4);
				}
				if (OwnerPlayer.HasPolicy(Policies.Type.Recruitment_boost_naval))
				{
					b /= 2;
				}
			}
			else if (!GetNameToLower().Contains("garrison"))
			{
				if (num >= 3f)
				{
					b += 3;
				}
				else
				{
					b = ((!(num >= 2f)) ? ((byte)(b + 1)) : ((byte)(b + 2)));
				}
			}
			if (b < p_currentProductionTime)
			{
				b = p_currentProductionTime;
			}
			return b;
		}
		return GetProductionTime();
	}

	public void SetDefaultProductionTime()
	{
		TurnToProduce = GetProductionTime();
	}

	public byte GetProductionTime()
	{
		byte b = 1;
		if (CustomTurnProduction != 0)
		{
			b = CustomTurnProduction;
		}
		else
		{
			if (IsMine && MaxMP == 0)
			{
				return 0;
			}
			if (FilterType == "FilterBuilding")
			{
				if (IsPontoonBridge || IsArmourObstacle)
				{
					b = 1;
				}
				else if (MaxMP == 0)
				{
					b = 4;
				}
			}
			else if (FilterType == "FilterBoat")
			{
				if (IsDestroyer)
				{
					b = 4;
				}
				else if (IsSubmarine)
				{
					b = 4;
				}
				else if (IsCarrier)
				{
					b = 15;
				}
				else if (!Name.Contains("battle") && !Name.Contains("coastal"))
				{
					if (Name.Contains("heavy"))
					{
						b = 7;
					}
					else if (Name.Contains("light"))
					{
						b = 5;
					}
					else
					{
						b = (byte)((!IsPatrolBoat) ? 4 : 3);
					}
				}
				else
				{
					b = 10;
				}
			}
			else if (Name.Contains("balloon"))
			{
				b = 1;
			}
			else if (Type == "Plane" && !IsParatrooper)
			{
				b = 2;
			}
			else if (FilterType == "FilterAntitank" && Type != "Tank")
			{
				if (HardDamage > 80)
				{
					b = 2;
				}
			}
			else if (Type == "Tank" && FilterType == "FilterArtillery")
			{
				b = (byte)((SoftDamage < 25) ? 2 : 3);
			}
			else if (Type == "Tank" && (HardDamage >= 60 || Armour >= 30))
			{
				b = (byte)((HardDamage < 90 && Armour < 90) ? 2 : 3);
			}
			else if (Type == "Artillery" && SoftDamage >= 18)
			{
				b = 2;
			}
			else if (Type == "AA" && (HardDamage >= 80 || SoftDamage >= 20))
			{
				b = 2;
			}
		}
		if (IsPrototype)
		{
			b *= 2;
		}
		if (IsEliteUnit)
		{
			b *= 2;
		}
		if (IsMountaineer)
		{
			b++;
		}
		if (IsPoliticalUnit)
		{
			b++;
		}
		if (IsMotorized)
		{
			b++;
		}
		else if (IsMechanized)
		{
			b++;
		}
		if (Country != TurnManager.currPlayer.Country)
		{
			b = ((!(Type == "Boat")) ? ((byte)(b + 1)) : ((byte)(b + 3)));
		}
		if (!MainMenu.isEditor)
		{
			if (IsMountaineer && OwnerPlayer.HasPolicy(Policies.Type.Highlands_experts))
			{
				b++;
			}
			if (IsCommander && OwnerPlayer.HasPolicy(Policies.Type.Commanders_development_program))
			{
				b *= 2;
			}
			if (FilterType == "FilterBuilding" && OwnerPlayer.HasPolicy(Policies.Type.Construction_engineering))
			{
				b = (byte)((float)(int)b * 0.5f);
			}
			if (FilterType == "FilterInfantry" && OwnerPlayer.HasPolicy(Policies.Type.Recruitment_boost_infantry))
			{
				b = (byte)((float)(int)b * 0.5f);
			}
			if (Type == "Tank" && OwnerPlayer.HasPolicy(Policies.Type.Recruitment_boost_armoured))
			{
				b = (byte)((float)(int)b * 0.5f);
			}
			if (Type == "Plane" && OwnerPlayer.HasPolicy(Policies.Type.Recruitment_boost_air))
			{
				b = (byte)((float)(int)b * 0.5f);
			}
			if (Type == "Boat" && OwnerPlayer.HasPolicy(Policies.Type.Recruitment_boost_naval))
			{
				b = (byte)((float)(int)b * 0.5f);
			}
			if ((Type == "Artillery" || Type == "AA") && OwnerPlayer.HasPolicy(Policies.Type.Recruitment_boost_guns))
			{
				b = (byte)((float)(int)b * 0.5f);
			}
		}
		if (b < 1)
		{
			b = 1;
		}
		return b;
	}

	public void SetCommissionDate()
	{
		if (StuffThatHappened == null)
		{
			StuffThatHappened = new List<string> { LocalizationManager.GetLocalizedDate(GameData.Instance.map.Date) + " : " + Name + LocalizationManager.Translate("history-commissioned") };
		}
	}

	public bool CanApplyZoneOfControl(Unit p_unit)
	{
		if (IsRecon)
		{
			return false;
		}
		if (IsTrain)
		{
			return false;
		}
		if (IsOnBoat)
		{
			return false;
		}
		if (IsMine)
		{
			return false;
		}
		if (IsPartisans)
		{
			return false;
		}
		if (IsArmouredTrain)
		{
			return false;
		}
		if (IsCommander)
		{
			return false;
		}
		if (CurrAmmo == 0)
		{
			return false;
		}
		if (IsSubmerged || p_unit.IsSubmerged)
		{
			return false;
		}
		if (SoftDamage == 0 && HardDamage == 0 && AirDamage == 0)
		{
			return false;
		}
		if (GetHPPercent(p_forDamagePurposes: false) <= 0.35f)
		{
			return false;
		}
		if (Type == "Boat")
		{
			if (p_unit.Type != "Boat")
			{
				return false;
			}
		}
		else if (Type == "Plane")
		{
			if (p_unit.Type != "Plane")
			{
				return false;
			}
		}
		else
		{
			if (p_unit.Type == "Boat" || p_unit.Type == "Plane")
			{
				return false;
			}
			if (Type == "Infantry")
			{
				string text = GetNameToLower();
				if (text.Contains("garrison"))
				{
					return false;
				}
				if (text.Contains("militia"))
				{
					return false;
				}
				if (text.Contains("volkssturm"))
				{
					return false;
				}
			}
		}
		if (!p_unit.OwnerPlayer.IsAtWarWith(OwnerPlayer))
		{
			return false;
		}
		return true;
	}

	public void CalculateAndSetUpkeepValue()
	{
		UpkeepValue = 0f;
		string text = (string.IsNullOrEmpty(BaseType) ? Type : BaseType);
		if (CustomUpkeepValue < 0f)
		{
			return;
		}
		if (CustomUpkeepValue > 0f)
		{
			UpkeepValue = CustomUpkeepValue;
		}
		else
		{
			if (IsPartisans || TileBuilding)
			{
				return;
			}
			string text2 = GetNameToLower();
			UpkeepValue = 1f;
			if (FilterType == "FilterInfantry")
			{
				if (text2.Contains("garrison"))
				{
					UpkeepValue = 1f;
				}
				else if (text2.Contains("heavy"))
				{
					UpkeepValue = 4f;
					if (IsMotorized || IsMechanized)
					{
						UpkeepValue = 5f;
					}
				}
				else if (text2.Contains("nkvd"))
				{
					UpkeepValue = 4f;
				}
				else if (text2.Contains("marines"))
				{
					UpkeepValue = 4f;
				}
				else if (text2.Contains("commando"))
				{
					UpkeepValue = 5f;
				}
				else if (text2.Contains("ranger"))
				{
					UpkeepValue = 5f;
				}
				else if (text2.Contains("shock"))
				{
					UpkeepValue = 5f;
				}
				else if (text2.Contains("grenadier"))
				{
					UpkeepValue = 5f;
				}
			}
			else if (FilterType == "FilterBuilding")
			{
				if (CoastalBattery || text2.Contains("bunker"))
				{
					UpkeepValue = 1.5f;
				}
			}
			else if (FilterType == "FilterBoat")
			{
				if (IsCarrier)
				{
					UpkeepValue = 5f;
				}
				else if (text2.Contains("battle"))
				{
					UpkeepValue += 3.75f;
				}
				else if (nameToLower.Contains("dreadnought"))
				{
					UpkeepValue += 3.75f;
				}
				else if (text2.Contains("heavy"))
				{
					UpkeepValue += 3.75f;
				}
				else if (text2.Contains("light"))
				{
					UpkeepValue += 2.75f;
				}
				else if (IsSubmarine)
				{
					UpkeepValue += 2.5f;
				}
				else if (IsDestroyer)
				{
					UpkeepValue += 2f;
				}
				else
				{
					UpkeepValue = 1f;
				}
			}
			else
			{
				switch (text)
				{
				case "Plane":
					UpkeepValue = 5f;
					if (IsRecon)
					{
						UpkeepValue *= 0.5f;
					}
					break;
				case "Tank":
					if (Armour <= 10)
					{
						UpkeepValue = 2.5f;
					}
					else if (Armour <= 20)
					{
						UpkeepValue = 3f;
					}
					else if (Armour <= 30)
					{
						UpkeepValue = 3.5f;
					}
					else if (Armour <= 50)
					{
						UpkeepValue = 4f;
					}
					else if (Armour <= 70)
					{
						UpkeepValue = 4.5f;
					}
					else if (Armour > 70)
					{
						UpkeepValue = 5f;
					}
					break;
				case "Artillery":
					UpkeepValue = 3f;
					break;
				}
			}
		}
		if (IsPartisans)
		{
			UpkeepValue = 0f;
			return;
		}
		if (IsPoliticalUnit)
		{
			UpkeepValue += 3f;
		}
		if (IsCavalry)
		{
			UpkeepValue += 2f;
		}
		if (IsEngineers)
		{
			UpkeepValue += 2.5f;
		}
		if (IsMountaineer)
		{
			UpkeepValue++;
		}
		if (IsParatrooper)
		{
			UpkeepValue += 3.5f;
		}
		if (IsHorsed)
		{
			UpkeepValue += 0.5f;
		}
		if (IsMotorized)
		{
			UpkeepValue += 1.5f;
		}
		if (IsMechanized)
		{
			UpkeepValue += 3f;
		}
		if (HasABomb)
		{
			UpkeepValue += 50f;
		}
		if (HasTorpedo)
		{
			UpkeepValue += 0.5f;
		}
		if (HasDropTank)
		{
			UpkeepValue += 0.5f;
		}
		if (HasBombs)
		{
			UpkeepValue += 0.5f;
		}
		if (HasRockets)
		{
			UpkeepValue += 0.5f;
		}
		if (OwnerPlayer != null && FilterType == "FilterAA" && OwnerPlayer.HasPolicy(Policies.Type.Superior_bombing_protection))
		{
			UpkeepValue *= 2f;
		}
		if (IsCommander)
		{
			if (PlayerSettings.Instance.Policies)
			{
				UpkeepValue = 15f;
			}
			if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Commanders_development_program))
			{
				UpkeepValue *= 2f;
			}
		}
	}

	public int GetUpkeepCost(bool p_getReserveCost)
	{
		CalculateAndSetUpkeepValue();
		if (UpkeepValue <= 0f)
		{
			return 0;
		}
		CalculateAndSetUnitPrice();
		if (p_getReserveCost && IsReserve)
		{
			if (TurnToProduce > 0)
			{
				return 0;
			}
			if (IsCoreUnit && GameData.Instance != null && GameData.Instance.map != null && GameData.Instance.map.IsCampaign)
			{
				return (int)Math.Round((float)Cost * (UpkeepValue / 100f) * 0.1f, MidpointRounding.ToEven);
			}
			return (int)Math.Round((float)Cost * (UpkeepValue / 100f) * 0.5f, MidpointRounding.ToEven);
		}
		return (int)Math.Round((float)Cost * (UpkeepValue / 100f), MidpointRounding.ToEven);
	}

	public void SetVeterancyOptionCost(int p_cost)
	{
		veterancyOptionCost = p_cost;
	}

	public int GetVeterancyOptionCost()
	{
		float num = 1f;
		if (baseUnitCost == 0)
		{
			Debug.LogError("No base unit cost set up for " + Name);
			return 0;
		}
		if (Type == "Infantry")
		{
			if (baseUnitCost < 100)
			{
				num = 3f;
			}
			else if (baseUnitCost <= 200)
			{
				num = 2f;
			}
			else if (baseUnitCost <= 300)
			{
				num = 2.5f;
			}
			else if (baseUnitCost > 300)
			{
				num = 2f;
			}
		}
		else if (!(Type == "Artillery") && !(Type == "AA"))
		{
			if (Type == "Tank")
			{
				if (baseUnitCost < 100)
				{
					num = 1f;
				}
				else if (baseUnitCost < 200)
				{
					num = 1.25f;
				}
				else if (baseUnitCost < 300)
				{
					num = 1.3f;
				}
				else if (baseUnitCost < 400)
				{
					num = 1.35f;
				}
				else
				{
					num = ((baseUnitCost < 500) ? 1f : 1.4f);
				}
			}
			else if (Type == "Plane")
			{
				if (baseUnitCost < 100)
				{
					num = 2f;
				}
				else if (baseUnitCost < 200)
				{
					num = 1.5f;
				}
				else if (baseUnitCost < 300)
				{
					num = 1.5f;
				}
				else
				{
					num = ((baseUnitCost < 500) ? 1f : 1f);
				}
			}
			else if (Type == "Boat")
			{
				if (IsSubmarine)
				{
					num = 1f;
				}
				else
				{
					num = ((!IsDestroyer) ? 1.5f : 1.3f);
				}
			}
			else if (baseUnitCost >= 1000)
			{
				num = 1.5f;
			}
			else if (baseUnitCost >= 500)
			{
				num = 2f;
			}
			else if (baseUnitCost >= 300)
			{
				num = 1.5f;
			}
			else
			{
				num = ((baseUnitCost < 100) ? 5f : 3f);
			}
		}
		else if (baseUnitCost < 100)
		{
			num = 2.5f;
		}
		else if (baseUnitCost < 200)
		{
			num = 2f;
		}
		else if (baseUnitCost < 300)
		{
			num = 1.8f;
		}
		else if (baseUnitCost >= 500)
		{
			num = 1.5f;
		}
		return (int)Math.Round((float)baseUnitCost * num, MidpointRounding.ToEven);
	}

	public bool IsSpecialUnit()
	{
		string text = GetNameToLower();
		if (text.Contains("rangers"))
		{
			return true;
		}
		if (text.Contains("grenadier"))
		{
			return true;
		}
		if (text.Contains("commando"))
		{
			return true;
		}
		if (text.Contains("nkvd"))
		{
			return true;
		}
		if (text.Contains("shock"))
		{
			return true;
		}
		if (text.Contains("imperial guards"))
		{
			return true;
		}
		return false;
	}

	public int GenerateNewID()
	{
		ID = GetNewID32();
		return ID;
	}

	public static int GetNewID32()
	{
		int num = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
		if (num != 0)
		{
			return num;
		}
		return 1;
	}

	public bool CanMoveOnRivers()
	{
		if (!IsAmphibious && !IsPatrolBoat && !IsPontoonBridge)
		{
			return Type == "Plane";
		}
		return true;
	}

	public bool CanMoveOnMarshes()
	{
		if (!(Type == "Plane"))
		{
			if (!IsMotorized && !IsMechanized && !IsHorsed && Type != "Tank")
			{
				return FilterType == "FilterInfantry";
			}
			return false;
		}
		return true;
	}

	public bool CanMoveOverMountains()
	{
		if (!IsMountaineer)
		{
			return Type == "Plane";
		}
		return true;
	}

	public string GetSuffix()
	{
		int categoryID = CategoryID;
		switch (PlayerSettings.Instance.Language)
		{
		default:
		{
			int num = categoryID % 100;
			if (num < 11 || num > 13)
			{
				switch (num % 10)
				{
				case 1:
					return "st";
				case 2:
					return "nd";
				case 3:
					return "rd";
				}
			}
			return "th";
		}
		case GameLanguages.FR:
			return "e";
		case GameLanguages.RU:
			return ".";
		case GameLanguages.UA:
			return ".";
		case GameLanguages.DE:
			return ".";
		case GameLanguages.ES:
			return "º";
		case GameLanguages.IT:
			return "º";
		case GameLanguages.PL:
			return ".";
		case GameLanguages.PT:
		case GameLanguages.BR:
			return "º";
		case GameLanguages.CZ:
			return ".";
		case GameLanguages.CNS:
		case GameLanguages.CNT:
			return "第";
		case GameLanguages.TR:
			return ".";
		case GameLanguages.FI:
			return ".";
		case GameLanguages.JP:
			return "第";
		case GameLanguages.SE:
			return ".";
		case GameLanguages.KR:
			return "제";
		case GameLanguages.GR:
			return "ος";
		case GameLanguages.NL:
			return "e";
		case GameLanguages.NO:
			return ".";
		case GameLanguages.CA:
			return "è";
		}
	}

	public void SetUnitCustomName()
	{
		PlayerSettings instance = PlayerSettings.Instance;
		bool unitSuffix = instance.UnitSuffix;
		bool quick2RespondOption = instance.Quick2RespondOption;
		if (!unitSuffix && !quick2RespondOption)
		{
			CustomName = TryTranslateUnitName(Name);
			return;
		}
		if (Name.Contains("alloon", StringComparison.OrdinalIgnoreCase) || FilterType == "FilterBuilding" || IsCommander || IsPartisans || IsMissile || IsConvoy || TileBuilding || MaxMP == 0)
		{
			CategoryID = 0;
			CustomName = TryTranslateUnitName(Name);
			return;
		}
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string name = Name;
		if (IsPoliticalUnit)
		{
			if (Country == "Nazi Germany")
			{
				text = LocalizationManager.Translate("unit.SS");
			}
			else if (Country == "Soviet Union")
			{
				text = LocalizationManager.Translate("unit.guards");
			}
		}
		name = TryTranslateUnitName(name);
		if (quick2RespondOption)
		{
			switch (FilterType)
			{
			case "FilterInfantry":
				if (IsRecon)
				{
					name = LocalizationManager.Translate("unit.reconBrigade");
				}
				else if (GetNameToLower().Contains("garrison"))
				{
					name = LocalizationManager.Translate("unit.garrison");
				}
				else
				{
					text3 = LocalizationManager.Translate("unit.division");
				}
				break;
			case "FilterArtillery":
				text3 = LocalizationManager.Translate("unit.brigade");
				if (Armour > 0)
				{
					text2 = LocalizationManager.Translate("unit.armored");
				}
				if (SoftDamage < 25)
				{
					name = ((SoftDamage < 18) ? LocalizationManager.Translate("unit.lightArtillery") : LocalizationManager.Translate("unit.mediumArtillery"));
				}
				else
				{
					name = LocalizationManager.Translate("unit.heavyArtillery");
				}
				break;
			case "FilterAntitank":
				text3 = LocalizationManager.Translate("unit.brigade");
				if (Armour > 0)
				{
					text2 = LocalizationManager.Translate("unit.armored");
				}
				if (HardDamage <= 80)
				{
					name = ((HardDamage <= 60) ? LocalizationManager.Translate("unit.lightAT") : LocalizationManager.Translate("unit.AT"));
				}
				else
				{
					name = LocalizationManager.Translate("unit.heavyAT");
				}
				break;
			case "FilterTank":
				text3 = LocalizationManager.Translate("unit.brigade");
				if (!IsCavalry)
				{
					if (IsRecon)
					{
						name = LocalizationManager.Translate("unit.armoredRecon");
					}
					else if (Armour <= 70)
					{
						name = ((Armour < 35) ? LocalizationManager.Translate("unit.lightTank") : LocalizationManager.Translate("unit.mediumTank"));
					}
					else
					{
						name = LocalizationManager.Translate("unit.heavyTank");
					}
				}
				break;
			case "FilterAA":
				text3 = LocalizationManager.Translate("unit.brigade");
				if (Armour > 0)
				{
					text2 = LocalizationManager.Translate("unit.armored");
				}
				if (AirDamage <= 30)
				{
					name = ((AirDamage < 20) ? LocalizationManager.Translate("unit.lightAA") : LocalizationManager.Translate("unit.mediumAA"));
				}
				else
				{
					name = LocalizationManager.Translate("unit.heavyAA");
				}
				break;
			case "FilterFighter":
				if (IsRecon)
				{
					name = LocalizationManager.Translate("unit.reconSquadron");
				}
				else
				{
					name = ((SoftDamage < 30) ? LocalizationManager.Translate("unit.fighterSquadron") : LocalizationManager.Translate("unit.heavyFighterSquadron"));
				}
				break;
			case "FilterCAS":
				name = LocalizationManager.Translate("unit.tacticalBomberSquadron");
				break;
			case "FilterBomber":
				name = ((!IsSupplyShip) ? LocalizationManager.Translate("unit.strategicBomberSquadron") : LocalizationManager.Translate("unit.supplyPlanes"));
				break;
			}
		}
		if (quick2RespondOption)
		{
			text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
			text2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text2);
			text3 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text3);
			name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
		}
		string text4;
		switch (instance.Language)
		{
		default:
			text4 = JoinTokens(new string[4] { text, text2, name, text3 });
			break;
		case GameLanguages.RU:
		case GameLanguages.ES:
		case GameLanguages.IT:
		case GameLanguages.PT:
		case GameLanguages.GR:
		case GameLanguages.CA:
		case GameLanguages.BR:
			text4 = JoinTokens(new string[4] { text3, text, text2, name });
			break;
		case GameLanguages.FR:
			text4 = JoinTokens(new string[4] { text3, text2, text, name });
			break;
		}
		string text5 = string.Empty;
		if (unitSuffix)
		{
			text5 = ((instance.Language != GameLanguages.KR && instance.Language != GameLanguages.JP && instance.Language != GameLanguages.CNS && instance.Language != GameLanguages.CNT) ? $"{CategoryID}{GetSuffix()} " : $"{GetSuffix()}{CategoryID} ");
		}
		CustomName = text5 + text4;
		string JoinTokens(IEnumerable<string> tokens)
		{
			return string.Join(" ", tokens.Where((string t) => !string.IsNullOrWhiteSpace(t)));
		}
	}

	public static string TryTranslateUnitName(string p_name)
	{
		string text = "unit.name." + p_name;
		string text2 = LocalizationManager.Translate(text);
		if (!(text2 != text))
		{
			return p_name;
		}
		return text2;
	}

	public UnitCategories GetUnitCategory()
	{
		if (IsCommander)
		{
			return UnitCategories.COMMANDER;
		}
		if (IsPartisans)
		{
			return UnitCategories.PARTISAN;
		}
		if (IsCavalry)
		{
			return UnitCategories.CAVALRY;
		}
		if (IsArmouredTrain)
		{
			return UnitCategories.ARMOURED_TRAIN;
		}
		switch (FilterType)
		{
		case "FilterInfantry":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_INFANTRY;
			}
			if (IsSpecialUnit())
			{
				return UnitCategories.SPECIAL_UNIT;
			}
			if (IsParatrooper)
			{
				return UnitCategories.PARATROOPER;
			}
			if (IsEngineers)
			{
				return UnitCategories.ENGINEER;
			}
			if (GetNameToLower().Contains("garrison"))
			{
				return UnitCategories.GARRISON;
			}
			if (GetNameToLower().Contains("marines"))
			{
				return UnitCategories.MARINE;
			}
			if (GetNameToLower().Contains("militia") || GetNameToLower().Contains("conscript") || GetNameToLower().Contains("volkssturm"))
			{
				return UnitCategories.MILITIA;
			}
			if (GetNameToLower().Contains("at infantry"))
			{
				return UnitCategories.INFANTRY_AT;
			}
			return UnitCategories.INFANTRY;
		case "FilterTank":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_TANK;
			}
			return UnitCategories.TANK;
		case "FilterAA":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_AA;
			}
			if (Type == "Tank")
			{
				return UnitCategories.SPG_AA;
			}
			return UnitCategories.AA;
		case "FilterAntitank":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_AT;
			}
			if (Type == "Tank")
			{
				return UnitCategories.SPG_AT;
			}
			return UnitCategories.AT;
		case "FilterArtillery":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_ARTILLERY;
			}
			if (Type == "Tank")
			{
				return UnitCategories.SPG_ARTY;
			}
			return UnitCategories.ARTILLERY;
		case "FilterFighter":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_FIGHTER;
			}
			return UnitCategories.FIGHTER;
		case "FilterCAS":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_LIGHT_BOMBER;
			}
			return UnitCategories.LIGHT_BOMBER;
		case "FilterBomber":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_HEAVY_BOMBER;
			}
			return UnitCategories.HEAVY_BOMBER;
		case "FilterBoat":
			if (IsPrototype)
			{
				return UnitCategories.PROTO_SHIP;
			}
			if (IsCarrier)
			{
				return UnitCategories.CARRIER;
			}
			if (IsSubmarine)
			{
				return UnitCategories.SUBMARINE;
			}
			if (IsPatrolBoat)
			{
				return UnitCategories.GUNBOAT;
			}
			if (IsDestroyer)
			{
				return UnitCategories.DESTROYER;
			}
			if (GetNameToLower().Contains("light"))
			{
				return UnitCategories.LIGHT_CRUISER;
			}
			if (GetNameToLower().Contains("heavy"))
			{
				return UnitCategories.HEAVY_CRUISER;
			}
			if (GetNameToLower().Contains("dreadnought"))
			{
				return UnitCategories.DREADNOUGHT;
			}
			if (GetNameToLower().Contains("battlecruiser"))
			{
				return UnitCategories.BATTLECRUISER;
			}
			if (GetNameToLower().Contains("battleship"))
			{
				return UnitCategories.BATTLESHIP;
			}
			return UnitCategories.SHIP;
		case "FilterBuilding":
			return UnitCategories.BUILDING;
		default:
			return UnitCategories.DEFAULT;
		}
	}

	public void SetDefaultAuthorizedRotation()
	{
		if (!Enum.IsDefined(typeof(AllowedRotations), AllowedRotation) || AllowedRotation == AllowedRotations.NOT_SETUP)
		{
			if (IsCommander || FilterType == "FilterBoat")
			{
				AllowedRotation = AllowedRotations.NONE;
			}
			else if (FilterType == "FilterFighter" || FilterType == "FilterCAS" || FilterType == "FilterBomber")
			{
				AllowedRotation = AllowedRotations.ALL;
			}
			else
			{
				AllowedRotation = AllowedRotations.LEFT_RIGHT;
			}
		}
	}

	public static string GetAuthorizedRotationLocalizationKey(AllowedRotations p_allowedRotation)
	{
		switch (p_allowedRotation)
		{
		default:
			return "txt-not setup";
		case AllowedRotations.NONE:
			return "txt-no rotation";
		case AllowedRotations.ALL:
			return "txt-all directions";
		case AllowedRotations.LEFT_RIGHT:
			return "txt-left right only";
		}
	}

	public static string GetAvailabilityOptionLocalizationKey(AvailabilityOptions p_availabilityOption)
	{
		switch (p_availabilityOption)
		{
		default:
			return "txt-no one";
		case AvailabilityOptions.ALL:
			return "txt-everyone";
		case AvailabilityOptions.AI:
			return "txt-AI only";
		case AvailabilityOptions.HUMANS:
			return "txt-humans only";
		}
	}

	public MoraleStates GetMoraleState()
	{
		if (Morale >= 85)
		{
			return MoraleStates.EXCELLENT;
		}
		if (Morale >= 60)
		{
			return MoraleStates.GOOD;
		}
		if (Morale <= 20)
		{
			return MoraleStates.AWFUL;
		}
		if (Morale <= 40)
		{
			return MoraleStates.BAD;
		}
		return MoraleStates.NORMAL;
	}

	public string GetFormattedManpowerCost()
	{
		if (manpowerCost == 0)
		{
			return "0";
		}
		return Utils.Truncate((float)manpowerCost / 1000f, 2) + " k";
	}

	public bool CanMove()
	{
		if (ActionPoints <= 0)
		{
			return false;
		}
		if (MaxMP <= 0)
		{
			return false;
		}
		if (CurrMP <= 0)
		{
			return false;
		}
		if (NumberOfTimesTheUnitMoved >= 2)
		{
			return false;
		}
		if (IsRecon)
		{
			if ((IsOnBoat || IsTrain) && NumberOfTimesTheUnitMoved > 0)
			{
				return false;
			}
		}
		else
		{
			if (NumberOfTimesTheUnitMoved < 2 && OwnerPlayer.HasPolicy(Policies.Type.Careful_planning))
			{
				return true;
			}
			if (HasMoved)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsConsideredHeavyEquipment()
	{
		if (GetArmour() > 20)
		{
			return true;
		}
		if (SoftDamage > 15 && FilterType == "FilterArtillery")
		{
			return true;
		}
		if (HardDamage > 60 && FilterType == "FilterAntitank")
		{
			return true;
		}
		return false;
	}

	public string GetMovementSound()
	{
		if (IsTrain)
		{
			return "Train";
		}
		if (IsDropable)
		{
			return "Heavy plane";
		}
		if (IsOnBoat)
		{
			return "Small boat";
		}
		if (IsHorsed)
		{
			return "Horse";
		}
		if (IsMotorized)
		{
			return "Motorized";
		}
		if (IsMechanized)
		{
			return "Mechanized";
		}
		return MovementSound;
	}

	public string GetCounterDisplayName()
	{
		if (FilterType != "FilterBoat" || IsCommander)
		{
			return string.Empty;
		}
		string text = GetNameToLower();
		if (text.Contains("convoy"))
		{
			return "CONVOY";
		}
		string text2 = text;
		string[] array = new string[22]
		{
			"class", "gunboat", "torpedo", "frigate", "destroyer", "submarine", "u-boot", "dreadnought", "battleship", "battlecruiser",
			"carrier", "escort carrier", "auxiliary", "cruiser", "light", "heavy", "coastal defence", "minesweeper", "boat", "motor",
			"gun", "corvette"
		};
		foreach (string oldValue in array)
		{
			text2 = text2.Replace(oldValue, string.Empty);
		}
		string text3 = ((!string.IsNullOrEmpty(text3 = (text.Contains("destroyer") ? "DD" : (text.Contains("frigate") ? "FF" : (text.Contains("corvette") ? "FS" : (text.Contains("minesweeper") ? "MS" : ((text.Contains("destroyer") && text.Contains("escort")) ? "DE" : ((text.Contains("submarine") || text.Contains("u-boot")) ? "SS" : (text.Contains("dreadnought") ? "DN" : (text.Contains("battleship") ? "BB" : (text.Contains("battlecruiser") ? "BC" : ((text.Contains("light") && text.Contains("cruiser")) ? "CL" : ((text.Contains("heavy") && text.Contains("cruiser")) ? "CA" : (text.Contains("auxiliary") ? "AX" : ((text.Contains("escort") && text.Contains("carrier")) ? "CVL" : (text.Contains("carrier") ? "CV" : (text.Contains("coastal") ? "CD" : ((text.Contains("motor") || text.Contains("gun")) ? "PG" : ((text.Contains("torpedo") && text.Contains("boat")) ? "PT" : string.Empty))))))))))))))))))) ? (text3 + " - " + text2) : text2);
		return text3.Trim().ToUpper();
	}

	public int GetUnitDangerValue()
	{
		return SoftDamage + HardDamage + AirDamage + (int)((float)Cost * 0.1f) + (int)((float)Armour * 2f);
	}

	public int GetRedeployCost()
	{
		return (int)Math.Round((float)Cost * 0.1f, MidpointRounding.ToEven);
	}

	public bool CanBeUpgraded()
	{
		if (IsTrain)
		{
			return false;
		}
		if (FilterType == "FilterBuilding")
		{
			return false;
		}
		if (unitGO != null)
		{
			if (FilterType == "FilterBoat")
			{
				return MapGO.IsThereACertainTileAround(unitGO.tileGO, "BigHarbour");
			}
			if (FilterType == "FilterFighter" || FilterType == "FilterCAS" || FilterType == "FilterBomber")
			{
				if (!MapGO.IsThereACertainTileAround(unitGO.tileGO, "Airport"))
				{
					return MapGO.IsThereACertainTileAround(unitGO.tileGO, "Reinforcement");
				}
				return true;
			}
			foreach (Tile neighbour in unitGO.tileGO.tile.neighbours)
			{
				if (neighbour.GroundUnit != null && neighbour.GroundUnit.FilterType != "FilterBuilding" && OwnerPlayer.IsAtWarWith(neighbour.GroundUnit.OwnerPlayer))
				{
					return false;
				}
			}
			if (!unitGO.tileGO.tile.IsVictoryPoint)
			{
				return false;
			}
			if (unitGO.tileGO.tile.NumberOfTurnsAtLowProd > 0)
			{
				return false;
			}
			if (!OwnerPlayer.IsAlliedWith(unitGO.tileGO.tile.ownerPlayer))
			{
				return false;
			}
			if (IsDropable)
			{
				return false;
			}
			if (IsOnBoat && !MapGO.IsThereACertainTileAround(unitGO.tileGO, "BigHarbour"))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanBeRedeployed()
	{
		if (IsReserve)
		{
			return false;
		}
		if (HasAttacked)
		{
			return false;
		}
		if (HasMoved)
		{
			return false;
		}
		if (FilterType == "FilterBuilding")
		{
			return false;
		}
		if (OwnerPlayer == null)
		{
			return false;
		}
		if (unitGO == null)
		{
			return false;
		}
		if (OwnerPlayer.MoneyAmount < GetRedeployCost())
		{
			return false;
		}
		if (OwnerPlayer.IsAtWarWith(unitGO.tileGO.tile.ownerPlayer))
		{
			return false;
		}
		if (Utils.GetAllTilesInRadiusAround(unitGO.tileGO.tile, 2).Any((Tile t) => (t.GroundUnit != null && OwnerPlayer.IsAtWarWith(t.GroundUnit.OwnerPlayer)) || (t.AirUnit != null && OwnerPlayer.IsAtWarWith(t.AirUnit.OwnerPlayer))))
		{
			return false;
		}
		return true;
	}

	public void RedeployUnit(bool p_removeMoney, bool p_sendRPC)
	{
		if (unitGO == null)
		{
			Debug.LogError("Trying to redeploy a unit with no unitGO");
			return;
		}
		if (IsTrain)
		{
			EmbarkDisembarkTrain();
		}
		else if (IsOnBoat)
		{
			EmbarkDisembarkBoat();
		}
		if (MapGO.selectedUnitGO == unitGO)
		{
			MapGO.instance.UnselectUnit();
		}
		if (Type == "Plane")
		{
			if (unitGO.tileGO.groundUnitGO != null)
			{
				unitGO.tileGO.groundUnitGO.gameObject.SetActive(value: true);
				unitGO.tileGO.groundUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
			}
			unitGO.tileGO.tile.AirUnit = null;
			unitGO.tileGO.airUnitGO = null;
		}
		else
		{
			if (unitGO.tileGO.airUnitGO != null)
			{
				unitGO.tileGO.airUnitGO.gameObject.SetActive(value: true);
				unitGO.tileGO.airUnitGO.twoUnitsIndicatorGO.SetActive(value: false);
			}
			unitGO.tileGO.tile.GroundUnit = null;
			unitGO.tileGO.groundUnitGO = null;
		}
		UnityEngine.Object.Destroy(unitGO.gameObject);
		if (IsCommander)
		{
			MapGO.listOfAllGeneralsOnTheMap.Remove(unitGO);
		}
		if (p_removeMoney)
		{
			OwnerPlayer.RemoveMoney(GetRedeployCost(), p_sendRPC: true);
		}
		IsReserve = true;
		Waypoints = new List<int[]>();
		hasBeenAttackedThisTurn = false;
		NextTurn();
		SetRedeploymentTime();
		EntrenchmentLevel = 0;
		if (!OwnerPlayer.IsComputer)
		{
			TutorialManager.PerformAction(TutorialActions.REDEPLOY_UNIT);
		}
		if (p_sendRPC)
		{
			MultiplayerManager.RPC("RPC_RedeployUnit", this);
		}
		ReservePanelGO.instance.GenerateButtonsList();
		ReservePanelGO.instance.RefreshDeployNotification();
		if (UIManager.instance.coreUnitsPanel.activeSelf)
		{
			UIManager.instance.GenerateCoreUnitsSlots();
		}
		UIManager.instance.RefreshCoreUnits();
		UIManager.instance.RefreshIncomePerTurnUI();
	}

	public void SetRedeploymentTime()
	{
		TurnToProduce = GetRedeploymentTime();
	}

	public byte GetRedeploymentTime()
	{
		if (FilterType == "FilterFighter")
		{
			return 1;
		}
		if (FilterType == "FilterCAS" || FilterType == "FilterBomber")
		{
			return 2;
		}
		if (FilterType == "FilterBoat")
		{
			return 4;
		}
		return 3;
	}

	public bool CanBeHorseTowed()
	{
		int num = ((GameData.Instance != null && GameData.Instance.map != null) ? GameData.Instance.map.Date.Year : 0);
		if (num >= 2000)
		{
			return false;
		}
		if (IsCavalry)
		{
			return false;
		}
		if (IsArmouredTrain)
		{
			return false;
		}
		if (HardDamage >= 90)
		{
			return false;
		}
		if (Type == "Infantry")
		{
			return false;
		}
		if (FilterType == "FilterBuilding")
		{
			return false;
		}
		if (Type == "Tank" && MaxMP > 1)
		{
			return false;
		}
		if (Type == "Plane" || Type == "Boat")
		{
			return false;
		}
		if ((Type == "Artillery" || Type == "AA") && MaxMP > 1)
		{
			return false;
		}
		if (FilterType == "FilterArtillery" && SoftDamage > 20 && num > 1935 && Range > 1)
		{
			return false;
		}
		return true;
	}

	public bool CanBeMotorized()
	{
		if (((GameData.Instance != null && GameData.Instance.map != null) ? GameData.Instance.map.Date.Year : 0) < 1920)
		{
			return false;
		}
		if (IsCavalry)
		{
			return false;
		}
		if (IsArmouredTrain)
		{
			return false;
		}
		if (FilterType == "FilterBuilding")
		{
			return false;
		}
		if (Type == "Tank" && MaxMP > 1)
		{
			return false;
		}
		if (Type == "Plane" || Type == "Boat")
		{
			return false;
		}
		if ((Type == "Artillery" || Type == "AA") && MaxMP > 1)
		{
			return false;
		}
		if (Type == "Infantry" && !string.IsNullOrEmpty(MovementSound) && !MovementSound.ToLowerInvariant().Contains("infantry") && !MovementSound.ToLowerInvariant().Contains("march") && !MovementSound.ToLowerInvariant().Contains("leg") && !MovementSound.ToLowerInvariant().Contains("foot") && !MovementSound.ToLowerInvariant().Contains("feet"))
		{
			return false;
		}
		return true;
	}

	public bool CanBeMechanized()
	{
		int num = ((GameData.Instance != null && GameData.Instance.map != null) ? GameData.Instance.map.Date.Year : 0);
		if (num < 1939)
		{
			return false;
		}
		if (IsCavalry)
		{
			return false;
		}
		if (IsArmouredTrain)
		{
			return false;
		}
		if (FilterType == "FilterBuilding")
		{
			return false;
		}
		if (Type == "Tank" && MaxMP > 1)
		{
			return false;
		}
		if (Type == "Plane" || Type == "Boat")
		{
			return false;
		}
		if ((Type == "Artillery" || Type == "AA") && MaxMP > 1)
		{
			return false;
		}
		if (Type == "Infantry" && !string.IsNullOrEmpty(MovementSound) && !MovementSound.ToLowerInvariant().Contains("infantry") && !MovementSound.ToLowerInvariant().Contains("march") && !MovementSound.ToLowerInvariant().Contains("leg") && !MovementSound.ToLowerInvariant().Contains("foot") && !MovementSound.ToLowerInvariant().Contains("feet"))
		{
			return false;
		}
		if (OwnerPlayer != null)
		{
			switch (OwnerPlayer.Country)
			{
			case "Nazi Germany":
				return num >= 1939;
			case "USA":
				return num >= 1940;
			case "Great Britain":
				return num >= 1941;
			case "France":
				return num >= 1943;
			case "Soviet Union":
			case "Australia":
			case "Canada":
			case "New Zealand":
				return num >= 1942;
			case "Japan":
			case "Italy":
			case "Hungary":
			case "Finland":
			case "Romania":
			case "Bulgaria":
			case "China":
			case "Slovakia":
			case "Croatia":
				return num >= 1943;
			}
			if (num < 1946)
			{
				return false;
			}
		}
		return true;
	}

	public bool CanRemoveEntrenchment()
	{
		if (HasFlamethrower || FilterType == "FilterArtillery" || (FilterType == "FilterBoat" && !IsPatrolBoat && !IsCarrier && !IsSubmarine && !IsMine && !IsDestroyer && !GetNameToLower().Contains("light")) || FilterType == "FilterBomber" || FilterType == "FilterCAS" || (CoastalBattery && FilterType == "FilterBuilding"))
		{
			return true;
		}
		return false;
	}

	public int GetEntrenchmentRemoval(Unit p_targetUnit)
	{
		if (p_targetUnit == null)
		{
			return 0;
		}
		int result = 1;
		if (FilterType == "FilterArtillery")
		{
			if (BaseSoftDamage >= 30)
			{
				result = 3;
			}
			else if (BaseSoftDamage >= 20)
			{
				result = 2;
			}
		}
		else if (FilterType == "FilterBomber" && !HasTorpedo && !IsRecon)
		{
			result = 2;
		}
		return result;
	}

	public bool IsBlitzkriegDoctrineApplicable()
	{
		if (OwnerPlayer != null && OwnerPlayer.HasPolicy(Policies.Type.Blitzkrieg))
		{
			if ((!(Type == "Tank") || (!(FilterType == "FilterArtillery") && !(FilterType == "FilterTank"))) && (!(Type == "Infantry") || (!IsMechanized && !IsMotorized)))
			{
				return FilterType == "FilterCAS";
			}
			return true;
		}
		return false;
	}

	public bool IsAvailable(Player p_player)
	{
		if (IsRemoved)
		{
			return false;
		}
		if (p_player == null)
		{
			return AvailabilityOption != AvailabilityOptions.NONE;
		}
		if (AvailabilityOption != AvailabilityOptions.ALL && (AvailabilityOption != AvailabilityOptions.AI || !p_player.IsComputer))
		{
			if (AvailabilityOption == AvailabilityOptions.HUMANS)
			{
				return !p_player.IsComputer;
			}
			return false;
		}
		return true;
	}

	public string GetInformationRules()
	{
		string text = string.Empty;
		if (MoveOrAttack)
		{
			text += LocalizationManager.Translate("tooltip-info move or attack");
		}
		if (!string.IsNullOrEmpty(text))
		{
			return LocalizationManager.Translate("tooltip-special rules") + "\n" + text;
		}
		return string.Empty;
	}

	public string GetDeploymentRule()
	{
		string text = LocalizationManager.Translate("tooltip-info deployment rule") + "\n";
		if (Type == "Boat")
		{
			if (IsMine)
			{
				return text + LocalizationManager.Translate("tooltip-deploy rule naval mines");
			}
			return text + LocalizationManager.Translate("tooltip-deploy rule ships");
		}
		if (CoastalBattery)
		{
			return text + LocalizationManager.Translate("tooltip-deploy rule ships");
		}
		if (IsMissile)
		{
			return text + LocalizationManager.Translate("tooltip-deploy rule missiles");
		}
		if (BuiltByEngineers)
		{
			return text + LocalizationManager.Translate("tooltip-deploy rule buildings");
		}
		if (IsParatrooper || Type == "Plane")
		{
			if (CanBeRefueledOnCarrier)
			{
				return text + LocalizationManager.Translate("tooltip-deploy rule carrier planes");
			}
			if (IsCatapultPlane)
			{
				return text + LocalizationManager.Translate("tooltip-deploy rule catapult planes");
			}
			return text + LocalizationManager.Translate("tooltip-deploy rule default planes");
		}
		if (Armour > 20 || (FilterType == "FilterArtillery" && SoftDamage > 15) || (FilterType == "FilterAntitank" && HardDamage > 60))
		{
			return text + LocalizationManager.Translate("tooltip-deploy rule heavies");
		}
		return text + LocalizationManager.Translate("tooltip-deploy rule default");
	}

	public Unit Clone(bool p_generateNewID)
	{
		return new Unit
		{
			ID = (p_generateNewID ? GetNewID32() : ID),
			Name = Name,
			nameToLower = nameToLower,
			CustomName = CustomName,
			ForcedCustomName = ForcedCustomName,
			Country = Country,
			Type = Type,
			BaseType = BaseType,
			FilterType = FilterType,
			EndUseYear = EndUseYear,
			ProductionYear = ProductionYear,
			ProductionMonth = ProductionMonth,
			XP = XP,
			Lvl = Lvl,
			MaxHP = MaxHP,
			CurrHP = CurrHP,
			BaseMaxHP = BaseMaxHP,
			MaxMP = MaxMP,
			CurrMP = CurrMP,
			TampMP = TampMP,
			BaseMaxMP = BaseMaxMP,
			TampMaxMP = TampMaxMP,
			MaxAutonomy = MaxAutonomy,
			CurrAutonomy = CurrAutonomy,
			TampAutonomy = TampAutonomy,
			BaseMaxAutonomy = BaseMaxAutonomy,
			TampMaxAutonomy = TampMaxAutonomy,
			MaxAmmo = MaxAmmo,
			CurrAmmo = CurrAmmo,
			BaseMaxAmmo = BaseMaxAmmo,
			Cost = Cost,
			CustomCost = CustomCost,
			UpkeepValue = UpkeepValue,
			CustomUpkeepValue = CustomUpkeepValue,
			manpowerCost = manpowerCost,
			CustomManpowerCost = CustomManpowerCost,
			Armour = Armour,
			BaseArmour = BaseArmour,
			Visibility = Visibility,
			BaseVisibility = BaseVisibility,
			Range = Range,
			BaseRange = BaseRange,
			AirDamage = AirDamage,
			BaseAirDamage = BaseAirDamage,
			SoftDamage = SoftDamage,
			BaseSoftDamage = BaseSoftDamage,
			HardDamage = HardDamage,
			BaseHardDamage = BaseHardDamage,
			AttackSound = AttackSound,
			MovementSound = MovementSound,
			MaxPurchaseTime = MaxPurchaseTime,
			ProgressiveCost = ProgressiveCost,
			CustomTurnProduction = CustomTurnProduction,
			AIPurchaseChanceOverride = AIPurchaseChanceOverride,
			Waypoints = Waypoints,
			CommanderTargets = CommanderTargets,
			StuffThatHappened = StuffThatHappened,
			AllowedRotation = AllowedRotation,
			AvailabilityOption = AvailabilityOption,
			IsConvoy = IsConvoy,
			IsEngineers = IsEngineers,
			IsAmphibious = IsAmphibious,
			IsCommander = IsCommander,
			NoLendLease = NoLendLease,
			NoCapture = NoCapture,
			PermanentDeath = PermanentDeath,
			ImmuneToWinterDamage = ImmuneToWinterDamage,
			CanAttackShips = CanAttackShips,
			IsPrototype = IsPrototype,
			IsRecon = IsRecon,
			IsRADAR = IsRADAR,
			IsParatrooper = IsParatrooper,
			IsSubmarine = IsSubmarine,
			IsArmouredTrain = IsArmouredTrain,
			IsCatapultPlane = IsCatapultPlane,
			IsCarrier = IsCarrier,
			IsDestroyer = IsDestroyer,
			IsSupplyDepot = IsSupplyDepot,
			IsSupplyShip = IsSupplyShip,
			IsMineFlail = IsMineFlail,
			IsMine = IsMine,
			IsPartisans = IsPartisans,
			IsPatrolBoat = IsPatrolBoat,
			IsMissile = IsMissile,
			IsLaunchSite = IsLaunchSite,
			IsCavalry = IsCavalry,
			IsPontoonBridge = IsPontoonBridge,
			IsArmourObstacle = IsArmourObstacle,
			NoAmmoTurn1 = NoAmmoTurn1,
			BuiltByEngineers = BuiltByEngineers,
			TileBuilding = TileBuilding,
			CoastalBattery = CoastalBattery,
			CanLayMines = CanLayMines,
			HasFlamethrower = HasFlamethrower,
			CanCatapultPlanes = CanCatapultPlanes,
			CanBeRefueledOnCarrier = CanBeRefueledOnCarrier,
			CanCarryRockets = CanCarryRockets,
			CanCarryABomb = CanCarryABomb,
			CanCarryBombs = CanCarryBombs,
			CanCarryDropTank = CanCarryDropTank,
			CanCarryTorpedo = CanCarryTorpedo,
			MoveOrAttack = MoveOrAttack,
			Hero = Hero,
			Morale = Morale,
			ActionPoints = ActionPoints,
			EntrenchmentLevel = EntrenchmentLevel,
			NumberOfTurnsUnderWater = NumberOfTurnsUnderWater,
			OwnerName = OwnerName,
			OwnerPlayer = OwnerPlayer,
			IsOnBoat = IsOnBoat,
			IsTrain = IsTrain,
			IsHorsed = IsHorsed,
			IsMechanized = IsMechanized,
			IsMotorized = IsMotorized,
			IsCoreUnit = IsCoreUnit,
			IsDropable = IsDropable,
			IsMountaineer = IsMountaineer,
			IsWinterSpecialized = IsWinterSpecialized,
			IsReserve = IsReserve,
			IsKillGoal = IsKillGoal,
			AutoSupplies = AutoSupplies,
			IsEliteUnit = IsEliteUnit,
			IsPoliticalUnit = IsPoliticalUnit,
			HasSonar = HasSonar,
			HasABomb = HasABomb,
			HasBombs = HasBombs,
			HasTorpedo = HasTorpedo,
			HasDropTank = HasDropTank,
			HasRockets = HasRockets,
			AsleepState = AsleepState,
			TurnToProduce = TurnToProduce
		};
	}

	public void ApplyModdedUnitModifications(Unit p_sourceUnit)
	{
		Name = p_sourceUnit.Name;
		Country = p_sourceUnit.Country;
		ProductionYear = p_sourceUnit.ProductionYear;
		ProductionMonth = p_sourceUnit.ProductionMonth;
		EndUseYear = p_sourceUnit.EndUseYear;
		Type = p_sourceUnit.Type;
		BaseType = p_sourceUnit.Type;
		FilterType = p_sourceUnit.FilterType;
		AttackSound = p_sourceUnit.AttackSound;
		MovementSound = p_sourceUnit.MovementSound;
		AllowedRotation = p_sourceUnit.AllowedRotation;
		AvailabilityOption = p_sourceUnit.AvailabilityOption;
		MaxPurchaseTime = p_sourceUnit.MaxPurchaseTime;
		ProgressiveCost = p_sourceUnit.ProgressiveCost;
		CustomCost = p_sourceUnit.CustomCost;
		CustomManpowerCost = p_sourceUnit.CustomManpowerCost;
		CustomTurnProduction = p_sourceUnit.CustomTurnProduction;
		SoftDamage = p_sourceUnit.SoftDamage;
		HardDamage = p_sourceUnit.HardDamage;
		AirDamage = p_sourceUnit.AirDamage;
		Armour = p_sourceUnit.Armour;
		Range = p_sourceUnit.Range;
		Visibility = p_sourceUnit.Visibility;
		MaxAmmo = p_sourceUnit.MaxAmmo;
		MaxHP = p_sourceUnit.MaxHP;
		MaxMP = p_sourceUnit.MaxMP;
		MaxAutonomy = p_sourceUnit.MaxAutonomy;
		CanCarryDropTank = p_sourceUnit.CanCarryDropTank;
		CanCarryBombs = p_sourceUnit.CanCarryBombs;
		CanCarryTorpedo = p_sourceUnit.CanCarryTorpedo;
		CanCarryRockets = p_sourceUnit.CanCarryRockets;
		CanCarryABomb = p_sourceUnit.CanCarryABomb;
		IsCarrier = p_sourceUnit.IsCarrier;
		CanBeRefueledOnCarrier = p_sourceUnit.CanBeRefueledOnCarrier;
		IsSubmarine = p_sourceUnit.IsSubmarine;
		HasFlamethrower = p_sourceUnit.HasFlamethrower;
		IsArmouredTrain = p_sourceUnit.IsArmouredTrain;
		IsParatrooper = p_sourceUnit.IsParatrooper;
		IsRecon = p_sourceUnit.IsRecon;
		IsDestroyer = p_sourceUnit.IsDestroyer;
		IsCatapultPlane = p_sourceUnit.IsCatapultPlane;
		IsCommander = p_sourceUnit.IsCommander;
		IsPrototype = p_sourceUnit.IsPrototype;
		IsSupplyShip = p_sourceUnit.IsSupplyShip;
		IsRADAR = p_sourceUnit.IsRADAR;
		HasSonar = p_sourceUnit.HasSonar;
		IsSupplyDepot = p_sourceUnit.IsSupplyDepot;
		IsPartisans = p_sourceUnit.IsPartisans;
		IsMissile = p_sourceUnit.IsMissile;
		IsLaunchSite = p_sourceUnit.IsLaunchSite;
		IsMineFlail = p_sourceUnit.IsMineFlail;
		IsCavalry = p_sourceUnit.IsCavalry;
		IsPatrolBoat = p_sourceUnit.IsPatrolBoat;
		IsConvoy = p_sourceUnit.IsConvoy;
		IsEngineers = p_sourceUnit.IsEngineers;
		BuiltByEngineers = p_sourceUnit.BuiltByEngineers;
		IsArmourObstacle = p_sourceUnit.IsArmourObstacle;
		IsPontoonBridge = p_sourceUnit.IsPontoonBridge;
		IsAmphibious = p_sourceUnit.IsAmphibious;
		CustomUpkeepValue = p_sourceUnit.CustomUpkeepValue;
		NoCapture = p_sourceUnit.NoCapture;
		PermanentDeath = p_sourceUnit.PermanentDeath;
		AIPurchaseChanceOverride = p_sourceUnit.AIPurchaseChanceOverride;
		ImmuneToWinterDamage = p_sourceUnit.ImmuneToWinterDamage;
		CanAttackShips = p_sourceUnit.CanAttackShips;
		CommanderTargets = p_sourceUnit.CommanderTargets;
		MoveOrAttack = p_sourceUnit.MoveOrAttack;
	}

	public void PatchOnNetworkReceived()
	{
		Unit unit = Utils.GenerateNewUnit(Name, Country);
		BaseType = (string.IsNullOrEmpty(unit.BaseType) ? unit.Type : unit.BaseType);
		BaseMaxHP = unit.MaxHP;
		BaseArmour = unit.Armour;
		BaseAirDamage = unit.AirDamage;
		BaseSoftDamage = unit.SoftDamage;
		BaseHardDamage = unit.HardDamage;
		BaseRange = unit.Range;
		BaseVisibility = unit.Visibility;
		BaseMaxAmmo = unit.MaxAmmo;
		BaseMaxMP = unit.MaxMP;
		BaseMaxAutonomy = unit.MaxAutonomy;
		if (OwnerPlayer == null && !string.IsNullOrEmpty(OwnerName) && GameData.Instance.TryFindPlayerByName(OwnerName, out var o_player))
		{
			OwnerPlayer = o_player;
		}
		if (!(this.unitGO == null) || IsReserve)
		{
			return;
		}
		UnitGO[] componentsInChildren = UIManager.instance.unitsParent.GetComponentsInChildren<UnitGO>(includeInactive: true);
		foreach (UnitGO unitGO in componentsInChildren)
		{
			if (unitGO.unit.ID == ID)
			{
				this.unitGO = unitGO;
			}
		}
	}

	public void SyncFromNetwork(Unit p_sourceUnit, bool p_refreshGraphics)
	{
		Name = p_sourceUnit.Name;
		Country = p_sourceUnit.Country;
		CustomName = p_sourceUnit.CustomName;
		ForcedCustomName = p_sourceUnit.ForcedCustomName;
		OwnerName = p_sourceUnit.OwnerName;
		PatchOnNetworkReceived();
		Type = p_sourceUnit.Type;
		FilterType = p_sourceUnit.FilterType;
		CurrHP = p_sourceUnit.CurrHP;
		MaxHP = p_sourceUnit.MaxHP;
		Cost = p_sourceUnit.Cost;
		CustomCost = p_sourceUnit.CustomCost;
		CustomUpkeepValue = p_sourceUnit.CustomUpkeepValue;
		MaxPurchaseTime = p_sourceUnit.MaxPurchaseTime;
		TampAutonomy = p_sourceUnit.TampAutonomy;
		TampMaxAutonomy = p_sourceUnit.TampMaxAutonomy;
		TampMaxMP = p_sourceUnit.TampMaxMP;
		TampMP = p_sourceUnit.TampMP;
		CurrAmmo = p_sourceUnit.CurrAmmo;
		MaxAmmo = p_sourceUnit.MaxAmmo;
		CurrAutonomy = p_sourceUnit.CurrAutonomy;
		MaxAutonomy = p_sourceUnit.MaxAutonomy;
		HasBuilt = p_sourceUnit.HasBuilt;
		TotalKill = p_sourceUnit.TotalKill;
		TotalDistance = p_sourceUnit.TotalDistance;
		XP = p_sourceUnit.XP;
		Lvl = p_sourceUnit.Lvl;
		Hero = p_sourceUnit.Hero;
		IsPoliticalUnit = p_sourceUnit.IsPoliticalUnit;
		TileBuilding = p_sourceUnit.TileBuilding;
		Range = p_sourceUnit.Range;
		Armour = p_sourceUnit.Armour;
		Visibility = p_sourceUnit.Visibility;
		SoftDamage = p_sourceUnit.SoftDamage;
		HardDamage = p_sourceUnit.HardDamage;
		AirDamage = p_sourceUnit.AirDamage;
		IsReserve = p_sourceUnit.IsReserve;
		HasMoved = p_sourceUnit.HasMoved;
		HasAttacked = p_sourceUnit.HasAttacked;
		MoveOrAttack = p_sourceUnit.MoveOrAttack;
		AttackedAmountThisTurn = p_sourceUnit.AttackedAmountThisTurn;
		FacingRotation = p_sourceUnit.FacingRotation;
		AllowedRotation = p_sourceUnit.AllowedRotation;
		IsRecon = p_sourceUnit.IsRecon;
		IsMineFlail = p_sourceUnit.IsMineFlail;
		IsParatrooper = p_sourceUnit.IsParatrooper;
		IsPartisans = p_sourceUnit.IsPartisans;
		IsCarrier = p_sourceUnit.IsCarrier;
		IsCatapultPlane = p_sourceUnit.IsCatapultPlane;
		IsCavalry = p_sourceUnit.IsCavalry;
		IsArmouredTrain = p_sourceUnit.IsArmouredTrain;
		IsSupplyDepot = p_sourceUnit.IsSupplyDepot;
		CanLayMines = p_sourceUnit.CanLayMines;
		IsWinterSpecialized = p_sourceUnit.IsWinterSpecialized;
		IsPatrolBoat = p_sourceUnit.IsPatrolBoat;
		IsPrototype = p_sourceUnit.IsPrototype;
		IsMountaineer = p_sourceUnit.IsMountaineer;
		AsleepState = p_sourceUnit.AsleepState;
		IsLaunchSite = p_sourceUnit.IsLaunchSite;
		IsMissile = p_sourceUnit.IsMissile;
		ActionPoints = p_sourceUnit.ActionPoints;
		HasTorpedo = p_sourceUnit.HasTorpedo;
		HasRockets = p_sourceUnit.HasRockets;
		HasBombs = p_sourceUnit.HasBombs;
		HasABomb = p_sourceUnit.HasABomb;
		HasDropTank = p_sourceUnit.HasDropTank;
		HasFlamethrower = p_sourceUnit.HasFlamethrower;
		HasSonar = p_sourceUnit.HasSonar;
		IsEliteUnit = p_sourceUnit.IsEliteUnit;
		IsMechanized = p_sourceUnit.IsMechanized;
		IsMotorized = p_sourceUnit.IsMotorized;
		IsHorsed = p_sourceUnit.IsHorsed;
		IsOnBoat = p_sourceUnit.IsOnBoat;
		IsDestroyer = p_sourceUnit.IsDestroyer;
		CanBeRefueledOnCarrier = p_sourceUnit.CanBeRefueledOnCarrier;
		IsRADAR = p_sourceUnit.IsRADAR;
		IsConvoy = p_sourceUnit.IsConvoy;
		BuiltByEngineers = p_sourceUnit.BuiltByEngineers;
		IsCommander = p_sourceUnit.IsCommander;
		EntrenchmentLevel = p_sourceUnit.EntrenchmentLevel;
		Morale = p_sourceUnit.Morale;
		CanCarryABomb = p_sourceUnit.CanCarryABomb;
		CanCarryABomb = p_sourceUnit.CanCarryABomb;
		CanCarryDropTank = p_sourceUnit.CanCarryDropTank;
		CanCarryRockets = p_sourceUnit.CanCarryRockets;
		CanCarryTorpedo = p_sourceUnit.CanCarryTorpedo;
		CanCatapultPlanes = p_sourceUnit.CanCatapultPlanes;
		CoastalBattery = p_sourceUnit.CoastalBattery;
		IsPontoonBridge = p_sourceUnit.IsPontoonBridge;
		IsArmourObstacle = p_sourceUnit.IsArmourObstacle;
		IsEngineers = p_sourceUnit.IsEngineers;
		IsAmphibious = p_sourceUnit.IsAmphibious;
		NoCapture = p_sourceUnit.NoCapture;
		NoLendLease = p_sourceUnit.NoLendLease;
		PermanentDeath = p_sourceUnit.PermanentDeath;
		ImmuneToWinterDamage = p_sourceUnit.ImmuneToWinterDamage;
		AIPurchaseChanceOverride = p_sourceUnit.AIPurchaseChanceOverride;
		CanAttackShips = p_sourceUnit.CanAttackShips;
		IsDropable = p_sourceUnit.IsDropable;
		CommanderTargets = p_sourceUnit.CommanderTargets;
		AAKill = p_sourceUnit.AAKill;
		AntitankKill = p_sourceUnit.AntitankKill;
		ArtilleryKill = p_sourceUnit.ArtilleryKill;
		BoatKill = p_sourceUnit.BoatKill;
		CASKill = p_sourceUnit.CASKill;
		FighterKill = p_sourceUnit.FighterKill;
		HeavyBomberKill = p_sourceUnit.HeavyBomberKill;
		InfantryKill = p_sourceUnit.InfantryKill;
		TankKill = p_sourceUnit.TankKill;
		Waypoints = p_sourceUnit.Waypoints;
		StuffThatHappened = p_sourceUnit.StuffThatHappened;
		if (!IsReserve && unitGO != null)
		{
			if (p_refreshGraphics)
			{
				unitGO.SetSprite();
			}
			else
			{
				unitGO.UpdateCounter();
			}
		}
	}

	private void CheckAchievements()
	{
		if (GameData.Instance.ironManMode && OwnerPlayer != null && !OwnerPlayer.IsComputer)
		{
			if (MapGO.selectedUnitGO != null && MapGO.selectedUnitGO.targetedUnitGO != null && (MapGO.selectedUnitGO.targetedUnitGO.unit.CurrAmmo <= 0 || MapGO.selectedUnitGO.targetedUnitGO.unit.CurrAutonomy <= 0))
			{
				SteamManager.UnlockAchievement("AttritionWar");
			}
			if (Hero != null)
			{
				SteamManager.UnlockAchievement("GetHero");
			}
			if (Name.Contains("battleship") && (InfantryKill > 0 || TankKill > 0 || ArtilleryKill > 0 || AntitankKill > 0))
			{
				SteamManager.UnlockAchievement("Shore bombardment");
			}
		}
	}
}

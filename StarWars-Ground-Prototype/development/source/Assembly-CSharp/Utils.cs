using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Utils
{
	private enum FloodFillTypes
	{
		LAND,
		SEA,
		RIVER
	}

	public const double KB = 1024.0;

	public const double MB = 1048576.0;

	private static Stopwatch stopwatch;

	private static RNGCryptoServiceProvider RNGcsp = new RNGCryptoServiceProvider();

	public static void StartStopWatch()
	{
		stopwatch = new Stopwatch();
		stopwatch.Start();
	}

	public static string ShowStopWatch()
	{
		UnityEngine.Debug.Log(stopwatch.Elapsed.TotalMilliseconds + " ms");
		return stopwatch.Elapsed.TotalMilliseconds + " ms";
	}

	public static Unit GenerateNewUnit(string p_unitName, string p_unitCountry)
	{
		List<Unit> allUnits = GetAllUnits();
		Unit unit = new Unit();
		foreach (Unit item in allUnits)
		{
			if (!(item.Country != p_unitCountry) && !(item.Name != p_unitName))
			{
				unit = item.Clone(p_generateNewID: true);
				unit.Lvl = 1;
				unit.XP = 0;
				unit.Morale = 75;
				unit.ActionPoints = 2;
				unit.TampMP = item.MaxMP;
				unit.TampMaxMP = item.MaxMP;
				unit.TampAutonomy = item.MaxAutonomy;
				unit.TampMaxAutonomy = item.MaxAutonomy;
				if (unit.IsParatrooper)
				{
					unit.ApplyModification(Database.planeTransportOption);
					unit.CurrAutonomy = unit.MaxAutonomy;
					unit.CurrMP = unit.MaxMP;
				}
				unit.CustomName = null;
				unit.GenerateNewID();
				unit.CalculateAndSetUnitPrice();
				unit.CalculateAndSetUpkeepValue();
				return unit;
			}
		}
		UnityEngine.Debug.LogError("Could not find the unit to generate: " + p_unitName + " " + p_unitCountry);
		return unit;
	}

	public static void ReplaceOutdatedUnit(Unit p_unit)
	{
		if (!IsModdedUnit(p_unit))
		{
			string name = p_unit.Name;
			p_unit.Name = GetUpdatedUnitName(p_unit.Name, p_unit.Country);
			if (p_unit.Name != name)
			{
				p_unit.SetUnitCustomName();
			}
		}
	}

	public static string GetUpdatedUnitName(string p_unitName, string p_country)
	{
		if (string.IsNullOrEmpty(p_unitName))
		{
			return p_unitName;
		}
		string text = p_unitName;
		switch (text)
		{
		case "Kavallerie":
			text = "Cavalry";
			break;
		case "Desantniki":
			text = "Paratroopers";
			break;
		case "Paracadutisti":
			text = "Paratroopers";
			break;
		case "Panzergrenadier":
			text = "Panzergrenadiers";
			break;
		case "M4A4 Sherman Mk. V Tulip":
			text = "Sherman V Tulip";
			goto default;
		case "Marmon-Herrington Armoured Car Mk. IV":
			text = "Marmon–Herrington Mk IV";
			goto default;
		case "M4A4 Firefly":
			switch (p_country)
			{
			case "United Kingdom":
				text = "Sherman VC";
				break;
			case "Canada":
				text = "Sherman VC";
				break;
			case "Australia":
				text = "Churchill Mk. IV";
				break;
			default:
				text = "Sherman VC";
				break;
			}
			goto default;
		default:
			if (text == "M4 Sherman")
			{
				text = "Sherman M4A4 75mm";
			}
			if (text == "M4A3")
			{
				text = "Sherman M4A3 75mm";
			}
			if (text == "M4A3 105mm")
			{
				text = "Sherman M4A3 105mm";
			}
			if (text == "M4A3E8(76)W")
			{
				text = "Sherman M4A3E8 76mm";
			}
			if (text == "M4A3E2 Jumbo" || text == "M4A3E2(76)W Jumbo")
			{
				text = "Sherman M4A3E2 75mm Jumbo";
			}
			if (text == "T34 Calliope")
			{
				text = "M4A3 T34 Calliope";
			}
			if (text == "T40 Whizbang")
			{
				text = "M4 T40 M17 Whizbang";
			}
			if (text == "Sherman VC")
			{
				text = "Sherman VC Firefly";
			}
			break;
		}
		return text;
	}

	public static void UpdateUnitStatsOnTurnOne(Unit p_unit, List<Unit> p_unitsDatabase)
	{
		ReplaceOutdatedUnit(p_unit);
		foreach (Unit item in p_unitsDatabase)
		{
			if (item.Country != p_unit.Country || item.Name != p_unit.Name)
			{
				continue;
			}
			Unit unit = item.Clone(p_generateNewID: false);
			unit.Lvl = 1;
			unit.XP = 0;
			bool flag = p_unit.CurrHP == p_unit.MaxHP;
			float num = Mathf.Clamp01((float)p_unit.CurrHP / (float)p_unit.MaxHP);
			p_unit.Lvl = (byte)Mathf.Clamp(p_unit.Lvl, 1, Unit.MAX_LEVEL);
			p_unit.ProductionYear = unit.ProductionYear;
			p_unit.ProductionMonth = unit.ProductionMonth;
			p_unit.EndUseYear = unit.EndUseYear;
			p_unit.Type = unit.Type;
			p_unit.BaseType = (string.IsNullOrEmpty(unit.BaseType) ? unit.Type : unit.BaseType);
			p_unit.FilterType = unit.FilterType;
			p_unit.AttackSound = unit.AttackSound;
			p_unit.MovementSound = unit.MovementSound;
			p_unit.AvailabilityOption = unit.AvailabilityOption;
			p_unit.AllowedRotation = unit.AllowedRotation;
			p_unit.Armour = unit.Armour;
			p_unit.BaseArmour = unit.BaseArmour;
			p_unit.MaxAmmo = unit.MaxAmmo;
			p_unit.BaseMaxAmmo = unit.BaseMaxAmmo;
			p_unit.Range = unit.Range;
			p_unit.BaseRange = unit.BaseRange;
			p_unit.Visibility = unit.Visibility;
			p_unit.BaseVisibility = unit.Visibility;
			p_unit.MaxHP = unit.MaxHP;
			p_unit.BaseMaxHP = unit.MaxHP;
			p_unit.MaxMP = unit.MaxMP;
			p_unit.BaseMaxMP = unit.MaxMP;
			p_unit.MaxAutonomy = unit.MaxAutonomy;
			p_unit.BaseMaxAutonomy = unit.MaxAutonomy;
			p_unit.SoftDamage = unit.SoftDamage;
			p_unit.BaseSoftDamage = unit.SoftDamage;
			p_unit.HardDamage = unit.HardDamage;
			p_unit.BaseHardDamage = unit.HardDamage;
			p_unit.AirDamage = unit.AirDamage;
			p_unit.BaseAirDamage = unit.AirDamage;
			p_unit.MaxPurchaseTime = unit.MaxPurchaseTime;
			p_unit.ProgressiveCost = unit.ProgressiveCost;
			p_unit.CustomCost = unit.CustomCost;
			p_unit.CustomManpowerCost = unit.CustomManpowerCost;
			p_unit.Cost = unit.Cost;
			p_unit.CustomUpkeepValue = unit.CustomUpkeepValue;
			p_unit.CustomTurnProduction = unit.CustomTurnProduction;
			p_unit.IsPartisans = unit.IsPartisans;
			p_unit.IsMine = unit.IsMine;
			p_unit.IsRADAR = unit.IsRADAR;
			p_unit.HasSonar = unit.HasSonar;
			p_unit.IsSupplyDepot = unit.IsSupplyDepot;
			p_unit.IsLaunchSite = unit.IsLaunchSite;
			p_unit.CanCarryABomb = unit.CanCarryABomb;
			p_unit.CanCarryBombs = unit.CanCarryBombs;
			p_unit.CanCarryDropTank = unit.CanCarryDropTank;
			p_unit.CanCarryRockets = unit.CanCarryRockets;
			p_unit.CanCarryTorpedo = unit.CanCarryTorpedo;
			p_unit.IsCavalry = unit.IsCavalry;
			p_unit.IsMineFlail = unit.IsMineFlail;
			p_unit.IsCarrier = unit.IsCarrier;
			p_unit.IsPatrolBoat = unit.IsPatrolBoat;
			p_unit.CanCatapultPlanes = unit.CanCatapultPlanes;
			p_unit.IsCatapultPlane = unit.IsCatapultPlane;
			p_unit.CanBeRefueledOnCarrier = unit.CanBeRefueledOnCarrier;
			p_unit.HasFlamethrower = unit.HasFlamethrower;
			p_unit.IsRecon = unit.IsRecon;
			p_unit.IsCommander = unit.IsCommander;
			p_unit.IsSupplyShip = unit.IsSupplyShip;
			p_unit.BuiltByEngineers = unit.BuiltByEngineers;
			p_unit.TileBuilding = unit.TileBuilding;
			p_unit.CoastalBattery = unit.CoastalBattery;
			p_unit.IsPontoonBridge = unit.IsPontoonBridge;
			p_unit.IsArmourObstacle = unit.IsArmourObstacle;
			p_unit.IsEngineers = unit.IsEngineers;
			p_unit.IsAmphibious = unit.IsAmphibious;
			p_unit.NoLendLease = unit.NoLendLease;
			p_unit.NoCapture = unit.NoCapture;
			p_unit.PermanentDeath = unit.PermanentDeath;
			p_unit.AIPurchaseChanceOverride = unit.AIPurchaseChanceOverride;
			p_unit.ImmuneToWinterDamage = unit.ImmuneToWinterDamage;
			p_unit.CanAttackShips = unit.CanAttackShips;
			p_unit.CommanderTargets = unit.CommanderTargets;
			p_unit.MoveOrAttack = item.MoveOrAttack;
			p_unit.CalculateAndSetUpkeepValue();
			p_unit.CalculateAndSetManpowerCost();
			if (p_unit.IsOnBoat)
			{
				p_unit.Type = "Boat";
			}
			else if (p_unit.IsDropable)
			{
				p_unit.Type = "Plane";
			}
			if (p_unit.XP > 0)
			{
				while (unit.XP < p_unit.XP)
				{
					unit.AddXP(1, p_canAddHero: false, p_showXPUI: false);
				}
			}
			if (!MainMenu.isLoadedGame)
			{
				if (p_unit.IsPoliticalUnit || p_unit.IsEliteUnit)
				{
					p_unit.Morale = Unit.MORALE_ELITE;
				}
				else
				{
					p_unit.Morale = Unit.MORALE_DEFAULT;
				}
				if (unit.Lvl < p_unit.Lvl)
				{
					while (unit.Lvl < p_unit.Lvl)
					{
						unit.AddXP(1, p_canAddHero: false, p_showXPUI: false);
					}
				}
				p_unit.XP = unit.XP;
				p_unit.CurrAmmo = p_unit.MaxAmmo;
				p_unit.MaxHP = unit.MaxHP;
				p_unit.CurrHP = unit.MaxHP;
				p_unit.CurrMP = unit.MaxMP;
				p_unit.CurrAutonomy = unit.MaxAutonomy;
			}
			else
			{
				p_unit.MaxHP = unit.MaxHP;
				if (flag)
				{
					p_unit.CurrHP = p_unit.MaxHP;
				}
				else
				{
					p_unit.CurrHP = (int)Math.Round((float)p_unit.MaxHP * num, MidpointRounding.ToEven);
				}
			}
			if (p_unit.HasBombs)
			{
				p_unit.ApplyModification(Database.bombsOption);
			}
			if (p_unit.HasRockets)
			{
				p_unit.ApplyModification(Database.rocketsOption);
			}
			if (p_unit.HasTorpedo)
			{
				p_unit.ApplyModification(Database.torpedoOption);
			}
			if (p_unit.HasABomb)
			{
				p_unit.ApplyModification(Database.atomBombOption);
			}
			if (p_unit.HasDropTank)
			{
				p_unit.ApplyModification(Database.dropTankOption);
			}
			if (p_unit.IsHorsed)
			{
				p_unit.ApplyModification(Database.horseTowedOption);
			}
			else if (p_unit.IsMotorized)
			{
				p_unit.ApplyModification(Database.motorizedOption);
			}
			else if (p_unit.IsMechanized)
			{
				p_unit.ApplyModification(Database.mechanizedOption);
			}
			if (p_unit.Hero != null)
			{
				p_unit.MaxMP += p_unit.Hero.MPBonus;
				p_unit.Visibility += p_unit.Hero.VisibilityBonus;
				p_unit.SoftDamage += p_unit.Hero.SoftDamageBonus;
				p_unit.HardDamage += p_unit.Hero.HardDamageBonus;
				p_unit.AirDamage += p_unit.Hero.AirDamageBonus;
			}
			if (!MainMenu.isLoadedGame)
			{
				p_unit.TampMP = p_unit.MaxMP;
				p_unit.TampMaxMP = p_unit.MaxMP;
				p_unit.TampAutonomy = p_unit.MaxAutonomy;
				p_unit.TampMaxAutonomy = p_unit.MaxAutonomy;
			}
			if (p_unit.IsDropable)
			{
				p_unit.ApplyModification(Database.planeTransportOption);
			}
			else if (p_unit.IsTrain)
			{
				p_unit.ApplyModification(Database.trainTransportOption);
			}
			else if (p_unit.IsOnBoat)
			{
				p_unit.ApplyModification(Database.shipTransportOption);
			}
			if (!MainMenu.isLoadedGame)
			{
				p_unit.EntrenchmentLevel = 0;
				if (p_unit.BaseMaxMP > 0 && p_unit.CanGetEntrenched() && !p_unit.NoEntrenchmentTurn1)
				{
					if (!p_unit.EntrenchmentLevelOnStart.HasValue)
					{
						p_unit.EntrenchmentLevel = 3;
					}
					else
					{
						p_unit.EntrenchmentLevel = p_unit.EntrenchmentLevelOnStart.Value;
					}
				}
			}
			if (GameData.Instance.map.turnCount == 1)
			{
				if (p_unit.NoAmmoTurn1 && !p_unit.HasResuppliedAmmo)
				{
					p_unit.CurrAmmo = 0;
				}
				if (!p_unit.HasMoved)
				{
					p_unit.CurrMP = p_unit.MaxMP;
					p_unit.CurrAutonomy = p_unit.MaxAutonomy;
				}
				if (p_unit.HPmodifierOnScenarioStart.HasValue && p_unit.HPmodifierOnScenarioStart != 0 && p_unit.HPmodifierOnScenarioStart < 100)
				{
					p_unit.CurrHP = (int)Math.Round((float)p_unit.MaxHP * ((float)(int)p_unit.HPmodifierOnScenarioStart.Value / 100f), MidpointRounding.ToEven);
				}
			}
			if (p_unit.CurrHP > p_unit.MaxHP)
			{
				p_unit.CurrHP = p_unit.MaxHP;
			}
			if (p_unit.CurrAmmo > p_unit.MaxAmmo)
			{
				p_unit.CurrAmmo = p_unit.MaxAmmo;
			}
			if (p_unit.CurrMP > p_unit.MaxMP)
			{
				p_unit.CurrMP = p_unit.MaxMP;
			}
			if (p_unit.CurrAutonomy > p_unit.MaxAutonomy)
			{
				p_unit.CurrAutonomy = p_unit.MaxAutonomy;
			}
			return;
		}
		UnityEngine.Debug.Log("Could not find the unit to update: " + p_unit.Name + " " + p_unit.Country);
		PatchUnit(p_unit);
	}

	public static int RoundUp(int p_number)
	{
		if (p_number % 5 == 0)
		{
			return p_number;
		}
		return 5 - p_number % 5 + p_number;
	}

	public static bool IsMasterClient()
	{
		if (!PhotonNetwork.InRoom)
		{
			return false;
		}
		return PhotonNetwork.IsMasterClient;
	}

	[Obsolete("Use MultiplayerManager.IsRealtimeMultiplayer() instead.")]
	public static bool IsRealtimeMultiplayer()
	{
		return MultiplayerManager.IsRealtimeMultiplayer();
	}

	public static int GetIndexByName(Dropdown p_dropDown, string p_name)
	{
		List<Dropdown.OptionData> options = p_dropDown.options;
		for (int i = 0; i < options.Count; i++)
		{
			if (options[i].text.Equals(p_name))
			{
				return i;
			}
		}
		return 0;
	}

	public static int GetIndexByName(TMP_Dropdown p_dropDown, string p_name)
	{
		List<TMP_Dropdown.OptionData> options = p_dropDown.options;
		for (int i = 0; i < options.Count; i++)
		{
			if (options[i].text.Equals(p_name))
			{
				return i;
			}
		}
		return 0;
	}

	public static int GetIndexByLocalizedName(string p_value, List<string> p_values)
	{
		for (int i = 0; i < p_values.Count; i++)
		{
			if (p_values[i] == p_value)
			{
				return i;
			}
		}
		return 0;
	}

	public static byte[] ObjectToBlob(object p_object)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			new BinaryFormatter().Serialize(memoryStream, p_object);
			return memoryStream.ToArray();
		}
	}

	public static object BlobToObject(byte[] p_bytes)
	{
		if (p_bytes == null)
		{
			throw new ArgumentNullException("p_bytes");
		}
		using (MemoryStream serializationStream = new MemoryStream(p_bytes, writable: false))
		{
			return new BinaryFormatter().Deserialize(serializationStream);
		}
	}

	public static byte[] Compress(byte[] p_bytes)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(memoryStream, System.IO.Compression.CompressionLevel.Fastest))
		{
			deflateStream.Write(p_bytes, 0, p_bytes.Length);
		}
		return memoryStream.ToArray();
	}

	public static byte[] Decompress(byte[] p_bytes)
	{
		MemoryStream stream = new MemoryStream(p_bytes);
		MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
		{
			deflateStream.CopyTo(memoryStream);
		}
		return memoryStream.ToArray();
	}

	public static byte[] ConvertObjectToByteArray(object p_object)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (DeflateStream serializationStream = new DeflateStream(memoryStream, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
			{
				binaryFormatter.Serialize(serializationStream, p_object);
			}
			return memoryStream.ToArray();
		}
	}

	public static object ConvertByteArrayToObject(byte[] p_bytes)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		try
		{
			using (MemoryStream stream = new MemoryStream(p_bytes, writable: false))
			{
				using (DeflateStream serializationStream = new DeflateStream(stream, CompressionMode.Decompress))
				{
					return binaryFormatter.Deserialize(serializationStream);
				}
			}
		}
		catch (Exception ex) when (ex is SerializationException || ex is EndOfStreamException || ex is InvalidCastException || ex is ArgumentException || ex is IndexOutOfRangeException || ex is NullReferenceException || ex is InvalidDataException || ex is IOException)
		{
			try
			{
				using (MemoryStream serializationStream2 = new MemoryStream(p_bytes, writable: false))
				{
					return binaryFormatter.Deserialize(serializationStream2);
				}
			}
			catch (Exception innerException)
			{
				throw new InvalidDataException("Save is neither a valid Deflate stream nor a valid raw BinaryFormatter stream.", innerException);
			}
		}
	}

	public static bool HasNKVDAround(Tile p_tile, Unit p_unit)
	{
		if (p_unit.OwnerPlayer == null)
		{
			return false;
		}
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (neighbour.tileGO.groundUnitGO != null && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer != null && neighbour.tileGO.groundUnitGO.unit.OwnerPlayer.Faction == p_unit.OwnerPlayer.Faction && neighbour.tileGO.groundUnitGO.unit.Name.Contains("NKVD"))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetRandomNumber(byte p_numberOfSides)
	{
		byte[] array = new byte[1];
		do
		{
			RNGcsp.GetBytes(array);
		}
		while (!IsFairRoll(array[0], p_numberOfSides));
		return array[0] % p_numberOfSides;
	}

	private static bool IsFairRoll(byte p_roll, byte p_numSides)
	{
		int num = 255 / p_numSides;
		return p_roll < p_numSides * num;
	}

	public static Sprite GetTileSprite(Tile p_tile)
	{
		if (string.IsNullOrWhiteSpace(p_tile.Season))
		{
			return Resources.Load<Sprite>("Tile no graphics");
		}
		if (string.IsNullOrWhiteSpace(p_tile.Name))
		{
			return Resources.Load<Sprite>("Tile no graphics");
		}
		string text = p_tile.Season + "/";
		string text2 = p_tile.Name;
		if (p_tile.Season != "Desert")
		{
			if (p_tile.Name.Contains("ThickForest") || p_tile.Name.Contains("ThinForest") || p_tile.Name.Contains("Bocage"))
			{
				int num = UnityEngine.Random.Range(1, 4);
				text2 = p_tile.Type + num;
			}
			else if (p_tile.isFrozen && p_tile.Type == "Sea")
			{
				text += "Frozen/";
			}
		}
		foreach (Sprite moddedTilesGraphic in ModsManager.moddedTilesGraphics)
		{
			if (moddedTilesGraphic.name == p_tile.Season + " " + text2)
			{
				return moddedTilesGraphic;
			}
		}
		Sprite sprite = Resources.Load<Sprite>(text + text2);
		if (MainMenu.isMainMenu)
		{
			if (sprite == null)
			{
				return Resources.Load<Sprite>("Tile no graphics");
			}
			return sprite;
		}
		return sprite;
	}

	public static Sprite GetUnitSprite(Unit p_unit)
	{
		Sprite prototypeSprite;
		if (!UIManager.isNATO && PrototypeBootstrap.TryGetUnitSprite(p_unit, out prototypeSprite))
		{
			return prototypeSprite;
		}
		string text = "Units/";
		string text2;
		if (!UIManager.isNATO || p_unit.TileBuilding)
		{
			if (p_unit.IsOnBoat && !p_unit.IsCommander)
			{
				text2 = ((p_unit.Armour <= 0) ? "Infantry Landing craft" : "Landing craft");
			}
			else if (p_unit.IsTrain)
			{
				text2 = "Train";
			}
			else
			{
				text2 = ((!p_unit.IsDropable || !p_unit.IsParatrooper) ? p_unit.Name : "Droppable");
			}
			string text3 = p_unit.Country + " " + text2;
			foreach (Sprite moddedUnitsGraphic in ModsManager.moddedUnitsGraphics)
			{
				if (moddedUnitsGraphic.name == text3)
				{
					return moddedUnitsGraphic;
				}
			}
			Sprite sprite = Resources.Load<Sprite>(text + p_unit.Country + "/" + text2);
			if (sprite != null)
			{
				return sprite;
			}
			foreach (Sprite moddedUnitsGraphic2 in ModsManager.moddedUnitsGraphics)
			{
				if (moddedUnitsGraphic2.name == "Shared " + text2)
				{
					return moddedUnitsGraphic2;
				}
			}
			sprite = Resources.Load<Sprite>("Units/Shared/" + text2);
			if (sprite != null)
			{
				return sprite;
			}
			if (p_unit.FilterType == "FilterBoat")
			{
				text2 = "Ship";
			}
			else if (p_unit.FilterType == "FilterFighter")
			{
				text2 = "Fighter";
			}
			else if (p_unit.FilterType == "FilterCAS")
			{
				text2 = "CAS";
			}
			else if (p_unit.FilterType == "FilterBomber")
			{
				text2 = "Bomber";
			}
			else if (p_unit.FilterType == "FilterAA")
			{
				text2 = "AA";
			}
			else if (p_unit.FilterType == "FilterAntitank")
			{
				text2 = "AT";
			}
			else if (p_unit.FilterType == "FilterArtillery")
			{
				text2 = "Artillery";
			}
			else if (p_unit.FilterType == "FilterBuilding")
			{
				text2 = "Building";
			}
			else if (p_unit.Type == "Infantry")
			{
				text2 = "Infantry";
			}
			else
			{
				text2 = ((!(p_unit.Type == "Tank")) ? "Null" : "Tank");
			}
			return Resources.Load<Sprite>(text + "Null/" + text2);
		}
		text += "NATO/";
		text2 = p_unit.Type;
		if (p_unit.IsCommander)
		{
			text2 = "Commander";
		}
		else if (p_unit.IsMine)
		{
			text2 = ((!(p_unit.Type == "Boat")) ? "Land mines" : "Naval mines");
		}
		else if (p_unit.IsRADAR && p_unit.FilterType == "FilterBuilding")
		{
			text2 = "RADAR";
		}
		else if (p_unit.IsSupplyDepot)
		{
			text2 = "Supply depot";
		}
		else if (p_unit.IsPatrolBoat)
		{
			text2 = "Gunboat";
		}
		else if (p_unit.IsArmourObstacle)
		{
			text2 = "Armour obstacle";
		}
		else if (p_unit.IsPontoonBridge && p_unit.FilterType == "FilterBuilding")
		{
			text2 = "Bridge";
		}
		else if (p_unit.IsDropable)
		{
			text2 = "Droppable";
		}
		else if (p_unit.IsPartisans)
		{
			text2 = "Partisans";
		}
		else if (p_unit.Name.Contains("Garrison"))
		{
			text2 = "Garrison";
		}
		else if (p_unit.IsParatrooper)
		{
			text2 = "Paratroopers";
		}
		else if (p_unit.IsSubmarine)
		{
			text2 = "Submarine";
		}
		else if (p_unit.IsCarrier || (p_unit.Name.Contains("carrier") && p_unit.FilterType == "FilterBoat"))
		{
			text2 = "Carrier";
		}
		else if (p_unit.IsArmouredTrain || p_unit.CoastalBattery)
		{
			text2 = "Armoured artillery";
		}
		else if (p_unit.Name.Contains("Bunker"))
		{
			text2 = "Bunker";
		}
		else if (p_unit.IsTrain)
		{
			text2 = "Train";
		}
		else if (p_unit.IsCavalry)
		{
			text2 = "Cavalry";
		}
		else if (p_unit.HasFlamethrower && p_unit.FilterType != "FilterInfantry")
		{
			text2 = "Flamethrower tank";
		}
		else if (p_unit.IsSupplyShip)
		{
			text2 = ((!(p_unit.Type == "Plane")) ? "Supply ship" : "Supply plane");
		}
		else if (p_unit.IsRecon)
		{
			if (p_unit.Type == "Plane")
			{
				text2 = "Plane recon";
			}
			else if (p_unit.Type == "Infantry")
			{
				text2 = "Infantry recon";
			}
			else if (p_unit.Armour > 0)
			{
				text2 = "Armoured recon";
			}
		}
		else if (p_unit.FilterType == "FilterInfantry")
		{
			if (p_unit.GetNameToLower().Contains("marine"))
			{
				text2 = "Marines";
			}
			else if (p_unit.IsEngineers)
			{
				if (p_unit.IsMotorized)
				{
					text2 = "Motorized engineers infantry";
				}
				else
				{
					text2 = ((!p_unit.IsMechanized) ? "Engineers infantry" : "Armoured engineers infantry");
				}
			}
			else if (p_unit.IsMotorized)
			{
				text2 = "Motorized infantry";
			}
			else
			{
				text2 = ((!p_unit.IsMechanized) ? "Infantry" : "Armoured infantry");
			}
		}
		else if (p_unit.FilterType == "FilterArtillery")
		{
			if (p_unit.Armour <= 5 && !p_unit.IsMechanized)
			{
				if (p_unit.IsMotorized)
				{
					text2 = "Motorized artillery";
				}
				else
				{
					text2 = ((!p_unit.IsHorsed) ? "Artillery" : "Horse artillery");
				}
			}
			else
			{
				text2 = "Armoured artillery";
			}
		}
		else if (p_unit.FilterType == "FilterAA")
		{
			if (p_unit.Armour <= 5 && !p_unit.IsMechanized)
			{
				if (p_unit.IsMotorized)
				{
					text2 = "Motorized AA";
				}
				else
				{
					text2 = ((!p_unit.IsHorsed) ? "AA" : "Horse AA");
				}
			}
			else
			{
				text2 = "Armoured AA";
			}
		}
		else if (p_unit.FilterType == "FilterAntitank")
		{
			if (p_unit.Armour <= 5 && !p_unit.IsMechanized)
			{
				if (p_unit.IsMotorized)
				{
					text2 = "Motorized AT";
				}
				else
				{
					text2 = ((!p_unit.IsHorsed) ? "AT" : "Horse AT");
				}
			}
			else
			{
				text2 = "Armoured AT";
			}
		}
		else if (p_unit.FilterType == "FilterTank")
		{
			if (p_unit.IsRecon)
			{
				text2 = "Armoured recon";
			}
			else if (p_unit.IsEngineers)
			{
				text2 = "Armoured engineers";
			}
			else
			{
				text2 = "Tank";
				if (p_unit.HardDamage >= 90 || p_unit.Armour >= 70)
				{
					text2 = "Heavy tank";
				}
			}
		}
		else if (p_unit.FilterType == "FilterBoat")
		{
			if (p_unit.Name.Contains("battleship") || p_unit.Name.Contains("Battleship") || p_unit.Name.Contains("coastal defence"))
			{
				text2 = "Battleship";
			}
			else if (p_unit.Name.Contains("Battlecruiser") || p_unit.Name.Contains("battlecruiser"))
			{
				text2 = "Battlecruiser";
			}
			else if (p_unit.Name.Contains("Heavy cruiser") || p_unit.Name.Contains("heavy cruiser"))
			{
				text2 = "Heavy cruiser";
			}
			else if (p_unit.Name.Contains("Light cruiser") || p_unit.Name.Contains("light cruiser"))
			{
				text2 = "Light cruiser";
			}
			else if (p_unit.Name.Contains("destroyer") || p_unit.IsDestroyer)
			{
				text2 = "Destroyer";
			}
			else if (p_unit.Name.Contains("dreadnought"))
			{
				text2 = "Dreadnought";
			}
		}
		else if (p_unit.Type == "Plane")
		{
			if (p_unit.FilterType == "FilterFighter")
			{
				text2 = "Fighter";
			}
			else if (p_unit.FilterType == "FilterCAS")
			{
				text2 = "CAS";
			}
			else
			{
				text2 = ((!(p_unit.FilterType == "FilterBomber")) ? "Fighter" : "Bomber");
			}
		}
		foreach (Sprite moddedUnitsGraphic3 in ModsManager.moddedUnitsGraphics)
		{
			if (moddedUnitsGraphic3.name == "NATO " + text2)
			{
				return moddedUnitsGraphic3;
			}
		}
		Sprite sprite2 = Resources.Load<Sprite>(text + text2);
		if (sprite2 != null)
		{
			return sprite2;
		}
		return Resources.Load<Sprite>("Units/Null");
	}

	public static Sprite GetModdedHeroSprite(Hero p_hero)
	{
		foreach (Sprite moddedHeroesGraphic in ModsManager.moddedHeroesGraphics)
		{
			if (moddedHeroesGraphic.name == p_hero.Country + " " + p_hero.Name)
			{
				return moddedHeroesGraphic;
			}
		}
		return null;
	}

	public static Sprite GetHeroIconSprite(Unit p_unit)
	{
		if (p_unit.Hero == null)
		{
			return null;
		}
		Sprite sprite = Resources.Load<Sprite>("UI/Hero icons/" + p_unit.OwnerPlayer.Country);
		if (sprite != null)
		{
			return sprite;
		}
		if (p_unit.OwnerPlayer.Country == "Japan")
		{
			return Resources.Load<Sprite>("UI/Hero icons/Japan");
		}
		if (p_unit.OwnerPlayer.Faction == "Axis")
		{
			return Resources.Load<Sprite>("UI/Hero icons/Axis");
		}
		if (p_unit.OwnerPlayer.Faction == "Allies")
		{
			return Resources.Load<Sprite>("UI/Hero icons/Allies");
		}
		if (p_unit.OwnerPlayer.Faction == "Communists")
		{
			return Resources.Load<Sprite>("UI/Hero icons/Communism");
		}
		if (p_unit.OwnerPlayer.Ideology == "Communism")
		{
			return Resources.Load<Sprite>("UI/Hero icons/Communism");
		}
		_ = p_unit.OwnerPlayer.Faction == "Neutral";
		return Resources.Load<Sprite>("UI/Hero icons/Neutral");
	}

	public static Sprite GetPoliticalSprite(Unit p_unit)
	{
		if (!p_unit.IsPoliticalUnit)
		{
			return null;
		}
		if (p_unit.Country == "Soviet Union")
		{
			return Resources.Load<Sprite>("UI/Political units/Guards");
		}
		if (p_unit.Country == "Nazi Germany")
		{
			if (PlayerSettings.Instance.CensoredGermany)
			{
				return Resources.Load<Sprite>("UI/Political units/Censored SS");
			}
			return Resources.Load<Sprite>("UI/Political units/SS");
		}
		return null;
	}

	public static Sprite GetUnitBackground(Unit p_unit, string p_notOriginalCountry)
	{
		string text = "Units/";
		string text2 = "Counter ground";
		if (UIManager.isNATO)
		{
			text2 = ((!(p_unit.Type == "Plane")) ? "Counter full square" : "Counter full round");
		}
		else if (p_unit.IsCommander)
		{
			if (p_unit.Type == "Plane")
			{
				text2 = "Counter full round";
			}
			else
			{
				text2 = ((!(p_unit.Type == "Boat")) ? "Counter ground" : "Counter navy");
			}
		}
		else
		{
			if (p_unit.IsOnBoat)
			{
				text2 = "Counter navy";
			}
			else if (p_unit.FilterType == "FilterInfantry" && !p_unit.IsDropable)
			{
				if (p_unit.IsEngineers)
				{
					text2 = "Counter engineers";
				}
				else if (p_unit.GetNameToLower().Contains("marine"))
				{
					text2 = "Counter marines";
				}
				else if (p_unit.IsParatrooper)
				{
					text2 = "Counter paratroopers";
				}
				else
				{
					text2 = ((!p_unit.IsRecon) ? "Counter infantry" : "Counter recon");
				}
			}
			else if (p_unit.FilterType == "FilterCAS")
			{
				text2 = "Counter CAS";
			}
			else if (p_unit.FilterType == "FilterFighter" || p_unit.IsDropable)
			{
				text2 = "Counter Fighter";
			}
			else if (p_unit.FilterType == "FilterBomber")
			{
				text2 = "Counter Bomber";
			}
			else if (p_unit.Armour > 0 && p_unit.GetNameToLower().Contains("marine"))
			{
				text2 = "Counter marines";
			}
			if (p_unit.FilterType == "FilterBuilding")
			{
				text2 = "Counter full square";
			}
			if (p_unit.FilterType == "FilterBoat")
			{
				text2 = ((!p_unit.IsMine) ? "Counter navy" : "Counter full round");
			}
		}
		foreach (Sprite moddedUnitsGraphic in ModsManager.moddedUnitsGraphics)
		{
			if (string.IsNullOrEmpty(p_notOriginalCountry))
			{
				if (p_unit.OwnerPlayer != null && moddedUnitsGraphic.name == p_unit.OwnerPlayer.Country + " " + text2)
				{
					return moddedUnitsGraphic;
				}
				if (p_unit.OwnerPlayer == null && moddedUnitsGraphic.name == p_unit.Country + " " + text2)
				{
					return moddedUnitsGraphic;
				}
			}
			else if (moddedUnitsGraphic.name == p_notOriginalCountry + " " + text2)
			{
				return moddedUnitsGraphic;
			}
		}
		Sprite sprite = ((p_unit.OwnerPlayer == null) ? Resources.Load<Sprite>(text + p_unit.Country + "/" + text2) : Resources.Load<Sprite>(text + p_unit.OwnerPlayer.Country + "/" + text2));
		if (sprite != null)
		{
			return sprite;
		}
		return Resources.Load<Sprite>(text + "Null");
	}

	public static void SetAutoCoasts(Tile p_tile)
	{
		if (p_tile.enumType != TileTypes.SEA || !p_tile.IsOfficial || p_tile.Bridge || p_tile.IsReinforcementTile || p_tile.isNavalConvoyRoute)
		{
			return;
		}
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (neighbour.enumType == TileTypes.SEA || neighbour.enumType == TileTypes.HARBOUR || neighbour.Type == "Beach")
			{
				continue;
			}
			if (neighbour.enumSeason == TileSeason.DESERT)
			{
				flag7 = true;
			}
			else if (neighbour.enumSeason == TileSeason.WINTER || p_tile.isFrozen)
			{
				flag8 = true;
			}
			num++;
			if (neighbour.PosX == p_tile.PosX && neighbour.PosY == p_tile.PosY + 1)
			{
				flag = true;
			}
			else if (neighbour.PosX == p_tile.PosX && neighbour.PosY == p_tile.PosY - 1)
			{
				flag2 = true;
			}
			if (p_tile.PosX % 2 == 0)
			{
				if (neighbour.PosX == p_tile.PosX - 1 && neighbour.PosY == p_tile.PosY + 1)
				{
					flag4 = true;
				}
				else if (neighbour.PosX == p_tile.PosX - 1 && neighbour.PosY == p_tile.PosY)
				{
					flag6 = true;
				}
				else if (neighbour.PosX == p_tile.PosX + 1 && neighbour.PosY == p_tile.PosY + 1)
				{
					flag3 = true;
				}
				else if (neighbour.PosX == p_tile.PosX + 1 && neighbour.PosY == p_tile.PosY)
				{
					flag5 = true;
				}
			}
			else if (neighbour.PosX == p_tile.PosX - 1 && neighbour.PosY == p_tile.PosY)
			{
				flag4 = true;
			}
			else if (neighbour.PosX == p_tile.PosX - 1 && neighbour.PosY == p_tile.PosY - 1)
			{
				flag6 = true;
			}
			else if (neighbour.PosX == p_tile.PosX + 1 && neighbour.PosY == p_tile.PosY)
			{
				flag3 = true;
			}
			else if (neighbour.PosX == p_tile.PosX + 1 && neighbour.PosY == p_tile.PosY - 1)
			{
				flag5 = true;
			}
		}
		if (num == 0)
		{
			if (MainMenu.isEditor && MapEditorGO.selectedTile != null)
			{
				MapEditorGO.selectedTile.Name = "Sea";
			}
			if (!p_tile.IsVictoryPoint)
			{
				p_tile.Name = "Sea";
			}
			return;
		}
		if (flag7)
		{
			p_tile.Season = "Desert";
		}
		else if (flag8)
		{
			p_tile.Season = "Winter";
		}
		else
		{
			p_tile.Season = "Summer";
		}
		switch (num)
		{
		case 1:
			if (flag2)
			{
				p_tile.Name = "Coast1";
			}
			else if (flag5)
			{
				p_tile.Name = "Coast2";
			}
			else if (flag3)
			{
				p_tile.Name = "Coast3";
			}
			else if (flag)
			{
				p_tile.Name = "Coast4";
			}
			else if (flag4)
			{
				p_tile.Name = "Coast5";
			}
			else if (flag6)
			{
				p_tile.Name = "Coast6";
			}
			break;
		case 2:
			if (flag2 & flag5)
			{
				p_tile.Name = "Coast7";
			}
			else if (flag5 & flag3)
			{
				p_tile.Name = "Coast8";
			}
			else if (flag & flag3)
			{
				p_tile.Name = "Coast9";
			}
			else if (flag & flag4)
			{
				p_tile.Name = "Coast10";
			}
			else if (flag4 & flag6)
			{
				p_tile.Name = "Coast11";
			}
			else if (flag6 & flag2)
			{
				p_tile.Name = "Coast12";
			}
			else if (flag2 & flag3)
			{
				p_tile.Name = "Coast54";
			}
			else if (flag & flag5)
			{
				p_tile.Name = "Coast55";
			}
			else if (flag & flag6)
			{
				p_tile.Name = "Coast56";
			}
			else if (flag2 & flag4)
			{
				p_tile.Name = "Coast57";
			}
			else if (flag6 & flag5)
			{
				p_tile.Name = "Coast58";
			}
			else if (flag & flag2)
			{
				p_tile.Name = "Coast59";
			}
			else if (flag4 & flag3)
			{
				p_tile.Name = "Coast60";
			}
			else if (flag4 & flag5)
			{
				p_tile.Name = "Coast61";
			}
			else if (flag3 & flag6)
			{
				p_tile.Name = "Coast62";
			}
			break;
		case 3:
			if (flag6 & flag2 & flag5)
			{
				p_tile.Name = "Coast13";
			}
			else if (flag2 & flag5 & flag3)
			{
				p_tile.Name = "Coast14";
			}
			else if (flag5 & flag3 & flag)
			{
				p_tile.Name = "Coast15";
			}
			else if (flag3 & flag & flag4)
			{
				p_tile.Name = "Coast16";
			}
			else if (flag & flag4 & flag6)
			{
				p_tile.Name = "Coast17";
			}
			else if (flag4 & flag6 & flag2)
			{
				p_tile.Name = "Coast18";
			}
			else if (flag2 & flag4 & flag3)
			{
				p_tile.Name = "Coast32";
			}
			else if (flag & flag6 & flag5)
			{
				p_tile.Name = "Coast33";
			}
			else if (flag & flag2 & flag3)
			{
				p_tile.Name = "Coast34";
			}
			else if (flag4 & flag & flag5)
			{
				p_tile.Name = "Coast35";
			}
			else if (flag3 & flag4 & flag6)
			{
				p_tile.Name = "Coast36";
			}
			else if (flag & flag2 & flag6)
			{
				p_tile.Name = "Coast37";
			}
			else if (flag4 & flag2 & flag5)
			{
				p_tile.Name = "Coast38";
			}
			else if (flag6 & flag5 & flag3)
			{
				p_tile.Name = "Coast39";
			}
			else if (flag2 & flag4 & flag)
			{
				p_tile.Name = "Coast40";
			}
			else if (flag4 & flag6 & flag5)
			{
				p_tile.Name = "Coast41";
			}
			else if (flag3 & flag6 & flag2)
			{
				p_tile.Name = "Coast42";
			}
			else if (flag2 & flag & flag5)
			{
				p_tile.Name = "Coast43";
			}
			else if (flag4 & flag3 & flag5)
			{
				p_tile.Name = "Coast44";
			}
			else if (flag & flag3 & flag6)
			{
				p_tile.Name = "Coast45";
			}
			break;
		case 4:
			if (flag6 & flag2 & flag5 & flag3)
			{
				p_tile.Name = "Coast19";
			}
			else if (flag2 & flag5 & flag3 & flag)
			{
				p_tile.Name = "Coast20";
			}
			else if (flag5 & flag3 & flag & flag4)
			{
				p_tile.Name = "Coast21";
			}
			else if (flag3 & flag & flag4 & flag6)
			{
				p_tile.Name = "Coast22";
			}
			else if (flag & flag4 & flag6 & flag2)
			{
				p_tile.Name = "Coast23";
			}
			else if (flag4 & flag6 & flag2 & flag5)
			{
				p_tile.Name = "Coast24";
			}
			else if (flag4 & flag & flag3 & flag2)
			{
				p_tile.Name = "Coast46";
			}
			else if (flag & flag4 & flag6 & flag5)
			{
				p_tile.Name = "Coast47";
			}
			else if (flag3 & flag4 & flag6 & flag2)
			{
				p_tile.Name = "Coast48";
			}
			else if (flag & flag6 & flag5 & flag2)
			{
				p_tile.Name = "Coast49";
			}
			else if (flag4 & flag3 & flag5 & flag2)
			{
				p_tile.Name = "Coast50";
			}
			else if (flag6 & flag5 & flag3 & flag)
			{
				p_tile.Name = "Coast51";
			}
			else if (flag & flag4 & flag2 & flag5)
			{
				p_tile.Name = "Coast52";
			}
			else if (flag & flag3 & flag2 & flag6)
			{
				p_tile.Name = "Coast53";
			}
			else if (flag4 & flag6 & flag3 & flag5)
			{
				p_tile.Name = "Coast63";
			}
			break;
		case 5:
			if (flag2 & flag5 & flag3 & flag4 & flag6)
			{
				p_tile.Name = "Coast25";
			}
			else if (flag2 & flag5 & flag3 & flag & flag6)
			{
				p_tile.Name = "Coast26";
			}
			else if (flag2 & flag5 & flag3 & flag & flag4)
			{
				p_tile.Name = "Coast27";
			}
			else if (flag5 & flag3 & flag & flag4 & flag6)
			{
				p_tile.Name = "Coast28";
			}
			else if (flag2 & flag3 & flag & flag4 & flag6)
			{
				p_tile.Name = "Coast29";
			}
			else if (flag2 & flag5 & flag & flag4 & flag6)
			{
				p_tile.Name = "Coast30";
			}
			break;
		case 6:
			p_tile.Name = "Lake";
			break;
		}
		if (MainMenu.isEditor && MapEditorGO.selectedTile != null)
		{
			MapEditorGO.selectedTile.Name = p_tile.Name;
		}
	}

	public static List<Tile> GetAllTilesInRadiusAround(Tile p_centerTile, int p_radius)
	{
		switch (p_radius)
		{
		case 0:
			return new List<Tile> { p_centerTile };
		case 1:
		{
			List<Tile> list = new List<Tile>(7);
			list.Add(p_centerTile);
			list.AddRange(p_centerTile.neighbours);
			return list;
		}
		default:
		{
			HashSet<Tile> hashSet = new HashSet<Tile> { p_centerTile };
			Queue<(Tile, int)> queue = new Queue<(Tile, int)>();
			queue.Enqueue((p_centerTile, 0));
			while (queue.Count > 0)
			{
				var (tile, num) = queue.Dequeue();
				foreach (Tile neighbour in tile.neighbours)
				{
					if (neighbour != p_centerTile && !hashSet.Contains(neighbour))
					{
						int num2 = num + 1;
						if (num2 <= p_radius)
						{
							hashSet.Add(neighbour);
							queue.Enqueue((neighbour, num2));
						}
					}
				}
			}
			return hashSet.ToList();
		}
		}
	}

	public static bool IsTileInsideRadius(Tile p_tileCheck, Tile p_sourceTile, int p_radius)
	{
		switch (p_radius)
		{
		case 0:
			return p_tileCheck == p_sourceTile;
		case 1:
			foreach (Tile neighbour in p_sourceTile.neighbours)
			{
				if (neighbour == p_tileCheck)
				{
					return true;
				}
			}
			return false;
		default:
		{
			int sizeX = GameData.Instance.map.SizeX;
			int sizeY = GameData.Instance.map.SizeY;
			bool[] array = new bool[sizeX * sizeY];
			Queue<(Tile, int)> queue = new Queue<(Tile, int)>();
			queue.Enqueue((p_sourceTile, 0));
			int num = p_sourceTile.PosY * sizeX + p_sourceTile.PosX;
			array[num] = true;
			while (queue.Count > 0)
			{
				var (tile, num2) = queue.Dequeue();
				if (tile == p_tileCheck)
				{
					return true;
				}
				if (num2 >= p_radius)
				{
					continue;
				}
				foreach (Tile neighbour2 in tile.neighbours)
				{
					int num3 = neighbour2.PosY * sizeX + neighbour2.PosX;
					if (neighbour2 == p_tileCheck)
					{
						return true;
					}
					if (!array[num3])
					{
						array[num3] = true;
						queue.Enqueue((neighbour2, num2 + 1));
					}
				}
			}
			return false;
		}
		}
	}

	public static bool IsTileInsideRadiusSimplified(Tile p_sourceTile, Tile p_tileCheck, int p_radius)
	{
		return GetHeuristicDistance(p_sourceTile, p_tileCheck) <= p_radius;
	}

	private static (int q, int r) ConvertToAxial(int p_posX, int p_posY)
	{
		int item = p_posY - (p_posX + (p_posX & 1)) / 2;
		return (q: p_posX, r: item);
	}

	private static int GetAxialDistance(int p_qA, int p_rA, int p_qB, int p_rB)
	{
		return (Mathf.Abs(p_qA - p_qB) + Mathf.Abs(p_qA + p_rA - p_qB - p_rB) + Mathf.Abs(p_rA - p_rB)) / 2;
	}

	public static int GetHeuristicDistance(Tile p_tileA, Tile p_tileB)
	{
		(int q, int r) tuple = ConvertToAxial(p_tileA.PosX, p_tileA.PosY);
		int item = tuple.q;
		int item2 = tuple.r;
		(int q, int r) tuple2 = ConvertToAxial(p_tileB.PosX, p_tileB.PosY);
		int item3 = tuple2.q;
		int item4 = tuple2.r;
		int axialDistance = GetAxialDistance(item, item2, item3, item4);
		Map map = GameData.Instance.map;
		if (!map.CircularMap)
		{
			return axialDistance;
		}
		int num = map.SizeX / 2;
		axialDistance = Mathf.Min(axialDistance, GetAxialDistance(item, item2, item3 - map.SizeX, item4 + num));
		return Mathf.Min(axialDistance, GetAxialDistance(item, item2, item3 + map.SizeX, item4 - num));
	}

	public static Tile GetOriginalTile(Tile p_tileInstance)
	{
		foreach (Tile allTile in GetAllTiles())
		{
			if (!(p_tileInstance.Season == allTile.Season))
			{
				continue;
			}
			if (p_tileInstance.NumberOfTurnsAtLowProd > 0 || p_tileInstance.IsBridgeDestroyed)
			{
				if (p_tileInstance.Name.Replace(" destroyed", string.Empty) == allTile.Name)
				{
					return allTile;
				}
			}
			else if (p_tileInstance.Name == allTile.Name)
			{
				return allTile;
			}
		}
		return p_tileInstance;
	}

	public static void AssignGeographicalAreas()
	{
		Map map = GameData.Instance.map;
		int sizeX = map.SizeX;
		int sizeY = map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		short num = 0;
		short num2 = 0;
		short num3 = 0;
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				Tile tile = tilesTable[i, j];
				switch (tile.enumType)
				{
				case TileTypes.SEA:
					if (!tile.seaAreaIndex.HasValue)
					{
						FloodFill(tile, num2, FloodFillTypes.SEA);
						num2++;
					}
					break;
				case TileTypes.RIVER:
					if (!tile.riverAreaIndex.HasValue)
					{
						FloodFill(tile, num3, FloodFillTypes.RIVER);
						num3++;
					}
					break;
				default:
					if (!tile.landAreaIndex.HasValue)
					{
						FloodFill(tile, num, FloodFillTypes.LAND);
						num++;
					}
					break;
				}
			}
		}
	}

	private static void FloodFill(Tile p_startTile, short p_index, FloodFillTypes p_floodFillType)
	{
		Queue<Tile> queue = new Queue<Tile>();
		queue.Enqueue(p_startTile);
		switch (p_floodFillType)
		{
		case FloodFillTypes.LAND:
			p_startTile.landAreaIndex = p_index;
			break;
		case FloodFillTypes.SEA:
			p_startTile.seaAreaIndex = p_index;
			break;
		case FloodFillTypes.RIVER:
			p_startTile.riverAreaIndex = p_index;
			break;
		}
		while (queue.Count > 0)
		{
			foreach (Tile neighbour in queue.Dequeue().neighbours)
			{
				switch (p_floodFillType)
				{
				case FloodFillTypes.LAND:
					if ((neighbour.enumType != TileTypes.SEA || neighbour.Bridge) && !neighbour.landAreaIndex.HasValue)
					{
						neighbour.landAreaIndex = p_index;
						queue.Enqueue(neighbour);
					}
					break;
				case FloodFillTypes.SEA:
					if ((neighbour.enumType == TileTypes.SEA || neighbour.enumType == TileTypes.HARBOUR) && !neighbour.seaAreaIndex.HasValue)
					{
						neighbour.seaAreaIndex = p_index;
						queue.Enqueue(neighbour);
					}
					break;
				case FloodFillTypes.RIVER:
					if ((neighbour.enumType == TileTypes.SEA || neighbour.enumType == TileTypes.RIVER || neighbour.enumType == TileTypes.HARBOUR || neighbour.enumType == TileTypes.CITY || neighbour.Bridge) && !neighbour.riverAreaIndex.HasValue)
					{
						neighbour.riverAreaIndex = p_index;
						queue.Enqueue(neighbour);
					}
					break;
				}
			}
		}
	}

	public static void UpdateTilesData(bool p_nonDestructive)
	{
		Dictionary<string, Tile> dictionary = new Dictionary<string, Tile>();
		Dictionary<string, Tile> dictionary2 = OfficialTiles.GetInstance().Tiles.ToDictionary((Tile t) => t.Name + t.Season);
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			foreach (Tile listOfCustomTile in activeMod.CustomTiles.ListOfCustomTiles)
			{
				dictionary[listOfCustomTile.Name + listOfCustomTile.Season] = listOfCustomTile;
			}
		}
		Map map = GameData.Instance.map;
		int sizeX = map.SizeX;
		int sizeY = map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		map.SetIsWinterBasedOnCurrentDate();
		bool flag = map.IsWinter && map.AllowSnowInWinter;
		for (short num = 0; num < sizeX; num++)
		{
			for (short num2 = 0; num2 < sizeY; num2++)
			{
				Tile tile = tilesTable[num, num2];
				string key = ((tile.NumberOfTurnsAtLowProd <= 0) ? (tile.Name + tile.Season) : (tile.Name.Replace(" destroyed", "") + tile.Season));
				if (tile.Name == "Coast31")
				{
					tile.Name = "Lake";
				}
				tile.isFrozen = flag && tile.CanFreezeInWinter;
				Tile value2;
				if (dictionary2.TryGetValue(key, out var value))
				{
					UpdateTileProperties(tile, value, p_nonDestructive);
				}
				else if (dictionary.TryGetValue(key, out value2))
				{
					UpdateTileProperties(tile, value2, p_nonDestructive);
				}
				if (tile.isFrozen)
				{
					tile.Season = "Winter";
				}
				tile.SetRuntimeOptimizations();
			}
		}
	}

	public static void UpdateTileProperties(Tile p_targetTile, Tile p_sourceTile, bool p_nonDestructive)
	{
		p_targetTile.Type = p_sourceTile.Type;
		p_targetTile.FilterType = p_sourceTile.FilterType;
		p_targetTile.MPCost = p_sourceTile.MPCost;
		p_targetTile.DamageModificator = p_sourceTile.DamageModificator;
		p_targetTile.MoneyReward = p_sourceTile.MoneyReward;
		p_targetTile.MoneyProductionPerTurn = p_sourceTile.MoneyProductionPerTurn;
		p_targetTile.OccupiedMoneyProductionPerTurn = p_sourceTile.OccupiedMoneyProductionPerTurn;
		p_targetTile.Manpower = p_sourceTile.Manpower;
		p_targetTile.OccupiedManpower = p_sourceTile.OccupiedManpower;
		p_targetTile.IsReinforcementTile = p_sourceTile.IsReinforcementTile;
		p_targetTile.IsOfficial = p_sourceTile.IsOfficial;
		p_targetTile.Railroad = p_sourceTile.Railroad;
		p_targetTile.Bridge = p_sourceTile.Bridge;
		p_targetTile.Capturable = p_sourceTile.Capturable;
		if (p_nonDestructive)
		{
			if (p_targetTile.EditorOverwrite_income != 0)
			{
				p_targetTile.MoneyProductionPerTurn = p_targetTile.EditorOverwrite_income;
			}
			if (p_targetTile.EditorOverwrite_manpower != 0)
			{
				p_targetTile.Manpower = p_targetTile.EditorOverwrite_manpower;
			}
		}
	}

	public static void GetAllTilesNeighbours()
	{
		int sizeX = GameData.Instance.map.SizeX;
		int sizeY = GameData.Instance.map.SizeY;
		Tile[,] tilesTable = GameData.Instance.map.TilesTable;
		bool circularMap = GameData.Instance.map.CircularMap;
		for (short num = 0; num < sizeX; num++)
		{
			for (short num2 = 0; num2 < sizeY; num2++)
			{
				tilesTable[num, num2].neighbours = new List<Tile>(6);
				tilesTable[num, num2].PosX = num;
				tilesTable[num, num2].PosY = num2;
				if (num2 != sizeY - 1)
				{
					tilesTable[num, num2].neighbours.Add(tilesTable[num, num2 + 1]);
				}
				if (num2 != 0)
				{
					tilesTable[num, num2].neighbours.Add(tilesTable[num, num2 - 1]);
				}
				if (num % 2 == 0)
				{
					if (num2 != sizeY - 1)
					{
						if (num != 0)
						{
							tilesTable[num, num2].neighbours.Add(tilesTable[num - 1, num2 + 1]);
						}
						else if (circularMap && num == 0)
						{
							tilesTable[num, num2].neighbours.Add(tilesTable[sizeX - 1, num2 + 1]);
						}
					}
					if (num != sizeX - 1)
					{
						if (num2 != sizeY - 1)
						{
							tilesTable[num, num2].neighbours.Add(tilesTable[num + 1, num2 + 1]);
						}
					}
					else if (circularMap)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[0, num2]);
					}
					if (num != 0)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[num - 1, num2]);
					}
					else if (circularMap && num == 0)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[sizeX - 1, num2]);
					}
					if (num != sizeX - 1)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[num + 1, num2]);
					}
					else if (circularMap && num2 != 0)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[0, num2 - 1]);
					}
				}
				else
				{
					if (num != 0)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[num - 1, num2]);
					}
					else if (circularMap && num == 0)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[sizeX - 1, num2]);
					}
					if (num != sizeX - 1)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[num + 1, num2]);
					}
					else if (circularMap && num == sizeX - 1)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[0, num2]);
					}
					if (num != 0 && num2 != 0)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[num - 1, num2 - 1]);
					}
					else if (circularMap && num == 0)
					{
						tilesTable[num, num2].neighbours.Add(tilesTable[sizeX - 1, num2 - 1]);
					}
					if (num2 != 0)
					{
						if (num != sizeX - 1)
						{
							tilesTable[num, num2].neighbours.Add(tilesTable[num + 1, num2 - 1]);
						}
						else if (circularMap && num == sizeX - 1)
						{
							tilesTable[num, num2].neighbours.Add(tilesTable[0, num2 - 1]);
						}
					}
				}
				tilesTable[num, num2].amountOfNeighbours = (byte)tilesTable[num, num2].neighbours.Count();
			}
		}
	}

	public static string GetManpowerForUI(int p_manpowerValue)
	{
		if (Mathf.Abs(p_manpowerValue) < 1000)
		{
			return Truncate((float)p_manpowerValue / 1000f, 3) + " K";
		}
		if (Mathf.Abs(p_manpowerValue) < 10000)
		{
			return Truncate((float)p_manpowerValue / 1000f, 2) + " K";
		}
		if (Mathf.Abs(p_manpowerValue) < 100000)
		{
			return Truncate((float)p_manpowerValue / 1000f, 1) + " K";
		}
		if (Mathf.Abs(p_manpowerValue) > 1000000)
		{
			return Truncate((float)p_manpowerValue / 1000000f, 2) + " M";
		}
		return Truncate((float)p_manpowerValue / 1000f, 0) + " K";
	}

	public static float Truncate(float p_value, int p_digits)
	{
		decimal num = (decimal)Math.Pow(10.0, p_digits);
		return (float)(Math.Truncate((decimal)p_value * num) / num);
	}

	public static string GetSizeInMB(byte[] p_byteArray)
	{
		return ((double)p_byteArray.Length / 1048576.0).ToString("F2") + " MB";
	}

	public static List<Unit> GetAllUnits(bool p_includeRemovedUnits = false)
	{
		List<Unit> list = OfficialUnits.GetInstance()?.Units?.ToList() ?? new List<Unit>();
		IEnumerable<Unit> collection = (from m in ModsManager.activeMods
			select m.CustomUnits?.ListOfCustomUnits into list2
			where list2 != null
			select list2).SelectMany((List<Unit> result) => result);
		list.AddRange(collection);
		PatchUnits(list);
		if (!p_includeRemovedUnits)
		{
			list.RemoveAll((Unit u) => !u.IsAvailable(null));
		}
		return list;
	}

	public static void PatchUnits(List<Unit> p_units)
	{
		foreach (Unit p_unit in p_units)
		{
			PatchUnit(p_unit);
		}
	}

	public static void PatchUnit(Unit p_unit)
	{
		p_unit.Lvl = 1;
		p_unit.XP = 0;
		p_unit.BaseType = p_unit.Type;
		if (p_unit.Morale == 0)
		{
			p_unit.Morale = 75;
		}
		p_unit.CalculateAndSetManpowerCost();
		p_unit.CurrHP = p_unit.MaxHP;
		p_unit.BaseMaxHP = p_unit.MaxHP;
		p_unit.CurrMP = p_unit.MaxMP;
		p_unit.BaseMaxMP = p_unit.MaxMP;
		p_unit.CurrAutonomy = p_unit.MaxAutonomy;
		p_unit.BaseMaxAutonomy = p_unit.MaxAutonomy;
		p_unit.CurrAmmo = p_unit.MaxAmmo;
		p_unit.BaseMaxAmmo = p_unit.MaxAmmo;
		p_unit.BaseRange = p_unit.Range;
		p_unit.BaseArmour = p_unit.Armour;
		p_unit.BaseVisibility = p_unit.Visibility;
		p_unit.BaseSoftDamage = p_unit.SoftDamage;
		p_unit.BaseHardDamage = p_unit.HardDamage;
		p_unit.BaseAirDamage = p_unit.AirDamage;
		p_unit.TampMP = 0;
		p_unit.TampMaxMP = 0;
		p_unit.TampAutonomy = 0;
		p_unit.TampMaxAutonomy = 0;
		if (p_unit.ProductionMonth == 0)
		{
			p_unit.ProductionMonth = 1;
		}
	}

	public static List<Hero> GetAllHeroes()
	{
		List<Hero> list = new List<Hero>();
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			if (activeMod.ModdedHeroes != null)
			{
				list.AddRange(activeMod.ModdedHeroes.heroes);
			}
		}
		return list;
	}

	public static List<Unit> GetModdedUnits()
	{
		List<Unit> list = new List<Unit>();
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			if (activeMod.CustomUnits != null)
			{
				list.AddRange(activeMod.CustomUnits.ListOfCustomUnits);
			}
		}
		return list;
	}

	public static List<Tile> GetAllTiles()
	{
		List<Tile> list = new List<Tile>(600);
		list.AddRange(OfficialTiles.GetInstance().Tiles);
		foreach (Mod activeMod in ModsManager.activeMods)
		{
			if (activeMod.CustomTiles != null)
			{
				list.AddRange(activeMod.CustomTiles.ListOfCustomTiles);
			}
		}
		return list;
	}

	public static double GetDirectorySize(string p_directoryPath)
	{
		try
		{
			return (double)Directory.EnumerateFiles(p_directoryPath, "*", SearchOption.AllDirectories).Sum((string path) => new FileInfo(path).Length) / 1048576.0;
		}
		catch (DirectoryNotFoundException)
		{
			UnityEngine.Debug.LogError("Directory not found: " + p_directoryPath);
			return 0.0;
		}
		catch (UnauthorizedAccessException ex2)
		{
			UnityEngine.Debug.LogError("Access denied: " + p_directoryPath + " → " + ex2.Message);
		}
		return 0.0;
	}

	public static int GetMaxCoreUnitsNumber(Player p_player)
	{
		return GameData.Instance.map.BaseCoreUnitValue + GameData.Instance.map.NumberOfNewCoreUnitPerRank * p_player.Rank - GameData.Instance.map.NumberOfNewCoreUnitPerRank;
	}

	public static Sprite GetFlagSprite(Player p_player)
	{
		return GetFlagSprite(p_player.Country);
	}

	public static Sprite GetFlagSprite(string p_country)
	{
		Sprite prototypeFlag;
		if (PrototypeBootstrap.TryGetFlag(p_country, out prototypeFlag))
		{
			return prototypeFlag;
		}
		Sprite sprite = null;
		if (!string.IsNullOrEmpty(p_country))
		{
			foreach (Sprite moddedFlagsGraphic in ModsManager.moddedFlagsGraphics)
			{
				if (moddedFlagsGraphic.name == p_country)
				{
					return moddedFlagsGraphic;
				}
			}
		}
		if (sprite == null)
		{
			sprite = Resources.Load<Sprite>("Flags/" + p_country);
			if (sprite == null)
			{
				sprite = Resources.Load<Sprite>("Flags/Null");
			}
		}
		return sprite;
	}

	public static void MakeScreenshot()
	{
		string text = "Screenshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
		ScreenCapture.CaptureScreenshot(PrototypePaths.DataPath + "/Screenshots/" + text, 3);
	}

	public static int GetMonthDifference(DateTime p_unitDate, DateTime p_currentDate, bool p_absoluteValue = false)
	{
		int num = 12 * (p_unitDate.Year - p_currentDate.Year) + p_unitDate.Month - p_currentDate.Month;
		if (p_absoluteValue)
		{
			num = Mathf.Abs(num);
		}
		return num;
	}

	public static bool IsPointerOverUIObject()
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		return list.Count > 0;
	}

	public static string GetFileNameWithoutExtension(FileInfo p_fileInfo)
	{
		if (p_fileInfo == null)
		{
			UnityEngine.Debug.LogError("FileInfo is null.");
			return "FILEINFO IS NULL";
		}
		return p_fileInfo.Name.Remove(p_fileInfo.Name.Length - 4);
	}

	public static bool IsHiddenOrAppleDoubleFile(string p_fileName)
	{
		if (!string.IsNullOrEmpty(p_fileName))
		{
			return p_fileName.StartsWith(".");
		}
		return true;
	}

	public static bool IsHiddenOrAppleDoubleFile(FileInfo p_fileInfo)
	{
		if (p_fileInfo != null)
		{
			return IsHiddenOrAppleDoubleFile(p_fileInfo.Name);
		}
		return true;
	}

	public static AudioType GetAudioType(string p_extension)
	{
		switch (p_extension)
		{
		default:
			return AudioType.UNKNOWN;
		case ".mp3":
			return AudioType.MPEG;
		case ".wav":
			return AudioType.WAV;
		case ".ogg":
			return AudioType.OGGVORBIS;
		}
	}

	public static void ClearChildrenWith<T>(Transform p_parent, bool p_includeInactive = false) where T : Component
	{
		T[] componentsInChildren = p_parent.GetComponentsInChildren<T>(p_includeInactive);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i].gameObject);
		}
	}

	public static FileInfo GetMostRecentSaveFile()
	{
		string[] files = Directory.GetFiles(PrototypePaths.DataPath + "/Saves", "*", SearchOption.AllDirectories);
		FileInfo[] array = new FileInfo[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			array[i] = new FileInfo(files[i]);
		}
		array = array.OrderByDescending((FileInfo f) => f.LastWriteTime).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			if (!(array[num].Extension != ".sav") && !IsHiddenOrAppleDoubleFile(array[num]) && !array[num].Name.Contains("multiplayer"))
			{
				return array[num];
			}
		}
		return null;
	}

	public static uint ComputeKeyID32(string p_key)
	{
		string s = (p_key ?? string.Empty).Trim().ToLowerInvariant();
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		uint num = 2166136261u;
		for (int i = 0; i < bytes.Length; i++)
		{
			num ^= bytes[i];
			num *= 16777619;
		}
		return num;
	}

	public static ulong ComputeKeyID64(string p_key)
	{
		string s = (p_key ?? string.Empty).Trim().ToLowerInvariant();
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		ulong num = 1469598103934665603uL;
		for (int i = 0; i < bytes.Length; i++)
		{
			num ^= bytes[i];
			num *= 1099511628211L;
		}
		return num;
	}

	public static bool IsModdedUnit(Unit p_unit)
	{
		return p_unit?.IsModded ?? false;
	}

	public static bool IsUnitRegistered(Unit p_unit, bool p_officialOnly = false)
	{
		if (p_officialOnly)
		{
			return OfficialUnits.IsOfficialUnit(p_unit);
		}
		foreach (Unit allUnit in GetAllUnits(p_includeRemovedUnits: true))
		{
			if (allUnit.Name == p_unit.Name && allUnit.Country == p_unit.Country)
			{
				return true;
			}
		}
		return false;
	}

	public static string FormatReceivedTotal(long p_receivedBytes, long p_totalBytes, bool p_includePercent = true)
	{
		double num = (((double)p_totalBytes >= 1048576.0) ? 1048576.0 : 1024.0);
		string text = ((num == 1048576.0) ? "MB" : "KB");
		double num2 = (double)p_receivedBytes / num;
		double num3 = (double)p_totalBytes / num;
		double num4 = ((p_totalBytes > 0) ? ((double)p_receivedBytes / (double)p_totalBytes * 100.0) : 0.0);
		if (p_includePercent)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:0.0} {2} / {1:0.0} {2} ({3:0}%)", num2, num3, text, num4);
		}
		return string.Format(CultureInfo.InvariantCulture, "{0:0.0} {2} / {1:0.0} {2}", num2, num3, text);
	}

	public static string FormatReceivedTotalBoth(long p_receivedBytes, long p_totalBytes)
	{
		double num = (double)p_receivedBytes / 1024.0;
		double num2 = (double)p_totalBytes / 1024.0;
		double num3 = (double)p_receivedBytes / 1048576.0;
		double num4 = (double)p_totalBytes / 1048576.0;
		double num5 = ((p_totalBytes > 0) ? ((double)p_receivedBytes / (double)p_totalBytes * 100.0) : 0.0);
		return string.Format(CultureInfo.InvariantCulture, "{0:0.0} KB / {1:0.0} KB ({2:0.0} MB / {3:0.0} MB) ({4:0}%)", num, num2, num3, num4, num5);
	}
}

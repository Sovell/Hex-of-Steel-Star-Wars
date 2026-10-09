using System;

[Serializable]
public struct GameSettings
{
	public bool isBackup;

	public bool saveLocked;

	public bool historicalFocusON;

	public bool fogOfWarON;

	public bool researchON;

	public bool manpowerON;

	public bool policiesON;

	public bool weatherON;

	public bool partisansON;

	public bool realisticPlanesON;

	public bool overrunON;

	public bool moraleON;

	public bool productionTakesTimeON;

	public bool infiniteAmmoAndFuelON;

	public bool complexSuppliesON;

	public bool planeMoveAfterAttackON;

	public bool zoneOfControlON;

	public bool upkeepON;

	public bool damageReductionRepeatedAttacks;

	public bool finiteManpower;

	public bool prototypesDisabled;

	public bool noAAsupportDuringAirFight;

	public bool stricterFogOfWar;

	public int difficultyValue;

	public int winterDamageValue;

	public int randomDamageValue;

	public void Backup()
	{
		isBackup = true;
		historicalFocusON = PlayerSettings.Instance.ISHistoricalFocus;
		fogOfWarON = PlayerSettings.Instance.IsFogOfWar;
		researchON = PlayerSettings.Instance.TechTree;
		manpowerON = PlayerSettings.Instance.Manpower;
		policiesON = PlayerSettings.Instance.Policies;
		weatherON = PlayerSettings.Instance.IsWeather;
		partisansON = PlayerSettings.Instance.Partisans;
		realisticPlanesON = PlayerSettings.Instance.RealisticPlanes;
		overrunON = PlayerSettings.Instance.Overrun;
		moraleON = PlayerSettings.Instance.IsMorale;
		productionTakesTimeON = PlayerSettings.Instance.ProductionTakesTime;
		infiniteAmmoAndFuelON = PlayerSettings.Instance.IsInfiniteAmmoAndFuel;
		complexSuppliesON = PlayerSettings.Instance.ComplexSupplies;
		planeMoveAfterAttackON = PlayerSettings.Instance.OneMoreTilePlanes;
		difficultyValue = PlayerSettings.Instance.DifficultyLevel;
		winterDamageValue = PlayerSettings.Instance.WinterDamageValue;
		randomDamageValue = PlayerSettings.Instance.RandomDamageModifier;
		upkeepON = PlayerSettings.Instance.Upkeep;
		zoneOfControlON = PlayerSettings.Instance.ZoneOfControl;
		damageReductionRepeatedAttacks = PlayerSettings.Instance.DamageReductionRepeatedAttacks;
		finiteManpower = PlayerSettings.Instance.FiniteManpower;
		prototypesDisabled = PlayerSettings.Instance.PrototypesDisabled;
		noAAsupportDuringAirFight = PlayerSettings.Instance.NoAAsupportDuringAirFight;
		stricterFogOfWar = PlayerSettings.Instance.StricterFogOfWar;
	}

	public void LoadBackup()
	{
		isBackup = false;
		PlayerSettings.ApplyGameSettings(this);
	}
}

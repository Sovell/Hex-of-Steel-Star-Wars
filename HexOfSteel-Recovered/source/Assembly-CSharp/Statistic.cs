using System;

[Serializable]
public class Statistic
{
	public byte HQpointsAMount { get; set; }

	public int IncomeAmount { get; set; }

	public int MoneyAmount { get; set; }

	public int ManpowerAmount { get; set; }

	public int RealisticManpowerAmount { get; set; }

	public short NumberOfUnits { get; set; }

	public short VPsAmount { get; set; }

	public short ShipsNumber { get; set; }

	public short PlanesNumber { get; set; }

	public short ArtilleryNumber { get; set; }

	public short TanksNumber { get; set; }

	public short InfantryNumber { get; set; }
}

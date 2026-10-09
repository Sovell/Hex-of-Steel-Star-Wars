using System;

[Serializable]
public class Hero
{
	public string Name { get; set; }

	public string Country { get; set; }

	public string Type { get; set; }

	public short YearStartAvailability { get; set; }

	public short YearStopAvailability { get; set; }

	public short SoftDamageBonus { get; set; }

	public short HardDamageBonus { get; set; }

	public short AirDamageBonus { get; set; }

	public byte MPBonus { get; set; }

	public byte VisibilityBonus { get; set; }
}

using System;

[Serializable]
public struct Coordinates
{
	public short posX;

	public short posY;

	public Coordinates(short p_posX, short p_posY)
	{
		posX = p_posX;
		posY = p_posY;
	}
}

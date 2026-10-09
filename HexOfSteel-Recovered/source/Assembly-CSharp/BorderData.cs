using System.Collections.Generic;

public sealed class BorderData
{
	private readonly HashSet<Tile> _myTiles = new HashSet<Tile>();

	private readonly HashSet<Tile> _theirTiles = new HashSet<Tile>();

	public Player OtherPlayer { get; private set; }

	public int EdgeCount { get; internal set; }

	public IReadOnlyCollection<Tile> MyTiles => _myTiles;

	public IReadOnlyCollection<Tile> TheirTiles => _theirTiles;

	public int MyTilesCount => _myTiles.Count;

	public int TheirTilesCount => _theirTiles.Count;

	public BorderData(Player p_otherPlayer)
	{
		OtherPlayer = p_otherPlayer;
	}

	internal void AddMy(Tile t)
	{
		_myTiles.Add(t);
	}

	internal void AddTheir(Tile t)
	{
		_theirTiles.Add(t);
	}
}

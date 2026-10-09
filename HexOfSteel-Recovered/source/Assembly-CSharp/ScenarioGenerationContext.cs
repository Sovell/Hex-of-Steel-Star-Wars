using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioGenerationContext
{
	public const int UnassignedRegionIndex = -1;

	public Map Map { get; private set; }

	public ScenarioGenerationRequest Request { get; private set; }

	public Tile[,] Tiles => Map.TilesTable;

	public System.Random Random { get; private set; }

	public float[,] Elevation { get; private set; }

	public float[,] Moisture { get; private set; }

	public int[,] Regions { get; private set; }

	public Vector2 Center { get; private set; }

	public float MaxCenterDistance { get; private set; }

	public List<ScenarioPlayerRegion> PlayerRegions { get; private set; }

	public int TotalVictoryPoints { get; set; }

	public ScenarioGenerationContext(Map p_map, ScenarioGenerationRequest p_request)
	{
		Map = p_map;
		Request = p_request;
		Random = new System.Random(p_request.RuntimeSeed);
		Elevation = new float[p_map.SizeX, p_map.SizeY];
		Moisture = new float[p_map.SizeX, p_map.SizeY];
		Regions = new int[p_map.SizeX, p_map.SizeY];
		for (int i = 0; i < p_map.SizeX; i++)
		{
			for (int j = 0; j < p_map.SizeY; j++)
			{
				Regions[i, j] = -1;
			}
		}
		Center = new Vector2((float)(p_map.SizeX - 1) * 0.5f, (float)(p_map.SizeY - 1) * 0.5f);
		float num = 1f;
		Vector2[] array = new Vector2[4]
		{
			Vector2.zero,
			new Vector2(0f, p_map.SizeY - 1),
			new Vector2(p_map.SizeX - 1, 0f),
			new Vector2(p_map.SizeX - 1, p_map.SizeY - 1)
		};
		foreach (Vector2 b in array)
		{
			num = Mathf.Max(num, Vector2.Distance(Center, b));
		}
		MaxCenterDistance = num;
		PlayerRegions = new List<ScenarioPlayerRegion>();
		for (int l = 0; l < GameData.Instance.listOfPlayers.Count; l++)
		{
			PlayerRegions.Add(new ScenarioPlayerRegion(GameData.Instance.listOfPlayers[l], l));
		}
	}

	public float NextFloat()
	{
		return (float)Random.NextDouble();
	}

	public int NextInt(int p_minInclusive, int p_maxExclusive)
	{
		return Random.Next(p_minInclusive, p_maxExclusive);
	}

	public void SetRegionIndex(Tile p_tile, int p_regionIndex)
	{
		Regions[p_tile.PosX, p_tile.PosY] = p_regionIndex;
	}

	public int GetRegionIndex(Tile p_tile)
	{
		return Regions[p_tile.PosX, p_tile.PosY];
	}

	public int GetDistanceToMapBorder(Tile p_tile)
	{
		short posX = p_tile.PosX;
		int b = Map.SizeX - 1 - p_tile.PosX;
		return Mathf.Min(b: Mathf.Min(b: p_tile.PosY, a: Map.SizeY - 1 - p_tile.PosY), a: Mathf.Min(posX, b));
	}

	public float GetNormalizedCenterDistance(Tile p_tile)
	{
		return Vector2.Distance(b: new Vector2(p_tile.PosX, p_tile.PosY), a: Center) / MaxCenterDistance;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public static class RailroadGenerator
{
	private class PathNode : IComparable<PathNode>
	{
		public Tile tile;

		public PathNode parent;

		public float g;

		public float h;

		public float F => g + h;

		public int CompareTo(PathNode other)
		{
			return F.CompareTo(other.F);
		}
	}

	private struct ConnectionCandidate
	{
		public Tile a;

		public Tile b;

		public int distance;
	}

	public static List<Tile> GenerateRailroads(Map map, int targetRailroads, Action<Tile> onBeforeModify = null, Action<Tile> onAfterModify = null)
	{
		return GenerateRailroads(map, targetRailroads, null, onBeforeModify, onAfterModify);
	}

	public static List<Tile> GenerateRailroads(Map p_map, int p_targetRailroads, Predicate<Tile> p_nodeFilter, Action<Tile> p_onBeforeModify = null, Action<Tile> p_onAfterModify = null)
	{
		Tile[,] tilesTable = p_map.TilesTable;
		int sizeX = p_map.SizeX;
		int sizeY = p_map.SizeY;
		List<Tile> list = new List<Tile>();
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				Tile tile = tilesTable[i, j];
				if (IsRailroadNode(tile) && (p_nodeFilter == null || p_nodeFilter(tile)))
				{
					list.Add(tile);
				}
			}
		}
		if (list.Count < 2)
		{
			Debug.LogWarning("RailroadGenerator: Not enough valid railroad nodes found.");
			return new List<Tile>();
		}
		HashSet<string> generatedRailroadPairs = GetGeneratedRailroadPairs(p_map);
		int num = 0;
		int num2 = 1000;
		int num3 = 0;
		HashSet<Tile> hashSet = new HashSet<Tile>();
		while (num < p_targetRailroads && num3 < num2)
		{
			num3++;
			Tile tile2 = list[UnityEngine.Random.Range(0, list.Count)];
			Tile tile3 = list[UnityEngine.Random.Range(0, list.Count)];
			if (tile2 == tile3 || GetDistance(tile2, tile3) <= 5 || generatedRailroadPairs.Contains(GetRailroadPairKey(tile2, tile3)))
			{
				continue;
			}
			List<Tile> list2 = FindRailroadPath(tile2, tile3, p_map);
			if (list2 == null || list2.Count <= 2)
			{
				continue;
			}
			bool flag = false;
			for (int k = 0; k < list2.Count; k++)
			{
				Tile tile4 = list2[k];
				bool flag2 = tile4 == tile2 || tile4 == tile3;
				hashSet.Add(tile4);
				if (tile4.enumType == TileTypes.CITY || tile4.enumType == TileTypes.FACTORY)
				{
					tile4.Railroad = true;
					tile4.SetRuntimeOptimizations();
					continue;
				}
				if (flag2)
				{
					tile4.Railroad = true;
					tile4.SetRuntimeOptimizations();
					continue;
				}
				int hexDirection = GetHexDirection(tile4, list2[k - 1]);
				int hexDirection2 = GetHexDirection(tile4, list2[k + 1]);
				bool flag3 = IsTileOverridable(tile4);
				string text = "";
				string type = "Plain";
				bool flag4 = false;
				if (IsRoad(tile4))
				{
					text = GetRoadIntersectionName(tile4, hexDirection, hexDirection2);
					if (!string.IsNullOrEmpty(text))
					{
						type = "Road";
					}
					else
					{
						flag3 = false;
					}
				}
				else if (tile4.enumType == TileTypes.RIVER)
				{
					text = GetBridgeRailroadName(tile4, hexDirection, hexDirection2);
					if (!string.IsNullOrEmpty(text))
					{
						type = "River";
						flag4 = true;
					}
					else
					{
						flag3 = false;
					}
				}
				else
				{
					text = GetRailroadSpriteName(hexDirection, hexDirection2);
					if (string.IsNullOrEmpty(text))
					{
						flag3 = false;
					}
				}
				if (flag3)
				{
					p_onBeforeModify?.Invoke(tile4);
					tile4.Name = text;
					tile4.Type = type;
					tile4.Railroad = true;
					tile4.FilterType = "FilterRailroad";
					if (flag4)
					{
						tile4.Bridge = true;
					}
					tile4.SetRuntimeOptimizations();
					flag = true;
					p_onAfterModify?.Invoke(tile4);
				}
			}
			if (flag)
			{
				num++;
				generatedRailroadPairs.Add(GetRailroadPairKey(tile2, tile3));
			}
		}
		Debug.Log($"RailroadGenerator: Created {num} railroads in {num3} attempts.");
		return new List<Tile>(hashSet);
	}

	public static bool IsMajorRailroadNode(Tile p_tile)
	{
		if (!IsRailroadNode(p_tile))
		{
			return false;
		}
		return p_tile.GetMinimumOccupancyForWork() >= 3;
	}

	public static string GetRailroadPairKey(List<Tile> p_tiles)
	{
		if (p_tiles == null)
		{
			return "";
		}
		Tile tile = null;
		Tile p_secondTile = null;
		for (int i = 0; i < p_tiles.Count; i++)
		{
			Tile tile2 = p_tiles[i];
			if (IsRailroadNode(tile2))
			{
				if (tile == null)
				{
					tile = tile2;
				}
				else if (tile2 != tile)
				{
					p_secondTile = tile2;
					break;
				}
			}
		}
		return GetRailroadPairKey(tile, p_secondTile);
	}

	public static void RemoveGeneratedRailroadPair(Map p_map, string p_pairKey)
	{
		if (p_map != null && !string.IsNullOrEmpty(p_pairKey))
		{
			GetGeneratedRailroadPairs(p_map).Remove(p_pairKey);
		}
	}

	private static HashSet<string> GetGeneratedRailroadPairs(Map p_map)
	{
		if (p_map.generatedRailroadPairs == null)
		{
			p_map.generatedRailroadPairs = new HashSet<string>();
		}
		return p_map.generatedRailroadPairs;
	}

	private static string GetRailroadPairKey(Tile p_firstTile, Tile p_secondTile)
	{
		if (!IsRailroadNode(p_firstTile) || !IsRailroadNode(p_secondTile))
		{
			return "";
		}
		string text = GetRailroadNodeKey(p_firstTile);
		string text2 = GetRailroadNodeKey(p_secondTile);
		if (string.CompareOrdinal(text, text2) > 0)
		{
			string text3 = text;
			text = text2;
			text2 = text3;
		}
		return text + "|" + text2;
	}

	private static string GetRailroadNodeKey(Tile p_tile)
	{
		return p_tile.PosX + "," + p_tile.PosY;
	}

	public static List<Tile> FindRailroadPath(Tile start, Tile end, Map map)
	{
		List<PathNode> list = new List<PathNode>();
		HashSet<string> hashSet = new HashSet<string>();
		PathNode item = new PathNode
		{
			tile = start,
			parent = null,
			g = 0f,
			h = GetDistance(start, end)
		};
		list.Add(item);
		int num = 0;
		int num2 = 10000;
		while (list.Count > 0 && num < num2)
		{
			num++;
			list.Sort();
			PathNode pathNode = list[0];
			list.RemoveAt(0);
			if (pathNode.tile == end)
			{
				return ReconstructPath(pathNode);
			}
			int num3 = -1;
			if (pathNode.parent != null)
			{
				num3 = GetHexDirection(pathNode.tile, pathNode.parent.tile);
			}
			string item2 = $"{pathNode.tile.PosX},{pathNode.tile.PosY},{num3}";
			if (hashSet.Contains(item2))
			{
				continue;
			}
			hashSet.Add(item2);
			foreach (Tile neighbour in pathNode.tile.neighbours)
			{
				if (neighbour == null || (!IsTileOverridable(neighbour) && neighbour != end) || ((neighbour.enumType == TileTypes.CITY || neighbour.enumType == TileTypes.FACTORY) && neighbour != end) || (IsRoad(pathNode.tile) && IsRoad(neighbour) && neighbour != end) || (pathNode.tile.enumType == TileTypes.RIVER && neighbour.enumType == TileTypes.RIVER && neighbour != end))
				{
					continue;
				}
				if (pathNode.parent != null)
				{
					int hexDirection = GetHexDirection(pathNode.tile, neighbour);
					if (IsRoad(pathNode.tile))
					{
						if (string.IsNullOrEmpty(GetRoadIntersectionName(pathNode.tile, num3, hexDirection)))
						{
							continue;
						}
					}
					else if (pathNode.tile.enumType == TileTypes.RIVER)
					{
						if (string.IsNullOrEmpty(GetBridgeRailroadName(pathNode.tile, num3, hexDirection)))
						{
							continue;
						}
					}
					else if (GetRailroadSpriteName(num3, hexDirection) == "")
					{
						continue;
					}
				}
				if (neighbour != end || pathNode.parent == null || !IsRoad(end) || !IsRoad(pathNode.tile))
				{
					float num4 = 1f + UnityEngine.Random.Range(0f, 0.8f);
					float g = pathNode.g + num4;
					PathNode item3 = new PathNode
					{
						tile = neighbour,
						parent = pathNode,
						g = g,
						h = GetDistance(neighbour, end)
					};
					list.Add(item3);
				}
			}
		}
		return null;
	}

	private static List<Tile> ReconstructPath(PathNode node)
	{
		List<Tile> list = new List<Tile>();
		while (node != null)
		{
			list.Add(node.tile);
			node = node.parent;
		}
		list.Reverse();
		return list;
	}

	private static bool IsTileOverridable(Tile t)
	{
		if (t.enumType == TileTypes.CITY || t.enumType == TileTypes.FACTORY)
		{
			return true;
		}
		if (t.Railroad)
		{
			return false;
		}
		if (t.enumType == TileTypes.HARBOUR)
		{
			return false;
		}
		if (t.enumType == TileTypes.AIRPORT)
		{
			return false;
		}
		if (t.enumType == TileTypes.REFINERY)
		{
			return false;
		}
		if (t.enumType == TileTypes.TRENCH)
		{
			return false;
		}
		if (t.IsVictoryPoint)
		{
			return false;
		}
		if (IsRoad(t))
		{
			return true;
		}
		if (t.enumType == TileTypes.RIVER)
		{
			return true;
		}
		if (t.enumType == TileTypes.PLAIN)
		{
			return true;
		}
		if (t.enumType == TileTypes.MARSH)
		{
			return true;
		}
		if (t.enumType == TileTypes.FOREST)
		{
			return true;
		}
		if (t.enumType == TileTypes.HILL)
		{
			return true;
		}
		if (t.enumType == TileTypes.MOUNTAIN)
		{
			return true;
		}
		return false;
	}

	private static bool IsRailroadNode(Tile p_tile)
	{
		if (p_tile != null)
		{
			if (p_tile.enumType != TileTypes.CITY)
			{
				return p_tile.enumType == TileTypes.FACTORY;
			}
			return true;
		}
		return false;
	}

	private static bool IsRoad(Tile t)
	{
		if (t.enumType != TileTypes.ROAD && !(t.Type == "Road"))
		{
			return t.Name.StartsWith("Road");
		}
		return true;
	}

	private static int GetDistance(Tile a, Tile b)
	{
		int posX = a.PosX;
		int num = a.PosY - (a.PosX - (a.PosX & 1)) / 2;
		int posX2 = b.PosX;
		int num2 = b.PosY - (b.PosX - (b.PosX & 1)) / 2;
		return (Mathf.Abs(posX - posX2) + Mathf.Abs(posX + num - posX2 - num2) + Mathf.Abs(num - num2)) / 2;
	}

	private static int GetHexDirection(Tile center, Tile neighbor)
	{
		int num = neighbor.PosX - center.PosX;
		int num2 = neighbor.PosY - center.PosY;
		bool flag = center.PosX % 2 == 0;
		if (num == 0)
		{
			if (num2 > 0)
			{
				return 0;
			}
			if (num2 < 0)
			{
				return 3;
			}
		}
		else if (num > 0)
		{
			if (flag)
			{
				switch (num2)
				{
				case 1:
					return 1;
				case 0:
					return 2;
				}
			}
			else
			{
				switch (num2)
				{
				case 0:
					return 1;
				case -1:
					return 2;
				}
			}
		}
		else if (num < 0)
		{
			if (flag)
			{
				switch (num2)
				{
				case 1:
					return 5;
				case 0:
					return 4;
				}
			}
			else
			{
				switch (num2)
				{
				case 0:
					return 5;
				case -1:
					return 4;
				}
			}
		}
		return -1;
	}

	private static string GetKey(int a, int b)
	{
		int num = Mathf.Min(a, b);
		int num2 = Mathf.Max(a, b);
		return num + "-" + num2;
	}

	private static string GetRailroadSpriteName(int sideA, int sideB)
	{
		if (sideA == -1 || sideB == -1)
		{
			return "";
		}
		switch (GetKey(sideA, sideB))
		{
		case "0-3":
			return "Railroad1";
		case "1-4":
			return "Railroad2";
		case "2-5":
			return "Railroad3";
		case "1-5":
			return "Railroad4";
		case "2-4":
			return "Railroad5";
		case "0-2":
			return "Railroad6";
		case "1-3":
			return "Railroad7";
		case "3-5":
			return "Railroad8";
		case "0-4":
			return "Railroad9";
		default:
			return "";
		}
	}

	private static string GetRoadIntersectionName(Tile roadTile, int railIn, int railOut)
	{
		if (!IsRoad(roadTile))
		{
			return "";
		}
		string roadAxis = GetRoadAxis(roadTile.Name);
		if (roadAxis == "")
		{
			return "";
		}
		string key = GetKey(railIn, railOut);
		if (roadAxis == "0-3" && key == "2-5")
		{
			return "Road1Intersection";
		}
		if (roadAxis == "0-3" && key == "1-4")
		{
			return "Road2Intersection";
		}
		if (roadAxis == "1-5" && key == "0-3")
		{
			return "Road3Intersection";
		}
		if (roadAxis == "1-4" && key == "0-3")
		{
			return "Road4Intersection";
		}
		if (roadAxis == "2-5" && key == "0-3")
		{
			return "Road5Intersection";
		}
		if (roadAxis == "2-5" && key == "1-4")
		{
			return "Road6Intersection";
		}
		if (roadAxis == "2-4" && key == "0-3")
		{
			return "Road7Intersection";
		}
		if (roadAxis == "1-4" && key == "2-5")
		{
			return "Road8Intersection";
		}
		if (roadAxis == "1-5" && key == "2-4")
		{
			return "Road9Intersection";
		}
		if (roadAxis == "3-5" && key == "1-4")
		{
			return "Road10Intersection";
		}
		if (roadAxis == "0-4" && key == "2-5")
		{
			return "Road11Intersection";
		}
		if (roadAxis == "0-2" && key == "1-4")
		{
			return "Road12Intersection";
		}
		if (roadAxis == "1-3" && key == "2-5")
		{
			return "Road13Intersection";
		}
		if (roadAxis == "2-4" && key == "1-5")
		{
			return "Road14Intersection";
		}
		if (roadAxis == "0-3" && key == "1-5")
		{
			return "Road15Intersection";
		}
		if (roadAxis == "0-3" && key == "2-4")
		{
			return "Road16Intersection";
		}
		if (roadAxis == "2-5" && key == "1-3")
		{
			return "Road17Intersection";
		}
		if (roadAxis == "1-4" && key == "0-2")
		{
			return "Road18Intersection";
		}
		if (roadAxis == "2-5" && key == "0-4")
		{
			return "Road19Intersection";
		}
		if (roadAxis == "1-4" && key == "3-5")
		{
			return "Road20Intersection";
		}
		return "";
	}

	private static string GetBridgeRailroadName(Tile riverTile, int railIn, int railOut)
	{
		if (riverTile.enumType != TileTypes.RIVER)
		{
			return "";
		}
		string riverAxis = GetRiverAxis(riverTile.Name);
		if (riverAxis == "")
		{
			return "";
		}
		string key = GetKey(railIn, railOut);
		if (riverAxis == "1-4" && key == "0-3")
		{
			return "River1BridgeRailroad";
		}
		if (riverAxis == "1-4" && key == "2-5")
		{
			return "River2BridgeRailroad";
		}
		if (riverAxis == "2-5" && key == "1-4")
		{
			return "River3BridgeRailroad";
		}
		if (riverAxis == "1-5" && key == "0-3")
		{
			return "River4BridgeRailroad";
		}
		if (riverAxis == "2-4" && key == "0-3")
		{
			return "River5BridgeRailroad";
		}
		if (riverAxis == "0-3" && key == "1-4")
		{
			return "River6BridgeRailroad";
		}
		if (riverAxis == "0-3" && key == "2-5")
		{
			return "River7BridgeRailroad";
		}
		if (riverAxis == "3-5" && key == "2-5")
		{
			return "River8BridgeRailroad";
		}
		if (riverAxis == "0-2" && key == "1-4")
		{
			return "River9BridgeRailroad";
		}
		if (riverAxis == "1-3" && key == "2-5")
		{
			return "River10BridgeRailroad";
		}
		if (riverAxis == "3-5" && key == "1-4")
		{
			return "River11BridgeRailroad";
		}
		if (riverAxis == "2-5" && key == "0-3")
		{
			return "River12BridgeRailroad";
		}
		if (riverAxis == "0-3" && key == "2-4")
		{
			return "River13BridgeRailroad";
		}
		if (riverAxis == "0-3" && key == "1-5")
		{
			return "River14BridgeRailroad";
		}
		return "";
	}

	private static string GetRoadAxis(string roadName)
	{
		switch (roadName.Replace("Road", ""))
		{
		case "1":
			return "0-3";
		case "2":
			return "1-5";
		case "3":
			return "2-4";
		case "4":
			return "1-4";
		case "5":
			return "2-5";
		case "6":
			return "0-4";
		case "7":
			return "0-2";
		case "8":
			return "1-3";
		case "9":
			return "3-5";
		default:
			return "";
		}
	}

	private static string GetRiverAxis(string riverName)
	{
		switch (riverName.Replace("River", ""))
		{
		case "1":
			return "0-3";
		case "2":
			return "1-4";
		case "3":
			return "2-5";
		case "4":
			return "1-5";
		case "5":
			return "2-4";
		case "6":
			return "0-4";
		case "7":
			return "0-2";
		case "8":
			return "1-3";
		case "9":
			return "3-5";
		default:
			return "";
		}
	}
}

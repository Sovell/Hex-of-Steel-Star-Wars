using System;
using System.Collections.Generic;
using UnityEngine;

public static class RiverGenerator
{
	public static List<Tile> GenerateRivers(Map map, int targetRivers, Action<Tile> onBeforeModify = null, Action<Tile> onAfterModify = null)
	{
		Tile[,] tilesTable = map.TilesTable;
		int sizeX = map.SizeX;
		int sizeY = map.SizeY;
		List<Tile> list = new List<Tile>();
		List<Tile> list2 = new List<Tile>();
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				Tile tile = tilesTable[i, j];
				bool flag = i == 0 || i == sizeX - 1 || j == 0 || j == sizeY - 1;
				bool flag2 = false;
				foreach (Tile neighbour in tile.neighbours)
				{
					if (neighbour.enumType == TileTypes.SEA || neighbour.Name == "Lake")
					{
						flag2 = true;
						break;
					}
				}
				if (tile.enumType != TileTypes.SEA && tile.Name != "Lake" && !IsRoad(tile) && !IsRailroad(tile) && (flag | flag2))
				{
					list.Add(tile);
					list2.Add(tile);
				}
			}
		}
		if (list.Count == 0 || list2.Count == 0)
		{
			Debug.LogWarning("RiverGenerator: No potential starts/ends found.");
			return new List<Tile>();
		}
		int num = 0;
		int num2 = 500;
		int num3 = 0;
		HashSet<Tile> hashSet = new HashSet<Tile>();
		List<Tile> list3 = new List<Tile>();
		while (num < targetRivers && num3 < num2)
		{
			num3++;
			Tile tile2 = list[UnityEngine.Random.Range(0, list.Count)];
			Tile tile3 = list2[UnityEngine.Random.Range(0, list2.Count)];
			if (SharesMapEdgeSide(tile2, tile3, sizeX, sizeY) || hashSet.Contains(tile2) || hashSet.Contains(tile3) || tile2.enumType == TileTypes.RIVER || tile3.enumType == TileTypes.RIVER)
			{
				continue;
			}
			bool flag3 = false;
			foreach (Tile item in list3)
			{
				if (GetDistance(tile2, item) < 3 || GetDistance(tile3, item) < 3)
				{
					flag3 = true;
					break;
				}
			}
			if (flag3 || GetDistance(tile2, tile3) < 5)
			{
				continue;
			}
			List<Tile> list4 = FindRiverPath(tile2, tile3, hashSet, map.SizeX);
			if (list4 == null || list4.Count <= 5)
			{
				continue;
			}
			num++;
			list3.Add(tile2);
			list3.Add(tile3);
			foreach (Tile item2 in list4)
			{
				hashSet.Add(item2);
			}
			for (int k = 0; k < list4.Count; k++)
			{
				Tile tile4 = list4[k];
				bool flag4 = IsRoad(tile4);
				bool flag5 = !flag4 && IsRailroad(tile4);
				int num4 = -1;
				int num5 = -1;
				num4 = ((k <= 0) ? GetEndpointDirection(tile4, GetHexDirection(tile4, list4[k + 1]), map) : GetHexDirection(tile4, list4[k - 1]));
				num5 = ((k >= list4.Count - 1) ? GetEndpointDirection(tile4, GetHexDirection(tile4, list4[k - 1]), map) : GetHexDirection(tile4, list4[k + 1]));
				bool flag6 = IsTileOverridable(tile4);
				bool flag7 = false;
				string text = "";
				bool flag8 = false;
				if (flag4)
				{
					text = GetBridgeSpriteName(num4, num5, tile4);
					if (!string.IsNullOrEmpty(text))
					{
						flag7 = true;
						flag6 = true;
					}
					else
					{
						flag6 = false;
					}
				}
				else if (flag5)
				{
					text = GetRailroadBridgeSpriteName(num4, num5, tile4);
					if (!string.IsNullOrEmpty(text))
					{
						flag7 = true;
						flag6 = true;
						flag8 = true;
					}
					else
					{
						flag6 = false;
					}
				}
				if (!flag6)
				{
					continue;
				}
				onBeforeModify?.Invoke(tile4);
				tile4.Type = "River";
				if (flag7)
				{
					tile4.Name = text;
					tile4.Bridge = true;
					if (flag8)
					{
						tile4.Railroad = true;
						tile4.FilterType = "FilterRailroad";
					}
				}
				else
				{
					tile4.Name = GetRiverSpriteName(num4, num5);
				}
				tile4.SetRuntimeOptimizations();
				onAfterModify?.Invoke(tile4);
			}
		}
		Debug.Log($"RiverGenerator: Created {num} rivers in {num3} attempts.");
		return new List<Tile>(hashSet);
	}

	public static List<Tile> FindRiverPath(Tile start, Tile end, HashSet<Tile> occupiedTiles, int mapSizeX)
	{
		List<Tile> list = new List<Tile>();
		Tile tile = start;
		list.Add(tile);
		int num = mapSizeX * 3;
		int num2 = 0;
		while (tile != end && num2 < num)
		{
			num2++;
			List<Tile> list2 = new List<Tile>();
			Tile tile2 = ((list.Count > 1) ? list[list.Count - 2] : null);
			int num3 = -1;
			if (tile2 != null)
			{
				num3 = GetHexDirection(tile, tile2);
			}
			List<Tile> list3 = new List<Tile>(tile.neighbours);
			list3.Sort((Tile a, Tile b) => GetDistance(a, end).CompareTo(GetDistance(b, end)));
			bool flag = IsRoad(tile);
			bool flag2 = !flag && IsRailroad(tile);
			foreach (Tile item in list3)
			{
				if (list.Contains(item) || (occupiedTiles.Contains(item) && item != end) || (item.enumType == TileTypes.RIVER && item != end) || ((item.enumType == TileTypes.SEA || item.Name == "Lake") && item != end))
				{
					continue;
				}
				if (flag)
				{
					if (IsRoad(item))
					{
						continue;
					}
					if (tile2 != null)
					{
						int hexDirection = GetHexDirection(tile, item);
						if (string.IsNullOrEmpty(GetBridgeSpriteName(num3, hexDirection, tile)))
						{
							continue;
						}
					}
				}
				if (flag2)
				{
					if (IsRailroad(item))
					{
						continue;
					}
					if (tile2 != null)
					{
						int hexDirection2 = GetHexDirection(tile, item);
						if (string.IsNullOrEmpty(GetRailroadBridgeSpriteName(num3, hexDirection2, tile)))
						{
							continue;
						}
					}
				}
				bool flag3 = false;
				foreach (Tile neighbour in item.neighbours)
				{
					if (neighbour != tile && list.Contains(neighbour))
					{
						flag3 = true;
						break;
					}
				}
				if (flag3)
				{
					continue;
				}
				if (tile2 != null)
				{
					int hexDirection3 = GetHexDirection(tile, item);
					if (!IsValidRiverTurn(num3, hexDirection3))
					{
						continue;
					}
				}
				list2.Add(item);
			}
			if (list2.Count == 0)
			{
				return null;
			}
			int maxExclusive = Mathf.Min(3, list2.Count);
			Tile tile3 = list2[UnityEngine.Random.Range(0, maxExclusive)];
			list.Add(tile3);
			tile = tile3;
			if (tile == end)
			{
				return list;
			}
			if (!tile.neighbours.Contains(end))
			{
				continue;
			}
			int hexDirection4 = GetHexDirection(tile, list[list.Count - 2]);
			int hexDirection5 = GetHexDirection(tile, end);
			if (!IsValidRiverTurn(hexDirection4, hexDirection5))
			{
				continue;
			}
			bool flag4 = true;
			if (IsRoad(tile))
			{
				if (string.IsNullOrEmpty(GetBridgeSpriteName(hexDirection4, hexDirection5, tile)))
				{
					flag4 = false;
				}
				if (IsRoad(end))
				{
					flag4 = false;
				}
			}
			if (IsRailroad(tile))
			{
				if (string.IsNullOrEmpty(GetRailroadBridgeSpriteName(hexDirection4, hexDirection5, tile)))
				{
					flag4 = false;
				}
				if (IsRailroad(end))
				{
					flag4 = false;
				}
			}
			if (flag4)
			{
				list.Add(end);
				return list;
			}
		}
		return null;
	}

	private static bool IsTileOverridable(Tile t)
	{
		if (t.enumType == TileTypes.CITY)
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
		if (t.enumType == TileTypes.FACTORY)
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
		if (t.Railroad && GetRailroadAxis(t.Name) != "")
		{
			return true;
		}
		if (t.Railroad)
		{
			return false;
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

	private static bool IsRoad(Tile t)
	{
		if (t.enumType != TileTypes.ROAD && !(t.Type == "Road"))
		{
			return t.Name.StartsWith("Road");
		}
		return true;
	}

	private static bool IsRailroad(Tile t)
	{
		if (!t.Railroad)
		{
			return t.Name.StartsWith("Railroad");
		}
		return true;
	}

	private static int GetEndpointDirection(Tile t, int existingSide, Map map)
	{
		List<int> list = new List<int>();
		foreach (Tile neighbour in t.neighbours)
		{
			if (neighbour.Type == "Sea" || neighbour.Name == "Lake")
			{
				list.Add(GetHexDirection(t, neighbour));
			}
		}
		int num = map.SizeX - 1;
		int num2 = map.SizeY - 1;
		if (t.PosX == 0)
		{
			list.Add(4);
			list.Add(5);
		}
		else if (t.PosX == num)
		{
			list.Add(1);
			list.Add(2);
		}
		if (t.PosY == 0)
		{
			list.Add(3);
		}
		else if (t.PosY == num2)
		{
			list.Add(0);
		}
		foreach (int item in list)
		{
			if (IsValidRiverTurn(existingSide, item) && (!IsRoad(t) || !string.IsNullOrEmpty(GetBridgeSpriteName(existingSide, item, t))))
			{
				return item;
			}
		}
		return -1;
	}

	private static bool IsValidRiverTurn(int sideA, int sideB)
	{
		int num = Mathf.Min(sideA, sideB);
		int num2 = Mathf.Max(sideA, sideB);
		switch (num + "-" + num2)
		{
		case "0-3":
		case "1-4":
		case "2-5":
		case "1-5":
		case "2-4":
		case "0-4":
		case "0-2":
		case "1-3":
		case "3-5":
			return true;
		default:
			return false;
		}
	}

	private static bool SharesMapEdgeSide(Tile p_startTile, Tile p_endTile, int p_sizeX, int p_sizeY)
	{
		int mapEdgeMask = GetMapEdgeMask(p_startTile, p_sizeX, p_sizeY);
		int mapEdgeMask2 = GetMapEdgeMask(p_endTile, p_sizeX, p_sizeY);
		if (mapEdgeMask == 0 || mapEdgeMask2 == 0)
		{
			return false;
		}
		return (mapEdgeMask & mapEdgeMask2) != 0;
	}

	private static int GetMapEdgeMask(Tile p_tile, int p_sizeX, int p_sizeY)
	{
		int num = 0;
		if (p_tile.PosX == 0)
		{
			num |= 1;
		}
		if (p_tile.PosX == p_sizeX - 1)
		{
			num |= 2;
		}
		if (p_tile.PosY == 0)
		{
			num |= 4;
		}
		if (p_tile.PosY == p_sizeY - 1)
		{
			num |= 8;
		}
		return num;
	}

	private static int GetDistance(Tile a, Tile b)
	{
		int posX = a.PosX;
		int num = a.PosY - (a.PosX - (a.PosX & 1)) / 2;
		int posX2 = b.PosX;
		int num2 = b.PosY - (b.PosX - (b.PosX & 1)) / 2;
		return (Mathf.Abs(posX - posX2) + Mathf.Abs(posX + num - posX2 - num2) + Mathf.Abs(num - num2)) / 2;
	}

	private static string GetBridgeSpriteName(int riverIn, int riverOut, Tile roadTile)
	{
		if (!roadTile.Name.StartsWith("Road"))
		{
			return "";
		}
		string roadType = roadTile.Name.Replace("Road", "");
		int num = Mathf.Min(riverIn, riverOut);
		int num2 = Mathf.Max(riverIn, riverOut);
		string text = num + "-" + num2;
		if (text == "1-4" && CheckRoadAxis(roadType, 5, 2))
		{
			return "River2Bridge";
		}
		if (text == "2-5" && CheckRoadAxis(roadType, 4, 1))
		{
			return "River3Bridge";
		}
		if (text == "1-5" && CheckRoadAxis(roadType, 0, 3))
		{
			return "River4Bridge";
		}
		if (text == "2-4" && CheckRoadAxis(roadType, 0, 3))
		{
			return "River5Bridge";
		}
		if (text == "0-4" && CheckRoadAxis(roadType, 5, 2))
		{
			return "River6Bridge";
		}
		if (text == "0-2" && CheckRoadAxis(roadType, 4, 1))
		{
			return "River7Bridge";
		}
		if (text == "1-3" && CheckRoadAxis(roadType, 5, 2))
		{
			return "River8Bridge";
		}
		if (text == "3-5" && CheckRoadAxis(roadType, 4, 1))
		{
			return "River9Bridge";
		}
		if (text == "0-3" && CheckRoadAxis(roadType, 4, 1))
		{
			return "River10Bridge";
		}
		if (text == "0-3" && CheckRoadAxis(roadType, 5, 2))
		{
			return "River11Bridge";
		}
		if (text == "1-4" && CheckRoadAxis(roadType, 0, 3))
		{
			return "River12Bridge";
		}
		if (text == "2-5" && CheckRoadAxis(roadType, 0, 3))
		{
			return "River13Bridge";
		}
		if (text == "0-3" && CheckRoadAxis(roadType, 4, 2))
		{
			return "River14Bridge";
		}
		if (text == "0-3" && CheckRoadAxis(roadType, 5, 1))
		{
			return "River15Bridge";
		}
		return "";
	}

	private static bool CheckRoadAxis(string roadType, int sideA, int sideB)
	{
		int num = Mathf.Min(sideA, sideB);
		int num2 = Mathf.Max(sideA, sideB);
		switch (roadType)
		{
		case "1":
			if (num == 0)
			{
				return num2 == 3;
			}
			return false;
		case "2":
			if (num == 1)
			{
				return num2 == 5;
			}
			return false;
		case "3":
			if (num == 2)
			{
				return num2 == 4;
			}
			return false;
		case "4":
			if (num == 1)
			{
				return num2 == 4;
			}
			return false;
		case "5":
			if (num == 2)
			{
				return num2 == 5;
			}
			return false;
		default:
			return false;
		}
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

	private static string GetRiverSpriteName(int sideA, int sideB)
	{
		if (sideA == -1 || sideB == -1)
		{
			switch ((sideA != -1) ? sideA : sideB)
			{
			case 3:
				return "EndRiver1";
			case 2:
				return "EndRiver2";
			case 1:
				return "EndRiver3";
			case 0:
				return "EndRiver4";
			case 5:
				return "EndRiver5";
			case 4:
				return "EndRiver6";
			default:
				return "EndRiver1";
			}
		}
		int num = Mathf.Min(sideA, sideB);
		int num2 = Mathf.Max(sideA, sideB);
		switch (num + "-" + num2)
		{
		case "0-3":
			return "River1";
		case "1-4":
			return "River2";
		case "2-5":
			return "River3";
		case "1-5":
			return "River4";
		case "2-4":
			return "River5";
		case "0-4":
			return "River6";
		case "0-2":
			return "River7";
		case "1-3":
			return "River8";
		case "3-5":
			return "River9";
		default:
			return "Lake";
		}
	}

	private static string GetRailroadAxis(string tileName)
	{
		if (!tileName.StartsWith("Railroad") || tileName.StartsWith("RailroadCross"))
		{
			return "";
		}
		switch (tileName.Replace("Railroad", ""))
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
			return "0-2";
		case "7":
			return "1-3";
		case "8":
			return "3-5";
		case "9":
			return "0-4";
		default:
			return "";
		}
	}

	private static string GetRailroadBridgeSpriteName(int riverIn, int riverOut, Tile railroadTile)
	{
		string railroadAxis = GetRailroadAxis(railroadTile.Name);
		if (railroadAxis == "")
		{
			return "";
		}
		int num = Mathf.Min(riverIn, riverOut);
		int num2 = Mathf.Max(riverIn, riverOut);
		string text = num + "-" + num2;
		if (text == "1-4" && railroadAxis == "0-3")
		{
			return "River1BridgeRailroad";
		}
		if (text == "1-4" && railroadAxis == "2-5")
		{
			return "River2BridgeRailroad";
		}
		if (text == "2-5" && railroadAxis == "1-4")
		{
			return "River3BridgeRailroad";
		}
		if (text == "1-5" && railroadAxis == "0-3")
		{
			return "River4BridgeRailroad";
		}
		if (text == "2-4" && railroadAxis == "0-3")
		{
			return "River5BridgeRailroad";
		}
		if (text == "0-3" && railroadAxis == "1-4")
		{
			return "River6BridgeRailroad";
		}
		if (text == "0-3" && railroadAxis == "2-5")
		{
			return "River7BridgeRailroad";
		}
		if (text == "3-5" && railroadAxis == "2-5")
		{
			return "River8BridgeRailroad";
		}
		if (text == "0-2" && railroadAxis == "1-4")
		{
			return "River9BridgeRailroad";
		}
		if (text == "1-3" && railroadAxis == "2-5")
		{
			return "River10BridgeRailroad";
		}
		if (text == "3-5" && railroadAxis == "1-4")
		{
			return "River11BridgeRailroad";
		}
		if (text == "2-5" && railroadAxis == "0-3")
		{
			return "River12BridgeRailroad";
		}
		if (text == "0-3" && railroadAxis == "2-4")
		{
			return "River13BridgeRailroad";
		}
		if (text == "0-3" && railroadAxis == "1-5")
		{
			return "River14BridgeRailroad";
		}
		return "";
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRegionStage : IRuntimeScenarioGenerationStage
{
	public virtual void Execute(ScenarioGenerationContext p_context)
	{
		SelectAnchors(p_context);
		AssignRegions(p_context);
	}

	protected virtual void SelectAnchors(ScenarioGenerationContext p_context)
	{
		List<Tile> list = new List<Tile>();
		float num = (float)(p_context.Map.SizeX - 1) * 0.35f;
		float num2 = (float)(p_context.Map.SizeY - 1) * 0.35f;
		foreach (ScenarioPlayerRegion playerRegion in p_context.PlayerRegions)
		{
			float f = MathF.PI * 2f * (float)playerRegion.Index / (float)p_context.PlayerRegions.Count + (p_context.NextFloat() - 0.5f) * 0.35f;
			Vector2 p_target = new Vector2(p_context.Center.x + Mathf.Cos(f) * num, p_context.Center.y + Mathf.Sin(f) * num2);
			Tile tile = FindBestAnchor(p_context, p_target, list);
			if (tile == null)
			{
				tile = FindFallbackAnchor(p_context, list);
			}
			playerRegion.AnchorTile = tile;
			if (tile != null)
			{
				list.Add(tile);
			}
		}
	}

	protected virtual Tile FindBestAnchor(ScenarioGenerationContext p_context, Vector2 p_target, List<Tile> p_existingAnchors)
	{
		Tile result = null;
		float num = float.MinValue;
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (IsAnchorCandidate(p_context, tile, p_existingAnchors))
			{
				float num2 = 0f;
				num2 -= Vector2.Distance(p_target, new Vector2(tile.PosX, tile.PosY)) * 3f;
				num2 += (float)CountOpenLand(tile, 2) * 4f;
				num2 += (float)p_context.GetDistanceToMapBorder(tile);
				num2 -= (float)CountTypeAround(tile, 2, "Mountain") * 4f;
				num2 -= (float)CountTypeAround(tile, 2, "Marsh") * 2.5f;
				num2 += p_context.NextFloat();
				if (num2 > num)
				{
					num = num2;
					result = tile;
				}
			}
		}
		return result;
	}

	protected virtual Tile FindFallbackAnchor(ScenarioGenerationContext p_context, List<Tile> p_existingAnchors)
	{
		Tile result = null;
		float num = float.MinValue;
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (!IsLandAnchorTile(tile))
			{
				continue;
			}
			float num2 = 99f;
			foreach (Tile p_existingAnchor in p_existingAnchors)
			{
				num2 = Mathf.Min(num2, Utils.GetHeuristicDistance(tile, p_existingAnchor));
			}
			if (p_existingAnchors.Count == 0)
			{
				num2 = 10f;
			}
			float num3 = num2 * 4f + (float)CountOpenLand(tile, 2);
			num3 += p_context.NextFloat();
			if (num3 > num)
			{
				num = num3;
				result = tile;
			}
		}
		return result;
	}

	protected virtual bool IsAnchorCandidate(ScenarioGenerationContext p_context, Tile p_tile, List<Tile> p_existingAnchors)
	{
		if (!IsLandAnchorTile(p_tile))
		{
			return false;
		}
		if (p_context.GetDistanceToMapBorder(p_tile) < 1)
		{
			return false;
		}
		if (CountOpenLand(p_tile, 2) < 6)
		{
			return false;
		}
		int num = Mathf.Max(5, Mathf.RoundToInt((float)Mathf.Min(p_context.Map.SizeX, p_context.Map.SizeY) / ((p_context.PlayerRegions.Count > 4) ? 2.5f : 2f)));
		foreach (Tile p_existingAnchor in p_existingAnchors)
		{
			if (Utils.GetHeuristicDistance(p_existingAnchor, p_tile) < num)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool IsLandAnchorTile(Tile p_tile)
	{
		if (p_tile.Type == "Sea")
		{
			return false;
		}
		if (p_tile.Type == "Mountain" || p_tile.Type == "Marsh")
		{
			return false;
		}
		return true;
	}

	protected virtual void AssignRegions(ScenarioGenerationContext p_context)
	{
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (tile.Type == "Sea")
			{
				p_context.SetRegionIndex(tile, -1);
				continue;
			}
			int p_regionIndex = -1;
			float num = float.MaxValue;
			foreach (ScenarioPlayerRegion playerRegion in p_context.PlayerRegions)
			{
				if (playerRegion.AnchorTile != null)
				{
					float num2 = (float)Utils.GetHeuristicDistance(tile, playerRegion.AnchorTile) + GetTerrainPenalty(tile);
					if (num2 < num)
					{
						num = num2;
						p_regionIndex = playerRegion.Index;
					}
				}
			}
			p_context.SetRegionIndex(tile, p_regionIndex);
		}
		foreach (ScenarioPlayerRegion playerRegion2 in p_context.PlayerRegions)
		{
			if (playerRegion2.AnchorTile != null)
			{
				p_context.SetRegionIndex(playerRegion2.AnchorTile, playerRegion2.Index);
			}
		}
	}

	protected virtual float GetTerrainPenalty(Tile p_tile)
	{
		float num = 0f;
		if (p_tile.Type == "Hill")
		{
			num += 0.5f;
		}
		else if (p_tile.Type == "Mountain")
		{
			num += 1.5f;
		}
		else if (p_tile.Type == "Marsh")
		{
			num += 0.9f;
		}
		else if (p_tile.Type == "ThickForest")
		{
			num += 0.4f;
		}
		else if (p_tile.Type == "ThinForest" || p_tile.Type == "Bocage")
		{
			num += 0.2f;
		}
		return num;
	}

	protected virtual int CountOpenLand(Tile p_tile, int p_radius)
	{
		int num = 0;
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			if (item.Type != "Sea" && item.Type != "Mountain" && item.Type != "Marsh")
			{
				num++;
			}
		}
		return num;
	}

	protected virtual int CountTypeAround(Tile p_tile, int p_radius, string p_type)
	{
		int num = 0;
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			if (item.Type == p_type)
			{
				num++;
			}
		}
		return num;
	}
}

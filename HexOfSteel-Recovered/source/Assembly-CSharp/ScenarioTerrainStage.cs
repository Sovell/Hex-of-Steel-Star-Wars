using System.Collections.Generic;
using UnityEngine;

public class ScenarioTerrainStage : IRuntimeScenarioGenerationStage
{
	protected enum CoastlineBorderSide
	{
		Left,
		Right,
		Bottom,
		Top
	}

	public virtual void Execute(ScenarioGenerationContext p_context)
	{
		SeedBaseFields(p_context);
		CreateMountainRidges(p_context);
		CreateHillMassifs(p_context);
		SmoothField(p_context, p_context.Elevation, 2);
		ApplyWater(p_context);
		SpreadMoistureFromWater(p_context);
		CreateForestMoistureClusters(p_context);
		SmoothField(p_context, p_context.Moisture, 2);
		ApplyElevationTypes(p_context);
		ApplyBiomeCover(p_context);
	}

	protected virtual void SeedBaseFields(ScenarioGenerationContext p_context)
	{
		float num = (p_context.Request.GenerateSeas ? Mathf.Lerp(0.1f, 0.35f, (float)p_context.Request.WaterAmount / 100f) : 0f);
		for (int i = 0; i < p_context.Map.SizeX; i++)
		{
			for (int j = 0; j < p_context.Map.SizeY; j++)
			{
				Tile p_tile = p_context.Tiles[i, j];
				float num2 = 0f;
				if (p_context.Request.GenerateSeas)
				{
					num2 = GetInitialSeaPressure(p_context, p_tile);
				}
				float num3 = SampleNoise(p_context, i, j, 0.055f, 17.3f);
				float num4 = SampleNoise(p_context, i, j, 0.13f, 71.9f);
				float num5 = SampleNoise(p_context, i, j, 0.06f, 113.7f);
				float num6 = SampleNoise(p_context, i, j, 0.18f, 221.2f);
				p_context.Elevation[i, j] = Mathf.Clamp01(0.24f + num3 * 0.24f + num4 * 0.08f + p_context.NextFloat() * 0.05f - num2 * num);
				p_context.Moisture[i, j] = Mathf.Clamp01(0.12f + num5 * 0.32f + num6 * 0.12f + p_context.NextFloat() * 0.04f);
			}
		}
	}

	protected virtual float SampleNoise(ScenarioGenerationContext p_context, int p_x, int p_y, float p_scale, float p_offset)
	{
		return SampleSeedNoise(p_context.Request.RuntimeSeed, p_x, p_y, p_scale, p_offset);
	}

	protected virtual float SampleSeedNoise(int p_runtimeSeed, int p_x, int p_y, float p_scale, float p_offset)
	{
		float num = (float)(uint)(p_runtimeSeed & 0xFFFF) * 0.001f + p_offset;
		float num2 = (float)(((uint)p_runtimeSeed >> 16) & 0xFFFF) * 0.001f + p_offset * 1.618f;
		return Mathf.PerlinNoise(((float)p_x + num) * p_scale, ((float)p_y + num2) * p_scale);
	}

	protected virtual void CreateMountainRidges(ScenarioGenerationContext p_context)
	{
		if (p_context.Request.MountainAmount <= 0)
		{
			return;
		}
		float num = Mathf.Clamp01((float)p_context.Request.MountainAmount / 100f);
		int num2 = Mathf.Clamp(1 + Mathf.RoundToInt((float)p_context.Request.MountainAmount / 28f) + Mathf.RoundToInt((float)(p_context.Map.SizeX + p_context.Map.SizeY) / 90f * num), 1, 6);
		float num3 = Mathf.Lerp(0.08f, 0.22f, num);
		float num4 = Mathf.Lerp(0.08f, 0.2f, num);
		for (int i = 0; i < num2; i++)
		{
			Tile tile = GetRandomInteriorTile(p_context, 3);
			Tile randomInteriorTile = GetRandomInteriorTile(p_context, 3);
			if (tile == null || randomInteriorTile == null)
			{
				continue;
			}
			int num5 = Mathf.Clamp(Utils.GetHeuristicDistance(tile, randomInteriorTile) + p_context.NextInt(3, 8), 6, Mathf.Max(p_context.Map.SizeX, p_context.Map.SizeY));
			Tile p_previousTile = null;
			for (int j = 0; j < num5; j++)
			{
				RaiseElevation(p_context, tile, 0.42f, 0);
				RaiseElevation(p_context, tile, 0.12f, 1);
				if (j % 3 == 0 || j == num5 - 1)
				{
					RaiseElevation(p_context, tile, 0.03f, 2);
				}
				if ((tile == randomInteriorTile && j > num5 / 3) || (j > 1 && j < num5 - 2 && p_context.NextFloat() < num3))
				{
					randomInteriorTile = GetRandomInteriorTile(p_context, 3);
				}
				Tile tile2 = ChooseNextRidgeTile(p_context, tile, p_previousTile, randomInteriorTile);
				if (tile2 == null)
				{
					break;
				}
				if (j > 1 && p_context.NextFloat() < num4)
				{
					Tile tile3 = ChooseRidgeShoulderTile(p_context, tile, p_previousTile, tile2);
					if (tile3 != null)
					{
						RaiseElevation(p_context, tile3, 0.1f, 0);
					}
				}
				p_previousTile = tile;
				tile = tile2;
			}
		}
	}

	protected virtual void CreateHillMassifs(ScenarioGenerationContext p_context)
	{
		if (p_context.Request.HillsAmount <= 0)
		{
			return;
		}
		int num = Mathf.Clamp(1 + Mathf.CeilToInt((float)p_context.Request.HillsAmount / 22f) + Mathf.RoundToInt((float)(p_context.Map.SizeX + p_context.Map.SizeY) / 100f), 1, 7);
		for (int i = 0; i < num; i++)
		{
			Tile tile = FindHighlandSeed(p_context);
			if (tile == null)
			{
				continue;
			}
			int num2 = 2 + p_context.NextInt(0, 3);
			for (int j = 0; j < num2; j++)
			{
				Tile p_tile = ShiftSeedAcrossOpenLand(p_context, tile, 1 + p_context.NextInt(0, 3));
				RaiseElevation(p_context, p_tile, 0.18f, 0);
				RaiseElevation(p_context, p_tile, 0.12f, 1);
				RaiseElevation(p_context, p_tile, 0.06f, 2);
				if (p_context.Request.HillsAmount >= 45)
				{
					RaiseElevation(p_context, p_tile, 0.03f, 3);
				}
			}
		}
	}

	protected virtual Tile FindHighlandSeed(ScenarioGenerationContext p_context)
	{
		Tile result = null;
		float num = float.MinValue;
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (!(tile.Type == "Sea") && p_context.GetDistanceToMapBorder(tile) > 1 && CountTypeAround(tile, "Mountain", 2) <= 0)
			{
				float num2 = p_context.Elevation[tile.PosX, tile.PosY];
				float num3 = 1f - Mathf.Abs(num2 - 0.44f);
				num3 += (float)CountOpenLand(tile, 2) * 0.08f;
				num3 -= (float)CountWaterNeighbours(tile) * 0.12f;
				num3 += p_context.NextFloat() * 0.15f;
				if (num3 > num)
				{
					num = num3;
					result = tile;
				}
			}
		}
		return result;
	}

	protected virtual Tile ShiftSeedAcrossOpenLand(ScenarioGenerationContext p_context, Tile p_seedTile, int p_steps)
	{
		Tile tile = p_seedTile;
		for (int i = 0; i < p_steps; i++)
		{
			Tile tile2 = tile;
			float num = float.MinValue;
			foreach (Tile neighbour in tile.neighbours)
			{
				if (!(neighbour.Type == "Sea") && !(neighbour.Type == "Mountain"))
				{
					float num2 = (float)CountOpenLand(neighbour, 1) - (float)CountWaterNeighbours(neighbour) * 0.5f + p_context.NextFloat();
					if (num2 > num)
					{
						num = num2;
						tile2 = neighbour;
					}
				}
			}
			tile = tile2;
		}
		return tile;
	}

	protected virtual Tile GetRandomInteriorTile(ScenarioGenerationContext p_context, int p_margin)
	{
		List<Tile> list = new List<Tile>();
		for (int i = p_margin; i < p_context.Map.SizeX - p_margin; i++)
		{
			for (int j = p_margin; j < p_context.Map.SizeY - p_margin; j++)
			{
				list.Add(p_context.Tiles[i, j]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[p_context.NextInt(0, list.Count)];
	}

	protected virtual Tile ChooseNextRidgeTile(ScenarioGenerationContext p_context, Tile p_currentTile, Tile p_previousTile, Tile p_targetTile)
	{
		List<(Tile, float)> list = new List<(Tile, float)>();
		bool flag = p_previousTile != null;
		Vector2 lhs = Vector2.zero;
		if (flag)
		{
			lhs = new Vector2(p_currentTile.PosX - p_previousTile.PosX, p_currentTile.PosY - p_previousTile.PosY).normalized;
		}
		foreach (Tile neighbour in p_currentTile.neighbours)
		{
			if (neighbour != p_previousTile || p_currentTile.neighbours.Count <= 1)
			{
				float num = (float)Utils.GetHeuristicDistance(neighbour, p_targetTile) * 0.72f;
				num -= p_context.Elevation[neighbour.PosX, neighbour.PosY] * 1.4f;
				num -= SampleNoise(p_context, neighbour.PosX, neighbour.PosY, 0.07f, 641.3f) * 0.9f;
				num -= SampleNoise(p_context, neighbour.PosX, neighbour.PosY, 0.16f, 883.7f) * 0.35f;
				num += p_context.NextFloat() * 0.9f;
				if (p_context.GetDistanceToMapBorder(neighbour) <= 1)
				{
					num += 1.5f;
				}
				else if (p_context.GetDistanceToMapBorder(neighbour) == 2)
				{
					num += 0.4f;
				}
				if (flag)
				{
					Vector2 normalized = new Vector2(neighbour.PosX - p_currentTile.PosX, neighbour.PosY - p_currentTile.PosY).normalized;
					float num2 = Vector2.Dot(lhs, normalized);
					num += Mathf.Max(0f, num2 - 0.15f) * 1.6f;
					num += Mathf.Max(0f, 0f - num2) * 0.4f;
				}
				list.Add((neighbour, num));
			}
		}
		if (list.Count == 0)
		{
			return p_previousTile;
		}
		list.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => a.score.CompareTo(b.score));
		int p_maxExclusive = Mathf.Min(4, list.Count);
		return list[p_context.NextInt(0, p_maxExclusive)].Item1;
	}

	protected virtual Tile ChooseRidgeShoulderTile(ScenarioGenerationContext p_context, Tile p_currentTile, Tile p_previousTile, Tile p_nextTile)
	{
		List<(Tile, float)> list = new List<(Tile, float)>();
		Vector2 zero = Vector2.zero;
		if (p_nextTile != null)
		{
			zero += new Vector2(p_nextTile.PosX - p_currentTile.PosX, p_nextTile.PosY - p_currentTile.PosY);
		}
		if (p_previousTile != null)
		{
			zero += new Vector2(p_currentTile.PosX - p_previousTile.PosX, p_currentTile.PosY - p_previousTile.PosY);
		}
		bool flag = zero.sqrMagnitude > 0.001f;
		if (flag)
		{
			zero.Normalize();
		}
		foreach (Tile neighbour in p_currentTile.neighbours)
		{
			if (neighbour != p_previousTile && neighbour != p_nextTile)
			{
				float num = (0f - p_context.Elevation[neighbour.PosX, neighbour.PosY]) * 0.9f;
				num += SampleNoise(p_context, neighbour.PosX, neighbour.PosY, 0.1f, 1193.4f) * 0.35f;
				num += (float)p_context.GetDistanceToMapBorder(neighbour) * 0.08f;
				num += p_context.NextFloat() * 0.4f;
				if (flag)
				{
					Vector2 normalized = new Vector2(neighbour.PosX - p_currentTile.PosX, neighbour.PosY - p_currentTile.PosY).normalized;
					float num2 = Mathf.Abs(Vector2.Dot(zero, normalized));
					num += (1f - num2) * 1.6f;
				}
				list.Add((neighbour, num));
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => b.score.CompareTo(a.score));
		int p_maxExclusive = Mathf.Min(2, list.Count);
		return list[p_context.NextInt(0, p_maxExclusive)].Item1;
	}

	protected virtual void RaiseElevation(ScenarioGenerationContext p_context, Tile p_tile, float p_amount, int p_radius)
	{
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			p_context.Elevation[item.PosX, item.PosY] = Mathf.Clamp01(p_context.Elevation[item.PosX, item.PosY] + p_amount);
		}
	}

	protected virtual void SmoothField(ScenarioGenerationContext p_context, float[,] p_field, int p_iterations)
	{
		for (int i = 0; i < p_iterations; i++)
		{
			float[,] array = new float[p_context.Map.SizeX, p_context.Map.SizeY];
			for (int j = 0; j < p_context.Map.SizeX; j++)
			{
				for (int k = 0; k < p_context.Map.SizeY; k++)
				{
					Tile tile = p_context.Tiles[j, k];
					float num = p_field[j, k] * 2f;
					int num2 = 2;
					foreach (Tile neighbour in tile.neighbours)
					{
						num += p_field[neighbour.PosX, neighbour.PosY];
						num2++;
					}
					array[j, k] = num / (float)num2;
				}
			}
			for (int l = 0; l < p_context.Map.SizeX; l++)
			{
				for (int m = 0; m < p_context.Map.SizeY; m++)
				{
					p_field[l, m] = array[l, m];
				}
			}
		}
	}

	protected virtual void ApplyWater(ScenarioGenerationContext p_context)
	{
		if (p_context.Request.GenerateSeas)
		{
			CreateCoastlines(p_context);
		}
		if (p_context.Request.WaterAmount > 0)
		{
			CreateLakes(p_context);
		}
	}

	protected virtual void CreateCoastlines(ScenarioGenerationContext p_context)
	{
		int num = p_context.Map.SizeX * p_context.Map.SizeY;
		int p_coastDepth = Mathf.Clamp(2 + Mathf.RoundToInt((float)p_context.Request.WaterAmount / 16f) + Mathf.RoundToInt((float)(p_context.Map.SizeX + p_context.Map.SizeY) / 120f), 2, 7);
		float p_coastlineThreshold = Mathf.Lerp(0.89f, 0.64f, (float)p_context.Request.WaterAmount / 100f);
		for (int i = 0; i < p_context.Map.SizeX; i++)
		{
			for (int j = 0; j < p_context.Map.SizeY; j++)
			{
				Tile p_tile = p_context.Tiles[i, j];
				if (ShouldCarveCoastTile(p_context, p_tile, p_coastDepth, p_coastlineThreshold))
				{
					MarkWaterTile(p_tile);
				}
			}
		}
		int num2 = Mathf.Max(4, Mathf.Clamp(2 + Mathf.CeilToInt((float)p_context.Request.WaterAmount / 14f) + Mathf.RoundToInt((float)num / 1200f), 2, 10));
		int num3 = CreateMandatorySideInlets(p_context, p_coastDepth);
		int num4 = Mathf.Max(0, num2 - num3);
		for (int k = 0; k < num4; k++)
		{
			Tile tile = FindBestSeaSeed(p_context);
			if (tile != null)
			{
				Tile tile2 = FindBestSeaInletTarget(p_context, tile, p_coastDepth);
				if (tile2 != null)
				{
					int p_maxSteps = Mathf.Clamp(Utils.GetHeuristicDistance(tile, tile2) + p_context.NextInt(2, 6), 6, Mathf.Max(p_context.Map.SizeX, p_context.Map.SizeY) / 2);
					CarveSeaInlet(p_context, tile, tile2, p_maxSteps);
				}
			}
		}
	}

	protected virtual float GetInitialSeaPressure(ScenarioGenerationContext p_context, Tile p_tile)
	{
		int p_coastDepth = Mathf.Clamp(2 + Mathf.RoundToInt((float)p_context.Request.WaterAmount / 22f), 2, 6);
		return GetBorderSeaPressure(p_context, p_tile, p_coastDepth);
	}

	protected virtual bool ShouldCarveCoastTile(ScenarioGenerationContext p_context, Tile p_tile, int p_coastDepth, float p_coastlineThreshold)
	{
		if (p_context.GetDistanceToMapBorder(p_tile) == 0)
		{
			return true;
		}
		float t = Mathf.Clamp01((float)p_context.Request.WaterAmount / 100f);
		float num = p_context.Elevation[p_tile.PosX, p_tile.PosY];
		float borderSeaPressure = GetBorderSeaPressure(p_context, p_tile, p_coastDepth);
		if (borderSeaPressure <= 0f)
		{
			return false;
		}
		float num2 = SampleNoise(p_context, p_tile.PosX, p_tile.PosY, 0.045f, 151.2f);
		float num3 = SampleNoise(p_context, p_tile.PosX, p_tile.PosY, 0.14f, 287.8f);
		float num4 = SampleNoise(p_context, p_tile.PosX, p_tile.PosY, 0.09f, 416.3f);
		float num5 = SampleNoise(p_context, p_tile.PosX, p_tile.PosY, 0.19f, 1679.6f);
		float num6 = borderSeaPressure * 1.28f + num2 * 0.18f + num3 * 0.16f + num4 * 0.13f + num5 * 0.1f - num * Mathf.Lerp(0.6f, 0.35f, t);
		float num7 = p_coastlineThreshold - borderSeaPressure * 0.14f;
		return num6 > num7;
	}

	protected virtual IEnumerable<CoastlineBorderSide> GetBorderSides()
	{
		yield return CoastlineBorderSide.Left;
		yield return CoastlineBorderSide.Right;
		yield return CoastlineBorderSide.Bottom;
		yield return CoastlineBorderSide.Top;
	}

	protected virtual float GetBorderSeaPressure(ScenarioGenerationContext p_context, Tile p_tile, int p_coastDepth)
	{
		float a = 0f;
		foreach (CoastlineBorderSide borderSide in GetBorderSides())
		{
			a = Mathf.Max(a, GetSingleBorderSeaPressure(p_context, p_tile, p_coastDepth, borderSide));
		}
		a = Mathf.Max(a, GetCornerSeaPressure(p_context, p_tile, p_coastDepth));
		return Mathf.Clamp01(a);
	}

	protected virtual float GetSingleBorderSeaPressure(ScenarioGenerationContext p_context, Tile p_tile, int p_coastDepth, CoastlineBorderSide p_borderSide)
	{
		int distanceFromBorderSide = GetDistanceFromBorderSide(p_context, p_tile, p_borderSide);
		float coastlineDepthForSide = GetCoastlineDepthForSide(p_context, p_tile, p_borderSide, p_coastDepth);
		float num = (GetSideContourNoise(p_context, p_tile, p_borderSide) - 0.5f) * 1.4f + (SampleNoise(p_context, p_tile.PosX, p_tile.PosY, 0.11f, 2141.7f + (float)p_borderSide * 47.3f) - 0.5f) * 0.9f;
		float num2 = coastlineDepthForSide + num;
		if ((float)distanceFromBorderSide > num2 + 1.5f)
		{
			return 0f;
		}
		return Mathf.Clamp01(1f - (float)distanceFromBorderSide / (num2 + 0.75f));
	}

	protected virtual float GetCornerSeaPressure(ScenarioGenerationContext p_context, Tile p_tile, int p_coastDepth)
	{
		return Mathf.Max(Mathf.Max(Mathf.Max(Mathf.Max(0f, GetSingleCornerSeaPressure(p_context, p_tile, p_coastDepth, 0, 0, 0)), GetSingleCornerSeaPressure(p_context, p_tile, p_coastDepth, 1, p_context.Map.SizeX - 1, 0)), GetSingleCornerSeaPressure(p_context, p_tile, p_coastDepth, 2, 0, p_context.Map.SizeY - 1)), GetSingleCornerSeaPressure(p_context, p_tile, p_coastDepth, 3, p_context.Map.SizeX - 1, p_context.Map.SizeY - 1));
	}

	protected virtual float GetSingleCornerSeaPressure(ScenarioGenerationContext p_context, Tile p_tile, int p_coastDepth, int p_cornerIndex, int p_cornerX, int p_cornerY)
	{
		float cornerSeaRadius = GetCornerSeaRadius(p_context, p_coastDepth, p_cornerIndex);
		float num = Vector2.Distance(new Vector2(p_tile.PosX, p_tile.PosY), new Vector2(p_cornerX, p_cornerY));
		if (num > cornerSeaRadius + 1.2f)
		{
			return 0f;
		}
		return Mathf.Clamp01(1f - num / (cornerSeaRadius + 0.6f));
	}

	protected virtual float GetCornerSeaRadius(ScenarioGenerationContext p_context, int p_coastDepth, int p_cornerIndex)
	{
		float num = SampleSeedNoise(p_context.Request.RuntimeSeed, 27 + p_cornerIndex * 31, 51 + p_cornerIndex * 43, 0.16f, 2311.4f);
		float f = SampleSeedNoise(p_context.Request.RuntimeSeed, 73 + p_cornerIndex * 17, 97 + p_cornerIndex * 29, 0.19f, 2459.8f);
		return Mathf.Clamp(1.4f + num * (0.9f + (float)p_coastDepth * 0.4f) + Mathf.Pow(f, 2f) * 1.1f, 1.4f, (float)p_coastDepth + 2.3f);
	}

	protected virtual float GetCoastlineDepthForSide(ScenarioGenerationContext p_context, Tile p_tile, CoastlineBorderSide p_borderSide, int p_coastDepth)
	{
		float num = Mathf.Clamp01((float)p_context.Request.WaterAmount / 100f);
		int p_x = ((p_borderSide == CoastlineBorderSide.Left || p_borderSide == CoastlineBorderSide.Right) ? p_tile.PosY : p_tile.PosX);
		float num2 = SampleSeedNoise(p_context.Request.RuntimeSeed, p_x, 19 + (int)p_borderSide * 23, 0.026f, 1717.4f + (float)p_borderSide * 37.1f);
		float num3 = SampleSeedNoise(p_context.Request.RuntimeSeed, p_x, 61 + (int)p_borderSide * 29, 0.073f, 1839.2f + (float)p_borderSide * 41.7f);
		float num4 = Mathf.Pow(SampleSeedNoise(p_context.Request.RuntimeSeed, p_x, 97 + (int)p_borderSide * 19, 0.049f, 1961.8f + (float)p_borderSide * 51.9f), 1.85f);
		float num5 = Mathf.Pow(SampleSeedNoise(p_context.Request.RuntimeSeed, p_x, 143 + (int)p_borderSide * 13, 0.16f, 2083.4f + (float)p_borderSide * 44.6f), 2.25f);
		return Mathf.Clamp(Mathf.Lerp(1.05f, 1.85f + (float)p_coastDepth * 0.28f, num) + num2 * (0.95f + (float)p_coastDepth * 0.34f) + num3 * 0.85f + num4 * (0.9f + num * 1.5f) + num5 * 0.9f, 1.1f, (float)p_coastDepth + 2.8f);
	}

	protected virtual float GetSideContourNoise(ScenarioGenerationContext p_context, Tile p_tile, CoastlineBorderSide p_borderSide)
	{
		int p_x = ((p_borderSide == CoastlineBorderSide.Left || p_borderSide == CoastlineBorderSide.Right) ? p_tile.PosY : p_tile.PosX);
		int distanceFromBorderSide = GetDistanceFromBorderSide(p_context, p_tile, p_borderSide);
		return SampleSeedNoise(p_context.Request.RuntimeSeed, p_x, distanceFromBorderSide + (int)(p_borderSide + 1) * 31, 0.12f, 1943.8f + (float)p_borderSide * 53.2f);
	}

	protected virtual int CreateMandatorySideInlets(ScenarioGenerationContext p_context, int p_coastDepth)
	{
		List<CoastlineBorderSide> list = new List<CoastlineBorderSide>
		{
			CoastlineBorderSide.Left,
			CoastlineBorderSide.Right,
			CoastlineBorderSide.Bottom,
			CoastlineBorderSide.Top
		};
		ShuffleBorderSides(p_context, list);
		int num = 0;
		foreach (CoastlineBorderSide item in list)
		{
			Tile tile = FindBestSeaSeedForSide(p_context, item);
			if (tile != null)
			{
				Tile tile2 = FindBestSeaInletTargetForSide(p_context, tile, p_coastDepth, item);
				if (tile2 != null)
				{
					int p_maxSteps = Mathf.Clamp(Utils.GetHeuristicDistance(tile, tile2) + p_context.NextInt(1, 4), 5, Mathf.Max(6, Mathf.Max(p_context.Map.SizeX, p_context.Map.SizeY) / 2));
					CarveSeaInlet(p_context, tile, tile2, p_maxSteps);
					num++;
				}
			}
		}
		return num;
	}

	protected virtual void ShuffleBorderSides(ScenarioGenerationContext p_context, List<CoastlineBorderSide> p_borderSides)
	{
		for (int num = p_borderSides.Count - 1; num > 0; num--)
		{
			int index = p_context.NextInt(0, num + 1);
			CoastlineBorderSide value = p_borderSides[num];
			p_borderSides[num] = p_borderSides[index];
			p_borderSides[index] = value;
		}
	}

	protected virtual Tile FindBestSeaSeedForSide(ScenarioGenerationContext p_context, CoastlineBorderSide p_borderSide)
	{
		Tile result = null;
		float num = float.MinValue;
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (!IsTileOnBorderSide(p_context, tile, p_borderSide, 1))
			{
				continue;
			}
			int distanceToPerpendicularBorder = GetDistanceToPerpendicularBorder(p_context, tile, p_borderSide);
			if (distanceToPerpendicularBorder <= 0)
			{
				continue;
			}
			bool flag = IsWaterTile(tile);
			if ((flag || !(tile.Type == "Mountain")) && (!flag || CountOpenLand(tile, 1) > 1))
			{
				float num2 = (1f - p_context.Elevation[tile.PosX, tile.PosY]) * (flag ? 0.42f : 0.72f);
				num2 += (float)CountOpenLand(tile, 2) * 0.03f;
				num2 += SampleNoise(p_context, tile.PosX, tile.PosY, 0.08f, 509.3f) * 0.22f;
				num2 += (float)Mathf.Min(4, distanceToPerpendicularBorder) * 0.06f;
				num2 += (flag ? 0.08f : 0f);
				num2 += p_context.NextFloat() * 0.1f;
				if (num2 > num)
				{
					num = num2;
					result = tile;
				}
			}
		}
		return result;
	}

	protected virtual Tile FindBestSeaInletTargetForSide(ScenarioGenerationContext p_context, Tile p_seedTile, int p_coastDepth, CoastlineBorderSide p_borderSide)
	{
		Tile result = null;
		float num = float.MinValue;
		int num2 = p_coastDepth + 1;
		int num3 = Mathf.Max(num2 + 3, Mathf.Min(p_context.Map.SizeX, p_context.Map.SizeY) / 2);
		int num4 = Mathf.Clamp(num2 + 3 + p_context.NextInt(0, 4), num2 + 2, num3);
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (IsWaterTile(tile) || tile.Type == "Mountain")
			{
				continue;
			}
			int distanceFromBorderSide = GetDistanceFromBorderSide(p_context, tile, p_borderSide);
			if (distanceFromBorderSide <= num2)
			{
				continue;
			}
			int heuristicDistance = Utils.GetHeuristicDistance(p_seedTile, tile);
			if (heuristicDistance >= 4 && heuristicDistance <= num3)
			{
				int lateralOffsetFromBorderSide = GetLateralOffsetFromBorderSide(tile, p_seedTile, p_borderSide);
				float num5 = (1f - p_context.Elevation[tile.PosX, tile.PosY]) * 0.5f;
				num5 += SampleNoise(p_context, tile.PosX, tile.PosY, 0.06f, 587.1f) * 0.28f;
				num5 += SampleNoise(p_context, tile.PosX, tile.PosY, 0.15f, 808.4f) * 0.16f;
				num5 -= (float)Mathf.Abs(heuristicDistance - num4) * 0.08f;
				num5 -= (float)Mathf.Abs(distanceFromBorderSide - num4) * 0.06f;
				num5 -= (float)lateralOffsetFromBorderSide * 0.08f;
				num5 -= (float)CountTypeAround(tile, "Mountain", 2) * 0.14f;
				num5 += (float)CountOpenLand(tile, 1) * 0.03f;
				num5 += p_context.NextFloat() * 0.06f;
				if (num5 > num)
				{
					num = num5;
					result = tile;
				}
			}
		}
		return result;
	}

	protected virtual bool IsTileOnBorderSide(ScenarioGenerationContext p_context, Tile p_tile, CoastlineBorderSide p_borderSide, int p_maxDistance)
	{
		return GetDistanceFromBorderSide(p_context, p_tile, p_borderSide) <= p_maxDistance;
	}

	protected virtual int GetDistanceFromBorderSide(ScenarioGenerationContext p_context, Tile p_tile, CoastlineBorderSide p_borderSide)
	{
		switch (p_borderSide)
		{
		case CoastlineBorderSide.Left:
			return p_tile.PosX;
		case CoastlineBorderSide.Right:
			return p_context.Map.SizeX - 1 - p_tile.PosX;
		case CoastlineBorderSide.Bottom:
			return p_tile.PosY;
		case CoastlineBorderSide.Top:
			return p_context.Map.SizeY - 1 - p_tile.PosY;
		default:
			return 0;
		}
	}

	protected virtual int GetDistanceToPerpendicularBorder(ScenarioGenerationContext p_context, Tile p_tile, CoastlineBorderSide p_borderSide)
	{
		switch (p_borderSide)
		{
		case CoastlineBorderSide.Left:
		case CoastlineBorderSide.Right:
			return Mathf.Min(p_tile.PosY, p_context.Map.SizeY - 1 - p_tile.PosY);
		case CoastlineBorderSide.Bottom:
		case CoastlineBorderSide.Top:
			return Mathf.Min(p_tile.PosX, p_context.Map.SizeX - 1 - p_tile.PosX);
		default:
			return 0;
		}
	}

	protected virtual int GetLateralOffsetFromBorderSide(Tile p_tile, Tile p_referenceTile, CoastlineBorderSide p_borderSide)
	{
		switch (p_borderSide)
		{
		case CoastlineBorderSide.Left:
		case CoastlineBorderSide.Right:
			return Mathf.Abs(p_tile.PosY - p_referenceTile.PosY);
		case CoastlineBorderSide.Bottom:
		case CoastlineBorderSide.Top:
			return Mathf.Abs(p_tile.PosX - p_referenceTile.PosX);
		default:
			return 0;
		}
	}

	protected virtual Tile FindBestSeaSeed(ScenarioGenerationContext p_context)
	{
		Tile result = null;
		float num = float.MinValue;
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (!IsWaterTile(tile) && p_context.GetDistanceToMapBorder(tile) <= 1)
			{
				float num2 = (1f - p_context.Elevation[tile.PosX, tile.PosY]) * 0.72f;
				num2 += (float)CountOpenLand(tile, 2) * 0.03f;
				num2 += SampleNoise(p_context, tile.PosX, tile.PosY, 0.08f, 509.3f) * 0.22f;
				num2 += p_context.NextFloat() * 0.1f;
				if (num2 > num)
				{
					num = num2;
					result = tile;
				}
			}
		}
		return result;
	}

	protected virtual Tile FindBestSeaInletTarget(ScenarioGenerationContext p_context, Tile p_seedTile, int p_coastDepth)
	{
		Tile result = null;
		float num = float.MinValue;
		int num2 = p_coastDepth + 1;
		int num3 = Mathf.Max(num2 + 3, Mathf.Min(p_context.Map.SizeX, p_context.Map.SizeY) / 2);
		int num4 = Mathf.Clamp(num2 + 3 + p_context.NextInt(0, 4), num2 + 2, num3);
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (IsWaterTile(tile) || tile.Type == "Mountain" || p_context.GetDistanceToMapBorder(tile) <= num2)
			{
				continue;
			}
			int heuristicDistance = Utils.GetHeuristicDistance(p_seedTile, tile);
			if (heuristicDistance >= 5 && heuristicDistance <= num3)
			{
				float num5 = (1f - p_context.Elevation[tile.PosX, tile.PosY]) * 0.5f;
				num5 += SampleNoise(p_context, tile.PosX, tile.PosY, 0.06f, 587.1f) * 0.28f;
				num5 += SampleNoise(p_context, tile.PosX, tile.PosY, 0.15f, 808.4f) * 0.16f;
				num5 -= (float)Mathf.Abs(heuristicDistance - num4) * 0.09f;
				num5 -= (float)CountTypeAround(tile, "Mountain", 2) * 0.14f;
				num5 += (float)CountOpenLand(tile, 1) * 0.03f;
				num5 += p_context.NextFloat() * 0.06f;
				if (num5 > num)
				{
					num = num5;
					result = tile;
				}
			}
		}
		return result;
	}

	protected virtual void CarveSeaInlet(ScenarioGenerationContext p_context, Tile p_seedTile, Tile p_targetTile, int p_maxSteps)
	{
		Tile tile = p_seedTile;
		Tile p_previousTile = null;
		for (int i = 0; i < p_maxSteps; i++)
		{
			MarkSeaInletTile(p_context, tile, i == 0);
			if (tile != p_targetTile || i <= p_maxSteps / 3)
			{
				Tile tile2 = ChooseNextSeaInletTile(p_context, tile, p_previousTile, p_targetTile);
				if (tile2 != null)
				{
					p_previousTile = tile;
					tile = tile2;
					continue;
				}
				break;
			}
			break;
		}
	}

	protected virtual Tile ChooseNextSeaInletTile(ScenarioGenerationContext p_context, Tile p_currentTile, Tile p_previousTile, Tile p_targetTile)
	{
		List<(Tile, float)> list = new List<(Tile, float)>();
		bool flag = p_previousTile != null;
		Vector2 lhs = Vector2.zero;
		if (flag)
		{
			lhs = new Vector2(p_currentTile.PosX - p_previousTile.PosX, p_currentTile.PosY - p_previousTile.PosY).normalized;
		}
		foreach (Tile neighbour in p_currentTile.neighbours)
		{
			if ((neighbour != p_previousTile || p_currentTile.neighbours.Count <= 1) && CanBecomeWater(p_context, neighbour, p_isSea: true))
			{
				float num = (float)Utils.GetHeuristicDistance(neighbour, p_targetTile) * 0.58f;
				num -= (1f - p_context.Elevation[neighbour.PosX, neighbour.PosY]) * 1.3f;
				num -= SampleNoise(p_context, neighbour.PosX, neighbour.PosY, 0.08f, 961.4f) * 0.7f;
				num += (float)CountWaterNeighbours(neighbour) * 0.12f;
				if (p_context.GetDistanceToMapBorder(neighbour) <= 1)
				{
					num += 0.8f;
				}
				if (flag)
				{
					Vector2 normalized = new Vector2(neighbour.PosX - p_currentTile.PosX, neighbour.PosY - p_currentTile.PosY).normalized;
					float num2 = Vector2.Dot(lhs, normalized);
					num += Mathf.Max(0f, num2 - 0.1f) * 0.9f;
					num += Mathf.Max(0f, 0f - num2) * 0.25f;
				}
				num += p_context.NextFloat() * 0.4f;
				list.Add((neighbour, num));
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => a.score.CompareTo(b.score));
		int p_maxExclusive = Mathf.Min(3, list.Count);
		return list[p_context.NextInt(0, p_maxExclusive)].Item1;
	}

	protected virtual void MarkSeaInletTile(ScenarioGenerationContext p_context, Tile p_tile, bool p_isMouth)
	{
		if (!IsWaterTile(p_tile))
		{
			MarkWaterTile(p_tile);
		}
		List<(Tile, float)> list = new List<(Tile, float)>();
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (CanBecomeWater(p_context, neighbour, p_isSea: true))
			{
				float num = (1f - p_context.Elevation[neighbour.PosX, neighbour.PosY]) * 0.75f;
				num += (float)CountWaterNeighbours(neighbour) * 0.25f;
				num += SampleNoise(p_context, neighbour.PosX, neighbour.PosY, 0.1f, 1181.9f) * 0.14f;
				if (p_context.GetDistanceToMapBorder(neighbour) <= 1)
				{
					num += (p_isMouth ? 0.35f : (-0.2f));
				}
				list.Add((neighbour, num));
			}
		}
		if (list.Count != 0)
		{
			list.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => b.score.CompareTo(a.score));
			int num2 = (p_isMouth ? Mathf.Min(2, list.Count) : ((p_context.NextFloat() < 0.45f) ? 1 : 0));
			for (int num3 = 0; num3 < num2; num3++)
			{
				MarkWaterTile(list[num3].Item1);
			}
		}
	}

	protected virtual void CreateLakes(ScenarioGenerationContext p_context)
	{
		float num = (p_context.Request.GenerateSeas ? 0.0015f : 0.004f);
		int num2 = Mathf.RoundToInt((float)(p_context.Map.SizeX * p_context.Map.SizeY * p_context.Request.WaterAmount) * num);
		if (num2 <= 0)
		{
			return;
		}
		int a = Mathf.Clamp(1 + Mathf.CeilToInt((float)p_context.Request.WaterAmount / 18f), 1, p_context.Request.GenerateSeas ? 5 : 7);
		a = Mathf.Min(a, Mathf.Max(1, num2 / 3));
		int num3 = num2;
		for (int i = 0; i < a; i++)
		{
			Tile tile = FindBestLakeSeed(p_context);
			if (tile == null)
			{
				break;
			}
			int b = a - i;
			int num4 = Mathf.Max(1, num3 / Mathf.Max(1, b));
			num4 += p_context.NextInt(0, Mathf.Max(2, num4 / 2 + 1));
			num4 = Mathf.Min(num4, num3);
			int p_targetSize = Mathf.Max(1, num4 - Mathf.Max(1, num4 / 4));
			List<Tile> list = GrowWaterCluster(p_context, tile, p_targetSize, p_isSea: false);
			if (list.Count != 0)
			{
				int p_remainingBudget = Mathf.Max(0, num4 - list.Count);
				AddLakeLobes(p_context, list, p_remainingBudget);
				num3 -= CountWaterTilesCreated(list);
				if (num3 <= 0)
				{
					break;
				}
			}
		}
	}

	protected virtual int CountWaterTilesCreated(List<Tile> p_tiles)
	{
		int num = 0;
		foreach (Tile p_tile in p_tiles)
		{
			if (IsWaterTile(p_tile))
			{
				num++;
			}
		}
		return num;
	}

	protected virtual void AddLakeLobes(ScenarioGenerationContext p_context, List<Tile> p_lakeTiles, int p_remainingBudget)
	{
		if (p_remainingBudget <= 0)
		{
			return;
		}
		int num = Mathf.Clamp(1 + Mathf.CeilToInt((float)p_remainingBudget / 5f), 1, 3);
		for (int i = 0; i < num; i++)
		{
			if (p_remainingBudget <= 0)
			{
				break;
			}
			Tile tile = FindLakeLobeSeed(p_context, p_lakeTiles);
			if (tile == null)
			{
				break;
			}
			int p_targetSize = Mathf.Max(1, p_remainingBudget / Mathf.Max(1, num - i));
			List<Tile> list = GrowWaterCluster(p_context, tile, p_targetSize, p_isSea: false);
			foreach (Tile item in list)
			{
				if (!p_lakeTiles.Contains(item))
				{
					p_lakeTiles.Add(item);
				}
			}
			p_remainingBudget -= CountWaterTilesCreated(list);
		}
	}

	protected virtual Tile FindLakeLobeSeed(ScenarioGenerationContext p_context, List<Tile> p_lakeTiles)
	{
		Tile result = null;
		float num = float.MinValue;
		foreach (Tile p_lakeTile in p_lakeTiles)
		{
			if (IsWaterTile(p_lakeTile) && CountWaterNeighbours(p_lakeTile) < p_lakeTile.neighbours.Count)
			{
				float num2 = (float)(p_lakeTile.neighbours.Count - CountWaterNeighbours(p_lakeTile)) + p_context.NextFloat();
				if (num2 > num)
				{
					num = num2;
					result = p_lakeTile;
				}
			}
		}
		return result;
	}

	protected virtual Tile FindBestLakeSeed(ScenarioGenerationContext p_context)
	{
		Tile result = null;
		float num = float.MinValue;
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (!IsWaterTile(tile) && p_context.GetDistanceToMapBorder(tile) > 1)
			{
				float num2 = 1f - p_context.Elevation[tile.PosX, tile.PosY];
				num2 += p_context.Moisture[tile.PosX, tile.PosY] * 0.25f;
				num2 += p_context.NextFloat() * 0.05f;
				if (num2 > num)
				{
					num = num2;
					result = tile;
				}
			}
		}
		return result;
	}

	protected virtual List<Tile> GrowWaterCluster(ScenarioGenerationContext p_context, Tile p_seedTile, int p_targetSize, bool p_isSea)
	{
		List<Tile> list = new List<Tile>();
		List<Tile> list2 = new List<Tile>();
		if (IsWaterTile(p_seedTile))
		{
			foreach (Tile neighbour in p_seedTile.neighbours)
			{
				if (!list.Contains(neighbour))
				{
					list.Add(neighbour);
				}
			}
		}
		else
		{
			list.Add(p_seedTile);
		}
		while (list2.Count < p_targetSize && list.Count > 0)
		{
			List<(Tile, float, int)> list3 = new List<(Tile, float, int)>();
			for (int i = 0; i < list.Count; i++)
			{
				Tile tile = list[i];
				if (CanBecomeWater(p_context, tile, p_isSea))
				{
					float num = (1f - p_context.Elevation[tile.PosX, tile.PosY]) * 0.55f;
					num += (float)CountWaterNeighbours(tile) * 0.18f;
					num += SampleNoise(p_context, tile.PosX, tile.PosY, 0.16f, p_isSea ? 311.4f : 411.7f) * 0.2f;
					num += p_context.NextFloat() * 0.15f;
					if (p_isSea)
					{
						num += Mathf.Clamp01((2f - (float)p_context.GetDistanceToMapBorder(tile)) / 2f) * 0.3f;
					}
					list3.Add((tile, num, i));
				}
			}
			if (list3.Count == 0)
			{
				break;
			}
			list3.Sort(((Tile tile, float score, int index) a, (Tile tile, float score, int index) b) => b.score.CompareTo(a.score));
			int p_maxExclusive = Mathf.Min(4, list3.Count);
			int index = p_context.NextInt(0, p_maxExclusive);
			Tile item = list3[index].Item1;
			int item2 = list3[index].Item3;
			list.RemoveAt(item2);
			if (IsWaterTile(item))
			{
				continue;
			}
			MarkWaterTile(item);
			list2.Add(item);
			foreach (Tile neighbour2 in item.neighbours)
			{
				if (!list.Contains(neighbour2) && !list2.Contains(neighbour2))
				{
					list.Add(neighbour2);
				}
			}
		}
		return list2;
	}

	protected virtual bool CanBecomeWater(ScenarioGenerationContext p_context, Tile p_tile, bool p_isSea)
	{
		if (IsWaterTile(p_tile))
		{
			return false;
		}
		if (p_tile.Type == "Mountain")
		{
			return false;
		}
		return true;
	}

	protected virtual void MarkWaterTile(Tile p_tile)
	{
		p_tile.Name = "Sea";
		p_tile.Type = "Sea";
	}

	protected virtual bool IsWaterTile(Tile p_tile)
	{
		return p_tile.Type == "Sea";
	}

	protected virtual void SpreadMoistureFromWater(ScenarioGenerationContext p_context)
	{
		for (int i = 0; i < p_context.Map.SizeX; i++)
		{
			for (int j = 0; j < p_context.Map.SizeY; j++)
			{
				Tile tile = p_context.Tiles[i, j];
				if (IsWaterTile(tile))
				{
					foreach (Tile item in Utils.GetAllTilesInRadiusAround(tile, 2))
					{
						int num = Mathf.Max(1, Utils.GetHeuristicDistance(tile, item));
						p_context.Moisture[item.PosX, item.PosY] = Mathf.Clamp01(p_context.Moisture[item.PosX, item.PosY] + 0.25f / (float)num);
					}
				}
				else
				{
					p_context.Moisture[i, j] = Mathf.Clamp01(p_context.Moisture[i, j] + (1f - p_context.Elevation[i, j]) * 0.25f);
					if (p_context.Request.Season == "Desert")
					{
						p_context.Moisture[i, j] = Mathf.Clamp01(p_context.Moisture[i, j] - 0.18f);
					}
				}
			}
		}
	}

	protected virtual void CreateForestMoistureClusters(ScenarioGenerationContext p_context)
	{
		int num = p_context.Request.ThickForestAmount + p_context.Request.ThinForestAmount;
		if (num <= 0)
		{
			return;
		}
		int num2 = p_context.Map.SizeX * p_context.Map.SizeY;
		int num3 = Mathf.Clamp(2 + Mathf.CeilToInt((float)num / 16f) + Mathf.RoundToInt((float)num2 / 1500f), 2, 16);
		List<Tile> list = new List<Tile>();
		for (int i = 0; i < num3; i++)
		{
			Tile tile = FindForestSeed(p_context, list);
			if (tile != null)
			{
				list.Add(tile);
				int num4 = 1 + p_context.NextInt(0, 2);
				if (num2 >= 3600 && p_context.NextFloat() < 0.3f)
				{
					num4++;
				}
				for (int j = 0; j < num4; j++)
				{
					Tile p_tile = ShiftSeedAcrossOpenLand(p_context, tile, 1 + p_context.NextInt(0, 4));
					RaiseMoisture(p_context, p_tile, 0.18f, 0);
					RaiseMoisture(p_context, p_tile, 0.08f, 1);
					RaiseMoisture(p_context, p_tile, 0.03f, 2);
				}
			}
		}
	}

	protected virtual float GetForestPatternScaleMultiplier(ScenarioGenerationContext p_context)
	{
		int num = Mathf.Max(1, Mathf.Min(p_context.Map.SizeX, p_context.Map.SizeY));
		return Mathf.Clamp(70f / (float)num, 1f, 1.4f);
	}

	protected virtual Tile FindForestSeed(ScenarioGenerationContext p_context, List<Tile> p_existingSeeds)
	{
		List<(Tile, float)> list = new List<(Tile, float)>();
		float forestPatternScaleMultiplier = GetForestPatternScaleMultiplier(p_context);
		int num = Mathf.Clamp(Mathf.Min(p_context.Map.SizeX, p_context.Map.SizeY) / 6, 4, 8);
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (tile.Type == "Sea" || tile.Type == "Mountain" || tile.Type == "Marsh" || CountOpenLand(tile, 2) < 8)
			{
				continue;
			}
			bool flag = false;
			foreach (Tile p_existingSeed in p_existingSeeds)
			{
				if (Utils.GetHeuristicDistance(tile, p_existingSeed) < num)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				float num2 = SampleNoise(p_context, tile.PosX, tile.PosY, 0.05f * forestPatternScaleMultiplier, 519.4f) + SampleNoise(p_context, tile.PosX, tile.PosY, 0.12f * forestPatternScaleMultiplier, 733.1f) * 0.55f;
				int num3 = CountTypeAround(tile, "Sea", 3);
				float num4 = Mathf.Clamp01(1f - (float)num3 / 8f);
				float num5 = num2 * 0.58f;
				num5 += num4 * 0.22f;
				num5 += p_context.Moisture[tile.PosX, tile.PosY] * 0.08f;
				num5 += (float)CountOpenLand(tile, 2) * 0.03f;
				num5 += ((tile.Type == "Hill") ? 0.12f : 0f);
				num5 -= (float)CountWaterNeighbours(tile) * 0.16f;
				num5 += p_context.NextFloat() * 0.06f;
				list.Add((tile, num5));
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => b.score.CompareTo(a.score));
		int p_maxExclusive = Mathf.Min(8, list.Count);
		return list[p_context.NextInt(0, p_maxExclusive)].Item1;
	}

	protected virtual void RaiseMoisture(ScenarioGenerationContext p_context, Tile p_tile, float p_amount, int p_radius)
	{
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			if (item.Type != "Sea")
			{
				p_context.Moisture[item.PosX, item.PosY] = Mathf.Clamp01(p_context.Moisture[item.PosX, item.PosY] + p_amount);
			}
		}
	}

	protected virtual void ApplyElevationTypes(ScenarioGenerationContext p_context)
	{
		bool flag = p_context.Request.MountainAmount > 0;
		float num = (flag ? Mathf.Lerp(0.96f, 0.72f, (float)p_context.Request.MountainAmount / 100f) : 1.1f);
		float num2 = Mathf.Clamp01((float)p_context.Request.HillsAmount / 100f);
		float num3 = Mathf.Lerp(0.82f, flag ? 0.5f : 0.46f, Mathf.Pow(num2, 0.75f));
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (!IsWaterTile(tile))
			{
				tile.Name = "Plain";
				tile.Type = "Plain";
			}
		}
		tiles = p_context.Tiles;
		foreach (Tile tile2 in tiles)
		{
			if (!IsWaterTile(tile2))
			{
				float num4 = p_context.Elevation[tile2.PosX, tile2.PosY];
				if (flag && num4 >= num)
				{
					tile2.Name = "Mountain";
					tile2.Type = "Mountain";
				}
			}
		}
		tiles = p_context.Tiles;
		foreach (Tile tile3 in tiles)
		{
			if (!IsWaterTile(tile3) && !(tile3.Type == "Mountain"))
			{
				float num5 = p_context.Elevation[tile3.PosX, tile3.PosY];
				int num6 = CountNeighboursOfType(tile3, "Mountain");
				int num7 = CountTypeAround(tile3, "Hill", 2);
				if (num5 >= num3 || (num6 > 0 && num5 >= num3 - 0.08f) || (num7 > 2 && num5 >= num3 - 0.06f))
				{
					tile3.Name = "Hill";
					tile3.Type = "Hill";
				}
			}
		}
		EnsureHillCoverage(p_context, num2);
	}

	protected virtual void EnsureHillCoverage(ScenarioGenerationContext p_context, float p_hillFactor)
	{
		if (p_hillFactor <= 0f)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		List<(Tile, float)> list = new List<(Tile, float)>();
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (!IsWaterTile(tile) && !(tile.Type == "Mountain"))
			{
				num++;
				if (tile.Type == "Hill")
				{
					num2++;
					continue;
				}
				float num3 = p_context.Elevation[tile.PosX, tile.PosY] * 1.35f;
				num3 += (float)CountNeighboursOfType(tile, "Hill") * 0.12f;
				num3 += (float)CountTypeAround(tile, "Hill", 2) * 0.035f;
				num3 += (float)CountNeighboursOfType(tile, "Mountain") * 0.08f;
				num3 += (float)CountOpenLand(tile, 2) * 0.01f;
				num3 -= (float)CountWaterNeighbours(tile) * 0.1f;
				num3 += SampleNoise(p_context, tile.PosX, tile.PosY, 0.09f, 1607.3f) * 0.08f;
				list.Add((tile, num3));
			}
		}
		if (num == 0 || num2 >= num)
		{
			return;
		}
		int num4 = Mathf.Clamp(Mathf.RoundToInt((float)num * p_hillFactor), num2, num);
		if (num4 <= num2)
		{
			return;
		}
		list.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => b.score.CompareTo(a.score));
		int num5 = num4 - num2;
		for (int num6 = 0; num6 < list.Count; num6++)
		{
			if (num5 <= 0)
			{
				break;
			}
			Tile item = list[num6].Item1;
			item.Name = "Hill";
			item.Type = "Hill";
			num5--;
		}
	}

	protected virtual void ApplyBiomeCover(ScenarioGenerationContext p_context)
	{
		float p_bocageThreshold = Mathf.Lerp(0.78f, 0.46f, (float)p_context.Request.BocageAmount / 100f);
		float forestPatternScaleMultiplier = GetForestPatternScaleMultiplier(p_context);
		List<(Tile, float)> list = new List<(Tile, float)>();
		List<(Tile, float, int)> list2 = new List<(Tile, float, int)>();
		int num = 0;
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (IsWaterTile(tile) || tile.Type == "Mountain")
			{
				continue;
			}
			int num2 = CountWaterNeighbours(tile);
			int num3 = CountTypeAround(tile, "Sea", 3);
			float num4 = p_context.Elevation[tile.PosX, tile.PosY];
			float num5 = SampleNoise(p_context, tile.PosX, tile.PosY, 0.05f * forestPatternScaleMultiplier, 947.1f);
			float num6 = SampleNoise(p_context, tile.PosX, tile.PosY, 0.13f * forestPatternScaleMultiplier, 1201.7f);
			float num7 = p_context.Moisture[tile.PosX, tile.PosY];
			float num8 = Mathf.Clamp01(1f - (float)num3 / 8f);
			float num9 = num5 * 0.42f + num6 * 0.28f + num8 * 0.18f + num7 * 0.18f + ((tile.Type == "Hill") ? 0.08f : 0f) - num4 * 0.08f - (float)num2 * 0.04f + p_context.NextFloat() * 0.08f;
			if (p_context.Request.Season == "Desert")
			{
				if (p_context.Request.DesertRockAmount > 0 && (tile.Type == "Hill" || num4 > 0.46f) && num9 < Mathf.Lerp(0.35f, 0.14f, (float)p_context.Request.DesertRockAmount / 100f))
				{
					tile.Name = "Desert rocks";
					tile.Type = "Plain";
				}
				else if (p_context.Request.DunesAmount > 0 && tile.Type != "Hill" && num9 < Mathf.Lerp(0.45f, 0.18f, (float)p_context.Request.DunesAmount / 100f))
				{
					tile.Name = "Dunes";
					tile.Type = "Plain";
				}
			}
			else if (tile.Type == "Hill")
			{
				tile.Name = "Hill";
				list.Add((tile, num9));
				num++;
			}
			else if (!(tile.Type != "Plain"))
			{
				tile.Name = "Plain";
				tile.Type = "Plain";
				list.Add((tile, num9));
				list2.Add((tile, num9, num2));
				num++;
			}
		}
		if (p_context.Request.Season != "Desert")
		{
			ApplyForestCoverageTargets(p_context, list, list2, num, p_bocageThreshold);
		}
	}

	protected virtual void ApplyForestCoverageTargets(ScenarioGenerationContext p_context, List<(Tile tile, float score)> p_thickForestCandidates, List<(Tile tile, float score, int waterNeighbours)> p_plainForestCandidates, int p_eligibleForestTiles, float p_bocageThreshold)
	{
		if (p_eligibleForestTiles <= 0)
		{
			return;
		}
		int num = Mathf.Clamp(Mathf.RoundToInt((float)p_eligibleForestTiles * ((float)p_context.Request.ThickForestAmount / 100f)), 0, p_thickForestCandidates.Count);
		int num2 = Mathf.Clamp(Mathf.RoundToInt((float)p_eligibleForestTiles * ((float)p_context.Request.ThinForestAmount / 100f)), 0, p_plainForestCandidates.Count);
		HashSet<Tile> hashSet = new HashSet<Tile>();
		p_thickForestCandidates.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => b.score.CompareTo(a.score));
		for (int num3 = 0; num3 < num; num3++)
		{
			Tile item = p_thickForestCandidates[num3].tile;
			if (hashSet.Add(item))
			{
				if (item.Type == "Hill")
				{
					item.Name = "HillForest";
					continue;
				}
				item.Name = "ThickForest";
				item.Type = "ThickForest";
			}
		}
		p_plainForestCandidates.Sort(((Tile tile, float score, int waterNeighbours) a, (Tile tile, float score, int waterNeighbours) b) => b.score.CompareTo(a.score));
		int num4 = 0;
		foreach (var p_plainForestCandidate in p_plainForestCandidates)
		{
			var (tile, _, _) = p_plainForestCandidate;
			if (!hashSet.Contains(tile))
			{
				if (num4 < num2)
				{
					tile.Name = "ThinForest";
					tile.Type = "ThinForest";
					num4++;
				}
				else if (p_context.Request.BocageAmount > 0 && p_plainForestCandidate.waterNeighbours <= 1 && p_plainForestCandidate.score > p_bocageThreshold)
				{
					tile.Name = "Bocage";
					tile.Type = "Bocage";
				}
			}
		}
	}

	public virtual void FinalizeBiomesAfterRivers(Map p_map, ScenarioGenerationRequest p_request)
	{
		if (p_map != null && p_request != null)
		{
			ApplyPostRiverMarshlands(p_map, p_request);
		}
	}

	protected virtual void ApplyPostRiverMarshlands(Map p_map, ScenarioGenerationRequest p_request)
	{
		if (p_request.MarshAmount <= 0 || p_request.Season == "Desert")
		{
			return;
		}
		List<(Tile, float)> list = new List<(Tile, float)>();
		Tile[,] tilesTable = p_map.TilesTable;
		foreach (Tile tile in tilesTable)
		{
			if (IsValidPostRiverMarshCandidate(tile))
			{
				float num = ScorePostRiverMarshCandidate(tile, p_request);
				if (!(num < 0.55f))
				{
					list.Add((tile, num));
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		list.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => b.score.CompareTo(a.score));
		int min = Mathf.Clamp(1 + Mathf.RoundToInt((float)p_request.MarshAmount / 6f), 1, list.Count);
		int value = Mathf.RoundToInt((float)list.Count * Mathf.Lerp(0.1f, 0.42f, (float)p_request.MarshAmount / 100f));
		value = Mathf.Clamp(value, min, list.Count);
		int num2 = 0;
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			if (num2 >= value)
			{
				break;
			}
			Tile item = list[num3].Item1;
			if (IsValidPostRiverMarshCandidate(item))
			{
				item.Name = "Marsh";
				item.Type = "Marsh";
				num2++;
			}
		}
	}

	protected virtual bool IsValidPostRiverMarshCandidate(Tile p_tile)
	{
		if (p_tile == null || p_tile.IsVictoryPoint)
		{
			return false;
		}
		if (p_tile.Type == "Road" || p_tile.Railroad || p_tile.Bridge)
		{
			return false;
		}
		if (p_tile.Type == "Sea" || p_tile.Type == "River" || p_tile.Type == "Mountain" || p_tile.Type == "Hill")
		{
			return false;
		}
		if (p_tile.Type == "City" || p_tile.Type == "Airport" || p_tile.Type == "Factory" || p_tile.Type == "Harbour")
		{
			return false;
		}
		if (p_tile.Type == "ThickForest")
		{
			return false;
		}
		if (CountNeighboursOfType(p_tile, "River") <= 0 && CountWaterNeighbours(p_tile) <= 0 && CountTypeAround(p_tile, "River", 2) <= 1)
		{
			return CountTypeAround(p_tile, "Sea", 2) > 1;
		}
		return true;
	}

	protected virtual float ScorePostRiverMarshCandidate(Tile p_tile, ScenarioGenerationRequest p_request)
	{
		int num = CountNeighboursOfType(p_tile, "River");
		int num2 = CountWaterNeighbours(p_tile);
		int num3 = CountTypeAround(p_tile, "River", 2);
		int num4 = CountTypeAround(p_tile, "Sea", 2);
		int num5 = CountNeighboursOfType(p_tile, "Marsh");
		float num6 = (float)num * 0.5f;
		num6 += (float)num2 * 0.35f;
		num6 += (float)num3 * 0.12f;
		num6 += (float)num4 * 0.08f;
		num6 += (float)num5 * 0.18f;
		num6 += SampleSeedNoise(p_request.RuntimeSeed, p_tile.PosX, p_tile.PosY, 0.14f, 1403.9f) * 0.18f;
		if (p_tile.Type == "ThinForest")
		{
			num6 -= 0.08f;
		}
		else if (p_tile.Type == "Bocage")
		{
			num6 += 0.04f;
		}
		return num6;
	}

	protected virtual int CountWaterNeighbours(Tile p_tile)
	{
		int num = 0;
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (IsWaterTile(neighbour))
			{
				num++;
			}
		}
		return num;
	}

	protected virtual int CountNeighboursOfType(Tile p_tile, string p_type)
	{
		int num = 0;
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (neighbour.Type == p_type)
			{
				num++;
			}
		}
		return num;
	}

	protected virtual int CountTypeAround(Tile p_tile, string p_type, int p_radius)
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

	protected virtual int CountOpenLand(Tile p_tile, int p_radius)
	{
		int num = 0;
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			if (item.Type != "Sea" && item.Type != "Mountain")
			{
				num++;
			}
		}
		return num;
	}
}

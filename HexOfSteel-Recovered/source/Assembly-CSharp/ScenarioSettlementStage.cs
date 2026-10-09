using System.Collections.Generic;
using UnityEngine;

public class ScenarioSettlementStage : IRuntimeScenarioGenerationStage
{
	public virtual void Execute(ScenarioGenerationContext p_context)
	{
		bool flag = HasOptionalSettlements(p_context);
		PlaceCapitals(p_context);
		if (flag)
		{
			PlaceOwnedSettlements(p_context, ScenarioSettlementTier.City, GetAdditionalCityQuotaPerPlayer(p_context));
			PlaceOwnedSettlements(p_context, ScenarioSettlementTier.Town, GetTownQuotaPerPlayer(p_context));
			PlaceOwnedSettlements(p_context, ScenarioSettlementTier.Village, GetVillageQuotaPerPlayer(p_context));
		}
		Utils.UpdateTilesData(p_nonDestructive: true);
		if (flag)
		{
			NormalizeOwnedEconomy(p_context);
		}
	}

	protected virtual void PlaceCapitals(ScenarioGenerationContext p_context)
	{
		ScenarioSettlementTier capitalSettlementTier = GetCapitalSettlementTier(p_context);
		foreach (ScenarioPlayerRegion playerRegion in p_context.PlayerRegions)
		{
			Tile tile = FindBestOwnedCandidate(p_context, playerRegion, ScenarioSettlementTier.City);
			if (tile != null)
			{
				playerRegion.Settlements.Add(PlaceSettlementCluster(p_context, tile, capitalSettlementTier, playerRegion.Index, p_isCapital: true));
			}
		}
	}

	protected virtual void PlaceOwnedSettlements(ScenarioGenerationContext p_context, ScenarioSettlementTier p_tier, int p_quotaPerPlayer)
	{
		foreach (ScenarioPlayerRegion playerRegion in p_context.PlayerRegions)
		{
			for (int i = 0; i < p_quotaPerPlayer; i++)
			{
				bool flag = ShouldForceCoastalTownHarbour(p_context, playerRegion, p_tier);
				Tile tile = FindBestOwnedCandidate(p_context, playerRegion, p_tier, flag);
				if ((tile == null) & flag)
				{
					tile = FindBestOwnedCandidate(p_context, playerRegion, p_tier);
				}
				if (tile == null)
				{
					break;
				}
				playerRegion.Settlements.Add(PlaceSettlementCluster(p_context, tile, p_tier, playerRegion.Index, p_isCapital: false));
			}
		}
	}

	protected virtual bool ShouldForceCoastalTownHarbour(ScenarioGenerationContext p_context, ScenarioPlayerRegion p_region, ScenarioSettlementTier p_tier)
	{
		if (!p_context.Request.GenerateSeas || p_tier != ScenarioSettlementTier.Town)
		{
			return false;
		}
		foreach (ScenarioSettlementCluster settlement in p_region.Settlements)
		{
			if (settlement.Tier == ScenarioSettlementTier.Town)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool HasOptionalSettlements(ScenarioGenerationContext p_context)
	{
		return GetUrbanPressure(p_context) > 0f;
	}

	protected virtual ScenarioSettlementTier GetCapitalSettlementTier(ScenarioGenerationContext p_context)
	{
		if (!HasOptionalSettlements(p_context))
		{
			return ScenarioSettlementTier.Village;
		}
		return ScenarioSettlementTier.City;
	}

	protected virtual int GetVillageQuotaPerPlayer(ScenarioGenerationContext p_context)
	{
		return GetAreaScaledSettlementQuota(p_context, 50f);
	}

	protected virtual int GetTownQuotaPerPlayer(ScenarioGenerationContext p_context)
	{
		return GetAreaScaledSettlementQuota(p_context, 120f);
	}

	protected virtual int GetAdditionalCityQuotaPerPlayer(ScenarioGenerationContext p_context)
	{
		return GetAreaScaledSettlementQuota(p_context, 300f);
	}

	protected virtual float GetAreaPerPlayer(ScenarioGenerationContext p_context)
	{
		return (float)(p_context.Map.SizeX * p_context.Map.SizeY) / (float)Mathf.Max(1, p_context.PlayerRegions.Count);
	}

	protected virtual float GetUrbanPressure(ScenarioGenerationContext p_context)
	{
		return Mathf.Clamp01((float)p_context.Request.CitiesAmount / 100f);
	}

	protected virtual int GetAreaScaledSettlementQuota(ScenarioGenerationContext p_context, float p_tilesPerSettlementAtFullDensity)
	{
		if (p_tilesPerSettlementAtFullDensity <= 0f)
		{
			return 0;
		}
		float areaPerPlayer = GetAreaPerPlayer(p_context);
		float urbanPressure = GetUrbanPressure(p_context);
		float num = areaPerPlayer / p_tilesPerSettlementAtFullDensity;
		return ScaleSettlementQuota(num, urbanPressure, Mathf.CeilToInt(num));
	}

	protected virtual int ScaleSettlementQuota(float p_quotaCap, float p_urbanPressure, int p_maxQuota)
	{
		if (p_quotaCap <= 0f || p_urbanPressure <= 0f)
		{
			return 0;
		}
		return Mathf.Clamp(Mathf.FloorToInt(p_quotaCap * p_urbanPressure + 0.5f), 0, p_maxQuota);
	}

	protected virtual ScenarioSettlementCluster PlaceSettlementCluster(ScenarioGenerationContext p_context, Tile p_coreTile, ScenarioSettlementTier p_tier, int p_ownerIndex, bool p_isCapital)
	{
		ScenarioSettlementCluster scenarioSettlementCluster = new ScenarioSettlementCluster(p_tier, p_coreTile, p_ownerIndex, p_isCapital);
		string p_name = GetSettlementName(p_tier);
		string p_type = "City";
		if (TryGetHarbourName(p_context, p_coreTile, p_tier, out var o_harbourName))
		{
			p_name = o_harbourName;
			p_type = "Harbour";
		}
		MarkVictoryPointTile(p_context, p_coreTile, p_name, p_type, p_ownerIndex);
		if (p_tier == ScenarioSettlementTier.City && p_context.Request.GenerateFactories)
		{
			scenarioSettlementCluster.FactoryTile = PlaceSupportTile(p_context, p_coreTile, "Factory", "Factory", p_ownerIndex, null);
		}
		if (p_tier != ScenarioSettlementTier.Village && p_context.Request.GenerateAirfields)
		{
			scenarioSettlementCluster.AirfieldTile = PlaceSupportTile(p_context, p_coreTile, "Airport", "Airport", p_ownerIndex, scenarioSettlementCluster.FactoryTile);
		}
		return scenarioSettlementCluster;
	}

	protected virtual string GetSettlementName(ScenarioSettlementTier p_tier)
	{
		switch (p_tier)
		{
		case ScenarioSettlementTier.City:
			return "City";
		case ScenarioSettlementTier.Town:
			return "Town";
		default:
			return "Village";
		}
	}

	protected virtual bool TryGetHarbourName(ScenarioGenerationContext p_context, Tile p_tile, ScenarioSettlementTier p_tier, out string o_harbourName)
	{
		o_harbourName = null;
		if (CountWaterAround(p_tile, 2) < GetMinimumNearbyWaterForHarbour(p_context, p_tier))
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		int num = 0;
		foreach (Tile neighbour in p_tile.neighbours)
		{
			if (!(neighbour.Type == "Sea"))
			{
				continue;
			}
			num++;
			if (neighbour.PosX == p_tile.PosX && neighbour.PosY == p_tile.PosY + 1)
			{
				flag = true;
			}
			else if (neighbour.PosX == p_tile.PosX && neighbour.PosY == p_tile.PosY - 1)
			{
				flag2 = true;
			}
			if (p_tile.PosX % 2 == 0)
			{
				if (neighbour.PosX == p_tile.PosX - 1 && neighbour.PosY == p_tile.PosY + 1)
				{
					flag3 = true;
				}
				else if (neighbour.PosX == p_tile.PosX - 1 && neighbour.PosY == p_tile.PosY)
				{
					flag5 = true;
				}
				else if (neighbour.PosX == p_tile.PosX + 1 && neighbour.PosY == p_tile.PosY + 1)
				{
					flag4 = true;
				}
				else if (neighbour.PosX == p_tile.PosX + 1 && neighbour.PosY == p_tile.PosY)
				{
					flag6 = true;
				}
			}
			else if (neighbour.PosX == p_tile.PosX - 1 && neighbour.PosY == p_tile.PosY)
			{
				flag3 = true;
			}
			else if (neighbour.PosX == p_tile.PosX - 1 && neighbour.PosY == p_tile.PosY - 1)
			{
				flag5 = true;
			}
			else if (neighbour.PosX == p_tile.PosX + 1 && neighbour.PosY == p_tile.PosY)
			{
				flag4 = true;
			}
			else if (neighbour.PosX == p_tile.PosX + 1 && neighbour.PosY == p_tile.PosY - 1)
			{
				flag6 = true;
			}
		}
		if (num == 0 || num == 6)
		{
			return false;
		}
		string text = ((p_tier == ScenarioSettlementTier.Village && p_context.Request.Season != "Desert") ? "SmallHarbour" : "Harbour");
		switch (num)
		{
		case 1:
			if (flag)
			{
				text += "1";
			}
			else if (flag4)
			{
				text += "2";
			}
			else if (flag6)
			{
				text += "3";
			}
			else if (flag2)
			{
				text += "4";
			}
			else if (flag5)
			{
				text += "5";
			}
			else if (flag3)
			{
				text += "6";
			}
			break;
		case 2:
			if (flag & flag3)
			{
				text += "1";
			}
			else if (flag & flag4)
			{
				text += "2";
			}
			else if (flag4 & flag6)
			{
				text += "3";
			}
			else if (flag2 & flag6)
			{
				text += "4";
			}
			else
			{
				text = ((!(flag2 & flag5)) ? (text + "6") : (text + "5"));
			}
			break;
		case 3:
			if (flag & flag3 & flag4)
			{
				text += "1";
			}
			else if (flag & flag4 & flag6)
			{
				text += "2";
			}
			else if (flag4 & flag6 & flag2)
			{
				text += "3";
			}
			else if (flag6 & flag2 & flag5)
			{
				text += "4";
			}
			else
			{
				text = ((!(flag2 & flag5 & flag3)) ? (text + "6") : (text + "5"));
			}
			break;
		case 4:
			if (flag5 & flag3 & flag & flag4)
			{
				text += "1";
			}
			else if (flag3 & flag & flag4 & flag6)
			{
				text += "2";
			}
			else if (flag & flag4 & flag6 & flag2)
			{
				text += "3";
			}
			else if (flag4 & flag6 & flag2 & flag5)
			{
				text += "4";
			}
			else
			{
				text = ((!(flag6 & flag2 & flag5 & flag3)) ? (text + "6") : (text + "5"));
			}
			break;
		case 5:
			if (flag5 & flag3 & flag & flag4 & flag6)
			{
				text += "1";
			}
			else if (flag3 & flag & flag4 & flag6 & flag2)
			{
				text += "2";
			}
			else if (flag & flag4 & flag6 & flag2 & flag5)
			{
				text += "3";
			}
			else if (flag4 & flag6 & flag2 & flag5 & flag3)
			{
				text += "4";
			}
			else
			{
				text = ((!(flag6 & flag2 & flag5 & flag3 & flag)) ? (text + "6") : (text + "5"));
			}
			break;
		}
		o_harbourName = text;
		return true;
	}

	protected virtual int GetMinimumNearbyWaterForHarbour(ScenarioGenerationContext p_context, ScenarioSettlementTier p_tier)
	{
		if (p_context.Request.GenerateSeas && p_tier == ScenarioSettlementTier.Town)
		{
			return 0;
		}
		return 5;
	}

	protected virtual void MarkVictoryPointTile(ScenarioGenerationContext p_context, Tile p_tile, string p_name, string p_type, int p_ownerIndex)
	{
		if (!p_tile.IsVictoryPoint)
		{
			p_context.TotalVictoryPoints++;
		}
		p_tile.IsVictoryPoint = true;
		p_tile.Name = p_name;
		p_tile.Type = p_type;
		Player player = (p_tile.ownerPlayer = p_context.PlayerRegions[p_ownerIndex].Player);
		p_tile.OwnerName = player.Name;
		p_tile.OriginalCountryOwner = player.Country;
		ApplyGeneratedYieldMultipliers(p_context, p_tile);
	}

	protected virtual void ApplyGeneratedYieldMultipliers(ScenarioGenerationContext p_context, Tile p_tile)
	{
		float p_multiplier = Mathf.Clamp(p_context.Request.IncomeMultiplier, 0f, 5f);
		float p_multiplier2 = Mathf.Clamp(p_context.Request.ManpowerMultiplier, 0f, 5f);
		if (Database.TryGetTile(p_tile.Name, p_tile.Season, out var o_tile))
		{
			if (o_tile.MoneyProductionPerTurn != 0)
			{
				p_tile.EditorOverwrite_income = ScaleIncomeValue(o_tile.MoneyProductionPerTurn, p_multiplier);
			}
			if (o_tile.Manpower != 0)
			{
				p_tile.EditorOverwrite_manpower = ScaleManpowerValue(o_tile.Manpower, p_multiplier2);
			}
		}
	}

	protected virtual short ScaleIncomeValue(short p_value, float p_multiplier)
	{
		return (short)Mathf.Clamp(Mathf.RoundToInt((float)p_value * Mathf.Max(0f, p_multiplier)), 0, 32767);
	}

	protected virtual int ScaleManpowerValue(int p_value, float p_multiplier)
	{
		return Mathf.Max(0, Mathf.RoundToInt((float)p_value * Mathf.Max(0f, p_multiplier)));
	}

	protected virtual Tile PlaceSupportTile(ScenarioGenerationContext p_context, Tile p_coreTile, string p_name, string p_type, int p_ownerIndex, Tile p_reservedTile)
	{
		Tile tile = FindBestSupportTile(p_context, p_coreTile, p_type, p_reservedTile);
		if (tile == null)
		{
			return null;
		}
		MarkVictoryPointTile(p_context, tile, p_name, p_type, p_ownerIndex);
		return tile;
	}

	protected virtual Tile FindBestOwnedCandidate(ScenarioGenerationContext p_context, ScenarioPlayerRegion p_region, ScenarioSettlementTier p_tier, bool p_requireHarbour = false)
	{
		List<(Tile, float)> list = new List<(Tile, float)>();
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (p_context.GetRegionIndex(tile) == p_region.Index && IsValidSettlementCandidate(p_context, tile, p_tier, p_requireHarbour))
			{
				list.Add((tile, ScoreOwnedCandidate(p_context, p_region, tile, p_tier)));
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort(((Tile tile, float score) a, (Tile tile, float score) b) => b.score.CompareTo(a.score));
		int p_maxExclusive = Mathf.Min(GetCandidatePoolSize(p_tier), list.Count);
		return list[p_context.NextInt(0, p_maxExclusive)].Item1;
	}

	protected virtual bool IsValidSettlementCandidate(ScenarioGenerationContext p_context, Tile p_tile, ScenarioSettlementTier p_tier, bool p_requireHarbour = false)
	{
		if (IsReservedTile(p_context, p_tile))
		{
			return false;
		}
		if (p_tile.Type == "Sea" || p_tile.Type == "Mountain" || p_tile.Type == "Marsh" || p_tile.Type == "River" || p_tile.Type == "Airport" || p_tile.Type == "Factory" || p_tile.Type == "Harbour")
		{
			return false;
		}
		if (p_requireHarbour && !CanBecomeHarbourSettlement(p_context, p_tile, p_tier))
		{
			return false;
		}
		if (HasSettlementWithin(p_context, p_tile, GetMinimumSettlementSpacing(p_tier)))
		{
			return false;
		}
		bool flag = p_tier == ScenarioSettlementTier.City && p_context.Request.GenerateFactories;
		bool flag2 = p_tier != ScenarioSettlementTier.Village && p_context.Request.GenerateAirfields;
		if (flag | flag2)
		{
			List<Tile> list = (flag ? GetSupportCandidates(p_context, p_tile, "Factory", null) : null);
			List<Tile> list2 = (flag2 ? GetSupportCandidates(p_context, p_tile, "Airport", null) : null);
			if (flag && (list == null || list.Count == 0))
			{
				return false;
			}
			if (flag2 && (list2 == null || list2.Count == 0))
			{
				return false;
			}
			if ((flag & flag2) && list.Count == 1 && list2.Count == 1 && list[0] == list2[0])
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool CanBecomeHarbourSettlement(ScenarioGenerationContext p_context, Tile p_tile, ScenarioSettlementTier p_tier)
	{
		string o_harbourName;
		return TryGetHarbourName(p_context, p_tile, p_tier, out o_harbourName);
	}

	protected virtual bool IsReservedTile(ScenarioGenerationContext p_context, Tile p_tile)
	{
		if (p_tile.IsVictoryPoint)
		{
			return true;
		}
		foreach (ScenarioPlayerRegion playerRegion in p_context.PlayerRegions)
		{
			foreach (ScenarioSettlementCluster settlement in playerRegion.Settlements)
			{
				if (settlement.CoreTile == p_tile || settlement.FactoryTile == p_tile || settlement.AirfieldTile == p_tile)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual bool HasSettlementWithin(ScenarioGenerationContext p_context, Tile p_tile, int p_distance)
	{
		foreach (ScenarioPlayerRegion playerRegion in p_context.PlayerRegions)
		{
			foreach (ScenarioSettlementCluster settlement in playerRegion.Settlements)
			{
				if (Utils.GetHeuristicDistance(settlement.CoreTile, p_tile) <= p_distance)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual int GetMinimumSettlementSpacing(ScenarioSettlementTier p_tier)
	{
		switch (p_tier)
		{
		case ScenarioSettlementTier.City:
			return 6;
		case ScenarioSettlementTier.Town:
			return 4;
		default:
			return 3;
		}
	}

	protected virtual float ScoreOwnedCandidate(ScenarioGenerationContext p_context, ScenarioPlayerRegion p_region, Tile p_tile, ScenarioSettlementTier p_tier)
	{
		float num = GetDesiredAnchorDistance(p_context, p_tier);
		if (p_tier == ScenarioSettlementTier.City && RegionAlreadyHasCapital(p_region))
		{
			num = Mathf.Max(5f, (float)Mathf.Min(p_context.Map.SizeX, p_context.Map.SizeY) * 0.18f);
		}
		float num2 = Utils.GetHeuristicDistance(p_region.AnchorTile, p_tile);
		float distanceToNearestRegionalSettlement = GetDistanceToNearestRegionalSettlement(p_region, p_tile);
		float num3 = 0f;
		num3 -= Mathf.Abs(num2 - num) * ((p_tier == ScenarioSettlementTier.Village) ? 2.2f : 3.2f);
		num3 += (float)CountOpenLandAround(p_tile, 2) * 3f;
		num3 += (float)CountTypeAround(p_tile, 2, "Plain") * 3.5f;
		num3 += (float)CountFrontierOpportunity(p_context, p_tile, 3) * ((p_tier == ScenarioSettlementTier.Town) ? 1.2f : 0.4f);
		num3 += (float)CountWaterAround(p_tile, 2) * 1.2f;
		num3 -= (float)CountTypeAround(p_tile, 2, "Mountain") * 4.5f;
		num3 -= (float)CountTypeAround(p_tile, 2, "Marsh") * 3f;
		if (distanceToNearestRegionalSettlement > 0f)
		{
			num3 += Mathf.Min(distanceToNearestRegionalSettlement, GetPreferredRegionalSpreadDistance(p_context, p_tier)) * ((p_tier == ScenarioSettlementTier.Village) ? 1.6f : 2.2f);
		}
		num3 -= (float)CountOwnedSettlementsAround(p_region, p_tile, GetMinimumSettlementSpacing(p_tier) + 3) * ((p_tier == ScenarioSettlementTier.City) ? 14f : 9f);
		return num3 + p_context.NextFloat();
	}

	protected virtual int GetCandidatePoolSize(ScenarioSettlementTier p_tier)
	{
		switch (p_tier)
		{
		case ScenarioSettlementTier.City:
			return 3;
		case ScenarioSettlementTier.Town:
			return 5;
		default:
			return 6;
		}
	}

	protected virtual bool RegionAlreadyHasCapital(ScenarioPlayerRegion p_region)
	{
		foreach (ScenarioSettlementCluster settlement in p_region.Settlements)
		{
			if (settlement.IsCapital)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual float GetDesiredAnchorDistance(ScenarioGenerationContext p_context, ScenarioSettlementTier p_tier)
	{
		float num = Mathf.Min(p_context.Map.SizeX, p_context.Map.SizeY);
		switch (p_tier)
		{
		case ScenarioSettlementTier.City:
			return 0f;
		case ScenarioSettlementTier.Town:
			return Mathf.Max(4f, num * 0.18f);
		default:
			return Mathf.Max(6f, num * 0.24f);
		}
	}

	protected virtual float GetPreferredRegionalSpreadDistance(ScenarioGenerationContext p_context, ScenarioSettlementTier p_tier)
	{
		float num = Mathf.Min(p_context.Map.SizeX, p_context.Map.SizeY);
		switch (p_tier)
		{
		case ScenarioSettlementTier.City:
			return Mathf.Max(8f, num * 0.24f);
		case ScenarioSettlementTier.Town:
			return Mathf.Max(6f, num * 0.18f);
		default:
			return Mathf.Max(4f, num * 0.14f);
		}
	}

	protected virtual float GetDistanceToNearestRegionalSettlement(ScenarioPlayerRegion p_region, Tile p_tile)
	{
		float num = float.MaxValue;
		foreach (ScenarioSettlementCluster settlement in p_region.Settlements)
		{
			num = Mathf.Min(num, Utils.GetHeuristicDistance(settlement.CoreTile, p_tile));
		}
		if (num == float.MaxValue)
		{
			return 0f;
		}
		return num;
	}

	protected virtual int CountOwnedSettlementsAround(ScenarioPlayerRegion p_region, Tile p_tile, int p_radius)
	{
		int num = 0;
		foreach (ScenarioSettlementCluster settlement in p_region.Settlements)
		{
			if (Utils.GetHeuristicDistance(settlement.CoreTile, p_tile) <= p_radius)
			{
				num++;
			}
		}
		return num;
	}

	protected virtual int CountFrontierOpportunity(ScenarioGenerationContext p_context, Tile p_tile, int p_radius)
	{
		int num = 0;
		int regionIndex = p_context.GetRegionIndex(p_tile);
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			int regionIndex2 = p_context.GetRegionIndex(item);
			if (regionIndex2 >= 0 && regionIndex2 != regionIndex)
			{
				num++;
			}
		}
		return num;
	}

	protected virtual int CountOpenLandAround(Tile p_tile, int p_radius)
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

	protected virtual int CountWaterAround(Tile p_tile, int p_radius)
	{
		int num = 0;
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_tile, p_radius))
		{
			if (item.Type == "Sea")
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

	protected virtual Tile FindBestSupportTile(ScenarioGenerationContext p_context, Tile p_coreTile, string p_supportType, Tile p_reservedTile)
	{
		List<Tile> supportCandidates = GetSupportCandidates(p_context, p_coreTile, p_supportType, p_reservedTile);
		Tile result = null;
		float num = float.MinValue;
		foreach (Tile item in supportCandidates)
		{
			float num2 = 10f - (float)Utils.GetHeuristicDistance(item, p_coreTile);
			if (item.Type == "Plain")
			{
				num2 += 5f;
			}
			else if (item.Type == "Hill")
			{
				num2 += ((p_supportType == "Factory") ? 1f : (-3f));
			}
			else if (item.Type == "ThickForest" || item.Type == "ThinForest" || item.Type == "Bocage")
			{
				num2 -= 1.5f;
			}
			if (item.Type == "Marsh")
			{
				num2 -= 6f;
			}
			num2 += p_context.NextFloat();
			if (num2 > num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	protected virtual List<Tile> GetSupportCandidates(ScenarioGenerationContext p_context, Tile p_coreTile, string p_supportType, Tile p_reservedTile)
	{
		List<Tile> list = new List<Tile>();
		foreach (Tile item in Utils.GetAllTilesInRadiusAround(p_coreTile, 2))
		{
			if (item != p_coreTile && item != p_reservedTile && !IsReservedTile(p_context, item) && !(item.Type == "Sea") && !(item.Type == "Mountain") && !(item.Type == "Marsh") && !(item.Type == "River") && !(item.Type == "City") && !(item.Type == "Airport") && !(item.Type == "Factory") && !(item.Type == "Harbour") && (!(p_supportType == "Airport") || !(item.Type == "Hill")))
			{
				list.Add(item);
			}
		}
		return list;
	}

	protected virtual void NormalizeOwnedEconomy(ScenarioGenerationContext p_context)
	{
		int num = p_context.PlayerRegions.Count * 2;
		for (int i = 0; i < num; i++)
		{
			ScenarioPlayerRegion scenarioPlayerRegion = null;
			ScenarioPlayerRegion scenarioPlayerRegion2 = null;
			int num2 = int.MaxValue;
			int num3 = int.MinValue;
			foreach (ScenarioPlayerRegion playerRegion in p_context.PlayerRegions)
			{
				int regionIncome = GetRegionIncome(p_context, playerRegion);
				if (regionIncome < num2)
				{
					num2 = regionIncome;
					scenarioPlayerRegion = playerRegion;
				}
				if (regionIncome > num3)
				{
					num3 = regionIncome;
					scenarioPlayerRegion2 = playerRegion;
				}
			}
			if (scenarioPlayerRegion != null && scenarioPlayerRegion2 != null)
			{
				int num4 = Mathf.Max(15, Mathf.RoundToInt((float)num3 * 0.1f));
				if (num3 - num2 > num4 && TryAddBalancingVillage(p_context, scenarioPlayerRegion))
				{
					Utils.UpdateTilesData(p_nonDestructive: true);
					continue;
				}
				break;
			}
			break;
		}
	}

	protected virtual int GetRegionIncome(ScenarioGenerationContext p_context, ScenarioPlayerRegion p_region)
	{
		int num = 0;
		Tile[,] tiles = p_context.Tiles;
		foreach (Tile tile in tiles)
		{
			if (tile.IsVictoryPoint && tile.OwnerName == p_region.Player.Name)
			{
				num += tile.MoneyProductionPerTurn;
			}
		}
		return num;
	}

	protected virtual bool TryAddBalancingVillage(ScenarioGenerationContext p_context, ScenarioPlayerRegion p_region)
	{
		Tile tile = FindBestOwnedCandidate(p_context, p_region, ScenarioSettlementTier.Village);
		if (tile == null)
		{
			return false;
		}
		p_region.Settlements.Add(PlaceSettlementCluster(p_context, tile, ScenarioSettlementTier.Village, p_region.Index, p_isCapital: false));
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Policies
{
	public enum Type
	{
		Efficient_workers,
		Minimized_destruction,
		Magnetic_mines,
		Improved_radars,
		Infantry_focus,
		Armoured_focus,
		Air_focus,
		Guns_focus,
		Naval_focus,
		Recruitment_boost_infantry,
		Recruitment_boost_armoured,
		Recruitment_boost_guns,
		Recruitment_boost_air,
		Recruitment_boost_naval,
		Efficient_modernization,
		Blitzkrieg,
		Shore_bombardment,
		Winter_trained,
		All_weather,
		Organized_supplyLines,
		No_step_back,
		Wolfpack,
		Air_supremacy,
		Artillery_barrage,
		Organized_defenses,
		Good_diplomats,
		Improved_HQ,
		Careful_planning,
		Improved_hospitals,
		Cheap_supplies,
		Better_scientists,
		Lessons_of_war,
		War_experience,
		Superior_bombing_protection,
		Dug_deep,
		Urban_tactics,
		Desert_rats,
		Forest_tactics,
		Winter_soldiers,
		Camouflage,
		Human_wave,
		Magnetic_grenades,
		Armoured_skirts,
		Landing_expert,
		Conscription1,
		Conscription2,
		Conscription3,
		Balanced_Approach,
		Aggressive_Approach,
		Secret_Police,
		Forced_labor,
		Scorched_earth,
		Early_mobilization,
		Partial_mobilization,
		War_economy,
		Total_mobilization,
		ImportFocus,
		Improved_AP_shells,
		Fleet_in_being,
		Army_training,
		Industrial_overtime,
		Aggressive_propaganda,
		Construction_engineering,
		Resistance_suppresion,
		Military_scientists_1,
		Military_scientists_2,
		Bigger_command_centers,
		Commanders_development_program,
		Highlands_experts,
		Mutual_Support_Pact,
		Strategic_Flexibility
	}

	public enum Categories
	{
		Doctrines,
		Units_production,
		Infrastructures,
		Military_tactics,
		Conscription,
		Occupation,
		Economy,
		Field_modifications,
		National_focuses
	}

	private static List<Policy> _policies;

	public static List<Policy> GetAllPolicies()
	{
		if (_policies == null)
		{
			InitializePolicies();
		}
		return _policies;
	}

	public static void InitializePolicies()
	{
		_policies = new List<Policy>();
		GeneratePolicies();
		_policies = _policies.OrderBy((Policy x) => x.HighCommandPointsCost).ToList();
	}

	public static void AddPolicy(Policy p_policy)
	{
		_policies.Add(p_policy);
	}

	public static void AddPolicy(string p_name, string p_description, Categories p_category, Type p_type, byte p_highCommandPointsCost)
	{
		AddPolicy(new Policy(p_name, p_description, p_category, p_type, p_highCommandPointsCost));
	}

	public static Policy GetPolicy(Type p_type)
	{
		foreach (Policy allPolicy in GetAllPolicies())
		{
			if (allPolicy.Type == p_type)
			{
				return allPolicy;
			}
		}
		Debug.LogError("Returning null for requested policy: " + p_type);
		return null;
	}

	public static Sprite GetPolicyIcon(Policy p_policy)
	{
		return Resources.Load<Sprite>("UI/Policies/" + p_policy.Name);
	}

	public static void GeneratePolicies()
	{
		DateTime date = GameData.Instance.map.Date;
		AddPolicy("Efficient workers", "+50% repair speed for destroyed tiles.", Categories.Infrastructures, Type.Efficient_workers, 30);
		AddPolicy("Minimized destruction", "+50% more income from destroyed tiles.", Categories.Infrastructures, Type.Minimized_destruction, 40);
		if (date.Year >= 1914)
		{
			AddPolicy("Magnetic mines", "+15% damage to naval mines.", Categories.Infrastructures, Type.Magnetic_mines, 20);
		}
		if (date.Year >= 1930)
		{
			AddPolicy("Improved RADARs", "+50% range to all RADARs.", Categories.Infrastructures, Type.Improved_radars, 30);
		}
		AddPolicy("Improved hospitals", "+50% heal rate.", Categories.Infrastructures, Type.Improved_hospitals, 100);
		AddPolicy("Bigger command centers", "Doubles the limit of HQ points you can have.\n-1 HQ point every turn.", Categories.Infrastructures, Type.Bigger_command_centers, 45);
		AddPolicy("Infantry production focus", "-25% infantry units cost.", Categories.Units_production, Type.Infantry_focus, 40);
		AddPolicy("Armoured production focus", "-25% armoured units cost.", Categories.Units_production, Type.Armoured_focus, 60);
		if (date.Year >= 1900)
		{
			AddPolicy("Air production focus", "-25% air units cost.", Categories.Units_production, Type.Air_focus, 65);
		}
		AddPolicy("Guns production focus", "-25% AA and artillery units cost.", Categories.Units_production, Type.Guns_focus, 50);
		AddPolicy("Naval production focus", "-25% ships units cost.", Categories.Units_production, Type.Naval_focus, 75);
		AddPolicy("Infantry recruitment boost", "-50% infantry recruiting time.", Categories.Units_production, Type.Recruitment_boost_infantry, 70);
		AddPolicy("Armoured recruitment boost", "-50% armoured units building time.", Categories.Units_production, Type.Recruitment_boost_armoured, 80);
		if (date.Year >= 1900)
		{
			AddPolicy("Air recruitment boost", "-50% air units building time.", Categories.Units_production, Type.Recruitment_boost_air, 80);
		}
		AddPolicy("Guns recruitment boost", "-50% AA and artillery building time.", Categories.Units_production, Type.Recruitment_boost_guns, 70);
		AddPolicy("Naval recruitment boost", "-50% ships building time.", Categories.Units_production, Type.Recruitment_boost_naval, 90);
		AddPolicy("Efficient modernization", "-25% upgrade cost.", Categories.Units_production, Type.Efficient_modernization, 50);
		AddPolicy("Import focus", "Cancels the price penalty when purchasing from other nations.", Categories.Units_production, Type.ImportFocus, 50);
		if (date.Year >= 1914)
		{
			AddPolicy("Blitzkrieg", "On the offensive only: \nTanks and mechanized units: +20% damage. \nCAS planes: +20% damage. \n+100% cost to ammo and fuel supplies for tanks, mechanized/motorized units, CAS planes.", Categories.Doctrines, Type.Blitzkrieg, 100);
		}
		AddPolicy("Shore bombardment", "+20% damage to all ships against ground targets.", Categories.Doctrines, Type.Shore_bombardment, 50);
		AddPolicy("Artillery barrage", "+20% damage to all artillery units. \n+50% dealt attrition. \n+100% cost to ammo for artillery supplies.", Categories.Doctrines, Type.Artillery_barrage, 100);
		AddPolicy("Organized defense", "-20% damage to all units attacking you.", Categories.Doctrines, Type.Organized_defenses, 100);
		if (date.Year >= 1900)
		{
			AddPolicy("All weather", "Air units can perform attacks during bad weather with a -50% damage penalty.", Categories.Doctrines, Type.All_weather, 90);
		}
		AddPolicy("Organized supply lines", "+50% supplies will reach the unit.", Categories.Doctrines, Type.Organized_supplyLines, 80);
		if (date.Year >= 1900)
		{
			AddPolicy("Air supremacy", "+20% damage to all air units.", Categories.Doctrines, Type.Air_supremacy, 70);
		}
		AddPolicy("Good diplomats", "-30% cost to perform diplomatic actions.", Categories.Doctrines, Type.Good_diplomats, 65);
		AddPolicy("Careful planning", "+1 movement action to all ground units.", Categories.Doctrines, Type.Careful_planning, 90);
		AddPolicy("Cheap supplies", "-50% supplies cost.", Categories.Doctrines, Type.Cheap_supplies, 80);
		if (PlayerSettings.Instance.TechTree)
		{
			AddPolicy("Better scientists", "-30% research time.", Categories.Doctrines, Type.Better_scientists, 75);
		}
		AddPolicy("Lessons of war", "+1 XP every time a unit earns XP.", Categories.Doctrines, Type.Lessons_of_war, 60);
		AddPolicy("War experience", "Heroes can be unlocked one level earlier.", Categories.Doctrines, Type.War_experience, 50);
		if (date.Year >= 1900 && PlayerSettings.Instance.Upkeep)
		{
			AddPolicy("Superior bombing protection", "+30% damage to planes with anti air.", Categories.Doctrines, Type.Superior_bombing_protection, 60);
		}
		AddPolicy("Fleet in being", "+25% damage in naval combat.\n+25% cost increase when repairing naval units.", Categories.Doctrines, Type.Fleet_in_being, 65);
		AddPolicy("Strategic Flexibility", "You will be refunded 30% of the spent HQ points whenever you deactivate a policy.\n-1 HQ point every turn.", Categories.Doctrines, Type.Strategic_Flexibility, 50);
		AddPolicy("Landing experts", "+30% damage when landing.", Categories.Military_tactics, Type.Landing_expert, 45);
		AddPolicy("Highlands experts", "+30% damage to ground units when fighting in mountain or hill terrain.\nMountaineer option is 50% more expensive.\n+100% training time for mountaineer units.", Categories.Military_tactics, Type.Highlands_experts, 35);
		AddPolicy("Urban tactics", "+30% damage to ground units when fighting in urban tiles.", Categories.Military_tactics, Type.Urban_tactics, 50);
		AddPolicy("Desert rats", "+30% damage to ground units when fighting in desert tiles.", Categories.Military_tactics, Type.Desert_rats, 50);
		AddPolicy("Forest tactics", "+30% damage to ground units when fighting in forest tiles.", Categories.Military_tactics, Type.Forest_tactics, 75);
		AddPolicy("Winter soldiers", "+30% damage to ground units when fighting in snowy tiles.", Categories.Military_tactics, Type.Winter_soldiers, 75);
		AddPolicy("Scorched earth", "When an enemy captures one of your victory points, \nit is immediately burned down and unusable for 3 turns.", Categories.Military_tactics, Type.Scorched_earth, 75);
		AddPolicy("No step back", "Units will not retreat anymore.", Categories.Military_tactics, Type.No_step_back, 50);
		if (date.Year >= 1900)
		{
			AddPolicy("Wolfpack", "+30% damage to all submarines against naval targets.", Categories.Military_tactics, Type.Wolfpack, 50);
		}
		AddPolicy("Human wave", "Infantry: +30% damage if the unit's level is inferior to 3.", Categories.Military_tactics, Type.Human_wave, 60);
		AddPolicy("Early mobilization", "+5% income. \n-5% manpower from villages. \n-5% research speed.", Categories.Economy, Type.Early_mobilization, 45);
		AddPolicy("Partial mobilization", "+10% income. \n-10% manpower from villages. \n-10% research speed.", Categories.Economy, Type.Partial_mobilization, 60);
		AddPolicy("War economy", "+15% income. \n-15% manpower from villages. \n-15% research speed.", Categories.Economy, Type.War_economy, 80);
		AddPolicy("Total mobilization", "+25% income. \n-25% manpower from villages. \n-25% research speed.", Categories.Economy, Type.Total_mobilization, 100);
		if (PlayerSettings.Instance.Manpower)
		{
			AddPolicy("War conscription", "+10% recruitable population. \n-5% damage dealt by units that are level 1.", Categories.Conscription, Type.Conscription1, 50);
			AddPolicy("Forced conscription", "+20% recruitable population. \n-10% damage dealt by units that are level 1.", Categories.Conscription, Type.Conscription2, 75);
			AddPolicy("Scraping the barrel", "+30% recruitable population. \n-15% damage dealt by units that are level 1.", Categories.Conscription, Type.Conscription3, 100);
		}
		if (PlayerSettings.Instance.Partisans)
		{
			AddPolicy("Balanced approach", "-15% partisans spawning rate. \n+15% recruitable population in occupied territories. \n+15% production from captured territories.", Categories.Occupation, Type.Balanced_Approach, 50);
			AddPolicy("Aggressive approach", "-30% partisans spawning rate. \n+8% recruitable population in occupied territories. \n+8% production from captured territories.", Categories.Occupation, Type.Aggressive_Approach, 75);
			AddPolicy("Secret police", "-50% partisans spawning rate. \n+5% recruitable population in occupied territories. \n+5% production from captured territories.", Categories.Occupation, Type.Secret_Police, 90);
			AddPolicy("Forced labor", "+30% partisans spawning rate. \n+5% recruitable population in occupied territories. \n+30% production from captured territories.", Categories.Occupation, Type.Forced_labor, 100);
		}
		if (date.Year >= 1943)
		{
			AddPolicy("Armoured skirts", "-30% damage from infantry to your armoured units.\n +5% price increase on purchase of armoured units.", Categories.Field_modifications, Type.Armoured_skirts, 50);
		}
		if (date.Year >= 1900)
		{
			AddPolicy("Improved AP shells", "+30% damage for all anti tank guns against armoured targets. \n+300% to ammunition cost for anti tank guns.", Categories.Field_modifications, Type.Improved_AP_shells, 50);
		}
		if (date.Year >= 1942)
		{
			AddPolicy("Magnetic grenades", "+30% damage from infantry to armoured units.", Categories.Field_modifications, Type.Magnetic_grenades, 50);
		}
		AddPolicy("Camouflage", "-10% damage when attacked. \n+30% damage when ambushing enemy units.", Categories.Field_modifications, Type.Camouflage, 70);
		AddPolicy("Winter equipment", "Cancels winter attrition of all ground units.", Categories.Field_modifications, Type.Winter_trained, 100);
		AddPolicy("Better shovels", "+1 to max entrenchment.", Categories.Field_modifications, Type.Dug_deep, 75);
		AddPolicy("Improved HQ", "+30% range to generals.", Categories.Field_modifications, Type.Improved_HQ, 65);
		AddPolicy("Army training", "All newly purchased units will start with one extra level (except veterans and buildings).\n+10% unit purchase cost.\n-2 HQ point every turn.", Categories.National_focuses, Type.Army_training, 85);
		AddPolicy("Industrial overtime", "+35% income output from industrial tiles.\n-40% income output from villages.", Categories.National_focuses, Type.Industrial_overtime, 85);
		if (PlayerSettings.Instance.Manpower)
		{
			AddPolicy("Aggressive propaganda", "+30% monthly population growth.\n-1 HQ point every turn.", Categories.National_focuses, Type.Aggressive_propaganda, 65);
		}
		if (PlayerSettings.Instance.ProductionTakesTime)
		{
			AddPolicy("Construction engineering", "+50% construction speed for all buildings.\n+5% unit purchase cost.\n-1 HQ point every turn.", Categories.National_focuses, Type.Construction_engineering, 50);
		}
		if (PlayerSettings.Instance.Partisans)
		{
			AddPolicy("Resistance suppression", "-50% partisans spawn rate.\n-2 HQ point every turn.", Categories.National_focuses, Type.Resistance_suppresion, 75);
		}
		if (PlayerSettings.Instance.TechTree)
		{
			AddPolicy("Military scientists x1", "+1 research point.\n-1 HQ point every turn.", Categories.National_focuses, Type.Military_scientists_1, 60);
		}
		if (PlayerSettings.Instance.TechTree)
		{
			AddPolicy("Military scientists x2", "+2 research point.\n-2 HQ point every turn.", Categories.National_focuses, Type.Military_scientists_2, 80);
		}
		if (PlayerSettings.Instance.Upkeep)
		{
			AddPolicy("Commanders development program", "+3 HQ points every turn.\n+100% production time for commanders.\n+50% upkeep for commanders.", Categories.National_focuses, Type.Commanders_development_program, 100);
		}
		AddPolicy("Mutual Support Pact", "Possibility to deploy your units in allied territory.\n-3 HQ points per turn.", Categories.National_focuses, Type.Mutual_Support_Pact, 75);
	}
}

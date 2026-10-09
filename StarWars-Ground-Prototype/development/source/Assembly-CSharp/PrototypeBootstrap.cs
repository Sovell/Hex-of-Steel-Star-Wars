using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

// Local prototype startup, content definitions and explicit runtime smoke checks.
public class PrototypeBootstrap : MonoBehaviour
{
    public static bool IsIsolatedProfile => true;
    public const string Empire = "Imperium Galaktyczne";
    public const string Rebels = "Sojusz Rebeliantow";
    public const string ScenarioFile = "SW - Placowka przekaznikowa";
    private static PrototypeBootstrap _instance;
    private static bool _autoLaunchConsumed;
    private static readonly Dictionary<string, Sprite> _unitSprites = new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, Sprite> _flags = new Dictionary<string, Sprite>();
    private bool _failed;
    private static bool HasArgument(string arg) => Environment.GetCommandLineArgs().Contains(arg);
    private static string ScenarioPath => Path.Combine(PrototypePaths.DataPath, "Maps", ScenarioFile + ".sav");

    public static void Begin()
    {
        if (_instance == null)
        {
            var obj = new GameObject("Star Wars ground prototype");
            DontDestroyOnLoad(obj);
            _instance = obj.AddComponent<PrototypeBootstrap>();
            Application.logMessageReceived += _instance.RecordException;
        }
        _instance.StartCoroutine(_instance.BootMenu());
    }

    private void RecordException(string message, string stack, LogType type)
    {
        if (type != LogType.Exception) return;
        _failed = true;
        File.AppendAllText(Path.Combine(PrototypePaths.Root, "Logs", "runtime-exceptions.log"), message + "\n" + stack + "\n");
    }

    private IEnumerator BootMenu()
    {
        yield return null;
        yield return null;
        try
        {
            foreach (string dir in new[] { "Maps", "Saves", "MODS", "Screenshots" })
                Directory.CreateDirectory(Path.Combine(PrototypePaths.DataPath, dir));
            Directory.CreateDirectory(Path.Combine(PrototypePaths.Root, "Logs"));
            Require(Application.companyName == "Local Prototypes" && Application.productName == "SW Prototype", "Native Unity identity isolated");
            ConfigureSettings();
            RegisterDefinitions();
            if (!File.Exists(ScenarioPath) || HasArgument("-prototype-regenerate")) CreateScenario();
            Debug.Log("[SW-PROTOTYPE] menu ready; profile=" + PrototypePaths.DataPath);
            if (HasArgument("-prototype-menu") || _autoLaunchConsumed) yield break;
            _autoLaunchConsumed = true;
            GameData.Instance.Load(ScenarioPath);
            MainMenu.selectedMap = new FileInfo(ScenarioPath);
            MainMenu.isLoadedGame = false;
            MainMenu.isEditor = false;
            MainMenu.isMainMenu = false;
            GameData.Instance.GenerateGameSettings(true);
            GameData.Instance.ironManMode = false;
            SceneManager.LoadScene("Game");
        }
        catch (Exception ex)
        {
            Fail(ex.ToString());
            yield break;
        }
        yield return new WaitForSecondsRealtime(3f);
        try
        {
            Require(MapGO.instance != null && TurnManager.currPlayer != null, "Game scene initialized");
            Require(GameData.Instance.map.SizeX == 12 && GameData.Instance.map.SizeY == 9, "Map size 12x9");
            Require(GameData.Instance.listOfPlayers.Sum(p => p.ListOfUnits.Count) == 11, "11 deployed units");
            Require(GameData.Instance.listOfPlayers.SelectMany(p => p.ListOfUnits).All(u => u.unitGO != null), "All units have scene objects");
            Require(GameData.Instance.listOfPlayers.SelectMany(p => p.ListOfUnits).All(u => Utils.GetUnitSprite(u) != null), "All unit sprites available");
            Require(!SteamManager.IsSteamReady, "Steam integration disabled for isolated prototype");
            MapGO.instance.StartScenario();
            // The stock camera's default zoom is too close for a 12x9 prototype map.
            CameraGO.instance.targetZoom = 28f;
            CameraGO.instance.cam.orthographicSize = 28f;
            CameraGO.instance.transform.position = new Vector3(24.31f, 21.675f, -80f);
            GameData.Instance.listOfPlayers[0].ListOfUnits.First(u => u.Name == "AT-ST patrol").unitGO.SelectUnit(false);
            Debug.Log("[SW-PROTOTYPE] SCENARIO_READY");
        }
        catch (Exception ex) { Fail(ex.ToString()); }
        if (HasArgument("-prototype-smoke"))
        {
            yield return StartCoroutine(SmokeCheck());
            yield break;
        }
        if (HasArgument("-prototype-capture"))
        {
            yield return new WaitForSecondsRealtime(2f);
            yield return new WaitForEndOfFrame();
            try
            {
                var image = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(PrototypePaths.Root, "Logs", "scenario-preview.png"), image.EncodeToPNG());
                Destroy(image);
                Debug.Log("[SW-PROTOTYPE] PREVIEW_CAPTURED");
            }
            catch (Exception ex) { Fail(ex.ToString()); }
            if (HasArgument("-prototype-preview")) Application.Quit(_failed ? 1 : 0);
        }
    }

    private static void ConfigureSettings()
    {
        var settings = PlayerSettings.Instance;
        settings.PublicChatOFF = true;
        settings.ISHistoricalFocus = false;
        settings.IsWeather = false;
        settings.ShowClouds = false;
        settings.Partisans = false;
        settings.TechTree = false;
        settings.Policies = false;
        settings.Manpower = false;
        settings.FiniteManpower = false;
        settings.Upkeep = false;
        settings.ComplexSupplies = false;
        settings.IsFogOfWar = false;
        settings.IsMorale = false;
        settings.IsInfiniteAmmoAndFuel = false;
        settings.UseNATObyDefault = false;
        settings.IsQuickMovement = true;
        settings.IsAISlowMode = false;
        settings.FastAI = true;
        settings.RandomDamageModifier = 0;
        settings.MaxFramerate = 60;
        settings.UIScale = Mathf.Clamp((float)Screen.width / 1920f, 0.75f, 1.25f);
        settings.MoveCameraWithMouseOnEdge = false;
        settings.MusicVolume = 0.15f;
        settings.AutosavesFrequency = 1;
        settings.ActiveMods = new List<string>();
        PlayerSettings.Save();
    }

    private static void RegisterDefinitions()
    {
        var originals = OfficialUnitsORIGINAL.GetInstance().Units;
        var definitions = OfficialUnits.GetInstance().Units;
        var infantry = originals.First(u => u.FilterType == "FilterInfantry" && u.MaxMP > 0 && u.MaxHP > 0 && u.Country == "USA" && !u.IsPartisans && !u.IsCommander);
        var tank = originals.Where(u => u.FilterType == "FilterTank" && u.MaxMP > 0 && u.Country == "USA" && u.ProductionYear <= 1943).OrderBy(u => u.Cost).First();
        var artillery = originals.First(u => u.FilterType == "FilterArtillery" && u.MaxHP > 0 && u.MaxMP > 0 && u.Country == "USA");
        var engineer = originals.FirstOrDefault(u => u.IsEngineers && u.Country == "USA") ?? infantry;
        AddDefinition(definitions, infantry, Empire, "Stormtrooper squad", 100, 4, 36, 12, 3, 1, 110);
        AddDefinition(definitions, engineer, Empire, "Imperial combat engineers", 100, 4, 30, 14, 3, 1, 120);
        AddDefinition(definitions, tank, Empire, "AT-ST patrol", 120, 5, 42, 34, 18, 1, 210);
        AddDefinition(definitions, artillery, Empire, "E-Web support battery", 90, 3, 32, 18, 2, 2, 150);
        AddDefinition(definitions, infantry, Rebels, "Rebel trooper squad", 100, 4, 34, 16, 2, 1, 105);
        AddDefinition(definitions, tank, Rebels, "Alliance scout tank", 100, 5, 34, 40, 12, 1, 180);
        AddDefinition(definitions, artillery, Rebels, "Rebel blaster battery", 90, 3, 30, 22, 2, 2, 145);
        foreach (Unit definition in definitions.Where(u => u.Country == Empire || u.Country == Rebels))
            Debug.Log("[SW-PROTOTYPE] definition " + definition.Country + " / " + definition.Name);
        File.WriteAllLines(Path.Combine(PrototypePaths.Root, "Logs", "prototype-units.tsv"),
            new[] { "Faction\tUnit\tHP\tMP\tSoft\tHard\tArmour\tRange" }.Concat(definitions.Where(u => u.Country == Empire || u.Country == Rebels).Select(u =>
                u.Country + "\t" + u.Name + "\t" + u.MaxHP + "\t" + u.MaxMP + "\t" + u.SoftDamage + "\t" + u.HardDamage + "\t" + u.Armour + "\t" + u.Range)));
    }

    private static void AddDefinition(List<Unit> definitions, Unit template, string faction, string name, int hp, byte mp, short soft, short hard, short armour, byte range, int cost)
    {
        string key = faction + "/" + name;
        if (!_unitSprites.ContainsKey(key)) _unitSprites[key] = Utils.GetUnitSprite(template);
        var u = template.Clone(true);
        u.Name = name; u.Country = faction; u.ForcedCustomName = name; u.CustomName = name; u.nameToLower = name.ToLowerInvariant();
        u.MaxHP = u.CurrHP = u.BaseMaxHP = hp;
        u.MaxMP = u.CurrMP = u.BaseMaxMP = mp;
        u.SoftDamage = u.BaseSoftDamage = soft; u.HardDamage = u.BaseHardDamage = hard;
        u.AirDamage = u.BaseAirDamage = 0; u.Armour = u.BaseArmour = armour; u.Range = u.BaseRange = range;
        u.MaxAmmo = u.CurrAmmo = u.BaseMaxAmmo = 12;
        u.MaxAutonomy = u.CurrAutonomy = u.BaseMaxAutonomy = 100;
        u.Visibility = u.BaseVisibility = 4; u.Cost = u.CustomCost = cost;
        u.ProductionYear = 2000; u.ProductionMonth = 1; u.EndUseYear = 3000;
        u.Lvl = 1; u.XP = 0; u.ActionPoints = 2; u.IsReserve = false; u.IsDead = false;
        u.IsCoreUnit = false; u.IsPoliticalUnit = false; u.IsPrototype = false; u.IsDefensive = false;
        u.BaseType = u.Type; u.Morale = Unit.MORALE_DEFAULT;
        u.Waypoints = new List<int[]>();
        definitions.RemoveAll(d => d.Country == faction && d.Name == name);
        definitions.Add(u);
    }

    public static bool TryGetUnitSprite(Unit unit, out Sprite sprite)
    {
        sprite = null;
        return unit != null && _unitSprites.TryGetValue(unit.Country + "/" + unit.Name, out sprite) && sprite != null;
    }

    public static bool TryGetFlag(string country, out Sprite sprite)
    {
        sprite = null;
        if (country != Empire && country != Rebels) return false;
        if (_flags.TryGetValue(country, out sprite)) return true;
        // Simple prototype faction markers, not finished franchise artwork.
        var texture = new Texture2D(48, 32, TextureFormat.RGBA32, false);
        var color = country == Empire ? new Color(0.7f, 0.15f, 0.13f) : new Color(0.16f, 0.4f, 0.8f);
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 48; x++)
        {
            float dx = x - 24f, dy = y - 16f;
            float radius = Mathf.Sqrt(dx * dx + dy * dy);
            bool ring = radius >= 9f && radius <= 11f;
            bool spokes = radius <= 9f && (Math.Abs(dx) <= 1f || Math.Abs(dy) <= 1f || Math.Abs(dx - dy) <= 1.5f || Math.Abs(dx + dy) <= 1.5f);
            bool mark = country == Empire ? ring || spokes : Math.Abs(x - 24) + Math.Abs(y - 16) < 10;
            texture.SetPixel(x, y, mark ? Color.white : color);
        }
        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0, 0, 48, 32), new Vector2(0.5f, 0.5f), 16f);
        _flags[country] = sprite;
        return true;
    }

    private static Tile NewTile(string type, string owner)
    {
        var template = OfficialTiles.GetInstance().Tiles.First(t => t.Type == type && t.Season == "Summer");
        var tile = new Tile();
        foreach (PropertyInfo property in typeof(Tile).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            if (property.CanWrite && property.Name != "GroundUnit" && property.Name != "AirUnit") property.SetValue(tile, property.GetValue(template, null), null);
        tile.OwnerName = owner; tile.OriginalCountryOwner = owner; tile.CityName = "";
        tile.IsVictoryPoint = false; tile.PrimaryObjective = false; tile.SecondaryObjective = false;
        tile.IsReinforcementTile = false; tile.IsOfficial = false; tile.Capturable = true;
        tile.GroundUnit = null; tile.AirUnit = null;
        tile.MoneyProductionPerTurn = 0; tile.OccupiedMoneyProductionPerTurn = 0;
        return tile;
    }

    private static void CreateScenario()
    {
        var empire = new Player(Empire) { Country = Empire, Faction = "Empire", Ideology = "None", IsComputer = false, IsPlayingFirst = true, MoneyAmount = 400, PlayersAtWar = new HashSet<string> { Rebels }, AlliedPlayers = new HashSet<string> { Empire } };
        var rebels = new Player(Rebels) { Country = Rebels, Faction = "Alliance", Ideology = "None", IsComputer = true, MoneyAmount = 400, PlayersAtWar = new HashSet<string> { Empire }, AlliedPlayers = new HashSet<string> { Rebels } };
        GameData.ResetForScenarioCreation(new List<Player> { empire, rebels });
        var map = new Map(12, 9) {
            ScenarioName = "Placowka przekaznikowa - prototyp SW", ScenarioDescription = "Imperium kontra Rebelia. Zdobadz glowna baze przeciwnika i utrzymaj wlasna. Centrum mapy to przekaznik. 11 oddzialow; bez lotnictwa, badan i zdarzen historycznych. Grafika jednostek jest tymczasowa.",
            ScenarioVictoryText = "Placowka zabezpieczona.", ScenarioDefeatText = "Baza utracona.",
            Date = new DateTime(2000, 6, 1), NumberOfDaysThatGoesByPerTurn = 1,
            FileName = ScenarioFile, turnCount = 1, IsOfficial = false, IsCampaign = false,
            DisableWeather = true, DisableResearch = true, DisableManpower = true, DisableUpkeep = true,
            NumberOfTurnsToWin = 0, CircularMap = false
        };
        GameData.Instance.map = map;
        for (int x = 0; x < map.SizeX; x++)
        for (int y = 0; y < map.SizeY; y++)
        {
            string owner = x <= 5 ? Empire : Rebels;
            string type = (x == 5 || x == 6) && (y <= 2 || y >= 6) ? "Hill" : "Plain";
            map.TilesTable[x, y] = NewTile(type, owner);
        }
        SetObjective(map, 1, 4, Empire, "Imperialny punkt dowodzenia", true);
        SetObjective(map, 10, 4, Rebels, "Baza Sojuszu", true);
        SetObjective(map, 6, 4, Rebels, "Przekaznik", false);
        Place(empire, "Stormtrooper squad", 2, 3); Place(empire, "Stormtrooper squad", 2, 5); Place(empire, "Stormtrooper squad", 3, 4);
        Place(empire, "AT-ST patrol", 4, 4); Place(empire, "E-Web support battery", 2, 4); Place(empire, "Imperial combat engineers", 1, 5);
        Place(rebels, "Rebel trooper squad", 8, 3); Place(rebels, "Rebel trooper squad", 8, 5); Place(rebels, "Rebel trooper squad", 7, 4);
        Place(rebels, "Alliance scout tank", 5, 4); Place(rebels, "Rebel blaster battery", 9, 4);
        GameData.Instance.GenerateGameSettings(true);
        GameData.Instance.SaveAsScenarioFile(ScenarioFile);
        GameData.Instance.Load(ScenarioPath);
        Require(GameData.Instance.map.TilesTable[4, 4].GroundUnit.Name == "AT-ST patrol", "Scenario serialization round trip");
        Debug.Log("[SW-PROTOTYPE] SCENARIO_CREATED " + ScenarioPath);
    }

    private static void SetObjective(Map map, int x, int y, string owner, string name, bool primary)
    {
        var tile = NewTile("City", owner);
        tile.CityName = name; tile.IsVictoryPoint = true; tile.PrimaryObjective = primary; tile.SecondaryObjective = !primary;
        tile.MoneyProductionPerTurn = 5; tile.OccupiedMoneyProductionPerTurn = 5; tile.IsReinforcementTile = true;
        map.TilesTable[x, y] = tile;
    }

    private static void Place(Player owner, string name, int x, int y)
    {
        var template = OfficialUnits.GetInstance().Units.Single(u => u.Name == name && u.Country == owner.Country);
        var unit = template.Clone(true);
        unit.OwnerName = owner.Name; unit.OwnerPlayer = owner; unit.IsReserve = false;
        unit.ForcedCustomName = name; unit.CurrHP = unit.MaxHP; unit.CurrMP = unit.MaxMP;
        unit.CurrAmmo = unit.MaxAmmo; unit.CurrAutonomy = unit.MaxAutonomy; unit.ActionPoints = 2;
        owner.ListOfUnits.Add(unit);
        GameData.Instance.map.TilesTable[x, y].GroundUnit = unit;
    }

    private IEnumerator SmokeCheck()
    {
        Unit mover = null;
        Tile destination = null;
        try
        {
            Require(!_failed, "No startup exceptions");
            mover = GameData.Instance.listOfPlayers[0].ListOfUnits.First(u => u.Name == "Stormtrooper squad");
            destination = mover.unitGO.tileGO.tile.neighbours.First(t => t.GroundUnit == null && t.enumType == TileTypes.PLAIN);
            mover.unitGO.SelectUnit(false);
            mover.unitGO.ShowPotentialPaths();
            mover.unitGO.GetPathToTile(destination);
            Require(mover.unitGO.currentPath.Count > 0, "Pathfinding found movement path");
            mover.unitGO.MoveUnitToTile(destination.tileGO);
        }
        catch (Exception ex) { Fail(ex.ToString()); }
        yield return new WaitForSecondsRealtime(2f);
        try
        {
            Require(mover != null && mover.unitGO.tileGO.tile == destination, "Unit movement completed");
            var attacker = GameData.Instance.listOfPlayers[0].ListOfUnits.First(u => u.Name == "AT-ST patrol");
            var defender = GameData.Instance.listOfPlayers[1].ListOfUnits.First(u => u.Name == "Alliance scout tank");
            int before = defender.CurrHP;
            Require(attacker.GetPotentialDamage(defender.unitGO, false, false) > 0, "Damage calculator produces damage");
            attacker.unitGO.targetedUnitGO = defender.unitGO;
            attacker.unitGO.AttackUnit(defender.unitGO, true, true, false);
            Require(defender.CurrHP < before, "Attack applies damage");
        }
        catch (Exception ex) { Fail(ex.ToString()); }
        yield return new WaitForSecondsRealtime(2f);
        try
        {
            GameData.Instance.SaveAsSaveFile("Prototype - smoke check");
            var saved = (GameData)Utils.ConvertByteArrayToObject(File.ReadAllBytes(Path.Combine(PrototypePaths.DataPath, "Saves", "Prototype - smoke check.sav")));
            Require(saved.map.SizeX == 12 && saved.listOfPlayers.Count == 2, "Save readback succeeds");
            TurnManager.instance.NextTurn(false);
        }
        catch (Exception ex) { Fail(ex.ToString()); }
        yield return new WaitForSecondsRealtime(8f);
        try
        {
            Require(!_failed, "No runtime exceptions during move/attack/turn");
            Require(GameData.Instance.map.turnCount >= 2, "AI completed its turn");
            File.WriteAllText(Path.Combine(PrototypePaths.Root, "Logs", "smoke-result.txt"), "PASS: scene, sprites, isolated profile, pathfinding, movement, attack, save readback, AI turn.\n");
            Debug.Log("[SW-PROTOTYPE] SMOKE_PASS");
        }
        catch (Exception ex) { Fail(ex.ToString()); }
        Application.Quit(_failed ? 1 : 0);
    }

    private static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
        Debug.Log("[SW-PROTOTYPE] CHECK " + label);
    }

    private void Fail(string message)
    {
        _failed = true;
        Debug.LogError("[SW-PROTOTYPE] FAIL " + message);
        File.WriteAllText(Path.Combine(PrototypePaths.Root, "Logs", "smoke-result.txt"), "FAIL: " + message);
        if (HasArgument("-prototype-smoke")) Application.Quit(1);
    }
}

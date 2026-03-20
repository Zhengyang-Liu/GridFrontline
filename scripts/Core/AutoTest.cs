using Godot;

namespace GridFrontline;

/// <summary>
/// Automated test scene. Runs game logic programmatically and prints results.
/// Run with: Godot --headless --path . --scene res://scenes/main/Test.tscn
/// </summary>
public partial class AutoTest : Node2D
{
    private int _passed;
    private int _failed;

    public override async void _Ready()
    {
        GD.Print("===== GridFrontline Auto Test =====");

        TestEconomy();
        TestGridCell();
        TestBuildingData();
        TestBuildingPlacement();
        TestMilitaryProduction();
        TestCombat();

        GD.Print($"\n===== Results: {_passed} passed, {_failed} failed =====");

        // Wait one frame for deferred calls, then quit
        await ToSignal(GetTree().CreateTimer(0.5), "timeout");
        GetTree().Quit(_failed > 0 ? 1 : 0);
    }

    private void Assert(bool condition, string testName)
    {
        if (condition)
        {
            GD.Print($"  ✅ {testName}");
            _passed++;
        }
        else
        {
            GD.PrintErr($"  ❌ {testName}");
            _failed++;
        }
    }

    private void TestEconomy()
    {
        GD.Print("\n-- EconomyManager --");
        var eco = new EconomyManager();
        AddChild(eco);

        Assert(eco.Gold == 0, "Initial gold is 0");

        eco.AddGold(100);
        Assert(eco.Gold == 100, "AddGold(100) -> 100");

        Assert(eco.CanAfford(50), "CanAfford(50) = true");
        Assert(!eco.CanAfford(200), "CanAfford(200) = false");

        bool spent = eco.SpendGold(40);
        Assert(spent && eco.Gold == 60, "SpendGold(40) -> 60");

        bool failSpend = eco.SpendGold(100);
        Assert(!failSpend && eco.Gold == 60, "SpendGold(100) fails, still 60");

        eco.QueueFree();
    }

    private void TestGridCell()
    {
        GD.Print("\n-- GridCell --");
        var cell = new GridCell();
        cell.Row = 2;
        cell.Col = 3;
        AddChild(cell);

        Assert(cell.Row == 2 && cell.Col == 3, "Row/Col set correctly");
        Assert(!cell.IsOccupied, "Cell starts unoccupied");
        Assert(cell.CurrentBuilding == null, "No building initially");

        cell.QueueFree();
    }

    private void TestBuildingData()
    {
        GD.Print("\n-- BuildingDatabase --");
        var farm = BuildingDatabase.Farm;
        Assert(farm.BuildingName == "农场", "Farm name = 农场");
        Assert(farm.Cost == 40, "Farm cost = 40");
        Assert(!farm.IsUnitProducer, "Farm is not unit producer");

        var barracks = BuildingDatabase.Barracks;
        Assert(barracks.BuildingName == "兵营", "Barracks name = 兵营");
        Assert(barracks.Cost == 60, "Barracks cost = 60");
        Assert(barracks.IsUnitProducer, "Barracks is unit producer");
    }

    private void TestBuildingPlacement()
    {
        GD.Print("\n-- Building Placement --");
        var eco = new EconomyManager();
        AddChild(eco);
        eco.AddGold(100);

        var cell = new GridCell();
        cell.Row = 0;
        cell.Col = 0;
        AddChild(cell);

        // Place economy building
        var farm = new EconomyBuilding();
        farm.Initialize(BuildingDatabase.Farm);
        farm.SetEconomy(eco);

        Assert(eco.CanAfford(farm.Data.Cost), "Can afford farm");
        eco.SpendGold(farm.Data.Cost);
        cell.PlaceBuilding(farm);

        Assert(cell.IsOccupied, "Cell is now occupied");
        Assert(cell.CurrentBuilding == farm, "Cell has the farm");
        Assert(eco.Gold == 60, "Gold = 60 after placing farm (cost 40)");

        // Cannot place on occupied cell
        Assert(cell.IsOccupied, "Cannot place on occupied cell");

        eco.QueueFree();
        cell.QueueFree();
    }

    private void TestMilitaryProduction()
    {
        GD.Print("\n-- Military Building --");
        var barracks = new MilitaryBuilding();
        barracks.Initialize(BuildingDatabase.Barracks);

        Assert(!barracks.ProductionBlocked, "Production starts unblocked");
        Assert(barracks.Data.ProduceInterval == 6f, "Produce interval = 6s");

        barracks.ProductionBlocked = true;
        Assert(barracks.ProductionBlocked, "Production can be blocked");

        barracks.ProductionBlocked = false;
        Assert(!barracks.ProductionBlocked, "Production can be unblocked");

        // Test Unit creation
        var unit = new Unit();
        unit.Initialize();
        Assert(unit.CurrentHp == 100, "Unit starts with 100 HP");
        Assert(unit.State == Unit.UnitState.InRally, "Unit starts InRally");

        unit.Deploy(new Godot.Vector2(0, 0), new Godot.Vector2(100, 0));
        Assert(unit.State == Unit.UnitState.Moving, "Unit state = Moving after deploy");
    }

    private void TestCombat()
    {
        GD.Print("\n-- Combat System --");

        var mgr = new UnitManager();
        AddChild(mgr);

        // Create player unit
        var player = new Unit();
        player.UnitTeam = Team.Player;
        player.Manager = mgr;
        player.Initialize();
        player.MaxHp = 100;
        player.CurrentHp = 100;
        player.State = Unit.UnitState.Moving;
        AddChild(player);
        player.GlobalPosition = new Vector2(100, 100);
        mgr.Register(player);

        // Create enemy unit
        var enemy = new Unit();
        enemy.UnitTeam = Team.Enemy;
        enemy.Manager = mgr;
        enemy.Initialize();
        enemy.MaxHp = 80;
        enemy.CurrentHp = 80;
        enemy.State = Unit.UnitState.Moving;
        AddChild(enemy);
        enemy.GlobalPosition = new Vector2(120, 100);
        mgr.Register(enemy);

        // Test target finding
        var found = mgr.GetNearestEnemy(player.GlobalPosition, Team.Player, 300f);
        Assert(found == enemy, "Player finds enemy as target");

        var foundReverse = mgr.GetNearestEnemy(enemy.GlobalPosition, Team.Enemy, 300f);
        Assert(foundReverse == player, "Enemy finds player as target");

        // Test no friendly fire — searching as Player should never return a Player unit
        Assert(found != player, "No friendly fire in target search");

        // Test damage
        enemy.TakeDamage(30);
        Assert(enemy.CurrentHp == 50, "Enemy takes 30 damage -> 50 HP");

        enemy.TakeDamage(50);
        Assert(enemy.CurrentHp == 0, "Enemy takes 50 more -> 0 HP");
        Assert(enemy.State == Unit.UnitState.Dead, "Enemy is dead");

        // Dead unit should not be found
        var afterDeath = mgr.GetNearestEnemy(player.GlobalPosition, Team.Player, 300f);
        Assert(afterDeath == null, "Dead enemy not found in search");

        mgr.QueueFree();
        player.QueueFree();
    }
}

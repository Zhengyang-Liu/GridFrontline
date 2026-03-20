using Godot;
using System.Collections.Generic;

namespace GridFrontline;

public enum GameState { Playing, Victory, Defeat }

/// <summary>
/// Central game manager. Root node of Main.tscn.
/// Creates all subsystems and wires up signals.
/// </summary>
public partial class GameManager : Node2D
{
    public EconomyManager Economy { get; private set; }
    public GameBoard Board { get; private set; }
    public RallyZone Rally { get; private set; }
    public UnitManager Units { get; private set; }
    public EnemySpawner Enemy { get; private set; }
    public HUD Hud { get; private set; }
    public BuildPanel Panel { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    // Player base
    public int PlayerBaseHp { get; private set; } = 500;
    public int PlayerBaseMaxHp { get; private set; } = 500;

    private BuildingData _selectedBuilding;
    private const int StartingGold = 100;

    // Player base visual
    private ColorRect _playerBaseHpBar;

    public override void _Ready()
    {
        CreateEconomy();
        CreateUnitManager();
        CreateCamera();
        CreateBoard();
        CreateRallyZone();
        CreateEnemySpawner();
        CreatePlayerBase();
        CreateUI();
        WireSignals();
        Economy.AddGold(StartingGold);
    }

    public override void _Process(double delta)
    {
        if (CurrentState != GameState.Playing) return;

        CheckWinLose();
        Units.CleanupDead();
        CheckPlayerBaseDamage();
    }

    private void CreateEconomy()
    {
        Economy = new EconomyManager();
        Economy.Name = "EconomyManager";
        AddChild(Economy);
    }

    private void CreateUnitManager()
    {
        Units = new UnitManager();
        Units.Name = "UnitManager";
        AddChild(Units);
    }

    private void CreateCamera()
    {
        var camera = new Camera2D();
        camera.Name = "Camera2D";
        // Center camera on the full board (player + rally + corridor + enemy)
        float totalWidth = GameBoard.Cols * GridCell.CellSize
                           + GameBoard.RallyWidth
                           + GameBoard.CorridorWidth
                           + GameBoard.EnemyZoneWidth
                           + GameBoard.GapSize * 3;
        float totalHeight = GameBoard.Rows * GridCell.CellSize;
        camera.Position = new Vector2(
            GameBoard.BoardOriginX + totalWidth / 2f,
            GameBoard.BoardOriginY + totalHeight / 2f
        );
        camera.Zoom = new Vector2(0.85f, 0.85f);
        AddChild(camera);
    }

    private void CreateBoard()
    {
        Board = new GameBoard();
        Board.Name = "GameBoard";
        Board.Position = new Vector2(GameBoard.BoardOriginX, GameBoard.BoardOriginY);
        AddChild(Board);
    }

    private void CreateRallyZone()
    {
        Rally = new RallyZone();
        Rally.Name = "RallyZone";
        float rallyX = GameBoard.BoardOriginX
                       + GameBoard.Cols * GridCell.CellSize
                       + GameBoard.GapSize;
        Rally.Position = new Vector2(rallyX, GameBoard.BoardOriginY);
        Rally.SetGameBoard(Board);
        Rally.SetUnitManager(Units);
        AddChild(Rally);
    }

    private void CreateEnemySpawner()
    {
        Enemy = new EnemySpawner();
        Enemy.Name = "EnemySpawner";
        float enemyX = GameBoard.BoardOriginX
                       + GameBoard.Cols * GridCell.CellSize
                       + GameBoard.GapSize + GameBoard.RallyWidth + GameBoard.GapSize
                       + GameBoard.CorridorWidth + GameBoard.GapSize;
        Enemy.Position = new Vector2(enemyX, GameBoard.BoardOriginY);

        // Phase 2 demo waves
        var waves = new List<EnemyWave>
        {
            new() { StartTime = 10f, UnitType = "soldier", Count = 3, Interval = 3f, Row = -1 },
            new() { StartTime = 30f, UnitType = "soldier", Count = 4, Interval = 2.5f, Row = -1 },
            new() { StartTime = 55f, UnitType = "soldier", Count = 3, Interval = 2f, Row = -1 },
            new() { StartTime = 55f, UnitType = "elite", Count = 1, Interval = 1f, Row = 2 },
            new() { StartTime = 80f, UnitType = "soldier", Count = 5, Interval = 2f, Row = -1 },
            new() { StartTime = 80f, UnitType = "elite", Count = 2, Interval = 4f, Row = -1 },
            new() { StartTime = 110f, UnitType = "elite", Count = 3, Interval = 3f, Row = -1 },
            new() { StartTime = 110f, UnitType = "soldier", Count = 6, Interval = 1.5f, Row = -1 },
        };

        Enemy.Setup(Board, Units, waves);
        AddChild(Enemy);
    }

    private void CreatePlayerBase()
    {
        // Player base visual at the left edge
        float baseX = GameBoard.BoardOriginX - 40;
        float baseY = GameBoard.BoardOriginY + GameBoard.Rows * GridCell.CellSize / 2f - 30;

        var baseRect = new ColorRect();
        baseRect.Size = new Vector2(30, 60);
        baseRect.Position = new Vector2(baseX, baseY);
        baseRect.Color = new Color(0.2f, 0.4f, 0.8f);
        AddChild(baseRect);

        var baseLabel = new Label();
        baseLabel.Text = "基地";
        baseLabel.Position = new Vector2(baseX, baseY - 18);
        baseLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(baseLabel);

        // HP bar
        var hpBg = new ColorRect();
        hpBg.Size = new Vector2(30, 6);
        hpBg.Position = new Vector2(baseX, baseY - 8);
        hpBg.Color = new Color(0.2f, 0.2f, 0.2f);
        AddChild(hpBg);

        _playerBaseHpBar = new ColorRect();
        _playerBaseHpBar.Size = new Vector2(30, 6);
        _playerBaseHpBar.Position = hpBg.Position;
        _playerBaseHpBar.Color = new Color(0.3f, 0.7f, 1f);
        AddChild(_playerBaseHpBar);
    }

    private void CreateUI()
    {
        var uiLayer = new CanvasLayer();
        uiLayer.Name = "UILayer";
        AddChild(uiLayer);

        Hud = new HUD();
        Hud.Name = "HUD";
        Hud.SetEconomy(Economy);
        uiLayer.AddChild(Hud);

        Panel = new BuildPanel();
        Panel.Name = "BuildPanel";
        uiLayer.AddChild(Panel);
    }

    private void WireSignals()
    {
        Board.CellClicked += OnCellClicked;
        Panel.BuildingSelected += OnBuildingSelected;
    }

    // ---- Signal Handlers ----

    private void OnBuildingSelected(BuildingData data)
    {
        _selectedBuilding = data;
    }

    private void OnCellClicked(GridCell cell)
    {
        if (_selectedBuilding == null || cell.IsOccupied) return;
        if (!Economy.CanAfford(_selectedBuilding.Cost)) return;

        Economy.SpendGold(_selectedBuilding.Cost);
        PlaceBuilding(cell, _selectedBuilding);
        _selectedBuilding = null;
        Panel.ClearSelection();
    }

    private void PlaceBuilding(GridCell cell, BuildingData data)
    {
        Building building;
        if (data.IsUnitProducer)
        {
            var mil = new MilitaryBuilding();
            mil.Initialize(data);
            mil.UnitProduced += OnUnitProduced;
            building = mil;
        }
        else if (data.IsDefenseBuilding)
        {
            var def = new DefenseBuilding();
            def.Initialize(data);
            def.SetUnitManager(Units);
            building = def;
        }
        else
        {
            var eco = new EconomyBuilding();
            eco.Initialize(data);
            eco.SetEconomy(Economy);
            building = eco;
        }
        cell.PlaceBuilding(building);
    }

    private void OnUnitProduced(Unit unit, MilitaryBuilding source)
    {
        Rally.AddUnit(unit, source);
    }

    // ---- Win/Lose ----

    private void CheckPlayerBaseDamage()
    {
        // Enemy units that reach the left edge damage the player base
        float baseLineX = Board.GlobalPosition.X + 10;
        var enemies = Units.GetAliveUnits(Team.Enemy);

        foreach (var enemy in enemies)
        {
            if (enemy.GlobalPosition.X <= baseLineX)
            {
                PlayerBaseHp -= (int)enemy.AttackDamage;
                enemy.TakeDamage(enemy.MaxHp * 2); // Kill the unit
                UpdatePlayerBaseHpBar();
            }
        }
    }

    private void CheckWinLose()
    {
        // Victory: enemy base destroyed
        if (Enemy.IsDestroyed())
        {
            CurrentState = GameState.Victory;
            ShowEndScreen("🏆 胜利！", new Color(0.2f, 0.8f, 0.3f));
            return;
        }

        // Defeat: player base destroyed
        if (PlayerBaseHp <= 0)
        {
            PlayerBaseHp = 0;
            CurrentState = GameState.Defeat;
            ShowEndScreen("💀 失败…", new Color(0.9f, 0.2f, 0.2f));
        }
    }

    private void UpdatePlayerBaseHpBar()
    {
        if (_playerBaseHpBar == null) return;
        float ratio = Mathf.Clamp((float)PlayerBaseHp / PlayerBaseMaxHp, 0f, 1f);
        _playerBaseHpBar.Size = new Vector2(30 * ratio, 6);
    }

    private void ShowEndScreen(string text, Color color)
    {
        GetTree().Paused = true;

        var overlay = new CanvasLayer();
        overlay.Name = "EndScreen";
        overlay.Layer = 10;
        AddChild(overlay);

        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.5f);
        bg.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        overlay.AddChild(bg);

        var container = new VBoxContainer();
        container.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        container.Alignment = BoxContainer.AlignmentMode.Center;
        overlay.AddChild(container);

        var label = new Label();
        label.Text = text;
        label.AddThemeFontSizeOverride("font_size", 48);
        label.AddThemeColorOverride("font_color", color);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(label);

        var retryBtn = new Button();
        retryBtn.Text = "重新开始";
        retryBtn.CustomMinimumSize = new Vector2(200, 50);
        retryBtn.Pressed += () =>
        {
            GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        };
        container.AddChild(retryBtn);
    }
}

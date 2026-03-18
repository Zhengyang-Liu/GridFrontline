using Godot;

namespace GridFrontline;

/// <summary>
/// Central game manager. Root node of Main.tscn.
/// Creates all subsystems and wires up signals.
/// </summary>
public partial class GameManager : Node2D
{
    public EconomyManager Economy { get; private set; }
    public GameBoard Board { get; private set; }
    public RallyZone Rally { get; private set; }
    public HUD Hud { get; private set; }
    public BuildPanel Panel { get; private set; }

    private BuildingData _selectedBuilding;

    private const int StartingGold = 100;

    public override void _Ready()
    {
        CreateEconomy();
        CreateCamera();
        CreateBoard();
        CreateRallyZone();
        CreateUI();
        WireSignals();
        Economy.AddGold(StartingGold);
    }

    private void CreateEconomy()
    {
        Economy = new EconomyManager();
        Economy.Name = "EconomyManager";
        AddChild(Economy);
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
        AddChild(Rally);
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
}

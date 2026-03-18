using Godot;
using System.Collections.Generic;

namespace GridFrontline;

/// <summary>
/// Rally zone where produced soldiers wait before deployment.
/// Located between the player grid and the corridor.
/// Each military building's production is blocked while its soldier waits here.
/// </summary>
public partial class RallyZone : Node2D
{
    [Signal]
    public delegate void UnitsDeployedEventHandler();

    private readonly Dictionary<MilitaryBuilding, Unit> _waitingUnits = new();
    private GameBoard _gameBoard;
    private ColorRect _background;
    private Button _deployButton;
    private Label _countLabel;

    public int WaitingCount => _waitingUnits.Count;

    public override void _Ready()
    {
        // Background strip
        _background = new ColorRect();
        _background.Size = new Vector2(GameBoard.RallyWidth, GameBoard.Rows * GridCell.CellSize);
        _background.Color = new Color(0.3f, 0.35f, 0.4f, 0.6f);
        AddChild(_background);

        // Title
        var title = new Label();
        title.Text = "集结";
        title.Position = new Vector2(15, 4);
        title.AddThemeFontSizeOverride("font_size", 14);
        title.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        AddChild(title);

        // Deploy button
        _deployButton = new Button();
        _deployButton.Text = "出发!";
        _deployButton.Position = new Vector2(5, GameBoard.Rows * GridCell.CellSize - 40);
        _deployButton.Size = new Vector2(GameBoard.RallyWidth - 10, 35);
        _deployButton.Pressed += OnDeployPressed;
        _deployButton.Visible = false;
        AddChild(_deployButton);

        // Count label
        _countLabel = new Label();
        _countLabel.Text = "";
        _countLabel.Position = new Vector2(15, 25);
        _countLabel.AddThemeFontSizeOverride("font_size", 12);
        AddChild(_countLabel);
    }

    public void SetGameBoard(GameBoard board)
    {
        _gameBoard = board;
    }

    public void AddUnit(Unit unit, MilitaryBuilding source)
    {
        _waitingUnits[source] = unit;
        AddChild(unit);

        // Position unit visually within the rally zone
        int index = _waitingUnits.Count - 1;
        int row = index % GameBoard.Rows;
        unit.Position = new Vector2(
            GameBoard.RallyWidth / 2f,
            row * GridCell.CellSize + GridCell.CellSize / 2f
        );
        unit.State = Unit.UnitState.InRally;

        source.ProductionBlocked = true;
        _deployButton.Visible = true;
        UpdateCountLabel();
    }

    private void OnDeployPressed()
    {
        if (_waitingUnits.Count == 0) return;

        var exitTarget = _gameBoard.GetCorridorExit();
        int i = 0;

        foreach (var kvp in _waitingUnits)
        {
            var building = kvp.Key;
            var unit = kvp.Value;

            // Get the row of the source building for corridor entrance
            int row = building.Cell?.Row ?? i;
            var entrance = _gameBoard.GetCorridorEntrance(row);

            // Reparent unit to game board so it can move freely
            unit.Reparent(_gameBoard);
            unit.Deploy(entrance, exitTarget);

            // Unblock the building
            building.ProductionBlocked = false;
            i++;
        }

        _waitingUnits.Clear();
        _deployButton.Visible = false;
        UpdateCountLabel();
        EmitSignal(SignalName.UnitsDeployed);
    }

    private void UpdateCountLabel()
    {
        _countLabel.Text = _waitingUnits.Count > 0
            ? $"待命: {_waitingUnits.Count}"
            : "";
    }
}

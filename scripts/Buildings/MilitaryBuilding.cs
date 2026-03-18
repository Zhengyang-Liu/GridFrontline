using Godot;

namespace GridFrontline;

/// <summary>
/// A building that periodically produces soldier units.
/// Production is blocked while a unit waits in the rally zone.
/// </summary>
public partial class MilitaryBuilding : Building
{
    [Signal]
    public delegate void UnitProducedEventHandler(Unit unit, MilitaryBuilding source);

    private double _timer;
    private bool _productionBlocked;
    private Label _progressLabel;
    private ColorRect _progressBar;

    public bool ProductionBlocked
    {
        get => _productionBlocked;
        set
        {
            _productionBlocked = value;
            if (_productionBlocked) _timer = 0;
        }
    }

    public override void _Ready()
    {
        base._Ready();

        // Production progress bar (bottom of cell)
        var barBg = new ColorRect();
        barBg.Size = new Vector2(GridCell.CellSize - 8, 6);
        barBg.Position = new Vector2(4, GridCell.CellSize - 12);
        barBg.Color = new Color(0.2f, 0.2f, 0.2f);
        AddChild(barBg);

        _progressBar = new ColorRect();
        _progressBar.Size = new Vector2(0, 6);
        _progressBar.Position = new Vector2(4, GridCell.CellSize - 12);
        _progressBar.Color = new Color(0.3f, 0.9f, 0.3f);
        AddChild(_progressBar);

        // Status label
        _progressLabel = new Label();
        _progressLabel.Position = new Vector2(4, GridCell.CellSize - 28);
        _progressLabel.AddThemeFontSizeOverride("font_size", 11);
        AddChild(_progressLabel);
    }

    public override void Initialize(BuildingData data)
    {
        base.Initialize(data);
        _timer = 0;
        _productionBlocked = false;
    }

    public override void _Process(double delta)
    {
        if (Data == null) return;

        if (_productionBlocked)
        {
            if (_progressLabel != null)
                _progressLabel.Text = "⏸ 等待";
            if (_progressBar != null)
                _progressBar.Size = new Vector2(0, 6);
            return;
        }

        _timer += delta;
        float progress = Mathf.Clamp((float)(_timer / Data.ProduceInterval), 0f, 1f);

        if (_progressBar != null)
            _progressBar.Size = new Vector2((GridCell.CellSize - 8) * progress, 6);
        if (_progressLabel != null)
            _progressLabel.Text = $"{(int)(progress * 100)}%";

        if (_timer >= Data.ProduceInterval)
        {
            _timer = 0;
            ProduceUnit();
        }
    }

    private void ProduceUnit()
    {
        var unit = new Unit();
        unit.Initialize();
        EmitSignal(SignalName.UnitProduced, unit, this);
        ProductionBlocked = true;
    }
}

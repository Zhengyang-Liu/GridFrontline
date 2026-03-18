using Godot;

namespace GridFrontline;

/// <summary>
/// Abstract base class for all buildings.
/// </summary>
public abstract partial class Building : Node2D
{
    public BuildingData Data { get; set; }
    public int CurrentHp { get; set; }
    public GridCell Cell { get; set; }

    protected ColorRect Visual;
    protected Label NameLabel;
    private bool _initialized;

    public override void _Ready()
    {
        if (!_initialized) return;
        CreateVisual();
    }

    protected virtual void CreateVisual()
    {
        Visual = new ColorRect();
        Visual.Size = new Vector2(GridCell.CellSize - 4, GridCell.CellSize - 4);
        Visual.Position = new Vector2(2, 2);
        Visual.Color = Data?.BuildingColor ?? Colors.White;
        AddChild(Visual);

        NameLabel = new Label();
        NameLabel.Text = Data?.DisplayChar ?? "?";
        NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        NameLabel.Position = new Vector2(GridCell.CellSize / 2f - 12, GridCell.CellSize / 2f - 14);
        NameLabel.AddThemeFontSizeOverride("font_size", 20);
        AddChild(NameLabel);
    }

    public virtual void Initialize(BuildingData data)
    {
        Data = data;
        CurrentHp = data.MaxHp;
        _initialized = true;
    }
}

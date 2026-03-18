using Godot;

namespace GridFrontline;

/// <summary>
/// A single cell on the player grid. Hover and click are driven by GameBoard.
/// </summary>
public partial class GridCell : Node2D
{
	public const int CellSize = 80;

	public int Row { get; set; }
	public int Col { get; set; }
	public bool IsOccupied { get; private set; }
	public Building CurrentBuilding { get; private set; }

	private ColorRect _background;
	private Label _coordLabel;

	private static readonly Color NormalColor = new(0.82f, 0.84f, 0.86f);
	private static readonly Color HoverColor = new(0.45f, 0.85f, 0.5f);
	private static readonly Color OccupiedColor = new(0.7f, 0.72f, 0.7f);

	public override void _Ready()
	{
		// Background rect
		_background = new ColorRect();
		_background.Size = new Vector2(CellSize - 2, CellSize - 2);
		_background.Position = new Vector2(1, 1);
		_background.Color = NormalColor;
		AddChild(_background);

		// Coord debug label (small, bottom-right)
		_coordLabel = new Label();
		_coordLabel.Text = $"{Row},{Col}";
		_coordLabel.Position = new Vector2(CellSize - 30, CellSize - 20);
		_coordLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f, 0.6f));
		_coordLabel.AddThemeFontSizeOverride("font_size", 10);
		AddChild(_coordLabel);
	}

	public void SetHovered(bool hovered)
	{
		if (_background == null) return;
		if (hovered && !IsOccupied)
			_background.Color = HoverColor;
		else
			_background.Color = IsOccupied ? OccupiedColor : NormalColor;
	}

	public void PlaceBuilding(Building building)
	{
		CurrentBuilding = building;
		IsOccupied = true;
		building.Cell = this;
		building.Position = Vector2.Zero;
		AddChild(building);
		_background.Color = OccupiedColor;
	}

	public void RemoveBuilding()
	{
		if (CurrentBuilding != null)
		{
			CurrentBuilding.QueueFree();
			CurrentBuilding = null;
		}
		IsOccupied = false;
		_background.Color = NormalColor;
	}
}

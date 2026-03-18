using Godot;

namespace GridFrontline;

/// <summary>
/// A single cell on the player grid. Handles hover, click, and building placement.
/// Extends Area2D for mouse detection.
/// </summary>
public partial class GridCell : Area2D
{
	[Signal]
	public delegate void CellClickedEventHandler(GridCell cell);
	[Signal]
	public delegate void CellHoveredEventHandler(GridCell cell);
	[Signal]
	public delegate void CellExitedEventHandler(GridCell cell);

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

		// Collision shape
		var collision = new CollisionShape2D();
		var shape = new RectangleShape2D();
		shape.Size = new Vector2(CellSize - 2, CellSize - 2);
		collision.Shape = shape;
		collision.Position = new Vector2(CellSize / 2f, CellSize / 2f);
		AddChild(collision);

		// Coord debug label (small, bottom-right)
		_coordLabel = new Label();
		_coordLabel.Text = $"{Row},{Col}";
		_coordLabel.Position = new Vector2(CellSize - 30, CellSize - 20);
		_coordLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f, 0.6f));
		_coordLabel.AddThemeFontSizeOverride("font_size", 10);
		AddChild(_coordLabel);

		// Signals
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		InputEvent += OnInputEvent;
	}

	private void OnMouseEntered()
	{
		if (!IsOccupied)
			_background.Color = HoverColor;
		EmitSignal(SignalName.CellHovered, this);
	}

	private void OnMouseExited()
	{
		_background.Color = IsOccupied ? OccupiedColor : NormalColor;
		EmitSignal(SignalName.CellExited, this);
	}

	private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mb
			&& mb.Pressed
			&& mb.ButtonIndex == MouseButton.Left)
		{
			EmitSignal(SignalName.CellClicked, this);
		}
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

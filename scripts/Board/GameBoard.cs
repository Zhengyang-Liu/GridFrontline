using Godot;

namespace GridFrontline;

/// <summary>
/// Renders the full game board: player grid, corridor, and enemy zone placeholder.
/// </summary>
public partial class GameBoard : Node2D
{
	public const int Rows = 6;
	public const int Cols = 8;
	public const int GapSize = 20;
	public const int RallyWidth = 80;
	public const int CorridorWidth = 400;
	public const int EnemyZoneWidth = 480;
	public const float BoardOriginX = 80f;
	public const float BoardOriginY = 120f;

	[Signal]
	public delegate void CellClickedEventHandler(GridCell cell);

	private GridCell[,] _cells = new GridCell[Rows, Cols];

	public GridCell[,] Cells => _cells;

	public override void _Ready()
	{
		CreatePlayerZone();
		CreateCorridorVisual();
		CreateEnemyZoneVisual();
	}

	// ---- Zone Creation ----

	private void CreatePlayerZone()
	{
		for (int row = 0; row < Rows; row++)
		{
			for (int col = 0; col < Cols; col++)
			{
				var cell = new GridCell();
				cell.Row = row;
				cell.Col = col;
				cell.Position = new Vector2(col * GridCell.CellSize, row * GridCell.CellSize);
				cell.CellClicked += OnCellClicked;
				AddChild(cell);
				_cells[row, col] = cell;
			}
		}
	}

	private void CreateCorridorVisual()
	{
		var rect = new ColorRect();
		rect.Color = new Color(0.25f, 0.28f, 0.32f);
		float x = Cols * GridCell.CellSize + GapSize + RallyWidth + GapSize;
		rect.Position = new Vector2(x, 0);
		rect.Size = new Vector2(CorridorWidth, Rows * GridCell.CellSize);
		AddChild(rect);

		// Label
		var label = new Label();
		label.Text = "— 走 廊 —";
		label.Position = new Vector2(x + CorridorWidth / 2f - 40, Rows * GridCell.CellSize / 2f - 10);
		label.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
		AddChild(label);
	}

	private void CreateEnemyZoneVisual()
	{
		var rect = new ColorRect();
		rect.Color = new Color(0.35f, 0.12f, 0.12f);
		float x = Cols * GridCell.CellSize + GapSize + RallyWidth + GapSize + CorridorWidth + GapSize;
		rect.Position = new Vector2(x, 0);
		rect.Size = new Vector2(EnemyZoneWidth, Rows * GridCell.CellSize);
		AddChild(rect);

		// Label
		var label = new Label();
		label.Text = "敌 方 基 地";
		label.Position = new Vector2(x + EnemyZoneWidth / 2f - 40, Rows * GridCell.CellSize / 2f - 10);
		label.AddThemeColorOverride("font_color", new Color(0.8f, 0.4f, 0.4f));
		AddChild(label);
	}

	// ---- Public API ----

	private void OnCellClicked(GridCell cell)
	{
		EmitSignal(SignalName.CellClicked, cell);
	}

	/// <summary>
	/// Global position of the corridor entrance for a given row.
	/// Units deploy here.
	/// </summary>
	public Vector2 GetCorridorEntrance(int row)
	{
		float localX = Cols * GridCell.CellSize + GapSize + RallyWidth + GapSize;
		float localY = row * GridCell.CellSize + GridCell.CellSize / 2f;
		return GlobalPosition + new Vector2(localX, localY);
	}

	/// <summary>
	/// Global position of the corridor exit (right side, center).
	/// </summary>
	public Vector2 GetCorridorExit()
	{
		float localX = Cols * GridCell.CellSize + GapSize + RallyWidth + GapSize + CorridorWidth;
		float localY = Rows * GridCell.CellSize / 2f;
		return GlobalPosition + new Vector2(localX, localY);
	}
}

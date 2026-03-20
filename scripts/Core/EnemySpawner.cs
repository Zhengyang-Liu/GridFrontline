using Godot;
using System.Collections.Generic;

namespace GridFrontline;

/// <summary>
/// Spawns enemy units in waves according to configuration.
/// Positioned at the right side of the map (enemy base area).
/// </summary>
public partial class EnemySpawner : Node2D
{
    [Signal]
    public delegate void AllWavesCompleteEventHandler();

    private List<EnemyWave> _waves = new();
    private GameBoard _gameBoard;
    private UnitManager _unitManager;
    private double _gameTime;
    private int _currentWaveIndex;
    private double _waveSpawnTimer;
    private int _waveSpawnCount;

    public int BaseHp { get; set; } = 500;
    public int CurrentBaseHp { get; set; } = 500;

    // Visual
    private ColorRect _baseVisual;
    private ColorRect _baseHpBar;
    private Label _baseLabel;

    public void Setup(GameBoard board, UnitManager unitManager, List<EnemyWave> waves)
    {
        _gameBoard = board;
        _unitManager = unitManager;
        _waves = waves;
        _currentWaveIndex = 0;
        _gameTime = 0;
    }

    public override void _Ready()
    {
        // Base core visual
        float zoneHeight = GameBoard.Rows * GridCell.CellSize;
        _baseVisual = new ColorRect();
        _baseVisual.Size = new Vector2(80, 80);
        _baseVisual.Position = new Vector2(GameBoard.EnemyZoneWidth / 2f - 40, zoneHeight / 2f - 40);
        _baseVisual.Color = new Color(0.6f, 0.1f, 0.1f);
        AddChild(_baseVisual);

        _baseLabel = new Label();
        _baseLabel.Text = "敌方核心";
        _baseLabel.Position = new Vector2(GameBoard.EnemyZoneWidth / 2f - 30, zoneHeight / 2f - 10);
        _baseLabel.AddThemeFontSizeOverride("font_size", 12);
        _baseLabel.AddThemeColorOverride("font_color", Colors.White);
        AddChild(_baseLabel);

        // HP bar
        var hpBg = new ColorRect();
        hpBg.Size = new Vector2(80, 8);
        hpBg.Position = new Vector2(GameBoard.EnemyZoneWidth / 2f - 40, zoneHeight / 2f - 50);
        hpBg.Color = new Color(0.2f, 0.2f, 0.2f);
        AddChild(hpBg);

        _baseHpBar = new ColorRect();
        _baseHpBar.Size = new Vector2(80, 8);
        _baseHpBar.Position = hpBg.Position;
        _baseHpBar.Color = new Color(0.9f, 0.2f, 0.2f);
        AddChild(_baseHpBar);
    }

    public override void _Process(double delta)
    {
        _gameTime += delta;
        ProcessWaves(delta);
    }

    private void ProcessWaves(double delta)
    {
        if (_currentWaveIndex >= _waves.Count) return;

        var wave = _waves[_currentWaveIndex];

        if (_gameTime < wave.StartTime) return;

        _waveSpawnTimer += delta;
        if (_waveSpawnTimer >= wave.Interval && _waveSpawnCount < wave.Count)
        {
            _waveSpawnTimer -= wave.Interval;
            SpawnEnemyUnit(wave);
            _waveSpawnCount++;

            if (_waveSpawnCount >= wave.Count)
            {
                _currentWaveIndex++;
                _waveSpawnTimer = 0;
                _waveSpawnCount = 0;
            }
        }
    }

    private void SpawnEnemyUnit(EnemyWave wave)
    {
        var unitData = wave.UnitType switch
        {
            "elite" => UnitDatabase.EnemyElite,
            _ => UnitDatabase.EnemySoldier
        };

        var unit = new Unit();
        unit.Initialize(unitData, Team.Enemy);

        // Spawn at enemy base entrance, specific row or random
        int row = wave.Row >= 0 ? wave.Row : (int)(GD.Randf() * GameBoard.Rows);
        row = Mathf.Clamp(row, 0, GameBoard.Rows - 1);

        // Position at right side of corridor
        float spawnX = _gameBoard.GlobalPosition.X
                       + GameBoard.Cols * GridCell.CellSize
                       + GameBoard.GapSize + GameBoard.RallyWidth + GameBoard.GapSize
                       + GameBoard.CorridorWidth - 20;
        float spawnY = _gameBoard.GlobalPosition.Y
                       + row * GridCell.CellSize + GridCell.CellSize / 2f;

        // Default target: left side (toward player base)
        float targetX = _gameBoard.GlobalPosition.X;
        float targetY = spawnY;

        _gameBoard.AddChild(unit);
        unit.Manager = _unitManager;
        unit.Deploy(new Vector2(spawnX, spawnY), new Vector2(targetX, targetY));
        _unitManager.Register(unit);
    }

    public void TakeDamage(int damage)
    {
        CurrentBaseHp -= damage;
        if (CurrentBaseHp < 0) CurrentBaseHp = 0;
        UpdateHpBar();
    }

    public bool IsDestroyed() => CurrentBaseHp <= 0;

    private void UpdateHpBar()
    {
        if (_baseHpBar == null) return;
        float ratio = (float)CurrentBaseHp / BaseHp;
        _baseHpBar.Size = new Vector2(80 * ratio, 8);
    }
}

/// <summary>
/// Configuration for a single enemy wave.
/// </summary>
public class EnemyWave
{
    public float StartTime { get; set; }
    public string UnitType { get; set; } = "soldier";
    public int Count { get; set; } = 3;
    public float Interval { get; set; } = 2f;
    public int Row { get; set; } = -1; // -1 = random
}

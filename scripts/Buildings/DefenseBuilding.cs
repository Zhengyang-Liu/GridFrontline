using Godot;

namespace GridFrontline;

/// <summary>
/// A defensive building that attacks enemies within range.
/// Used for Arrow Tower (attacks corridor) and Wall (blocks enemies).
/// </summary>
public partial class DefenseBuilding : Building
{
    private float _attackRange;
    private float _attackDamage;
    private float _attackInterval;
    private double _attackTimer;
    private bool _canAttack;
    private UnitManager _unitManager;
    private ColorRect _rangIndicator;

    public void SetUnitManager(UnitManager manager)
    {
        _unitManager = manager;
    }

    public override void Initialize(BuildingData data)
    {
        base.Initialize(data);
        _canAttack = data.ProduceAmount > 0; // Reuse ProduceAmount as attack damage indicator
        _attackDamage = data.ProduceAmount;
        _attackInterval = data.ProduceInterval;
        _attackRange = data.AttackRange;
        _attackTimer = 0;
    }

    public override void _Process(double delta)
    {
        if (!_canAttack || _unitManager == null) return;

        _attackTimer += delta;
        if (_attackTimer >= _attackInterval)
        {
            _attackTimer -= _attackInterval;
            AttackNearestEnemy();
        }
    }

    private void AttackNearestEnemy()
    {
        // Calculate attack origin from global position of this building
        var origin = GlobalPosition + new Vector2(GridCell.CellSize / 2f, GridCell.CellSize / 2f);
        var target = _unitManager.GetNearestEnemy(origin, Team.Player, _attackRange);

        if (target != null)
        {
            target.TakeDamage((int)_attackDamage);
            ShowAttackEffect(target.GlobalPosition);
        }
    }

    private void ShowAttackEffect(Vector2 targetPos)
    {
        var popup = new Label();
        popup.Text = "⚡";
        popup.AddThemeFontSizeOverride("font_size", 12);
        popup.Position = new Vector2(GridCell.CellSize / 2f - 6, -5);
        AddChild(popup);

        var tween = CreateTween();
        tween.TweenProperty(popup, "modulate:a", 0.0, 0.4);
        tween.TweenCallback(Callable.From(() => popup.QueueFree()));
    }
}

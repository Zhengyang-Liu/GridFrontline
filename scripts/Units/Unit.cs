using Godot;

namespace GridFrontline;

/// <summary>
/// Base class for all soldier units.
/// Handles movement, target finding, combat, health bar, and death.
/// </summary>
public partial class Unit : CharacterBody2D
{
    public enum UnitState { InRally, Moving, Fighting, Dead }

    // --- Stats ---
    [Export] public int MaxHp { get; set; } = 100;
    [Export] public float MoveSpeed { get; set; } = 120f;
    [Export] public float AttackDamage { get; set; } = 10f;
    [Export] public float AttackSpeed { get; set; } = 1f;
    [Export] public float AttackRange { get; set; } = 30f;
    [Export] public float SearchRange { get; set; } = 300f;

    // --- Runtime state ---
    public int CurrentHp { get; set; }
    public UnitState State { get; set; } = UnitState.InRally;
    public Team UnitTeam { get; set; } = Team.Player;
    public Unit CurrentTarget { get; private set; }
    public UnitManager Manager { get; set; }

    // Default march direction when no target found
    public Vector2 DefaultTarget { get; set; }

    // --- Internals ---
    private ColorRect _visual;
    private ColorRect _hpBarBg;
    private ColorRect _hpBarFill;
    private bool _visualCreated;
    private double _attackTimer;

    // Visual config per team
    private static readonly Color PlayerColor = new(0.25f, 0.45f, 0.9f);
    private static readonly Color EnemyColor = new(0.85f, 0.25f, 0.2f);

    public override void _Ready()
    {
        if (!_visualCreated)
        {
            CreateVisual();
            _visualCreated = true;
        }
    }

    private void CreateVisual()
    {
        // Unit body
        _visual = new ColorRect();
        _visual.Size = new Vector2(18, 18);
        _visual.Position = new Vector2(-9, -9);
        _visual.Color = UnitTeam == Team.Player ? PlayerColor : EnemyColor;
        AddChild(_visual);

        // Collision shape
        var collision = new CollisionShape2D();
        var shape = new CircleShape2D();
        shape.Radius = 9;
        collision.Shape = shape;
        AddChild(collision);

        // Health bar background
        _hpBarBg = new ColorRect();
        _hpBarBg.Size = new Vector2(22, 4);
        _hpBarBg.Position = new Vector2(-11, -16);
        _hpBarBg.Color = new Color(0.2f, 0.2f, 0.2f);
        AddChild(_hpBarBg);

        // Health bar fill
        _hpBarFill = new ColorRect();
        _hpBarFill.Size = new Vector2(22, 4);
        _hpBarFill.Position = new Vector2(-11, -16);
        _hpBarFill.Color = new Color(0.2f, 0.9f, 0.2f);
        AddChild(_hpBarFill);
    }

    public void Initialize()
    {
        CurrentHp = MaxHp;
        State = UnitState.InRally;
        _attackTimer = 0;
    }

    public void Deploy(Vector2 startGlobal, Vector2 defaultTarget)
    {
        GlobalPosition = startGlobal;
        DefaultTarget = defaultTarget;
        State = UnitState.Moving;
    }

    // ---- Main Loop ----

    public override void _PhysicsProcess(double delta)
    {
        switch (State)
        {
            case UnitState.Moving:
                ProcessMoving(delta);
                break;
            case UnitState.Fighting:
                ProcessFighting(delta);
                break;
        }
    }

    private void ProcessMoving(double delta)
    {
        // Try to find an enemy
        var target = Manager?.GetNearestEnemy(GlobalPosition, UnitTeam, SearchRange);

        if (target != null)
        {
            CurrentTarget = target;
            float dist = GlobalPosition.DistanceTo(target.GlobalPosition);

            if (dist <= AttackRange)
            {
                // In attack range — start fighting
                Velocity = Vector2.Zero;
                State = UnitState.Fighting;
                _attackTimer = 0;
                return;
            }

            // Move toward target
            var dir = (target.GlobalPosition - GlobalPosition).Normalized();
            Velocity = dir * MoveSpeed;
        }
        else
        {
            // No enemy found — march toward default target
            CurrentTarget = null;
            var dir = (DefaultTarget - GlobalPosition).Normalized();
            Velocity = dir * MoveSpeed;

            // Reached the end — stop (base attack will come in Step 6)
            if (GlobalPosition.DistanceTo(DefaultTarget) < 10f)
            {
                Velocity = Vector2.Zero;
                State = UnitState.Fighting; // Will try to find target or idle
                return;
            }
        }

        MoveAndSlide();
    }

    private void ProcessFighting(double delta)
    {
        // Check if target is still valid
        if (CurrentTarget == null || CurrentTarget.State == UnitState.Dead
            || !IsInstanceValid(CurrentTarget))
        {
            CurrentTarget = null;
            State = UnitState.Moving;
            return;
        }

        // Check if target moved out of attack range (chase)
        float dist = GlobalPosition.DistanceTo(CurrentTarget.GlobalPosition);
        if (dist > AttackRange * 1.5f)
        {
            State = UnitState.Moving;
            return;
        }

        // Attack timer
        _attackTimer += delta;
        float attackInterval = 1.0f / AttackSpeed;
        if (_attackTimer >= attackInterval)
        {
            _attackTimer -= attackInterval;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (CurrentTarget == null || CurrentTarget.State == UnitState.Dead) return;
        CurrentTarget.TakeDamage((int)AttackDamage);
    }

    // ---- Damage & Death ----

    public void TakeDamage(int damage)
    {
        if (State == UnitState.Dead) return;

        CurrentHp -= damage;
        UpdateHealthBar();

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (_hpBarFill == null) return;

        float ratio = Mathf.Clamp((float)CurrentHp / MaxHp, 0f, 1f);
        _hpBarFill.Size = new Vector2(22 * ratio, 4);

        // Color: green > yellow > red
        if (ratio > 0.5f)
            _hpBarFill.Color = new Color(0.2f, 0.9f, 0.2f);
        else if (ratio > 0.25f)
            _hpBarFill.Color = new Color(0.9f, 0.9f, 0.2f);
        else
            _hpBarFill.Color = new Color(0.9f, 0.2f, 0.2f);
    }

    private void Die()
    {
        State = UnitState.Dead;
        Velocity = Vector2.Zero;
        Manager?.Unregister(this);

        // Death flash effect then remove
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0, 0.4);
        tween.TweenCallback(Callable.From(() => QueueFree()));
    }
}

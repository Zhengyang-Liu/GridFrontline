using Godot;

namespace GridFrontline;

/// <summary>
/// Base class for all soldier units.
/// Uses CharacterBody2D for 2D physics-based movement.
/// </summary>
public partial class Unit : CharacterBody2D
{
    public enum UnitState { InRally, Moving, Arrived, Fighting, Dead }

    [Export] public int MaxHp { get; set; } = 100;
    [Export] public float MoveSpeed { get; set; } = 120f;
    [Export] public float AttackDamage { get; set; } = 10f;
    [Export] public float AttackSpeed { get; set; } = 1f;

    public int CurrentHp { get; set; }
    public UnitState State { get; set; } = UnitState.InRally;
    public Vector2 TargetPosition { get; set; }

    private ColorRect _visual;
    private bool _visualCreated;

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
        // Blue circle-ish square to represent a soldier
        _visual = new ColorRect();
        _visual.Size = new Vector2(18, 18);
        _visual.Position = new Vector2(-9, -9);
        _visual.Color = new Color(0.25f, 0.45f, 0.9f);
        AddChild(_visual);

        var collision = new CollisionShape2D();
        var shape = new CircleShape2D();
        shape.Radius = 9;
        collision.Shape = shape;
        AddChild(collision);
    }

    public void Initialize()
    {
        CurrentHp = MaxHp;
        State = UnitState.InRally;
    }

    /// <summary>
    /// Deploy the unit from the rally zone onto the battlefield.
    /// </summary>
    public void Deploy(Vector2 startGlobal, Vector2 targetGlobal)
    {
        GlobalPosition = startGlobal;
        TargetPosition = targetGlobal;
        State = UnitState.Moving;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (State != UnitState.Moving) return;

        var direction = (TargetPosition - GlobalPosition).Normalized();
        Velocity = direction * MoveSpeed;
        MoveAndSlide();

        if (GlobalPosition.DistanceTo(TargetPosition) < 10f)
        {
            // Phase 1: just stop at the corridor exit
            Velocity = Vector2.Zero;
            State = UnitState.Arrived;
        }
    }
}

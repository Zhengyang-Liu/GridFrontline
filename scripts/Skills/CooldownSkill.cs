namespace GridFrontline;

/// <summary>
/// Skill triggered on a fixed timer cooldown.
/// </summary>
public partial class CooldownSkill : Skill
{
    public float CooldownTime { get; set; } = 8f;
    private double _timer;

    public override bool CanActivate() => _timer >= CooldownTime;

    public override void UpdateSkill(double delta)
    {
        _timer += delta;
    }

    public override void Activate()
    {
        _timer = 0;
    }
}

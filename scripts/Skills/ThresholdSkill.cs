namespace GridFrontline;

/// <summary>
/// Skill triggered when HP drops below a threshold. Activates once per fight.
/// </summary>
public partial class ThresholdSkill : Skill
{
    public float HpThreshold { get; set; } = 0.3f;
    private bool _triggered;

    public override bool CanActivate()
    {
        if (_triggered || Owner == null) return false;
        return (float)Owner.CurrentHp / Owner.MaxHp <= HpThreshold;
    }

    public override void Activate()
    {
        _triggered = true;
    }

    /// <summary>Reset for reuse (e.g., new battle).</summary>
    public void Reset() => _triggered = false;
}

using Godot;

namespace GridFrontline;

/// <summary>
/// Cavalry threshold skill: doubles attack when HP drops below 30%.
/// </summary>
public partial class FrenzySkill : ThresholdSkill
{
    private float _originalDamage;

    public FrenzySkill()
    {
        SkillName = "狂暴";
        HpThreshold = 0.3f;
    }

    public override void Activate()
    {
        base.Activate();
        if (Owner == null) return;

        _originalDamage = Owner.AttackDamage;
        Owner.AttackDamage *= 2f;

        ShowSkillPopup("💥 狂暴!", new Color(1f, 0.2f, 0.2f));
    }
}

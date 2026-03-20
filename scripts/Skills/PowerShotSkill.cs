using Godot;

namespace GridFrontline;

/// <summary>
/// Archer cooldown skill: high damage single target shot.
/// </summary>
public partial class PowerShotSkill : CooldownSkill
{
    public float DamageMultiplier { get; set; } = 3f;

    public PowerShotSkill()
    {
        SkillName = "强力射击";
        CooldownTime = 8f;
    }

    public override void Activate()
    {
        base.Activate();
        if (Owner?.CurrentTarget == null) return;

        int damage = (int)(Owner.AttackDamage * DamageMultiplier);
        Owner.CurrentTarget.TakeDamage(damage);

        ShowSkillPopup("🏹 强力射击!", new Color(0.4f, 1f, 0.5f));
    }
}

using Godot;

namespace GridFrontline;

/// <summary>
/// Skill triggered when energy reaches max. Energy gained from attacks/taking damage.
/// </summary>
public partial class ChargeSkill : Skill
{
    public float MaxEnergy { get; set; } = 100f;
    public float EnergyPerAttack { get; set; } = 20f;
    public float EnergyPerHit { get; set; } = 15f;
    public float CurrentEnergy { get; set; }

    public override bool CanActivate() => CurrentEnergy >= MaxEnergy;

    public override void OnOwnerAttack()
    {
        CurrentEnergy += EnergyPerAttack;
    }

    public override void OnOwnerTakeDamage(int damage)
    {
        CurrentEnergy += EnergyPerHit;
    }

    public override void Activate()
    {
        CurrentEnergy = 0;
    }
}

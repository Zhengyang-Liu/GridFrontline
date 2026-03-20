using Godot;
using System.Collections.Generic;

namespace GridFrontline;

/// <summary>
/// Swordsman charge skill: AOE damage to all enemies in range.
/// </summary>
public partial class WhirlwindSkill : ChargeSkill
{
    public float AoeRange { get; set; } = 60f;
    public float AoeDamage { get; set; } = 25f;

    public WhirlwindSkill()
    {
        SkillName = "旋风斩";
        MaxEnergy = 100f;
        EnergyPerAttack = 25f;
        EnergyPerHit = 15f;
    }

    public override void Activate()
    {
        base.Activate();
        if (Owner?.Manager == null) return;

        var enemies = Owner.Manager.GetEnemiesInRange(
            Owner.GlobalPosition, Owner.UnitTeam, AoeRange);

        foreach (var enemy in enemies)
        {
            enemy.TakeDamage((int)AoeDamage);
        }

        ShowSkillPopup("⚔ 旋风斩!", new Color(1f, 0.6f, 0.2f));
    }
}

using Godot;

namespace GridFrontline;

/// <summary>
/// Data resource defining a unit type's stats.
/// </summary>
[GlobalClass]
public partial class UnitData : Resource
{
    [Export] public string UnitName { get; set; } = "";
    [Export] public int MaxHp { get; set; } = 100;
    [Export] public float MoveSpeed { get; set; } = 120f;
    [Export] public float AttackDamage { get; set; } = 10f;
    [Export] public float AttackSpeed { get; set; } = 1f;
    [Export] public float AttackRange { get; set; } = 30f;
    [Export] public float SearchRange { get; set; } = 300f;
    [Export] public Color UnitColor { get; set; } = new(0.25f, 0.45f, 0.9f);
    [Export] public string DisplayChar { get; set; } = "S";
    [Export] public string SkillId { get; set; } = "";
}

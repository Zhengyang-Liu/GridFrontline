using Godot;

namespace GridFrontline;

/// <summary>
/// Predefined unit types for Phase 2.
/// </summary>
public static class UnitDatabase
{
    public static UnitData Swordsman => new()
    {
        UnitName = "剑士",
        MaxHp = 120,
        MoveSpeed = 100f,
        AttackDamage = 15f,
        AttackSpeed = 1.0f,
        AttackRange = 30f,
        SearchRange = 300f,
        UnitColor = new Color(0.3f, 0.5f, 0.95f),
        DisplayChar = "剑",
        SkillId = "whirlwind"
    };

    public static UnitData Archer => new()
    {
        UnitName = "弓箭手",
        MaxHp = 70,
        MoveSpeed = 90f,
        AttackDamage = 12f,
        AttackSpeed = 1.2f,
        AttackRange = 200f,
        SearchRange = 400f,
        UnitColor = new Color(0.2f, 0.75f, 0.4f),
        DisplayChar = "弓",
        SkillId = "powershot"
    };

    public static UnitData Cavalry => new()
    {
        UnitName = "骑兵",
        MaxHp = 100,
        MoveSpeed = 180f,
        AttackDamage = 20f,
        AttackSpeed = 0.8f,
        AttackRange = 35f,
        SearchRange = 350f,
        UnitColor = new Color(0.85f, 0.6f, 0.15f),
        DisplayChar = "骑",
        SkillId = "frenzy"
    };

    // Enemy unit variants
    public static UnitData EnemySoldier => new()
    {
        UnitName = "敌兵",
        MaxHp = 90,
        MoveSpeed = 80f,
        AttackDamage = 10f,
        AttackSpeed = 1.0f,
        AttackRange = 30f,
        SearchRange = 300f,
        UnitColor = new Color(0.85f, 0.25f, 0.2f),
        DisplayChar = "兵",
        SkillId = ""
    };

    public static UnitData EnemyElite => new()
    {
        UnitName = "精锐",
        MaxHp = 160,
        MoveSpeed = 70f,
        AttackDamage = 18f,
        AttackSpeed = 0.9f,
        AttackRange = 30f,
        SearchRange = 300f,
        UnitColor = new Color(0.7f, 0.15f, 0.15f),
        DisplayChar = "精",
        SkillId = ""
    };
}

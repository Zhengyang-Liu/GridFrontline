namespace GridFrontline;

/// <summary>
/// Factory that creates Skill instances from skill IDs defined in UnitData.
/// </summary>
public static class SkillFactory
{
    public static Skill Create(string skillId)
    {
        return skillId switch
        {
            "whirlwind" => new WhirlwindSkill(),
            "powershot" => new PowerShotSkill(),
            "frenzy" => new FrenzySkill(),
            _ => null
        };
    }
}

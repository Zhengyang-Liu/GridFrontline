namespace GridFrontline;

/// <summary>
/// Predefined building types.
/// </summary>
public static class BuildingDatabase
{
    public static BuildingData Farm => new()
    {
        BuildingName = "农场",
        Cost = 40,
        MaxHp = 100,
        ProduceInterval = 4f,
        ProduceAmount = 8,
        BuildingColor = new Godot.Color(0.92f, 0.78f, 0.2f),
        DisplayChar = "F",
        IsUnitProducer = false
    };

    public static BuildingData SwordsmanBarracks => new()
    {
        BuildingName = "剑士营",
        Cost = 60,
        MaxHp = 150,
        ProduceInterval = 6f,
        ProduceAmount = 1,
        BuildingColor = new Godot.Color(0.3f, 0.5f, 0.95f),
        DisplayChar = "剑",
        IsUnitProducer = true,
        ProducedUnit = UnitDatabase.Swordsman
    };

    public static BuildingData ArcherRange => new()
    {
        BuildingName = "弓箭营",
        Cost = 55,
        MaxHp = 100,
        ProduceInterval = 7f,
        ProduceAmount = 1,
        BuildingColor = new Godot.Color(0.2f, 0.75f, 0.4f),
        DisplayChar = "弓",
        IsUnitProducer = true,
        ProducedUnit = UnitDatabase.Archer
    };

    public static BuildingData Stable => new()
    {
        BuildingName = "马厩",
        Cost = 80,
        MaxHp = 130,
        ProduceInterval = 9f,
        ProduceAmount = 1,
        BuildingColor = new Godot.Color(0.85f, 0.6f, 0.15f),
        DisplayChar = "骑",
        IsUnitProducer = true,
        ProducedUnit = UnitDatabase.Cavalry
    };

    // Keep old name for backward compatibility with tests
    public static BuildingData Barracks => SwordsmanBarracks;
}

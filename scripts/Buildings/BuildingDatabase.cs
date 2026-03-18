namespace GridFrontline;

/// <summary>
/// Predefined building types for Phase 1.
/// Later these will be loaded from .tres resource files.
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

    public static BuildingData Barracks => new()
    {
        BuildingName = "兵营",
        Cost = 60,
        MaxHp = 150,
        ProduceInterval = 6f,
        ProduceAmount = 1,
        BuildingColor = new Godot.Color(0.75f, 0.22f, 0.22f),
        DisplayChar = "B",
        IsUnitProducer = true
    };
}

using Godot;

namespace GridFrontline;

/// <summary>
/// Data resource defining a building's stats and appearance.
/// For Phase 1, instances are created from code (see BuildingDatabase).
/// </summary>
[GlobalClass]
public partial class BuildingData : Resource
{
    [Export] public string BuildingName { get; set; } = "";
    [Export] public int Cost { get; set; } = 50;
    [Export] public int MaxHp { get; set; } = 100;
    [Export] public float ProduceInterval { get; set; } = 5f;
    [Export] public int ProduceAmount { get; set; } = 10;
    [Export] public Color BuildingColor { get; set; } = Colors.White;
    [Export] public string DisplayChar { get; set; } = "?";
    [Export] public bool IsUnitProducer { get; set; } = false;
    [Export] public UnitData ProducedUnit { get; set; }
}

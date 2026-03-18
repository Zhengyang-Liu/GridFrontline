using Godot;

namespace GridFrontline;

/// <summary>
/// Manages the single economy resource: gold.
/// </summary>
public partial class EconomyManager : Node
{
    [Signal]
    public delegate void GoldChangedEventHandler(int newAmount);

    private int _gold;

    public int Gold
    {
        get => _gold;
        private set
        {
            _gold = Mathf.Max(0, value);
            EmitSignal(SignalName.GoldChanged, _gold);
        }
    }

    public bool CanAfford(int cost) => _gold >= cost;

    public void AddGold(int amount)
    {
        Gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (!CanAfford(amount)) return false;
        Gold -= amount;
        return true;
    }
}

using Godot;

namespace GridFrontline;

/// <summary>
/// A building that periodically produces gold.
/// </summary>
public partial class EconomyBuilding : Building
{
    private double _timer;
    private EconomyManager _economy;

    public void SetEconomy(EconomyManager economy)
    {
        _economy = economy;
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public override void Initialize(BuildingData data)
    {
        base.Initialize(data);
        _timer = 0;
    }

    public override void _Process(double delta)
    {
        if (Data == null || _economy == null) return;

        _timer += delta;
        if (_timer >= Data.ProduceInterval)
        {
            _timer -= Data.ProduceInterval;
            ProduceGold();
        }
    }

    private void ProduceGold()
    {
        _economy.AddGold(Data.ProduceAmount);
        ShowGoldPopup();
    }

    private void ShowGoldPopup()
    {
        var popup = new Label();
        popup.Text = $"+{Data.ProduceAmount}";
        popup.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0f));
        popup.AddThemeFontSizeOverride("font_size", 16);
        popup.Position = new Vector2(GridCell.CellSize / 2f - 12, -5);
        AddChild(popup);

        var tween = CreateTween();
        tween.TweenProperty(popup, "position:y", -35.0, 0.8);
        tween.Parallel().TweenProperty(popup, "modulate:a", 0.0, 0.8);
        tween.TweenCallback(Callable.From(() => popup.QueueFree()));
    }
}

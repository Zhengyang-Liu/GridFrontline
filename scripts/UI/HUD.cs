using Godot;

namespace GridFrontline;

/// <summary>
/// Heads-up display showing gold count and debug controls.
/// </summary>
public partial class HUD : Control
{
    private EconomyManager _economy;
    private Label _goldLabel;

    public void SetEconomy(EconomyManager economy)
    {
        _economy = economy;
        _economy.GoldChanged += OnGoldChanged;
    }

    public override void _Ready()
    {
        // Let clicks pass through to the game world
        MouseFilter = MouseFilterEnum.Ignore;

        // Anchor to top-left
        AnchorLeft = 0;
        AnchorTop = 0;
        OffsetLeft = 20;
        OffsetTop = 10;

        // Gold display
        var panel = new PanelContainer();
        panel.MouseFilter = MouseFilterEnum.Pass;
        AddChild(panel);

        var hbox = new HBoxContainer();
        panel.AddChild(hbox);

        var goldIcon = new Label();
        goldIcon.Text = "💰";
        goldIcon.AddThemeFontSizeOverride("font_size", 24);
        hbox.AddChild(goldIcon);

        _goldLabel = new Label();
        _goldLabel.Text = "0";
        _goldLabel.AddThemeFontSizeOverride("font_size", 24);
        _goldLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.2f));
        hbox.AddChild(_goldLabel);

        // Spacer
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(30, 0);
        hbox.AddChild(spacer);

        // Debug: add gold button
        var debugBtn = new Button();
        debugBtn.Text = "+100 💰 (Debug)";
        debugBtn.Pressed += () => _economy?.AddGold(100);
        hbox.AddChild(debugBtn);
    }

    private void OnGoldChanged(int newAmount)
    {
        if (_goldLabel != null)
            _goldLabel.Text = newAmount.ToString();
    }
}

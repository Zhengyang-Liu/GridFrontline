using Godot;

namespace GridFrontline;

/// <summary>
/// Panel at the bottom of the screen for selecting buildings to place.
/// Phase 1: hardcoded Farm and Barracks buttons.
/// </summary>
public partial class BuildPanel : Control
{
    [Signal]
    public delegate void BuildingSelectedEventHandler(BuildingData data);

    private Button _farmButton;
    private Button _barracksButton;
    private Button _selectedButton;

    private static readonly Color SelectedColor = new(0.3f, 0.7f, 0.3f);
    private static readonly Color NormalColor = new(1f, 1f, 1f);

    public override void _Ready()
    {
        // Let clicks pass through to the game world
        MouseFilter = MouseFilterEnum.Ignore;

        // Anchor to bottom-center
        AnchorLeft = 0.5f;
        AnchorTop = 1f;
        AnchorRight = 0.5f;
        AnchorBottom = 1f;
        OffsetLeft = -200;
        OffsetTop = -90;
        OffsetRight = 200;
        OffsetBottom = -10;

        var panel = new PanelContainer();
        panel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        panel.MouseFilter = MouseFilterEnum.Pass;
        AddChild(panel);

        var hbox = new HBoxContainer();
        hbox.Alignment = BoxContainer.AlignmentMode.Center;
        hbox.AddThemeConstantOverride("separation", 20);
        panel.AddChild(hbox);

        // Farm button
        var farmData = BuildingDatabase.Farm;
        _farmButton = CreateBuildButton(farmData);
        _farmButton.Pressed += () => SelectBuilding(farmData, _farmButton);
        hbox.AddChild(_farmButton);

        // Barracks button
        var barracksData = BuildingDatabase.Barracks;
        _barracksButton = CreateBuildButton(barracksData);
        _barracksButton.Pressed += () => SelectBuilding(barracksData, _barracksButton);
        hbox.AddChild(_barracksButton);
    }

    private Button CreateBuildButton(BuildingData data)
    {
        var btn = new Button();
        btn.CustomMinimumSize = new Vector2(150, 60);
        btn.Text = $"{data.DisplayChar} {data.BuildingName}\n💰 {data.Cost}";
        return btn;
    }

    private void SelectBuilding(BuildingData data, Button btn)
    {
        // Toggle selection
        if (_selectedButton == btn)
        {
            ClearSelection();
            return;
        }

        ClearSelection();
        _selectedButton = btn;
        btn.Modulate = SelectedColor;
        EmitSignal(SignalName.BuildingSelected, data);
    }

    public void ClearSelection()
    {
        if (_selectedButton != null)
        {
            _selectedButton.Modulate = NormalColor;
            _selectedButton = null;
        }
    }
}

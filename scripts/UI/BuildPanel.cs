using Godot;
using System.Collections.Generic;

namespace GridFrontline;

/// <summary>
/// Panel at the bottom of the screen for selecting buildings to place.
/// </summary>
public partial class BuildPanel : Control
{
    [Signal]
    public delegate void BuildingSelectedEventHandler(BuildingData data);

    private readonly List<Button> _buttons = new();
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
        OffsetLeft = -450;
        OffsetTop = -90;
        OffsetRight = 450;
        OffsetBottom = -10;

        var panel = new PanelContainer();
        panel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        panel.MouseFilter = MouseFilterEnum.Pass;
        AddChild(panel);

        var hbox = new HBoxContainer();
        hbox.Alignment = BoxContainer.AlignmentMode.Center;
        hbox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(hbox);

        // All available buildings
        AddBuildingButton(hbox, BuildingDatabase.Farm);
        AddBuildingButton(hbox, BuildingDatabase.SwordsmanBarracks);
        AddBuildingButton(hbox, BuildingDatabase.ArcherRange);
        AddBuildingButton(hbox, BuildingDatabase.Stable);
        AddBuildingButton(hbox, BuildingDatabase.Wall);
        AddBuildingButton(hbox, BuildingDatabase.ArrowTower);
    }

    private void AddBuildingButton(HBoxContainer parent, BuildingData data)
    {
        var btn = new Button();
        btn.CustomMinimumSize = new Vector2(130, 60);
        btn.Text = $"{data.DisplayChar} {data.BuildingName}\n💰 {data.Cost}";
        btn.Pressed += () => SelectBuilding(data, btn);
        parent.AddChild(btn);
        _buttons.Add(btn);
    }

    private void SelectBuilding(BuildingData data, Button btn)
    {
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

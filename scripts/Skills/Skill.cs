using Godot;

namespace GridFrontline;

/// <summary>
/// Abstract base class for unit skills.
/// Skills are attached as child nodes of a Unit.
/// </summary>
public abstract partial class Skill : Node
{
    public new Unit Owner { get; set; }
    public string SkillName { get; set; } = "";

    /// <summary>Check if the skill can activate this frame.</summary>
    public abstract bool CanActivate();

    /// <summary>Execute the skill effect.</summary>
    public abstract void Activate();

    /// <summary>Called every frame to update internal state (energy, cooldowns).</summary>
    public virtual void UpdateSkill(double delta) { }

    /// <summary>Called when the owner attacks.</summary>
    public virtual void OnOwnerAttack() { }

    /// <summary>Called when the owner takes damage.</summary>
    public virtual void OnOwnerTakeDamage(int damage) { }

    /// <summary>Called when the owner dies.</summary>
    public virtual void OnOwnerDeath() { }

    protected void ShowSkillPopup(string text, Color color)
    {
        if (Owner == null) return;
        var popup = new Label();
        popup.Text = text;
        popup.AddThemeColorOverride("font_color", color);
        popup.AddThemeFontSizeOverride("font_size", 14);
        popup.Position = new Vector2(-15, -28);
        Owner.AddChild(popup);

        var tween = Owner.CreateTween();
        tween.TweenProperty(popup, "position:y", -50.0, 0.6);
        tween.Parallel().TweenProperty(popup, "modulate:a", 0.0, 0.6);
        tween.TweenCallback(Callable.From(() => popup.QueueFree()));
    }
}

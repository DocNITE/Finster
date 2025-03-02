using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Shared.Alert;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Systems.Alerts.Controls;

public class HUDAlertControl : HUDButton
{
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;

    public AlertPrototype Alert { get; }

    private (TimeSpan Start, TimeSpan End)? _cooldown;

    private HUDAnimatedTextureRect _textureRect;

    private short? _severity;

    public HUDAlertControl(AlertPrototype alert, short? severity)
    {
        IoCManager.InjectDependencies(this);

        Alert = alert;
        _severity = severity;
        _textureRect = new HUDAnimatedTextureRect();
        AddChild(_textureRect);

        Name = alert.Name;

        var icon = Alert.GetIcon(_severity);
        var sprite = _vpUIManager.GetThemeRsi(Alert.Sprite, icon);
        _textureRect.SetFromSpriteSpecifier(sprite);

        Size = _textureRect.Size;
        if (!alert.IsGeneric)
            Position = (alert.HudPositionX, alert.HudPositionY);
    }

    /// <summary>
    /// Change the alert severity, changing the displayed icon
    /// </summary>
    public void SetSeverity(short? severity)
    {
        if (_severity == severity)
            return;
        _severity = severity;

        var icon = Alert.GetIcon(_severity);
        var sprite = _vpUIManager.GetThemeRsi(Alert.Sprite, icon);
        _textureRect.SetFromSpriteSpecifier(sprite);
    }
}

public enum AlertVisualLayers : byte
{
    Base
}

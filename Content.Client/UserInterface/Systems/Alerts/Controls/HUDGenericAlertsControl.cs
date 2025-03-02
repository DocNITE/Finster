using System.Linq;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Shared.Alert;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.Alerts.Controls;

/// <summary>
/// Works similar like Alerts control, but should switch icons every each seconds
/// </summary>
public class HUDGenericAlertsControl : HUDTextureRect
{
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;

    private float _elapsedTime = 0f;
    private const float Interval = 1f; // 1 seconds

    private int _currentChild = -1;

    private HUDControl? _visibleControl;

    public HUDGenericAlertsControl()
    {
        IoCManager.InjectDependencies(this);

        Texture = _vpUIManager.GetThemeTexture("screen_background");
        Size = Texture.Size;
    }

    protected override void ChildAdded(HUDControl newChild)
    {
        ShowNextAlert(true);

        base.ChildAdded(newChild);
    }
    protected override void ChildRemoved(HUDControl child)
    {
        if (child == _visibleControl)
            ShowNextAlert(true);

        base.ChildRemoved(child);
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        // Accumulate the elapsed time
        _elapsedTime += args.DeltaSeconds;

        // Check if the interval has been reached
        if (_elapsedTime >= Interval)
        {
            // Execute your logic here
            ShowNextAlert();
        }
    }

    private void ShowNextAlert(bool forceToEnd = false)
    {
        var maxIdx = ChildCount - 1;

        if (!forceToEnd)
        {
            if (_currentChild > maxIdx)
                _currentChild = 0;
            else
                _currentChild++;
        }
        else
        {
            _currentChild = ChildCount - 1;
        }

        // Hide unnecessary alerts
        var idx = 0;
        foreach (var child in Children)
        {
            child.Visible = false;
            if (idx == _currentChild)
                _visibleControl = child;

            idx++;
        }

        // Show current alert
        if (_visibleControl != null)
            _visibleControl.Visible = true;

        // Reset the elapsed time
        _elapsedTime = 0f;
    }
}

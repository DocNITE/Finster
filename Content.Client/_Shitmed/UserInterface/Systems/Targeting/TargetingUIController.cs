using Content.Client.Gameplay;
using Content.Client._Shitmed.UserInterface.Systems.Targeting.Widgets;
using Content.Shared._Shitmed.Targeting;
using Content.Client._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.Player;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._Shitmed.UserInterface.Systems.Targeting.Controls;
using Content.Client.UserInterface.Systems.Alerts.Controls;

namespace Content.Client._Shitmed.UserInterface.Systems.Targeting;

public sealed class TargetingUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<TargetingSystem>
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!; // VPGui edit

    private TargetingComponent? _targetingComponent;
    //private TargetingControl? TargetingControl => UIManager.GetActiveUIWidgetOrNull<TargetingControl>();

    private HUDTargetDoll? TargetingControl;

    public void OnSystemLoaded(TargetingSystem system)
    {
        system.TargetingStartup += AddTargetingControl;
        system.TargetingShutdown += RemoveTargetingControl;
        system.TargetChange += CycleTarget;
    }

    public void OnSystemUnloaded(TargetingSystem system)
    {
        system.TargetingStartup -= AddTargetingControl;
        system.TargetingShutdown -= RemoveTargetingControl;
        system.TargetChange -= CycleTarget;

        TargetingControl?.Dispose();
        TargetingControl = null;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (TargetingControl == null)
            return;

        TargetingControl.SetTargetDollVisible(_targetingComponent != null);

        if (_targetingComponent != null)
            TargetingControl.SetBodyPartsVisible(_targetingComponent.Target);
    }

    public void AddTargetingControl(TargetingComponent component)
    {
        _targetingComponent = component;

        if (TargetingControl == null)
        {
            TargetingControl = new HUDTargetDoll(this);
            if (_vpUIManager.TryGetControl<HUDAlertsPanel>(out var panel) && panel is not null)
                panel.AddChild(TargetingControl);
        }

        TargetingControl.SetTargetDollVisible(_targetingComponent != null);

        if (_targetingComponent != null)
            TargetingControl.SetBodyPartsVisible(_targetingComponent.Target);
    }

    public void RemoveTargetingControl()
    {
        if (TargetingControl != null)
        {
            TargetingControl.SetTargetDollVisible(false);
            TargetingControl?.Dispose();
            TargetingControl = null;
        }

        _targetingComponent = null;
    }

    public void CycleTarget(TargetBodyPart bodyPart)
    {
        if (_playerManager.LocalEntity is not { } user
            || !_entManager.TryGetComponent<TargetingComponent>(user, out var targetingComponent)
            || TargetingControl == null)
            return;

        var player = _entManager.GetNetEntity(user);
        if (bodyPart != targetingComponent.Target)
        {
            var msg = new TargetChangeEvent(player, bodyPart);
            _net.SendSystemNetworkMessage(msg);
            TargetingControl?.SetBodyPartsVisible(bodyPart);
        }
    }
}

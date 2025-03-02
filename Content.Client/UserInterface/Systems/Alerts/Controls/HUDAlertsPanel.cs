using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Shared.Alert;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.UserInterface.Systems.Alerts.Controls;

/// <summary>
/// Right panel of the HUD.
/// </summary>
public class HUDAlertsPanel : HUDTextureRect
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private readonly Dictionary<AlertKey, HUDAlertControl> _alertControls = new();

    public event EventHandler<ProtoId<AlertPrototype>>? AlertPressed;

    /// <summary>
    /// Contains all controls, what should be placed on specific screen position.
    /// </summary>
    public HUDControl Container;

    /// <summary>
    /// Contains all generic or unnecessary alerts (like hunger, thrist, toxins, bleed, piloting...)
    /// </summary>
    public HUDGenericAlertsControl GenericContainer;

    public HUDAlertsPanel()
    {
        IoCManager.InjectDependencies(this);

        Container = new HUDControl();
        AddChild(Container);

        GenericContainer = new HUDGenericAlertsControl();
        GenericContainer.Position = (0, 416); // TODO: Should be configurated
        AddChild(GenericContainer);
    }

    public void SyncControls(AlertsSystem alertsSystem,
        AlertOrderPrototype? alertOrderPrototype,
        IReadOnlyDictionary<AlertKey,
        AlertState> alertStates)
    {
        // remove any controls with keys no longer present
        if (SyncRemoveControls(alertStates))
            return;

        // now we know that alertControls contains alerts that should still exist but
        // may need to updated,
        // also there may be some new alerts we need to show.
        // further, we need to ensure they are ordered w.r.t their configured order
        SyncUpdateControls(alertsSystem, alertOrderPrototype, alertStates);
    }

    public void ClearAllControls()
    {
        foreach (var alertControl in _alertControls.Values)
        {
            alertControl.OnPressed -= AlertControlPressed;
            alertControl.Dispose();
        }

        _alertControls.Clear();
    }

    private bool SyncRemoveControls(IReadOnlyDictionary<AlertKey, AlertState> alertStates)
    {
        var toRemove = new List<AlertKey>();
        foreach (var existingKey in _alertControls.Keys)
        {
            if (!alertStates.ContainsKey(existingKey))
                toRemove.Add(existingKey);
        }

        foreach (var alertKeyToRemove in toRemove)
        {
            _alertControls.Remove(alertKeyToRemove, out var control);
            if (control == null)
                return true;

            // If we wanna move some Alerts into another UI.
            control.Parent?.RemoveChild(control);
        }

        return false;
    }

    private HUDControl EnsureControlsContainer(AlertKey alertKey)
    {
        var alertControlsContainer = Container;
        var alertType = alertKey.AlertType;

        if (_prototypeManager.TryIndex<AlertPrototype>(alertType, out var alert) && alert.IsGeneric)
            alertControlsContainer = (HUDControl) GenericContainer;

        return alertControlsContainer;
    }

    private void SyncUpdateControls(AlertsSystem alertsSystem, AlertOrderPrototype? alertOrderPrototype,
        IReadOnlyDictionary<AlertKey, AlertState> alertStates)
    {
        foreach (var (alertKey, alertState) in alertStates)
        {
            if (!alertKey.AlertType.HasValue)
            {
                Logger.WarningS("alert", "found alertkey without alerttype," +
                                         " alert keys should never be stored without an alerttype set: {0}", alertKey);
                continue;
            }

            var alertType = alertKey.AlertType.Value;
            if (!alertsSystem.TryGet(alertType, out var newAlert))
            {
                Logger.ErrorS("alert", "Unrecognized alertType {0}", alertType);
                continue;
            }

            if (_alertControls.TryGetValue(newAlert.AlertKey, out var existingAlertControl) &&
                existingAlertControl.Alert.ID == newAlert.ID)
            {
                // key is the same, simply update the existing control severity / cooldown
                existingAlertControl.SetSeverity(alertState.Severity);
                //if (alertState.ShowCooldown)
                //    existingAlertControl.Cooldown = alertState.Cooldown;
            }
            else
            {
                // If we wanna move some Alerts into another UI.
                var alertControlsContainer = EnsureControlsContainer(alertKey);

                if (existingAlertControl != null)
                    alertControlsContainer.RemoveChild(existingAlertControl);

                // this is a new alert + alert key or just a different alert with the same
                // key, create the control and add it in the appropriate order
                var newAlertControl = CreateAlertControl(newAlert, alertState);

                //TODO: Can the presenter sort the states before giving it to us?
                if (alertOrderPrototype != null)
                {
                    var added = false;
                    foreach (var alertControl in alertControlsContainer.Children)
                    {
                        if (alertOrderPrototype.Compare(newAlert, ((HUDAlertControl) alertControl).Alert) >= 0)
                            continue;

                        var idx = alertControl.GetPositionInParent();
                        alertControlsContainer.AddChild(newAlertControl);
                        newAlertControl.SetPositionInParent(idx);
                        added = true;
                        break;
                    }

                    if (!added)
                        alertControlsContainer.AddChild(newAlertControl);
                }
                else
                {
                    alertControlsContainer.AddChild(newAlertControl);
                }

                _alertControls[newAlert.AlertKey] = newAlertControl;
            }
        }
    }

    private HUDAlertControl CreateAlertControl(AlertPrototype alert, AlertState alertState)
    {
        //(TimeSpan, TimeSpan)? cooldown = null;
        //if (alertState.ShowCooldown)
        //    cooldown = alertState.Cooldown;

        var alertControl = new HUDAlertControl(alert, alertState.Severity);
        alertControl.OnPressed += AlertControlPressed;
        return alertControl;
    }

    private void AlertControlPressed(HUDBoundKeyEventArgs args)
    {
        if (args.Button is not HUDAlertControl control)
            return;

        //if (args.Event.Function != EngineKeyFunctions.UIClick)
        //    return;

        AlertPressed?.Invoke(this, control.Alert.ID);
    }
}

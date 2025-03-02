using Content.Client._Finster.UserActions.Tabs;
using Content.Client._ViewportGui.ViewportUserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client._Finster.Lookup;

public sealed class LookupUIController : UIController, IOnSystemChanged<LookupSystem>
{
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;

    public HUDLookupLabel? LookupLabel;

    public void OnSystemLoaded(LookupSystem system)
    {
        LookupLabel = new();
        _vpUIManager.Root.AddChild(LookupLabel);

        //system.PlayerAttachedEvent += OnAttached;
        //system.PlayerDetachedEvent += OnDetached;
    }

    public void OnSystemUnloaded(LookupSystem system)
    {
        LookupLabel?.Dispose();
        LookupLabel = null;

        //system.PlayerAttachedEvent -= OnAttached;
        //system.PlayerDetachedEvent -= OnDetached;
    }

    private void OnAttached()
    {
    }

    private void OnDetached()
    {
    }
}

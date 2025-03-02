
using Robust.Client.Graphics;

namespace Content.Client._Finster.Lookup;
public sealed class LookupSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        //_overlay.AddOverlay(new LookupOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        //_overlay.RemoveOverlay<LookupOverlay>();
    }
}

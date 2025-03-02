using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// Enables the automatic voting system.
    public static readonly CVarDef<bool> PullingAnimationEffect =
        CVarDef.Create("pulling.animation_effect", false, CVar.REPLICATED);

    /// Automatically starts a map vote when returning to the lobby.
    /// Requires auto voting to be enabled.
}

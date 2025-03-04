using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    # region White Dream (OLD)

    public static readonly CVarDef<bool> LogChatActions =
        CVarDef.Create("white.log_to_chat", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    # endregion

    #region Development

    /// <summary>
    /// Run ShaderViewer only.
    /// </summary>
    public static readonly CVarDef<bool> LaunchShaderViewer =
        CVarDef.Create("launch.shader_viewer", false, CVar.CLIENTONLY);

    #endregion

    # region Z Levels

    public static readonly CVarDef<bool> ZLayersBackgroundShader =
        CVarDef.Create("zlayers.background_shader", true, CVar.CLIENT | CVar.REPLICATED);

    # endregion

    # region HUD

    public static readonly CVarDef<bool> ShowLookupHint =
        CVarDef.Create("hud.show_lookup_hint", true, CVar.ARCHIVE | CVar.CLIENTONLY);

    # endregion
}

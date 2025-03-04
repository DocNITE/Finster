using Robust.Shared.GameStates;

namespace Content.Shared._Finster.FieldOfView;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FieldOfViewComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool Enabled { get; set; } = true;

    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float Angle { get; set; } = 270.0f;

    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float Distance { get; set; } = 128.0f;
}

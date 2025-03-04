using Content.Server.Interaction;
using Content.Shared._White.Intent;
using Content.Shared.Timing;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Interactions;

public sealed partial class InteractWithOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    /// <summary>
    /// Key that contains the target entity.
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = default!;

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (_entManager.TryGetComponent<UseDelayComponent>(owner, out var useDelay) && _entManager.System<UseDelaySystem>().IsDelayed((owner, useDelay)) ||
            !blackboard.TryGetValue<EntityUid>(TargetKey, out var moveTarget, _entManager) ||
            !_entManager.TryGetComponent<TransformComponent>(moveTarget, out var targetXform))
        {
            return HTNOperatorStatus.Continuing;
        }

        if (_entManager.TryGetComponent<IntentComponent>(owner, out var intent)) // WD EDIT
            _entManager.System<SharedIntentSystem>().SetIntent(owner, Intent.Help, intent);

        _entManager.System<InteractionSystem>().UserInteraction(owner, targetXform.Coordinates, moveTarget);
        return HTNOperatorStatus.Finished;
    }
}

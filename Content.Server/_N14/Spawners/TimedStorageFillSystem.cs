using Content.Server.Spawners.Components;
using Content.Server.Storage.EntitySystems;
using Content.Server.Storage.Components;
using Content.Shared.Storage;
using Robust.Shared.Random;

namespace Content.Server._N14.Spawners;

public sealed class TimedStorageFillSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly StorageSystem _storageSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TimedStorageFillComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<TimedStorageFillComponent, AnchorStateChangedEvent>(OnUnanchor);
    }

    private void OnStartup(EntityUid uid, TimedStorageFillComponent component, MapInitEvent _)
    {
        FillStorage(uid, component);
        component.NextRefillTime = _robustRandom.NextFloat(component.MinimumSeconds, component.MaximumSeconds);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<TimedStorageFillComponent>();
        while (query.MoveNext(out var timedComp))
        {
            timedComp.NextRefillTime -= frameTime;

            if (timedComp.NextRefillTime < 0)
            {
                FillStorage(timedComp.Owner, timedComp);
                timedComp.NextRefillTime = frameTime + _robustRandom.NextFloat(timedComp.MinimumSeconds, timedComp.MaximumSeconds);
            }
        }
    }

    private void FillStorage(EntityUid uid, TimedStorageFillComponent component)
    {
        if (component.Contents.Count == 0)
            return;

        TryComp<StorageComponent>(uid, out var serverStorageComp);
        TryComp<EntityStorageComponent>(uid, out var entityStorageComp);

        if (entityStorageComp == null && serverStorageComp == null)
        {
            Logger.Error($"StorageFillComponent couldn't find any StorageComponent ({uid})");
            return;
        }

        if (serverStorageComp?.Container?.ContainedEntities.Count > 0 || entityStorageComp?.Contents.ContainedEntities.Count > 0)
            return;

        var coordinates = Transform(uid).Coordinates;

        var spawnItems = EntitySpawnCollection.GetSpawns(component.Contents, _robustRandom);
        foreach (var item in spawnItems)
        {
            var ent = EntityManager.SpawnEntity(item, coordinates);

            // handle depending on storage component, again this should be unified after ECS
            if (entityStorageComp != null && _entityStorage.Insert(ent, uid, entityStorageComp))
               continue;

            if (serverStorageComp != null && _storageSystem.Insert(uid, ent, out _))
                continue;

            Logger.ErrorS("storage", $"Tried to StorageFill {item} inside {ToPrettyString(uid)} but can't.");
            EntityManager.DeleteEntity(ent);
        }
    }

    private void OnUnanchor(EntityUid uid, TimedStorageFillComponent component, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            RemCompDeferred<TimedStorageFillComponent>(uid);
        }
    }
}

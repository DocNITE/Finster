using System.Diagnostics.CodeAnalysis;
using Content.KayMisaZlevels.Server.Components;
using Content.KayMisaZlevels.Shared.Components;
using Content.KayMisaZlevels.Shared.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.KayMisaZlevels.Server.Systems;

public sealed partial class ZDefinedStackSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ZStackSystem _zStack = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZDefinedStackComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ZDefinedStackComponent> initialMapUid, ref MapInitEvent args)
    {
        // If there is nothing tracker comp (like - round start, instead of mapping)
        LoadMap(initialMapUid, initialMapUid.Comp, initializeMaps: true);
    }

    public bool LoadMap(EntityUid initialMapUid, ZDefinedStackComponent? defStackComp = null, bool initializeMaps = false)
    {
        if (!Resolve(initialMapUid, ref defStackComp))
            return false;

        // We should use our initial map as stack for Z levels
        // Also, check if tracker comp already exist.
        // It need for mapping tools, instead of round gaming.
        var stackLoc = (EntityUid?) initialMapUid;
        if (TryComp<ZStackTrackerComponent>(initialMapUid, out var _))
            return false;
        else
            AddComp<ZStackTrackerComponent>(initialMapUid);

        // Load levels downer
        foreach (var path in defStackComp.DownLevels)
        {
            LoadLevel(stackLoc, path, initializeMaps: initializeMaps);
        }

        // Add initial map as lowest level in the world
        _zStack.AddToStack(initialMapUid, ref stackLoc);

        // Load level upper
        foreach (var path in defStackComp.UpLevels)
        {
            LoadLevel(stackLoc, path, initializeMaps: initializeMaps);
        }

        return true;
    }

    /// <summary>
    /// Load children map of the map.
    /// </summary>
    /// <param name="stackLoc">What the fuck is this?</param>
    /// <param name="path">YAML Map path</param>
    /// <param name="initializeMaps">Should we initialize maps whe it was loaded.</param>
    public void LoadLevel(EntityUid? stackLoc, ResPath path, bool initializeMaps = false)
    {
        var options = new DeserializationOptions()
        {
            InitializeMaps = initializeMaps
        };

        if (_mapLoader.TryLoadMap(path, out var map, out _, options: options))
        {
            // Add to stack
            _zStack.AddToStack(map.Value, ref stackLoc);

            // Mark as a member of defined maps. It needs for multi saving
            AddComp(map.Value,
                new ZDefinedStackMemberComponent()
                {
                    SavePath = path
                });

            Log.Info($"Created map {map.Value} for ZDefinedStackSystem system");
        }
        else
        {
            Log.Error($"Failed to load map from {path}!");
            return;
        }
    }
}

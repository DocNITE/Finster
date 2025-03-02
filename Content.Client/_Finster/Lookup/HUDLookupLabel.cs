using System.Numerics;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Client.UserInterface.Systems.Inventory.Controls;
using Content.Client.Viewport;
using Content.Shared.CCVar;
using Content.Shared.Maps;
using MathNet.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._Finster.Lookup;

public class HUDLookupLabel : HUDControl
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    private string _fontPath = "/Fonts/home-video-font/HomeVideo-BLG6G.ttf";
    private Font _font;
    private string _text = string.Empty;

    /// <summary>
    /// Text's font scale.
    /// </summary>
    public int Scale { get; set; } = 8;

    /// <summary>
    /// Return current font path or set a new font with the path.
    /// </summary>
    public string FontPath
    {
        get => _fontPath;
        set
        {
            _fontPath = value;
            _font = new VectorFont(_cache.GetResource<FontResource>(_fontPath), Scale);
        }
    }

    public HUDLookupLabel()
    {
        IoCManager.InjectDependencies(this);

        _font = new VectorFont(_cache.GetResource<FontResource>(_fontPath), Scale);
        _cfg.OnValueChanged(CCVars.ShowLookupHint, (toggle) =>
        {
            Visible = toggle;
        }, true);
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
    }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        base.Draw(args);

        var handle = args.ScreenHandle;
        var bounds = args.ContentSize;

        // TODO: Make it more modularity
        _text = string.Empty;

        // First - try to find focused HUD controls, instead find tiles or entity in the world
        var hudBoundsArgs = _vpUIManager.TryFindHUDControl();
        if (hudBoundsArgs is not null && (hudBoundsArgs.Value.IsFocused || hudBoundsArgs.Value.InBounds))
            FindInHUD(hudBoundsArgs);
        else
            FindInWorld();

        var targetX = bounds.X / 2;
        var targetY = Scale * 2;

        var dimensions = handle.GetDimensions(_font, _text, 1f);
        handle.DrawString(_font,
            new Vector2(targetX, targetY) - new Vector2(dimensions.X / 2, dimensions.Y + (Scale / 2)),
            _text,
            1f,
            Color.Gainsboro.WithAlpha(0.25f));
    }

    private void FindInHUD(HUDBoundsCheckArgs? args)
    {
        if (args is null)
            return;

        var control = args.Value.FocusedControl;
        if (control is null)
            return;

        switch (control)
        {
            case HUDAlertControl alertControl:
                if (alertControl.Name is not null)
                {
                    _text = Loc.GetString(alertControl.Name);
                    return;
                }
                break;
            case HUDSlotControl slotControl:
                if (slotControl.Entity is not null &&
                    _entManager.TryGetComponent<MetaDataComponent>(slotControl.Entity, out var metaData))
                {
                    _text = Loc.GetString(metaData.EntityName);
                    return;
                }

                _text = Loc.GetString(slotControl.HoverName);
                return;
                break;
        }

        // TODO: Maybe need make something like "FocusName"?
        if (control.Name is not null)
            _text = control.Name;
    }

    private void FindInWorld()
    {
        var mouseScreenPos = _inputManager.MouseScreenPosition;
        var mousePos = _eyeManager.ScreenToMap(mouseScreenPos);

        var examineSys = _entManager.System<ExamineSystem>();
        var mapSys = _entManager.System<SharedMapSystem>();

        EntityCoordinates mouseGridPos;
        TileRef? tile = null;

        if (_player.LocalEntity is null)
            return;
        if (!_entManager.TryGetComponent<TransformComponent>(_player.LocalEntity, out var xformComp))
            return;

        if (mousePos.MapId == MapId.Nullspace || mousePos.MapId != xformComp.MapID)
            return;

        var mapUid = _mapManager.GetMapEntityId(xformComp.MapID);
        //var nodePos = _maps.WorldToTile(mapUid, grid, mousePos.Position);

        if (!examineSys.CanExamine(_player.LocalEntity.Value, mousePos))
            return;

        if (mousePos != MapCoordinates.Nullspace)
        {
            if (_mapManager.TryFindGridAt(mousePos, out var mouseGridUid, out var mouseGrid))
            {
                mouseGridPos = mapSys.MapToGrid(mouseGridUid, mousePos);
                tile = mapSys.GetTileRef(mouseGridUid, mouseGrid, mouseGridPos);
            }
            else
            {
                mouseGridPos = new EntityCoordinates(mapUid, mousePos.Position);
                tile = null;
            }
        }

        var currentState = _stateManager.CurrentState;
        if (currentState is not GameplayStateBase screen)
            return;

        var entityToClick = screen.GetClickedEntity(mousePos);

        if (entityToClick is not null &&
            _entManager.TryGetComponent<MetaDataComponent>(entityToClick, out var metaComp))
        {
            //if (_examine.CanExamine(_player.LocalEntity.Value, entityToClick.Value))
            _text = metaComp.EntityName;
        }
        else if (tile is not null)
        {
            var tileDef = (ContentTileDefinition) _tileDefManager[tile.Value.Tile.TypeId];
            if (tileDef.ID != ContentTileDefinition.SpaceID)
                _text = $"{Loc.GetString(tileDef.Name)}";
        }
    }
}

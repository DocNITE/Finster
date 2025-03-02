using System.Numerics;
using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.Client.Viewport;
using Content.KayMisaZlevels.Client;
using Content.Shared.CCVar;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client._Finster.Lookup;

public sealed class LookupOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private SharedMapSystem _maps;
    private TileSystem _tile;
    private SharedTransformSystem _xform;
    private ExamineSystem _examine;

    private Font _font;
    private int _fontScale = 16;
    private bool _showHint = true;

    public LookupOverlay()
    {
        IoCManager.InjectDependencies(this);

        //_biomes = _entManager.System<BiomeSystem>();
        _maps = _entManager.System<SharedMapSystem>();
        _tile = _entManager.System<TileSystem>();
        _xform = _entManager.System<SharedTransformSystem>();
        _examine = _entManager.System<ExamineSystem>();

        _font = new VectorFont(_cache.GetResource<FontResource>("/Fonts/home-video-font/HomeVideo-BLG6G.ttf"), _fontScale);
        _cfg.OnValueChanged(CCVars.ShowLookupHint, (toggle) => {
            _showHint = toggle;
        }, true);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return _showHint;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = (args.ViewportControl as ScalingViewport);
        var uiScale = (args.ViewportControl as ScalingViewport)?.UIScale ?? 1f;

        var mouseScreenPos = _inputManager.MouseScreenPosition;
        var mousePos = _eyeManager.ScreenToMap(mouseScreenPos);

        EntityCoordinates mouseGridPos;
        TileRef? tile = null;

        if (mousePos.MapId == MapId.Nullspace || mousePos.MapId != args.MapId)
            return;

        var mapUid = _mapManager.GetMapEntityId(args.MapId);

        var strContent = "";
        //var nodePos = _maps.WorldToTile(mapUid, grid, mousePos.Position);

        if (_player.LocalEntity is null)
            return;

        if (!_examine.CanExamine(_player.LocalEntity.Value, mousePos))
            return;

        if (mousePos != MapCoordinates.Nullspace)
        {
            if (_mapManager.TryFindGridAt(mousePos, out var mouseGridUid, out var mouseGrid))
            {
                mouseGridPos = _maps.MapToGrid(mouseGridUid, mousePos);
                tile = _maps.GetTileRef(mouseGridUid, mouseGrid, mouseGridPos);
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
            strContent = metaComp.EntityName;
        }
        else if (tile is not null)
        {
            var tileDef = (ContentTileDefinition) _tileDefManager[tile.Value.Tile.TypeId];
            if (tileDef.ID != ContentTileDefinition.SpaceID)
                strContent = $"{Loc.GetString(tileDef.Name)}";
        }

        if (viewport is null)
            return;

        var dimensions = args.ScreenHandle.GetDimensions(_font, strContent, uiScale);
        var drawBox = viewport.GetDrawBox();
        var drawBoxGlobal = drawBox.Translated(viewport.GlobalPixelPosition);
        //var center = viewport.PixelSizeBox.Center; // drawBoxGlobal.Center;
        var center = drawBoxGlobal.Right - ((drawBoxGlobal.Right - drawBoxGlobal.Left) / 2);
        var bottom = viewport.PixelSizeBox.Bottom;

        args.ScreenHandle.DrawString(_font, new Vector2(center, bottom) - new Vector2(dimensions.X / 2, dimensions.Y + (_fontScale / 2)), strContent, uiScale, Color.Gray);
    }
}

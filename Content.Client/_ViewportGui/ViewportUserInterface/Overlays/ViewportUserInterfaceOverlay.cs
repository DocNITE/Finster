using System.Numerics;
using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Viewport;
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
using Robust.Shared.Timing;

namespace Content.Client._ViewportGui.ViewportUserInterface.Overlays;

public sealed class ViewportUserInterfaceOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    private ViewportUIController _viewportUIController;
    private ScalingViewport? _viewport;

    private Vector2i _viewportPosition;
    private Vector2i _viewportSize;
    private Vector2i _contentSize;

    private IRenderTexture _buffer;

    public ViewportUserInterfaceOverlay()
    {
        IoCManager.InjectDependencies(this);

        _viewportUIController = _uiManager.GetUIController<ViewportUIController>();

        // TODO: Move position definition into CVar
        // Or into prototype, instead of using xaml or avalonia
        // Also, why we define it in overlay too, instead manager?
        // Well, because we need define _buffer with _contentSize.
        // Maybe i rewrite it later...
        _viewportSize = new Vector2i(CCVars.ViewportWidth.DefaultValue, ViewportUIController.ViewportHeight);
        _viewportPosition = new Vector2i(0, 0);
        _contentSize = new Vector2i((_viewportSize.X + 1) * EyeManager.PixelsPerMeter, _viewportSize.Y * EyeManager.PixelsPerMeter);

        /*
        _cfg.OnValueChanged(CCVars.ViewportWidth, (newValue) =>
        {
            _viewportSize.X = newValue;
            RestoreBuffer();
            ResolveViewport();
        });
        */

        _buffer = _clyde.CreateRenderTarget(
            _contentSize,
            RenderTargetColorFormat.Rgba8Srgb);

        ResolveViewport();
    }

    private void RestoreBuffer()
    {
        _buffer.Dispose();
        _buffer = _clyde.CreateRenderTarget(
            _contentSize,
            RenderTargetColorFormat.Rgba8Srgb);
    }

    private void ResolveViewport(ScalingViewport? control = null)
    {
        // FIXME: Need send some information into VPGui manager for KeyBind events.
        // Need refactor or rewrite idk just make it better than this shit
        _vpUIManager.DrawingInfo = new ViewportDrawingInfo(_viewportPosition, _viewportSize, _contentSize);

        // What the fuck i am douing?
        var mainViewport = _uiManager.ActiveScreen?.GetWidget<MainViewport>();
        if (mainViewport is null)
            return;

        if (control is null)
            _viewport = mainViewport.Viewport;
        else
            _viewport = control;

        _vpUIManager.Viewport = _viewport;
        _vpUIManager.Viewport.OffsetSize = (1, 0); // TODO: Also, need be configured
    }

    protected override void DisposeBehavior()
    {
        base.DisposeBehavior();

        _buffer.Dispose();
    }

    /*
    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return true;
    }
    */

    protected override void FrameUpdate(FrameEventArgs args)
    {
        _vpUIManager.FrameUpdate(args);

        base.FrameUpdate(args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var viewport = (args.ViewportControl as ScalingViewport);
        var uiScale = (args.ViewportControl as ScalingViewport)?.UIScale ?? 1f;

        if (viewport is null)
            return;
        if (_viewport is null)
        {
            ResolveViewport(_viewport);
            return; // Because we can draw in next frame
        }

        var drawBounds = _vpUIManager.GetDrawingBounds();
        if (drawBounds is null)
            return;

        var drawingArgs = new ViewportUIDrawArgs(_buffer, _contentSize, drawBounds.Value.DrawBox, drawBounds.Value.Scale, viewport, handle);
        _vpUIManager.Draw(drawingArgs);

        handle.DrawTextureRect(_buffer.Texture, drawBounds.Value.DrawBox);
    }
}

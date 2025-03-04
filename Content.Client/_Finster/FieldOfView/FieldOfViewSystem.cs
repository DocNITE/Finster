using Content.Shared._Finster.FieldOfView;
using Content.Shared.Movement.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;

namespace Content.Client._Finster.FieldOfView;

public sealed class FieldOfViewSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    /*
    private EntityQuery<AirtightComponent> _airtightQuery;
    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<NavMapComponent> _navQuery;
    */
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<MobMoverComponent> _mobMoverQuery;
    private EntityQuery<SpriteComponent> _spriteQuery;

    public override void Initialize()
    {
        _xformQuery = GetEntityQuery<TransformComponent>();
        _mobMoverQuery = GetEntityQuery<MobMoverComponent>();
        _spriteQuery = GetEntityQuery<SpriteComponent>();

        //_overlay.AddOverlay(new LookupOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        //_overlay.RemoveOverlay<LookupOverlay>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalEntity is null)
            return;

        var playerUid = _player.LocalEntity.Value;

        if (!TryComp<FieldOfViewComponent>(playerUid, out var fovComp))
            return;

        if (!_xformQuery.TryGetComponent(playerUid, out var xform) ||
            xform.MapUid == null)
            return;

        var lookup = _lookup.GetEntitiesInRange(playerUid, 64, LookupFlags.Dynamic);
        //LookupFlags.Dynamic

        foreach (var ent in lookup)
        {
            if (!_mobMoverQuery.TryComp(ent, out var moverComp) ||
                !_spriteQuery.TryComp(ent, out var sprite))
                continue;

            sprite.Visible = IsInFieldOfView(playerUid, ent, fovComp.Angle, fovComp.Distance);
        }
    }

    public bool IsInFieldOfView(
            EntityUid viewer,
            EntityUid target,
            float fovAngle,
            float maxDistance,
            float offsetAngle = -90f,
            TransformComponent? viewerTransform = null,
            TransformComponent? targetTransform = null)
    {
        if (!Resolve(viewer, ref viewerTransform) ||
            !Resolve(target, ref targetTransform))
            return false;

        // Вычисляем направление от наблюдателя к цели
        var direction = targetTransform.WorldPosition - viewerTransform.WorldPosition;

        // Проверяем расстояние
        if (direction.LengthSquared() > maxDistance * maxDistance)
            return false;

        // Вычисляем угол между направлением взгляда наблюдателя и направлением на цель
        var viewerAngle = viewerTransform.WorldRotation; // Угол поворота наблюдателя
        var angleToTarget = Math.Atan2(direction.Y, direction.X); // Угол до цели

        // Нормализуем углы в диапазоне [0, 2π)
        viewerAngle = NormalizeAngle(viewerAngle);
        angleToTarget = NormalizeAngle(angleToTarget);

        // Добавляем смещение (offsetAngle) к углу взгляда наблюдателя
        float offsetAngleRadians = MathHelper.DegreesToRadians(offsetAngle);
        viewerAngle += offsetAngleRadians;

        // Нормализуем угол после добавления смещения
        viewerAngle = NormalizeAngle(viewerAngle);

        // Вычисляем разницу между углами
        var angleDifference = Math.Abs(viewerAngle - angleToTarget);

        // Если разница больше π, корректируем её
        if (angleDifference > Math.PI)
            angleDifference = 2 * Math.PI - angleDifference;

        // Переводим fovAngle из градусов в радианы
        float fovAngleRadians = MathHelper.DegreesToRadians(fovAngle);

        // Проверяем, попадает ли цель в поле зрения
        return angleDifference <= fovAngleRadians / 2;
    }

    private static double NormalizeAngle(double angle)
    {
        angle = (angle % (2 * Math.PI)); // Приводим угол к диапазону [0, 2π)
        if (angle < 0)
            angle += 2 * Math.PI; // Если угол отрицательный, добавляем 2π
        return angle;
    }
}

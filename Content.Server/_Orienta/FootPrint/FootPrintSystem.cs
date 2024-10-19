using System.Numerics;
using Content.Server.Decals;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Server._Orienta.FootPrint;

public sealed class FootPrintSystem : EntitySystem
{
    [Dependency] private readonly DecalSystem _decal = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
	
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FootPrintTrailerComponent, MoveEvent>(OnTrailerMove);
        SubscribeLocalEvent<FootPrintTrailerComponent, StartCollideEvent>(OnTrailerCollide);
        SubscribeLocalEvent<FootPrintComponent, GotEquippedEvent>(OnHolderEquipped);
        SubscribeLocalEvent<FootPrintComponent, GotUnequippedEvent>(OnHolderUnequipped);
        SubscribeLocalEvent<FootPrintCleanerComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
    }
    private void OnBeforeInteract(Entity<FootPrintCleanerComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (!args.CanReach)
            return;
        // Remove old decals
        var gridUid = Transform(ent).GridUid;
		
        if (gridUid is null)
            return;
		
        if (!TryComp<MapGridComponent>(gridUid, out var map))
            return;
		
        var vec = _transform.GetGridOrMapTilePosition(ent);
        var tileCenterVec = vec + map.TileSizeHalfVector;
        var oldDecals = _decal.GetDecalsInRange(gridUid.Value, tileCenterVec, ent.Comp.Range);
		
        foreach (var (id, decal) in oldDecals)
        {
            if (decal.Cleanable)
                _decal.RemoveDecal(gridUid.Value, id);
        }
    }
    private void OnHolderUnequipped(Entity<FootPrintComponent> ent, ref GotUnequippedEvent args)
    {
        if (!TryComp<FootPrintTrailerComponent>(args.Equipee, out var trailer))
            return;
        trailer.holder = null;
		
        if (TryComp<FootPrintComponent>(args.Equipee, out var selfHolder))
        {
            trailer.holder = selfHolder;
        }
    }
	
    private void OnHolderEquipped(Entity<FootPrintComponent> ent, ref GotEquippedEvent args)
    {
        if (!TryComp<FootPrintTrailerComponent>(args.Equipee, out var trailer))
            return;
        trailer.holder = ent.Comp;
    }
	
    private void OnTrailerCollide(Entity<FootPrintTrailerComponent> ent, ref StartCollideEvent args)
    {
        if (ent.Comp.holder is null)
            return;
        var footprint = ent.Comp.holder;
        if (!TryComp<PuddleComponent>(args.OtherEntity, out var puddle))
            return;
        if (puddle.Solution is null)
            return;
		
        var sol = puddle.Solution;
        var splittedSol = sol.Value.Comp.Solution.SplitSolutionWithout(footprint.PickSolution, SharedPuddleSystem.EvaporationReagents);
		
        if (splittedSol.Volume > 0)
            UpdateFootprint(footprint, splittedSol.GetColor(_proto));
    }
	
    private void UpdateFootprint(FootPrintComponent comp, Color color)
    {
        comp.FootPrintColor = color;
        comp.Intensity = 1f;
    }
	
    private void OnTrailerMove(Entity<FootPrintTrailerComponent> ent, ref MoveEvent args)
    {
        if (ent.Comp.holder is null)
            return;
        var footprint = ent.Comp.holder;
        var distance = Vector2.Distance(args.OldPosition.Position, args.NewPosition.Position);
        footprint.DistanceTraveled += distance;
		
        if (footprint.DistanceTraveled < footprint.FootPrintDistance)
            return;
        footprint.DistanceTraveled = 0f;
        var xform = Transform(ent);
		
        if (!TryComp<MapGridComponent>(xform.GridUid, out var mapGrid))
            return;
		
        var tileRef = _maps.GetTileRef(xform.GridUid.Value, mapGrid, xform.Coordinates);
        var tileDef = (ContentTileDefinition)_tileDefManager[tileRef.Tile.TypeId];
		
        if (tileDef.Weather && tileDef.Color is not null)
        {
            UpdateFootprint(footprint, tileDef.Color.Value);
            return;
        }
		
        if (footprint.Intensity <= 0)
            return;
        _decal.TryAddDecal(footprint.SlediProto,
            xform.Coordinates.Offset(new Vector2(-0.5f, -0.5f)),
            out var decal,
            footprint.FootPrintColor.WithAlpha(footprint.Intensity),
            xform.LocalRotation,
            cleanable: true);
        footprint.Intensity = MathF.Max(0, footprint.Intensity - footprint.DistanceIntensityCost);
    }
}

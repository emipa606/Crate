using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ThingBag;

[StaticConstructorOnStartup]
public class ThingBagComp : ThingComp
{
    private static readonly Texture2D rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename");

    private static readonly Texture2D pack = ContentFinder<Texture2D>.Get("Crate/CratePack");

    private static readonly Texture2D unpack = ContentFinder<Texture2D>.Get("Crate/CrateUnpack");

    public readonly ThingFilter Filter = new();

    public float ContentMass;

    public List<Thing> Items = [];

    public string Label = "";

    private List<Thing> plan = [];

    public ThingBag_Properties Props => props as ThingBag_Properties;

    public float Fill => ContentMass / Props.MaxMass;

    private bool Full => ContentMass > Props.MaxMass;

    private bool Empty => Items.Count == 0;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        Filter.CopyAllowancesFrom(Props.DefaultFilter);
    }

    public override string TransformLabel(string label)
    {
        return string.IsNullOrEmpty(Label) ? label : Label;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref Items, "items", LookMode.Deep);
        Scribe_Values.Look(ref Label, "label", "");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            ContentMass = Items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
        }
    }

    public void UnpackOne(IntVec3 pos, Map map, Thing item)
    {
        if (Items.Count == 0 || !Items.Contains(item))
        {
            return;
        }

        if (GenPlace.TryPlaceThing(item, pos, map, ThingPlaceMode.Direct))
        {
            Items.Remove(item);
        }

        ContentMass = Items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
    }

    public Thing UnpackOneRaw(Thing item)
    {
        if (Items.Count == 0)
        {
            return null;
        }

        if (!Items.Contains(item))
        {
            return null;
        }

        Items.Remove(item);
        ContentMass = Items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
        return item;
    }

    public bool CanPack(Thing thing)
    {
        if (!Filter.Allows(thing))
        {
            return false;
        }

        var num = Props.MaxMass - ContentMass;
        return num > thing.GetStatValue(StatDefOf.Mass);
    }

    public bool PackOne(Thing thing)
    {
        if (!CanPack(thing))
        {
            return false;
        }

        var num = Props.MaxMass - ContentMass;
        var num2 = Mathf.FloorToInt(num / thing.GetStatValue(StatDefOf.Mass));
        var splitted = false;
        if (num2 < thing.stackCount)
        {
            thing = thing.SplitOff(num2);
            splitted = true;
        }

        foreach (var item in Items)
        {
            if (!item.TryAbsorbStack(thing, true))
            {
                continue;
            }

            ContentMass = Items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
            return !splitted;
        }

        Items.Add(thing);
        if (thing.Spawned)
        {
            thing.DeSpawn();
        }

        ContentMass = Items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
        return !splitted;
    }

    private void startPlanPack()
    {
        plan.Clear();
    }

    private bool planCanPack(Thing thing)
    {
        if (!Filter.Allows(thing))
        {
            return false;
        }

        return !(thing.GetStatValue(StatDefOf.Mass) + ContentMass +
            plan.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount) > Props.MaxMass);
    }

    private void PlanPackOne(Thing thing)
    {
        if (!planCanPack(thing))
        {
            return;
        }

        plan.Add(thing);
    }

    private void startPlanUnpack(List<Thing> planitems = null)
    {
        plan = planitems != null ? planitems.Where(i => Items.Contains(i)).ToList() : Items.ToList();
    }

    private Thing PlanUnpackOne()
    {
        if (plan.Count <= 0)
        {
            return null;
        }

        var result = plan[0];
        plan.RemoveAt(0);
        return result;
    }

    public Job Build_PackJob(IntVec3 pos, Pawn pawn)
    {
        var job = new Job(DefDatabase<JobDef>.GetNamed("PackBag"), parent);
        startPlanPack();
        foreach (var item in GenRadial.RadialCellsAround(pos, Props.Radius, true))
        {
            var firstItem = item.GetFirstItem(parent.Map);
            if (firstItem != null)
            {
                Log.Message($"Can pack {firstItem} = {planCanPack(firstItem)}");
            }

            if (firstItem == null || !planCanPack(firstItem) ||
                !pawn.CanReserveAndReach((LocalTargetInfo)firstItem, PathEndMode.Touch, Danger.Deadly))
            {
                continue;
            }

            PlanPackOne(firstItem);
            job.AddQueuedTarget(TargetIndex.B, firstItem);
        }

        if (job.targetQueueB == null || job.targetQueueB.Count == 0)
        {
            return null;
        }

        job.targetQueueB.SortBy(targ => targ.Cell.DistanceToSquared(pawn.Position));
        Log.Message($"Job: {job}");
        return job;
    }

    public Job Build_PackJobSpecific(Pawn pawn, List<Thing> things)
    {
        var job = new Job(DefDatabase<JobDef>.GetNamed("PackBag"), parent);
        startPlanPack();
        foreach (var item in things)
        {
            if (item == null || !planCanPack(item) ||
                !pawn.CanReserveAndReach((LocalTargetInfo)item, PathEndMode.Touch, Danger.Deadly))
            {
                continue;
            }

            PlanPackOne(item);
            job.AddQueuedTarget(TargetIndex.B, item);
        }

        if (job.targetQueueB == null || job.targetQueueB.Count == 0)
        {
            return null;
        }

        job.targetQueueB.SortBy(targ => targ.Cell.DistanceToSquared(pawn.Position));
        Log.Message($"Job: {job}");
        return job;
    }

    public Job Build_PackSingleJob(IntVec3 pos, Pawn pawn, Thing item)
    {
        _ = new Job(DefDatabase<JobDef>.GetNamed("PackBagSingle"), parent);
        startPlanPack();
        if (item != null && planCanPack(item) &&
            pawn.CanReserveAndReach((LocalTargetInfo)item, PathEndMode.Touch, Danger.Deadly))
        {
            return new Job(DefDatabase<JobDef>.GetNamed("PackBagSingle"), parent, item);
        }

        return null;
    }

    public Job Build_UnpackJob(IntVec3 pos, Pawn pawn, List<Thing> things = null)
    {
        var job = new Job(DefDatabase<JobDef>.GetNamed("UnpackBag"));
        job.SetTarget(TargetIndex.C, parent);
        startPlanUnpack(things);
        var thing = PlanUnpackOne();
        foreach (var item in GenRadial.RadialCellsAround(pos, Props.Radius, true))
        {
            if (thing == null)
            {
                break;
            }

            if (GenPlace.HaulPlaceBlockerIn(thing, item, parent.Map, true) != null ||
                !pawn.CanReserveAndReach(item, PathEndMode.Touch, Danger.Deadly))
            {
                continue;
            }

            job.AddQueuedTarget(TargetIndex.A, thing);
            job.AddQueuedTarget(TargetIndex.B, item);
            thing = PlanUnpackOne();
        }

        if (job.targetQueueB == null || job.targetQueueB.Count == 0)
        {
            return null;
        }

        job.targetQueueB.SortBy(targ => targ.Cell.DistanceToSquared(pawn.Position));
        return job;
    }

    public Job Build_UnpackSingleJob(IntVec3 pos, Pawn pawn, Thing item = null)
    {
        var job = new Job(DefDatabase<JobDef>.GetNamed("UnpackBagSingle"));
        job.SetTarget(TargetIndex.C, parent);
        startPlanUnpack([item]);
        var thing = PlanUnpackOne();
        foreach (var item2 in GenRadial.RadialCellsAround(pos, Props.Radius, true))
        {
            if (GenPlace.HaulPlaceBlockerIn(thing, item2, parent.Map, true) != null ||
                !pawn.CanReserveAndReach(item2, PathEndMode.Touch, Danger.Deadly))
            {
                continue;
            }

            job.SetTarget(TargetIndex.A, thing);
            job.SetTarget(TargetIndex.B, item2);
            return job;
        }

        return null;
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        foreach (var item in Items)
        {
            item.Destroy(mode);
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = "Rename".Translate(),
            icon = rename,
            action = delegate { Find.WindowStack.Add(new Dialog_RenameCrate(this)); }
        };
        if (!Full)
        {
            yield return new Command_Action
            {
                defaultLabel = "Pack".Translate(),
                icon = pack,
                action = delegate
                {
                    var targetParams2 = new TargetingParameters
                    {
                        canTargetSelf = false,
                        canTargetBuildings = false,
                        canTargetFires = false,
                        canTargetItems = false,
                        canTargetPawns = false,
                        canTargetLocations = true
                    };
                    Find.Targeter.BeginTargeting(targetParams2,
                        delegate(LocalTargetInfo t)
                        {
                            Find.CurrentMap.GetThingBagTasks().AddTask(true, parent, null, t.Cell);
                        });
                }
            };
        }

        if (Empty)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "Unpack".Translate(),
            icon = unpack,
            action = delegate
            {
                var targetParams = new TargetingParameters
                {
                    canTargetSelf = false,
                    canTargetBuildings = false,
                    canTargetFires = false,
                    canTargetItems = false,
                    canTargetPawns = false,
                    canTargetLocations = true
                };
                Find.Targeter.BeginTargeting(targetParams, delegate(LocalTargetInfo t)
                {
                    Find.CurrentMap.GetThingBagTasks().AddTask(false, parent, Items.Where(i => !Find.CurrentMap
                            .GetThingBagTasks().Tasks(false)
                            .Any(task => task.bag.Thing == parent && task.items != null && task.items.Contains(i)))
                        .ToList(), t.Cell);
                });
            }
        };
    }
}
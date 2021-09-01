using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ThingBag
{
    [StaticConstructorOnStartup]
    public class ThingBagComp : ThingComp
    {
        public static readonly Texture2D Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename");

        public static readonly Texture2D Pack = ContentFinder<Texture2D>.Get("Crate/CratePack");

        public static readonly Texture2D Unpack = ContentFinder<Texture2D>.Get("Crate/CrateUnpack");

        public float ContentMass;

        public ThingFilter filter = new ThingFilter();

        public List<Thing> items = new List<Thing>();

        public string Label = "";

        private List<Thing> plan = new List<Thing>();

        public ThingBag_Properties Props => props as ThingBag_Properties;

        public float Fill => ContentMass / Props.MaxMass;

        public bool Full
        {
            get
            {
                if (ContentMass > Props.MaxMass)
                {
                    return true;
                }

                return false;
            }
        }

        public bool Empty => items.Count == 0;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            filter.CopyAllowancesFrom(Props.DefaultFilter);
        }

        public override string TransformLabel(string label)
        {
            if (string.IsNullOrEmpty(Label))
            {
                return label;
            }

            return Label;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref items, "items", LookMode.Deep);
            Scribe_Values.Look(ref Label, "label", "");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ContentMass = items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
            }
        }

        public void UnpackOne(IntVec3 pos, Map map, Thing item)
        {
            if (items.Count == 0 || !items.Contains(item))
            {
                return;
            }

            if (GenPlace.TryPlaceThing(item, pos, map, ThingPlaceMode.Direct))
            {
                items.Remove(item);
            }

            ContentMass = items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
        }

        public Thing UnpackOneRaw(Thing item)
        {
            if (items.Count == 0)
            {
                return null;
            }

            if (!items.Contains(item))
            {
                return null;
            }

            items.Remove(item);
            ContentMass = items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
            return item;
        }

        public bool CanPack(Thing thing)
        {
            if (!filter.Allows(thing))
            {
                return false;
            }

            var num = Props.MaxMass - ContentMass;
            if (num > thing.GetStatValue(StatDefOf.Mass))
            {
                return true;
            }

            return false;
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

            foreach (var item in items)
            {
                if (!item.TryAbsorbStack(thing, true))
                {
                    continue;
                }

                ContentMass = items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
                return !splitted;
            }

            items.Add(thing);
            if (thing.Spawned)
            {
                thing.DeSpawn();
            }

            ContentMass = items.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount);
            return !splitted;
        }

        private void StartPlanPack()
        {
            plan.Clear();
        }

        private bool PlanCanPack(Thing thing)
        {
            if (!filter.Allows(thing))
            {
                return false;
            }

            if (thing.GetStatValue(StatDefOf.Mass) + ContentMass +
                plan.Sum(i => i.GetStatValue(StatDefOf.Mass) * i.stackCount) > Props.MaxMass)
            {
                return false;
            }

            return true;
        }

        private bool PlanPackOne(Thing thing)
        {
            if (!PlanCanPack(thing))
            {
                return false;
            }

            plan.Add(thing);
            return true;
        }

        private void StartPlanUnpack(List<Thing> planitems = null)
        {
            plan = planitems != null ? planitems.Where(i => items.Contains(i)).ToList() : items.ToList();
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
            StartPlanPack();
            foreach (var item in GenRadial.RadialCellsAround(pos, Props.Radius, true))
            {
                var firstItem = item.GetFirstItem(parent.Map);
                if (firstItem != null)
                {
                    Log.Message(string.Concat("Can pack ", firstItem, " = ", PlanCanPack(firstItem).ToString()));
                }

                if (firstItem == null || !PlanCanPack(firstItem) ||
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
            Log.Message("Job: " + job);
            return job;
        }

        public Job Build_PackJobSpecific(Pawn pawn, List<Thing> items)
        {
            var job = new Job(DefDatabase<JobDef>.GetNamed("PackBag"), parent);
            StartPlanPack();
            foreach (var item in items)
            {
                if (item == null || !PlanCanPack(item) ||
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
            Log.Message("Job: " + job);
            return job;
        }

        public Job Build_PackSingleJob(IntVec3 pos, Pawn pawn, Thing item)
        {
            var unused = new Job(DefDatabase<JobDef>.GetNamed("PackBagSingle"), parent);
            StartPlanPack();
            if (item != null && PlanCanPack(item) &&
                pawn.CanReserveAndReach((LocalTargetInfo)item, PathEndMode.Touch, Danger.Deadly))
            {
                return new Job(DefDatabase<JobDef>.GetNamed("PackBagSingle"), parent, item);
            }

            return null;
        }

        public Job Build_UnpackJob(IntVec3 pos, Pawn pawn, List<Thing> items = null)
        {
            var job = new Job(DefDatabase<JobDef>.GetNamed("UnpackBag"));
            job.SetTarget(TargetIndex.C, parent);
            StartPlanUnpack(items);
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
            StartPlanUnpack(new List<Thing> { item });
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
            foreach (var item in items)
            {
                item.Destroy(mode);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "Rename".Translate(),
                icon = Rename,
                action = delegate { Find.WindowStack.Add(new Dialog_RenameCrate(this)); }
            };
            if (!Full)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Pack".Translate(),
                    icon = Pack,
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
                icon = Unpack,
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
                        Find.CurrentMap.GetThingBagTasks().AddTask(false, parent, items.Where(i => !Find.CurrentMap
                                .GetThingBagTasks().Tasks(false)
                                .Any(task => task.bag.Thing == parent && task.items != null && task.items.Contains(i)))
                            .ToList(), t.Cell);
                    });
                }
            };
        }
    }
}
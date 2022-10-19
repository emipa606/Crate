using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ThingBag;

internal class WorkGiver_Unpack : WorkGiver_Scanner
{
    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var thingBagComp = t.TryGetComp<ThingBagComp>();
        if (thingBagComp == null)
        {
            return null;
        }

        var task = pawn.Map.GetThingBagTasks().FirstTaskFor(t, false);
        if (task == null)
        {
            task = pawn.Map.GetThingBagTasks().FirstTaskFor(t, false, true);
        }

        if (task == null)
        {
            return null;
        }

        if (task.items == null || task.items.Count > 1)
        {
            var job = thingBagComp.Build_UnpackJob(task.pos.Cell, pawn, task.items);
            if (job == null)
            {
                pawn.Map.GetThingBagTasks().RemoveTask(task);
            }

            return job;
        }

        if (task.items.Count != 1)
        {
            return base.JobOnThing(pawn, t, forced);
        }

        var job2 = thingBagComp.Build_UnpackSingleJob(task.pos.Cell, pawn, task.items[0]);
        if (job2 == null)
        {
            pawn.Map.GetThingBagTasks().RemoveTask(task);
        }

        return job2;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn Pawn)
    {
        var list = Pawn.Map.GetThingBagTasks().tasks;
        foreach (var task in list)
        {
            if (!task.Pack &&
                Pawn.CanReserveAndReach((LocalTargetInfo)task.bag.Thing, PathEndMode.Touch, Danger.Deadly))
            {
                yield return task.bag.Thing;
            }
        }
    }
}
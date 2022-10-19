using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ThingBag;

internal class JobDriver_UnpackBagSingle : JobDriver
{
    private ThingBagComp bag => (TargetC.Thing as ThingWithComps)?.GetComp<ThingBagComp>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        pawn.jobs.curJob.count = 1;
        this.FailOnDestroyedOrNull(TargetIndex.C);
        if (!TargetC.Thing.IsForbidden(pawn))
        {
            this.FailOnForbidden(TargetIndex.C);
        }

        yield return Toils_Reserve.Reserve(TargetIndex.B);
        yield return Toils_Reserve.Reserve(TargetIndex.C);
        yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
        var unpackItem = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = 60
        };
        unpackItem.AddFinishAction(delegate
        {
            var thing = bag.UnpackOneRaw(TargetThingA);
            if (thing != null)
            {
                unpackItem.actor.carryTracker.TryStartCarry(thing);
            }

            var task = Map.GetThingBagTasks().FirstTaskFor(TargetC.Thing, false, true);
            if (task != null)
            {
                Map.GetThingBagTasks().RemoveTask(task);
            }
        });
        yield return unpackItem;
        yield return Toils_Reserve.Release(TargetIndex.C);
        yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
        yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, null, false);
    }
}
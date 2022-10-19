using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ThingBag;

internal class JobDriver_PackBag : JobDriver
{
    private ThingBagComp bag => (TargetThingA as ThingWithComps)?.GetComp<ThingBagComp>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        pawn.jobs.curJob.count = 1;
        this.FailOnDestroyedOrNull(TargetIndex.A);
        if (!TargetThingA.IsForbidden(pawn))
        {
            this.FailOnForbidden(TargetIndex.A);
        }

        yield return Toils_Reserve.ReserveQueue(TargetIndex.B);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        yield return Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.B);
        var extracttarget = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
        yield return extracttarget;
        var checkNextQueuedTarget = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch)
            .JumpIfDespawnedOrNullOrForbidden(TargetIndex.B, checkNextQueuedTarget);
        var packItem = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = 60
        };
        packItem.AddFinishAction(delegate { bag.PackOne(TargetThingB); });
        packItem.WithProgressBarToilDelay(TargetIndex.B);
        packItem.JumpIfDespawnedOrNullOrForbidden(TargetIndex.B, checkNextQueuedTarget);
        yield return packItem;
        yield return checkNextQueuedTarget;
        yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, extracttarget);
        var finishJob = new Toil();
        finishJob.AddFinishAction(delegate
        {
            var task = bag.parent.MapHeld.GetThingBagTasks().FirstTaskFor(bag.parent, true);
            if (task != null)
            {
                bag.parent.MapHeld.GetThingBagTasks().RemoveTask(task);
            }
        });
        yield return finishJob;
    }
}
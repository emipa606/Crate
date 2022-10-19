using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ThingBag;

internal class JobDriver_UnpackBag : JobDriver
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

        yield return Toils_Reserve.ReserveQueue(TargetIndex.B);
        yield return Toils_Reserve.Reserve(TargetIndex.C);
        yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
        yield return Toils_Haul.StartCarryThing(TargetIndex.C);
        var initExtractItem = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
        yield return initExtractItem;
        yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
        var JumpNext = Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, initExtractItem);
        yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch);
        yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
        var packItem = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            defaultDuration = 60
        };
        packItem.AddFinishAction(delegate { bag.UnpackOne(TargetB.Cell, Map, TargetThingA); });
        packItem.WithProgressBarToilDelay(TargetIndex.B);
        yield return packItem;
        yield return JumpNext;
        var finishJob = new Toil();
        finishJob.AddFinishAction(delegate
        {
            var task = bag.parent.MapHeld.GetThingBagTasks().FirstTaskFor(bag.parent, false);
            if (task != null)
            {
                bag.parent.MapHeld.GetThingBagTasks().RemoveTask(task);
            }
        });
        yield return finishJob;
    }
}
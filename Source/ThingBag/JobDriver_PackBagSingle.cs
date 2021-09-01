using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ThingBag
{
    internal class JobDriver_PackBagSingle : JobDriver
    {
        private ThingBagComp bag => (TargetThingA as ThingWithComps)?.GetComp<ThingBagComp>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            pawn.jobs.curJob.count = TargetThingB.stackCount;
            this.FailOnDestroyedOrNull(TargetIndex.A);
            if (!TargetThingA.IsForbidden(pawn))
            {
                this.FailOnForbidden(TargetIndex.A);
            }

            this.FailOnDestroyedOrNull(TargetIndex.B);
            if (!TargetThingA.IsForbidden(pawn))
            {
                this.FailOnForbidden(TargetIndex.B);
            }

            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            var packItem = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 60
            };
            packItem.AddFinishAction(delegate
            {
                if (bag.PackOne(TargetThingB))
                {
                    packItem.actor.carryTracker.innerContainer.Remove(TargetThingB);
                }
            });
            packItem.WithProgressBarToilDelay(TargetIndex.A);
            yield return packItem;
            var finishJob = new Toil();
            finishJob.AddFinishAction(delegate
            {
                var task = bag.parent.MapHeld.GetThingBagTasks().FirstTaskFor(bag.parent, true, true);
                if (task != null)
                {
                    bag.parent.MapHeld.GetThingBagTasks().RemoveTask(task);
                }
            });
            yield return finishJob;
        }
    }
}
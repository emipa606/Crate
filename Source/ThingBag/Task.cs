using System.Collections.Generic;
using Verse;

namespace ThingBag;

public class Task : IExposable
{
    public LocalTargetInfo bag;

    public List<Thing> items;
    public bool Pack;

    public LocalTargetInfo pos;

    public void ExposeData()
    {
        Scribe_Values.Look(ref Pack, "pack");
        Scribe_TargetInfo.Look(ref bag, "bag");
        Scribe_Collections.Look(ref items, "items", LookMode.Reference);
        Scribe_TargetInfo.Look(ref pos, "pos");
    }
}
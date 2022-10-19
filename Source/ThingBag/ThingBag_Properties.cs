using Verse;

namespace ThingBag;

public class ThingBag_Properties : CompProperties
{
    public ThingFilter DefaultFilter = new ThingFilter();

    public float MaxMass = 100f;

    public int Radius = 3;

    public ThingBag_Properties()
    {
        compClass = typeof(ThingBagComp);
    }

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        DefaultFilter?.ResolveReferences();
    }
}
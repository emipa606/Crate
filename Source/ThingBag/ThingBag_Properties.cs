using Verse;

namespace ThingBag;

public class ThingBag_Properties : CompProperties
{
    public readonly ThingFilter DefaultFilter = new();

    public readonly float MaxMass = 100f;

    public readonly int Radius = 3;

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
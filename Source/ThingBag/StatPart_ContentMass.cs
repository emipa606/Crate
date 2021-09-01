using RimWorld;
using Verse;

namespace ThingBag
{
    [StaticConstructorOnStartup]
    internal class StatPart_ContentMass : StatPart
    {
        static StatPart_ContentMass()
        {
            StatDefOf.Mass.parts.Add(new StatPart_ContentMass());
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (TryGetValue(req, out var value))
            {
                return "StatsReport_ThingBagContentsMass".Translate() + ": " + value.ToString("+0.##;-0.##;0") + "kg";
            }

            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (TryGetValue(req, out var value))
            {
                val += value;
            }
        }

        private bool TryGetValue(StatRequest req, out float value)
        {
            value = 0f;
            if (!req.HasThing || req.Thing is not ThingWithComps comps)
            {
                return false;
            }

            var comp = comps.GetComp<ThingBagComp>();
            if (comp == null)
            {
                return false;
            }

            value = comp.ContentMass;
            return true;
        }
    }
}
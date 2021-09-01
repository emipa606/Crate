using System.Linq;
using RimWorld;
using Verse;

namespace ThingBag
{
    [StaticConstructorOnStartup]
    internal class StatPart_ContentValue : StatPart
    {
        static StatPart_ContentValue()
        {
            StatDefOf.MarketValue.parts.Add(new StatPart_ContentValue());
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (TryGetValue(req, out var value))
            {
                return "StatsReport_ThingBagContentsValue".Translate() + ": " + value.ToString("+0.##;-0.##;0");
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

            value = comp.items.Sum(t => t.GetStatValue(StatDefOf.MarketValue) * t.stackCount);
            return true;
        }
    }
}
using RimWorld;
using UnityEngine;
using Verse;

namespace ThingBag;

public class ITab_Filter : ITab
{
    private readonly ThingFilterUI.UIState thingFilterState = new();

    public ITab_Filter()
    {
        labelKey = "TabFilter";
        size = new Vector2(300f, 480f);
    }

    protected override void FillTab()
    {
        var thingFilter = SelThing.TryGetComp<ThingBagComp>()?.Filter;
        var rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
        ThingFilterUI.DoThingFilterConfigWindow(rect, thingFilterState, thingFilter, null, 8);
    }
}
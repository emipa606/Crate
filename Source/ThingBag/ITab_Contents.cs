using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ThingBag;

internal class ITab_Contents : ITab
{
    private Vector2 scrollPosition;

    public ITab_Contents()
    {
        labelKey = "TabContents";
        size = new Vector2(300f, 480f);
    }

    protected override void FillTab()
    {
        var tbc = SelThing.TryGetComp<ThingBagComp>();
        if (tbc == null)
        {
            return;
        }

        var items = tbc.items;
        var rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
        Widgets.Label(rect.TopPartPixels(30f).LeftHalf().TopPartPixels(25f),
            $"{tbc.ContentMass:0.##;-0.##;0}kg / {tbc.Props.MaxMass:0.##;-0.##;0}kg");
        Widgets.FillableBar(rect.TopPartPixels(30f).LeftHalf().BottomPartPixels(5f), tbc.Fill);
        if (Widgets.ButtonText(rect.TopPartPixels(30f).RightHalf(), "Pack".Translate(), true, false))
        {
            var targetingParameters = new TargetingParameters
            {
                canTargetBuildings = false,
                canTargetPawns = false,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = t => t.HasThing && tbc.CanPack(t.Thing)
            };
            Find.Targeter.BeginTargeting(targetingParameters,
                delegate(LocalTargetInfo t)
                {
                    Find.CurrentMap.GetThingBagTasks()
                        .AddTask(true, SelThing, new List<Thing> { t.Thing }, IntVec3.Invalid);
                });
        }

        Widgets.DrawMenuSection(rect.BottomPartPixels(rect.height - 32f));
        var rect2 = new Rect(0f, 0f, rect.width - 16f, (items.Count * 24) + 8);
        Widgets.BeginScrollView(rect.BottomPartPixels(rect.height - 32f), ref scrollPosition, rect2);
        rect2 = rect2.ContractedBy(4f);
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect2);
        var num = Text.CalcSize("Unpack".Translate()).x + 16f + 4f;
        foreach (var item in items)
        {
            var rect3 = listing_Standard.GetRect(24f);
            var widgetRow = new WidgetRow(rect3.x, rect3.y, UIDirection.RightThenUp, rect3.width);
            GUI.color = item.DrawColor;
            Texture2D texture2D = null;
            if (!item.def.uiIconPath.NullOrEmpty())
            {
                texture2D = item.def.uiIcon;
            }
            else if (item is Pawn || item is Corpse)
            {
                if (item is not Pawn pawn)
                {
                    pawn = ((Corpse)item).InnerPawn;
                }

                if (!pawn.RaceProps.Humanlike)
                {
                    if (!pawn.Drawer.renderer.graphics.AllResolved)
                    {
                        pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    }

                    var matSingle = pawn.Drawer.renderer.graphics.nakedGraphic.MatSingle;
                    texture2D = matSingle.mainTexture as Texture2D;
                    GUI.color = matSingle.color;
                }
            }
            else
            {
                texture2D = item.Graphic.ExtractInnerGraphicFor(item).MatSingle.mainTexture as Texture2D;
            }

            widgetRow.Icon(texture2D ?? item.def.uiIcon);
            GUI.color = Color.white;
            widgetRow.Label(item.LabelCap, rect3.width - (widgetRow.FinalX + num + 4f));
            if (Find.CurrentMap.GetThingBagTasks().Tasks(false)
                    .Any(t => t.bag.Thing == SelThing && t.items != null && t.items.Contains(item)) ||
                !widgetRow.ButtonText("Unpack".Translate(), null, true, false))
            {
                continue;
            }

            var targetParams = new TargetingParameters
            {
                canTargetSelf = false,
                canTargetBuildings = false,
                canTargetFires = false,
                canTargetItems = false,
                canTargetPawns = false,
                canTargetLocations = true
            };
            var unused = SelThing;
            var dropitem = item;
            Find.Targeter.BeginTargeting(targetParams,
                delegate(LocalTargetInfo t)
                {
                    Find.CurrentMap.GetThingBagTasks()
                        .AddTask(false, SelThing, new List<Thing> { dropitem }, t.Cell);
                });
        }

        listing_Standard.End();
        Widgets.EndScrollView();
    }
}
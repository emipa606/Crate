using RimWorld;
using UnityEngine;
using Verse;

namespace ThingBag;

internal class Dialog_RenameCrate : Window
{
    private readonly ThingBagComp tbc;
    protected string curName;

    private bool focusedRenameField;
    private int startAcceptingInputAtFrame;

    public Dialog_RenameCrate(ThingBagComp t)
    {
        tbc = t;
        curName = tbc.Label;
        forcePause = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        closeOnAccept = false;
        closeOnClickedOutside = true;
    }

    private bool AcceptsInput => startAcceptingInputAtFrame <= Time.frameCount;
    protected int MaxNameLength => 28;
    public override Vector2 InitialSize => new Vector2(280f, 175f);

    protected AcceptanceReport NameIsValid(string name)
    {
        return true;
    }

    public void WasOpenedByHotkey()
    {
        startAcceptingInputAtFrame = Time.frameCount + 1;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var returnPressed = false;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            returnPressed = true;
            Event.current.Use();
        }

        GUI.SetNextControlName("RenameField");
        var text = Widgets.TextField(new Rect(0f, 15f, inRect.width, 35f), curName);
        if (AcceptsInput && text.Length < MaxNameLength)
        {
            curName = text;
        }
        else if (!AcceptsInput)
        {
            ((TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).SelectAll();
        }

        if (!focusedRenameField)
        {
            UI.FocusControl("RenameField", this);
            focusedRenameField = true;
        }

        if (!Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 15f, inRect.width - 15f - 15f, 35f), "OK") &&
            !returnPressed)
        {
            return;
        }

        var acceptanceReport = NameIsValid(curName);
        if (!acceptanceReport.Accepted)
        {
            if (acceptanceReport.Reason.NullOrEmpty())
            {
                Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
            return;
        }

        SetName(curName);
        Find.WindowStack.TryRemove(this);
    }

    protected void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = "";
        }

        tbc.Label = name;
    }
}
using Verse;

namespace ThingBag;

internal class Dialog_RenameCrate : Dialog_Rename
{
    private readonly ThingBagComp tbc;

    public Dialog_RenameCrate(ThingBagComp t)
    {
        tbc = t;
        curName = tbc.Label;
    }

    protected override AcceptanceReport NameIsValid(string name)
    {
        return true;
    }

    protected override void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = "";
        }

        tbc.Label = name;
    }
}
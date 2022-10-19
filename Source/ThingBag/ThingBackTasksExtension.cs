using Verse;

namespace ThingBag;

public static class ThingBackTasksExtension
{
    public static ThingBagTasks GetThingBagTasks(this Map map)
    {
        var thingBagTasks = map.GetComponent<ThingBagTasks>();
        if (thingBagTasks != null)
        {
            return thingBagTasks;
        }

        thingBagTasks = new ThingBagTasks(map);
        map.components.Add(thingBagTasks);

        return thingBagTasks;
    }
}
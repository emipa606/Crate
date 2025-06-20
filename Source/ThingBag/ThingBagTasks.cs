using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ThingBag;

public class ThingBagTasks(Map map) : MapComponent(map)
{
    public List<Task> MapTasks = [];

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref MapTasks, "tasks");
    }

    public IEnumerable<Task> Tasks(bool pack)
    {
        return MapTasks.Where(t => t.Pack == pack);
    }

    public Task FirstTaskFor(Thing t, bool pack, bool single = false)
    {
        return Tasks(pack).FirstOrDefault(task =>
            task.bag.Thing == t && task.items is { Count: 1 } == single);
    }

    public void RemoveTask(Task task)
    {
        MapTasks.Remove(task);
    }

    public void AddTask(bool pack, Thing t, List<Thing> items, IntVec3 pos)
    {
        MapTasks.Add(new Task
        {
            Pack = pack,
            bag = t,
            items = items,
            pos = pos
        });
    }
}
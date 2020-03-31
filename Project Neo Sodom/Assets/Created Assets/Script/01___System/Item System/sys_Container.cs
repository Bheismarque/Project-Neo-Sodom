using UnityEngine;
using System.Collections.Generic;

public class sys_Container : sys_Item
{
    [SerializeField] private int capacity = 0;
    [SerializeField] private List<sys_Item> items = new List<sys_Item>();

    private int occupied = 0;
    public bool addItem(sys_Item item)
    {
        if(occupied + item.getSize() > capacity) { return false; }
        occupied += item.getSize();
        items.Add(item);
        return true;
    }

    public List<sys_Item> getItems() { return items; }
    public sys_Item getItem(int index)
    {
        if (index > items.Count) { return null; }
        return items[index];
    }

    public override int getSize() { return occupied; }
}

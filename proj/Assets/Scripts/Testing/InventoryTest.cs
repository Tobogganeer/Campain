using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualVoid.Events;

/*
public class InventoryTest : MonoBehaviour
{
    void Start()
    {
        RegisterItemsEvent e = new RegisterItemsEvent();
        VVEventBus.Send(e);
        foreach (Item item in e.items)
            Debug.Log(item.name);
    }
}

#region Event
class RegisterItemsEvent : VVEvent
{
    public List<Item> items = new List<Item>();
}
#endregion

[VVEventSubscriber]
public class Init
{
    [VVEventHandler]
    internal static void RegisterItems(RegisterItemsEvent e)
    {
        e.items.Add(new Item { name = "Chapstick" });
        Debug.Log("Hello :)");
    }
}
*/
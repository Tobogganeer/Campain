using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualVoid.Events;

[VVEventSubscriber]
public class EventSystemTest : MonoBehaviour
{
    [VVEventHandler(typeof(MyCoolEvent))]
    internal static void TestEventHandler(VVEvent e)
    {
        Debug.Log("Got event! " + ((MyCoolEvent)e).message);
    }

    private void Start()
    {
        //EventBus.RegisterHandler(typeof(MyCoolEvent), EventSystemTest.TestEventHandler);
        VVEventBus.Send(new MyCoolEvent { message = "Hello :)" });
    }
}

class MyCoolEvent : VVEvent
{
    public string message;
}

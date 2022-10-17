using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class Registry<T> : VirtualVoid.Events.VVEvent where T : class, IRegistryEntry<T>
{
    public delegate T DefaultSupplier();

    private Dictionary<string, T> values;
    private DefaultSupplier defaultSupplier;

    public Registry(string RegistryName, DefaultSupplier defaultSupplier)
    {
        this.RegistryName = RegistryName;
        this.defaultSupplier = defaultSupplier;
        values = new Dictionary<string, T>();
        VirtualVoid.Events.VVEventBus.Send(this);
    }

    public readonly string RegistryName;

    public T Register(T value)
    {
        values[value.RegistryName] = value;
        return value;
    }

    public T Get(string key)
    {
        return values[key];
    }

    public bool Contains(string key)
    {
        return values.ContainsKey(key);
    }

    public T Default()
    {
        return defaultSupplier();
    }

    public static Registry<Item> Item = new Registry<Item>("item", () => ItemInit.Air);
}

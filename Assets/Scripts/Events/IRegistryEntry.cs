using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRegistryEntry<T> where T : class
{
    public string RegistryName { get; }
    public T SetRegistryName(string registryName);
}

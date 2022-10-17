using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegistryEntry<T> : IRegistryEntry<T> where T : class
{
    private string _registryName;
    public string RegistryName
    {
        get { return _registryName; }
        protected set { _registryName = value; }
    }
    public T SetRegistryName(string registryName)
    {
        RegistryName = registryName;
        return this as T;
    }
}

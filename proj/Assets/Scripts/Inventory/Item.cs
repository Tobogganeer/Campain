using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : RegistryEntry<Item>
{
    //group
    //rarity
    //stack
    //id

    public Item()
    {
        
    }

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }

    public virtual void OnUpdate(float dt) { }

    public virtual void OnPrimaryStart() { }
    public virtual void OnPrimaryUse() { }
    public virtual void OnPrimaryStop() { }

    public virtual void OnSecondaryStart() { }
    public virtual void OnSecondaryUse() { }
    public virtual void OnSecondaryStop() { }

    public virtual void OnReload() { }
    public virtual void OnInteract() { }
}
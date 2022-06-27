using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public static ItemStack EmptyItem = new ItemStack(ItemInit.Air, 0);

    public readonly Item item;
    public int count;

    public ItemStack(Item item, int amount)
    {
        this.item = item;
        this.count = 0;
    }


    public bool empty => IsEmpty();
    public bool IsEmpty()
    {
        if (this == EmptyItem)
            return true;
        else if (item != null && item != ItemInit.Air)
            return count <= 0;
        return true;
    }
}

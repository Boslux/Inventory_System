using System;

public class ItemRules
{
    public Func<string, bool> IsStackable;
    public Func<string, int> MaxStack;

    public ItemRules(Func<string, bool> isStackable, Func<string, int> maxStack)
    {
        IsStackable = isStackable;
        MaxStack = maxStack;
    }
}
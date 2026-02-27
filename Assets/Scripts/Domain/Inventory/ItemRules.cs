using System;

public class ItemRules
{
    public Func<string, bool> IsStackable;
    public Func<string, int> MaxStack;
    public Func<string, float> Weight;

    public ItemRules
        (
            Func<string, bool> isStackable,
            Func<string, int> maxStack,
            Func<string, float> weight
        )
    {
        IsStackable = isStackable;
        MaxStack = maxStack;
        Weight = weight;
    }
}
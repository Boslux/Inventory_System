using JetBrains.Annotations;

public class Slot
{
    public ItemStack Stack;
    public bool IsEmpty => Stack == null;
    public void Clear()
    {
        Stack=null;
    }
}
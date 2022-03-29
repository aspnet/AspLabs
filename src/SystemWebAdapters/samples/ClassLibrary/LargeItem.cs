namespace ClassLibrary;

public class LargeItem
{
    private static long _count;

    private LargeItem(long count)
    {
        Count = count;
    }

    public static LargeItem GetNext() => new(_count++);

    public long Count { get; }
}

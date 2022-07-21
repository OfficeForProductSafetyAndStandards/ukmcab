namespace UKMCAB.Common;

public static class Indeed
{
    public static void If(bool condition, Action action)
    {
        if (condition)
        {
            action();
        }
    }

    public static async Task IfAsync(bool condition, Func<Task> action)
    {
        if (condition)
        {
            await action();
        }
    }
}

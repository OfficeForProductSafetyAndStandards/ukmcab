namespace UKMCAB.Common;

public static class Efficiency
{
    /// <summary>
    /// Should really be named 'IdeallyDoOnce" - it doesn't do locking.  It will attempt to perform an operation once, but there's no locking - so no guarantees
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="name"></param>
    /// <param name="task"></param>
    /// <returns></returns>
    public static async Task DoOnceAsync(string scope, string name, Func<Task> task)
    {
        if (!Memory.Get<bool>(scope, name))
        {
            await task();
            Memory.Set(scope, name, true);
        }
    }

    /// <summary>
    /// Should really be named 'IdeallyDoOnce" - it doesn't do locking.  It will attempt to perform an operation once, but there's no locking - so no guarantees
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="innerScope"></param>
    /// <param name="name"></param>
    /// <param name="task"></param>
    /// <returns></returns>
    public static async Task DoOnceAsync(string? scope, string innerScope, string name, Func<Task> task)
        => await DoOnceAsync(StringExt.Keyify(scope, innerScope), name, task);

    /// <summary>
    /// Should really be named 'IdeallyDoOnce" - it doesn't do locking.  It will attempt to perform an operation once, but there's no locking - so no guarantees
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="name"></param>
    /// <param name="task"></param>
    public static void DoOnce(string scope, string name, Action task)
    {
        if (!Memory.Get<bool>(scope, name))
        {
            task();
            Memory.Set(scope, name, true);
        }
    }
}

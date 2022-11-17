namespace UKMCAB.Common;

public static class Pred
{
    public static void When(bool condition, Action action)
    {
        if (condition)
        {
            action();
        }
    }

    public static void When<T>(T model, Func<T, bool> predicate, Action<T> action)
    {
        if (predicate(model))
        {
            action(model);
        }
    }

    public static void WhenNotNull<T>(T model, Action<T> action)
    {
        if (model != null)
        {
            action(model);
        }
    }

    public static void When<T>(bool condition, T model, Action<T> action)
    {
        if (condition)
        {
            action(model);
        }
    }

    /// <summary>
    /// Cleaner version of the tenary operator; but supplies null when condition is false.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="condition"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T? When<T>(bool condition, T value) => condition ? value : default;

    public static T? WhenNot<T>(bool condition, T value) where T : class => When(!condition, value);

    public static T AdaptIf<T>(bool condition, T model, Func<T, T> action)
    {
        if (condition)
        {
            return action(model);
        }
        else
        {
            return model;
        }
    }
}
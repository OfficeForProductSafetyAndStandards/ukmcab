using UKMCAB.Common.Exceptions;

namespace UKMCAB.Common;

public static class Rule
{
    public static void IsTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new DomainException(message);
        }
    }

    public static void IsFalse(bool condition, string message)
    {
        if (condition)
        {
            throw new DomainException(message);
        }
    }
}
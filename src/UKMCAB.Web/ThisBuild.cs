namespace UKMCAB.Web;

public static class ThisBuild
{
    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}

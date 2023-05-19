using System;

namespace WhatsUpToday.Core.Extensions;

public static class StringConversionExtensions
{
    public static bool ToBoolean(this string str)
    {
        bool result = false;
        if (str != null)
            _ = bool.TryParse(str, out result);
        return result;
    }

    public static int ToInteger(this string str)
    {
        int result = 0;
        if (str != null)
            _ = int.TryParse(str, out result);
        return result;
    }

    public static long ToLongInt(this string str)
    {
        long result = 0;
        if (str != null)
            _ = long.TryParse(str, out result);
        return result;
    }
}

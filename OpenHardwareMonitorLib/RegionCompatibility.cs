using System;

namespace OpenHardwareMonitor;

/// <summary>
/// Wrapper around <c>OSHelper.IsCompatible</c> that keeps OS/architecture checks
/// but ignores the geographic restriction from SergiyE.Common.
/// </summary>
public static class RegionCompatibility
{
    public static bool IsCompatible(bool checkRedist, out string errorMessage, out Action fixAction)
    {
        try
        {
            if (OSHelper.IsCompatible(checkRedist, out errorMessage, out fixAction))
                return true;
        }
        catch (Exception ex) when (IsRegionRestriction(ex.Message))
        {
            errorMessage = null;
            fixAction = null;
            return true;
        }

        if (IsRegionRestriction(errorMessage))
        {
            errorMessage = null;
            fixAction = null;
            return true;
        }

        return false;
    }

    public static bool IsRegionRestriction(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        return Contains(message, "region")
            || Contains(message, "russia")
            || Contains(message, "your country")
            || Contains(message, "country lock");
    }

    private static bool Contains(string message, string value)
    {
        return message.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

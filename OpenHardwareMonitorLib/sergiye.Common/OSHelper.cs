using System;
using System.Globalization;
using System.Security.Principal;

namespace sergiye.Common;

public static class OSHelper
{
    public static bool Is64Bit { get; }
    public static bool IsUnix { get; }
    public static bool IsWindows7OrLower { get; }
    public static bool IsWindows8OrGreater { get; }
    public static bool IsWindows11OrGreater { get; }
    public static bool IsMetricSystemUsed => RegionInfo.CurrentRegion.IsMetric;

    static OSHelper()
    {
        PlatformID platform = Environment.OSVersion.Platform;
        IsUnix = platform == PlatformID.Unix || platform == PlatformID.MacOSX;
        Is64Bit = Environment.Is64BitOperatingSystem;
        Version version = Environment.OSVersion.Version;
        IsWindows7OrLower = !IsUnix && version.Major + version.Minor / 10.0 <= 6.1;
        IsWindows8OrGreater = !IsUnix && ((version.Major == 6 && version.Minor >= 2) || version.Major > 6);
        IsWindows11OrGreater = !IsUnix && version.Major >= 10 && version.Build >= 22000;
    }

    public static bool IsCompatible(bool checkRedist, out string errorMessage, out Action fixAction, bool checkArchitecture = true)
    {
        errorMessage = null;
        fixAction = null;
        if (checkArchitecture && Environment.Is64BitOperatingSystem != Environment.Is64BitProcess)
        {
            errorMessage = "You are running an application build made for a different OS architecture.\nIt is not compatible!";
            return false;
        }

        return true;
    }

    public static bool IsAdministrator()
    {
        if (IsUnix)
            return true;

        try
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}

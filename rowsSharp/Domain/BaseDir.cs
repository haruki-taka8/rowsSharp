using System;

namespace rowsSharp.Domain;

internal static class BaseDir
{
    private const string Metavariable = "$baseDir";
    private static readonly string BasePath = Environment.CurrentDirectory + "/Userdata/";
    private static readonly string BasePathEscaped = BasePath.Replace("\\", "\\\\");

    internal static string Expand(string value)
    {
        return value.Replace(Metavariable, BasePath);
    }

    internal static string ExpandEscaped(string value)
    {
        return value.Replace(Metavariable, BasePathEscaped);
    }
}

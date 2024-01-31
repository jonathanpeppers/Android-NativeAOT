using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Android;

/// <summary>
/// Android log priority values, in increasing order of priority.
/// from Android's log.h
/// </summary>
enum LogPriority
{
    /// <summary>
    /// For internal use only.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The default priority, for internal use only.
    /// </summary>
    Default, /* only for SetMinPriority() */
    /// <summary>
    /// Verbose logging. Should typically be disabled for a release apk.
    /// </summary>
    Verbose,
    /// <summary>
    /// Debug logging. Should typically be disabled for a release apk.
    /// </summary>
    Debug,
    /// <summary>
    /// Informational logging. Should typically be disabled for a release apk.
    /// </summary>
    Info,
    /// <summary>
    /// Warning logging. For use with recoverable failures.
    /// </summary>
    Warn,
    /// <summary>
    /// Error logging. For use with unrecoverable failures.
    /// </summary>
    Error,
    /// <summary>
    /// Fatal logging. For use when aborting.
    /// </summary>
    Fatal,
    /// <summary>
    /// For internal use only.
    /// </summary>
    Silent, /* only for SetMinPriority(); must be last */
}

static class Log
{
    public const string Tag = "DOTNET";

    /// <summary>
    /// Writes a formatted string to the log, with priority prio and tag tag.
    /// The details of formatting are the same as for printf(3): http://man7.org/linux/man-pages/man3/printf.3.html
    /// https://developer.android.com/ndk/reference/group/logging#__android_log_print
    /// </summary>
    [DllImport("log", EntryPoint = "__android_log_print", CallingConvention = CallingConvention.Cdecl)]
    static extern int LogPrint(LogPriority priority, string tag, string message);

    [Conditional("TRACE")]
    public static void Info(string message) =>
        LogPrint(LogPriority.Info, Tag, message);

    [Conditional("TRACE")]
    public static void IsNull(string message, object obj) =>
        Info($"{message}: {obj?.ToString() ?? "NULL!"}");

    public static void Error(string message) =>
        LogPrint(LogPriority.Info, Tag, message);

    public static void Exception(string message, Exception exc) =>
        Error($"{message}: {exc}");
}
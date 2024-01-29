using System.Reflection.Metadata.Ecma335;
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

static class NativeMethods
{
    [DllImport("__Internal", EntryPoint = "__android_log_print", CallingConvention = CallingConvention.Cdecl)]
    public static extern int LogPrint(LogPriority priority, string tag, string format);
}
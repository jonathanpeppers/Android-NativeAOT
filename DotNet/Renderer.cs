﻿using Android;
using System.Runtime.InteropServices;
using static Android.NativeMethods;

namespace hellonativeaot;

public class Renderer
{
    [UnmanagedCallersOnly(EntryPoint = "Render")]
    public static void Render()
    {
        // NOTE: doesn't work as stdio doesn't appear in Android logcat output
        // Console.WriteLine($"ManagedAdd({x}, {y})");
        
        // p/invoke to call __android_log_print
        LogPrint(LogPriority.Info, "Managed", "Render()");

        return x + y;
    }
}

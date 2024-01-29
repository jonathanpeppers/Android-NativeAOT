﻿using Android;
using System.Runtime.InteropServices;
using static Android.NativeMethods;

namespace hellonativeaot;

public class Class1
{
    [UnmanagedCallersOnly(EntryPoint = "ManagedAdd")]
    public static int ManagedAdd(int x, int y)
    {
        // NOTE: doesn't work as stdio doesn't appear in Android logcat output
        // Console.WriteLine($"ManagedAdd({x}, {y})");

        LogPrint(LogPriority.Info, "Managed", $"ManagedAdd({x}, {y})");

        return x + y;
    }
}

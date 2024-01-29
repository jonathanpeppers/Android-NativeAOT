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
        try
        {
            LogPrint(LogPriority.Info, "Managed", $"ManagedAdd({x}, {y})");
        }
        catch (Exception exc)
        {
            File.AppendAllText ("/data/user/0/com.jonathanpeppers.nativeaot/files/log.txt", exc.ToString() + "\n");
        }
        File.AppendAllText ("/data/user/0/com.jonathanpeppers.nativeaot/files/log.txt", $"ManagedAdd({x}, {y})\n");

        return x + y;
    }
}

﻿using System.Runtime.InteropServices;

namespace hellonativeaot;

public class Class1
{
    [UnmanagedCallersOnly(EntryPoint = "ManagedAdd")]
    public static int ManagedAdd(int x, int y)
    {
        Console.WriteLine($"ManagedAdd({x}, {y})");
        return x + y;
    }
}

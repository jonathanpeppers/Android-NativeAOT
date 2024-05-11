using System.Runtime.InteropServices;

class Class1
{
    [UnmanagedCallersOnly(EntryPoint = "ManagedAdd")]
    public static int ManagedAdd(int x, int y) => x + y;
}
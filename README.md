# Android-NativeAOT

A .NET 8, NativeAOT example on Android.

I had this managed code:

```csharp
[UnmanagedCallersOnly(EntryPoint = "ManagedAdd")]
public static int ManagedAdd(int x, int y)
{
    Console.WriteLine($"ManagedAdd({x}, {y})");
    return x + y;
}
```

I created a C++ Android project using NativeActivity, and I called the managed
code from C++:

```c++
// in dotnet.h
extern "C" int ManagedAdd(int x, int y);

// in native-lib.cpp
int result = ManagedAdd(1, 2);
__android_log_print (ANDROID_LOG_INFO, "Native", "ManagedAdd(1, 2) returned: %i", result);
```

Results in the message:

```log
01-29 10:41:04.363 11016 11042 I Native  : ManagedAdd(1, 2) returned: 3
```

See [DotNet/README.md](DotNet/README.md) on how to build `libdotnet.so`.

## Notes

`Console.WriteLine()` doesn't work because it basically just writes to Unix stdout. stdout does not appear in `adb logcat` output, as you have to call `__android_log_print` instead.

This was an interesting example, to start a thread that processes stdout and calls the appropriate Android API:

* https://codelab.wordpress.com/2014/11/03/how-to-use-standard-output-streams-for-logging-in-android-apps/

Instead, we can p/invoke into:

```csharp
[DllImport("log", EntryPoint = "__android_log_print", CallingConvention = CallingConvention.Cdecl)]
public static extern int LogPrint(LogPriority priority, string tag, string format);
```

## TODO

* `Console.WriteLine()` didn't work inside managed code. Why?

* Try something more complicated. SkiaSharp?

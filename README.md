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

# Android-NativeAOT

A .NET 8, NativeAOT example on Android.

## Example using SkiaSharp

This sample has a C++ Android Studio project:

* Uses [Native Activity](https://developer.android.com/ndk/samples/sample_na)
* No Java/Kotlin code
* Configures OpenGL
* Calls into C# / managed code
* Managed code uses SkiaSharp for rendering a random Skia shader
* Tap input randomly changes the shader

Some screenshots of the Skia content:

<img width="256" src="docs/screenshot1.gif" />
<img width="256" src="docs/screenshot2.gif" />
<img width="256" src="docs/screenshot3.gif" />

(Note these look completely smooth on a Pixel 5, I just tried to snap quick gifs with Vysor)

The C# side is a:

* .NET 8 class library
* Built with RID `linux-bionic-arm64`
* Uses the SkiaSharp NuGet package, as one would.
  * Used a nightly build of SkiaSharp, as I wanted a feature from Skia 3.0

## App Size

The release `.apk` file of the SkiaSharp sample is ~4.3 MB

A breakdown of the files inside:

```
> 7z l app-release.apk
   Date      Time    Attr         Size   Compressed  Name
------------------- ----- ------------ ------------  ------------------------
1981-01-01 01:01:02 .....           56           52  META-INF\com\android\build\gradle\app-metadata.properties
1981-01-01 01:01:02 .....         1524          753  classes.dex
1981-01-01 01:01:02 .....      8525024      3733033  lib\arm64-v8a\libSkiaSharp.so
1981-01-01 01:01:02 .....      1326720       501321  lib\arm64-v8a\libdotnet.so
1981-01-01 01:01:02 .....        19504         6869  lib\arm64-v8a\libnativeaot.so
1981-01-01 01:01:02 .....         2376          867  AndroidManifest.xml
1981-01-01 01:01:02 .....         7778         7778  res\-6.webp
1981-01-01 01:01:02 .....          548          239  res\0K.xml
1981-01-01 01:01:02 .....         5696          987  res\0w.xml
1981-01-01 01:01:02 .....          788          347  res\9s.xml
1981-01-01 01:01:02 .....          548          239  res\BW.xml
1981-01-01 01:01:02 .....         1404         1404  res\MO.webp
1981-01-01 01:01:02 .....         1572          703  res\PF.xml
1981-01-01 01:01:02 .....         2884         2884  res\Sn.webp
1981-01-01 01:01:02 .....          982          982  res\d2.webp
1981-01-01 01:01:02 .....         2898         2898  res\fq.webp
1981-01-01 01:01:02 .....         5914         5914  res\j_.webp
1981-01-01 01:01:02 .....         1900         1900  res\qs.webp
1981-01-01 01:01:02 .....         3844         3844  res\sK.webp
1981-01-01 01:01:02 .....         3918         3918  res\u5.webp
1981-01-01 01:01:02 .....         1772         1772  res\yw.webp
1981-01-01 01:01:02 .....         2036         2036  resources.arsc
1981-01-01 01:01:02 .....         2085         1126  META-INF\CERT.SF
1981-01-01 01:01:02 .....         1167         1020  META-INF\CERT.RSA
1981-01-01 01:01:02 .....         2011         1048  META-INF\MANIFEST.MF
------------------- ----- ------------ ------------  ------------------------
1981-01-01 01:01:02            9924949      4283934  25 files
```

`libdotnet.so` is ~1.3MB, and `libSkiaSharp.so` is ~8.5MB!

If we reduce this to a "Hello World" example:

* `hello.apk` is ~561 KB!
* `libdotnet.so` (uncompressed) is ~1.1 MB

## "Hello World" Example

See the [HelloWorld](https://github.com/jonathanpeppers/Android-NativeAOT/tree/HelloWorld) branch.

I had this managed code:

```csharp
[UnmanagedCallersOnly(EntryPoint = "ManagedAdd")]
public static int ManagedAdd(int x, int y) => x + y;
```

I created a C++ Android project using NativeActivity, and I called the managed
code from C++:

```c++
// in dotnet.h
extern "C" int ManagedAdd(int x, int y);

// in native-lib.cpp
int result = ManagedAdd(1, 2);
__android_log_print (ANDROID_LOG_INFO, TAG, "ManagedAdd(1, 2) returned: %i", result);
```

Results in the message:

```log
01-31 11:42:44.545 28239 28259 I NATIVE  : Entering android_main
01-31 11:42:44.550 28239 28259 I NATIVE  : ManagedAdd(1, 2) returned: 3
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

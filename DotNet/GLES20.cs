using System.Runtime.InteropServices;

namespace Android;

/// <summary>
/// https://github.com/mono/SkiaSharp/blob/9274aeec807fd17eec2a3266ad4c2475c37d8a0c/source/SkiaSharp.Views/SkiaSharp.Views.Shared/GlesInterop/Gles.cs
/// </summary>
public static class GLES20
{
    const string libGLESv2 = "libGLESv2.so";

    public const int FRAMEBUFFER_BINDING = 0x8CA6;
    public const int RENDERBUFFER_BINDING = 0x8CA7;

    // ClearBufferMask
    public const int DEPTH_BUFFER_BIT = 0x00000100;
    public const int STENCIL_BUFFER_BIT = 0x00000400;
    public const int COLOR_BUFFER_BIT = 0x00004000;

    // GetPName
    public const int SUBPIXEL_BITS = 0x0D50;
    public const int RED_BITS = 0x0D52;
    public const int GREEN_BITS = 0x0D53;
    public const int BLUE_BITS = 0x0D54;
    public const int ALPHA_BITS = 0x0D55;
    public const int DEPTH_BITS = 0x0D56;
    public const int STENCIL_BITS = 0x0D57;
    public const int SAMPLES = 0x80A9;

    [DllImport(libGLESv2)]
    public static extern void glClear(uint mask);

    [DllImport(libGLESv2)]
    public static extern void glClearColor(float red, float green, float blue, float alpha);

    [DllImport(libGLESv2)]
    public static extern void glGetIntegerv(uint pname, out int data);

    [DllImport(libGLESv2)]
    public static extern void glViewport(int x, int y, int width, int height);
}
﻿using Android;
using System.Runtime.InteropServices;
using SkiaSharp;
using static Android.NativeMethods;

namespace hellonativeaot;

/// <summary>
/// https://github.com/mono/SkiaSharp/blob/9274aeec807fd17eec2a3266ad4c2475c37d8a0c/source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceViewRenderer.cs#L32
/// </summary>
public class Renderer
{
    const SKColorType colorType = SKColorType.Rgba8888;
    const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

    static GRContext? context;
    static GRGlFramebufferInfo glInfo;
    static GRBackendRenderTarget? renderTarget;
    static SKSurface? surface;
    static SKCanvas? canvas;
    static SKSizeI lastSize;
    static SKSizeI newSize;

    [UnmanagedCallersOnly(EntryPoint = "Render")]
    public static void Render()
    {
        LogPrint(LogPriority.Info, "Managed", "Render()");

        GLES20.glClear(GLES20.COLOR_BUFFER_BIT | GLES20.DEPTH_BUFFER_BIT | GLES20.STENCIL_BUFFER_BIT);

        // create the contexts if not done already
        if (context == null)
        {
            var glInterface = GRGlInterface.Create();
            context = GRContext.CreateGl(glInterface);
        }

        // manage the drawing surface
        if (renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
        {
            // create or update the dimensions
            lastSize = newSize;

            // read the info from the buffer
            GLES20.glGetIntegerv(GLES20.FRAMEBUFFER_BINDING, out int frame);
            GLES20.glGetIntegerv(GLES20.STENCIL_BITS, out int stencil);
            GLES20.glGetIntegerv(GLES20.SAMPLES, out int samples);
            var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
            if (samples > maxSamples)
                samples = maxSamples;
            glInfo = new GRGlFramebufferInfo((uint)frame, colorType.ToGlSizedFormat());

            // destroy the old surface
            surface?.Dispose();
            surface = null;
            canvas = null;

            // re-create the render target
            renderTarget?.Dispose();
            renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, samples, stencil, glInfo);
        }

        // create the surface
        if (surface == null)
        {
            surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
            canvas = surface.Canvas;
        }

        using (new SKAutoCanvasRestore(canvas, true))
        {
            // start drawing
        }

        // flush the SkiaSharp contents to GL
        canvas?.Flush();
        context.Flush();
    }

    [UnmanagedCallersOnly(EntryPoint = "Resize")]
    public static void Resize(int width, int height)
    {
        LogPrint(LogPriority.Info, "Managed", $"Resize({width}, {height})");

        GLES20.glViewport(0, 0, width, height);

        // get the new surface size
        newSize = new SKSizeI(width, height);
    }
}

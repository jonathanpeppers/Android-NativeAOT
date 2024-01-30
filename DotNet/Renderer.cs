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

    static void LogObject(string message, object obj)
    {
        LogPrint(LogPriority.Info, "Managed", $"{message}: {obj?.ToString() ?? "NULL!"}");
    }

    [UnmanagedCallersOnly(EntryPoint = "Render")]
    public static void Render()
    {
        LogPrint(LogPriority.Info, "Managed", "Render()");
        try
        {
            GLES20.glClear(GLES20.COLOR_BUFFER_BIT | GLES20.DEPTH_BUFFER_BIT | GLES20.STENCIL_BUFFER_BIT);
            GLES20.glClearColor(0, 1, 0, 1);

            // create the contexts if not done already
            if (context == null)
            {
                var glInterface = GRGlInterface.Create();
                LogObject("Created GRGlInterface", glInterface);
                context = GRContext.CreateGl(glInterface);
                LogObject("Created GRContext", context);
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
                LogObject("Created GRBackendRenderTarget", renderTarget);
            }

            // create the surface
            if (surface == null)
            {
                LogObject("Creating SKSurface, context", context);
                LogObject("Creating SKSurface, renderTarget", renderTarget);
                surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
                LogObject("Created SKSurface", surface);
                canvas = surface.Canvas;
                LogObject("surface.Canvas", canvas);
            }

            ArgumentNullException.ThrowIfNull(canvas);
            using (new SKAutoCanvasRestore(canvas, true))
            {
                // start drawing
                canvas.Clear(SKColors.Pink);

                var paint = new SKPaint();
                paint.Color = SKColors.Blue;
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 10;

                canvas.DrawLine(20, 20, 100, 100, paint);
            }

            // flush the SkiaSharp contents to GL
            canvas.Flush();
            context.Flush();
        }
        catch (Exception exc)
        {
            LogPrint(LogPriority.Error, "Managed", $"Exception in Render(): {exc}");
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "Resize")]
    public static void Resize(int width, int height)
    {
        LogPrint(LogPriority.Info, "Managed", $"Resize({width}, {height})");
        try
        {
            GLES20.glViewport(0, 0, width, height);

            // get the new surface size
            newSize = new SKSizeI(width, height);
        }
        catch (Exception exc)
        {
            LogPrint(LogPriority.Error, "Managed", $"Exception in Resize(): {exc}");
        }
    }
}

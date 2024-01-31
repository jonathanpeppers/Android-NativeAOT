﻿using Android;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace libdotnet;

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

    static readonly SKFrameCounter frameCounter = new();
    static readonly SKPaint currentPaint = new();
    static SKRuntimeShaderBuilder? currentEffectBuilder;
    static SKShader? currentShader;
    static float X, Y;

    [UnmanagedCallersOnly(EntryPoint = "Render")]
    public static void Render()
    {
        Log.Info("Render()");
        try
        {
            GLES20.glClear(GLES20.COLOR_BUFFER_BIT | GLES20.DEPTH_BUFFER_BIT | GLES20.STENCIL_BUFFER_BIT);

            // create the contexts if not done already
            if (context == null)
            {
                var glInterface = GRGlInterface.Create();
                Log.IsNull("Created GRGlInterface", glInterface);
                context = GRContext.CreateGl(glInterface);
                Log.IsNull("Created GRContext", context);
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
                Log.IsNull("Created GRBackendRenderTarget", renderTarget);
            }

            // create the surface
            if (surface == null)
            {
                Log.IsNull("Creating SKSurface, context", context);
                Log.IsNull("Creating SKSurface, renderTarget", renderTarget);
                surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
                Log.IsNull("Created SKSurface", surface);
                canvas = surface.Canvas;
                Log.IsNull("surface.Canvas", canvas);
            }

            ArgumentNullException.ThrowIfNull(canvas);
            using (new SKAutoCanvasRestore(canvas, true))
            {
                frameCounter.NextFrame();

                currentEffectBuilder ??= Shaders.GetRandomShader();
                currentEffectBuilder.Uniforms["iResolution"] = new SKPoint3(lastSize.Width, lastSize.Height, 0);
                currentEffectBuilder.Uniforms["iTime"] = (float)frameCounter.TotalDuration.TotalSeconds;
                currentEffectBuilder.Uniforms["iMouse"] = new SKPoint(X, Y);
                currentShader = currentEffectBuilder.Build();
                currentPaint.Shader = currentShader;

                canvas.DrawRect(0, 0, lastSize.Width, lastSize.Height, currentPaint);
            }

            // flush the SkiaSharp contents to GL
            canvas.Flush();
            context.Flush();
        }
        catch (Exception exc)
        {
            Log.Exception("Exception in Render()", exc);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "Resize")]
    public static void Resize(int width, int height)
    {
        Log.Info($"Resize({width}, {height})");
        try
        {
            GLES20.glViewport(0, 0, width, height);

            // get the new surface size
            newSize = new SKSizeI(width, height);
        }
        catch (Exception exc)
        {
            Log.Exception("Exception in Resize()", exc);
        }
    }

    /// <summary>
    /// Called from C++ on input
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "OnTap")]
    public static void OnTap(float x, float y)
    {
        Log.Info($"OnTap({x}, {y})");
        try
        {
            X = x;
            Y = y;

            // Should trigger a new random shader
            currentEffectBuilder = null;
        }
        catch (Exception exc)
        {
            Log.Exception("Exception in OnTap()", exc);
        }
    }
}

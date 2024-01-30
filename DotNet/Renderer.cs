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

    static readonly SKFrameCounter frameCounter = new();
    static readonly SKPaint currentPaint = new();
    static SKRuntimeShaderBuilder? currentEffectBuilder;
    static SKShader? currentShader;

    /// <summary>
    /// https://shaders.skia.org/?id=de2a4d7d893a7251eb33129ddf9d76ea517901cec960db116a1bbd7832757c1f
    /// Source: @notargs https://twitter.com/notargs/status/1250468645030858753
    /// </summary>
    const string shaderSource =
        """
        float f(vec3 p) {
            p.z -= iTime * 10.;
            float a = p.z * .1;
            p.xy *= mat2(cos(a), sin(a), -sin(a), cos(a));
            return .1 - length(cos(p.xy) + sin(p.yz));
        }

        half4 main(vec2 fragcoord) { 
            vec3 d = .5 - fragcoord.xy1 / iResolution.y;
            vec3 p=vec3(0);
            for (int i = 0; i < 32; i++) {
            p += f(p) * d;
            }
            return ((sin(p) + vec3(2, 5, 12)) / length(p)).xyz1;
        }
        """;

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
                var duration = frameCounter.NextFrame();

                currentEffectBuilder ??= SKRuntimeEffect.BuildShader(shaderSource);
                currentEffectBuilder.Uniforms["iResolution"] = new SKPoint3(lastSize.Width, lastSize.Height, 0);
                currentEffectBuilder.Uniforms["iTime"] = (float)duration.TotalSeconds;
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

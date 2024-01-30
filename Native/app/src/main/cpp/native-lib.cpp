#include <EGL/egl.h>
#include <GLES/gl.h>
#include <cassert>
#include <memory>
#include <android_native_app_glue.h>
#include <jni.h>
#include <android/log.h>
#include "dotnet.h"

/**
 * Shared state for our app.
 */
struct engine {
    struct android_app* app;

    int animating = 0;
    EGLDisplay display;
    EGLSurface surface;
    EGLContext context;
    int32_t width;
    int32_t height;
};

extern "C" {

    void init_display(struct engine* engine)
    {
        const EGLint configAttribs[] = {
                EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
                EGL_SURFACE_TYPE,    EGL_WINDOW_BIT,
                EGL_BLUE_SIZE,       8,
                EGL_GREEN_SIZE,      8,
                EGL_RED_SIZE,        8,
                EGL_ALPHA_SIZE,      8,
                EGL_STENCIL_SIZE,    8,
                EGL_NONE
        };

        const EGLint contextAttribs[] = {
                EGL_CONTEXT_CLIENT_VERSION, 2,
                EGL_NONE
        };

        const EGLint surfaceAttribs[] = {
                EGL_RENDER_BUFFER, EGL_BACK_BUFFER,
                EGL_NONE
        };

        EGLint w, h, format;
        EGLint numConfigs;
        EGLConfig config;
        EGLSurface surface;
        EGLContext context;

        EGLDisplay display = eglGetDisplay(EGL_DEFAULT_DISPLAY);

        eglInitialize(display, NULL, NULL);

        /* Here, the application chooses the configuration it desires. In this
         * sample, we have a very simplified selection process, where we pick
         * the first EGLConfig that matches our criteria */
        eglChooseConfig(display, configAttribs, &config, 1, &numConfigs);

        /* EGL_NATIVE_VISUAL_ID is an attribute of the EGLConfig that is
         * guaranteed to be accepted by ANativeWindow_setBuffersGeometry().
         * As soon as we picked a EGLConfig, we can safely reconfigure the
         * ANativeWindow buffers to match, using EGL_NATIVE_VISUAL_ID. */
        eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_ID, &format);

        ANativeWindow_setBuffersGeometry(engine->app->window, 0, 0, format);

        surface = eglCreateWindowSurface(display, config, engine->app->window, surfaceAttribs);
        context = eglCreateContext(display, config, EGL_NO_CONTEXT, contextAttribs);

        if (!eglMakeCurrent(display, surface, surface, context)) {
            __android_log_print(ANDROID_LOG_WARN, "Native", "Unable to eglMakeCurrent");
            return;
        }

        eglQuerySurface(display, surface, EGL_WIDTH, &w);
        eglQuerySurface(display, surface, EGL_HEIGHT, &h);

        __android_log_print(ANDROID_LOG_INFO, "Native", "Canvas size: %d x %d", w, h);
        glViewport(0, 0, w, h);

        engine->display = display;
        engine->context = context;
        engine->surface = surface;
        engine->width   = w;
        engine->height  = h;
    }

    void term_display(struct engine* engine) {
        if (engine->display != EGL_NO_DISPLAY) {
            eglMakeCurrent(engine->display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
            if (engine->context != EGL_NO_CONTEXT) {
                eglDestroyContext(engine->display, engine->context);
            }
            if (engine->surface != EGL_NO_SURFACE) {
                eglDestroySurface(engine->display, engine->surface);
            }
            eglTerminate(engine->display);
        }
        engine->animating = 0;
        engine->display   = EGL_NO_DISPLAY;
        engine->context   = EGL_NO_CONTEXT;
        engine->surface   = EGL_NO_SURFACE;
    }

    void draw_frame(struct engine* engine) {
        if (engine->display == NULL) return;

        // Call into managed
        Render();

        // Swap buffers
        eglSwapBuffers(engine->display, engine->surface);
    }

    void handle_cmd(android_app *pApp, int32_t cmd) {
        auto* engine = (struct engine*)pApp->userData;
        switch (cmd) {
            case APP_CMD_INIT_WINDOW:
                // The window is being shown, get it ready.
                if (engine->app->window != nullptr) {
                    init_display(engine);
                    engine->animating = 1;
                    __android_log_print (ANDROID_LOG_INFO, "Native", "Calling draw_frame() from APP_CMD_INIT_WINDOW");
                    draw_frame(engine);
                }
                break;
            case APP_CMD_TERM_WINDOW:
                engine->animating = 0;
                term_display(engine);
                break;
            case APP_CMD_GAINED_FOCUS:
                engine->animating = 1;
                break;
            case APP_CMD_LOST_FOCUS:
                engine->animating = 0;
                __android_log_print (ANDROID_LOG_INFO, "Native", "Calling draw_frame() from APP_CMD_LOST_FOCUS");
                draw_frame(engine);
                break;
            default:
                break;
        }
    }

    int32_t handle_input(struct android_app* app, AInputEvent* event) {
        return 0;
    }

    void android_main(struct android_app *state) {
        struct engine engine;
        memset(&engine, 0, sizeof(engine));
        state->userData     = &engine;
        state->onAppCmd     = handle_cmd;
        state->onInputEvent = handle_input;
        engine.app = state;

        __android_log_print (ANDROID_LOG_INFO, "Native", "Entering android_main");

        int events;
        android_poll_source *pSource;
        do {
            if (ALooper_pollAll(0, nullptr, &events, (void **) &pSource) >= 0) {
                if (pSource) {
                    pSource->process(state, pSource);
                }
            }

            if (engine.animating) {
                __android_log_print (ANDROID_LOG_INFO, "Native", "Calling draw_frame() from loop");
                draw_frame(&engine);
            }

        } while (!state->destroyRequested);
    }
}
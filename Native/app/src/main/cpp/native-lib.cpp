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

    int animating;
    EGLDisplay display;
    EGLSurface surface;
    EGLContext context;
    int32_t width;
    int32_t height;
};

extern "C" {

    void handle_cmd(android_app *pApp, int32_t cmd) {
        auto* engine = (struct engine*)pApp->userData;
        switch (cmd) {
            case APP_CMD_INIT_WINDOW:
                // The window is being shown, get it ready.
                if (engine->app->window != nullptr) {
                    Render();
                }
                break;
            case APP_CMD_GAINED_FOCUS:
                engine->animating = 1;
                break;
            case APP_CMD_LOST_FOCUS:
                engine->animating = 0;
                Render();
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
                Render();
            }

        } while (!state->destroyRequested);
    }
}
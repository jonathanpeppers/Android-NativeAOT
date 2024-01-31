#include <android_native_app_glue.h>
#include <jni.h>
#include <android/log.h>
#include "dotnet.h"

const char* TAG = "NATIVE";

extern "C" {

    void handle_cmd(android_app *pApp, int32_t cmd) {
        __android_log_print (ANDROID_LOG_INFO, TAG, "handle_cmd: %i", cmd);
        switch (cmd) {
            case APP_CMD_INIT_WINDOW:
                // TODO: you can handle app lifecycle here
            default:
                break;
        }
    }

    int32_t handle_input(struct android_app* app, AInputEvent* event) {
        // TODO: you could handle input here
        return 0;
    }

    void android_main(struct android_app *state) {
        state->onAppCmd     = handle_cmd;
        state->onInputEvent = handle_input;

        __android_log_print (ANDROID_LOG_INFO, TAG, "Entering android_main");

        int result = ManagedAdd(1, 2);
        __android_log_print (ANDROID_LOG_INFO, TAG, "ManagedAdd(1, 2) returned: %i", result);

        int events;
        android_poll_source *pSource;
        do {
            if (ALooper_pollAll(0, nullptr, &events, (void **) &pSource) >= 0) {
                if (pSource) {
                    pSource->process(state, pSource);
                }
            }

            // TODO: this is the main message loop

        } while (!state->destroyRequested);
    }
}
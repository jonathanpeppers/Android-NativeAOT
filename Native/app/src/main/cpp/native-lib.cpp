#include <android_native_app_glue.h>
#include <jni.h>
#include "dotnet.h"

extern "C" {

    void handle_cmd(android_app *pApp, int32_t cmd) {
    }

    void android_main(struct android_app *pApp) {
        pApp->onAppCmd = handle_cmd;

        int result = ManagedAdd(1, 2);
        __android_log_print (ANDROID_LOG_INFO, "Native", "ManagedAdd(1, 2) returned: %i", result);

        int events;
        android_poll_source *pSource;
        do {
            if (ALooper_pollAll(0, nullptr, &events, (void **) &pSource) >= 0) {
                if (pSource) {
                    pSource->process(pApp, pSource);
                }
            }
        } while (!pApp->destroyRequested);
    }
}
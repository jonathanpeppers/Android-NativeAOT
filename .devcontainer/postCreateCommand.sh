#!/bin/bash -eux

echo "ANDROID_HOME=$ANDROID_HOME"

yes | sudo $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager "ndk;23.2.8568313" "cmake;3.22.1"

export "ANDROID_NDK_HOME=$ANDROID_HOME/ndk/23.2.8568313"
echo "ANDROID_NDK_HOME=$ANDROID_NDK_HOME"

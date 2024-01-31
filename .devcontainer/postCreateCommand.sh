#!/bin/bash -eux

echo "ANDROID_HOME=$ANDROID_HOME"

yes | sudo $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager "ndk;23.2.8568313" "cmake;3.22.1"

#!/bin/bash -eux

echo "ANDROID_HOME=$ANDROID_HOME"

yes | sudo $ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager "ndk;26.1.10909125"

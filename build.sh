#!/bin/bash -eux

PATH=~/Library/Android/sdk/ndk/25.1.8937393/toolchains/llvm/prebuilt/darwin-x86_64/bin/:$PATH \
    dotnet publish -r linux-bionic-arm64 -c Release -bl
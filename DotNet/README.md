# Build `libdotnet.so`

It's likely easiest on GitHub Codespaces, as I have configured a working environment.

Simply do:

```sh
cd DotNet
dotnet publish -c Release
```

This will create:

* `bin/Release/net8.0/linux-bionic-arm64/publish/libdotnet.so`

And copy to:

* `Native/app/src/main/libs/arm64-v8a/`

## Helpful Links

* https://github.com/exelix11/SysDVR/blob/master/Client/Client.csproj
* https://github.com/exelix11/SysDVR/blob/4f75bc6c985af4e4aca393077384ad6b8e27c4a7/Client/Platform/Android/buildbinaries.sh#L106-L108
* https://github.com/dotnet/runtime/issues/92272#issuecomment-1727181322

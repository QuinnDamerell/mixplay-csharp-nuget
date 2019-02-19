# MixPlay C# SDK
A C# Nuget Package For MixPlay! This soon-to-be nuget package wraps the official MixPlay C++ interactive SDK.

**In progress - the sdk not complete yet.**

# Sample Implementation 
See the mixplay-csharp-sample for an example usage.

# To Install 
- In Visual Studio, right click your project and click `Manage Nuget Packages...`
- Click the `browse` tab in the top left
- Search for `MixPlay.Unofficial`
- Click install

# To Build From Source
- Clone this repo
- Run `git submodule init` and `git submodule update`
- Open the solution in Visual Studio
- Build `mixplay-cpp` in Release for both `x86` and `x64`
- Build `mixplay-csharp-nuget` in Release
- Run `nuget pack Package.nuspec` from the command line to create a nuget package.

Note: If you try to manually link to the project from source in Visual Studio, the MixPlayCpp.dll native file will not be copied to the output directory correctly.

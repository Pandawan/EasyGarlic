# EasyGarlic (WIP)
An easy to use Garlicoin Miner

## TODO
- Add console debug
- Add support for user/password for pools that require it
- Add support for multi-download (it already works, just make the info text work too)
- Add AMD support (need and AMD card to test and contact API system)
- Make new logo & icons
- Display more information when mining (pool, pool's data...)
- Remove Xceed WPF (too many DLLs)

## Testing
Just open the SLN file in VS 2017 (2015 should work too), and click Start (in Debug Mode).

## Building for Release
Follow these steps to prepare a new release:
- Make sure that Config.Version, and AssemblyInfo.Version are changed
- Build the Solution in Release Mode
- Edit the `.nuspec` file to change version
- Package the app into a `.nupkg` file using NuGet pack (or the NuGet Package Explorer)
- Use Squirrel to build a new release file with the command `Squirrel --releasify <path to .nupkg> --releaseDir <path to Releases folder>`
- Upload new release to server

## Contributing
If you want to help, feel free to submit a PR.
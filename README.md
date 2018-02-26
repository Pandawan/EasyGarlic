# EasyGarlic (WIP)
An easy to use Garlicoin Miner

## TODO
- Remove Xceed WPF, too many DLLs just for a simple number picker
- Add support for multiple GPUs
- Add support for SGMiner (AMD)
- Allow installing multiple miners at once (it already works, just make the info text work too)
- Add advanced option to switch to cpuminer-opt
- Add more info on Mining Status
- Add info like estimated reward & estimated earned
- Add real logo & icons
- Make stratum use the best choice? (prefer dynamic 3333)
- Show more pool info when mining

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
# EasyGarlic (WIP)
An easy to use Garlicoin Miner

## TODO
- Add AMD support (need and AMD card to test and contact API system)
- Make new logo & icons
- Display more information when mining (pool, pool's data...)
- Add more error checking (connection errors, downloading errors)
- Add auto sending of log when app crashes (will also need computer's info to analyze data)
- Add some sort of data analysis tool (user count, download count...)
- Change Release URL to GitHub Releases (instead of local path)
- Remove Xceed WPF (too many DLLs)

## Testing
Just open the SLN file in VS 2017 (2015 should work too), and click Start (in Debug Mode).  
Make sure that your VS installation also includes the NuGet Package Manager, you can learn more about how to install it [here](https://docs.microsoft.com/en-us/nuget/install-nuget-client-tools#visual-studio).  

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

## Donate
Donate to support this project. Thank you!  

BTC: 13xqEPwdfkYFVdQvF596k91mWeUHRJTRjk  
LTC: LhrWcYnXxnA3TcznRRvkfuZ8TVYNJ2nisU  
BCH: 1PidoMufaqTyFMgdn37Gf4fCHf1GAKZT8w  
GRLC: GeVxW6scnydU7RhJZfCTr7eHXmxRFZccrB  
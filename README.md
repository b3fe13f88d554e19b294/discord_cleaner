# discord_cleaner
A Cleaner for Discord Malware! Cleans up simple malware taking advantage of Electron Injection.

## Logic:
The following `^app-\d\.\d\.\d+$` regex string to find all app-x.x.xxxx folders in the `%localappdata%\<flavor>` directory. It then finds the latest version for said flavor via this manipulation: `app-x.x.xxxx` -> `x.x.xxxx` -> `[x, x, xxxx]` -> `xxxxxx` and uses the highest number as the working directory. It then reaches for the `index.js` file stored in `<versionFolder>\modules\discord_desktop_core-x\discord_desktop_core` (I use a wildcard (*) to take any version of the desktop core.) and then removes every single bit of code past the mandatory `module.exports = require("./core.asar");`, which is where most Discord Electron Injections are! 
Voila, your Discord Flavor has been cleaned! It simply repeats this process for every flavor installed on your system!

## Issues
If you find a different kind of injection that could be implemented in this cleaner, do let me know by making an issue!

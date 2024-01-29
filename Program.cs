
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;


class Program {
    static string rootFolder = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft", "DiscordUtils");

    static Regex appChecker = new Regex(@"^app-\d\.\d\.\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    static string[] defaultFlavors = new string[]{ 
        "Discord", "DiscordPTB", "DiscordCanary"
    };

    static string[] startupRegistryKeys = new string[]{
        "Software\\Microsoft\\Windows NT\\CurrentVersion\\Windows",
        "Software\\Microsoft\\Windows\\CurrentVersion\\Run"
    };

    static string[] shortcutNames = new string[]{
        "Windows Explorer.lnk",
        "Utils.lnk"
    };

    static string getLatestVersion(string path) {
        IDictionary<double, string> versions = new Dictionary<double, string>();
        foreach (var file in Directory.GetDirectories(path, "app*")){
            string folderName = Path.GetFileName(file);
            
            MatchCollection match = appChecker.Matches(folderName);
            if (match.Count() < 1) continue;
             
            double.TryParse(string.Join("", folderName.Split("-")[1].Split(".")), out double version);
            versions.Add(version, file);
        }
        return versions[versions.Keys.Max()];
    }

    static void fixFlavor(string flavor="Discord") {
        string path = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), flavor);        
        if(!Directory.Exists(path)) return;
        
        string vers = getLatestVersion(path);
        if(vers.Length == 0) return;

        string[] directories = Directory.GetDirectories(Path.Combine(vers, "modules"), "discord_desktop_core-*");
        
        string highest = directories[0];
        int oldNumber = 0;  
        foreach (var dir in directories) {
            int.TryParse(Path.GetFileName(dir).Split("-")[1], out int number);
            if (number > oldNumber) {
                highest = dir;
                oldNumber = number;
            }
        }

        string jsPath = Path.Combine(highest, "discord_desktop_core", "index.js");
        string finalString = File.ReadAllText(jsPath);
        
        if (File.ReadLines(jsPath).Count() > 1) {
            Regex r = new Regex(@"module\.exports = require\(.*\);(.*)", RegexOptions.Singleline);
            string quote = "\"";
            finalString = r.Replace(finalString, @"module.exports = require("+ quote +"./core.asar"+ quote +");");
        }

        File.WriteAllText(jsPath, finalString);
    }

    static void Main(string[] args) { // discord_cleaner.exe <--PTB|--Canary|--Development>
        // Uninjects Discord
        if (args.Length > 1) {
            foreach (var farg in args) {
                string arg = farg[2..];
                fixFlavor("Discord"+arg);
            }
        } else {
            foreach (var flavor in defaultFlavors){
                fixFlavor(flavor);
            }
        }

        // Removes root folder
        if(Directory.Exists(rootFolder)) Directory.Delete(rootFolder, true);

        // Removes any registry key
        RegistryKey ureg = Environment.Is64BitOperatingSystem ?
            RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64) : RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);

        foreach(var key in startupRegistryKeys) {
            RegistryKey subKey = ureg.OpenSubKey(key, true);
            if (subKey == null) continue;
            subKey.DeleteValue("Explorer", false);
            subKey.DeleteValue("Discord Utils", false);
            subKey.DeleteValue("Load", false);
        }

        // Removes Scheduled Tasks
        using (TaskService t = new TaskService()) {
            t.RootFolder.DeleteTask("Explorer", false);
            t.RootFolder.DeleteTask("Discord Utils", false);
        }

        // Remove Shortcuts
        DirectoryInfo dir = new(Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "Microsoft", "Windows", "Start Menu", "Programs", "Startup"));
        foreach (var p in Directory.GetFiles(dir.FullName)) {
            FileInfo f = new(p);
            if (shortcutNames.Contains(f.Name)) {
                f.Delete();
            }
        }
    }
}
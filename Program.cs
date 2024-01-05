
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program {
    static Regex appChecker = new Regex(@"^app-\d\.\d\.\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    static string[] defaultFlavors = new string[]{ 
        "Discord", "DiscordPTB", "DiscordCanary"
    };

    static string getLatestVersion(string path) {
        IDictionary<double, string> versions = new Dictionary<double, string>();
        foreach (var file in Directory.GetDirectories(path, "app*")){
            string folderName = Path.GetFileName(file);
            
            MatchCollection match = appChecker.Matches(folderName);
            if (match.Count() < 1) continue;
            
            string[] n = folderName.Split("-")[1].Split(".");
            string versionString = n[0].ToString() + n[1].ToString() + "." + n[2].ToString();
            double.TryParse(versionString, out double version);
            versions.Add(version, file);
        }
        return versions[versions.Keys.Max()];
    }

    static void fixFlavor(string flavor="Discord") {
        string path = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), flavor);        
        if(!Directory.Exists(path)) return;
        
        string vers = getLatestVersion(path);
        if(vers.Length == 0) return;

        string jsPath = Path.Combine(Directory.GetDirectories(Path.Combine(vers, "modules"), "discord_desktop_core-*")[0], "discord_desktop_core", "index.js");
        string finalString = File.ReadAllText(jsPath);
        
        if (File.ReadLines(jsPath).Count() > 1) {
            Regex r = new Regex(@"module\.exports = require\(.*\);(.*)", RegexOptions.Singleline);
            string quote = "\"";
            finalString = r.Replace(finalString, @"module.exports = require("+ quote +"./core.asar"+ quote +");");
        }

        File.WriteAllText(jsPath, finalString);
    }

    static void Main(string[] args) { // discord_cleaner.exe <--PTB|--Canary|--Development>
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
    }
}
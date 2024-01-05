
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program {
    static Regex appChecker = new Regex(@"^app-\d\.\d\.\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    static string[] flavors = new string[]{ 
        "Discord", "DiscordPTB", "DiscordCanary"
    };

    static void fixFlavor(string flavor="Discord") {
        string path = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), flavor);
        Console.WriteLine(path);
        if(Directory.Exists(path)) {
            IDictionary<int, string> vers = new Dictionary<int, string>();
            foreach (var file in Directory.GetDirectories(path, "app*")){
                string folderName = Path.GetFileName(file);
                MatchCollection match = appChecker.Matches(folderName);
                if(!(match.Count > 0)) {
                    // Not an app.x.x.x+ folder
                    continue;
                }

                string versionString = folderName.Split("-")[1];
                string[] vComp = versionString.Split(".");
                versionString = "";
                foreach (var item in vComp){
                    int vnum;
                    int.TryParse(item, out vnum);
                    versionString += vnum.ToString();
                }
                int version;
                int.TryParse(versionString, out version);
                vers.Add(version, file);
            }
            string finalPath = vers[vers.Keys.Max()];

            string jsPath = Path.Combine(Directory.GetDirectories(Path.Combine(finalPath, "modules"), "discord_desktop_core-*")[0], "discord_desktop_core", "index.js");
            
            int lineCount = File.ReadLines(jsPath).Count();
            

            string finalString = File.ReadAllText(jsPath);
            if (lineCount > 1) {
                Regex r = new Regex(@"module\.exports = require\(.*\);(.*)", RegexOptions.Singleline);
                string quote = "\"";
                finalString = r.Replace(finalString, @"module.exports = require("+ quote +"./core.asar"+ quote +");");
            }
            File.WriteAllText(jsPath, finalString);
        }
    }

    static void Main(string[] args) {
        foreach (var flavor in flavors){
            fixFlavor(flavor);
        }
    }
}
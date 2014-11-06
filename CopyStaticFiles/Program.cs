﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CopyStaticFiles {

    class Program {

        static void Main(string[] args) {
            // NOTE paths are relative to Bin 

            const string
                outputDir = "../StaticFiles/",
                samplesDir = "../AjaxControlToolkit.SampleSite",
                contentDir = "Content/AjaxControlToolkit/",
                scriptsDir = "Scripts/AjaxControlToolkit/",
                stylesDir = contentDir + "Styles",
                imagesDir = contentDir + "Images";

            foreach(var path in Directory.EnumerateFiles("../AjaxControlToolkit/Scripts", "*.js"))
                LinkScript(Path.Combine(outputDir, scriptsDir), path);

            foreach(var path in Directory.EnumerateFiles("../AjaxControlToolkit/Scripts/Localization", "*.js"))
                LinkScript(Path.Combine(outputDir, scriptsDir), path, TransformLocalizationScriptName);

            foreach(var path in Directory.EnumerateFiles("../AjaxControlToolkit/Styles", "*.css"))
                LinkStyle(Path.Combine(outputDir, stylesDir), path);

            foreach(var path in Directory.EnumerateFiles("../AjaxControlToolkit/Images")) {
                if(Regex.IsMatch(path, @"\.(gif|jpg|png)$"))
                    LinkStyle(Path.Combine(outputDir, imagesDir), path);
            }

            LinkSamples(outputDir, samplesDir, scriptsDir);
            LinkSamples(outputDir, samplesDir, contentDir);
        }

        static void LinkScript(string prefix, string path, Func<string, string> fileNameTransformer = null) {
            var fileName = Path.GetFileName(path);

            if(fileNameTransformer != null)
                fileName = fileNameTransformer(fileName);

            if(fileName.EndsWith(".min.js"))
                fileName = Path.Combine("Release", fileName.Replace(".min.js", ".js"));
            else
                fileName = Path.Combine("Debug", fileName.Replace(".js", ".debug.js"));

            CreateHardLink(path, Path.Combine(prefix, fileName));
        }

        static void LinkStyle(string prefix, string path) {
            var fileName = Path.GetFileName(path);

            switch(fileName) {
                case "Backgrounds.css":
                case "Backgrounds.min.css":
                    return;

                case "Backgrounds_static.css":
                    fileName = "Backgrounds.css";
                    break;

                case "Backgrounds_static.min.css":
                    fileName = "Backgrounds.min.css";
                    break;
            }

            CreateHardLink(path, Path.Combine(prefix, fileName));
        }

        static void LinkSamples(string outputDir, string samplesDir, string filePrefix) {
            var samplesScriptsDirName = Path.Combine(samplesDir, filePrefix);
            var staticFilesDirName = Path.GetFullPath(Path.Combine(outputDir, filePrefix));

            CreateSymbolicLink(staticFilesDirName, samplesScriptsDirName);
        }

        static string TransformLocalizationScriptName(string name) {
            return "Localization." + name.Replace("Resources_", "Resources.");
        }

        static void CreateHardLink(string source, string destination) {
            EnsurePath(destination);

            if(!CreateHardLink(destination, source, IntPtr.Zero))
                throw new Exception("Failed to create hardlink");
        }

        static void CreateSymbolicLink(string source, string destination) {
            EnsurePath(destination);

            Directory.Delete(destination);

            if(!CreateSymbolicLink(destination, source, 1))
                throw new Exception("Failed to create symlink");
        }

        static void EnsurePath(string path) {
            var dir = Path.GetDirectoryName(path);
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if(File.Exists(path))
                File.Delete(path);
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);
    }

}

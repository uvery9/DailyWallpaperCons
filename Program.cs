﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyWallpaper
{
    class Program
    {
        static async Task MainSTD(string[] args)
        {
            await DailyWallpaper();
        }
        static async Task Main(string[] args)
        {

            string logFile = new FileInfo("log.txt").FullName;
            Console.WriteLine($"Set stdoutput and stderr to file: {logFile}");
            Console.WriteLine("Please be PATIENT, the result will not be lost.");           
            Console.WriteLine($"------  {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}  ------");
            using (var writer = new StreamWriter(logFile))
            {
                Console.SetOut(writer);
                Console.SetError(writer);
                //Console.Error.WriteLine("Error information written to begin");
                try
                {
                    await DailyWallpaper();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
                //Console.Error.WriteLine("Error information written to TEST");
                Console.Out.Close();
                Console.Error.Close();
                writer.Close();
                // Console.SetOut(Console.OpenStandardOutput());
            }
            
            // redirect stderr to default.
            var standardError = new StreamWriter(Console.OpenStandardError());
            standardError.AutoFlush = true;
            Console.SetError(standardError);

            // redirect stdout to default.
            var standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);

            // print the log file.
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(File.ReadAllText("log.txt"));
        }

        /*TODO*/
        // generate single file excutable
        private async static Task DailyWallpaper()
        {
            var iniFile = new ConfigIni();           
            RunAtStartup(iniFile);
            CreateUsageText(iniFile, "USAGE.TXT");
            var iniDict = iniFile.GetCfgFromIni();
            if (iniDict["useLocal"].ToLower().Equals("yes")) {
                var localImage = new LocalImage(iniFile, iniDict["imgDir"]);
                localImage.RandomSelectOneImgToWallpaper();
            } else if (iniDict["useOnline"].ToLower().Equals("yes"))
            {
                var onlineImage = new OnlineImage(iniFile);
                await onlineImage.RandomChoiceFromList();
            }             
        }

        public static void RunAtStartup(ConfigIni ini)
        {
            // ConfigIni iniFile
            string want2AutoRun = ini.Read("want2AutoRun").ToLower();
            if (want2AutoRun.Equals("yes") || want2AutoRun.Equals("once"))
            {
                AutoStartupHelper.CreateAutorunShortcut();
                if (want2AutoRun.Equals("once"))
                {
                    ini.Write("want2AutoRun", "yes/no");
                }
            }
            else if (want2AutoRun.Equals("no"))
            {
                if (!AutoStartupHelper.IsAutorun())
                {
                    return;
                }
                Console.WriteLine("You don't want this program run at Windows startup.");
                AutoStartupHelper.RemoveAutorunShortcut();
            }
        }

        private static void CreateUsageText(ConfigIni ini, string textFile, string usageText=null)
        {
            if (ini.GetCfgFromIni()["createUsageStat"].ToLower().Equals("no"))
            {
                return;
            }
            if (ini.GetCfgFromIni()["createUsageStat"].ToLower().Equals("once"))
            {
                ini.UpdateIniItem("createUsageStat", "no", ini.exeName);
            }
            if (File.Exists(textFile)) {
                Console.WriteLine($"File already exists: {textFile}");
                return;
            }
            usageText = usageText ?? GetUsageText();
            File.WriteAllText(textFile, usageText);
            Console.WriteLine($"Created usage file: {textFile}");
        }

        private static string GetUsageText() {
            string usageText = "Usage for DailyWallpaper.exe\n" +
                "AUTHOR: HDC <jared.dcx@gmail.com>\n" +
                "-----------------------------------------\n" +
                "Notice: there is only ONE file you need to configure: config.ini, \n" +
                "        it should be with DailyWallpaper.exe\n" +
                "-----------------------------------------\n" +
                "here is a sample of config.ini:\n" +
                "\n" +
                "[DailyWallpaper]\n" +
                "useLocal=no\n" +
                "useOnline=yes\n" +
                "createUsageStat=no\n" +
                "want2AutoRun=yes/no\n" +
                "[Online]\n" +
                "saveDir=C:\\Users\\jared\\Pictures\\DailyWallpaper\n" +
                "bingChina=yes\n" +
                "alwaysDLBingWallpaper=yes\n" +
                "dailySpotlight=yes\n" +
                "dailySpotlightDir=AUTO\n" +
                "[Local]\n" +
                "imgDir=C:\\Users\\jared\\Pictures\n" +
                "scan=yes\n" +
                "copyFolder=None\n" +
                "want2Copy=no\n" +
                "mTime=NULL\n" +
                "lastImgDir=NULL\n" +
                "[LOG]\n" +
                "wallpaper=C:\\Users\\jared\\Pictures\\DailyWallpaper\\2021-0622_11-44-45.jpeg    2021-06-22 23:52:59\n" +
                "\n" +
                "---------------------\n" +
                "Section DailyWallpaper\n" +
                "1. useLocal                       Use local image, which means use \"Section WallpaperSetter\" feature.\n" +
                "2. useOnline                      Download the image and set it as wallpaper.\n" +
                "3. createUsageStat                Create and usage file flag: yes, once, no\n" +
                "                                    once:   when 'USAGE.TXT' doesn't exist, create once, you can delete, it won't create next time.\n" +
                "                                    yes:   when 'USAGE.TXT' doesn't exist, create\n" +
                "                                    no:     literally.\n" +
                "4. want2AutoRun                   copy .lnk to startup folder: autorun DailyWallpaper.exe when windows starup.\n" +
                "--------\n" +
                "Section Online\n" +
                "1. saveDir                        Where the image will be saved.\n" +
                "2. bingChina                      Download \"bingchina\" 's image and set it as wallpaper\n" +
                "3. alwaysDLBingWallpaper          Always download bingchina wallpaper\n" +
                "4. dailySpotlight                 Copy the image from daily.spotlight folder and set it as wallpaper \n" +
                "                                    [You have to open the feature in Windows10]\n" +
                "5. dailySpotlightDir              Set to AUTO, or set by yourself.\n" +
                "--------\n" +
                "Section Local\n" +
                "1. imgDir:                       The program will scan this folder and select a image as wallpaper\n" +
                "2. scan:                         Controlling the action of SCANNING, it has three options: yes, no, force\n" +
                "                                    yes:   when 'img_dir' has been modified by OS, scan and update '_img_list.txt'\n" +
                "                                    no:    never scan 'img_dir' unless '_img_list.txt' doesn't exist.\n" +
                "                                    force: Mandatory scan 'img_dir' and update '_img_list.txt'\n" +
                "3. copyFolder:                   Copy all suitable pictures to this folder from copy_folder, control by 'want2copy'\n" +
                "4. want2Copy:                    Controlling the action of COPYING, it has two options: yes, no\n" +
                "-----------------------------------------\n" +
                "FOR FREEDOM!";
            return usageText;
        }
    }
}

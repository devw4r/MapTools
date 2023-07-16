// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using AlphaCoreExtractor.MPQ;
using AlphaCoreExtractor.DBC;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Generators;
using AlphaCoreExtractor.Core.Terrain;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor
{
    class Program
    {
        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        private static Thread MapsThread;
        private static volatile bool IsRunning = false;

        static void Main(string[] args)
        {
            IsRunning = true;
            MapsThread = new Thread(new ThreadStart(StartProcess));
            MapsThread.Name = "MapsThread";
            MapsThread.Start();

            while (IsRunning)
                Thread.Sleep(2000);

            MapsThread.Join(2000);
            MapsThread.Interrupt();
            Console.ReadLine();
            SetDefaultTitle();
        }

        private static void StartProcess()
        {
            SetDefaultTitle();
            PrintHeader();

            try
            {
                if (!Configuration.Initialize())
                {
                    Logger.Error("Unable to read Config.ini, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                else
                    PrintConfiguration();

                if (Configuration.AllParseDisabled)
                {
                    Logger.Error("MapGeneration, ObjGeneration and MeshGeneration are disabled, please select at least one in the configuration file.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // Extract Map.dbc and AreaTable.dbc
                if (!MPQExtractor.ExtractDBC())
                {
                    Logger.Error("Unable to extract DBC files, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // Load both files in memory.
                if (!DBCStorage.Initialize())
                {
                    Logger.Error("Unable to initialize DBC Storage, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // If user wants to build nav or obj, extract WMOs for future parsing.
                if (Configuration.ShouldParseWMOs)
                {
                    // Extract WMO (World Model Object) files.
                    if (!MPQExtractor.ExtractWMOFiles())
                    {
                        Logger.Error("Unable to extract WMO files, exiting...");
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                }

                // If user wants to build nav or obj, extract MDXs for future parsing.
                if (Configuration.ShouldParseMDXs)
                {
                    // Extract MDX models.
                    if (!MPQExtractor.ExtractMDX())
                    {
                        Logger.Error("Unable to extract MDX files, exiting...");
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                }

                // Extract available WDT Terrains and match them to a DBC map.
                Dictionary<DBCMap, string> WDTFiles;
                if (!MPQExtractor.ExtractWDTFiles(out WDTFiles))
                {
                    Logger.Error("Unable to extract WDT files, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // Flush .map files output dir.
                Directory.Delete(Paths.OutputMapsPath, true);

                //Begin parsing adt files and generate .map files.
                foreach (var entry in WDTFiles)
                    using (WDT map = new WDT(entry.Key, entry.Value)) // Key:DbcMap Value:FilePath
                        Logger.Success($"Finished processing {map.Name}.");

                WDTFiles?.Clear();
                Console.WriteLine();
                if (Configuration.GenerateMaps == GenerateMaps.Enabled)
                    Logger.Success($"Generated a total of {DataGenerator.GeneratedMaps} .map files.");
                if (Configuration.GenerateObjs == GenerateObjs.Enabled)
                    Logger.Success($"Generated a total of {DataGenerator.GeneratedObjs} .obj files.");
                if (Configuration.GenerateNavs == GenerateNavs.Enabled)
                    Logger.Success($"Generated a total of {DataGenerator.GeneratedNavs} .nav files.");
                Logger.Success("Process Complete, press any key to exit...");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
            }
            finally
            {
                IsRunning = false;
            }
        }

        public static void SetDefaultTitle()
        {
            Console.Title = $"AlphaCore Map Extractor {Version}";
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("TheAlphaProject");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Discord: https://discord.gg/RzBMAKU");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Github: https://github.com/The-Alpha-Project");
            Console.WriteLine();
            Console.ResetColor();
        }

        private static void PrintConfiguration()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Configuration");
            Console.WriteLine($"Heightmap Z Resolution: {Configuration.ZResolution}");
            Console.WriteLine($"WoW Path: {Configuration.WoWPath}");
            Console.WriteLine($"GenerateMaps: {Configuration.GenerateMaps}");
            Console.WriteLine($"GenerateNavs: {Configuration.GenerateNavs}");
            Console.WriteLine($"GenerateObjs: {Configuration.GenerateObjs}");
            Console.WriteLine($"ParseSelection: {Configuration.ParseSelection}");
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}

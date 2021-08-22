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
using AlphaCoreExtractor.Core;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Generators;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor
{
    class Program
    {
        public static List<DBCMap> LoadedMaps = new List<DBCMap>();
        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        private static Thread MapsThread;
        private static volatile bool IsRunning = false;
        public static int ZResolution = 256;

        static void Main(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out int zResolution))
                ZResolution = zResolution;

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
                // Extract Map.dbc and AreaTable.dbc
                if (!DBCExtractor.ExtractDBC())
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

                // Extract available maps inside MPQ
                Dictionary<DBCMap, string> WDTFiles;
                if (!WDTExtractor.ExtractWDTFiles(out WDTFiles))
                {
                    Logger.Error("Unable to extract WDT files, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // Flush .map files output dir.
                Directory.Delete(Paths.OutputMapsPath, true);

                int GeneratedMapFiles = 0;
                //Begin parsing adt files and generate .map files.
                foreach (var entry in WDTFiles)
                {
                    using (CMapObj map = new CMapObj(entry.Key, entry.Value)) // Key:DbcMap Value:FilePath
                    {
                        //TerrainMeshGenerator.BuildTerrainMesh(map);
                        MapFilesGenerator.GenerateMapFiles(map, out int generatedMaps);
                        GeneratedMapFiles += generatedMaps;
                        LoadedMaps.Add(entry.Key);
                    }

                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                }

                WDTFiles?.Clear();
                Console.WriteLine();
                Logger.Success($"Generated a total of {GeneratedMapFiles} .map files.");
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
            Console.WriteLine("TheAlphaProject");
            Console.WriteLine("Discord: https://discord.gg/RzBMAKU");
            Console.WriteLine("Github: https://github.com/The-Alpha-Project");
            Console.WriteLine($"Using Z Resolution: {ZResolution}");
            Console.WriteLine();
        }
    }
}

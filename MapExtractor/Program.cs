// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Reflection;
using System.Collections.Generic;

using AlphaCoreExtractor.MPQ;
using AlphaCoreExtractor.DBC;
using AlphaCoreExtractor.Core;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor
{
    class Program
    {
        public static List<CMapObj> LoadedMaps = new List<CMapObj>();
        private static Version Version;
        private static Queue<char> Loading = new Queue<char>();

        static void Main(string[] args)
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;
            SetDefaultTitle();
            PrintHeader();

            if (!DBCExtractor.ExtractDBC())
            {
                Console.WriteLine("Unable to extract DBC files, exiting...");
                Console.ReadLine();
                Environment.Exit(0);
            }

            if (!DBCStorage.Initialize())
            {
                Console.WriteLine("Unable to initialize DBC Storage, exiting...");
                Console.ReadLine();
                Environment.Exit(0);
            }

            Dictionary<DBCMap, string> WDTFiles;
            if (!WDTExtractor.ExtractWDTFiles(out WDTFiles))
            {
                Console.WriteLine("Unable to extract WDT files, exiting...");
                Console.ReadLine();
                Environment.Exit(0);
            }

            // For test only, extract only Azeroth.
            // TODO: We need to process, build map files, and release memory ASAP.
            // Right now, loading all data we are parsing would result in around 10gb.

            //Begin loading all wdt information.
            if (!Globals.LoadAsync)
            {
                foreach (var entry in WDTFiles)
                {
                    var map = new CMapObj(entry.Key, entry.Value, null); // Key:DbcMap Value:FilePath
                    map.LoadData();
                    LoadedMaps.Add(map);
                    break;
                }
            }
            else
            {
                foreach (var entry in WDTFiles)
                {
                    AsyncMapLoader loadMapTask = new AsyncMapLoader(entry.Key, entry.Value);
                    loadMapTask.OnMapLoaded += OnMapLoaded;
                    loadMapTask.RunWorkerAsync();
                    break;
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Map finished loading, clean up and add it to our LoadedMaps collection.
        /// </summary>
        private static void OnMapLoaded(object sender, AsyncMapLoaderEventArgs e)
        {
            if (sender is AsyncMapLoader loader)
            {
                loader.OnMapLoaded -= OnMapLoaded;
                loader.Dispose();
            }

            if (e.Map != null)
                LoadedMaps.Add(e.Map);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);

            SetDefaultTitle();
        }

        #region crap
        public static void SetDefaultTitle()
        {
            Console.Title = $"AlphaCore Map Extractor {Version}";
        }

        private static void PrintHeader()
        {
            Console.WriteLine("TheAlphaProject");
            Console.WriteLine("Discord: https://discord.gg/RzBMAKU");
            Console.WriteLine("Github: https://github.com/The-Alpha-Project");
            Console.WriteLine();
        }

        private static DateTime LastReport = DateTime.UtcNow;
        private static object _lock = new object();
        /// <summary>
        /// We dont report real progress, eventually, we could estimate.
        /// </summary>
        public static void UpdateLoadingStatus()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                if (now.Subtract(LastReport).Milliseconds > 300)
                {
                    Loading.Enqueue('.');
                    Console.Title = $"AlphaCore Map Extractor {Version}  |  Loading, please wait {string.Join("", Loading.ToArray())}";

                    if (Loading.Count > 5)
                        Loading.Clear();

                    LastReport = now; // + elapsed? I dont care.
                }
            }
        }
        #endregion
    }
}

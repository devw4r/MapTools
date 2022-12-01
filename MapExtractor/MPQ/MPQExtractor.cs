// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using MpqLib;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.DBC;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor.MPQ
{
    public static class MPQExtractor
    {
        private static List<string> InterestedDBC = new List<string>() { "AreaTable", "Map" };
        private static List<string> InterestedMDX = new List<string>() { "World" };

        /// <summary>
        /// Extract all available MDX models.
        /// </summary>
        public static bool ExtractMDX()
        {
            try
            {
                Logger.Notice("Extracting MDX models for mesh generation...");
                //Check if dbc.MPQ exist.
                if (!File.Exists(Paths.ModelsPath))
                {
                    Logger.Error($"Unable to locate model.MPQ at path {Paths.ModelsPath}, please check Config.ini and set a proper installation path.");
                    return false;
                }

                // Clean up output directory if neccesary.
                if (Directory.Exists(Paths.ModelsLoadPath))
                    Directory.Delete(Paths.ModelsLoadPath, true);

                using (MpqArchive archive = new MpqArchive(Paths.ModelsPath))
                {
                    archive.AddListfileFilenames();
                    var interestedMDX = archive.Select(e => e).Where(e => !string.IsNullOrEmpty(e.Filename) && InterestedMDX.Any(name => e.Filename.ToLower().StartsWith("world"))).ToList();
                    var total = interestedMDX.Count;
                    var proccesed = 0;
                    foreach (var entry in interestedMDX)
                    {
                        if (!string.IsNullOrEmpty(entry.Filename))
                        {
                            var outputFileName = Paths.GetModelPath(entry.Filename);
                            var outputPlainName = Path.GetFileNameWithoutExtension(outputFileName);

                            if (File.Exists(outputFileName))
                                File.Delete(outputFileName);

                            byte[] buf = new byte[0x40000];
                            using (Stream streamIn = archive.OpenFile(entry))
                            {
                                using (Stream streamOut = new FileStream(outputFileName, FileMode.Create))
                                {
                                    while (true)
                                    {
                                        int cb = streamIn.Read(buf, 0, buf.Length);
                                        if (cb == 0)
                                            break;

                                        streamOut.Write(buf, 0, cb);
                                    }

                                    streamOut.Close();
                                }
                            }

                            proccesed++;
                            Logger.Progress($"Extracting MDX models for mesh generation", proccesed, total, 0);
                        }
                    }

                    Logger.Success($"Extracted {total} MDX files.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message);
                    Logger.Error(ex.InnerException.StackTrace);
                }
            }

            return false;
        }

        /// <summary>
        /// Extract WMO files from 0.5.3 client.
        /// </summary>
        /// <returns></returns>
        public static bool ExtractWMOFiles()
        {
            try
            {               
                Logger.Notice("Extracting WMO files for mesh generation...");
                // Clean up output directory if neccesary.
                if (Directory.Exists(Paths.WMOLoadPath))
                    Directory.Delete(Paths.WMOLoadPath, true);
                Directory.CreateDirectory(Paths.WMOLoadPath);

                if (!Directory.Exists(Paths.InputWMOPath))
                {
                    Logger.Error($"Unable to locate {Paths.InputWMOPath}, please check Config.ini and set a proper installation path.");
                    return false;
                }

                var wmos = Directory.EnumerateFiles(Paths.InputWMOPath, "*", SearchOption.AllDirectories).ToList();
                var total = wmos.Count;
                var proccesed = 0;
                foreach (var file in wmos)
                {
                    if (Path.GetExtension(file).ToLower() == ".mpq")
                    {
                        var wmoOutputName = Paths.GetWMOPath(file);
                        try
                        {
                            if (!ExtractFile(file, Path.GetDirectoryName(wmoOutputName), false, out string outputWdtPath))
                                Logger.Warning($"Unable to extract WMO {Path.GetFileName(file)}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message);
                            return false;
                        }
                    }

                    proccesed++;
                    Logger.Progress("Extracting WMO files for mesh generation", proccesed, total, 0);
                }

                Logger.Success($"Extracted {total} WMO files.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Extract all WDT (Terrain) from 0.5.3 client.
        /// </summary>
        /// <returns></returns>
        public static bool ExtractWDTFiles(out Dictionary<DBCMap, string> wdtFiles)
        {
            wdtFiles = new Dictionary<DBCMap, string>();
            try
            {
                Logger.Notice("Extracting WDT files...");
                // Clean up output directory if neccesary.
                if (Directory.Exists(Paths.WDTLoadPath))
                    Directory.Delete(Paths.WDTLoadPath, true);
                Directory.CreateDirectory(Paths.WDTLoadPath);

                if (!Directory.Exists(Paths.InputMapsPath))
                {
                    Logger.Error($"Unable to locate {Paths.InputMapsPath}, please check Config.ini and set a proper installation path.");
                    return false;
                }

                foreach (var dir in Directory.EnumerateDirectories(Paths.InputMapsPath))
                {
                    var folderMapName = Path.GetFileName(dir);
                    if (DBCStorage.TryGetMapByName(folderMapName, out DBCMap map))
                    {
                        foreach (var file in Directory.EnumerateFiles(dir))
                        {
                            if (file.Contains("wdt"))
                            {
                                var filePath = Paths.Combine(dir, Path.GetFileName(file));
                                if (ExtractFile(filePath, Paths.WDTLoadPath, true, out string outputWdtPath))
                                    wdtFiles.Add(map, outputWdtPath);
                            }
                        }
                    }
                    else
                    {
                        Logger.Warning($"Unable to locate DBC map for: {folderMapName}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private static bool ExtractFile(string fileName, string outputDir, bool reportProgress, out string outputPath)
        {
            outputPath = string.Empty;
            try
            {
                using (MpqArchive archive = new MpqArchive(fileName))
                {
                    byte[] buf = new byte[0x80000];
                    var outputName = Path.GetFileNameWithoutExtension(fileName);

                    if(reportProgress)
                        Logger.Info($"Extracting {outputName}...");

                    foreach (MpqEntry entry in archive)
                    {
                        if (!entry.IsCompressed || entry.IsEncrypted)
                            continue;

                        entry.Filename = outputName;
                        string srcFile = entry.Filename;

                        outputPath = Paths.Combine(outputDir, srcFile.ToLower());

                        // Copy to destination file
                        using (Stream streamIn = archive.OpenFile(entry))
                        {
                            Int64 total = streamIn.Length;
                            Int64 processed = 0;
                            using (Stream streamOut = new FileStream(outputPath, FileMode.Create))
                            {
                                while (true)
                                {
                                    int cb = streamIn.Read(buf, 0, buf.Length);
                                    if (cb == 0)
                                        break;

                                    streamOut.Write(buf, 0, cb);

                                    processed += cb;
                                    if(reportProgress)
                                        Logger.Progress($"Extracting {Path.GetExtension(entry.Filename)} file", processed, (uint)total, 200);
                                }

                                streamOut.Close();
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if(reportProgress)
                    Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Extract AreaTable and Map dbc from 0.5.3 client.
        /// </summary>
        public static bool ExtractDBC()
        {
            try
            {
                Logger.Notice("Extracting DBC files...");
                //Check if dbc.MPQ exist.
                if(!File.Exists(Paths.DBCMPQPath))
                {
                    Logger.Error($"Unable to locate dbc.MPQ at path {Paths.DBCMPQPath}, please check Config.ini and set a proper installation path.");
                    return false;
                }

                // Clean up output directory if neccesary.
                if (Directory.Exists(Paths.DBCLoadPath))
                    Directory.Delete(Paths.DBCLoadPath, true);
 

                using (MpqArchive archive = new MpqArchive(Paths.DBCMPQPath))
                {
                    archive.AddListfileFilenames();
                    foreach (var entry in archive)
                    {
                        if (!string.IsNullOrEmpty(entry.Filename))
                        {                         
                            var outputFileName = Paths.Combine(Paths.DBCLoadPath, Path.GetFileName(entry.Filename));
                            var outputPlainName = Path.GetFileNameWithoutExtension(outputFileName);

                            if (File.Exists(outputFileName))
                                File.Delete(outputFileName);

                            if (InterestedDBC.Any(name => outputPlainName.ToLower().Equals(name.ToLower())))
                            {
                                byte[] buf = new byte[0x40000];
                                using (Stream streamIn = archive.OpenFile(entry))
                                {
                                    using (Stream streamOut = new FileStream(outputFileName, FileMode.Create))
                                    {
                                        while (true)
                                        {
                                            int cb = streamIn.Read(buf, 0, buf.Length);
                                            if (cb == 0)
                                                break;

                                            streamOut.Write(buf, 0, cb);
                                        }

                                        streamOut.Close();
                                    }
                                }

                                Logger.Success($"Extracted DBC file [{entry.Filename}].");
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message);
                    Logger.Error(ex.InnerException.StackTrace);
                }
            }

            return false;
        }
    }
}

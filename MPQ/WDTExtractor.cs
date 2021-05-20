// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using MpqLib;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.MPQ
{
    public static class WDTExtractor
    {
        private static string OutputDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "wdt");
        public static bool ExtractWDTFiles(out List<string> wdtFiles)
        {
            wdtFiles = new List<string>();

            try
            {
                Console.WriteLine("Extracting WDT files...");
                // Clean up output directory if neccesary.
                if (Directory.Exists(OutputDirectory))
                    Directory.Delete(OutputDirectory, true);
                Directory.CreateDirectory(OutputDirectory);

                if (!Directory.Exists(Globals.MapsPath))
                {
                    Console.WriteLine($"Unable to locate {Globals.MapsPath}, please check Config.txt and set a proper installation path.");
                    return false;
                }

                foreach (var dir in Directory.EnumerateDirectories(Globals.MapsPath))
                {
                    foreach (var file in Directory.EnumerateFiles(dir))
                    {
                        if (file.Contains("wdt"))
                        {
                            if (ExtractWDT(file, out string outputWdtPath))
                                wdtFiles.Add(outputWdtPath);

                            // TODO: Just load Azeroth.wdt, while we figure how to actually generate map files from this.
                            break;
                        }
                    }

                    // TODO: Just load Azeroth.wdt, while we figure how to actually generate map files from this.
                    break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private static bool ExtractWDT(string fileName, out string outputWdtPath)
        {
            outputWdtPath = string.Empty;
            try
            {
                using (MpqArchive archive = new MpqArchive(fileName))
                {
                    byte[] buf = new byte[0x40000];
                    var outputName = Path.GetFileNameWithoutExtension(fileName);

                    Console.WriteLine($"Extracting {outputName} ...");

                    foreach (MpqEntry entry in archive)
                    {
                        if (!entry.IsCompressed || entry.IsEncrypted)
                            continue;

                        entry.Filename = outputName;
                        string srcFile = entry.Filename;
                        outputWdtPath = Path.Combine(OutputDirectory, srcFile);

                        // Copy to destination file
                        using (Stream streamIn = archive.OpenFile(entry))
                        {
                            using (Stream streamOut = new FileStream(outputWdtPath, FileMode.Create))
                            {
                                while (true)
                                {
                                    int cb = streamIn.Read(buf, 0, buf.Length);
                                    if (cb == 0)
                                        break;

                                    streamOut.Write(buf, 0, cb);
                                    Program.UpdateLoadingStatus();
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
                Console.WriteLine(ex.Message);
            }

            return false;

        }
    }
}

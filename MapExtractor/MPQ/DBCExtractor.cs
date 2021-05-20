﻿// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Reflection;

using MpqLib;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.MPQ
{
    public static class DBCExtractor
    {
        private static string OutputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dbc");
        
        public static bool ExtractDBC()
        {
            try
            {
                Console.WriteLine("Extracting DBC files...");
                //Check if dbc.MPQ exist.
                if(!File.Exists(Globals.DBCPath))
                {
                    Console.WriteLine($"Unable to locate dbc.MPQ at path {Globals.DBCPath}, please check Config.txt and set a proper installation path.");
                    return false;
                }

                // Clean up output directory if neccesary.
                if (Directory.Exists(OutputPath))
                    Directory.Delete(OutputPath, true);
                Directory.CreateDirectory(OutputPath);

                using (MpqArchive archive = new MpqArchive(Globals.DBCPath))
                {
                    archive.AddListfileFilenames();
                    foreach (var entry in archive)
                    {
                        if (!string.IsNullOrEmpty(entry.Filename))
                        {
                            var outputFileName = Path.Combine(OutputPath, Path.GetFileName(entry.Filename));

                            if (File.Exists(outputFileName))
                                File.Delete(outputFileName);

                            if (entry.Filename.Equals("DBFilesClient\\AreaTable.dbc"))
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
                                            Program.UpdateLoadingStatus();
                                        }

                                        streamOut.Close();
                                    }
                                }

                                Console.WriteLine($"Extracted DBC file {entry.Filename}");
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
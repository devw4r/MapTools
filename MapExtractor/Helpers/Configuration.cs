// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using AlphaCoreExtractor.Log;

namespace AlphaCoreExtractor.Helpers
{
    public static class Configuration
    {
        private static IniFile IniFile = new IniFile();
        public static bool Initialize()
        {
            try
            {
                IniFile.Load("Config.ini");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        public static string WoWPath
        {
            get
            {
                var path = IniFile["Configuration"]["WoWPath"].ToString();

                if (string.IsNullOrEmpty(path))
                    Logger.Error("Unable to read World of Warcraft installation path from Config.ini.");

                return path;
            }
        }


        public static int ZResolution
        {
            get
            {
                var zResolution = IniFile["Configuration"]["ZResolution"].ToString();

                if (string.IsNullOrEmpty(zResolution))
                    Logger.Error("Unable to read ZResolution from Config.ini.");
                else if (int.TryParse(zResolution, out int zRes))
                    return zRes;
                else
                    Logger.Error("Unable to parse ZResolution from Config.ini.");

                return 256;
            }
        }
    }
}

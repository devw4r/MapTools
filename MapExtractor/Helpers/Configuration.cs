// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers.Enums;

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

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
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

        public static bool AllParseDisabled
        {
            get
            {
                return GenerateMaps == GenerateMaps.Disabled && GenerateNavs == GenerateNavs.Disabled && GenerateObjs == GenerateObjs.Disabled;
            }
        }

        public static bool ShouldParseWMOs
        {
            get
            {
                return GenerateNavs == GenerateNavs.Enabled || GenerateObjs == GenerateObjs.Enabled && ParseSelection > ParseSelection.Terrain;
            }
        }

        public static bool ShouldParseMDXs
        {
            get
            {
                return GenerateNavs == GenerateNavs.Enabled || GenerateObjs == GenerateObjs.Enabled && ParseSelection > ParseSelection.TerrainWMO;
            }
        }

        public static GenerateObjs GenerateObjs
        {
            get
            {
                var selection = IniFile["Configuration"]["GenerateObjs"].ToString();
                if (string.IsNullOrEmpty(selection))
                    Logger.Error("Unable to read GenerateObjs configuration from Config.ini.");
                return (GenerateObjs)Convert.ToInt32(selection);
            }
        }

        public static GenerateNavs GenerateNavs
        {
            get
            {
                var selection = IniFile["Configuration"]["GenerateNavs"].ToString();
                if (string.IsNullOrEmpty(selection))
                    Logger.Error("Unable to read GenerateNavs configuration from Config.ini.");
                return (GenerateNavs)Convert.ToInt32(selection);
            }
        }

        public static GenerateMaps GenerateMaps
        {
            get
            {
                var selection = IniFile["Configuration"]["GenerateMaps"].ToString();
                if (string.IsNullOrEmpty(selection))
                    Logger.Error("Unable to read GenerateMaps configuration from Config.ini.");
                return (GenerateMaps)Convert.ToInt32(selection);
            }
        }

        public static ParseSelection ParseSelection
        {
            get
            {
                var selection = IniFile["Configuration"]["ParseSelection"].ToString();
                if (string.IsNullOrEmpty(selection))
                    Logger.Error("Unable to read ParseSelection configuration from Config.ini.");
                return (ParseSelection)Convert.ToInt32(selection);
            }
        }

        public static bool GenerateNoCollision
        {
            get
            {
                var selection = IniFile["Configuration"]["GenNoCollision"].ToString();
                if (string.IsNullOrEmpty(selection))
                    Logger.Error("Unable to read GenNoCollision configuration from Config.ini.");
                return Convert.ToBoolean(Convert.ToInt32(selection));
            }
        }

        public static bool GenerateMdxPlacement
        {
            get
            {
                var selection = IniFile["Configuration"]["GenerateMdxPlacement"].ToString();
                if (string.IsNullOrEmpty(selection))
                    Logger.Error("Unable to read GenerateMdxPlacement configuration from Config.ini.");
                return Convert.ToBoolean(Convert.ToInt32(selection));
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
                    Logger.Warning("Unable to parse ZResolution from Config.ini, using default value of 256.");

                return 256;
            }
        }
    }
}

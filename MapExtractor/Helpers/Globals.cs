// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Linq;

namespace AlphaCoreExtractor.Helpers
{
    public static class Globals
    {
        public static bool Verbose = false;

        private static string _cacheDBCPath = string.Empty;
        public static string DBCPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheDBCPath))
                    return _cacheDBCPath;
                _cacheDBCPath = Path.Combine(WoWRootPath, @"Data\dbc.MPQ");
                return _cacheDBCPath;
            }
        }

        private static string _cacheMapPath = string.Empty;
        public static string MapsPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheMapPath))
                    return _cacheMapPath;
                _cacheMapPath = Path.Combine(WoWRootPath, @"Data\World\Maps\");
                return _cacheMapPath;
            }
        }

        private static string _cachedWowPath = string.Empty;
        public static string WoWRootPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cachedWowPath))
                    return _cachedWowPath;
                _cachedWowPath = File.ReadLines(@"Config.txt").First();
                return _cachedWowPath;
            }
        }      
    }
}

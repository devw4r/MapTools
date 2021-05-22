// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Linq;
using System.Reflection;

namespace AlphaCoreExtractor.Helpers
{
    public static class Paths
    {
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

        private static string _cacheInputMapPath = string.Empty;
        public static string InputMapsPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheInputMapPath))
                    return _cacheInputMapPath;
                _cacheInputMapPath = Path.Combine(WoWRootPath, @"Data\World\Maps\");
                return _cacheInputMapPath;
            }
        }

        private static string _cacheOutputMapPath = string.Empty;
        public static string OutputMapsPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheOutputMapPath))
                    return _cacheOutputMapPath;
                _cacheOutputMapPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"maps\");

                if (!Directory.Exists(_cacheOutputMapPath))
                    Directory.CreateDirectory(_cacheOutputMapPath);

                return _cacheOutputMapPath;
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

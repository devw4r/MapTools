// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AlphaCoreExtractor.Helpers
{
    public static class Paths
    {
        private static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        private static string _cacheDBCPath = string.Empty;
        public static string DBCPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheDBCPath))
                    return _cacheDBCPath;
                _cacheDBCPath = Paths.Combine(WoWRootPath, @"Data\dbc.MPQ");

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
                _cacheInputMapPath = Paths.Combine(WoWRootPath, @"Data\World\Maps\");

                return _cacheInputMapPath;
            }
        }

        private static string _cacheOutputMapPath = string.Empty;
        public static string OutputMapsPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheOutputMapPath))
                {
                    if (!Directory.Exists(_cacheOutputMapPath))
                        Directory.CreateDirectory(_cacheOutputMapPath);

                    return _cacheOutputMapPath;
                }

                _cacheOutputMapPath = Paths.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"maps\");

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

                if (IsLinux)
                    _cachedWowPath.Replace(@"\", "/");

                return _cachedWowPath;
            }
        }

        public static string Combine(string path1, string path2)
        {
            var path = Path.Combine(path1, path2);

            if (IsLinux)
                path = path.Replace(@"\", "/");

            return path;
        }
    }
}

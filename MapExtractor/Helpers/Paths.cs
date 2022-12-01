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
        private static string _cacheCurrentAssemblyLocation = string.Empty;
        public static string CurrentAssemlyLocation
        {
            get
            {
                if(_cacheCurrentAssemblyLocation == string.Empty)
                    _cacheCurrentAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (Configuration.IsLinux)
                    _cacheCurrentAssemblyLocation = _cacheCurrentAssemblyLocation.Replace(@"\", "/");

                return _cacheCurrentAssemblyLocation;
            }
        }

        private static string _cacheDBCLoadPath = string.Empty;
        public static string DBCLoadPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheDBCLoadPath))
                {
                    if (!Directory.Exists(_cacheDBCLoadPath))
                        Directory.CreateDirectory(_cacheDBCLoadPath);

                    return _cacheDBCLoadPath;
                }

                _cacheDBCLoadPath = Paths.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"dbc");
                if (Configuration.IsLinux)
                    _cacheDBCLoadPath = _cacheDBCLoadPath.Replace(@"\", "/");

                if (!Directory.Exists(_cacheDBCLoadPath))
                    Directory.CreateDirectory(_cacheDBCLoadPath);

                return _cacheDBCLoadPath;
            }
        }

        private static string _cacheDBCMPQPath = string.Empty;
        public static string DBCMPQPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheDBCMPQPath))
                    return _cacheDBCMPQPath;
                _cacheDBCMPQPath = Paths.Combine(Paths.Combine(WoWRootPath, "Data"), "dbc.MPQ");
                if (Configuration.IsLinux)
                    _cacheDBCMPQPath = _cacheDBCMPQPath.Replace(@"\", "/");

                return _cacheDBCMPQPath;
            }
        }

        private static string _cachedWowPath = string.Empty;
        public static string WoWRootPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cachedWowPath))
                    return _cachedWowPath;

                _cachedWowPath = Configuration.WoWPath;

                if (Configuration.IsLinux)
                    _cachedWowPath = _cachedWowPath.Replace(@"\", "/");

                return _cachedWowPath;
            }
        }

        private static string _linuxExtractorPath = string.Empty;
        public static string LinuxExtractorPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_linuxExtractorPath))
                    return _linuxExtractorPath;

                _linuxExtractorPath = Paths.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"recast\") + "./AlphaCoreRecastLinux";
                if(Configuration.IsLinux)
                    _linuxExtractorPath = _linuxExtractorPath.Replace(@"\", "/");

                return _linuxExtractorPath;
            }
        }

        private static string _cacheWDTPath = string.Empty;
        public static string WDTLoadPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheWDTPath))
                {
                    if (!Directory.Exists(_cacheWDTPath))
                        Directory.CreateDirectory(_cacheWDTPath);

                    return _cacheWDTPath;
                }

                _cacheWDTPath = Paths.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"wdt\");
                if (Configuration.IsLinux)
                    _cacheWDTPath = _cacheWDTPath.Replace(@"\", "/");

                if (!Directory.Exists(_cacheWDTPath))
                    Directory.CreateDirectory(_cacheWDTPath);

                return _cacheWDTPath;
            }
        }

        /// <summary>
        /// Converts an internal WMO path to our extractor folder structure path.
        /// </summary>
        public static string GetWMOPath(string filename)
        {
            var separator = Configuration.IsLinux ? '/' : '\\';
            var directories = filename.Split(separator).ToList();
            var wmoIdx = directories.IndexOf("wmo");
            var rootPath = WMOLoadPath;

            foreach (var d in directories.Skip(wmoIdx).Take(directories.Count - (wmoIdx + 1)))
            {
                rootPath = Paths.Transform(Path.Combine(rootPath, d.ToLower()));
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
            }

            var path = Path.Combine(rootPath, Path.GetFileName(filename).ToLower());
            if (Configuration.IsLinux)
                path = path.Replace(@"\", "/");

            return path;
        }

        /// <summary>
        /// Converts an internal MDX path to our extractor folder structure path.
        /// </summary>
        public static string GetModelPath(string filename)
        {
            var separator = Configuration.IsLinux ? '/' : '\\';
            var directories = filename.Split(separator).ToList();
            var rootPath = ModelsLoadPath;
            foreach (var d in directories.Take(directories.Count - 1))
            {
                rootPath = Paths.Transform(Path.Combine(rootPath, d.ToLower()));
                if(!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
            }

            var path = Path.Combine(rootPath, Path.GetFileName(filename).ToLower());
            if (Configuration.IsLinux)
                path = path.Replace(@"\", "/");

            return path;
        }

        private static string _cacheWMOLoadPath = string.Empty;
        public static string WMOLoadPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheWMOLoadPath))
                {
                    if (!Directory.Exists(_cacheWMOLoadPath))
                        Directory.CreateDirectory(_cacheWMOLoadPath);

                    return _cacheWMOLoadPath;
                }

                _cacheWMOLoadPath = Paths.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"world");
                if (Configuration.IsLinux)
                    _cacheWMOLoadPath = _cacheWMOLoadPath.Replace(@"\", "/");

                if (!Directory.Exists(_cacheWMOLoadPath))
                    Directory.CreateDirectory(_cacheWMOLoadPath);

                return _cacheWMOLoadPath;
            }
        }

        private static string _cacheModelsLoadPath = string.Empty;
        public static string ModelsLoadPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheModelsLoadPath))
                {
                    if (!Directory.Exists(_cacheModelsLoadPath))
                        Directory.CreateDirectory(_cacheModelsLoadPath);

                    return _cacheModelsLoadPath;
                }

                _cacheModelsLoadPath = Paths.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"models");
                if (Configuration.IsLinux)
                    _cacheModelsLoadPath = _cacheModelsLoadPath.Replace(@"\", "/");

                if (!Directory.Exists(_cacheModelsLoadPath))
                    Directory.CreateDirectory(_cacheModelsLoadPath);

                return _cacheModelsLoadPath;
            }
        }

        private static string _cacheModelsPath = string.Empty;
        public static string ModelsPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheModelsPath))
                    return _cacheModelsPath;
                _cacheModelsPath = Paths.Combine(Paths.Combine(WoWRootPath, "Data"), "model.MPQ");
                if (Configuration.IsLinux)
                    _cacheModelsPath = _cacheModelsPath.Replace(@"\", "/");

                return _cacheModelsPath;
            }
        }

        private static string _cacheInputWMOPath = string.Empty;
        public static string InputWMOPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheInputWMOPath))
                    return _cacheInputWMOPath;
                _cacheInputWMOPath = Paths.Combine(WoWRootPath, @"Data\World\wmo\");
                if (Configuration.IsLinux)
                    _cacheInputWMOPath = _cacheInputWMOPath.Replace(@"\", "/");

                return _cacheInputWMOPath;
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
                if (Configuration.IsLinux)
                    _cacheInputMapPath = _cacheInputMapPath.Replace(@"\", "/");

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
                if (Configuration.IsLinux)
                    _cacheOutputMapPath = _cacheOutputMapPath.Replace(@"\", "/");

                if (!Directory.Exists(_cacheOutputMapPath))
                    Directory.CreateDirectory(_cacheOutputMapPath);

                return _cacheOutputMapPath;
            }
        }

        private static string _cacheOutputNavPath = string.Empty;
        public static string OutputNavPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheOutputNavPath))
                {
                    if (!Directory.Exists(_cacheOutputNavPath))
                        Directory.CreateDirectory(_cacheOutputNavPath);

                    return _cacheOutputNavPath;
                }

                _cacheOutputNavPath = Paths.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"nav\");
                if (Configuration.IsLinux)
                    _cacheOutputNavPath = _cacheOutputNavPath.Replace(@"\", "/");

                if (!Directory.Exists(_cacheOutputNavPath))
                    Directory.CreateDirectory(_cacheOutputNavPath);

                return _cacheOutputNavPath;
            }
        }

        private static string _cacheOutputGeomPath = string.Empty;
        public static string OutputGeomPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheOutputGeomPath))
                {
                    if (!Directory.Exists(_cacheOutputGeomPath))
                        Directory.CreateDirectory(_cacheOutputGeomPath);

                    return _cacheOutputGeomPath;
                }

                _cacheOutputGeomPath = Paths.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"geom\");
                if (Configuration.IsLinux)
                    _cacheOutputGeomPath = _cacheOutputGeomPath.Replace(@"\", "/");

                if (!Directory.Exists(_cacheOutputGeomPath))
                    Directory.CreateDirectory(_cacheOutputGeomPath);

                return _cacheOutputGeomPath;
            }
        }

        public static string Transform(string filename)
        {
            string outPath = filename;
            if (Configuration.IsLinux)
                outPath = outPath.Replace(@"\", "/");
            return outPath;
        }

        public static string Combine(string path1, string path2)
        {
            var path = Path.Combine(path1, path2);

            if (Configuration.IsLinux)
                path = path.Replace(@"\", "/");

            return path;
        }
    }
}

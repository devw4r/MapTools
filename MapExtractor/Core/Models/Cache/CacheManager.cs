// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System;
using System.IO;
using System.Collections.Generic;

using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using System.Linq;

namespace AlphaCoreExtractor.Core.Models.Cache
{
    public static class CacheManager
    {
        private static Dictionary<string, GeometryHolder> MDXCollisionCache = new Dictionary<string, GeometryHolder>();
        private static HashSet<string> NoCollisionMDX = new HashSet<string>();
        private static bool Initialized = false;
        public static bool ShouldLoad(string filePath)
        {
            CheckInitialize();
            var key = Path.GetFileNameWithoutExtension(filePath).ToLower();
            return !NoCollisionMDX.Contains(key);
        }

        public static void PushNoCollision(string filePath)
        {
            CheckInitialize();
            var key = Path.GetFileNameWithoutExtension(filePath).ToLower();
            if (!NoCollisionMDX.Contains(key))
            {
                NoCollisionMDX.Add(key);
                try
                {
                    // We use this list to blacklist mdx models before attempting to read them.
                    if (Configuration.GenerateNoCollision)
                        using (StreamWriter sw = new StreamWriter("NoCollision.cache", true))
                            sw.WriteLine(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }
            }
        }

        public static void AddMDXCollisionCache(string filePath, GeometryHolder geom)
        {
            var key = Path.GetFileNameWithoutExtension(filePath).ToLower();
            if(!MDXCollisionCache.ContainsKey(key))
            {
                MDXCollisionCache.Add(key, geom);
            }
        }

        public static bool GetMDXCollisionCache(string filePath, out GeometryHolder geom)
        {
            geom = null;
            var key = Path.GetFileNameWithoutExtension(filePath).ToLower();
            if (MDXCollisionCache.ContainsKey(key))
            {
                geom = MDXCollisionCache[key];
                return true;
            }
            return false;
        }

        private static void CheckInitialize()
        {
            // If we have collision cache file, load it.
            if (!Initialized && File.Exists("NoCollision.cache"))
            {
                var lines = File.ReadAllLines("NoCollision.cache");
                NoCollisionMDX = new HashSet<string>(lines.Select(l => Path.GetFileNameWithoutExtension(l).ToLower()));
            }

            Initialized = true;
        }
    }
}

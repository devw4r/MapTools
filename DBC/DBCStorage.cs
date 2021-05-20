// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Linq;
using System.Collections.Concurrent;

using AlphaCoreExtractor.DBC.Reader;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor.DBC
{
    public static class DBCStorage
    {
        private static ConcurrentDictionary<uint, AreaTable> AreaTable;
        public static bool Initialize()
        {
            try
            {
                AreaTable = DBCReader.Read<uint, AreaTable>("AreaTable.dbc", "ID");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public static bool TryGetByID(uint id, out AreaTable areaTable)
        {
            if (AreaTable.TryGetValue(id, out areaTable))
                return true;
            return false;
        }

        public static bool TryGetByAreaNumber(uint areaNumber, out AreaTable areaTable)
        {
            areaTable = null;
            try
            {
                areaTable = AreaTable.Values.First(v => v.AreaNumber == areaNumber);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public static bool TryGetByParentAreaNumber(uint areaNumber, out AreaTable areaTable)
        {
            areaTable = null;
            try
            {
                areaTable = AreaTable.Values.First(v => v.ParentAreaNum == areaNumber);
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

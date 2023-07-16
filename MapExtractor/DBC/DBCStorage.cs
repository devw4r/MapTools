// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.DBC.Reader;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor.DBC
{
    public static class DBCStorage
    {
        private static Dictionary<uint, AreaTable> AreaTable = new Dictionary<uint, AreaTable>();
        private static Dictionary<uint, Dictionary<uint, AreaTable>> MappedAreaTables = new Dictionary<uint, Dictionary<uint, AreaTable>>();
        private static ConcurrentDictionary<uint, DBCMap> Maps;

        public static bool Initialize()
        {
            try
            {
                var areatable_alpha = DBCReader.Read<uint, AreaTable_Alpha>(Paths.Combine(Paths.DBCLoadPath, "AreaTable.dbc"), "ID");
                var areatable_vanilla = DBCReader.Read<uint, AreaTable_Vanilla>("vanilla.alc", "ID");
                Maps = DBCReader.Read<uint, DBCMap>(Paths.Combine(Paths.DBCLoadPath, "Map.dbc"), "ID");

                MergeAreaTables(areatable_alpha, areatable_vanilla);
                areatable_alpha.Clear();
                areatable_vanilla.Clear();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private static void MergeAreaTables(ConcurrentDictionary<uint, AreaTable_Alpha> alpha, ConcurrentDictionary<uint, AreaTable_Vanilla> vanilla)
        {
            uint explore_bit = 0;
            foreach (var entry in alpha)
            {
                if (vanilla.ContainsKey(entry.Key))
                {
                    AreaTable.Add(entry.Value.ID, new AreaTable(alpha[entry.Key], vanilla[entry.Key], explore_bit++));

                    if (!MappedAreaTables.ContainsKey(alpha[entry.Key].ContinentID))
                        MappedAreaTables.Add(alpha[entry.Key].ContinentID, new Dictionary<uint, AreaTable>());

                    if (!MappedAreaTables[alpha[entry.Key].ContinentID].ContainsKey(alpha[entry.Key].AreaNumber))
                        MappedAreaTables[alpha[entry.Key].ContinentID].Add(alpha[entry.Key].AreaNumber, AreaTable[entry.Value.ID]);
                }
            }
        }

        public static Dictionary<uint, DBCMap> GetMaps()
        {
            return Maps.ToDictionary(k => k.Key, v => v.Value);
        }

        public static bool TryGetMapByName(string name, out DBCMap map)
        {
            map = null;
            try
            {
                map = Maps.Values.First(v => (v.MapName_enUS.ToLower().Replace("instance", "").Trim()).Equals(name.ToLower()) || v.Directory.ToLower().Equals(name.ToLower()));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        public static bool TryGetAreaByMapIdAndAreaNumber(uint mapID, uint areaNumber, out AreaTable areaTable)
        {
            areaTable = null;

            if(MappedAreaTables.ContainsKey(mapID) && MappedAreaTables[mapID].ContainsKey(areaNumber))
                areaTable = MappedAreaTables[mapID][areaNumber];

            return areaTable != null;
        }

        public static bool TryGetAreaByParentAreaNumber(uint areaNumber, out AreaTable areaTable)
        {
            areaTable = null;
            try
            {
                areaTable = AreaTable.Values.First(v => v.ParentAreaNum == areaNumber);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }
    }
}

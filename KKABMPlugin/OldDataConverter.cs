using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace KKABMX.Core
{
    internal static class OldDataConverter
    {
        private const string ExtDataBoneDataKey = "boneData";

        public static List<BoneModifier> MigrateOldExtData(PluginData pluginData)
        {
            if (pluginData == null) return null;
            if (!pluginData.data.TryGetValue(ExtDataBoneDataKey, out var value)) return null;
            if (!(value is string textData)) return null;

            return MigrateOldStringData(textData);
        }

        public static List<BoneModifier> MigrateOldStringData(string textData)
        {
            if (string.IsNullOrEmpty(textData)) return null;
            return DeserializeToModifiers(textData.Split());
        }

        private static List<BoneModifier> DeserializeToModifiers(IEnumerable<string> lines)
        {
            string GetTrimmedName(string[] splitValues)
            {
                // Turn cf_d_sk_top__1 into cf_d_sk_top 
                var boneName = splitValues[1];
                return boneName[boneName.Length - 2] == '_' && boneName[boneName.Length - 3] == '_'
                    ? boneName.Substring(0, boneName.Length - 3)
                    : boneName;
            }

            var query = from lineText in lines
                        let trimmedText = lineText?.Trim()
                        where !string.IsNullOrEmpty(trimmedText)
                        let splitValues = trimmedText.Split(',')
                        where splitValues.Length >= 6
                        group splitValues by GetTrimmedName(splitValues);

            var results = new List<BoneModifier>();

            foreach (var groupedBoneDataEntries in query)
            {
                var groupedOrderedEntries = groupedBoneDataEntries.OrderBy(x => x[1]).ToList();

                var coordinateModifiers = new List<BoneModifierData>(groupedOrderedEntries.Count);

                foreach (var singleEntry in groupedOrderedEntries)
                {
                    try
                    {
                        //var boneName = singleEntry[1];
                        //var isEnabled = bool.Parse(singleEntry[2]);
                        var x = float.Parse(singleEntry[3]);
                        var y = float.Parse(singleEntry[4]);
                        var z = float.Parse(singleEntry[5]);

                        var lenMod = singleEntry.Length > 6 ? float.Parse(singleEntry[6]) : 1f;

                        coordinateModifiers.Add(new BoneModifierData(new Vector3(x, y, z), lenMod));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, $"[KKABMX] Failed to load legacy line \"{string.Join(",", singleEntry)}\" - {ex.Message}");
                        Logger.Log(LogLevel.Debug, "Error details: " + ex);
                    }
                }

                if (coordinateModifiers.Count == 0)
                    continue;

                if (coordinateModifiers.Count > BoneModifier.CoordinateCount)
                    coordinateModifiers.RemoveRange(0, coordinateModifiers.Count - BoneModifier.CoordinateCount);
                if (coordinateModifiers.Count > 1 && coordinateModifiers.Count < BoneModifier.CoordinateCount)
                    coordinateModifiers.RemoveRange(0, coordinateModifiers.Count - 1);

                results.Add(new BoneModifier(groupedBoneDataEntries.Key, coordinateModifiers.ToArray()));
            }

            return results;
        }
    }
}

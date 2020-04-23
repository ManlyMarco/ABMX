using System.Collections.Generic;
using System.IO;
using System.Linq;
using Character;
using UnityEngine;
using MessagePack;

namespace KKABMX.Core
{
    internal static class OldDataConverter
    {
        public static List<BoneModifier> ImportOldData(string cardPath, string charaName, SEX sex)
        {
            Dictionary<int, BoneModHarmony.BoneModifier> modifiers;

            string path = null;
            if (!string.IsNullOrEmpty(cardPath))
                path = cardPath.Substring(0, cardPath.Length - 4) + ".bonemod";
            if (!File.Exists(path))
                path = BoneModHarmony.Prefs.GetCharaPathMsgPack(charaName, sex);
            if (File.Exists(path))
            {
                KKABMX_Core.Logger.LogInfo("Importing BoneModHarmony data from " + path);
                modifiers = LZ4MessagePackSerializer.Deserialize<Dictionary<int, BoneModHarmony.BoneModifier>>(File.ReadAllBytes(path));
            }
            else
            {
                modifiers = BoneModHarmony.Prefs.LoadBoneModifiers(charaName, sex);
            }

            return modifiers?.Values.Select(ConvertBoneData).ToList();
        }

        private static BoneModifier ConvertBoneData(BoneModHarmony.BoneModifier oldModifier)
        {
            return new BoneModifier(oldModifier.boneName, new[]
            {
                new BoneModifierData(
                    oldModifier.isScale ? oldModifier.Scale : Vector3.one,
                    1,
                    oldModifier.isPosition ? oldModifier.Position : Vector3.zero,
                    oldModifier.isRotate ? oldModifier.Rotation : Vector3.zero)
            });
        }
    }
}

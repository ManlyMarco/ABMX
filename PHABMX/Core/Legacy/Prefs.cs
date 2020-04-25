using System;
using System.Collections.Generic;
using System.IO;
using Character;
using KKABMX.Core;
using UnityEngine;

namespace BoneModHarmony
{
    /// <summary>
    /// Code borrowed from BoneModHarmony
    /// </summary>
    internal static class Prefs
	{
		public static string GetCharaPathMsgPack(string charaName, SEX sex)
		{
			if (sex == SEX.MALE)
			{
				return Path.GetFullPath(Path.Combine(Application.dataPath, "..\\UserData\\Chara\\male\\" + charaName + ".bonemod"));
			}
			return Path.GetFullPath(Path.Combine(Application.dataPath, "..\\UserData\\Chara\\female\\" + charaName + ".bonemod"));
		}

		public static Dictionary<int, BoneModifier> LoadBoneModifiers(string charaName, SEX sex)
		{
			string path;
			if (sex == SEX.MALE)
			{
				path = defaultMaleBMPath;
				if (charaName != null || charaName != "")
				{
					string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..\\UserData\\Chara\\male\\" + charaName + ".png.bmm.txt"));
					if (File.Exists(fullPath))
					{
						path = fullPath;
					}
				}
			}
			else
			{
				path = defaultBMPath;
				if (charaName != null || charaName != "")
				{
					string fullPath2 = Path.GetFullPath(Path.Combine(Application.dataPath, "..\\UserData\\Chara\\female\\" + charaName + ".png.bmm.txt"));
					if (File.Exists(fullPath2))
					{
						path = fullPath2;
					}
				}
			}
			return _LoadBoneModifiers(path);
		}

		private static Dictionary<int, BoneModifier> _LoadBoneModifiers(string path)
		{
			if (!File.Exists(path))
			{
				return null;
			}

            KKABMX_Core.Logger.LogInfo("Importing legacy bonemod data from " + path);

			Dictionary<int, BoneModifier> dictionary = new Dictionary<int, BoneModifier>();
			using (StreamReader streamReader = File.OpenText(path))
			{
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					string[] array = text.Split(separator, StringSplitOptions.None);
					if (array != null && array.Length >= 5)
					{
						string text2 = array[1];
						Vector3 vector = new Vector3(float.Parse(array[2]), float.Parse(array[3]), float.Parse(array[4]));
						string a = array[0].ToLower();
						if (!(a == "s"))
						{
							if (!(a == "r"))
							{
								if (a == "p")
								{
									if (!dictionary.ContainsKey(Animator.StringToHash(text2)))
									{
										dictionary.Add(Animator.StringToHash(text2), new BoneModifier(text2));
									}
									dictionary[Animator.StringToHash(text2)].Position = vector;
									dictionary[Animator.StringToHash(text2)].isPosition = true;
								}
							}
							else
							{
								if (!dictionary.ContainsKey(Animator.StringToHash(text2)))
								{
									dictionary.Add(Animator.StringToHash(text2), new BoneModifier(text2));
								}
								dictionary[Animator.StringToHash(text2)].Rotation = vector;
								dictionary[Animator.StringToHash(text2)].isRotate = true;
							}
						}
						else
						{
							if (!dictionary.ContainsKey(Animator.StringToHash(text2)))
							{
								dictionary.Add(Animator.StringToHash(text2), new BoneModifier(text2));
							}
							dictionary[Animator.StringToHash(text2)].Scale = vector;
							dictionary[Animator.StringToHash(text2)].isScale = true;
						}
					}
				}
			}
			return dictionary;
		}

		private static readonly string[] separator = new string[]
		{
			","
		};

        private static readonly string defaultBMPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..\\UserData\\BoneModifiers.txt"));

        private static readonly string defaultMaleBMPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..\\UserData\\BoneModifiersMale.txt"));
	}
}

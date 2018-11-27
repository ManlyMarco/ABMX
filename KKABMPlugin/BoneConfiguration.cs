using System.Collections.Generic;

namespace KKABMX.Core
{
    /// <summary>
    /// Additional bone data and bone categorization info
    /// </summary>
    public static class BoneConfiguration
    {
        public static readonly HashSet<int> ScaleBodyBonesF = new HashSet<int>
        {
            0,
            1,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            2,
            20,
            21,
            22,
            23,
            24,
            25,
            26,
            27,
            28,
            29,
            3,
            30,
            31,
            32,
            33,
            34,
            35,
            36,
            37,
            4,
            40,
            41,
            42,
            43,
            44,
            45,
            46,
            47,
            48,
            49,
            5,
            50,
            51,
            52,
            54,
            55,
            56,
            57,
            58,
            59,
            6,
            60,
            61,
            62,
            63,
            64,
            65,
            66,
            67,
            68,
            69,
            7,
            70,
            71,
            72,
            73,
            74,
            75,
            76,
            77,
            8,
            9
        };

        public static readonly HashSet<int> ScaleBodyBonesM = new HashSet<int>();

        public static readonly HashSet<int> ScaleFaceBonesF = new HashSet<int>
        {
            0,
            10,
            11,
            12,
            22,
            23,
            3,
            47,
            48,
            49,
            5,
            50,
            6,
            7,
            8,
            9
        };

        public static readonly HashSet<int> ScaleFaceBonesM = new HashSet<int>();

        /// <summary>
        /// Extra bones to handle. Add extra bone names to handle before characters are created.
        /// </summary>
        public static readonly List<string> AdditionalBoneNames = new List<string>
        {
            "cf_j_shoulder_L",
            "cf_j_shoulder_R",
            "cf_j_arm00_L",
            "cf_j_arm00_R",
            "cf_j_forearm01_L",
            "cf_j_forearm01_R",
            "cf_j_hand_L",
            "cf_j_hand_R",
            "cf_j_waist01",
            "cf_j_waist02",
            "cf_j_thigh00_L",
            "cf_j_thigh00_R",
            "cf_j_leg01_L",
            "cf_j_leg01_R",
            "cf_j_leg03_L",
            "cf_j_leg03_R",
            "cf_j_foot_L",
            "cf_j_foot_R",
            "cf_j_ana",
            "cm_J_dan109_00",
            "cm_J_dan100_00",
            "cm_J_dan_f_L",
            "cm_J_dan_f_R",
            "cf_j_kokan",
            "cf_j_toes_L",
            "cf_j_toes_R",
            "cf_hit_head",
            "cf_j_index01_L" ,
            "cf_j_index02_L" ,
            "cf_j_index03_L" ,
            "cf_j_little01_L",
            "cf_j_little02_L",
            "cf_j_little03_L",
            "cf_j_middle01_L",
            "cf_j_middle02_L",
            "cf_j_middle03_L",
            "cf_j_ring01_L"  ,
            "cf_j_ring02_L"  ,
            "cf_j_ring03_L"  ,
            "cf_j_thumb01_L" ,
            "cf_j_thumb02_L" ,
            "cf_j_thumb03_L" ,
            "cf_j_index01_R" ,
            "cf_j_index02_R" ,
            "cf_j_index03_R" ,
            "cf_j_little01_R",
            "cf_j_little02_R",
            "cf_j_little03_R",
            "cf_j_middle01_R",
            "cf_j_middle02_R",
            "cf_j_middle03_R",
            "cf_j_ring01_R"  ,
            "cf_j_ring02_R"  ,
            "cf_j_ring03_R"  ,
            "cf_j_thumb01_R" ,
            "cf_j_thumb02_R" ,
            "cf_j_thumb03_R" ,
            "cf_hit_thigh01_L",
            "cf_hit_thigh02_L",
            "cf_hit_thigh01_R",
            "cf_hit_thigh02_R"
        };

        /// <summary>
        /// Bones that have separate values for each coordinate / uniform type
        /// </summary>
        public static readonly List<string> CoordinateBoneNames = new List<string>
        {
            "cf_d_sk_top",
            "cf_d_sk_00_00",
            "cf_d_sk_07_00",
            "cf_d_sk_06_00",
            "cf_d_sk_05_00",
            "cf_d_sk_04_00",
            "cf_d_sk_01_00",
            "cf_d_sk_02_00",
            "cf_d_sk_03_00"
        };
    }
}
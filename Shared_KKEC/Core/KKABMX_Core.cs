using System.Collections.Generic;
using BepInEx;

namespace KKABMX.Core
{
    public partial class KKABMX_Core : BaseUnityPlugin
    {
        // Bones that misbehave with rotation adjustments
        internal static HashSet<string> IssuesWithRotationBones = new HashSet<string>
        {
            "cf_t_hips",
            "cf_j_head",
            "k_f_legupL_00",
            "k_f_legupR_00",
            "k_f_leglowL_00",
            "k_f_leglowL_01",
            "k_f_leglowL_02",
            "k_f_leglowL_03",
            "k_f_leglowR_00",
            "k_f_leglowR_01",
            "k_f_leglowR_02",
            "k_f_leglowR_03",

            "k_f_armupL_00",
            "k_f_armupL_01",
            "k_f_armupL_02",
            "k_f_armupL_03",
            "k_f_armupR_00",
            "k_f_armupR_01",
            "k_f_armupR_02",
            "k_f_armupR_03",
            "k_f_handL_00",
            "k_f_handL_01",
            "k_f_handL_02",
            "k_f_handL_03",
            "k_f_handR_00",
            "k_f_handR_01",
            "k_f_handR_02",
            "k_f_handR_03",
        };

        // Bones that misbehave with scale adjustments
        internal static HashSet<string> IssuesWithScaleBones = new HashSet<string>
        {
            // All 3 are caused by ShapeHeadInfoFemale.Update recalculating xyz scale based on global/lossy scale
            "p_cf_head_bone",
            "cf_J_N_FaceRoot",
            "cf_J_FaceRoot",
            
            "k_f_armupL_00",
            "k_f_armupL_01",
            "k_f_armupL_02",
            "k_f_armupL_03",
            "k_f_armupR_00",
            "k_f_armupR_01",
            "k_f_armupR_02",
            "k_f_armupR_03",
            "k_f_handL_00",
            "k_f_handL_01",
            "k_f_handL_02",
            "k_f_handL_03",
            "k_f_handR_00",
            "k_f_handR_01",
            "k_f_handR_02",
            "k_f_handR_03",
        };
    }
}

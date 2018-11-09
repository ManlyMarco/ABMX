using System.Collections.Generic;

namespace KKABMX.GUI
{
    internal static class InterfaceData
    {
        // 6 Max per 1 empty page
        public static List<BoneMeta> Metadata { get; } = new List<BoneMeta>
        {
            //BoneMeta.Separator("00_FaceTop"   , "tglHead")                   ,
            new BoneMeta("cf_J_FaceBase"        , "Head Scale"                 , 0, 3f, "00_FaceTop"   , "tglHead"         , "")                 ,
            new BoneMeta("cf_s_head"            , "Head + Neck Scale"          , 0, 3f, "00_FaceTop"   , "tglHead"         , "")                 ,
            new BoneMeta("cf_J_FaceUp_ty"       , "Upper Head Scale"           , 0, 3f, "00_FaceTop"   , "tglHead"         , "")                 ,
            new BoneMeta("cf_J_FaceUp_tz"       , "Upper Front Head Scale"     , 0, 3f, "00_FaceTop"   , "tglHead"         , "")                 ,
            new BoneMeta("cf_J_FaceLow_sx"      , "Lower Head Scale 1"         , 0, 3f, "00_FaceTop"   , "tglHead"         , "")                 ,
            new BoneMeta("cf_J_FaceLow_tz"      , "Lower Head Scale 2"         , 0, 3f, "00_FaceTop"   , "tglHead"         , "")                 ,

            //BoneMeta.Separator("00_FaceTop"     , "tglAll")                    ,
            
            BoneMeta.Separator("00_FaceTop"     , "tglEar")                    ,
            BoneMeta.Separator("00_FaceTop"     , "tglEar")                    ,
            new BoneMeta("cf_J_EarBase_ry_L"    , "Ear Scale"                  , 0, 3f, "00_FaceTop"   , "tglEar"          , "cf_J_EarBase_ry_R"),
            new BoneMeta("cf_J_EarUp_L"         , "Upper Ear Scale"            , 0, 3f, "00_FaceTop"   , "tglEar"          , "cf_J_EarUp_R")     ,
            new BoneMeta("cf_J_EarLow_L"        , "Lower Ear Scale"            , 0, 3f, "00_FaceTop"   , "tglEar"          , "cf_J_EarLow_R")    ,

            BoneMeta.Separator("00_FaceTop"     , "tglChin")                   ,
            BoneMeta.Separator("00_FaceTop"     , "tglChin")                   ,
            new BoneMeta("cf_J_Chin_Base"       , "Jaw Scale"                  , 0, 3f, "00_FaceTop"   , "tglChin"         , "")                 ,
            new BoneMeta("cf_J_ChinLow"         , "Chin Scale"                 , 0, 3f, "00_FaceTop"   , "tglChin"         , "")                 ,
            new BoneMeta("cf_J_ChinTip_Base"    , "Chin Tip Scale"             , 0, 3f, "00_FaceTop"   , "tglChin"         , "")                 ,

            BoneMeta.Separator("00_FaceTop"     , "tglCheek")                  ,
            BoneMeta.Separator("00_FaceTop"     , "tglCheek")                  ,
            new BoneMeta("cf_J_CheekUpBase"     , "Cheek Scale"                , 0, 3f, "00_FaceTop"   , "tglCheek"        , "")                 ,
            new BoneMeta("cf_J_CheekUp2_L"      , "Cheekbone Scale"            , 0, 3f, "00_FaceTop"   , "tglCheek"        , "cf_J_CheekUp2_R")  ,
            new BoneMeta("cf_J_CheekUp_s_L"     , "Upper Cheek Scale"          , 0, 3f, "00_FaceTop"   , "tglCheek"        , "cf_J_CheekUp_s_R") ,
            new BoneMeta("cf_J_CheekLow_s_L"    , "Lower Cheek Scale"          , 0, 3f, "00_FaceTop"   , "tglCheek"        , "cf_J_CheekLow_s_R"),

            BoneMeta.Separator("00_FaceTop"     , "tglEyebrow")                ,
            BoneMeta.Separator("00_FaceTop"     , "tglEyebrow")                ,
            new BoneMeta("cf_J_Mayu_L"          , "Eyebrow Scale"              , 0, 3f, "00_FaceTop"   , "tglEyebrow"      , "cf_J_Mayu_R")      ,
            new BoneMeta("cf_J_MayuMid_s_L"     , "Inner Eyebrow Scale"        , 0, 3f, "00_FaceTop"   , "tglEyebrow"      , "cf_J_MayuMid_s_R") ,
            new BoneMeta("cf_J_MayuTip_s_L"     , "Outer Eyebrow Scale"        , 0, 3f, "00_FaceTop"   , "tglEyebrow"      , "cf_J_MayuTip_s_R") ,

            //BoneMeta.Separator("00_FaceTop"     , "tglEye01")                  ,
            //BoneMeta.Separator("00_FaceTop"     , "tglEye01")                  ,
            new BoneMeta("cf_J_Eye_tz"          , "Both Eyeballs Scale"        , 0, 3f, "00_FaceTop"   , "tglEyes 2"        , "")                 ,
            new BoneMeta("cf_J_Eye_rz_L"        , "Eyeball Scale 1"            , 0, 3f, "00_FaceTop"   , "tglEyes 2"        , "cf_J_Eye_rz_R")    ,
            new BoneMeta("cf_J_Eye_tx_L"        , "Eyeball Scale 2"            , 0, 3f, "00_FaceTop"   , "tglEyes 2"        , "cf_J_Eye_tx_R")    ,

            new BoneMeta("cf_J_Eye01_s_L"       , "Upper Eyelashes Scale 1"    , 0, 3f, "00_FaceTop"   , "tglUpper Eyelash", "cf_J_Eye01_s_R")   ,
            new BoneMeta("cf_J_Eye02_s_L"       , "Upper Eyelashes Scale 2"    , 0, 3f, "00_FaceTop"   , "tglUpper Eyelash", "cf_J_Eye02_s_R")   ,
            new BoneMeta("cf_J_Eye03_s_L"       , "Upper Eyelashes Scale 3"    , 0, 3f, "00_FaceTop"   , "tglUpper Eyelash", "cf_J_Eye03_s_R")   ,
            new BoneMeta("cf_J_Eye04_s_L"       , "Upper Eyelashes Scale 4"    , 0, 3f, "00_FaceTop"   , "tglUpper Eyelash", "cf_J_Eye04_s_R")   ,
            new BoneMeta("cf_J_Eye05_s_L"       , "Upper Eyelashes Scale 5"    , 0, 3f, "00_FaceTop"   , "tglUpper Eyelash", "cf_J_Eye05_s_R")   ,

            new BoneMeta("cf_J_Eye06_s_L"       , "Lower Eyelashes Scale 1"    , 0, 3f, "00_FaceTop"   , "tglLower Eyelash", "cf_J_Eye06_s_R")   ,
            new BoneMeta("cf_J_Eye07_s_L"       , "Lower Eyelashes Scale 2"    , 0, 3f, "00_FaceTop"   , "tglLower Eyelash", "cf_J_Eye07_s_R")   ,
            new BoneMeta("cf_J_Eye08_s_L"       , "Lower Eyelashes Scale 3"    , 0, 3f, "00_FaceTop"   , "tglLower Eyelash", "cf_J_Eye08_s_R")   ,

            BoneMeta.Separator("00_FaceTop"     , "tglNose")                   ,
            BoneMeta.Separator("00_FaceTop"     , "tglNose")                   ,
            new BoneMeta("cf_J_NoseBase_rx"     , "Nose Scale"                 , 0, 3f, "00_FaceTop"   , "tglNose"         , "")                 ,
            new BoneMeta("cf_J_Nose_tip"        , "Nose Tip Scale"             , 0, 3f, "00_FaceTop"   , "tglNose"         , "")                 ,
            new BoneMeta("cf_J_NoseBridge_rx"   , "Nose Bridge Scale 1"        , 0, 3f, "00_FaceTop"   , "tglNose"         , "")                 ,
            new BoneMeta("cf_J_NoseBridge_ty"   , "Nose Bridge Scale 2"        , 0, 3f, "00_FaceTop"   , "tglNose"         , "")                 ,
            new BoneMeta("cf_J_megane_rx_ear"   , "Glasses Accessory Scale"    , 0, 3f, "00_FaceTop"   , "tglNose"          ,"")                  , //This will scale any Accessories parented to glasses

            //BoneMeta.Separator("00_FaceTop"   , "tglMouth")                  ,

            new BoneMeta("cf_J_MouthBase_rx"    , "Mouth Scale 1"              , 0, 3f, "00_FaceTop"   , "tglMouth 2"      , "")                 ,
            new BoneMeta("cf_J_MouthBase_ty"    , "Mouth Scale 2"              , 0, 3f, "00_FaceTop"   , "tglMouth 2"      , "")                 ,
            new BoneMeta("cf_J_Mouth_L"         , "Mouth Side Scale"           , 0, 3f, "00_FaceTop"   , "tglMouth 2"      , "cf_J_Mouth_R")     ,
            new BoneMeta("cf_J_Mouthup"         , "Upper Lip Scale"            , 0, 3f, "00_FaceTop"   , "tglMouth 2"      , "")                 ,
            new BoneMeta("cf_J_MouthLow"        , "Lower Lip Scale"            , 0, 3f, "00_FaceTop"   , "tglMouth 2"      , "")                 ,

            BoneMeta.Separator("01_BodyTop"     , "tglAll")                    ,
            BoneMeta.Separator("01_BodyTop"     , "tglAll")                    ,
            new BoneMeta("cf_n_height"          , "Body Scale"                 , 0, 3f, "01_BodyTop"   , "tglAll"          , "")                 ,

            BoneMeta.Separator("01_BodyTop"     , "tglBreast")                 ,
            BoneMeta.Separator("01_BodyTop"     , "tglBreast")                 ,
            new BoneMeta("cf_hit_bust02_L"      , "Breast Collision Scale"     , 0, 3f, "01_BodyTop"   , "tglBreast"       , "cf_hit_bust02_R")  ,
            new BoneMeta("cf_d_bust01_L"        , "Breast Scale 1"             , 0, 3f, "01_BodyTop"   , "tglBreast"       , "cf_d_bust01_R")    ,

            new BoneMeta("cf_d_bust02_L"        , "Breast Scale 2"             , 0, 3f, "01_BodyTop"   , "tglChest 2"      , "cf_d_bust02_R")    ,
            new BoneMeta("cf_d_bust03_L"        , "Breast Scale 3"             , 0, 3f, "01_BodyTop"   , "tglChest 2"      , "cf_d_bust03_R")    ,
            new BoneMeta("cf_s_bust00_L"        , "Extra Breast Scale 1"       , 0, 3f, "01_BodyTop"   , "tglChest 2"      , "cf_s_bust00_R")    ,
            new BoneMeta("cf_s_bust01_L"        , "Extra Breast Scale 2"       , 0, 3f, "01_BodyTop"   , "tglChest 2"      , "cf_s_bust01_R")    ,
            new BoneMeta("cf_s_bust02_L"        , "Extra Breast Scale 3"       , 0, 3f, "01_BodyTop"   , "tglChest 2"      , "cf_s_bust02_R")    ,
            new BoneMeta("cf_s_bust03_L"        , "Extra Breast Scale 4"       , 0, 3f, "01_BodyTop"   , "tglChest 2"      , "cf_s_bust03_R")    ,

            new BoneMeta("cf_s_bnip01_L"        , "Areola Scale 1"             , 0, 3f, "01_BodyTop"   , "tglNipples"      , "cf_s_bnip01_R")    ,
            new BoneMeta("cf_s_bnip025_L"       , "Areola Scale 2"             , 0, 3f, "01_BodyTop"   , "tglNipples"      , "cf_s_bnip025_R")   ,
            new BoneMeta("cf_d_bnip01_L"        , "Nipple Scale 1"             , 0, 3f, "01_BodyTop"   , "tglNipples"      , "cf_d_bnip01_R")    ,
            new BoneMeta("cf_s_bnip015_L"       , "Nipple Scale 2"             , 0, 3f, "01_BodyTop"   , "tglNipples"      , "cf_s_bnip015_R")   , //This is the one that doesn't seem doing anything
            new BoneMeta("cf_s_bnip02_L"        , "Nipple Tip Scale"           , 0, 3f, "01_BodyTop"   , "tglNipples"      , "cf_s_bnip02_R")    ,
            new BoneMeta("cf_s_bnipacc_L"       , "Nipple Accessory Scale"     , 0, 3f, "01_BodyTop"   , "tglNipples"      , "cf_s_bnipacc_R")   ,

            BoneMeta.Separator("01_BodyTop"     , "tglUpper")                  ,
            BoneMeta.Separator("01_BodyTop"     , "tglUpper")                  ,
            new BoneMeta("cf_s_neck"            , "Neck Scale"                 , 0, 3f, "01_BodyTop"   , "tglUpper"        , "")                 ,
            new BoneMeta("cf_s_shoulder02_L"    , "Shoulder Scale"             , 0, 3f, "01_BodyTop"   , "tglUpper"        , "cf_s_shoulder02_R"),
            new BoneMeta("cf_hit_shoulder_L"    , "Shoulder Collision Scale"   , 0, 3f, "01_BodyTop"   , "tglUpper"        , "cf_hit_shoulder_R"),

            new BoneMeta("cf_s_spine01"         , "Lower Torso Scale 1"        , 0, 3f, "01_BodyTop"   , "tglUpper Body 2" , "")                 ,
            new BoneMeta("cf_s_spine02"         , "Lower Torso Scale 2"        , 0, 3f, "01_BodyTop"   , "tglUpper Body 2" , "")                 ,
            new BoneMeta("cf_s_spine03"         , "Lower Torso Scale 3"        , 0, 3f, "01_BodyTop"   , "tglUpper Body 2" , "")                 ,
            new BoneMeta("cf_hit_spine01"       , "Upper Torso Collision Scale", 0, 3f, "01_BodyTop"   , "tglUpper Body 2" , "")                 ,
            new BoneMeta("cf_hit_spine02_L"     , "Lower Torso Collision Scale", 0, 3f, "01_BodyTop"   , "tglUpper Body 2" , "")                 ,

            BoneMeta.Separator("01_BodyTop"     , "tglLower")                  ,
            BoneMeta.Separator("01_BodyTop"     , "tglLower")                  ,
            new BoneMeta("cf_d_kokan"           , "Genital Scale"              , 0, 3f, "01_BodyTop"   , "tglLower"        , "")                 ,
            new BoneMeta("cf_j_ana"             , "Anus Scale"                 , 0, 3f, "01_BodyTop"   , "tglLower"        , "")                 ,
            new BoneMeta("cf_hit_berry"         , "Belly Collision Scale"      , 0, 3f, "01_BodyTop"   , "tglLower"        , "")                 ,
            new BoneMeta("cf_hit_waist_L"       , "Waist Collision Scale"      , 0, 3f, "01_BodyTop"   , "tglLower"        , "")                 ,
            new BoneMeta("cf_hit_siri_L"        , "Ass Collision Scale"        , 0, 3f, "01_BodyTop"   , "tglLower"        , "cf_hit_siri_R")    ,

            BoneMeta.Separator("01_BodyTop"     , "tglLeg")                    ,
            BoneMeta.Separator("01_BodyTop"     , "tglLeg")                    ,
            new BoneMeta("cf_s_waist01"         , "Upper Waist Scale"          , 0, 3f, "01_BodyTop"   , "tglLeg"          , "")                 ,
            new BoneMeta("cf_s_waist02"         , "Lower Waist Scale"          , 0, 3f, "01_BodyTop"   , "tglLeg"          , "")                 ,
            new BoneMeta("cf_s_siri_L"          , "Ass Scale"                  , 0, 3f, "01_BodyTop"   , "tglLeg"          , "cf_s_siri_R")      ,
            new BoneMeta("cf_j_waist01"         , "Waist & Leg Scale 1"        , 0, 3f, "01_BodyTop"   , "tglLeg"          , "")                 ,
            new BoneMeta("cf_j_waist02"         , "Waist & Leg Scale 2"        , 0, 3f, "01_BodyTop"   , "tglLeg"          , "")                 ,

            //BoneMeta.Separator("01_BodyTop"   , "tglArm")                    ,

            new BoneMeta("cf_j_shoulder_L"      , "Shoulder & Arm Scale"       , 0, 3f, "01_BodyTop"   , "tglArms 2"       , "cf_j_shoulder_R")  ,
            new BoneMeta("cf_j_arm00_L"         , "Arm Scale"                  , 0, 3f, "01_BodyTop"   , "tglArms 2"       , "cf_j_arm00_R")     ,
            new BoneMeta("cf_s_arm01_L"         , "Upper Arm Thickness 1"      , 0, 3f, "01_BodyTop"   , "tglArms 2"       , "cf_s_arm01_R")     ,
            new BoneMeta("cf_s_arm02_L"         , "Upper Arm Thickness 2"      , 0, 3f, "01_BodyTop"   , "tglArms 2"       , "cf_s_arm02_R")     ,
            new BoneMeta("cf_s_arm03_L"         , "Upper Arm Thickness 3"      , 0, 3f, "01_BodyTop"   , "tglArms 2"       , "cf_s_arm03_R")     ,

            new BoneMeta("cf_j_forearm01_L"     , "Forearm Scale"              , 0, 3f, "01_BodyTop"   , "tglForearms"     , "cf_j_forearm01_R") ,
            new BoneMeta("cf_s_forearm01_L"     , "Forearm Thickness 1"        , 0, 3f, "01_BodyTop"   , "tglForearms"     , "cf_s_forearm01_R") ,
            new BoneMeta("cf_s_forearm02_L"     , "Forearm Thickness 2"        , 0, 3f, "01_BodyTop"   , "tglForearms"     , "cf_s_forearm02_R") ,

            new BoneMeta("cf_s_wrist_L"         , "Wrist Scale"                , 0, 3f, "01_BodyTop"   , "tglHands"        , "cf_s_wrist_R")     ,
            new BoneMeta("cf_j_hand_L"          , "Hand Scale 1"               , 0, 3f, "01_BodyTop"   , "tglHands"        , "cf_j_hand_R")      ,
            new BoneMeta("cf_s_hand_L"          , "Hand Scale 2"               , 0, 3f, "01_BodyTop"   , "tglHands"        , "cf_s_hand_R")      ,
            new BoneMeta("cf_hit_arm_L"         , "Hand Collision Scale"       , 0, 3f, "01_BodyTop"   , "tglHands"        , "cf_hit_arm_R")     ,

            //BoneMeta.Separator("01_BodyTop"   , "tglLeg")                    ,

            new BoneMeta("cf_j_thigh00_L"       , "Thigh & Leg Scale"          , 0, 3f, "01_BodyTop"   , "tglThighs"       , "cf_j_thigh00_R")   ,
            new BoneMeta("cf_s_thigh01_L"       , "Upper Thigh Scale"          , 0, 3f, "01_BodyTop"   , "tglThighs"       , "cf_s_thigh01_R")   ,
            new BoneMeta("cf_s_thigh02_L"       , "Middle Thigh Scale"         , 0, 3f, "01_BodyTop"   , "tglThighs"       , "cf_s_thigh02_R")   ,
            new BoneMeta("cf_s_thigh03_L"       , "Lower Thigh Scale"          , 0, 3f, "01_BodyTop"   , "tglThighs"       , "cf_s_thigh03_R")   ,

            new BoneMeta("cf_j_leg01_L"         , "Lower Leg Scale"            , 0, 3f, "01_BodyTop"   , "tglFeet & Calves", "cf_j_leg01_R")     ,
            new BoneMeta("cf_s_leg01_L"         , "Upper Calve Scale"          , 0, 3f, "01_BodyTop"   , "tglFeet & Calves", "cf_s_leg02_R")     ,
            new BoneMeta("cf_s_leg02_L"         , "Lower Calve Scale"          , 0, 3f, "01_BodyTop"   , "tglFeet & Calves", "cf_s_leg03_R")     ,
            new BoneMeta("cf_s_leg03_L"         , "Angkle Scale"               , 0, 3f, "01_BodyTop"   , "tglFeet & Calves", "cf_s_leg01_R")     ,
            new BoneMeta("cf_j_foot_L"          , "Foot Scale 1"               , 0, 3f, "01_BodyTop"   , "tglFeet & Calves", "cf_j_foot_R")      ,
            new BoneMeta("cf_j_leg03_L"         , "Foot Scale 2"               , 0, 3f, "01_BodyTop"   , "tglFeet & Calves", "cf_j_leg03_R")     ,

            //BoneMeta.Separator("03_ClothesTop", "tglBot")                    ,

            new BoneMeta("cf_d_sk_top"          , "Whole Skirt Scale"          , 0, 3f, "03_ClothesTop", "tglSkirt"        , "")                 ,
            new BoneMeta("cf_d_sk_00_00"        , "Skirt Front"                , 0, 3f, "03_ClothesTop", "tglSkirt"        , "")                 ,
            new BoneMeta("cf_d_sk_07_00"        , "Skirt Front Sides"          , 0, 3f, "03_ClothesTop", "tglSkirt"        , "cf_d_sk_01_00")    ,
            new BoneMeta("cf_d_sk_06_00"        , "Skirt Sides"                , 0, 3f, "03_ClothesTop", "tglSkirt"        , "cf_d_sk_02_00")    ,
            new BoneMeta("cf_d_sk_05_00"        , "Skirt Back Sides"           , 0, 3f, "03_ClothesTop", "tglSkirt"        , "cf_d_sk_03_00")    ,
            new BoneMeta("cf_d_sk_04_00"        , "Skirt Back"                 , 0, 3f, "03_ClothesTop", "tglSkirt"        , "")                 ,
        };
    }
}
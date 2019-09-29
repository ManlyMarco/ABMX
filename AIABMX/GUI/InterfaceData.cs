using System.Collections.Generic;
using KKAPI.Maker;

namespace KKABMX.GUI
{
    public static class InterfaceData
    {
        public static List<BoneMeta> BoneControls { get; }
        public static string[] FingerNamePrefixes { get; }

        static InterfaceData()
        {
            FingerNamePrefixes = new[]
            {
                "cf_J_Hand_Thumb",
                "cf_J_Hand_Index",
                "cf_J_Hand_Middle",
                "cf_J_Hand_Ring",
                "cf_J_Hand_Little"
            };
            BoneControls = new List<BoneMeta>
            {
                new BoneMeta("cf_J_FaceBase"        , "Head"              , 0   , 2f  , MakerConstants.Face.All      , "")                    {L = true          , LMax = 5f  , LDisplayName = "Head Position"}    ,
                new BoneMeta("cf_J_Head_s"          , "Head + Neck"       , 0   , 2f  , MakerConstants.Face.All      , "")                                       ,
                // Causes problems when stock slider "lower depth" is changed
                //new BoneMeta("cf_J_FaceLowBase"     , "Lower Head All"    , 0   , 2f  , MakerConstants.Face.All      , "")                                       ,
                new BoneMeta("cf_J_FaceLow_s"       , "Lower Head Cheek"  , 0   , 2f  , MakerConstants.Face.All      , "")                                       ,
                new BoneMeta("cf_J_FaceUp_ty"       , "Upper Head"        , 0   , 2f  , MakerConstants.Face.All      , "")                                       ,
                new BoneMeta("cf_J_FaceUp_tz"       , "Upper Front Head"  , 0   , 2f  , MakerConstants.Face.All      , "")                                       ,

                new BoneMeta("cf_J_EarBase_s_L"     , "Ear"               , 0   , 3f  , MakerConstants.Face.Ear      , "cf_J_EarBase_s_R")                       ,
                new BoneMeta("cf_J_EarUp_L"         , "Upper Ear"         , 0   , 4f  , MakerConstants.Face.Ear      , "cf_J_EarUp_R")                           ,
                new BoneMeta("cf_J_EarLow_L"        , "Lower Ear"         , 0   , 4f  , MakerConstants.Face.Ear      , "cf_J_EarLow_R")                          ,

                new BoneMeta("cf_J_Chin_rs"         , "Jaw"               , 0   , 3f  , MakerConstants.Face.Chin     , "")                    {L = true          , LMax = 2f  , LDisplayName = "Jaw Offset"}       ,
                new BoneMeta("cf_J_ChinLow"         , "Chin"              , 0   , 3f  , MakerConstants.Face.Chin     , "")                                       ,
                new BoneMeta("cf_J_ChinTip_s"       , "Chin Tip"          , 0   , 3f  , MakerConstants.Face.Chin     , "")                                       ,

                new BoneMeta("cf_J_CheekUp_L"       , "Upper Cheek"       , 0   , 2f  , MakerConstants.Face.Cheek    , "cf_J_CheekUp_R")                         ,
                new BoneMeta("cf_J_CheekLow_L"      , "Lower Cheek"       , 0   , 2f  ,  MakerConstants.Face.Cheek   , "cf_J_CheekLow_R")     {L = true          , LMax = 2.5f, LDisplayName = "Cheek Offset"}     ,

                new BoneMeta("cf_J_Mayu_L"          , "Eyebrow"           , 0   , 5f  , MakerConstants.Face.Eyebrow  , "cf_J_Mayu_R")                            ,
                new BoneMeta("cf_J_MayuMid_s_L"     , "Inner Eyebrow"     , 0   , 5f  , MakerConstants.Face.Eyebrow  , "cf_J_MayuMid_s_R")                       ,
                new BoneMeta("cf_J_MayuTip_s_L"     , "Outer Eyebrow"     , 0   , 5f  , MakerConstants.Face.Eyebrow  , "cf_J_MayuTip_s_R")                       ,

                new BoneMeta("cf_J_Eye_s_L"         , "Eye"               , 0   , 1.5f, MakerConstants.Face.Eyes     , "cf_J_Eye_s_R")                           ,

                new BoneMeta("cf_J_Eye01_s_L"       , "Inner Eyelashes"   , 0   , 3f  , MakerConstants.Face.Eyelashes, "cf_J_Eye01_s_R")                         ,
                new BoneMeta("cf_J_Eye02_s_L"       , "Upper Eyelashes"   , 0   , 3f  , MakerConstants.Face.Eyelashes, "cf_J_Eye02_s_R")                         ,
                new BoneMeta("cf_J_Eye03_s_L"       , "Outer Eyelashes"   , 0   , 3f  , MakerConstants.Face.Eyelashes, "cf_J_Eye03_s_R")                         ,
                new BoneMeta("cf_J_Eye04_s_L"       , "Lower Eyelashes"   , 0   , 3f  , MakerConstants.Face.Eyelashes, "cf_J_Eye04_s_R")                         ,

                new BoneMeta("cf_J_NoseBase_trs"    , "Nose + Bridge"     , 0   , 2f  , MakerConstants.Face.Nose     , "")                                       ,
                new BoneMeta("cf_J_NoseBase_s"      , "Nose"              , 0   , 2f  , MakerConstants.Face.Nose     , "")                                       ,
                new BoneMeta("cf_J_Nose_tip"        , "Nose Tip"          , 0   , 3f  , MakerConstants.Face.Nose     , "")                    {L = true          , LMax = 2.5f, LDisplayName = "Nose Tip Offset"}  ,
                new BoneMeta("cf_J_NoseBridge_t"    , "Nose Bridge"       , 0   , 2f  , MakerConstants.Face.Nose     , "")                                       ,

                new BoneMeta("cf_J_MouthBase_tr"    , "Mouth"             , 0   , 2f  , MakerConstants.Face.Mouth    , "")                                       ,
                new BoneMeta("cf_J_MouthMove"       , "Lips"              , 0   , 2f  , MakerConstants.Face.Mouth    , "")                                       ,
                new BoneMeta("cf_J_Mouth_L"         , "Mouth Side"        , 0   , 6f  , MakerConstants.Face.Mouth    , "cf_J_Mouth_R")                           ,
                new BoneMeta("cf_J_Mouthup"         , "Upper Lip"         , 0   , 6f  , MakerConstants.Face.Mouth    , "")                                       ,
                new BoneMeta("cf_J_MouthLow"        , "Lower Lip"         , 0   , 6f  , MakerConstants.Face.Mouth    , "")                                       ,

                new BoneMeta("cf_N_height"          , "Body"              , 0.5f, 1.5f, MakerConstants.Body.All      , "")                                       ,

                new BoneMeta("cf_J_Mune00_s_L"      , "Breast Scale 1"    , 0   , 3f  , MakerConstants.Body.Breast   , "cf_J_Mune00_s_R")     {XYZPostfix = null},
                new BoneMeta("cf_J_Mune01_s_L"      , "Breast Scale 2"    , 0   , 3f  , MakerConstants.Body.Breast   , "cf_J_Mune01_s_R")     {XYZPostfix = null},
                new BoneMeta("cf_J_Mune02_s_L"      , "Breast Scale 3"    , 0   , 3f  , MakerConstants.Body.Breast   , "cf_J_Mune02_s_R")     {XYZPostfix = null},
                new BoneMeta("cf_J_Mune03_s_L"      , "Breast Tip"        , 0   , 3f  , MakerConstants.Body.Breast   , "cf_J_Mune03_s_R")                        ,
                new BoneMeta("cf_J_Mune04_s_L"      , "Areola"            , 0   , 3f  , MakerConstants.Body.Breast   , "cf_J_Mune04_s_R")     {L = true          , LMax = 4f  , LDisplayName = "Areola Protrusion"},
                new BoneMeta("cf_J_Mune_Nip01_s_L"  , "Nipple"            , 0   , 3f  , MakerConstants.Body.Breast   , "cf_J_Mune_Nip01_s_R")                    ,
                new BoneMeta("cf_J_Mune_Nip02_s_L"  , "Nipple Tip"        , 0   , 3f  , MakerConstants.Body.Breast   , "cf_J_Mune_Nip02_s_R")                    ,
                new BoneMeta("cf_hit_Mune02_s_L"    , "Breast Collision"  , 0   , 3f  , MakerConstants.Body.Breast   , "cf_hit_Mune02_s_R")                      ,

                new BoneMeta("cf_J_Neck_s"          , "Neck"              , 0   , 3f  , MakerConstants.Body.Upper    , "")                                       ,
                new BoneMeta("cf_J_Shoulder02_s_L"  , "Shoulder"          , 0   , 3f  , MakerConstants.Body.Upper    , "cf_J_Shoulder02_s_R") {L = true          , LMax = 2f  , LDisplayName = "Shoulder Shape"}   ,

                new BoneMeta("cf_J_Spine03_s"       , "Upper Torso"       , 0   , 3f  , MakerConstants.Body.Upper    , "")                                       ,
                new BoneMeta("cf_J_Spine02_s"       , "Middle Torso"      , 0   , 3f  , MakerConstants.Body.Upper    , "")                                       ,
                new BoneMeta("cf_J_Spine01_s"       , "Lower Torso"       , 0   , 3f  , MakerConstants.Body.Upper    , "")                                       ,

                new BoneMeta("cf_J_Siri_s_L"        , "Ass"               , 0   , 3f  , MakerConstants.Body.Lower    , "cf_J_Siri_s_R")       {L = true          , LMax = 3f  , LDisplayName = "Ass Position"}     ,
                new BoneMeta("cf_hit_Siri_s_L"      , "Ass Collision"     , 0   , 3f  , MakerConstants.Body.Lower    , "cf_hit_Siri_s_R")                        ,

                new BoneMeta("cf_J_Kosi01_s"        , "Pelvis"            , 0   , 3f  , MakerConstants.Body.Lower    , "")                                       ,
                new BoneMeta("cf_J_Kosi02_s"        , "Hips"              , 0   , 3f  , MakerConstants.Body.Lower    , "")                                       ,
                new BoneMeta("cf_J_Kosi01"          , "Pelvis & Legs"     , 0   , 1.5f, MakerConstants.Body.Lower    , "")                    {Y = false}        ,
                new BoneMeta("cf_J_Kosi02"          , "Hips & Legs"       , 0   , 1.5f, MakerConstants.Body.Lower    , "")                    {Y = false}        ,

                new BoneMeta("cf_J_ArmUp00_L"       , "Whole Arm"         , 0   , 1.5f, MakerConstants.Body.Arm      , "cf_J_ArmUp00_R")      {L = true          , LMax = 2f  , LDisplayName = "Arm Offset"}       ,
                new BoneMeta("cf_J_ArmUp01_s_L"     , "Upper Arm Deltoids", 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_ArmUp01_s_R")    {Y = false}        ,
                new BoneMeta("cf_J_ArmUp02_s_L"     , "Upper Arm Triceps" , 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_ArmUp02_s_R")    {Y = false}        ,
                new BoneMeta("cf_J_ArmUp03_s_L"     , "Upper Arm Lower"   , 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_ArmUp03_s_R")    {Y = false}        ,
                new BoneMeta("cf_J_ArmElboura_s_L"  , "Elbow"             , 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_ArmElboura_s_R") {Y = false}        ,
                new BoneMeta("cf_J_ArmElbo_low_s_L" , "Elbow Cap"         , 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_ArmElbo_low_s_R"){Y = false}        ,

                new BoneMeta("cf_J_ArmLow01_s_L"    , "Forearm Upper"     , 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_ArmLow01_s_R")   {Y = false}        ,
                new BoneMeta("cf_J_ArmLow02_s_L"    , "Forearm Lower"     , 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_ArmLow02_s_R")   {Y = false}        ,

                new BoneMeta("cf_J_Hand_Wrist_s_L"  , "Wrist"             , 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_Hand_Wrist_s_R") {Y = false}        ,
                new BoneMeta("cf_J_Hand_s_L"        , "Hand"              , 0   , 3f  , MakerConstants.Body.Arm      , "cf_J_Hand_s_R")                          ,

                new BoneMeta("cf_J_LegUp00_L"       , "Whole Leg"         , 0   , 1.5f, MakerConstants.Body.Leg      , "cf_J_LegUp00_R")      {L = true          , LMax = 2f  , LDisplayName = "Leg Offset"}       ,
                new BoneMeta("cf_J_LegUpDam_s_L"    , "Outer Upper Thigh" , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegUpDam_s_R")   {Y = false}        ,
                new BoneMeta("cf_J_LegUp01_s_L"     , "Upper Thigh"       , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegUp01_s_R")    {Y = false}        ,
                new BoneMeta("cf_J_LegUp02_s_L"     , "Center Thigh"      , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegUp02_s_R")    {Y = false}        ,
                new BoneMeta("cf_J_LegUp03_s_L"     , "Lower Thigh"       , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegUp03_s_R")    {Y = false}        ,

                new BoneMeta("cf_J_LegKnee_low_s_L" , "Kneecap"           , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegKnee_low_s_R")                   ,
                new BoneMeta("cf_J_LegKnee_back_s_L", "Kneecap Back"      , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegKnee_back_s_R")                  ,

                new BoneMeta("cf_J_LegLow01_s_L"    , "Upper Calve"       , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegLow01_s_R")   {Y = false}        ,
                new BoneMeta("cf_J_LegLow02_s_L"    , "Center Calve"      , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegLow02_s_R")   {Y = false}        ,
                new BoneMeta("cf_J_LegLow03_s_L"    , "Lower Calve"       , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_LegLow03_s_R")   {Y = false}        ,
                new BoneMeta("cf_J_Foot01_L"        , "Foot Scale 1"      , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_Foot01_R")       {XYZPostfix = null},
                new BoneMeta("cf_J_Foot02_L"        , "Foot Scale 2"      , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_Foot02_R")       {XYZPostfix = null},
                new BoneMeta("cf_J_Toes01_L"        , "Foot Toes"         , 0   , 3f  , MakerConstants.Body.Leg      , "cf_J_Toes01_R")       {L = true          , LMax = 2f  , LDisplayName = "Foot Toes Offset"} ,

                new BoneMeta("cf_J_Kokan"           , "Genital Area"      , 0   , 3f  , MakerConstants.Body.Lower    , "")                                       ,
                //todo add when males get added and move to a separate category? cm_J_dan_s
                //new BoneMeta("cm_J_dan100_00"     , "Penis"             , 0   , 3f  , BodyGenitals                 , "")                                       ,
                //new BoneMeta("cm_J_dan109_00"     , "Penis Tip"         , 0   , 3f  , BodyGenitals                 , "")                                       ,
                //new BoneMeta("?"                  , "Testicles"         , 0   , 4f  , BodyGenitals                 , "?")                                      ,
                new BoneMeta("cf_J_Ana"             , "Anus"              , 0   , 3f  , MakerConstants.Body.Lower    , "")                                       ,
            };
        }
    }
}
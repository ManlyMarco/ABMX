using System.Collections.Generic;
using MakerAPI;

namespace KKABMX.GUI
{
    public static class InterfaceData
    {
        public static List<BoneMeta> BoneControls { get; }

        public static readonly MakerCategory FaceAll;
        public static readonly MakerCategory FaceHead;
        public static readonly MakerCategory FaceEar;
        public static readonly MakerCategory FaceChin;
        public static readonly MakerCategory FaceCheek;
        public static readonly MakerCategory FaceEyebrow;
        public static readonly MakerCategory FaceEye01;
        public static readonly MakerCategory FaceEyes2;
        public static readonly MakerCategory FaceEyelashUp;
        public static readonly MakerCategory FaceEyelashDn;
        public static readonly MakerCategory FaceNose;
        public static readonly MakerCategory FaceMouth;
        public static readonly MakerCategory FaceMouth2;
        public static readonly MakerCategory BodyAll;
        public static readonly MakerCategory BodyBreast;
        public static readonly MakerCategory BodyBreast2;
        public static readonly MakerCategory BodyNipples;
        public static readonly MakerCategory BodyUpper; 
        public static readonly MakerCategory BodyUpper2;
        public static readonly MakerCategory BodyLeg;
        public static readonly MakerCategory BodyLower;
        public static readonly MakerCategory BodyLower2;
        public static readonly MakerCategory BodyArm;
        public static readonly MakerCategory BodyArm2;
        public static readonly MakerCategory BodyForearms;
        public static readonly MakerCategory BodyHands;
        public static readonly MakerCategory BodyThighs;
        public static readonly MakerCategory BodyFeet;
        public static readonly MakerCategory BodyBot;
        public static readonly MakerCategory BodySkirtScl;
        public static readonly MakerCategory BodyUnderhair;
        public static readonly MakerCategory BodyGenitals;

        static InterfaceData()
        {
            FaceAll       = MakerConstants.GetBuiltInCategory("00_FaceTop"   , "tglAll"                                                     );
            FaceHead      = new MakerCategory                ("00_FaceTop"   , "tglHeadABM"     , FaceAll.Position + 2 , "Head"             );
            FaceEar       = MakerConstants.GetBuiltInCategory("00_FaceTop"   , "tglEar"                                                     );
            FaceChin      = MakerConstants.GetBuiltInCategory("00_FaceTop"   , "tglChin"                                                    );
            FaceCheek     = MakerConstants.GetBuiltInCategory("00_FaceTop"   , "tglCheek"                                                   );
            FaceEyebrow   = MakerConstants.GetBuiltInCategory("00_FaceTop"   , "tglEyebrow"                                                 );
            FaceEyelashUp = new MakerCategory                ("00_FaceTop"   , "tglEyelashUpABM", FaceEyebrow.Position + 4, "Upper Eyelashes");
            FaceEyelashDn = new MakerCategory                ("00_FaceTop"   , "tglEyelashLoABM", FaceEyebrow.Position + 6, "Lower Eyelashes");
            FaceEye01     = MakerConstants.GetBuiltInCategory("00_FaceTop"   , "tglEye01"                                                   );
            FaceEyes2     = new MakerCategory                ("00_FaceTop"   , "tglEye02ABM"    , FaceEye01.Position + 2 , "Eyes 2"         );
            FaceNose      = MakerConstants.GetBuiltInCategory("00_FaceTop"   , "tglNose"                                                    );
            FaceMouth     = MakerConstants.GetBuiltInCategory("00_FaceTop"   , "tglMouth"                                                   );
            FaceMouth2    = new MakerCategory                ("00_FaceTop"   , "tglMouth2ABM"   , FaceMouth.Position + 2 , "Mouth 2"        );

            BodyAll       = MakerConstants.GetBuiltInCategory("01_BodyTop"   , "tglAll"                                                     );
            BodyBreast    = MakerConstants.GetBuiltInCategory("01_BodyTop"   , "tglBreast"                                                  );
            BodyBreast2   = new MakerCategory                ("01_BodyTop"   , "tglBreast2ABM"  , BodyBreast.Position + 2, "Chest 2"        );
            BodyNipples   = new MakerCategory                ("01_BodyTop"   , "tglNipplesABM"  , BodyBreast.Position + 4, "Nipples"        );
            BodyUpper     = MakerConstants.GetBuiltInCategory("01_BodyTop"   , "tglUpper"                                                   );
            BodyUpper2    = new MakerCategory                ("01_BodyTop"   , "tglUpper2ABM"   , BodyUpper.Position + 2 , "Upper Body 2"   );
            BodyLower     = MakerConstants.GetBuiltInCategory("01_BodyTop"   , "tglLower"                                                   );
            BodyLower2    = new MakerCategory                ("01_BodyTop"   , "tglLower2ABM"   , BodyLower.Position + 2 , "Lower Body 2"   );
            BodyArm       = MakerConstants.GetBuiltInCategory("01_BodyTop"   , "tglArm"                                                     );
            BodyArm2      = new MakerCategory                ("01_BodyTop"   , "tglArm2ABM"     , BodyArm.Position + 2   , "Arms 2"         );
            BodyForearms  = new MakerCategory                ("01_BodyTop"   , "tglForearmsABM" , BodyArm.Position + 4   , "Forearms"       );
            BodyHands     = new MakerCategory                ("01_BodyTop"   , "tglHandsABM"    , BodyArm.Position + 6   , "Hands"          );
            BodyLeg       = MakerConstants.GetBuiltInCategory("01_BodyTop"   , "tglLeg"                                                     );
            BodyThighs    = new MakerCategory                ("01_BodyTop"   , "tglThighsABM"   , BodyLeg.Position + 2   , "Thighs"         );
            BodyFeet      = new MakerCategory                ("01_BodyTop"   , "tglFeetABM"     , BodyLeg.Position + 4   , "Feet & Calves"  );
            BodyUnderhair = MakerConstants.GetBuiltInCategory("01_BodyTop"   , "tglUnderhair"                                               );
            BodyGenitals  = new MakerCategory                ("01_BodyTop"   , "tglGenitalsABM" , BodyUnderhair.Position + 2, "Genitals"    );

            BodyBot       = MakerConstants.GetBuiltInCategory("03_ClothesTop", "tglBot"                                                     );
            BodySkirtScl  = new MakerCategory                ("03_ClothesTop", "tglSkirtSclABM" , BodyBot.Position + 2   , "Skirt Scale"    );
            
            BoneControls = new List<BoneMeta>
            {
                //BoneMeta.Separator("00_FaceTop"   , "tglHead")                   ,
                new BoneMeta("cf_J_FaceBase"        , "Head Scale"                 , 0, 3f, FaceHead         , "")                 ,
                new BoneMeta("cf_s_head"            , "Head + Neck Scale"          , 0, 3f, FaceHead         , "")                 ,
                new BoneMeta("cf_J_FaceUp_ty"       , "Upper Head Scale"           , 0, 3f, FaceHead         , "")                 ,
                new BoneMeta("cf_J_FaceUp_tz"       , "Upper Front Head Scale"     , 0, 3f, FaceHead         , "")                 ,
                new BoneMeta("cf_J_FaceLow_sx"      , "Lower Head Scale 1"         , 0, 3f, FaceHead         , "")                 ,
                new BoneMeta("cf_J_FaceLow_tz"      , "Lower Head Scale 2"         , 0, 3f, FaceHead         , "")                 ,
                                                                                                             
                //BoneMeta.Separator("00_FaceTop"   , "tglAll")                    ,                         
                                                                                                             
                //BoneMeta.Separator(FaceEar)         ,                                                        
                //BoneMeta.Separator(FaceEar)         ,                                                        
                new BoneMeta("cf_J_EarBase_ry_L"    , "Ear Scale"                  , 0, 3f, FaceEar          , "cf_J_EarBase_ry_R"),
                new BoneMeta("cf_J_EarUp_L"         , "Upper Ear Scale"            , 0, 3f, FaceEar          , "cf_J_EarUp_R")     ,
                new BoneMeta("cf_J_EarLow_L"        , "Lower Ear Scale"            , 0, 3f, FaceEar          , "cf_J_EarLow_R")    ,
                                                                                                             
                //BoneMeta.Separator(FaceChin)        ,                                                        
                //BoneMeta.Separator(FaceChin)        ,                                                        
                new BoneMeta("cf_J_Chin_Base"       , "Jaw Scale"                  , 0, 3f, FaceChin         , "")                 ,
                new BoneMeta("cf_J_ChinLow"         , "Chin Scale"                 , 0, 3f, FaceChin         , "")                 ,
                new BoneMeta("cf_J_ChinTip_Base"    , "Chin Tip Scale"             , 0, 3f, FaceChin         , "")                 ,
                                                                                                             
                //BoneMeta.Separator(FaceCheek)       ,                                                        
                //BoneMeta.Separator(FaceCheek)       ,                                                        
                new BoneMeta("cf_J_CheekUpBase"     , "Cheek Scale"                , 0, 3f, FaceCheek        , "")                 ,
                new BoneMeta("cf_J_CheekUp2_L"      , "Cheekbone Scale"            , 0, 3f, FaceCheek        , "cf_J_CheekUp2_R")  ,
                new BoneMeta("cf_J_CheekUp_s_L"     , "Upper Cheek Scale"          , 0, 3f, FaceCheek        , "cf_J_CheekUp_s_R") ,
                new BoneMeta("cf_J_CheekLow_s_L"    , "Lower Cheek Scale"          , 0, 3f, FaceCheek        , "cf_J_CheekLow_s_R"),
                                                                                                             
                //BoneMeta.Separator(FaceEyebrow)     ,                                                        
                //BoneMeta.Separator(FaceEyebrow)     ,                                                        
                new BoneMeta("cf_J_Mayu_L"          , "Eyebrow Scale"              , 0, 3f, FaceEyebrow      , "cf_J_Mayu_R")      ,
                new BoneMeta("cf_J_MayuMid_s_L"     , "Inner Eyebrow Scale"        , 0, 3f, FaceEyebrow      , "cf_J_MayuMid_s_R") ,
                new BoneMeta("cf_J_MayuTip_s_L"     , "Outer Eyebrow Scale"        , 0, 3f, FaceEyebrow      , "cf_J_MayuTip_s_R") ,
                                                                                                             
                //BoneMeta.Separator(FaceEyes1)     ,                                                        
                //BoneMeta.Separator(FaceEyes1)     ,                                                        
                new BoneMeta("cf_J_Eye_tz"          , "Both Eyeballs Scale"        , 0, 3f, FaceEyes2        , "")                 ,
                new BoneMeta("cf_J_Eye_rz_L"        , "Eyeball Scale 1"            , 0, 3f, FaceEyes2        , "cf_J_Eye_rz_R")    ,
                new BoneMeta("cf_J_Eye_tx_L"        , "Eyeball Scale 2"            , 0, 3f, FaceEyes2        , "cf_J_Eye_tx_R")    ,
                                                                                                             
                new BoneMeta("cf_J_Eye01_s_L"       , "Upper Eyelashes Scale 1"    , 0, 3f, FaceEyelashUp    , "cf_J_Eye01_s_R")   ,
                new BoneMeta("cf_J_Eye02_s_L"       , "Upper Eyelashes Scale 2"    , 0, 3f, FaceEyelashUp    , "cf_J_Eye02_s_R")   ,
                new BoneMeta("cf_J_Eye03_s_L"       , "Upper Eyelashes Scale 3"    , 0, 3f, FaceEyelashUp    , "cf_J_Eye03_s_R")   ,
                new BoneMeta("cf_J_Eye04_s_L"       , "Upper Eyelashes Scale 4"    , 0, 3f, FaceEyelashUp    , "cf_J_Eye04_s_R")   ,
                new BoneMeta("cf_J_Eye05_s_L"       , "Upper Eyelashes Scale 5"    , 0, 3f, FaceEyelashUp    , "cf_J_Eye05_s_R")   ,
                                                                                                             
                new BoneMeta("cf_J_Eye06_s_L"       , "Lower Eyelashes Scale 1"    , 0, 3f, FaceEyelashDn    , "cf_J_Eye06_s_R")   ,
                new BoneMeta("cf_J_Eye07_s_L"       , "Lower Eyelashes Scale 2"    , 0, 3f, FaceEyelashDn    , "cf_J_Eye07_s_R")   ,
                new BoneMeta("cf_J_Eye08_s_L"       , "Lower Eyelashes Scale 3"    , 0, 3f, FaceEyelashDn    , "cf_J_Eye08_s_R")   ,
                                                                                                             
                //BoneMeta.Separator(FaceNose)        ,                                                        
                //BoneMeta.Separator(FaceNose)        ,                                                        
                new BoneMeta("cf_J_NoseBase_rx"     , "Nose Scale"                 , 0, 3f, FaceNose         , "")                 ,
                new BoneMeta("cf_J_Nose_tip"        , "Nose Tip Scale"             , 0, 3f, FaceNose         , "")                 ,
                new BoneMeta("cf_J_NoseBridge_rx"   , "Nose Bridge Scale 1"        , 0, 3f, FaceNose         , "")                 ,
                new BoneMeta("cf_J_NoseBridge_ty"   , "Nose Bridge Scale 2"        , 0, 3f, FaceNose         , "")                 ,
                //This will scale any Accessories parented to glasses                                     
                new BoneMeta("cf_J_megane_rx_ear"   , "Glasses Accessory Scale"    , 0, 3f, FaceNose         , "")                 , 
                                                                                                             
                ////BoneMeta.Separator("00_FaceTop"   , "tglMouth")                  ,                         
                                                                                                             
                new BoneMeta("cf_J_MouthBase_rx"    , "Mouth Scale 1"              , 0, 3f, FaceMouth2       , "")                 ,
                new BoneMeta("cf_J_MouthBase_ty"    , "Mouth Scale 2"              , 0, 3f, FaceMouth2       , "")                 ,
                new BoneMeta("cf_J_Mouth_L"         , "Mouth Side Scale"           , 0, 3f, FaceMouth2       , "cf_J_Mouth_R")     ,
                new BoneMeta("cf_J_Mouthup"         , "Upper Lip Scale"            , 0, 3f, FaceMouth2       , "")                 ,
                new BoneMeta("cf_J_MouthLow"        , "Lower Lip Scale"            , 0, 3f, FaceMouth2       , "")                 ,
                                                                                                             
                //BoneMeta.Separator(BodyAll)         ,                                                        
                //BoneMeta.Separator(BodyAll)         ,                                                        
                new BoneMeta("cf_n_height"          , "Body Scale"                 , 0, 2f, BodyAll          , "")                 ,
                                                                                                             
                //BoneMeta.Separator(BodyBreast)      ,                                                        
                //BoneMeta.Separator(BodyBreast)      ,                                                        
                new BoneMeta("cf_hit_bust02_L"      , "Breast Collision Scale"     , 0, 3f, BodyBreast       , "cf_hit_bust02_R")  ,
                new BoneMeta("cf_d_bust01_L"        , "Breast Scale 1"             , 0, 3f, BodyBreast       , "cf_d_bust01_R")    ,
                                                                                                             
                new BoneMeta("cf_d_bust02_L"        , "Breast Scale 2"             , 0, 3f, BodyBreast2      , "cf_d_bust02_R")    ,
                new BoneMeta("cf_d_bust03_L"        , "Breast Scale 3"             , 0, 3f, BodyBreast2      , "cf_d_bust03_R")    ,
                new BoneMeta("cf_s_bust00_L"        , "Extra Breast Scale 1"       , 0, 3f, BodyBreast2      , "cf_s_bust00_R")    ,
                new BoneMeta("cf_s_bust01_L"        , "Extra Breast Scale 2"       , 0, 3f, BodyBreast2      , "cf_s_bust01_R")    ,
                new BoneMeta("cf_s_bust02_L"        , "Extra Breast Scale 3"       , 0, 3f, BodyBreast2      , "cf_s_bust02_R")    ,
                new BoneMeta("cf_s_bust03_L"        , "Extra Breast Scale 4"       , 0, 3f, BodyBreast2      , "cf_s_bust03_R")    ,
                                                                                                             
                new BoneMeta("cf_s_bnip01_L"        , "Areola Scale 1"             , 0, 3f, BodyNipples      , "cf_s_bnip01_R")    ,
                new BoneMeta("cf_s_bnip025_L"       , "Areola Scale 2"             , 0, 3f, BodyNipples      , "cf_s_bnip025_R")   ,
                new BoneMeta("cf_d_bnip01_L"        , "Nipple Scale 1"             , 0, 3f, BodyNipples      , "cf_d_bnip01_R")    ,
                //This is the one that doesn't seem doing anything
                new BoneMeta("cf_s_bnip015_L"       , "Nipple Scale 2"             , 0, 3f, BodyNipples      , "cf_s_bnip015_R")   ,
                new BoneMeta("cf_s_bnip02_L"        , "Nipple Tip Scale"           , 0, 3f, BodyNipples      , "cf_s_bnip02_R")    ,
                new BoneMeta("cf_s_bnipacc_L"       , "Nipple Accessory Scale"     , 0, 3f, BodyNipples      , "cf_s_bnipacc_R")   ,

                //BoneMeta.Separator(BodyUpper)       ,
                //BoneMeta.Separator(BodyUpper)       ,
                new BoneMeta("cf_s_neck"            , "Neck Scale"                 , 0, 3f, BodyUpper        , "")                 ,
                new BoneMeta("cf_s_shoulder02_L"    , "Shoulder Scale"             , 0, 3f, BodyUpper        , "cf_s_shoulder02_R"),
                new BoneMeta("cf_hit_shoulder_L"    , "Shoulder Collision Scale"   , 0, 3f, BodyUpper        , "cf_hit_shoulder_R"),

                new BoneMeta("cf_s_spine01"         , "Lower Torso Scale 1"        , 0, 3f, BodyUpper2       , "")                 ,
                new BoneMeta("cf_s_spine02"         , "Lower Torso Scale 2"        , 0, 3f, BodyUpper2       , "")                 ,
                new BoneMeta("cf_s_spine03"         , "Lower Torso Scale 3"        , 0, 3f, BodyUpper2       , "")                 ,
                new BoneMeta("cf_hit_spine01"       , "Upper Torso Collision Scale", 0, 3f, BodyUpper2       , "")                 ,
                new BoneMeta("cf_hit_spine02_L"     , "Lower Torso Collision Scale", 0, 3f, BodyUpper2       , "")                 ,

                //BoneMeta.Separator(BodyLower)                  ,
                //BoneMeta.Separator(BodyLower)                  ,
                new BoneMeta("cf_s_siri_L"          , "Ass Scale"                  , 0, 3f, BodyLower        , "cf_s_siri_R")      ,
                new BoneMeta("cf_hit_siri_L"        , "Ass Collision Scale"        , 0, 3f, BodyLower        , "cf_hit_siri_R")    ,

                //BoneMeta.Separator("01_BodyTop"     , "tglLeg")                    ,
                //BoneMeta.Separator("01_BodyTop"     , "tglLeg")                    ,

                new BoneMeta("cf_s_waist01"         , "Upper Waist Scale"          , 0, 3f, BodyLower2       , "")                 ,
                new BoneMeta("cf_s_waist02"         , "Lower Waist Scale"          , 0, 3f, BodyLower2       , "")                 ,
                new BoneMeta("cf_j_waist01"         , "Waist & Leg Scale 1"        , 0, 3f, BodyLower2       , "")                 ,
                new BoneMeta("cf_j_waist02"         , "Waist & Leg Scale 2"        , 0, 3f, BodyLower2       , "")                 ,
                new BoneMeta("cf_hit_berry"         , "Belly Collision Scale"      , 0, 3f, BodyLower2       , "")                 ,
                new BoneMeta("cf_hit_waist_L"       , "Waist Collision Scale"      , 0, 3f, BodyLower2       , "")                 ,

                //BoneMeta.Separator("01_BodyTop"   , "tglArm")                    ,

                new BoneMeta("cf_j_shoulder_L"      , "Shoulder & Arm Scale"       , 0, 3f, BodyArm2         , "cf_j_shoulder_R")  ,
                new BoneMeta("cf_j_arm00_L"         , "Arm Scale"                  , 0, 3f, BodyArm2         , "cf_j_arm00_R")     ,
                new BoneMeta("cf_s_arm01_L"         , "Upper Arm Thickness 1"      , 0, 3f, BodyArm2         , "cf_s_arm01_R")     ,
                new BoneMeta("cf_s_arm02_L"         , "Upper Arm Thickness 2"      , 0, 3f, BodyArm2         , "cf_s_arm02_R")     ,
                new BoneMeta("cf_s_arm03_L"         , "Upper Arm Thickness 3"      , 0, 3f, BodyArm2         , "cf_s_arm03_R")     ,

                new BoneMeta("cf_j_forearm01_L"     , "Forearm Scale"              , 0, 3f, BodyForearms     , "cf_j_forearm01_R") ,
                new BoneMeta("cf_s_forearm01_L"     , "Forearm Thickness 1"        , 0, 3f, BodyForearms     , "cf_s_forearm01_R") ,
                new BoneMeta("cf_s_forearm02_L"     , "Forearm Thickness 2"        , 0, 3f, BodyForearms     , "cf_s_forearm02_R") ,

                new BoneMeta("cf_s_wrist_L"         , "Wrist Scale"                , 0, 3f, BodyHands        , "cf_s_wrist_R")     ,
                new BoneMeta("cf_j_hand_L"          , "Hand Scale 1"               , 0, 3f, BodyHands        , "cf_j_hand_R")      ,
                new BoneMeta("cf_s_hand_L"          , "Hand Scale 2"               , 0, 3f, BodyHands        , "cf_s_hand_R")      ,
                new BoneMeta("cf_hit_arm_L"         , "Hand Collision Scale"       , 0, 3f, BodyHands        , "cf_hit_arm_R")     ,

                new BoneMeta("cf_j_thigh00_L"       , "Thigh & Leg Scale"          , 0, 3f, BodyThighs       , "cf_j_thigh00_R")   ,
                new BoneMeta("cf_s_thigh01_L"       , "Upper Thigh Scale"          , 0, 3f, BodyThighs       , "cf_s_thigh01_R")   ,
                new BoneMeta("cf_s_thigh02_L"       , "Middle Thigh Scale"         , 0, 3f, BodyThighs       , "cf_s_thigh02_R")   ,
                new BoneMeta("cf_s_thigh03_L"       , "Lower Thigh Scale"          , 0, 3f, BodyThighs       , "cf_s_thigh03_R")   ,

                new BoneMeta("cf_j_leg01_L"         , "Lower Leg Scale"            , 0, 3f, BodyFeet         , "cf_j_leg01_R")     ,
                new BoneMeta("cf_s_leg01_L"         , "Upper Calve Scale"          , 0, 3f, BodyFeet         , "cf_s_leg01_R")     ,
                new BoneMeta("cf_s_leg02_L"         , "Lower Calve Scale"          , 0, 3f, BodyFeet         , "cf_s_leg02_R")     ,
                new BoneMeta("cf_s_leg03_L"         , "Ankle Scale"                , 0, 3f, BodyFeet         , "cf_s_leg03_R")     ,
                new BoneMeta("cf_j_foot_L"          , "Foot Scale 1"               , 0, 3f, BodyFeet         , "cf_j_foot_R")      ,
                new BoneMeta("cf_j_leg03_L"         , "Foot Scale 2"               , 0, 3f, BodyFeet         , "cf_j_leg03_R")     ,

                //BoneMeta.Separator("03_ClothesTop", "tglBot")                    ,

                new BoneMeta("cf_d_sk_top"          , "Whole Skirt Scale"          , 0, 3f, BodySkirtScl     , "")                 ,
                new BoneMeta("cf_d_sk_00_00"        , "Skirt Front"                , 0, 3f, BodySkirtScl     , "")                 ,
                new BoneMeta("cf_d_sk_07_00"        , "Skirt Front Sides"          , 0, 3f, BodySkirtScl     , "cf_d_sk_01_00")    ,
                new BoneMeta("cf_d_sk_06_00"        , "Skirt Sides"                , 0, 3f, BodySkirtScl     , "cf_d_sk_02_00")    ,
                new BoneMeta("cf_d_sk_05_00"        , "Skirt Back Sides"           , 0, 3f, BodySkirtScl     , "cf_d_sk_03_00")    ,
                new BoneMeta("cf_d_sk_04_00"        , "Skirt Back"                 , 0, 3f, BodySkirtScl     , "")                 ,


                new BoneMeta("cf_d_kokan"           , "Genital area Scale"         , 0, 3f, BodyGenitals     , "")                 ,
                new BoneMeta("cf_j_kokan"           , "Pubic mound Scale"          , 0, 3f, BodyGenitals     , "")                 ,
                new BoneMeta("cm_J_dan100_00"       , "Penis Scale"                , 0, 3f, BodyGenitals     , "")                 ,
                new BoneMeta("cm_J_dan109_00"       , "Penis tip Scale"            , 0, 3f, BodyGenitals     , "")                 ,
                new BoneMeta("cm_J_dan_f_L"         , "Testicles Scale"            , 0, 4f, BodyGenitals     , "cm_J_dan_f_R")     ,
                new BoneMeta("cf_j_ana"             , "Anus Scale"                 , 0, 3f, BodyGenitals     , "")                 ,
            };
        }
    }
}
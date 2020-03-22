using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Linq;
using KKABMX.Core;
using KKAPI.Maker;
using UnityEngine;
using Logger = KKABMX.Core.KKABMX_Core;
using CharaCustom;

namespace KKABMX.GUI
{
    /// <summary>
    /// Old style ABM GUI by essu, modified to work with ABMX
    /// </summary>
    internal class KKABMX_AdvancedGUI : MonoBehaviour
    {
        private const int BoneNameWidth = 120;
        private Rect _windowRect = new Rect(20, 220, 800, 600);

        private BoneController _boneControllerMgr;

        private readonly GUILayoutOption _gloHeight = GUILayout.Height(23);
        private readonly GUILayoutOption _gloSlider = GUILayout.ExpandWidth(true);
        private readonly GUILayoutOption _gloSliderWidth = GUILayout.Width(125);
        private readonly GUILayoutOption _gloWidth30 = GUILayout.Width(30);

        private GUIStyle _gsButtonReset;
        private GUIStyle _gsInput;
        private GUIStyle _gsLabel;

        private bool _initGui = true;

        private Vector2 _scrollPosition = Vector2.zero;

        private Vector2 _suggestionScrollPosition = Vector2.zero;

        private readonly HashSet<string> _addedBones = new HashSet<string>();
        private bool _onlyShowAdditional;
        private string _boneAddFieldValue = "";

        private float incrementSize = 0.1f;

        private void Awake()
        {
            MakerAPI.MakerFinishedLoading += (sender, args) => _boneControllerMgr = MakerAPI.GetCharacterControl().GetComponent<BoneController>();
        }

        private void OnGUI()
        {
            if (_initGui)
            {
                _gsInput = new GUIStyle(UnityEngine.GUI.skin.textArea);
                _gsLabel = new GUIStyle(UnityEngine.GUI.skin.label);
                _gsButtonReset = new GUIStyle(UnityEngine.GUI.skin.button);
                _gsLabel.alignment = TextAnchor.MiddleRight;
                _gsLabel.normal.textColor = Color.white;
                _initGui = false;
            }

            if (!MakerAPI.InsideAndLoaded) return;

            _windowRect = GUILayout.Window(1724, _windowRect, LegacyWindow, "Advanced Bone Sliders");
            _windowRect.x = Mathf.Min(Screen.width - _windowRect.width, Mathf.Max(0, _windowRect.x));
            _windowRect.y = Mathf.Min(Screen.height - _windowRect.height, Mathf.Max(0, _windowRect.y));

            if (_windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                Input.ResetInputAxes();
        }

        private void suggestionList()
        {
            GUILayout.BeginVertical();
            {

            }
            GUILayout.EndVertical();
        }

        private void LegacyWindow(int id)
        {


            DrawHeader();

            GUILayout.BeginVertical();
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, _gloSlider);

                var shownModifiers = _onlyShowAdditional ?
                    _boneControllerMgr.Modifiers.Where(x => _addedBones.Contains(x.BoneName)) :
                    _boneControllerMgr.Modifiers.OrderBy(x => x.BoneName);

                if (!string.IsNullOrEmpty(_boneAddFieldValue))
                    shownModifiers = shownModifiers.Where(x => x.BoneName.ToLower().Contains(_boneAddFieldValue.ToLower()));

                foreach (var mod in shownModifiers)
                {
                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box);
                    {
#if KK
                        var modData = mod.GetModifier(MakerAPI.GetCurrentCoordinateType());
#elif EC
                        var modData = mod.GetModifier(KoikatsuCharaFile.ChaFileDefine.CoordinateType.School01);
#elif AI
                        var modData = mod.GetModifier(CoordinateType.Unknown);
#endif
                        var counterBoneName = "";
                        var _scaleSymmetry = modData.scaleSymmetry;
                        var _posSymmetry = false;
                        var _rotSymmetry = false;

                        var scale = modData.ScaleModifier;
                        var len = modData.LengthModifier;
                        var position = modData.PositionModifier;
                        var rotation = modData.RotationModifier;

                        var newScale = new Vector3();
                        var newLen = new float();
                        var newPosition = new Vector3();
                        var newRotation = new Vector3();

                        if (mod.BoneName.EndsWith("R") || mod.BoneName.EndsWith("L") || mod.BoneName.EndsWith("R_00") || mod.BoneName.EndsWith("L_00"))
                        {
                            if (mod.BoneName.EndsWith("R"))
                            {
                                counterBoneName = mod.BoneName.Remove(mod.BoneName.Length - 1) + "L";
                            }
                            else if (mod.BoneName.EndsWith("L"))
                            {
                                counterBoneName = mod.BoneName.Remove(mod.BoneName.Length - 1) + "R";
                            }
                            else if (mod.BoneName.EndsWith("R_00"))
                            {
                                counterBoneName = mod.BoneName.Remove(mod.BoneName.Length - 4) + "L_00";
                            }
                            else if (mod.BoneName.EndsWith("L_00"))
                            {
                                counterBoneName = mod.BoneName.Remove(mod.BoneName.Length - 4) + "R_00";
                            }
                        }

                        if (mod.BoneName.Contains("Mune"))
                        {
                            var bc = _boneControllerMgr;
                            bc.ChaControl.UpdateBustGravity();
                        }

                        GUILayout.BeginHorizontal(_gloSlider);
                        {
                            GUILayout.Label(mod.BoneName, _gsLabel, GUILayout.Width(BoneNameWidth));
                            len = GUILayout.HorizontalSlider(len, -2f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);

                            if (GUILayout.Button("X", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                len = 1f;
                            }
                            modData.LengthModifier = len;
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(_gloSlider);
                        {
                            GUILayout.Label("Length", GUILayout.Width(BoneNameWidth));
                            float.TryParse(GUILayout.TextField(len.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out len);

                            modData.LengthModifier = len;
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(_gloSlider);
                        {
                            GUILayout.Label(mod.BoneName, _gsLabel, GUILayout.Width(BoneNameWidth));

                            if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                scale.x = scale.x - incrementSize;
                            }
                            scale.x = GUILayout.HorizontalSlider(scale.x, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                scale.x = scale.x + incrementSize;
                            }
                            if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                scale.y = scale.y - incrementSize;
                            }
                            scale.y = GUILayout.HorizontalSlider(scale.y, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                scale.y = scale.y + incrementSize;
                            }
                            if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                scale.z = scale.z - incrementSize;
                            }
                            scale.z = GUILayout.HorizontalSlider(scale.z, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                scale.z = scale.z + incrementSize;
                            }

                            if (GUILayout.Button("X", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                scale = Vector3.one;
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(_gloSlider);
                        {
#if AI
                            GUILayout.Label("X / Y / Z / Scale", GUILayout.Width(BoneNameWidth));
#else
                            if (GUILayout.Toggle(mod.IsCoordinateSpecific(), "Per coordinate", GUILayout.Width(BoneNameWidth)))
                                mod.MakeCoordinateSpecific();
                            else
                                mod.MakeNonCoordinateSpecific();
#endif
                            GUILayout.Space(34);
                            float.TryParse(GUILayout.TextField(scale.x.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out scale.x);
                            GUILayout.Space(34);
                            GUILayout.Space(34);
                            float.TryParse(GUILayout.TextField(scale.y.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out scale.y);
                            GUILayout.Space(34);
                            GUILayout.Space(34);
                            float.TryParse(GUILayout.TextField(scale.z.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out scale.z);


                            if (counterBoneName != "") {
                                modData.scaleSymmetry = GUILayout.Toggle(modData.scaleSymmetry, "Symmetry"); }
                                                                                                          
                            while (modData.scaleSymmetry && scale != modData.ScaleModifier && counterBoneName != "")
                            {
                                if (_boneControllerMgr.GetModifier(counterBoneName) == null)
                                {
                                    var newMod = new BoneModifier(counterBoneName);
                                    _boneControllerMgr.AddModifier(newMod);
                                    if (newMod.BoneTransform == null)
                                    {
                                        Logger.Logger.LogMessage($"Failed to add bone {counterBoneName}, make sure the name is correct.");
                                        _boneControllerMgr.Modifiers.Remove(newMod);
                                        break;
                                    }
                                }
                                var symmetricModData = _boneControllerMgr.GetModifier(counterBoneName).GetModifier(CoordinateType.Unknown);

                                symmetricModData.ScaleModifier.x = scale.x;
                                symmetricModData.ScaleModifier.y = scale.y;
                                symmetricModData.ScaleModifier.z = scale.z;
                                break;
                            }
                            modData.ScaleModifier = scale;

                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(_gloSlider);
                        {

                            GUILayout.Label(mod.BoneName, _gsLabel, GUILayout.Width(BoneNameWidth));


                            if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                position.x = position.x - incrementSize;
                            }
                            position.x = GUILayout.HorizontalSlider(position.x, -0.2f, 0.2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                position.x = position.x + incrementSize;
                            }
                            if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                position.y = position.y - incrementSize;
                            }
                            position.y = GUILayout.HorizontalSlider(position.y, -0.2f, 0.2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                position.y = position.y + incrementSize;
                            }
                            if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                position.z = position.z - incrementSize;
                            }
                            position.z = GUILayout.HorizontalSlider(position.z, -0.2f, 0.2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                position.z = position.z + incrementSize;
                            }


                            if (GUILayout.Button("X", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                position = Vector3.zero;
                                len = 1f;
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(_gloSlider);
                        {

#if AI
                            GUILayout.Label("X / Y / Z Position", GUILayout.Width(BoneNameWidth));
#else
                            if (GUILayout.Toggle(mod.IsCoordinateSpecific(), "Per coordinate", GUILayout.Width(BoneNameWidth)))
                                mod.MakeCoordinateSpecific();
                            else
                                mod.MakeNonCoordinateSpecific();
#endif
                            GUILayout.Space(34);
                            float.TryParse(GUILayout.TextField(position.x.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out position.x);
                            GUILayout.Space(34);
                            GUILayout.Space(34);
                            float.TryParse(GUILayout.TextField(position.y.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out position.y);
                            GUILayout.Space(34);
                            GUILayout.Space(34);
                            float.TryParse(GUILayout.TextField(position.z.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out position.z);

                            if (counterBoneName != "")
                            {
                                modData.posSymmetry = GUILayout.Toggle(modData.posSymmetry, "Symmetry");
                            }

                            while (modData.posSymmetry && position != modData.PositionModifier && counterBoneName != "")
                            {
                                if (_boneControllerMgr.GetModifier(counterBoneName) == null)
                                {
                                    var newMod = new BoneModifier(counterBoneName);
                                    _boneControllerMgr.AddModifier(newMod);
                                    if (newMod.BoneTransform == null)
                                    {
                                        Logger.Logger.LogMessage($"Failed to add bone {counterBoneName}, make sure the name is correct.");
                                        _boneControllerMgr.Modifiers.Remove(newMod);
                                        break;
                                    }
                                }
                                var symmetricModData = _boneControllerMgr.GetModifier(counterBoneName).GetModifier(CoordinateType.Unknown);

                                symmetricModData.PositionModifier.x = position.x * -1;
                                symmetricModData.PositionModifier.y = position.y;
                                symmetricModData.PositionModifier.z = position.z;
                                break;
                            }
                            GUILayout.Space(30);

                            modData.PositionModifier = position;
                        }
                        GUILayout.EndHorizontal();

                        if (!_boneControllerMgr.NoRotationBones.Contains(mod.BoneName))
                        {
                            GUILayout.BeginHorizontal(_gloSlider);
                            {
                                GUILayout.Label(mod.BoneName, _gsLabel, GUILayout.Width(BoneNameWidth));


                                if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                                {
                                    rotation.x = rotation.x - incrementSize;

                                }
                                rotation.x = GUILayout.HorizontalSlider(rotation.x, -180f, 180f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                                if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                                {
                                    rotation.x = rotation.x + incrementSize;

                                }
                                if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                                {
                                    rotation.y = rotation.y - incrementSize;

                                }
                                rotation.y = GUILayout.HorizontalSlider(rotation.y, -180f, 180f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                                if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                                {
                                    rotation.y = rotation.y + incrementSize;

                                }
                                if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                                {
                                    rotation.z = rotation.z - incrementSize;

                                }
                                rotation.z = GUILayout.HorizontalSlider(rotation.z, -180f, 180f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                                if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                                {
                                    rotation.z = rotation.z + incrementSize;

                                }


                                if (GUILayout.Button("X", _gsButtonReset, _gloWidth30, _gloHeight))
                                {
                                    rotation = Vector3.zero;
                                }
                            }
                            GUILayout.EndHorizontal();


                            GUILayout.BeginHorizontal(_gloSlider);
                            {
#if AI
                                GUILayout.Label("X / Y / Z Rotation", GUILayout.Width(BoneNameWidth));
#else
                            if (GUILayout.Toggle(mod.IsCoordinateSpecific(), "Per coordinate", GUILayout.Width(BoneNameWidth)))
                                mod.MakeCoordinateSpecific();
                            else
                                mod.MakeNonCoordinateSpecific();
#endif
                                GUILayout.Space(34);
                                float.TryParse(GUILayout.TextField(rotation.x.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out rotation.x);
                                GUILayout.Space(34);
                                GUILayout.Space(34);
                                float.TryParse(GUILayout.TextField(rotation.y.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out rotation.y);
                                GUILayout.Space(34);
                                GUILayout.Space(34);
                                float.TryParse(GUILayout.TextField(rotation.z.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out rotation.z);

                                if (counterBoneName != "")
                                {
                                    modData.rotSymmetry = GUILayout.Toggle(modData.rotSymmetry, "Symmetry");
                                }

                                while (modData.rotSymmetry && rotation != modData.RotationModifier && counterBoneName != "")
                                {
                                    if (_boneControllerMgr.GetModifier(counterBoneName) == null)
                                    {
                                        var newMod = new BoneModifier(counterBoneName);
                                        _boneControllerMgr.AddModifier(newMod);
                                        if (newMod.BoneTransform == null)
                                        {
                                            Logger.Logger.LogMessage($"Failed to add bone {counterBoneName}, make sure the name is correct.");
                                            _boneControllerMgr.Modifiers.Remove(newMod);
                                            break;
                                        }
                                    }
                                    var symmetricModData = _boneControllerMgr.GetModifier(counterBoneName).GetModifier(CoordinateType.Unknown);

                                    symmetricModData.RotationModifier.x = rotation.x;
                                    symmetricModData.RotationModifier.y = rotation.y * -1;
                                    symmetricModData.RotationModifier.z = rotation.z * -1;
                                    break;
                                }
                                GUILayout.Space(30);

                                modData.RotationModifier = rotation;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            UnityEngine.GUI.DragWindow();
        }

        private void DrawHeader()
        {

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                {
                    if (GUILayout.Button("Add"))
                    {

                        Logger.Logger.LogMessage($"Bone {_boneAddFieldValue} is already added.");
                    }
                    GUILayout.Label("Add a new bone to the list or search existing bones");

                    // todo Use _boneControllerMgr.GetAllPossibleBoneNames() for autocomplete/suggestions
                    _boneAddFieldValue = GUILayout.TextField(_boneAddFieldValue, GUILayout.Width(110));
                                       
                    if (GUILayout.Button("Add"))
                    {
                        _addedBones.Add(_boneAddFieldValue);

                        var bc = _boneControllerMgr;

                        if (bc.GetModifier(_boneAddFieldValue) != null)
                        {
                            Logger.Logger.LogMessage($"Bone {_boneAddFieldValue} is already added.");
                            _boneAddFieldValue = "";
                        }
                        else
                        {
                            var newMod = new BoneModifier(_boneAddFieldValue);
                            bc.AddModifier(newMod);
                            if (newMod.BoneTransform == null)
                            {
                                Logger.Logger.LogMessage($"Failed to add bone {_boneAddFieldValue}, make sure the name is correct.");
                                bc.Modifiers.Remove(newMod);
                            }
                            else
                            {
                                Logger.Logger.LogMessage($"Added bone {_boneAddFieldValue} successfully. Modify it to make it save.");
                                _boneAddFieldValue = "";
                            }
                        }
                    }

                    _onlyShowAdditional = GUILayout.Toggle(_onlyShowAdditional, "Only show added bones");
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box, GUILayout.MaxHeight(36), GUILayout.Height(36));
                {
                    GUILayout.Label("Increment: ");
                    if (GUILayout.Button("-", _gsButtonReset, _gloWidth30, _gloHeight))
                    {
                        incrementSize = incrementSize * 0.1f;
                    }

                    float.TryParse(GUILayout.TextField(incrementSize.ToString(CultureInfo.InvariantCulture), _gsInput, GUILayout.Width(60), _gloHeight), out incrementSize);

                    if (GUILayout.Button("+", _gsButtonReset, _gloWidth30, _gloHeight))
                    {
                        incrementSize = incrementSize * 10f;
                    }


                    if (_boneAddFieldValue != "")
                    {
                        var possibleBones = _boneControllerMgr.GetAllPossibleBoneNames();
                        var bonesArray = possibleBones.Where(x => x.ToLower().Contains(_boneAddFieldValue.ToLower()));


                        _suggestionScrollPosition = GUILayout.BeginScrollView(_suggestionScrollPosition, GUILayout.Height(40), GUILayout.MaxHeight(40), GUILayout.Width(530), GUILayout.MaxWidth(530));
                        GUILayout.BeginHorizontal(GUILayout.MaxHeight(20), GUILayout.Width(500));
                        {
                            foreach (var boneResult in bonesArray)
                            {
                                if (GUILayout.Button(boneResult, _gsButtonReset, GUILayout.MinWidth(120), _gloHeight))
                                {
                                    _boneAddFieldValue = boneResult;
                                }
                            }

                        }
                        GUILayout.EndHorizontal();
                        GUILayout.EndScrollView();

                    }
                    else
                    {
                        GUILayout.BeginHorizontal(GUILayout.MaxHeight(30));
                        {
                            GUILayout.Space(500);
                        }
                        GUILayout.EndHorizontal();
                    }

                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

        }
    }
}

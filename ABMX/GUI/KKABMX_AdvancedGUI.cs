using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Logging;
using KKABMX.Core;
using KKAPI.Maker;
using UnityEngine;
using Logger = KKABMX.Core.KKABMX_Core;

namespace KKABMX.GUI
{
    /// <summary>
    /// Old style ABM GUI by essu, modified to work with ABMX
    /// </summary>
    internal class KKABMX_AdvancedGUI : MonoBehaviour
    {
        private const int BoneNameWidth = 120;
        private Rect _windowRect = new Rect(20, 220, 725, 400);

        private CameraControl_Ver2 _ccv2;
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

        private readonly HashSet<string> _addedBones = new HashSet<string>();
        private bool _onlyShowAdditional;
        private string _boneAddFieldValue = "";

        private void Awake()
        {
            MakerAPI.MakerFinishedLoading += (sender, args) =>
            {
                _ccv2 = FindObjectOfType<CameraControl_Ver2>();
                _boneControllerMgr = FindObjectOfType<BoneController>();
            };
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

            var mp = Input.mousePosition;
            mp.y = Screen.height - mp.y; //Mouse Y is inverse Screen Y
            _ccv2.enabled = !_windowRect.Contains(mp); //Disable camera when inside menu. 100% guaranteed to cause conflicts.
            _windowRect = GUILayout.Window(1724, _windowRect, LegacyWindow, "Advanced KKABM Sliders"); //1724 guaranteed to be unique orz
            _windowRect.x = Mathf.Min(Screen.width - _windowRect.width, Mathf.Max(0, _windowRect.x));
            _windowRect.y = Mathf.Min(Screen.height - _windowRect.height, Mathf.Max(0, _windowRect.y));
        }

        private void LegacyWindow(int id)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, _gloSlider);
            GUILayout.BeginVertical();
            {
                DrawHeader();

                foreach (var mod in (_onlyShowAdditional ? _boneControllerMgr.Modifiers.Where(x => _addedBones.Contains(x.BoneName)) : _boneControllerMgr.Modifiers))
                {
                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box);
                    {
#if KK
                        var modData = mod.GetModifier(MakerAPI.GetCurrentCoordinateType());
#elif EC
                        var modData = mod.GetModifier(KoikatsuCharaFile.ChaFileDefine.CoordinateType.School01);
#endif
                        var v3 = modData.ScaleModifier;
                        var len = modData.LengthModifier;

                        GUILayout.BeginHorizontal(_gloSlider);
                        {
                            GUILayout.Label(mod.BoneName, _gsLabel, GUILayout.Width(BoneNameWidth));

                            v3.x = GUILayout.HorizontalSlider(v3.x, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            v3.y = GUILayout.HorizontalSlider(v3.y, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            v3.z = GUILayout.HorizontalSlider(v3.z, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);

                            len = GUILayout.HorizontalSlider(len, 0.1f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);

                            if (GUILayout.Button("X", _gsButtonReset, _gloWidth30, _gloHeight))
                            {
                                v3 = Vector3.one;
                                len = 1f;
                            }

                            modData.ScaleModifier = v3;
                            modData.LengthModifier = len;
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(_gloSlider);
                        {
                            //GUILayout.Label("X / Y / Z / Length", gs_Label, GUILayout.Width(BoneNameWidth));
                            if (GUILayout.Toggle(mod.IsCoordinateSpecific(), "Per coordinate", GUILayout.Width(BoneNameWidth)))
                                mod.MakeCoordinateSpecific();
                            else
                                mod.MakeNonCoordinateSpecific();

                            float.TryParse(GUILayout.TextField(v3.x.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out v3.x);
                            float.TryParse(GUILayout.TextField(v3.y.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out v3.y);
                            float.TryParse(GUILayout.TextField(v3.z.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out v3.z);

                            float.TryParse(GUILayout.TextField(len.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out len);

                            GUILayout.Space(30);

                            modData.ScaleModifier = v3;
                            modData.LengthModifier = Mathf.Max(len, 0.1f);
                        }
                        GUILayout.EndHorizontal();
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
            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            {
                GUILayout.Label("Add a new bone to the list. If valid, it will be saved to the card.");

                // todo Use _boneControllerMgr.GetAllPossibleBoneNames() for autocomplete/suggestions
                _boneAddFieldValue = GUILayout.TextField(_boneAddFieldValue, GUILayout.Width(90));

                if (GUILayout.Button("Add"))
                {
                    _addedBones.Add(_boneAddFieldValue);

                    var bc = _boneControllerMgr;

                    if (bc.GetModifier(_boneAddFieldValue) != null)
                    {
                        Logger.Log(LogLevel.Message, $"Bone {_boneAddFieldValue} is already added.");
                        _boneAddFieldValue = "";
                    }
                    else
                    {
                        var newMod = new BoneModifier(_boneAddFieldValue);
                        bc.AddModifier(newMod);
                        if (newMod.BoneTransform == null)
                        {
                            Logger.Log(LogLevel.Message, $"Failed to add bone {_boneAddFieldValue}, make sure the name is correct.");
                            bc.Modifiers.Remove(newMod);
                        }
                        else
                        {
                            Logger.Log(LogLevel.Message, $"Added bone {_boneAddFieldValue} successfully. Modify it to make it save.");
                            _boneAddFieldValue = "";
                        }
                    }
                }

                _onlyShowAdditional = GUILayout.Toggle(_onlyShowAdditional, "Only show added bones");
            }
            GUILayout.EndHorizontal();
        }
    }
}

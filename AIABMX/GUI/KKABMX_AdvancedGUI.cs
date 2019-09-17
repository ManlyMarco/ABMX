using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            _windowRect = GUILayout.Window(1724, _windowRect, LegacyWindow, "Advanced Bonemod Sliders");
            _windowRect.x = Mathf.Min(Screen.width - _windowRect.width, Mathf.Max(0, _windowRect.x));
            _windowRect.y = Mathf.Min(Screen.height - _windowRect.height, Mathf.Max(0, _windowRect.y));

            if (_windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                Input.ResetInputAxes();
        }

        private void LegacyWindow(int id)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, _gloSlider);
            GUILayout.BeginVertical();
            {
                DrawHeader();

                var shownModifiers = _onlyShowAdditional ?
                    _boneControllerMgr.Modifiers.Where(x => _addedBones.Contains(x.BoneName)) :
                    _boneControllerMgr.Modifiers.OrderBy(x => x.BoneName);

                if (!string.IsNullOrEmpty(_boneAddFieldValue))
                    shownModifiers = shownModifiers.Where(x => x.BoneName.Contains(_boneAddFieldValue));

                foreach (var mod in shownModifiers)
                {
                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box);
                    {
                        //todo var modData = mod.GetModifier(MakerAPI.GetCurrentCoordinateType());
                        var modData = mod.GetModifier(CoordinateType.Unknown);

                        var v3 = modData.ScaleModifier;
                        var len = modData.LengthModifier;

                        GUILayout.BeginHorizontal(_gloSlider);
                        {
                            GUILayout.Label(mod.BoneName, _gsLabel, GUILayout.Width(BoneNameWidth));

                            v3.x = GUILayout.HorizontalSlider(v3.x, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            v3.y = GUILayout.HorizontalSlider(v3.y, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);
                            v3.z = GUILayout.HorizontalSlider(v3.z, 0f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);

                            len = GUILayout.HorizontalSlider(len, -2f, 2f, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);

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
                            GUILayout.Label("X / Y / Z / Length", GUILayout.Width(BoneNameWidth));
                            //todo
                            // if (GUILayout.Toggle(mod.IsCoordinateSpecific(), "Per coordinate", GUILayout.Width(BoneNameWidth)))
                            //     mod.MakeCoordinateSpecific();
                            // else
                            //     mod.MakeNonCoordinateSpecific();

                            float.TryParse(GUILayout.TextField(v3.x.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out v3.x);
                            float.TryParse(GUILayout.TextField(v3.y.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out v3.y);
                            float.TryParse(GUILayout.TextField(v3.z.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out v3.z);

                            float.TryParse(GUILayout.TextField(len.ToString(CultureInfo.InvariantCulture), _gsInput, _gloSliderWidth, _gloHeight), out len);

                            GUILayout.Space(30);

                            modData.ScaleModifier = v3;
                            modData.LengthModifier = len;
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
        }
    }
}

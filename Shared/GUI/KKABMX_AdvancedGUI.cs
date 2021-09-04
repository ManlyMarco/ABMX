using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KKABMX.Core;
using UnityEngine;

namespace KKABMX.GUI
{
    /// <summary>
    /// Advanced bonemod interface, can be used to access all possible sliders and put in unlimited value ranges
    /// </summary>
    internal sealed class KKABMX_AdvancedGUI : MonoBehaviour
    {
        private static KKABMX_AdvancedGUI _instance;

        private static BoneController _currentBoneController;
        public static bool Enabled => _currentBoneController != null && _instance.enabled;
        public static Action<bool> OnEnabledChanged;
        public static void Enable(BoneController controller)
        {
            _currentBoneController = controller;
            _instance.enabled = controller != null;
            _currentCharacterName = controller != null ? " - " + controller.ChaControl.fileParam.fullname : null;
        }
        public static void Disable()
        {
            _currentBoneController = null;
            _instance.enabled = false;
        }

        private static string _currentCharacterName;

        private static Rect _windowRect = new Rect(20, 220, 705, 600);
        private static readonly GUILayoutOption _gloHeight = GUILayout.Height(23);
        private static readonly GUILayoutOption _gloExpand = GUILayout.ExpandWidth(true);
        private static readonly GUILayoutOption _gloSmallButtonWidth = GUILayout.Width(20);
        private static readonly GUILayoutOption _gloTextfieldWidth = GUILayout.Width(43);
        private static readonly GUILayoutOption _gloSliderWidth = GUILayout.Width(80);
        private static readonly GUILayoutOption[] _gloSuggestionsStyle = { GUILayout.Height(48), GUILayout.MaxHeight(48), GUILayout.ExpandWidth(true) };

        private static GUIStyle _gsButtonReset;
        private static GUIStyle _gsInput;
        private static GUIStyle _gsLabel;

        private static bool _initGui = true;

        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _suggestionScrollPosition = Vector2.zero;

        private readonly HashSet<string> _symmetryBones = new HashSet<string>();
        private readonly HashSet<string> _addedBones = new HashSet<string>();
        private bool _onlyShowAdditional;
        private bool _onlyShowCoords;

        private bool _searchFieldValueChanged = false;
        private string _searchFieldValue = "";
        public string SearchFieldValue
        {
            get => _searchFieldValue;
            set
            {
                if (_searchFieldValue != value)
                {
                    _searchFieldValue = value;
                    _searchFieldValueChanged = true;
                }
            }
        }

        private float _incrementSize = 0.1f;
        private readonly List<BoneModifier> _visibleModifiers = new List<BoneModifier>();
        private List<string> _boneSuggestions;
        private List<string> _getAllPossibleBoneNames;
        private const string SearchControlName = "bsbox";
        private readonly Dictionary<BoneModifier, int> _listItemHeights = new Dictionary<BoneModifier, int>();

        private void Awake()
        {
            _instance = this;
            enabled = false;
        }

        private void Update()
        {
            if (!Enabled)
                enabled = false;
            else
                //todo trigger update only when needed?
                RefreshBoneInfo(false);
        }

        private void OnEnable()
        {
            if (_currentBoneController == null)
            {
                enabled = false;
            }
            else
            {
                _getAllPossibleBoneNames = _currentBoneController.GetAllPossibleBoneNames().OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
                RefreshBoneInfo(true);
                OnEnabledChanged?.Invoke(true);
            }
        }

        private void OnDisable()
        {
            OnEnabledChanged?.Invoke(false);
        }

        private void RefreshBoneInfo(bool force)
        {
            var filteredModifiers = _currentBoneController.Modifiers.AsEnumerable();

            if (_onlyShowAdditional)
                filteredModifiers = filteredModifiers.Where(x => _addedBones.Contains(x.BoneName));
            if (_onlyShowCoords)
                filteredModifiers = filteredModifiers.Where(x => x.IsCoordinateSpecific());

            if (!string.IsNullOrEmpty(SearchFieldValue))
                filteredModifiers = filteredModifiers.Where(x =>
                    x.BoneName.IndexOf(SearchFieldValue, StringComparison.OrdinalIgnoreCase) >= 0);

            _visibleModifiers.Clear();
            _visibleModifiers.AddRange(filteredModifiers.OrderBy(x => x.BoneName, StringComparer.OrdinalIgnoreCase));

            if (force || _searchFieldValueChanged)
            {
                _boneSuggestions = string.IsNullOrEmpty(SearchFieldValue)
                    ? _getAllPossibleBoneNames
                    : _getAllPossibleBoneNames.Where(x => x.IndexOf(SearchFieldValue, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                _searchFieldValueChanged = false;
            }
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

                _windowRect.x = Mathf.Min(Screen.width - _windowRect.width, Mathf.Max(0, _windowRect.x));
                _windowRect.y = Mathf.Min(Screen.height - _windowRect.height, Mathf.Max(0, _windowRect.y));

                _initGui = false;
            }

            if (!KKABMX_Core.TransparentAdvancedWindow.Value)
                KKAPI.Utilities.IMGUIUtils.DrawSolidBox(_windowRect);

            var newRect = GUILayout.Window(1724, _windowRect, DrawWindowContents, "Advanced Bone Sliders" + _currentCharacterName);
            // Prevent window resizing if content overflows
            _windowRect.x = newRect.x;
            _windowRect.y = newRect.y;

            // Prevent clicks from going through
            if (_windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                Input.ResetInputAxes();
        }

        private void DrawWindowContents(int id)
        {
            GUILayout.BeginVertical();
            {
                DrawHeader();

                UnityEngine.GUI.changed = false;

                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true, _gloExpand);
                {
                    var totalHeight = 0;
                    var drawAll = false;

                    foreach (var mod in _visibleModifiers)
                    {
                        // Virtualize the list
                        if (!drawAll && _listItemHeights.TryGetValue(mod, out var thisHeight) && thisHeight > 0)
                        {
                            totalHeight += thisHeight;
                            if (totalHeight - thisHeight > _scrollPosition.y + _windowRect.height || totalHeight < _scrollPosition.y)
                            {
                                GUILayout.Space(thisHeight);
                                continue;
                            }
                        }
                        else
                        {
                            // If any heights were not measured then draw everything once
                            drawAll = true;
                        }

                        DrawSingleModifier(mod);

                        // Can only get heights during repaint
                        if (Event.current.type == EventType.Repaint)
                        {
                            var height = GUILayoutUtility.GetLastRect().height;
                            if (height > 0) _listItemHeights[mod] = Mathf.CeilToInt(height);
                        }
                    }

                    if (_visibleModifiers.Count == 0)
                    {
                        GUILayout.Label(_currentBoneController.Modifiers.Count == 0
                            ? "No bone modifiers to show. You can add new modifiers by using the search box above, or by using the yellow in-game sliders in maker."
                            : "No bone modifiers were found. Change your search parameters or add a new bone above.");
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndScrollView();

                if (UnityEngine.GUI.changed)
                    _currentBoneController.NeedsBaselineUpdate = true;
            }
            GUILayout.EndVertical();

            UnityEngine.GUI.DragWindow();
        }

        private void DrawSingleModifier(BoneModifier mod)
        {
            var anyChanged = UnityEngine.GUI.changed;

            GUILayout.BeginVertical(UnityEngine.GUI.skin.box);
            {
#if KK || KKS
                var currentCoordinate = (ChaFileDefine.CoordinateType)_currentBoneController.ChaControl.fileStatus.coordinateType;
#elif EC
                var currentCoordinate = KoikatsuCharaFile.ChaFileDefine.CoordinateType.School01;
#elif AI || HS2
                var currentCoordinate = CoordinateType.Unknown;
#endif
                var modData = mod.GetModifier(currentCoordinate);

                BoneModifierData linkedModData = null;

                GUILayout.BeginHorizontal(_gloExpand);
                {
                    GUILayout.TextField(mod.BoneName, _gsLabel);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("X", _gsButtonReset, _gloSmallButtonWidth))
                        _currentBoneController.RemoveModifier(mod);

                    var counterBoneName = GetCounterBoneName(mod);
                    if (counterBoneName != null)
                    {
                        var currLink = _symmetryBones.Contains(mod.BoneName);
                        var link = GUILayout.Toggle(currLink, "Link R/L bones");

                        if (currLink != link)
                        {
                            if (link)
                            {
                                _symmetryBones.Add(mod.BoneName);
                                _symmetryBones.Add(counterBoneName);
                            }
                            else
                            {
                                _symmetryBones.Remove(mod.BoneName);
                                _symmetryBones.Remove(counterBoneName);
                            }
                        }

                        if (link)
                        {
                            var linkedBone = _currentBoneController.GetModifier(counterBoneName);
                            if (linkedBone == null)
                            {
                                linkedBone = new BoneModifier(counterBoneName);
                                _currentBoneController.AddModifier(linkedBone);
                                if (linkedBone.BoneTransform == null)
                                {
                                    KKABMX_Core.Logger.LogMessage($"Failed to add bone {counterBoneName}, make sure the name is correct.");
                                    _currentBoneController.Modifiers.Remove(linkedBone);
                                    linkedBone = null;
                                    _symmetryBones.Remove(mod.BoneName);
                                    _symmetryBones.Remove(counterBoneName);
                                }
                            }

                            if (linkedBone != null) linkedModData = linkedBone.GetModifier(currentCoordinate);
                        }
                    }

#if !EC && !AI && !HS2
                    if (GUILayout.Toggle(mod.IsCoordinateSpecific(), "Per coordinate", GUILayout.ExpandWidth(false)))
                        mod.MakeCoordinateSpecific(_currentBoneController.ChaFileControl.coordinate.Length);
                    else
                        mod.MakeNonCoordinateSpecific();
#endif

                    GUILayout.Space(8);

                    // Length slider
                    var lengthModifier = modData.LengthModifier;
                    UnityEngine.GUI.changed = false;

                    GUILayout.Label("Length:", GUILayout.ExpandWidth(false));

                    DrawSingleSlider(null, ref lengthModifier, -2, 2);

                    if (GUILayout.Button("0", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) lengthModifier = 1;

                    if (UnityEngine.GUI.changed)
                    {
                        modData.LengthModifier = lengthModifier;
                        if (linkedModData != null) linkedModData.LengthModifier = lengthModifier;
                        anyChanged = true;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(_gloExpand);
                {
                    // Scale sliders
                    var scale = modData.ScaleModifier;
                    UnityEngine.GUI.changed = false;

                    GUILayout.Label("Scale", _gloExpand);

                    DrawSingleSlider("X:", ref scale.x, 0, 2);
                    DrawSingleSlider("Y:", ref scale.y, 0, 2);
                    DrawSingleSlider("Z:", ref scale.z, 0, 2);

                    if (GUILayout.Button("0", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) scale = Vector3.one;

                    if (UnityEngine.GUI.changed)
                    {
                        modData.ScaleModifier = scale;
                        if (linkedModData != null) linkedModData.ScaleModifier = scale;
                        anyChanged = true;
                    }
                }
                GUILayout.EndHorizontal();

                if (!KKABMX_Core.NoRotationBones.Contains(mod.BoneName))
                {
                    GUILayout.BeginHorizontal(_gloExpand);
                    {
                        // Position sliders
                        var position = modData.PositionModifier;
                        UnityEngine.GUI.changed = false;

                        GUILayout.Label("Offset", _gloExpand);

                        DrawSingleSlider("X:", ref position.x, -1, 1);
                        DrawSingleSlider("Y:", ref position.y, -1, 1);
                        DrawSingleSlider("Z:", ref position.z, -1, 1);

                        if (GUILayout.Button("0", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) position = Vector3.zero;

                        if (UnityEngine.GUI.changed)
                        {
                            modData.PositionModifier = position;
                            if (linkedModData != null) linkedModData.PositionModifier = new Vector3(position.x * -1, position.y, position.z);
                            anyChanged = true;
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(_gloExpand);
                    {
                        // Rotation sliders
                        var rotation = modData.RotationModifier;
                        UnityEngine.GUI.changed = false;

                        GUILayout.Label("Tilt", _gloExpand);

                        DrawSingleSlider("X:", ref rotation.x, -180, 180);
                        DrawSingleSlider("Y:", ref rotation.y, -180, 180);
                        DrawSingleSlider("Z:", ref rotation.z, -180, 180);

                        if (GUILayout.Button("0", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) rotation = Vector3.zero;

                        if (UnityEngine.GUI.changed)
                        {
                            modData.RotationModifier = rotation;
                            if (linkedModData != null) linkedModData.RotationModifier = new Vector3(rotation.x, rotation.y * -1, rotation.z * -1);
                            anyChanged = true;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            if (anyChanged) UnityEngine.GUI.changed = true;
        }

        private void DrawSingleSlider(string sliderName, ref float value, float minValue, float maxValue)
        {
            if (sliderName != null)
                GUILayout.Label(sliderName, GUILayout.Width(14));

            value = GUILayout.HorizontalSlider(value, minValue, maxValue, _gsButtonReset, _gsButtonReset, _gloSliderWidth, _gloHeight);

            float.TryParse(GUILayout.TextField(value.ToString(maxValue >= 100 ? "F1" : "F3", CultureInfo.InvariantCulture), _gsInput, _gloTextfieldWidth, _gloHeight), out value);

            if (GUILayout.Button("-", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) value -= _incrementSize;
            if (GUILayout.Button("+", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) value += _incrementSize;
        }

        private static string GetCounterBoneName(BoneModifier mod)
        {
            if (mod.BoneName.EndsWith("R", StringComparison.Ordinal))
                return mod.BoneName.Remove(mod.BoneName.Length - 1) + "L";
            if (mod.BoneName.EndsWith("L", StringComparison.Ordinal))
                return mod.BoneName.Remove(mod.BoneName.Length - 1) + "R";
            if (mod.BoneName.EndsWith("R_00", StringComparison.Ordinal))
                return mod.BoneName.Remove(mod.BoneName.Length - 4) + "L_00";
            if (mod.BoneName.EndsWith("L_00", StringComparison.Ordinal))
                return mod.BoneName.Remove(mod.BoneName.Length - 4) + "R_00";
            return null;
        }

        private void DrawHeader()
        {
            void AddNewBone(string boneName)
            {
                if (string.IsNullOrEmpty(boneName)) return;

                _addedBones.Add(boneName);

                if (_currentBoneController.GetModifier(boneName) != null)
                {
                    KKABMX_Core.Logger.LogMessage($"Bone {boneName} is already added.");
                }
                else
                {
                    var newMod = new BoneModifier(boneName);
                    _currentBoneController.AddModifier(newMod);
                    if (newMod.BoneTransform == null)
                    {
                        KKABMX_Core.Logger.LogMessage($"Failed to add bone {boneName}, make sure the name is correct.");
                        _currentBoneController.Modifiers.Remove(newMod);
                    }
                    else
                    {
                        KKABMX_Core.Logger.LogMessage($"Added bone {boneName} successfully. Modify it to make it save.");
                    }
                }
            }

            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            {
                GUILayout.Label("Search:", GUILayout.ExpandWidth(false));

                UnityEngine.GUI.SetNextControlName(SearchControlName);
                SearchFieldValue = GUILayout.TextField(SearchFieldValue, _gloExpand);

                if (SearchControlName.Equals(UnityEngine.GUI.GetNameOfFocusedControl(), StringComparison.Ordinal))
                {
                    var currentEvent = Event.current;
                    if (currentEvent.isKey && (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter))
                    {
                        currentEvent.Use();
                        AddNewBone(SearchFieldValue);
                    }
                }

                if (string.IsNullOrEmpty(SearchFieldValue))
                    UnityEngine.GUI.enabled = false;
                if (GUILayout.Button("Add new", GUILayout.ExpandWidth(false)))
                {
                    AddNewBone(SearchFieldValue);
                    UnityEngine.GUI.FocusControl(SearchControlName);
                }
                UnityEngine.GUI.enabled = true;

                if (GUILayout.Button("Revert", GUILayout.ExpandWidth(false))) _currentBoneController.RevertChanges();

                _onlyShowAdditional = GUILayout.Toggle(_onlyShowAdditional, "Only new", GUILayout.ExpandWidth(false));

#if !AI
                _onlyShowCoords = GUILayout.Toggle(_onlyShowCoords, "Only per coordinate", GUILayout.ExpandWidth(false));
#endif

                GUILayout.Space(6);

                GUILayout.Label("Increment:", GUILayout.ExpandWidth(false));

                float RoundToPowerOf10(float value) => Mathf.Pow(10, Mathf.Round(Mathf.Log10(value)));

                float.TryParse(GUILayout.TextField(_incrementSize.ToString(CultureInfo.InvariantCulture), _gsInput, _gloTextfieldWidth, _gloHeight), out _incrementSize);
                if (GUILayout.Button("-", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) _incrementSize = RoundToPowerOf10(_incrementSize * 0.1f);
                if (GUILayout.Button("+", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) _incrementSize = RoundToPowerOf10(_incrementSize * 10f);

                GUILayout.Space(4);
                if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) enabled = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            {
                _suggestionScrollPosition = GUILayout.BeginScrollView(_suggestionScrollPosition, true, false, _gloSuggestionsStyle);
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
                    {
                        const int singleItemWidth = 120;
                        var leftItemCount = Mathf.FloorToInt(_suggestionScrollPosition.x / singleItemWidth);
                        GUILayout.Space(leftItemCount * singleItemWidth);

                        var shownItemCount = Mathf.CeilToInt(_windowRect.width / singleItemWidth) + 1; // Add one more to account for spaces between buttons
                        foreach (var boneResult in _boneSuggestions.Skip(leftItemCount).Take(shownItemCount))
                        {
                            if (GUILayout.Button(boneResult, _gsButtonReset, GUILayout.Width(singleItemWidth), _gloHeight))
                            {
                                if (_currentBoneController.GetModifier(boneResult) == null)
                                    AddNewBone(boneResult);

                                UnityEngine.GUI.FocusControl(SearchControlName);
                            }
                        }

                        var rightItemCount = Mathf.Max(0, _boneSuggestions.Count - (leftItemCount + shownItemCount));
                        GUILayout.Space(rightItemCount * singleItemWidth);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }
    }
}

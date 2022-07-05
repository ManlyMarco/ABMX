using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KKABMX.Core;
using KKAPI.Utilities;
using UnityEngine;

namespace KKABMX.GUI
{
   
    /// <summary>
    /// Advanced bonemod interface, can be used to access all possible sliders and put in unlimited value ranges
    /// </summary>
    internal sealed class KKABMX_AdvancedGUI : MonoBehaviour
    {
        private static KKABMX_AdvancedGUI _instance;

        private static string _windowTitle;
        private static Rect _windowRect = new Rect(20, 220, 705, 600);

        private static float _objectTreeHeight => _windowRect.height - 100; //todo properly calc or get
        private int _singleObjectTreeItemHeight;
        private Vector2 _treeScrollPosition = Vector2.zero;
        private Vector2 _slidersScrollPosition = Vector2.zero;

        private static GUIStyle _gsButtonReset;
        private static GUIStyle _gsInput;
        private static GUIStyle _gsLabel;

        private static readonly GUILayoutOption _gloExpand = GUILayout.ExpandWidth(true);
        private static readonly GUILayoutOption _gloSmallButtonWidth = GUILayout.Width(20);
        private static readonly GUILayoutOption _gloHeight = GUILayout.Height(23);

        private static readonly Color _dangerColor = new Color(1, 0.4f, 0.4f, 1);
        private static readonly Color _warningColor = new Color(1, 1, 0.7f, 1);

        private readonly HashSet<BoneModifier> _changedBones = new HashSet<BoneModifier>();
        private readonly HashSet<GameObject> _openedObjects = new HashSet<GameObject>();

        public static Transform SelectedTransform { get; set; }
        private static BoneController _currentBoneController;
        private static ChaControl _currentChaControl;

        private static readonly float[] _defaultIncrementSize = { 0.1f, 0.1f, 0.01f, 1f };
        private static readonly float[] _incrementSize = _defaultIncrementSize.ToArray();

        private BoneModifierData[] _copiedModifier;
        private bool _editSymmetry = true;

        private bool _onlyShowCoords;
        private bool _onlyShowModified;
        private bool _onlyShowNewChanges;

        private List<Transform> _searchResults;
        private string _searchFieldValue = "";
        private bool _searchFieldValueChanged;

        public string SearchFieldValue
        {
            get => _searchFieldValue;
            set
            {
                if (value == null) value = "";
                if (_searchFieldValue != value)
                {
                    _searchFieldValue = value;
                    _searchFieldValueChanged = true;
                    _searchResults = value.Length == 0 ? null : _currentChaControl.objTop.GetComponentsInChildren<Transform>(true).Where(CheckSearchMatch).ToList();
                }
            }
        }

        public static Action<bool> OnEnabledChanged;
        public static bool Enabled => _currentBoneController != null && _instance.enabled;

        public static void Enable(BoneController controller)
        {
            if (controller == null)
                Disable();
            else
            {
                _currentBoneController = controller;
                _instance.enabled = true;
                _currentChaControl = controller.ChaControl;

                var charaName = controller.ChaControl.fileParam.fullname;
                TranslationHelper.TryTranslate(charaName, out var charaNameTl);
                _windowTitle = $"Advanced Bone Sliders [{charaNameTl ?? charaName}]";
            }
        }

        public static void Disable()
        {
            _currentBoneController = null;
            _instance.enabled = false;
            _instance._changedBones.Clear();
        }

        private bool CheckSearchMatch(Transform tr)
        {
            return tr.name.IndexOf(_searchFieldValue, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Awake()
        {
            _instance = this;
            enabled = false;
        }

        private void OnEnable()
        {
            if (_currentBoneController == null)
                enabled = false;
            else
                //_getAllPossibleBoneNames = _currentBoneController.GetAllPossibleBoneNames().OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
                //RefreshBoneInfo(true);
                OnEnabledChanged?.Invoke(true);
        }

        private void OnDisable()
        {
            OnEnabledChanged?.Invoke(false);
        }

        private void OnGUI()
        {
            if (_currentBoneController == null)
            {
                enabled = false;
                return;
            }

            if (_gsInput == null)
            {
                _gsInput = new GUIStyle(UnityEngine.GUI.skin.textArea);
                _gsLabel = new GUIStyle(UnityEngine.GUI.skin.label);
                _gsButtonReset = new GUIStyle(UnityEngine.GUI.skin.button);
                _gsLabel.alignment = TextAnchor.MiddleRight;
                _gsLabel.normal.textColor = Color.white;

                _windowRect.x = Mathf.Min(Screen.width - _windowRect.width, Mathf.Max(0, _windowRect.x));
                _windowRect.y = Mathf.Min(Screen.height - _windowRect.height, Mathf.Max(0, _windowRect.y));
            }

            var skin = UnityEngine.GUI.skin;

            //if (!KKABMX_Core.TransparentAdvancedWindow.Value)
            UnityEngine.GUI.skin = IMGUIUtils.SolidBackgroundGuiSkin;

            _windowRect = GUILayout.Window(1724, _windowRect, DrawWindowContents, _windowTitle);

            UnityEngine.GUI.skin = skin;
        }

        private void DrawWindowContents(int id)
        {
            GUILayout.BeginVertical();
            {
                //DrawHeader();

                UnityEngine.GUI.changed = false;


                // |------|-------|
                // |      |options|
                // |      |-------|
                // |      |       |
                // |      |       |
                // |------|       |
                // |search|       |
                // |------|-------|

                GUILayout.BeginHorizontal();
                {
                    //todo save format - add new field for "target" = body/acc123..., legacy fallback to body if empty
                    // todo need to gather all body bones and then separately accessory bones and list them separately as root objects
                    // list face as root object too but leave it also int he body tree

                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(_windowRect.width / 2), GUILayout.ExpandHeight(true));
                    {
                        // Search box and filters
                        GUILayout.BeginVertical(UnityEngine.GUI.skin.box);
                        {
                            GUILayout.BeginHorizontal();
                            {
                                UnityEngine.GUI.changed = false;
                                UnityEngine.GUI.SetNextControlName("sbox");
                                if (_searchFieldValueChanged && Event.current.type == EventType.Repaint)
                                {
                                    _searchFieldValueChanged = false;
                                    UnityEngine.GUI.FocusControl("sbox");
                                }

                                var showTipString = SearchFieldValue.Length == 0 && UnityEngine.GUI.GetNameOfFocusedControl() != "sbox";
                                //if(showTipString) UnityEngine.GUI.color = Color.gray; - grays the whole box, no easy way to only gray the label
                                var newVal = GUILayout.TextField(showTipString ? "Search..." : SearchFieldValue, _gloExpand);
                                if (UnityEngine.GUI.changed)
                                    SearchFieldValue = newVal;
                                //UnityEngine.GUI.color = Color.white;

                                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                                {
                                    SearchFieldValue = "";
                                    UnityEngine.GUI.FocusControl("");
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Only");
                                UnityEngine.GUI.color = Color.green;
                                _onlyShowModified = GUILayout.Toggle(_onlyShowModified, "modified", GUILayout.ExpandWidth(false));
                                UnityEngine.GUI.color = Color.white;
                                _onlyShowNewChanges = GUILayout.Toggle(_onlyShowNewChanges, "new", GUILayout.ExpandWidth(false));
#if !AI
                                UnityEngine.GUI.color = Color.yellow;
                                _onlyShowCoords = GUILayout.Toggle(_onlyShowCoords, "per coordinate", GUILayout.ExpandWidth(false));
#endif
                                UnityEngine.GUI.color = Color.white;
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();

                            //todo show only body/head/acc
                        }
                        GUILayout.EndVertical();


                        // Bone list
                        _treeScrollPosition = GUILayout.BeginScrollView(_treeScrollPosition, false, true, GUILayout.ExpandHeight(true));
                        {
                            //todo 
                            // pin bones to have them show up at root level persistently

                            var currentCount = 0;
                            if (_onlyShowModified || _onlyShowCoords || _onlyShowNewChanges)
                            {
                                foreach (var boneModifier in _currentBoneController.Modifiers)
                                {
                                    if (boneModifier.BoneTransform != null && !boneModifier.IsEmpty())
                                    {
                                        if ((!_onlyShowCoords || boneModifier.IsCoordinateSpecific()) &&
                                            (!_onlyShowNewChanges || _changedBones.Contains(boneModifier)))
                                        {
                                            if (_searchFieldValue.Length == 0 || CheckSearchMatch(boneModifier.BoneTransform))
                                                DisplayObjectTreeHelper(boneModifier.BoneTransform.gameObject, 0, ref currentCount);
                                        }
                                    }
                                }
                            }
                            else if (_searchResults != null)
                            {
                                foreach (var searchResult in _searchResults)
                                    DisplayObjectTreeHelper(searchResult.gameObject, 0, ref currentCount);
                            }
                            else
                            {
                                DisplayObjectTreeHelper(_currentChaControl.objTop, 0, ref currentCount);
                                DisplayObjectTreeHelper(_currentChaControl.objHeadBone, 0, ref currentCount);
                                foreach (var rootGameObject in _currentChaControl.objAccessory)
                                {
                                    if (rootGameObject != null)
                                        DisplayObjectTreeHelper(rootGameObject, 0, ref currentCount);
                                }
                            }

                            if (currentCount == 0)
                            {
                                GUILayout.Label(_searchResults != null
                                                    ? "No bones matching the search parameters were found. Make sure you've typed the bone name correctly and that other filters aren't interfering."
                                                    : "No bone modifiers to show, to add a new modifier simply click on a bone and edit any of the sliders.");
                            }
                        }
                        GUILayout.EndScrollView();


                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Collapse all", _gloExpand))
                                _openedObjects.Clear();
                            if (GUILayout.Button("Expand all", _gloExpand))
                                Array.ForEach(_currentBoneController.gameObject.GetComponentsInChildren<Transform>(), child => _openedObjects.Add(child.gameObject));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            //if (GUILayout.Button("Export", _gloExpand))
                            //{
                            //    //todo proper
                            //    var toSave = _currentBoneController.Modifiers.Where(x => !x.IsEmpty())
                            //                                       .Select(x => new MyClass { BoneName = x.BoneName, CoordinateModifiers = x.CoordinateModifiers })
                            //                                       .ToList(); //.ToDictionary(x => x.BoneName, x => x.CoordinateModifiers);
                            //
                            //    using (var w = new StringWriter())
                            //    {
                            //        new XmlSerializer(toSave.GetType()).Serialize(w, toSave);
                            //        Console.WriteLine(w.ToString());
                            //    }
                            //}
                            //
                            //if (GUILayout.Button("Import", _gloExpand)) ; //todo

                            UnityEngine.GUI.color = _dangerColor;

                            if (GUILayout.Button("Revert", _gloExpand))
                                _currentBoneController.RevertChanges();

                            if (GUILayout.Button("Clear", _gloExpand))
                            {
                                _currentBoneController.Modifiers.ForEach(modifier => modifier.Reset());
                                _currentBoneController.Modifiers.Clear();
                                _currentChaControl.updateShapeFace = true;
                                _currentChaControl.updateShapeBody = true;
                                _changedBones.Clear();
                                SelectedTransform = null;
                            }

                            UnityEngine.GUI.color = Color.white;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    // Sliders
                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box, _gloExpand, GUILayout.ExpandHeight(true));
                    {
                        var mod = SelectedTransform == null ? null : GetOrAddBoneModifier(SelectedTransform.name);

                        // Slider list
                        _slidersScrollPosition = GUILayout.BeginScrollView(_slidersScrollPosition, false, false, GUILayout.ExpandHeight(true));
                        {
                            if (SelectedTransform == null)
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("Select a bone transform on the left to show available controls.");
                                GUILayout.FlexibleSpace();
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.FlexibleSpace();
                                    GUILayout.Label("Current bone:");
                                    GUILayout.Label(SelectedTransform.name);
                                    GUILayout.FlexibleSpace();
                                }
                                GUILayout.EndHorizontal();

                                var counterBone = GetCounterBoneName(mod);
                                if (counterBone == null) UnityEngine.GUI.enabled = false;
                                var otherMod = _editSymmetry && counterBone != null ? GetOrAddBoneModifier(counterBone) : null;
                                if (_editSymmetry) UnityEngine.GUI.color = _warningColor;
                                _editSymmetry = GUILayout.Toggle(_editSymmetry, "Edit both left and right side bones");
                                UnityEngine.GUI.color = Color.white;
                                GUILayout.Label("Other side bone: " + (counterBone ?? "No bone found"));
                                UnityEngine.GUI.enabled = true;

                                GUILayout.Space(5);

                                UnityEngine.GUI.changed = false;
                                var oldVal = mod.IsCoordinateSpecific();
                                if (oldVal) UnityEngine.GUI.color = _warningColor;
                                var newval = GUILayout.Toggle(oldVal, "Use different values for each coordinate");
                                UnityEngine.GUI.color = Color.white;
                                if (UnityEngine.GUI.changed)
                                {
                                    if (newval) mod.MakeCoordinateSpecific(_currentChaControl.chaFile.coordinate.Length);
                                    else mod.MakeNonCoordinateSpecific();
                                }

                                if (otherMod != null && otherMod.IsCoordinateSpecific() != newval)
                                {
                                    if (newval) otherMod.MakeCoordinateSpecific(_currentChaControl.chaFile.coordinate.Length);
                                    else otherMod.MakeNonCoordinateSpecific();
                                }

                                GUILayout.Space(10);

                                DrawSliders(mod, otherMod);
                            }
                        }
                        GUILayout.EndScrollView();

                        // Slider options
                        GUILayout.BeginVertical();
                        {
                            // Toolbar
                            GUILayout.BeginHorizontal();
                            {
                                if (mod != null)
                                {
                                    if (mod.IsEmpty()) UnityEngine.GUI.enabled = false;
                                    if (GUILayout.Button("Copy", _gloExpand))
                                        _copiedModifier = mod.CoordinateModifiers.Select(x => x.Clone()).ToArray();
                                    UnityEngine.GUI.enabled = true;

                                    if (_copiedModifier == null) UnityEngine.GUI.enabled = false;
                                    if (GUILayout.Button("Paste", _gloExpand))
                                        mod.CoordinateModifiers = _copiedModifier.Select(x => x.Clone()).ToArray();
                                    UnityEngine.GUI.enabled = true;

                                    UnityEngine.GUI.color = _dangerColor;
                                    if (mod.IsEmpty()) UnityEngine.GUI.enabled = false;
                                    if (GUILayout.Button("Remove", _gloExpand))
                                    {
                                        _changedBones.Remove(mod);
                                        _currentBoneController.RemoveModifier(mod);
                                        SelectedTransform = null;
                                    }

                                    UnityEngine.GUI.color = Color.white;
                                    UnityEngine.GUI.enabled = true;

                                    GUILayout.Space(10);
                                }
                                else
                                    GUILayout.FlexibleSpace();

                                if (GUILayout.Button(" Close ", GUILayout.ExpandWidth(false)))
                                    enabled = false;
                            }
                            GUILayout.EndHorizontal();

                            // Selection grid for gizmo pos/rot/scl? Use the all at once mode? Hotkeys?
                            //todo GUILayout.Toggle(false, "Show gizmo", _gloExpand);
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                if (UnityEngine.GUI.changed)
                    _currentBoneController.NeedsBaselineUpdate = true;
            }
            GUILayout.EndVertical();

            _windowRect = IMGUIUtils.DragResizeEatWindow(id, _windowRect);
        }

        private BoneModifier GetOrAddBoneModifier(string boneName)
        {
            if (boneName == null) throw new ArgumentNullException(nameof(boneName));
            var mod = _currentBoneController.GetModifier(boneName);
            if (mod == null)
            {
                mod = new BoneModifier(boneName);
                _currentBoneController.AddModifier(mod);
                _changedBones.Add(mod);
            }
            else if (mod.IsEmpty()) _changedBones.Add(mod);

            return mod;
        }

        private void DrawSliders(BoneModifier mod, BoneModifier linkedMod)
        {
            var coordinateType = (ChaFileDefine.CoordinateType)_currentChaControl.fileStatus.coordinateType;
            var modData = mod.GetModifier(coordinateType);
            var linkedModData = linkedMod?.GetModifier(coordinateType);

            var anyChanged = false;

            GUILayout.BeginVertical();

            // Length slider
            var lengthModifier = modData.LengthModifier;
            if (DrawSingleSlider("Length:", ref lengthModifier, -2, 2, 1, 0))
            {
                modData.LengthModifier = lengthModifier;
                if (linkedModData != null) linkedModData.LengthModifier = lengthModifier;
                anyChanged = true;
            }

            DrawIncrementControl(0);

            GUILayout.Space(10);

            // Scale sliders
            var scale = modData.ScaleModifier;
            //if (GUILayout.Button("0", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) scale = Vector3.one;
            if (DrawSingleSlider("Scale X:", ref scale.x, 0, 2, 1, 1) |
                DrawSingleSlider("Scale Y:", ref scale.y, 0, 2, 1, 1) |
                DrawSingleSlider("Scale Z:", ref scale.z, 0, 2, 1, 1))
            {
                modData.ScaleModifier = scale;
                if (linkedModData != null) linkedModData.ScaleModifier = scale;
                anyChanged = true;
            }

            DrawIncrementControl(1);

            //if (!KKABMX_Core.NoRotationBones.Contains(mod.BoneName)) //todo
            {
                GUILayout.Space(10);

                // Position sliders
                var position = modData.PositionModifier;
                //if (GUILayout.Button("0", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) position = Vector3.zero;
                if (DrawSingleSlider("Offset X:", ref position.x, -1, 1, 0, 2) |
                    DrawSingleSlider("Offset Y:", ref position.y, -1, 1, 0, 2) |
                    DrawSingleSlider("Offset Z:", ref position.z, -1, 1, 0, 2))
                {
                    modData.PositionModifier = position;
                    if (linkedModData != null) linkedModData.PositionModifier = new Vector3(position.x * -1, position.y, position.z);
                    anyChanged = true;
                }

                DrawIncrementControl(2);

                GUILayout.Space(10);

                // Rotation sliders
                var rotation = modData.RotationModifier;
                //if (GUILayout.Button("0", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) rotation = Vector3.zero;
                if (DrawSingleSlider("Tilt X:", ref rotation.x, -180, 180, 0, 3) |
                    DrawSingleSlider("Tilt Y:", ref rotation.y, -180, 180, 0, 3) |
                    DrawSingleSlider("Tilt Z:", ref rotation.z, -180, 180, 0, 3))
                {
                    modData.RotationModifier = rotation;
                    if (linkedModData != null) linkedModData.RotationModifier = new Vector3(rotation.x, rotation.y * -1, rotation.z * -1);
                    anyChanged = true;
                }

                DrawIncrementControl(3);
            }

            //todo reset all, copy/paste, L/R with the other bone name shown

            GUILayout.EndVertical();

            if (anyChanged)
                UnityEngine.GUI.changed = true;
        }

        private void DrawIncrementControl(int index)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Increment:", GUILayout.Width(70));

                float RoundToPowerOf10(float value)
                {
                    return Mathf.Pow(10, Mathf.Round(Mathf.Log10(value)));
                }

                float.TryParse(GUILayout.TextField(_incrementSize[index].ToString(CultureInfo.InvariantCulture), _gsInput, _gloExpand, _gloHeight), out _incrementSize[index]);
                if (GUILayout.Button("-", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) _incrementSize[index] = RoundToPowerOf10(_incrementSize[index] * 0.1f);
                if (GUILayout.Button("+", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) _incrementSize[index] = RoundToPowerOf10(_incrementSize[index] * 10f);
                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) _incrementSize[index] = _defaultIncrementSize[index];
            }
            GUILayout.EndHorizontal();
        }


        private bool DrawSingleSlider(string sliderName, ref float value, float minValue, float maxValue, float defaultValue, int incrementIndex)
        {
            UnityEngine.GUI.changed = false;
            GUILayout.BeginHorizontal();
            {
                if (sliderName != null)
                    GUILayout.Label(sliderName, GUILayout.Width(70), _gloHeight);

                value = GUILayout.HorizontalSlider(value, minValue, maxValue, _gsButtonReset, _gsButtonReset, _gloExpand, _gloHeight); //todo better style, stock too low height

                float.TryParse(GUILayout.TextField(value.ToString(maxValue >= 100 ? "F1" : "F3", CultureInfo.InvariantCulture), _gsInput, GUILayout.Width(43), _gloHeight),
                               out value);

                if (GUILayout.Button("-", _gsButtonReset, GUILayout.Width(20), _gloHeight)) value -= _incrementSize[incrementIndex];
                if (GUILayout.Button("+", _gsButtonReset, GUILayout.Width(20), _gloHeight)) value += _incrementSize[incrementIndex];

                if (GUILayout.Button("0", _gsButtonReset, _gloSmallButtonWidth, _gloHeight)) value = defaultValue;
            }
            GUILayout.EndHorizontal();
            return UnityEngine.GUI.changed;
        }

        private void DisplayObjectTreeHelper(GameObject go, int indent, ref int currentCount)
        {
            currentCount++;

            var needsHeightMeasure = _singleObjectTreeItemHeight == 0;

            var isVisible = currentCount * _singleObjectTreeItemHeight >= _treeScrollPosition.y &&
                            (currentCount - 1) * _singleObjectTreeItemHeight <= _treeScrollPosition.y + _objectTreeHeight;

            if (isVisible || needsHeightMeasure)
            {
                var c = UnityEngine.GUI.color;
                if (SelectedTransform == go.transform)
                    UnityEngine.GUI.color = Color.cyan;
                //else if (_changedBones.Any(modifier => modifier.BoneTransform == go.transform && !modifier.IsEmpty()))
                //{
                //    UnityEngine.GUI.color = Color.green;
                //}
                else
                {
                    var mod = _currentBoneController.Modifiers.Find(modifier => modifier.BoneTransform == go.transform && !modifier.IsEmpty());
                    if (mod != null) UnityEngine.GUI.color = mod.IsCoordinateSpecific() ? Color.yellow : Color.green;
                }

                if (!go.activeSelf) UnityEngine.GUI.color = new Color(UnityEngine.GUI.color.r, UnityEngine.GUI.color.g, UnityEngine.GUI.color.b, 0.6f);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(indent * 20f);

                    GUILayout.BeginHorizontal();
                    {
                        if (go.transform.childCount != 0)
                        {
                            if (GUILayout.Toggle(_openedObjects.Contains(go), "", GUILayout.ExpandWidth(false)))
                                _openedObjects.Add(go);
                            else
                                _openedObjects.Remove(go);
                        }
                        else
                            GUILayout.Space(20f);

                        if (GUILayout.Button(go.name, UnityEngine.GUI.skin.label, _gloExpand, GUILayout.MinWidth(200)))
                        {
                            if (SelectedTransform == go.transform)
                            {
                                // Toggle on/off
                                if (!_openedObjects.Add(go))
                                    _openedObjects.Remove(go);
                            }
                            else
                                SelectedTransform = go.transform;
                        }

                        UnityEngine.GUI.color = c;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                if (needsHeightMeasure && Event.current.type == EventType.Repaint)
                    _singleObjectTreeItemHeight = Mathf.CeilToInt(GUILayoutUtility.GetLastRect().height);
            }
            else
                GUILayout.Space(_singleObjectTreeItemHeight);

            if (_openedObjects.Contains(go))
            {
                for (var i = 0; i < go.transform.childCount; ++i)
                {
                    var cgo = go.transform.GetChild(i).gameObject;
                    //todo bone controller needs to deal with this as well
                    // dont do it with head bone at all?
                    if (cgo == _currentChaControl.objHeadBone || Array.IndexOf(_currentChaControl.objAccessory, cgo) >= 0)
                        continue;
                    DisplayObjectTreeHelper(cgo, indent + 1, ref currentCount);
                }
            }
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
    }
}

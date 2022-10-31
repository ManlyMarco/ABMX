using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine;

#if AI || HS2
using ChaControl = AIChara.ChaControl;
#endif

namespace KKABMX.Core
{
    // todo support multiple characters, first node on the list
    /// <summary>
    /// Advanced bonemod interface, can be used to access all possible sliders and put in unlimited value ranges
    /// </summary>
    public sealed class KKABMX_AdvancedGUI : ImguiWindow<KKABMX_AdvancedGUI>
    {
        private float _objectTreeHeight => WindowRect.height - 100; //todo properly calc or get
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

        public static KeyValuePair<BoneLocation, Transform> SelectedTransform { get; set; }
        private static BoneController _currentBoneController; //todo support multiple characters
        private static ChaControl _currentChaControl;

        private static readonly float[] _defaultIncrementSize = { 0.1f, 0.1f, 0.01f, 1f };
        private static readonly float[] _incrementSize = _defaultIncrementSize.ToArray();
        private static readonly bool[] _lockXyz = new bool[_defaultIncrementSize.Length];

        //private BoneModifierData[] _copiedModifier;
        private bool _editSymmetry = true;

        private bool _onlyShowCoords;
        private bool _onlyShowModified;
        private bool _onlyShowNewChanges;

        private bool _enableHelp;

        private List<KeyValuePair<BoneLocation, Transform>> _searchResults;
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
                    if (value.Length == 0)
                        _searchResults = null;
                    else
                    {
                        _searchResults = new List<KeyValuePair<BoneLocation, Transform>>();

                        void AddResults(BoneLocation boneLocation)
                        {
                            _searchResults.AddRange(_currentBoneController.BoneSearcher.GetAllBones(boneLocation)
                                                                          .Where(pair => CheckSearchMatch(pair.Key))
                                                                          .Select(x => new KeyValuePair<BoneLocation, Transform>(boneLocation, x.Value.transform)));
                        }
                        AddResults(BoneLocation.BodyTop);
                        for (int i = 0; i < _currentChaControl.objAccessory.Length; i++)
                        {
                            var accObj = _currentChaControl.objAccessory[i];
                            if (accObj != null) AddResults(BoneLocation.Accessory + i);
                        }
                    }
                }
            }
        }

        public static Action<bool> OnEnabledChanged;
        public static bool Enabled => _currentBoneController != null && Instance.enabled;

        public static void Enable(BoneController controller)
        {
            if (controller == null)
                Disable();
            else
            {
                _currentBoneController = controller;
                Instance.enabled = true;
                _currentChaControl = controller.ChaControl;

                var charaName = controller.ChaControl.fileParam.fullname;
                TranslationHelper.TryTranslate(charaName, out var charaNameTl);
                Instance.Title = $"Advanced Bone Sliders [{charaNameTl ?? charaName}]";

                if (_boneTooltips == null)
                {
                    try
                    {
                        // How to generate BoneTooltips.txt
                        // var li = File.ReadAllLines(@"E:\List_of_bones_and_what_they_scale_Open_in_notepad_to_read.py");
                        // var results = li.Where(x => !x.TrimStart().StartsWith("#") && x.Contains("#"))
                        //                 .Select(x => x.Split(new[] { '#' }, 2).Select(x => x.Trim()).ToArray())
                        //                 .SelectMany(x => x[0].Contains("_L") ? new[] { x, new[] { x[0].Replace("_L", "_R"), x[1] } } : new[] { x })
                        //                 .GroupBy(x => x[0])
                        //                 .ToDictionary(x => x.Key, x => x.First()[1]);
                        // File.WriteAllLines(@"E:\BoneTooltips.txt", results.Select(x => x.Key + "|" + x.Value));

                        var t = Encoding.UTF8.GetString(ResourceUtils.GetEmbeddedResource("BoneTooltips.txt"));
                        _boneTooltips = t.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(x => x.Split(new[] { '|' }, 2))
                                         .Where(x => x.Length == 2)
                                         .ToDictionary(x => x[0], x => x[1]);
                    }
                    catch (Exception e)
                    {
                        KKABMX_Core.Logger.LogError("Failed to read BoneTooltips" + e);
                        _boneTooltips = new Dictionary<string, string>();
                    }
                }
            }
        }

        public static void Disable()
        {
            _currentBoneController = null;
            Instance.enabled = false;
            Instance._changedBones.Clear();
        }

        private bool CheckSearchMatch(string transformName)
        {
            return transformName.IndexOf(_searchFieldValue, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected override Rect GetDefaultWindowRect(Rect screenRect)
        {
            return new Rect(20, 220, 705, 600); //todo
        }

        protected override void OnEnable()
        {
            if (_currentBoneController == null)
            {
                enabled = false;
            }
            else
            {
                base.OnEnable();
                //_getAllPossibleBoneNames = _currentBoneController.GetAllPossibleBoneNames().OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
                //RefreshBoneInfo(true);
                OnEnabledChanged?.Invoke(true);
            }
        }

        private void OnDisable()
        {
            OnEnabledChanged?.Invoke(false);
        }

        protected override void OnGUI()
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

                //todo
                //WindowRect.x = Mathf.Min(Screen.width - WindowRect.width, Mathf.Max(0, WindowRect.x));
                //WindowRect.y = Mathf.Min(Screen.height - WindowRect.height, Mathf.Max(0, WindowRect.y));
            }

            var skin = UnityEngine.GUI.skin;

            //if (!KKABMX_Core.TransparentAdvancedWindow.Value)
            UnityEngine.GUI.skin = IMGUIUtils.SolidBackgroundGuiSkin;

            base.OnGUI();

            UnityEngine.GUI.skin = skin;
        }

        protected override void DrawContents()
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

                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(WindowRect.width / 2), GUILayout.ExpandHeight(true));
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

                                // todo reduce len
                                UnityEngine.GUI.color = _enableHelp ? Color.cyan : Color.white;
                                if (GUILayout.Button(new GUIContent("?", @"Bones adjusted with yellow ABMX sliders in maker tabs are automatically added to this window.

Hover over bones in the list below to see notes (only some bones have them). Bones commented with ""ANIMCOR"" are used by animation correction system, adjusting them can cause weird 
BL = Bone length. It's the distance between given bone and its parent. In some cases (like cf_j_spine02/03) effect is pretty obvious, in some not so much. Offset and rotation controls can be used instead, but the effect may be a bit different during animations.effects and glitches.

Adjusting any bone also affects all of its children. To get a sense of what adjusting one bone will do, just look at its children.
# cf_j_neck affects both neck and head since cf_j_head is parented to it, while cf_s_neck only affects the neck itself.
# cf_d_bust01_L affects entire left breast, cf_s_bust01_L only affects root part of it.

Dont be too afraid to move and rotate bones around, sometimes this is the only way to make things look good (broad shoulders for example)
If adjusting cf_d_* / j_s_* / cf_s_* bones seem to do the same thing, use cf_s_* - they are least likely to cause issues.

Things to keep in mind:
- Uneven XYZ scaling of bones with animated child bones (whole arm/leg, torso, cf_j_neck, finger roots) will produce deformities during animations.
- Rotating cf_j_* bones of the body is a bad idea - those are joits used in animation (face bones are usually fine). They may still prove useful in Studio for tweaking static poses.
- To fix face Scale sliders are generally safe to use with no major glitches. Length, offset and rotation sliders can cause glitches and misplaced body parts if used on some bones.
- glithces just reload the character. Body shape glithes (breasts drifting out of place, scewed pelvis etc.) can often only be fixed by restarting character maker."), GUILayout.ExpandWidth(false)))
                                    _enableHelp = !_enableHelp;
                                UnityEngine.GUI.color = Color.white;
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
                                foreach (var kvp in _currentBoneController.ModifierDict)
                                {
                                    foreach (var boneModifier in kvp.Value)
                                    {
                                        if (boneModifier.BoneTransform != null && !boneModifier.IsEmpty())
                                        {
                                            if ((!_onlyShowCoords || boneModifier.IsCoordinateSpecific()) && (!_onlyShowNewChanges || _changedBones.Contains(boneModifier)))
                                            {
                                                if (_searchFieldValue.Length == 0 || CheckSearchMatch(boneModifier.BoneTransform.name))
                                                    DisplayObjectTreeHelper(boneModifier.BoneTransform.gameObject, 0, ref currentCount, kvp.Key);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (_searchResults != null)
                            {
                                foreach (var searchResult in _searchResults)
                                    DisplayObjectTreeHelper(searchResult.Value.gameObject, 0, ref currentCount, searchResult.Key);
                            }
                            else
                            {
                                DisplayObjectTreeHelper(_currentChaControl.objTop, 0, ref currentCount, BoneLocation.BodyTop);
                                DisplayObjectTreeHelper(_currentChaControl.objHeadBone, 0, ref currentCount, BoneLocation.BodyTop);
                                for (var index = 0; index < _currentChaControl.objAccessory.Length; index++)
                                {
                                    var rootGameObject = _currentChaControl.objAccessory[index];
                                    if (rootGameObject != null)
                                        DisplayObjectTreeHelper(rootGameObject, 0, ref currentCount, BoneLocation.Accessory + index);
                                }
                            }

                            if (currentCount == 0)
                            {
                                GUILayout.Label(_searchResults != null
                                                    ? "No bones matching the search parameters were found. Make sure you've typed the bone name correctly and that other filters aren't interfering."
                                                    : "No bone modifiers to show, to add a new modifier simply click on a bone and edit any of the sliders.");
                            }

                            GUILayout.Space(_objectTreeHeight / 3);
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
                            if (GUILayout.Button(new GUIContent("Export", "Export all current modifiers in a human-readable form to clipboard."), _gloExpand))
                            {
                                try
                                {
                                    _currentBoneController.CleanEmptyModifiers();
                                    //todo proper
                                    List<SerializedBoneModifier> toSave = _currentBoneController.ModifierDict.Values.SelectMany(x => x)
                                                                                                .Where(x => !x.IsEmpty())
                                                                                                .Select(x => new SerializedBoneModifier(x))
                                                                                                .ToList();
                                    if (toSave.Count == 0)
                                    {
                                        KKABMX_Core.Logger.LogMessage("There are no modifiers to export. Change some bonemod sliders first and try again.");
                                    }
                                    else
                                    {
                                        using (var w = new StringWriter())
                                        {
                                            new XmlSerializer(typeof(List<SerializedBoneModifier>)).Serialize(w, toSave);
                                            var output = w.ToString();
                                            Console.WriteLine(output);
                                            GUIUtility.systemCopyBuffer = output;
                                            KKABMX_Core.Logger.LogMessage("Exported modifiers to clipboard!");
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    KKABMX_Core.Logger.LogMessage("Failed to export modifiers to clipboard: " + e.Message);
                                    KKABMX_Core.Logger.LogError(e);
                                }
                            }

                            if (GUILayout.Button(new GUIContent("Import", "Import previously exported data from clipboard (copy the exported text in any text editor)."), _gloExpand))
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
                                    {
                                        KKABMX_Core.Logger.LogMessage("Nothing found in clipboard. Copy serialized bonemod data to clipboard and try again.");
                                    }
                                    else
                                    {
                                        using (var r = new StringReader(GUIUtility.systemCopyBuffer))
                                        {
                                            var result = (List<SerializedBoneModifier>)new XmlSerializer(typeof(List<SerializedBoneModifier>)).Deserialize(r);
                                            foreach (var modifier in result)
                                            {
                                                var m = _currentBoneController.GetModifier(modifier.BoneName, modifier.BoneLocation);
                                                if (m == null)
                                                    _currentBoneController.AddModifier(modifier.ToBoneModifier());
                                                else
                                                {
                                                    if (modifier.CoordinateModifiers == null || modifier.CoordinateModifiers.Length < 1)
                                                        throw new ArgumentException("Invalid data", nameof(modifier.CoordinateModifiers));
                                                    m.CoordinateModifiers = modifier.CoordinateModifiers;
                                                }
                                            }
                                            KKABMX_Core.Logger.LogMessage("Imported modifiers from clipboard!");
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    KKABMX_Core.Logger.LogMessage("Failed to import modifiers from clipboard: " + e.Message);
                                    KKABMX_Core.Logger.LogError(e);
                                }
                            }

                            UnityEngine.GUI.color = _dangerColor;
                            if (GUILayout.Button(new GUIContent("Revert", "Reset modifiers to the state from after the current character card was loaded."), _gloExpand))
                                _currentBoneController.RevertChanges();
                            if (GUILayout.Button(new GUIContent("Clear", "Remove all modifiers, even the ones added by using yellow sliders in maker UI."), _gloExpand))
                            {
                                _currentBoneController.ModifierDict.Values.SelectMany(x => x).Do(modifier => modifier.Reset());
                                _currentBoneController.ModifierDict.Clear();
                                _currentChaControl.updateShapeFace = true;
                                _currentChaControl.updateShapeBody = true;
                                _changedBones.Clear();
                                SelectedTransform = default;
                            }

                            UnityEngine.GUI.color = Color.white;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    // Sliders
                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box, _gloExpand, GUILayout.ExpandHeight(true));
                    {
                        var mod = SelectedTransform.Value == null ? null : GetOrAddBoneModifier(SelectedTransform.Value.name, SelectedTransform.Key);

                        // Slider list
                        _slidersScrollPosition = GUILayout.BeginScrollView(_slidersScrollPosition, false, false, GUILayout.ExpandHeight(true));
                        {
                            if (SelectedTransform.Value == null)
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
                                    GUILayout.Label(ToDisplayString(SelectedTransform.Key));
                                    GUILayout.Label(" > ");
                                    GUILayout.TextField(SelectedTransform.Value.name, _gsLabel);
                                    GUILayout.FlexibleSpace();
                                }
                                GUILayout.EndHorizontal();

                                var counterBone = GetCounterBoneName(mod);
                                if (counterBone == null) UnityEngine.GUI.enabled = false;
                                var otherMod = _editSymmetry && counterBone != null ? GetOrAddBoneModifier(counterBone, mod.BoneLocation) : null;
                                if (_editSymmetry) UnityEngine.GUI.color = _warningColor;
                                _editSymmetry = GUILayout.Toggle(_editSymmetry, new GUIContent("Edit both left and right side bones", "Some bones have a symmetrical pair, like left and right elbow. They all end with _L or _R suffix. This setting will let you edit both sides at the same time (two separate bone modifiers are still used)."));
                                UnityEngine.GUI.color = Color.white;
                                GUILayout.Label("Other side bone: " + (counterBone ?? "No bone found"));
                                UnityEngine.GUI.enabled = true;

#if !AI && !HS2
                                GUILayout.Space(5);

                                UnityEngine.GUI.changed = false;
                                var oldVal = mod.IsCoordinateSpecific();
                                if (oldVal) UnityEngine.GUI.color = _warningColor;
                                var newval = GUILayout.Toggle(oldVal, new GUIContent("Use different values for each coordinate", "This will let you set different slider values for each coordinate (outfit slot).\nModifiers set as per-coordinate are saved to coordinate cards (outfit cards) and later loaded from them (they are added to existing modifiers).\nDisabling the option will cause all coordinates to use current slider values."));
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
#endif

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
                                    if (GUILayout.Button(new GUIContent("Copy", "Copy data of this modifier to clipboard so it can be pasted into another modifier, or into any text editor to hand-edit or save for later.\nIf the modifier is per-coordinate, data for all coordinates is copied."), _gloExpand))
                                    {
                                        //_copiedModifier = mod.CoordinateModifiers.Select(x => x.Clone()).ToArray();
                                        try
                                        {
                                            using (var w = new StringWriter())
                                            {
                                                new XmlSerializer(typeof(BoneModifierData[])).Serialize(w, mod.CoordinateModifiers.Select(x => x.Clone()).ToArray());
                                                var output = w.ToString();
                                                Console.WriteLine(output);
                                                GUIUtility.systemCopyBuffer = output;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            KKABMX_Core.Logger.LogError(e);
                                        }
                                    }

                                    UnityEngine.GUI.enabled = true;

                                    if (//_copiedModifier == null && 
                                        string.IsNullOrEmpty(GUIUtility.systemCopyBuffer)) UnityEngine.GUI.enabled = false;
                                    if (GUILayout.Button(new GUIContent("Paste", "Paste modifier data that is currently in the clipboard. To get modifier data to paste use the Copy button, or copy previously saved data in any text editor."), _gloExpand))
                                    {
                                        //if (_copiedModifier != null) mod.CoordinateModifiers = _copiedModifier.Select(x => x.Clone()).ToArray();
                                        //else
                                        {
                                            try
                                            {
                                                if (string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
                                                {
                                                    KKABMX_Core.Logger.LogMessage("Nothing to paste.");
                                                }
                                                else
                                                {
                                                    using (var r = new StringReader(GUIUtility.systemCopyBuffer))
                                                    {
                                                        var result = (BoneModifierData[])new XmlSerializer(typeof(BoneModifierData[])).Deserialize(r);
                                                        if (result == null || result.Length < 1) throw new ArgumentException("Invalid data", nameof(result));
                                                        mod.CoordinateModifiers = result;
                                                        KKABMX_Core.Logger.LogMessage("Imported modifiers from clipboard!");
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                KKABMX_Core.Logger.LogMessage("Failed to import modifiers from clipboard: " + e.Message);
                                                KKABMX_Core.Logger.LogError(e);
                                            }
                                        }
                                    }

                                    UnityEngine.GUI.enabled = true;

                                    UnityEngine.GUI.color = _dangerColor;
                                    if (mod.IsEmpty()) UnityEngine.GUI.enabled = false;
                                    if (GUILayout.Button(new GUIContent("Remove", "Reset and remove this modifier."), _gloExpand))
                                    {
                                        _changedBones.Remove(mod);
                                        _currentBoneController.RemoveModifier(mod);
                                        SelectedTransform = default;
                                    }

                                    UnityEngine.GUI.color = Color.white;
                                    UnityEngine.GUI.enabled = true;
                                }

                                //GUILayout.FlexibleSpace();
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

            if (!_enableHelp) UnityEngine.GUI.tooltip = null;
        }

        private static Dictionary<string, string> _boneTooltips;

        private BoneModifier GetOrAddBoneModifier(string boneName, BoneLocation location)
        {
            if (boneName == null) throw new ArgumentNullException(nameof(boneName));
            var mod = _currentBoneController.GetModifier(boneName, location);
            if (mod == null)
            {
                mod = new BoneModifier(boneName, location);
                _currentBoneController.AddModifier(mod);
                _changedBones.Add(mod);
            }
            else if (mod.IsEmpty()) _changedBones.Add(mod);

            return mod;
        }

        private static void DrawSliders(BoneModifier mod, BoneModifier linkedMod)
        {
#if AI || HS2
            var coordinateType = CoordinateType.Unknown;
#else
            var coordinateType = (ChaFileDefine.CoordinateType)_currentChaControl.fileStatus.coordinateType;
#endif
            var modData = mod.GetModifier(coordinateType);
            var linkedModData = linkedMod?.GetModifier(coordinateType);

            var anyChanged = false;

            GUILayout.BeginVertical();

            GUILayout.BeginVertical(UnityEngine.GUI.skin.box); // Scale sliders ------------------------------------------------------------
            {
                var scale = modData.ScaleModifier;
                if (DrawXyzSliders(sliderName: "Scale", value: ref scale, minValue: 0, maxValue: 2, defaultValue: 1, incrementIndex: 1))
                {
                    modData.ScaleModifier = scale;
                    if (linkedModData != null) linkedModData.ScaleModifier = scale;
                    anyChanged = true;
                }

                DrawIncrementControl(1, true);
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);

            if (KKABMX_Core.NoRotationBones.Contains(mod.BoneName))
            {
                UnityEngine.GUI.color = Color.yellow;
                GUILayout.Label("Warning: This bone has known issues with Tilt and possibly Offset/Length sliders. Use at your own risk.");
                UnityEngine.GUI.color = Color.white;

                GUILayout.Space(5);
            }

            GUILayout.BeginVertical(UnityEngine.GUI.skin.box); // Length slider ------------------------------------------------------------
            {
                var lengthModifier = modData.LengthModifier;
                if (DrawSingleSlider(sliderName: "Length:", value: ref lengthModifier, minValue: -2, maxValue: 2, defaultValue: 1, incrementIndex: 0))
                {
                    modData.LengthModifier = lengthModifier;
                    if (linkedModData != null) linkedModData.LengthModifier = lengthModifier;
                    anyChanged = true;
                }

                DrawIncrementControl(0, false);
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical(UnityEngine.GUI.skin.box); // Position sliders ------------------------------------------------------------
            {
                var position = modData.PositionModifier;
                if (DrawXyzSliders(sliderName: "Offset", value: ref position, minValue: -1, maxValue: 1, defaultValue: 0, incrementIndex: 2))
                {
                    modData.PositionModifier = position;
                    if (linkedModData != null) linkedModData.PositionModifier = new Vector3(position.x * -1, position.y, position.z);
                    anyChanged = true;
                }

                DrawIncrementControl(2, true);
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical(UnityEngine.GUI.skin.box); // Rotation sliders ------------------------------------------------------------
            {
                var rotation = modData.RotationModifier;
                if (DrawXyzSliders(sliderName: "Tilt", value: ref rotation, minValue: -180, maxValue: 180, defaultValue: 0, incrementIndex: 3))
                {
                    modData.RotationModifier = rotation;
                    if (linkedModData != null) linkedModData.RotationModifier = new Vector3(rotation.x, rotation.y * -1, rotation.z * -1);
                    anyChanged = true;
                }

                DrawIncrementControl(3, true);
            }
            GUILayout.EndVertical();

            GUILayout.EndVertical();

            if (anyChanged)
                UnityEngine.GUI.changed = true;
        }

        private static bool DrawXyzSliders(string sliderName, ref Vector3 value, float minValue, float maxValue, float defaultValue, int incrementIndex)
        {
            var x = DrawSingleSlider(sliderName + " X:", ref value.x, minValue, maxValue, defaultValue, incrementIndex);
            var y = DrawSingleSlider(sliderName + " Y:", ref value.y, minValue, maxValue, defaultValue, incrementIndex);
            var z = DrawSingleSlider(sliderName + " Z:", ref value.z, minValue, maxValue, defaultValue, incrementIndex);

            if (_lockXyz[incrementIndex])
            {
                if (x)
                {
                    value.y = value.x;
                    value.z = value.x;
                }
                else if (y)
                {
                    value.x = value.y;
                    value.z = value.y;
                }
                else if (z)
                {
                    value.x = value.z;
                    value.y = value.z;
                }
            }

            return x || y || z;
        }

        private static void DrawIncrementControl(int index, bool showLock)
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

                if (showLock)
                {
                    GUILayout.Space(4);

                    var isLock = _lockXyz[index];
                    if (isLock) UnityEngine.GUI.color = Color.yellow;
                    _lockXyz[index] = GUILayout.Toggle(isLock, "Lock XYZ", GUILayout.ExpandWidth(false));
                    UnityEngine.GUI.color = Color.white;
                }
            }
            GUILayout.EndHorizontal();
        }


        private static bool DrawSingleSlider(string sliderName, ref float value, float minValue, float maxValue, float defaultValue, int incrementIndex)
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

        private void DisplayObjectTreeHelper(GameObject go, int indent, ref int currentCount, BoneLocation location)
        {
            currentCount++;

            var needsHeightMeasure = _singleObjectTreeItemHeight == 0;

            var isVisible = currentCount * _singleObjectTreeItemHeight >= _treeScrollPosition.y &&
                            (currentCount - 1) * _singleObjectTreeItemHeight <= _treeScrollPosition.y + _objectTreeHeight;

            if (isVisible || needsHeightMeasure)
            {
                var c = UnityEngine.GUI.color;
                if (SelectedTransform.Value == go.transform)
                    UnityEngine.GUI.color = Color.cyan;
                //else if (_changedBones.Any(modifier => modifier.BoneTransform == go.transform && !modifier.IsEmpty()))
                //{
                //    UnityEngine.GUI.color = Color.green;
                //}
                else
                {
                    BoneModifier FindModifier(BoneLocation boneLocation, Transform boneTransform)
                    {
                        return _currentBoneController.ModifierDict.TryGetValue(boneLocation, out var list)
                            ? list.Find(modifier => modifier.BoneTransform == boneTransform)
                            : null;
                    }
                    BoneModifier mod;
                    if (location >= BoneLocation.Accessory)
                        mod = FindModifier(location, go.transform);
                    else
                        mod = FindModifier(BoneLocation.BodyTop, go.transform) ?? FindModifier(BoneLocation.Unknown, go.transform);

                    if (mod != null && !mod.IsEmpty()) UnityEngine.GUI.color = mod.IsCoordinateSpecific() ? Color.yellow : Color.green;
                }

                if (!go.activeSelf) UnityEngine.GUI.color = new Color(UnityEngine.GUI.color.r, UnityEngine.GUI.color.g, UnityEngine.GUI.color.b, 0.6f);

                GUILayout.BeginHorizontal();
                {
                    if (indent > 0)
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

                        var goName = indent > 0 ? go.name : $"{go.name} ({ToDisplayString(location)})";
                        string tooltip = null;
                        if (_enableHelp) _boneTooltips.TryGetValue(go.name, out tooltip);
                        if (GUILayout.Button(new GUIContent(goName, tooltip), UnityEngine.GUI.skin.label, _gloExpand, GUILayout.MinWidth(200)))
                        {
                            if (SelectedTransform.Value == go.transform)
                            {
                                // Toggle on/off
                                if (!_openedObjects.Add(go))
                                    _openedObjects.Remove(go);
                            }
                            else
                                SelectedTransform = new KeyValuePair<BoneLocation, Transform>(location, go.transform);
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
                    DisplayObjectTreeHelper(cgo, indent + 1, ref currentCount, location);
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

        private static string ToDisplayString(BoneLocation location)
        {
            if (location == BoneLocation.BodyTop) return "Body";
            if (location < BoneLocation.Unknown) return "Invalid";
            if (location >= BoneLocation.Accessory) return $"Slot {1 + location - BoneLocation.Accessory:00}";
            return location.ToString();
        }
    }

#pragma warning disable CS1591
    public class SerializedBoneModifier
    {
        // Needed for serialization
        public SerializedBoneModifier() { }

        public SerializedBoneModifier(BoneModifier orig)
        {
            BoneName = orig.BoneName;
            BoneLocation = orig.BoneLocation;
            CoordinateModifiers = orig.CoordinateModifiers;
        }

        public string BoneName { get; set; }
        public BoneLocation BoneLocation { get; set; }
        public BoneModifierData[] CoordinateModifiers { get; set; }

        public BoneModifier ToBoneModifier()
        {
            return new BoneModifier(BoneName, BoneLocation, CoordinateModifiers);
        }
    }
#pragma warning restore CS1591
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using HarmonyLib;
using KKABMX.Core;
using KKAPI.Maker;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

#if AI || HS2
using ChaControl = AIChara.ChaControl;
#endif

namespace KKABMX.GUI
{
    /// <summary>
    /// Advanced bonemod interface, can be used to access all possible sliders and put in unlimited value ranges
    /// </summary>
    public sealed class KKABMX_AdvancedGUI : ImguiWindow<KKABMX_AdvancedGUI>
    {
        private float ObjectTreeHeight => WindowRect.height - 100; //todo properly calc or get
        private int _singleObjectTreeItemHeight;
        private Vector2 _treeScrollPosition = Vector2.zero;
        private Vector2 _slidersScrollPosition = Vector2.zero;

        private static GUIStyle _gsButtonReset;
        private static GUIStyle _gsInput;
        private static GUIStyle _gsLabel;

        private static readonly GUILayoutOption _GloExpand = GUILayout.ExpandWidth(true);
        private static readonly GUILayoutOption _GloSmallButtonWidth = GUILayout.Width(20);
        private static readonly GUILayoutOption _GloHeight = GUILayout.Height(23);

        private static readonly Color _DangerColor = new Color(1, 0.4f, 0.4f, 1);
        private static readonly Color _WarningColor = new Color(1, 1, 0.6f, 1);

        private readonly HashSet<BoneModifier> _changedBones = new HashSet<BoneModifier>();
        private readonly HashSet<GameObject> _openedObjects = new HashSet<GameObject>();

        private static KeyValuePair<BoneLocation, Transform> _selectedTransform;
        private static BoneController _currentBoneController;
        private static ChaControl _currentChaControl;

        private readonly ImguiComboBox _charaSelectBox = new ImguiComboBox();
        private static GUIContent _selectedContent;

        private static readonly float[] _DefaultIncrementSize = { 0.1f, 0.1f, 0.01f, 1f };
        private static readonly float[] _IncrementSize = _DefaultIncrementSize.ToArray();
        private static readonly bool[] _LockXyz = new bool[_DefaultIncrementSize.Length];

        //private BoneModifierData[] _copiedModifier;
        private bool _editSymmetry = true;

        private bool _onlyShowCoords;
        private bool _onlyShowModified;
        private bool _onlyShowNewChanges;
        private bool _onlyShowFavorites;

        private static readonly Color _FavColor = new Color(1, 0.45f, 1);
        private readonly HashSet<string> _favorites = new HashSet<string>();
        private List<KeyValuePair<BoneLocation, Transform>> _favoritesResults;

        private bool _enableHelp;
        private static Dictionary<string, string> _boneTooltips;

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
                    _searchResults = value.Length == 0 ? null : FindAllBones(CheckSearchMatch);
                }
            }
        }

        private static List<KeyValuePair<BoneLocation, Transform>> FindAllBones(Func<string, bool> nameFilter)
        {
            IEnumerable<KeyValuePair<BoneLocation, Transform>> FindForLocation(BoneLocation boneLocation, Func<string, bool> filter)
            {
                return (_currentBoneController.BoneSearcher.GetAllBones(boneLocation)
                                                              .Where(pair => filter(pair.Key))
                                                              .Select(x => new KeyValuePair<BoneLocation, Transform>(boneLocation, x.Value.transform)));
            }
            var results = new List<KeyValuePair<BoneLocation, Transform>>();
            results.AddRange(FindForLocation(BoneLocation.BodyTop, nameFilter));
            for (int i = 0; i < _currentChaControl.objAccessory.Length; i++)
            {
                var accObj = _currentChaControl.objAccessory[i];
                if (accObj != null) results.AddRange(FindForLocation(BoneLocation.Accessory + i, nameFilter));
            }
            return results;
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

                var characterName = GetCharacterName(controller);
                _selectedContent = new GUIContent(characterName);
                Instance.Title = $"Advanced Bone Sliders - {characterName}";

                Instance.RepopulateFavorites();

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

        private void RepopulateFavorites()
        {
            _favorites.Clear();
            KKABMX_Core.Favorites.Value.Split('/').Select(x => x.Trim()).Where(x => x.Length > 0).Do(x => _favorites.Add(x));
        }
        private bool AddFavorite(string boneName)
        {
            if (!string.IsNullOrEmpty(boneName) && _favorites.Add(boneName))
            {
                KKABMX_Core.Favorites.Value = string.Join("/", _favorites.ToArray());
                return true;
            }

            return false;
        }
        private bool RemoveFavorite(string boneName)
        {
            return _favorites.Remove(boneName);
        }
        private bool IsFavorite(string boneName)
        {
            return _favorites.Contains(boneName);
        }

        private static string GetCharacterName(BoneController controller)
        {
            var objName = controller.name;
            var charaName = controller.ChaControl.fileParam.fullname;
            TranslationHelper.TryTranslate(charaName, out var charaNameTl);
            return string.IsNullOrEmpty(charaNameTl) ? charaName.Trim() : $"{objName} ({charaNameTl.Trim()})";
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
            //todo
            //WindowRect.x = Mathf.Min(Screen.width - WindowRect.width, Mathf.Max(0, WindowRect.x));
            //WindowRect.y = Mathf.Min(Screen.height - WindowRect.height, Mathf.Max(0, WindowRect.y));
            return new Rect(20, 220, 705, 600);
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
                Camera.onPostRender += OnRendered;
            }
        }

        private void OnDisable()
        {
            OnEnabledChanged?.Invoke(false);
            Camera.onPostRender -= OnRendered;
        }

        private static Material _gizmoMaterial;
        private static bool _enableGizmo = true;
        private static bool _gizmoOnTop = true;

        private static void OnRendered(Camera camera)
        {
            //todo cache main cam
            if (!_enableGizmo || _selectedTransform.Value == null || camera != Camera.main)
                return;

            if (_gizmoMaterial == null)
            {
                // Unity has a built-in shader that is useful for drawing simple colored things.
                var shader = Shader.Find("Hidden/Internal-Colored");
                _gizmoMaterial = new Material(shader);
                _gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;

                // Turn on alpha blending
                _gizmoMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                _gizmoMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                // Always draw
                _gizmoMaterial.SetInt("_Cull", (int)CullMode.Off);
                _gizmoMaterial.SetInt("_ZWrite", 0);
            }

            _gizmoMaterial.SetInt("_ZTest", (int)(_gizmoOnTop ? CompareFunction.Always : CompareFunction.LessEqual));
            // Set the material as currently used https://docs.unity3d.com/ScriptReference/Material.SetPass.html
            _gizmoMaterial.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);

            var tr = _selectedTransform.Value;
            GL.Color(new Color(0, 1, 0, 0.7f));
            GL.Vertex(tr.position);
            GL.Vertex(tr.position + tr.forward * 0.05f);
            GL.Color(new Color(1, 0, 0, 0.7f));
            GL.Vertex(tr.position);
            GL.Vertex(tr.position + tr.right * 0.05f);
            GL.Color(new Color(0, 0, 1, 0.7f));
            GL.Vertex(tr.position);
            GL.Vertex(tr.position + tr.up * 0.05f);

            GL.End();
            GL.PopMatrix();
        }

        protected override void OnGUI()
        {
            if (_currentBoneController == null)
            {
                Disable();
                return;
            }

            if (_gsInput == null)
            {
                _gsInput = new GUIStyle(UnityEngine.GUI.skin.textArea);
                _gsLabel = new GUIStyle(UnityEngine.GUI.skin.label);
                _gsButtonReset = new GUIStyle(UnityEngine.GUI.skin.button);
                _gsLabel.alignment = TextAnchor.MiddleRight;
                _gsLabel.normal.textColor = Color.white;
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
                UnityEngine.GUI.changed = false;

                // |-------|-------|
                // |search |options|
                // |-------|-------|
                // |       |       |
                // |       |       |
                // |-------|-------|
                // |buttons|buttons|
                // |-------|-------|

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box, GUILayout.Width(WindowRect.width / 2), GUILayout.ExpandHeight(true));
                    {
                        if (!MakerAPI.InsideMaker)
                        {
                            // Select character
                            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
                            {
                                GUILayout.Label("Character: ", GUILayout.ExpandWidth(false));
                                _charaSelectBox.Show(_selectedContent,
                                                     () => GetAllControllersSorted().Select(x => new GUIContent(GetCharacterName(x))).ToArray(),
                                                     i => Enable(GetAllControllersSorted().Skip(i).FirstOrDefault()),
                                                     (int)WindowRect.yMax);
                            }
                            GUILayout.EndHorizontal();
                        }

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
                                var newVal = GUILayout.TextField(showTipString ? "Search..." : SearchFieldValue, _GloExpand);
                                if (UnityEngine.GUI.changed)
                                    SearchFieldValue = newVal;
                                //UnityEngine.GUI.color = Color.white;

                                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                                {
                                    SearchFieldValue = "";
                                    UnityEngine.GUI.FocusControl("");
                                }

                                UnityEngine.GUI.color = _enableHelp ? Color.cyan : Color.white;
                                if (GUILayout.Button(new GUIContent("?", @"Bones adjusted with yellow ABMX sliders in maker tabs are automatically added to this window.

Hover over bones in the list below to see notes (only some bones have them). Bones commented with ""ANIMCOR"" are used by animation correction system, adjusting them can cause weird effects during gameplay.
BL = Bone length. It's the distance between given bone and its parent. In some cases (like cf_j_spine02/03) effect is pretty obvious, in some not so much. Offset and rotation controls can be used instead, but the effect may be a bit different during animations.effects and glitches. If the BL slider is grayed out it means it has no effect on that bone.

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
                                var origEnabled = UnityEngine.GUI.enabled;
                                if (_onlyShowFavorites) UnityEngine.GUI.enabled = false;
                                UnityEngine.GUI.color = Color.green;
                                _onlyShowModified = GUILayout.Toggle(_onlyShowModified, "Modified", GUILayout.ExpandWidth(false));
                                UnityEngine.GUI.color = Color.white;
                                _onlyShowNewChanges = GUILayout.Toggle(_onlyShowNewChanges, "New", GUILayout.ExpandWidth(false));
#if !AI
                                UnityEngine.GUI.color = Color.yellow;
                                _onlyShowCoords = GUILayout.Toggle(_onlyShowCoords, "Per-coord", GUILayout.ExpandWidth(false));
#endif
                                GUILayout.FlexibleSpace();

                                UnityEngine.GUI.enabled = origEnabled;
                                UnityEngine.GUI.changed = false;
                                UnityEngine.GUI.color = _FavColor;
                                _onlyShowFavorites = GUILayout.Toggle(_onlyShowFavorites, "Fav's", GUILayout.ExpandWidth(false));
                                if (UnityEngine.GUI.changed && _onlyShowFavorites)
                                    _favoritesResults = FindAllBones(IsFavorite);

                                UnityEngine.GUI.color = Color.white;
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();


                        // Bone list
                        _treeScrollPosition = GUILayout.BeginScrollView(_treeScrollPosition, false, true, GUILayout.ExpandHeight(true));
                        {
                            var currentCount = 0;
                            if (_onlyShowFavorites && _favoritesResults != null)
                            {
                                foreach (var favoritesResult in _favoritesResults)
                                {
                                    if (_searchFieldValue.Length == 0 || CheckSearchMatch(favoritesResult.Value.name))
                                        DisplayObjectTreeHelper(favoritesResult.Value.gameObject, 0, ref currentCount, favoritesResult.Key);
                                }
                            }
                            else if (_onlyShowModified || _onlyShowCoords || _onlyShowNewChanges)
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
                                DisplayObjectTreeHelper(_currentChaControl.objBodyBone, 0, ref currentCount, BoneLocation.BodyTop);
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

                            GUILayout.Space(ObjectTreeHeight / 3);
                        }
                        GUILayout.EndScrollView();


                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Collapse all", _GloExpand))
                                _openedObjects.Clear();
                            if (GUILayout.Button("Expand all", _GloExpand))
                                Array.ForEach(_currentBoneController.gameObject.GetComponentsInChildren<Transform>(), child => _openedObjects.Add(child.gameObject));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button(new GUIContent("Export", "Export all current modifiers in a human-readable form to clipboard."), _GloExpand))
                            {
                                try
                                {
                                    _currentBoneController.CleanEmptyModifiers();
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

                            if (GUILayout.Button(new GUIContent("Import", "Import previously exported data from clipboard (copy the exported text in any text editor)."), _GloExpand))
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

                            UnityEngine.GUI.color = _DangerColor;
                            if (GUILayout.Button(new GUIContent("Revert", "Reset modifiers to the state from after the current character card was loaded."), _GloExpand))
                                _currentBoneController.RevertChanges();
                            if (GUILayout.Button(new GUIContent("Clear", "Remove all modifiers, even the ones added by using yellow sliders in maker UI."), _GloExpand))
                            {
                                _currentBoneController.ModifierDict.Values.SelectMany(x => x).Do(modifier => modifier.Reset());
                                _currentBoneController.ModifierDict.Clear();
                                _currentChaControl.updateShapeFace = true;
                                _currentChaControl.updateShapeBody = true;
                                _changedBones.Clear();
                                _selectedTransform = default;
                            }

                            UnityEngine.GUI.color = Color.white;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    // Sliders
                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box, _GloExpand, GUILayout.ExpandHeight(true));
                    {
                        var mod = _selectedTransform.Value == null ? null : GetOrAddBoneModifier(_selectedTransform.Value.name, _selectedTransform.Key);

                        // Slider list
                        _slidersScrollPosition = GUILayout.BeginScrollView(_slidersScrollPosition, false, false, GUILayout.ExpandHeight(true));
                        {
                            if (_selectedTransform.Value == null)
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("Select a bone transform on the left to show available controls.");
                                GUILayout.FlexibleSpace();
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    var boneName = _selectedTransform.Value.name;
                                    GUILayout.Label(ToDisplayString(_selectedTransform.Key));
                                    GUILayout.Label(">");
                                    GUILayout.TextField(boneName, _gsLabel);
                                    GUILayout.FlexibleSpace();

                                    var isFav = IsFavorite(boneName);
                                    if (isFav)
                                        UnityEngine.GUI.color = _FavColor;
                                    if (GUILayout.Button("Fav", GUILayout.ExpandWidth(false)))
                                    {
                                        if (isFav) RemoveFavorite(boneName);
                                        else AddFavorite(boneName);
                                    }
                                    UnityEngine.GUI.color = Color.white;
                                }
                                GUILayout.EndHorizontal();

                                var counterBone = GetCounterBoneName(mod);
                                var origEnabled = UnityEngine.GUI.enabled;
                                if (counterBone == null) UnityEngine.GUI.enabled = false;
                                var otherMod = _editSymmetry && counterBone != null ? GetOrAddBoneModifier(counterBone, mod.BoneLocation) : null;
                                if (_editSymmetry) UnityEngine.GUI.color = _WarningColor;
                                _editSymmetry = GUILayout.Toggle(_editSymmetry, new GUIContent("Edit both left and right side bones", "Some bones have a symmetrical pair, like left and right elbow. They all end with _L or _R suffix. This setting will let you edit both sides at the same time (two separate bone modifiers are still used)."));
                                UnityEngine.GUI.color = Color.white;
                                GUILayout.Label("Other side bone: " + (counterBone ?? "No bone found"));
                                UnityEngine.GUI.enabled = origEnabled;

#if !AI && !HS2 && !EC
                                UnityEngine.GUI.changed = false;
                                var oldVal = mod.IsCoordinateSpecific();
                                if (oldVal) UnityEngine.GUI.color = Color.yellow;
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

                                GUILayout.BeginHorizontal();
                                {
                                    _enableGizmo = UnityEngine.GUILayout.Toggle(_enableGizmo, "Show bone gizmo ");
                                    var prevEnabled = UnityEngine.GUI.enabled;
                                    UnityEngine.GUI.enabled = _enableGizmo;
                                    _gizmoOnTop = GUILayout.Toggle(_gizmoOnTop, "on top ");
                                    UnityEngine.GUILayout.Label("(", GUILayout.ExpandWidth(false));
                                    UnityEngine.GUI.color = new Color(1, 0.3f, 0.3f);
                                    UnityEngine.GUILayout.Label("X", GUILayout.ExpandWidth(false));
                                    UnityEngine.GUI.color = new Color(0.3f, 0.3f, 1);
                                    UnityEngine.GUILayout.Label("Y", GUILayout.ExpandWidth(false));
                                    UnityEngine.GUI.color = new Color(0.3f, 1, 0.3f);
                                    UnityEngine.GUILayout.Label("Z", GUILayout.ExpandWidth(false));
                                    UnityEngine.GUI.color = Color.white;
                                    UnityEngine.GUILayout.Label(")", GUILayout.ExpandWidth(false));
                                    GUILayout.FlexibleSpace();
                                    UnityEngine.GUI.enabled = prevEnabled;
                                }
                                GUILayout.EndHorizontal();

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
                                    var origEnabled = UnityEngine.GUI.enabled;
                                    if (mod.IsEmpty()) UnityEngine.GUI.enabled = false;
                                    if (GUILayout.Button(new GUIContent("Copy", "Copy data of this modifier to clipboard so it can be pasted into another modifier, or into any text editor to hand-edit or save for later.\nIf the modifier is per-coordinate, data for all coordinates is copied."), _GloExpand))
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

                                    UnityEngine.GUI.enabled = origEnabled;

                                    if (//_copiedModifier == null && 
                                        string.IsNullOrEmpty(GUIUtility.systemCopyBuffer)) UnityEngine.GUI.enabled = false;
                                    if (GUILayout.Button(new GUIContent("Paste", "Paste modifier data that is currently in the clipboard. To get modifier data to paste use the Copy button, or copy previously saved data in any text editor."), _GloExpand))
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

                                    UnityEngine.GUI.enabled = origEnabled;

                                    UnityEngine.GUI.color = _DangerColor;
                                    if (mod.IsEmpty()) UnityEngine.GUI.enabled = false;
                                    if (GUILayout.Button(new GUIContent("Remove", "Reset and remove this modifier."), _GloExpand))
                                    {
                                        _changedBones.Remove(mod);
                                        _currentBoneController.RemoveModifier(mod);
                                        _selectedTransform = default;
                                    }

                                    UnityEngine.GUI.color = Color.white;
                                    UnityEngine.GUI.enabled = origEnabled;
                                }

                                //GUILayout.FlexibleSpace();
                            }
                            GUILayout.EndHorizontal();
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

            // Do not show tooltip if help is disabled or it could get in the way
            if (_charaSelectBox.DrawDropdownIfOpen() || !_enableHelp)
                UnityEngine.GUI.tooltip = null;
        }

        private static IOrderedEnumerable<BoneController> GetAllControllersSorted()
        {
            return FindObjectsOfType<BoneController>().OrderBy(x => x.name);
        }

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
#elif EC
            var coordinateType = KoikatsuCharaFile.ChaFileDefine.CoordinateType.School01;
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

            GUILayout.Space(2);

            if (KKABMX_Core.NoRotationBones.Contains(mod.BoneName))
            {
                UnityEngine.GUI.color = _WarningColor;
                GUILayout.Label("Warning: This bone has known issues with Tilt and possibly Offset/Length sliders. Use at your own risk.");
                UnityEngine.GUI.color = Color.white;

                GUILayout.Space(2);
            }

            GUILayout.BeginVertical(UnityEngine.GUI.skin.box); // Length slider ------------------------------------------------------------
            {
                var origEnabled = UnityEngine.GUI.enabled;
                if (!mod.CanApplyLength())
                    UnityEngine.GUI.enabled = false;

                var lengthModifier = modData.LengthModifier;
                if (DrawSingleSlider(sliderName: "Length:", value: ref lengthModifier, minValue: -2, maxValue: 2, defaultValue: 1, incrementIndex: 0))
                {
                    modData.LengthModifier = lengthModifier;
                    if (linkedModData != null) linkedModData.LengthModifier = lengthModifier;
                    anyChanged = true;
                }

                DrawIncrementControl(0, false);
                UnityEngine.GUI.enabled = origEnabled;
            }
            GUILayout.EndVertical();

            GUILayout.Space(2);

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

            GUILayout.Space(2);

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

            if (_LockXyz[incrementIndex])
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
                GUILayout.Label("Increment:", GUILayout.Width(65));

                float RoundToPowerOf10(float value)
                {
                    return Mathf.Pow(10, Mathf.Round(Mathf.Log10(value)));
                }

                float.TryParse(GUILayout.TextField(_IncrementSize[index].ToString(CultureInfo.InvariantCulture), _gsInput, _GloExpand, _GloHeight), out _IncrementSize[index]);
                if (GUILayout.Button("-", _gsButtonReset, _GloSmallButtonWidth, _GloHeight)) _IncrementSize[index] = RoundToPowerOf10(_IncrementSize[index] * 0.1f);
                if (GUILayout.Button("+", _gsButtonReset, _GloSmallButtonWidth, _GloHeight)) _IncrementSize[index] = RoundToPowerOf10(_IncrementSize[index] * 10f);
                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false))) _IncrementSize[index] = _DefaultIncrementSize[index];

                if (showLock)
                {
                    GUILayout.Space(4);

                    var isLock = _LockXyz[index];
                    if (isLock) UnityEngine.GUI.color = _WarningColor;
                    _LockXyz[index] = GUILayout.Toggle(isLock, "Lock XYZ", GUILayout.ExpandWidth(false));
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
                    GUILayout.Label(sliderName, GUILayout.Width(65), _GloHeight);

                value = GUILayout.HorizontalSlider(value, minValue, maxValue, _gsButtonReset, _gsButtonReset, _GloExpand, _GloHeight);

                float.TryParse(GUILayout.TextField(value.ToString(maxValue >= 100 ? "F1" : "F3", CultureInfo.InvariantCulture), _gsInput, GUILayout.Width(43), _GloHeight),
                               out value);

                if (GUILayout.Button("-", _gsButtonReset, GUILayout.Width(20), _GloHeight)) value -= _IncrementSize[incrementIndex];
                if (GUILayout.Button("+", _gsButtonReset, GUILayout.Width(20), _GloHeight)) value += _IncrementSize[incrementIndex];

                if (GUILayout.Button("0", _gsButtonReset, _GloSmallButtonWidth, _GloHeight)) value = defaultValue;
            }
            GUILayout.EndHorizontal();
            return UnityEngine.GUI.changed;
        }

        private void DisplayObjectTreeHelper(GameObject go, int indent, ref int currentCount, BoneLocation location)
        {
            currentCount++;

            var needsHeightMeasure = _singleObjectTreeItemHeight == 0;

            var isVisible = currentCount * _singleObjectTreeItemHeight >= _treeScrollPosition.y &&
                            (currentCount - 1) * _singleObjectTreeItemHeight <= _treeScrollPosition.y + ObjectTreeHeight;

            if (isVisible || needsHeightMeasure)
            {
                var originalColor = UnityEngine.GUI.color;
                var isSelected = _selectedTransform.Value == go.transform;
                if (isSelected)
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

                        var fancyObjName = indent > 0 ? go.name : $"{go.name} ({ToDisplayString(location)})";
                        string tooltip = null;
                        if (_enableHelp) _boneTooltips.TryGetValue(go.name, out tooltip);
                        if (GUILayout.Button(new GUIContent(fancyObjName, tooltip), UnityEngine.GUI.skin.label, _GloExpand, GUILayout.MinWidth(120)))
                        {
                            if (isSelected)
                            {
                                // Toggle on/off
                                if (!_openedObjects.Add(go))
                                    _openedObjects.Remove(go);
                            }
                            else
                                _selectedTransform = new KeyValuePair<BoneLocation, Transform>(location, go.transform);
                        }

                        if (IsFavorite(go.name))
                        {
                            UnityEngine.GUI.color = _FavColor;
                            GUILayout.Label("Fav", GUILayout.ExpandWidth(false));
                        }

                        UnityEngine.GUI.color = originalColor;
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

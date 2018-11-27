using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Logging;
using KKABMX.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace KKABMX.GUI
{
    /// <summary>
    /// Old style ABM GUI by essu, modified to work with ABMX
    /// </summary>
    internal class KKABMX_LegacyGUI : MonoBehaviour
    {
        private const int BoneNameWidth = 120;
        private Rect abmRect = new Rect(20, 220, 725, 400);

        private CameraControl_Ver2 ccv2;
        private readonly GUILayoutOption glo_HEIGHT = GUILayout.Height(23);

        private readonly GUILayoutOption glo_Slider = GUILayout.ExpandWidth(true);
        private readonly GUILayoutOption glo_SliderWidth = GUILayout.Width(125);
        private readonly GUILayoutOption glo_WIDTH_30 = GUILayout.Width(30);
        private GUIStyle gs_ButtonReset;

        private GUIStyle gs_Input;
        private GUIStyle gs_Label;
        private bool initGUI = true;

        private bool inMaker;
        private BoneModifierBody[] modifiers;
        private Vector2 scrollPosition = Vector2.zero;
        private bool visible = true;

        private void Awake()
        {
            UnityAction<Scene, LoadSceneMode> sl = (s, lsm) =>
            {
                if (lsm != LoadSceneMode.Single) return;
                inMaker = s.name == SceneNames.CustomScene;
                ccv2 = FindObjectOfType<CameraControl_Ver2>();
                if (inMaker) StartCoroutine(WaitForABM());
                _boneControllerMgr = FindObjectOfType<BoneControllerMgr>();
            };

            SceneManager.sceneLoaded += sl;

            sl(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private IEnumerator WaitForABM()
        {
            while (true)
            {
                var bc = FindObjectOfType<BoneController>();
                modifiers = bc?.Modifiers?.Values.ToArray();
                //if (modifiers != null && modifiers.Length > 0) break;
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                visible = !visible;
                if (!visible) ccv2.enabled = true;
            }
        }

        private void OnGUI()
        {
            if (initGUI)
            {
                gs_Input = new GUIStyle(UnityEngine.GUI.skin.textArea);
                gs_Label = new GUIStyle(UnityEngine.GUI.skin.label);
                gs_ButtonReset = new GUIStyle(UnityEngine.GUI.skin.button);
                gs_Label.alignment = TextAnchor.MiddleRight;
                gs_Label.normal.textColor = Color.white;
                initGUI = false;
            }

            if (!inMaker || modifiers == null) return;

            if (!visible) return;

            var mp = Input.mousePosition;
            mp.y = Screen.height - mp.y; //Mouse Y is inverse Screen Y
            ccv2.enabled = !abmRect.Contains(mp); //Disable camera when inside menu. 100% guaranteed to cause conflicts.
            abmRect = GUILayout.Window(1724, abmRect, LegacyWindow, "Legacy KKABM Sliders"); //1724 guaranteed to be unique orz
            abmRect.x = Mathf.Min(Screen.width - abmRect.width, Mathf.Max(0, abmRect.x));
            abmRect.y = Mathf.Min(Screen.height - abmRect.height, Mathf.Max(0, abmRect.y));
        }

        private bool _onlyShowAdditional;
        private string _boneAddFieldValue = "";
        private BoneControllerMgr _boneControllerMgr;

        private readonly HashSet<string> _addedBones = new HashSet<string>();

        private void LegacyWindow(int id)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, glo_Slider);
            GUILayout.BeginVertical();
            {
                DrawHeader();

                foreach (var mod in (_onlyShowAdditional ? modifiers.Where(x => _addedBones.Contains(x.BoneName)) : modifiers))
                {
                    GUILayout.BeginVertical(UnityEngine.GUI.skin.box);
                    {
                        var v3 = mod.SclMod;
                        var len = mod.LenMod;

                        GUILayout.BeginHorizontal(glo_Slider);
                        {
                            GUILayout.Label(mod.BoneName, gs_Label, GUILayout.Width(BoneNameWidth));

                            v3.x = GUILayout.HorizontalSlider(v3.x, 0f, 2f, gs_ButtonReset, gs_ButtonReset, glo_SliderWidth, glo_HEIGHT);
                            v3.y = GUILayout.HorizontalSlider(v3.y, 0f, 2f, gs_ButtonReset, gs_ButtonReset, glo_SliderWidth, glo_HEIGHT);
                            v3.z = GUILayout.HorizontalSlider(v3.z, 0f, 2f, gs_ButtonReset, gs_ButtonReset, glo_SliderWidth, glo_HEIGHT);

                            len = GUILayout.HorizontalSlider(len, 0.1f, 2f, gs_ButtonReset, gs_ButtonReset, glo_SliderWidth, glo_HEIGHT);

                            if (GUILayout.Button("X", gs_ButtonReset, glo_WIDTH_30, glo_HEIGHT))
                            {
                                v3 = Vector3.one;
                                len = 1f;
                            }

                            mod.SclMod = v3;
                            mod.LenMod = len;
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(glo_Slider);
                        {
                            GUILayout.Label("X / Y / Z / Length", gs_Label, GUILayout.Width(BoneNameWidth));

                            float.TryParse(GUILayout.TextField(v3.x.ToString(CultureInfo.InvariantCulture), gs_Input, glo_SliderWidth, glo_HEIGHT), out v3.x);
                            float.TryParse(GUILayout.TextField(v3.y.ToString(CultureInfo.InvariantCulture), gs_Input, glo_SliderWidth, glo_HEIGHT), out v3.y);
                            float.TryParse(GUILayout.TextField(v3.z.ToString(CultureInfo.InvariantCulture), gs_Input, glo_SliderWidth, glo_HEIGHT), out v3.z);

                            float.TryParse(GUILayout.TextField(len.ToString(CultureInfo.InvariantCulture), gs_Input, glo_SliderWidth, glo_HEIGHT), out len);

                            GUILayout.Space(30);

                            mod.SclMod = v3;
                            mod.LenMod = Mathf.Max(len, 0.1f);
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
            GUILayout.Label("Press Right Shift to hide/show this window. Slow, disable this window completely in plugin settings if unused.");

            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);
            {
                GUILayout.Label("Add a new bone to the list. If valid, it will be saved to the card.");

                _boneAddFieldValue = GUILayout.TextField(_boneAddFieldValue, GUILayout.Width(90));

                if (GUILayout.Button("Add"))
                {
                    _addedBones.Add(_boneAddFieldValue);

                    var bc = FindObjectOfType<BoneController>();

                    if (bc.Modifiers.ContainsKey(_boneAddFieldValue))
                    {
                        Logger.Log(LogLevel.Message, $"Bone {_boneAddFieldValue} is already added.");
                        _boneAddFieldValue = "";
                    }
                    else
                    {
                        var result = bc.InsertAdditionalModifier(_boneAddFieldValue);
                        if (result == null)
                        {
                            Logger.Log(LogLevel.Message, $"Failed to add bone {_boneAddFieldValue}, make sure the name is correct.");
                        }
                        else
                        {
                            BoneConfiguration.AdditionalBoneNames.Add(_boneAddFieldValue);
                            modifiers = bc.Modifiers.Values.ToArray();
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
using System.Collections;
using System.Linq;
using KKABMX.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace KKABMX.GUI
{
    /// <summary>
    /// Old style ABM GUI by essu, modified to work with ABMX
    /// </summary>
    internal class KKABMX_LegacyGUI : MonoBehaviour
    {
        private Rect abmRect = new Rect(20, 220, 600, 400);

        private CameraControl_Ver2 ccv2;
        private readonly GUILayoutOption glo_HEIGHT_30 = GUILayout.Height(30);

        private readonly GUILayoutOption glo_Slider = GUILayout.ExpandWidth(true);
        private readonly GUILayoutOption glo_SliderWidth = GUILayout.Width(125);
        private readonly GUILayoutOption glo_WIDTH_120 = GUILayout.Width(120);
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
            abmRect = GUILayout.Window(1724, abmRect, window, "KKABM Sliders"); //1724 guaranteed to be unique orz
            abmRect.x = Mathf.Min(Screen.width - abmRect.width, Mathf.Max(0, abmRect.x));
            abmRect.y = Mathf.Min(Screen.height - abmRect.height, Mathf.Max(0, abmRect.y));
        }

        private void window(int id)
        {
            //MarC0's gonna burst a blood vessel, by all means make this less awful.

            //GUILayout.BeginVertical();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, glo_Slider);

            foreach (var mod in modifiers)
            {
                GUILayout.BeginHorizontal(glo_HEIGHT_30);
                GUILayout.Space(40);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(glo_Slider);
                GUILayout.Label(mod.BoneName, gs_Label, glo_WIDTH_120);

                var v3 = mod.SclMod;

                v3.x = GUILayout.HorizontalSlider(v3.x, 0f, 2f, gs_ButtonReset, gs_ButtonReset, glo_SliderWidth,
                    glo_HEIGHT_30);
                v3.y = GUILayout.HorizontalSlider(v3.y, 0f, 2f, gs_ButtonReset, gs_ButtonReset, glo_SliderWidth,
                    glo_HEIGHT_30);
                v3.z = GUILayout.HorizontalSlider(v3.z, 0f, 2f, gs_ButtonReset, gs_ButtonReset, glo_SliderWidth,
                    glo_HEIGHT_30);

                if (GUILayout.Button("X", gs_ButtonReset, glo_WIDTH_30, glo_HEIGHT_30))
                    v3 = Vector3.one;

                mod.SclMod = v3;

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(glo_Slider);
                GUILayout.Space(128);

                float.TryParse(GUILayout.TextField(v3.x.ToString(), gs_Input, glo_SliderWidth, glo_HEIGHT_30),
                    out v3.x);
                float.TryParse(GUILayout.TextField(v3.y.ToString(), gs_Input, glo_SliderWidth, glo_HEIGHT_30),
                    out v3.y);
                float.TryParse(GUILayout.TextField(v3.z.ToString(), gs_Input, glo_SliderWidth, glo_HEIGHT_30),
                    out v3.z);

                GUILayout.Space(30);

                mod.SclMod = v3;

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            
            UnityEngine.GUI.DragWindow();
        }
    }
}
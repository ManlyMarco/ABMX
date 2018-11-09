using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MakerAPI
{
    public class MakerRadioButtons : MakerGuiEntryBase
    {
        private readonly string _settingName;
        private readonly string _button1;
        private readonly string _button2;
        private readonly string _button3;

        private readonly BehaviorSubject<int> _incomingValue;
        private readonly Subject<int> _outgoingValue;

        private static Transform _radioCopy;

        /// <summary>
        /// Buttons 1, 2, 3 are values 0, 1, 2
        /// </summary>
        public int Value
        {
            get => _incomingValue.Value;
            set => _incomingValue.OnNext(value);
        }

        public IObservable<int> ValueChanged => _outgoingValue;

        public MakerRadioButtons(MakerCategory category, string settingName, string button1, string button2, string button3) : base(category)
        {
            _settingName = settingName;
            _button1 = button1;
            _button2 = button2;
            _button3 = button3;
            _incomingValue = new BehaviorSubject<int>(0);
            _outgoingValue = new Subject<int>();
        }

        protected internal override void CreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(RadioCopy, subCategoryList, true);

            tr.name = "rb" + GuiApiNameAppendix;

            tr.Find("textTglTitle").GetComponent<TextMeshProUGUI>().text = _settingName;
            var t1 = tr.Find("rb00").GetComponent<Toggle>();
            var t2 = tr.Find("rb01").GetComponent<Toggle>();
            var t3 = tr.Find("rb02").GetComponent<Toggle>();

            t1.GetComponentInChildren<TextMeshProUGUI>().text = _button1;
            t2.GetComponentInChildren<TextMeshProUGUI>().text = _button2;
            t3.GetComponentInChildren<TextMeshProUGUI>().text = _button3;
            
            t1.onValueChanged.AddListener(a =>
            {
                if (a)
                {
                    _incomingValue.OnNext(0);
                    _outgoingValue.OnNext(0);
                }
            });
            t2.onValueChanged.AddListener(a =>
            {
                if (a)
                {
                    _incomingValue.OnNext(1);
                    _outgoingValue.OnNext(1);
                }
            });
            t3.onValueChanged.AddListener(a =>
            {
                if (a)
                {
                    _incomingValue.OnNext(2);
                    _outgoingValue.OnNext(2);
                }
            });

            _incomingValue.Subscribe(i =>
            {
                switch (i)
                {
                    case 0:
                        t1.isOn = true;
                        break;
                    case 1:
                        t2.isOn = true;
                        break;
                    case 2:
                        t3.isOn = true;
                        break;
                }
            });
            
            tr.gameObject.SetActive(true);
        }

        private static Transform RadioCopy
        {
            get
            {
                if (_radioCopy == null)
                {
                    // Exists in male and female maker
                    // CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglEye02/Eye02Top/rbEyeSettingType
                    var originalSlider = GameObject.Find("00_FaceTop").transform.Find("tglEye02/Eye02Top/rbEyeSettingType");

                    _radioCopy = Object.Instantiate(originalSlider, MakerAPI.Instance.transform, true);
                    _radioCopy.gameObject.SetActive(false);

                    foreach (var toggle in _radioCopy.GetComponentsInChildren<Toggle>())
                    {
                        toggle.onValueChanged.RemoveAllListeners();
                        toggle.image.raycastTarget = true;
                        toggle.graphic.raycastTarget = true;
                        toggle.gameObject.name = "rb" + toggle.gameObject.name.Substring(8);
                    }
                }

                return _radioCopy;
            }
        }
    }
}
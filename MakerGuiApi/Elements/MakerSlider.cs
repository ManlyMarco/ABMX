using System;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MakerAPI
{
    public class MakerSlider : MakerGuiEntryBase
    {
        private static Transform _sliderCopy;

        private readonly string _settingName;

        private readonly float _maxValue;
        private readonly float _minValue;
        private readonly float _defaultValue;

        private readonly BehaviorSubject<float> _incomingValue;
        private readonly Subject<float> _outgoingValue;

        public MakerSlider(MakerCategory category, string settingName,
            float minValue, float maxValue, float defaultValue) : base(category)
        {
            _settingName = settingName;

            _minValue = minValue;
            _maxValue = maxValue;
            _defaultValue = defaultValue;

            _outgoingValue = new Subject<float>();
            _incomingValue = new BehaviorSubject<float>(defaultValue);
        }

        public Func<string, float> StringToValue { get; set; }
        public Func<float, string> ValueToString { get; set; }

        public float Value
        {
            get => _incomingValue.Value;
            set => _incomingValue.OnNext(value);
        }

        public IObservable<float> ValueChanged => _outgoingValue;

        private static Transform SliderCopy
        {
            get
            {
                if (_sliderCopy == null)
                {
                    // Exists in male and female maker
                    // CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglAll/AllTop/sldTemp
                    var originalSlider = GameObject.Find("00_FaceTop").transform.Find("tglAll/AllTop/sldTemp");

                    _sliderCopy = Object.Instantiate(originalSlider, MakerAPI.Instance.transform, true);
                    _sliderCopy.gameObject.SetActive(false);

                    var slider = _sliderCopy.Find("Slider").GetComponent<Slider>();
                    slider.onValueChanged.RemoveAllListeners();
                    
                    var inputField = _sliderCopy.Find("InputField").GetComponent<TMP_InputField>();
                    inputField.onValueChanged.RemoveAllListeners();
                    inputField.onSubmit.RemoveAllListeners();
                    inputField.onEndEdit.RemoveAllListeners();

                    var resetButton = _sliderCopy.Find("Button").GetComponent<Button>();
                    resetButton.onClick.RemoveAllListeners();

                    foreach (var renderer in _sliderCopy.GetComponentsInChildren<Image>())
                        renderer.raycastTarget = true;
                }

                return _sliderCopy;
            }
        }

        protected internal override void CreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(SliderCopy, subCategoryList, true);

            tr.name = "sldTemp" + GuiApiNameAppendix;

            tr.Find("textShape").GetComponent<TextMeshProUGUI>().text = _settingName;

            var slider = tr.Find("Slider").GetComponent<Slider>();
            slider.minValue = _minValue;
            slider.maxValue = _maxValue;
            slider.onValueChanged.AddListener(val =>
            {
                _incomingValue.OnNext(val);
                _outgoingValue.OnNext(val);
            });

            slider.GetComponent<ObservableScrollTrigger>().OnScrollAsObservable().Subscribe(data =>
            {
                var scrollDelta = data.scrollDelta.y;
                var valueChange = Mathf.Pow(10, Mathf.Round(Mathf.Log10(slider.maxValue / 100)));

                if (scrollDelta < 0f)
                    slider.value += valueChange;
                else if (scrollDelta > 0f)
                    slider.value -= valueChange;
            });

            var inputField = tr.Find("InputField").GetComponent<TMP_InputField>();
            inputField.onEndEdit.AddListener(txt =>
            {
                var result = StringToValue?.Invoke(txt) ?? float.Parse(txt);
                slider.value = Mathf.Clamp(result, slider.minValue, slider.maxValue);
            });

            slider.onValueChanged.AddListener(f =>
            {
                if (ValueToString != null)
                    inputField.text = ValueToString(f);
                else
                    inputField.text = f.ToString("F2");
            });

            var resetButton = tr.Find("Button").GetComponent<Button>();
            resetButton.onClick.AddListener(() => slider.value = _defaultValue);

            _incomingValue.Subscribe(f => slider.value = f);

            tr.gameObject.SetActive(true);
        }

        public override void Dispose()
        {
            _incomingValue.Dispose();
            _outgoingValue.Dispose();
        }
    }
}
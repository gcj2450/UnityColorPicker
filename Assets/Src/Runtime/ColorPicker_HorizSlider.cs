using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BW_ColorPicker
{
    public class ColorPicker_HorizSlider : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IEndDragHandler, IDragHandler
    {
        public InputField RInput;
        public InputField GInput;
        public InputField BInput;
        public InputField AInput;

        public HorizSliderType curColorType = HorizSliderType.RGB;

        public Action<Color> OnColorChanged;
        public Action<bool> OnCursorDown;
        [SerializeField]
        private Color CurColor;

        void Awake()
        {
            RInput.onValueChanged.AddListener(InputChanged);
            GInput.onValueChanged.AddListener(InputChanged);
            BInput.onValueChanged.AddListener(InputChanged);
            AInput.onValueChanged.AddListener(InputChanged);

            //SetColor(CurColor, false);
        }

        /// <summary>
        /// 屏蔽UI Input的onValueChanged事件响应
        /// </summary>
        /// <param name="_fireEvt"></param>
        public void ToogleInputEvt(bool _fireEvt)
        {
            fireInputEvt = _fireEvt;
        }

        ClickSlider curClickSlider = ClickSlider.None;
        //鼠标在slider内不响应输入框变化事件
        bool fireInputEvt = true;
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (OnCursorDown != null)
                OnCursorDown(true);
            Vector2 uvPos = getUVPos(eventData.enterEventCamera);
            fireInputEvt = false;
            float firstBarY = GetComponent<Image>().material.GetFloat("_FirstBarY");
            float bardist = GetComponent<Image>().material.GetFloat("_BarDist");
            float barWidth = GetComponent<Image>().material.GetFloat("_BarWidth");
            //点击时判断点击在哪个slider内
            if (uvPos.y > (firstBarY - barWidth) && uvPos.y < (firstBarY + barWidth))
            {
                //A
                curClickSlider = ClickSlider.A;
            }
            else if (uvPos.y > (firstBarY - barWidth + bardist) && uvPos.y < (barWidth + firstBarY + bardist))
            {
                //V
                curClickSlider = ClickSlider.V;
            }
            else if (uvPos.y > (firstBarY - barWidth + 2 * bardist) && uvPos.y < (barWidth + firstBarY + 2 * bardist))
            {
                //S
                curClickSlider = ClickSlider.S;
            }
            else if (uvPos.y > (firstBarY - barWidth + 3 * bardist) && uvPos.y < (barWidth + firstBarY + 3 * bardist))
            {
                //H
                curClickSlider = ClickSlider.H;
            }
            SetShaderPos(curClickSlider, getUVPos(eventData.enterEventCamera));
        }
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (OnCursorDown != null)
                OnCursorDown(false);
            fireInputEvt = true;
            curClickSlider = ClickSlider.None;
        }
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            SetShaderPos(curClickSlider, getUVPos(eventData.enterEventCamera));
        }
        public virtual void OnDrag(PointerEventData eventData)
        {
            SetShaderPos(curClickSlider, getUVPos(eventData.enterEventCamera));
        }

        Vector2 getUVPos(Camera camera)
        {
            Vector2 _pos = Vector2.one;
            RectTransform rect = transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
                Input.mousePosition, camera, out _pos);

            Vector2 uvPos = new Vector2(_pos.x / rect.sizeDelta.x, _pos.y / rect.sizeDelta.y) + new Vector2(0.5f, 0.5f);
            //avoid  bar to be discard
            //if (uvPos.x < 0f)
            //    uvPos.x = 0.0001f;
            //if (uvPos.y < 0)
            //    uvPos.y = 0.0001f;
            //if (uvPos.x > 1f)
            //    uvPos.x = 1;
            //if (uvPos.y > 1.0f)
            //    uvPos.y = 1;
            return uvPos;
        }
        /// <summary>
        /// 设置Shader参数
        /// </summary>
        /// <param name="uvPos"></param>
        void SetShaderPos(ClickSlider _sliderType, Vector2 uvPos)
        {
            if (uvPos.x < 0)
                uvPos.x = 0;
            if (uvPos.x > 1)
                uvPos.x = 1;

            Vector4 oriPos = GetComponent<Image>().material.GetVector("_SelPos");
            Vector4 pos = new Vector4(0, 0, 0, 1);

            switch (_sliderType)
            {
                case ClickSlider.H:
                    //H
                    pos = new Vector4(uvPos.x, oriPos.y, oriPos.z, oriPos.w);
                    SetColor(new Color(pos.x, pos.y, pos.z, pos.w), true);
                    break;
                case ClickSlider.S:
                    //S
                    pos = new Vector4(oriPos.x, uvPos.x, oriPos.z, oriPos.w);
                    SetColor(new Color(pos.x, pos.y, pos.z, pos.w), true);
                    break;
                case ClickSlider.V:
                    //V
                    pos = new Vector4(oriPos.x, oriPos.y, uvPos.x, oriPos.w);
                    SetColor(new Color(pos.x, pos.y, pos.z, pos.w), true);
                    break;
                case ClickSlider.A:
                    //A
                    pos = new Vector4(oriPos.x, oriPos.y, oriPos.z, uvPos.x);
                    SetColor(new Color(pos.x, pos.y, pos.z, pos.w), true);
                    break;
            }

        }

        /// <summary>
        /// 设置颜色,是否对外发送颜色变化事件
        /// </summary>
        /// <param name="_color"></param>
        /// <param name="fireEvt"></param>
        public void SetColor(Color _color, bool fireEvt, bool updateUI = true)
        {
            GetComponent<Image>().material.SetVector("_SelPos", new Vector4(_color.r, _color.g, _color.b, _color.a));
            if (updateUI)
                UpdateRGBAHexInput(_color.r, _color.g, _color.b, _color.a);
            CurColor = _color;
            if (fireEvt && OnColorChanged != null)
                OnColorChanged(CurColor);
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="_r"></param>
        /// <param name="_g"></param>
        /// <param name="_b"></param>
        /// <param name="_a"></param>
        void UpdateRGBAHexInput(float _r, float _g, float _b, float _a)
        {
            int r = Mathf.FloorToInt(_r * 255.0f);
            int g = Mathf.FloorToInt(_g * 255.0f);
            int b = Mathf.FloorToInt(_b * 255.0f);
            RInput.text = r.ToString();
            GInput.text = g.ToString();
            BInput.text = b.ToString();
            int a = Mathf.FloorToInt(_a * 255.0f);
            AInput.text = a.ToString();
        }

        //==================Input输入部分==================
        /// <summary>
        /// R 输入框事件
        /// </summary>
        /// <param name="arg0"></param>
        private void InputChanged(string arg0)
        {
            if (!fireInputEvt)
                return;
            Debug.Log("InputChanged Evt");
            if (curColorType == HorizSliderType.RGB)
            {
                float r = ConvertVal(RInput);
                float g = ConvertVal(GInput);
                float b = ConvertVal(BInput);
                float a = ConvertVal(AInput);
                SetColor(new Color(r, g, b, a), true, false);
            }
        }

        /// <summary>
        /// 判断输入的数值，大于255的限制到255，
        /// 自动转换大于1的值为小数
        /// </summary>
        /// <param name="_inputfield"></param>
        /// <param name="_val"></param>
        /// <returns></returns>
        float ConvertVal(InputField _inputfield)
        {
            float val = 0;
            bool result = float.TryParse(_inputfield.text, out val);
            if (val < 0)
            {
                val = 0;
                _inputfield.text = "0";
            }
            if (val > 255)
            {
                _inputfield.text = "255";
                val = 255;
            }
            if (val >= 1)
            {
                val = val / 255.0f;
            }
            return val;
        }

    }

    /// <summary>
    /// 颜色条色彩模式
    /// </summary>
    public enum HorizSliderType
    {
        //Rgb模式的Slider
        RGB,
        //Hsv模式的Slider
        HSV
    }

    /// <summary>
    /// 从上到下四个slider
    /// </summary>
    public enum ClickSlider
    {
        None,
        H,
        S,
        V,
        A
    }
}
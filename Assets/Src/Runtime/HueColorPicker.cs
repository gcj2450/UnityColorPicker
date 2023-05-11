using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BW_ColorPicker
{
    public class HueColorPicker : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public Action<Color> OnColorChanged;
        private Color _curColor = Color.white;
        public Color CurColor
        {
            get
            {
                return _curColor;
            }
            set
            {
                if (_curColor != value)
                {
                    Debug.Log("set new color");
                    if (OnColorChanged != null)
                        OnColorChanged(value);
                }
                _curColor = value;
            }
        }
        public Button CloseBtn;
        public ColorWheelPlane ColorWheel;
        public ColorPicker_HorizSlider HorizSlider;

        public Image OriginColorImg;
        public Image NewColorImg;
        //颜色吸管
        public Button ColorTube;

        void Awake()
        {
            windowParent = transform.parent.GetComponent<RectTransform>();
            CloseBtn.onClick.AddListener(CloseBtnClick);
            HorizSlider.SetColor(CurColor, false);
            ColorWheel.SetColor(CurColor, false);
            ColorWheel.OnColorChanged += ColorWheelChanged;
            HorizSlider.OnColorChanged += HorizSliderChanged;

            ColorWheel.OnCursorDown += ColorWheelOnCursorDown;

            ColorTube.onClick.AddListener(ColorTubeClick);

            OriginColorImg.GetComponent<Button>().onClick.AddListener(OriginColorImgClick);
            OriginColorImg.color = CurColor;
            NewColorImg.color = CurColor;
        }

        private void CloseBtnClick()
        {
            OnColorChanged = null;
            gameObject.SetActive(false);
        }

        private void ColorWheelOnCursorDown(bool obj)
        {
            HorizSlider.ToogleInputEvt(!obj);
        }

        private void OriginColorImgClick()
        {
            SetCurColor(OriginColorImg.color);
        }


        /// <summary>
        /// 设置拾色器颜色
        /// </summary>
        /// <param name="_color"></param>
        public void SetCurColor(Color _color)
        {
            Debug.Log(_color.r.ToString() + _color.g + _color.b + _color.a);
            CurColor = _color;
            NewColorImg.color = CurColor;
            OriginColorImg.color = CurColor;
            ColorWheel.SetColor(CurColor, true);
        }

        private void ColorWheelChanged(Color obj)
        {
            //Debug.Log("ColorWheelChanged");
            CurColor = obj;
            NewColorImg.color = CurColor;
            HorizSlider.SetColor(CurColor, false);
        }

        private void HorizSliderChanged(Color obj)
        {
            //Debug.Log("bbb");
            CurColor = obj;
            NewColorImg.color = CurColor;
            ColorWheel.SetColor(CurColor, false);
        }
        //======吸管操作
        private void ColorTubeClick()
        {
            isPickingColor = true;
            ColorTube.image.color = Color.gray;
        }

        bool isPickingColor = false;

        void Update()
        {
            if (isPickingColor)
            {
                if (Input.GetMouseButton(0))
                {
                    StartCoroutine(CaptureScreenshot());
                    isPickingColor = false;
                    ColorTube.image.color = Color.white;
                }
            }
        }

        //吸管获取当前点击位置的颜色
        IEnumerator CaptureScreenshot()
        {
            //只在每一帧渲染完成后才读取屏幕信息
            yield return new WaitForEndOfFrame();

            Texture2D m_texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            // 读取Rect范围内的像素并存入纹理中
            m_texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            // 实际应用纹理
            m_texture.Apply();

            Color color = m_texture.GetPixel((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            SetCurColor(color);
        }

        void OnDisable()
        {
            OnColorChanged = null;
        }

        Rect screenRect = new Rect(0, 0, 0, 0);

        /// The root gameobject of this window.
        public RectTransform windowParent;

        public void OnBeginDrag(PointerEventData eventData)
        {
            screenRect.width = Screen.width;
            screenRect.height = Screen.height;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (windowParent == null)
            {
                Debug.LogWarning("Window parent is null, cannot drag a null window.");
                return;
            }

            if (screenRect.Contains(eventData.position))
                windowParent.position += (Vector3)eventData.delta;
        }
    }
}
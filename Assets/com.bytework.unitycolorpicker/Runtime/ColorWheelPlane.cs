using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BW_ColorPicker
{
    /// <summary>
    /// 根据点击位置判断拖动区域：色相环/渐变方块
    /// 拖拽色相环或者渐变方块会调用SetHuePos/SetRampPos设置shader位置变量
    /// 然后通过GetColor()获取Shader位置得到颜色并发送颜色改变事件
    /// SetColor()用于外部调用手动设置颜色
    /// </summary>
    public class ColorWheelPlane : MonoBehaviour,
    IPointerDownHandler, IEndDragHandler, IDragHandler, IPointerUpHandler
    {
        public Action<Color> OnColorChanged;
        public Action<bool> OnCursorDown;
        [SerializeField]
        private Color CurColor;

        //点击位置在色相环内
        bool isInHue = false;
        //点击位置在渐变方块内
        bool isInRamp = false;
        void Awake()
        {
            //SetColor(CurColor,false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DragSetData(eventData.enterEventCamera);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            DragSetData(eventData.enterEventCamera);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (OnCursorDown != null)
                OnCursorDown(true);
            Vector2 _pos = Vector2.one;
            RectTransform rect = transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
                Input.mousePosition, eventData.enterEventCamera, out _pos);

            Vector2 uvPos = new Vector2(_pos.x / rect.sizeDelta.x, _pos.y / rect.sizeDelta.y) + new Vector2(0.5f, 0.5f);

            //判断是否点击在圆环内
            float dist = circle(uvPos, 0.95f);
            if (dist > 0 && dist < 0.45f)
            {
                isInHue = true;
                isInRamp = false;
                SetHuePos(eventData.enterEventCamera);
            }

            if (uvPos.x > 0.25f && uvPos.x < 0.75f && uvPos.y > 0.25f && uvPos.y < 0.75f)
            {
                isInHue = false;
                isInRamp = true;
                SetRampPos(eventData.enterEventCamera);
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (OnCursorDown != null)
                OnCursorDown(false);
            isInRamp = false;
            isInHue = false;
        }
        /// <summary>
        /// 拖拽时设置数据
        /// </summary>
        void DragSetData(Camera camera)
        {
            if (isInHue)
            {
                SetHuePos(camera);
            }
            else if (isInRamp)
            {
                SetRampPos(camera);
            }
        }
        //获取中间渐变方块的位置
        void SetRampPos(Camera camera)
        {
            Vector2 _pos = Vector2.one;
            RectTransform rect = transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
                Input.mousePosition, camera, out _pos);

            Vector2 uvPos = new Vector2(_pos.x / rect.sizeDelta.x, _pos.y / rect.sizeDelta.y) + new Vector2(0.5f, 0.5f);
            uvPos = uvPos * 2.0f - new Vector2(1.0f, 1.0f);

            if (uvPos.x < -0.5f)
                uvPos.x = -0.5f;
            if (uvPos.x > 0.5f)
                uvPos.x = 0.5f;
            if (uvPos.y < -0.5f)
                uvPos.y = -0.5f;
            if (uvPos.y > 0.5f)
                uvPos.y = 0.5f;

            GetComponent<Image>().material.SetVector("_SelRampPos", new Vector4(uvPos.x, uvPos.y, 0, 0));
            GetColor();
        }
        //获取色相环的位置
        void SetHuePos(Camera camera)
        {
            Vector2 _pos = Vector2.one;
            RectTransform rect = transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
                Input.mousePosition, camera, out _pos);

            Vector2 uvPos = new Vector2(_pos.x / rect.sizeDelta.x, _pos.y / rect.sizeDelta.y) + new Vector2(0.5f, 0.5f);

            Vector2 center = (uvPos - new Vector2(0.5f, 0.5f)).normalized;

            float rad = Mathf.Atan2(center.y, center.x);

            float hue = (rad * Mathf.Rad2Deg) / 180.0f;
            if (hue < 0)
            {
                hue += 2;
            }
            GetComponent<Image>().material.SetFloat("_Hue", hue * 0.5f);
            GetColor();
        }

        /// <summary>
        /// 根据Shader位置获取当前颜色
        /// </summary>
        private void GetColor()
        {
            float h = GetComponent<Image>().material.GetFloat("_Hue");
            Vector4 vec = GetComponent<Image>().material.GetVector("_SelRampPos");

            float s = vec.x + 0.5f;
            float b = vec.y + 0.5f;

            Color cl = Color.HSVToRGB(h, s, b);
            CurColor = cl;
            if (OnColorChanged != null)
                OnColorChanged(CurColor);
        }

        /// <summary>
        /// 设置拾色器颜色
        /// </summary>
        public void SetColor(Color _color, bool fireEvt)
        {
            float h, s, b = 0;
            Color.RGBToHSV(_color, out h, out s, out b);
            GetComponent<Image>().material.SetFloat("_Hue", h);
            GetComponent<Image>().material.SetVector("_SelRampPos", new Vector4(s - 0.5f, b - 0.5f, 0, 0));
            CurColor = _color;
            if (fireEvt && OnColorChanged != null)
                OnColorChanged(CurColor);
            //Debug.Log("H: " + h + " S: " + s + " V: " + b);
        }

        //和shader中画圆的方法保持一致
        //判断点击位置是否在环形内
        float circle(Vector2 _st, float _radius)
        {
            Vector2 dist = _st - new Vector2(0.5f, 0.5f);
            return 1.0f - Vector2.Dot(dist, dist) * 4.0f;
        }

    }
}
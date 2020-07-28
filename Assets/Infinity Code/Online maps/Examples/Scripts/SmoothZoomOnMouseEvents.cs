/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsDemos
{
    [OnlineMapsPlugin("Smooth Zoom On Mouse Events", typeof(OnlineMapsControlBase), true)]
    public class SmoothZoomOnMouseEvents : MonoBehaviour
    {
        public float duration = 0.3f;

        private float fromZoom;
        private float toZoom;
        private float progress = 0;
        private bool isAnim = false;
        private OnlineMapsControlBase control;
        private OnlineMaps map;
        private Vector2 inputPosition;
        private float lastUpdateTime;

        private bool OnValidateZoom(OnlineMapsZoomEvent zoomEvent, float value)
        {
            if (zoomEvent != OnlineMapsZoomEvent.wheel && zoomEvent != OnlineMapsZoomEvent.doubleClick) return true;

            float z = map.floatZoom;
            if (isAnim) z = toZoom;
            int delta = 1;
            if (zoomEvent == OnlineMapsZoomEvent.wheel && map.floatZoom > value) delta = -1;
            z = Mathf.RoundToInt(z + delta);
            StartAnim(z);

            return false;

        }

        private void Start()
        {
            map = OnlineMaps.instance;
            control = OnlineMapsControlBase.instance;
            control.OnValidateZoom += OnValidateZoom;
        }

        private void StartAnim(float targetZoom)
        {
            fromZoom = map.floatZoom;
            toZoom = targetZoom;
            if (map.zoomRange != null) toZoom = map.zoomRange.CheckAndFix(toZoom);
            progress = 0;
            inputPosition = control.GetInputPosition();
            isAnim = true;

            lastUpdateTime = Time.time;

            map.OnMapUpdated -= UpdateZoom;
            map.OnMapUpdated += UpdateZoom;

            map.Redraw();
        }

        private void UpdateZoom()
        {
            if (!isAnim)
            {
                map.OnMapUpdated -= UpdateZoom;
                return;
            }

            progress += (Time.time - lastUpdateTime) / duration;
            lastUpdateTime = Time.time;

            if (progress >= 1)
            {
                progress = 1;
                isAnim = false;
                map.OnMapUpdated -= UpdateZoom;
            }

            float z = Mathf.Lerp(fromZoom, toZoom, progress);
            if (control.zoomMode == OnlineMapsZoomMode.center) map.floatZoom = z;
            else control.ZoomOnPoint(z - map.floatZoom, inputPosition);
            map.Redraw();
        }
    }
}
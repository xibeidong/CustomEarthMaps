/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adjusts map size to fit screen.
/// </summary>
[AddComponentMenu("Infinity Code/Online Maps/Plugins/Adjust to Screen")]
[OnlineMapsPlugin("Adjust to Screen", typeof(OnlineMapsControlBase))]
public class OnlineMapsAdjustToScreen : MonoBehaviour
{
    [Header("Recommended for 2D Controls.")]
    public bool halfSize = false;

    private int screenWidth;
    private int screenHeight;

    private void ResizeMap()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        int width = screenWidth / 256 * 256;
        int height = screenHeight / 256 * 256;

        if (halfSize)
        {
            width /= 2;
            height /= 2;
        }

        if (screenWidth % 256 != 0) width += 256;
        if (screenHeight % 256 != 0) height += 256;

        int viewWidth = width;
        int viewHeight = height;

        if (halfSize)
        {
            viewWidth *= 2;
            viewHeight *= 2;
        }

        if (OnlineMapsControlBase.instance.resultIsTexture)
        {
            OnlineMapsUtils.Destroy(OnlineMapsControlBase.instance.activeTexture);
            if (OnlineMapsUIImageControl.instance != null)
            {
                OnlineMapsUtils.Destroy(GetComponent<Image>().sprite);
            }
            else if (OnlineMapsSpriteRendererControl.instance != null)
            {
                OnlineMapsUtils.Destroy(GetComponent<SpriteRenderer>().sprite);
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            OnlineMaps.instance.SetTexture(texture);

            if (OnlineMapsUIRawImageControl.instance != null)
            {
                RectTransform rt = transform as RectTransform;
                rt.sizeDelta = new Vector2(viewWidth, viewHeight);
            }
            else if (OnlineMapsUIImageControl.instance != null)
            {
                RectTransform rt = transform as RectTransform;
                rt.sizeDelta = new Vector2(viewWidth, viewHeight);
            }
            else if (OnlineMapsSpriteRendererControl.instance != null)
            {
                GetComponent<BoxCollider>().size = new Vector3(viewWidth / 100f, viewHeight / 100f, 0.2f);
            }

            OnlineMaps.instance.RedrawImmediately();
        }
        else if (OnlineMapsTileSetControl.instance != null)
        {
            OnlineMapsTileSetControl.instance.Resize(width, height, viewWidth, viewHeight);
            if (OnlineMapsTileSetControl.instance.activeCamera.orthographic)
            {
                OnlineMapsTileSetControl.instance.activeCamera.orthographicSize = screenHeight / 2f;
            }
            else
            {
                OnlineMapsCameraOrbit cameraOrbit = GetComponent<OnlineMapsCameraOrbit>();
                if (cameraOrbit != null) cameraOrbit.distance = screenHeight * 0.8f;
            }
        }
    }

    private void Start()
    {
        ResizeMap();
    }

    private void Update()
    {
        if (screenWidth != Screen.width || screenHeight != Screen.height) ResizeMap();
    }
}
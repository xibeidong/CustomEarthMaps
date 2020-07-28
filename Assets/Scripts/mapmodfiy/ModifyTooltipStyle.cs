using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyTooltipStyle : MonoBehaviour
{
    private void Start()
    {
       // OnlineMapsMarker m = OnlineMapsMarkerManager.CreateItem(120.254126, 36.023108, "123 2020/7/25 14:46:32");
        // Subscribe to the event preparation of tooltip style.
        OnlineMapsGUITooltipDrawer.OnPrepareTooltipStyle += OnPrepareTooltipStyle;
    }

    private void OnPrepareTooltipStyle(ref GUIStyle style)
    {
        // Change the style settings.
        style.fontSize = Screen.width / 50;
        
    }
}

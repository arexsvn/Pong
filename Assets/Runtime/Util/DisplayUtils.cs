using System.Collections;
using UnityEngine;

public class DisplayUtils
{
    public static void ApplySafeArea(Canvas canvas, RectTransform safeArea)
    {
        Rect screenSafeArea = Screen.safeArea;
        Vector3 anchorMin = screenSafeArea.position;
        Vector3 anchorMax = screenSafeArea.position + screenSafeArea.size;
        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;
        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;

        safeArea.anchorMin = anchorMin;
        safeArea.anchorMax = anchorMax;
    }
}
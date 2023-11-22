using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudView : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public Button backButton;
    public TextMeshProUGUI serverStatus;
    public TextMeshProUGUI scoreTop;
    public TextMeshProUGUI scoreBottom;
    public RectTransform safeAreaTransform;

    public void show(bool show = true, bool instant = true)
    {
        enableButtons(show);

        if (instant)
        {
            canvas.enabled = show;
        }
        else
        {
            canvas.enabled = true;
            UITransitions.fade(gameObject, canvasGroup, !show, false);
        }

        if (show)
        {
            DisplayUtils.ApplySafeArea(canvas, safeAreaTransform);
        }
    }

    public void enableButtons(bool enable)
    {
        backButton.interactable = enable;
    }
}
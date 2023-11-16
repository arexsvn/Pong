using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuView : MonoBehaviour
{
    public TextMeshProUGUI title;
    public Transform buttonContainer;
    public RectTransform safeAreaTransform;
    public CanvasGroup canvasGroup;
    public Canvas canvas;
    public Button settingsButton;
    public Button themeButton;

    public void show(bool show = true, bool instant = true)
    {
        if (show)
        {
            DisplayUtils.ApplySafeArea(canvas, safeAreaTransform);
        }

        if (instant)
        {
            gameObject.SetActive(show);
        }
        else
        {
            UITransitions.fade(gameObject, canvasGroup, !show, false, UITransitions.LONG_FADE_TIME);
        }
    }
}
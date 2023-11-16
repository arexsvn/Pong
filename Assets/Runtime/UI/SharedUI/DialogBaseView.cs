using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogBaseView : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public CanvasGroup canvasGroup;
    public Canvas canvas;
    public Button backButton;
    public GameObject backButtonVisual;
    public GameObject root;
    public GameObject dialogBackground;
    public Image background;
    public Transform content;
    private bool _showing;

    public bool showing { get => _showing; }


    public void fade(bool fadeOut, float fadeTime = -1f, bool destroyOnComplete = false)
    {
        _showing = !fadeOut;
        gameObject.SetActive(true);
        UITransitions.fade(root, canvasGroup, fadeOut, destroyOnComplete, fadeTime);
    }

    public void EnableBack(bool enable)
    {
        backButton.interactable = enable;
    }

    public void ShowBack(bool show)
    {
        backButtonVisual.SetActive(show);
    }

    public void ApplyTheme(VisualTheme theme)
    {
        VisualThemeContainer[] visualThemeContainers = GetComponentsInChildren<VisualThemeContainer>();
        foreach (VisualThemeContainer visualThemeContainer in visualThemeContainers)
        {
            visualThemeContainer.Apply(theme);
        }
    }

    public void showBackground(bool show)
    {
        background.gameObject.SetActive(show);
    }

    public void showDialogBackground(bool show)
    {
        dialogBackground.gameObject.SetActive(show);
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            titleText.gameObject.SetActive(false);
        }
        else
        {
            titleText.gameObject.SetActive(true);
            titleText.text = title;
        }
    }

    public void SetPosition(Vector2 position)
    {
        content.localPosition = position;
    }
}
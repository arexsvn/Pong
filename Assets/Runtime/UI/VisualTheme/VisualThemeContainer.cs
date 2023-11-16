using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualThemeContainer : MonoBehaviour
{
    public List<Image> Primary;
    public List<Image> Highlight;
    public List<Image> Border;
    public List<Image> Background;
    public List<TextMeshProUGUI> PrimaryText;
    public List<TextMeshProUGUI> HighlightText;
    public List<TextMeshProUGUI> TitleText;
    public Transform BackgroundImageContainer;
    private BackgroundImage _currentBackgroundImage;
    private string BACKGROUND_IMAGE_PREFAB = "UI/BackgroundImage";
    private VisualTheme _current;
    public VisualTheme Current { get => _current; }

    public void Apply(VisualTheme theme, float time = 0f)
    {        
        if (theme == _current)
        {
            return;
        }
        _current = theme;

        ApplyColor(Primary, theme.colorTheme.primaryColor, time);
        ApplyColor(Highlight, theme.colorTheme.highlightColor, time);
        ApplyColor(Border, theme.colorTheme.borderColor, time);
        ApplyColor(Background, theme.colorTheme.backgroundColor, time);
        ApplyColor(PrimaryText, theme.colorTheme.primaryTextColor, time);
        ApplyColor(HighlightText, theme.colorTheme.highlightTextColor, time);
        ApplyColor(TitleText, theme.colorTheme.titleTextColor, time);

        if (!string.IsNullOrEmpty(theme.backgroundImage) && BackgroundImageContainer != null)
        {
            setBackgroundImage(theme.backgroundImage);
        }
    }
    
    private void ApplyColor(List<Image> images, string hexColor, float time)
    {
        Color baseColor;
        ColorUtility.TryParseHtmlString(hexColor, out baseColor);

        foreach (Image image in images)
        {
            if (image == null)
            {
                Debug.LogError("ApplyColor :: Image is null.");
                continue;
            }
            Color finalColor = new Color(baseColor.r, baseColor.g, baseColor.b, image.color.a);
            image.DOColor(finalColor, time);
        }
    }

    private void ApplyColor(List<TextMeshProUGUI> texts, string hexColor, float time)
    {
        Color baseColor;
        ColorUtility.TryParseHtmlString(hexColor, out baseColor);

        foreach (TextMeshProUGUI text in texts)
        {
            if (text == null)
            {
                Debug.LogError("ApplyColor :: Text is null.");
                continue;
            }

            Color finalColor = new Color(baseColor.r, baseColor.g, baseColor.b, text.color.a);
            text.DOColor(finalColor, time);
        }
    }

    public void setBackgroundImage(string imageName)
    {
        if (_currentBackgroundImage != null && _currentBackgroundImage.ImageName == imageName)
        {
            return;
        }

        BackgroundImage backgroundImage = Object.Instantiate(Resources.Load<BackgroundImage>(BACKGROUND_IMAGE_PREFAB), BackgroundImageContainer);
        backgroundImage.canvasGroup.alpha = 0;
        backgroundImage.LoadImage(imageName);

        if (_currentBackgroundImage == null)
        {
            _currentBackgroundImage = backgroundImage;
            _currentBackgroundImage.canvasGroup.alpha = 1;
        }
        else
        {
            BackgroundImage oldImage = _currentBackgroundImage;
            _currentBackgroundImage = backgroundImage;
            UITransitions.fade(oldImage.gameObject, oldImage.canvasGroup, true, false, -1, handleOldImageFadeout);
            _currentBackgroundImage.canvasGroup.alpha = 0;
            UITransitions.fade(_currentBackgroundImage.gameObject, _currentBackgroundImage.canvasGroup, false);
        }
    }

    private void handleOldImageFadeout(GameObject oldImage)
    {
        BackgroundImage backgroundImage = oldImage.GetComponent<BackgroundImage>();
        backgroundImage.Cleanup();
    }
}

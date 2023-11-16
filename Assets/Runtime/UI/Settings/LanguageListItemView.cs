using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class LanguageListItemView : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Image selectedImage;
    public Image deselectedImage;
    public Button selectButton;
    public Language currentLanguage;

    public void Set(Language language, System.Action<Language> selectAction, bool selected)
    {
        currentLanguage = language;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => selectAction(language));

        label.text = language.label;

        select(selected);
    }

    public void select(bool selected)
    {
        if (selected)
        {
            selectedImage.DOFade(1f, .2f);
            deselectedImage.DOFade(0f, .2f);
        }
        else
        {
            selectedImage.DOFade(0f, .2f);
            deselectedImage.DOFade(1f, .2f);
        }
    }
}
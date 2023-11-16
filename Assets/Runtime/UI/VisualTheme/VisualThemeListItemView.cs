using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VisualThemeListItemView : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Image themeImage;
    public Button selectButton;
    public VisualTheme visualTheme;
    public Image selectedImage;
    public Image deselectedImage;

    public void Set(VisualTheme visualTheme, string label, System.Action<VisualTheme> selectAction, bool selected)
    {
        this.visualTheme = visualTheme;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => selectAction(visualTheme));

        SetLabel(label);

        select(selected);
    }

    public void SetLabel(string label)
    {
        this.label.text = label;
    }

    public void select(bool selected)
    {
        selectedImage.gameObject.SetActive(selected);
        deselectedImage.gameObject.SetActive(!selected);
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BasicListView : MonoBehaviour, IUIView
{
    public TextMeshProUGUI titleText;
    public Button closeButton;
    public RectTransform listContainer;
    public BasicButtonView listItemPrefab;

    public event System.Action<IUIView> OnReady;
    public event System.Action<IUIView> OnClosed;

    public void Show(bool animate = true)
    {
        gameObject.SetActive(true);
    }

    public void Hide(bool animate = true)
    {
        OnClosed?.Invoke(this);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }

    public void ApplyTheme(VisualTheme theme)
    {
        VisualThemeContainer[] visualThemeContainers = GetComponentsInChildren<VisualThemeContainer>();
        foreach (VisualThemeContainer visualThemeContainer in visualThemeContainers)
        {
            visualThemeContainer.Apply(theme);
        }
    }

    public void Init()
    {
        gameObject.SetActive(false);
    }

    public void ClearList()
    {
        foreach (Transform child in listContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateList(List<ButtonData> listItems)
    {
        int index = 0;
        BasicButtonView[] existingViews = listContainer.GetComponentsInChildren<BasicButtonView>();

        foreach (BasicButtonView existingView in existingViews)
        {
            if (index < listItems.Count)
            {
                existingView.labelText.text = listItems[index].label;
                existingView.button.onClick.AddListener(listItems[index].action);
            }
            else
            {
                Destroy(existingView.gameObject);
            }

            index++;
        }

        if (index < listItems.Count)
        {
            for (int n = index; n < listItems.Count; n++)
            {
                BasicButtonView buttonView = Instantiate(listItemPrefab, listContainer);
                buttonView.labelText.text = listItems[n].label;
                buttonView.button.onClick.AddListener(listItems[n].action);
            }
        }
    }

    public void Set(string title, List<ButtonData> listItems, bool showCloseButton = true, UnityAction closeAction = null)
    {
        titleText.text = title;

        UpdateList(listItems);

        closeButton.gameObject.SetActive(showCloseButton);

        closeButton.onClick.AddListener(() =>
        {
            Hide();
            closeAction?.Invoke();
        });

        Show();
    }

    public bool IsFullScreen { get => false; set { } }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BasicDialogView : MonoBehaviour, IUIView
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button closeButton;
    public RectTransform buttonContainer;
    public BasicButtonView buttonPrefab;

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
    }

    public void Init()
    {
        gameObject.SetActive(false);
    }

    public void Set(string title, string message = null, List<ButtonData> buttons = null, bool showCloseButton = true, UnityAction closeAction = null)
    {
        titleText.text = title;

        if (!string.IsNullOrEmpty(message))
        {
            messageText.text = message;
        }

        if (buttons != null)
        {
            foreach(ButtonData buttonData in buttons)
            {
                BasicButtonView buttonView = Instantiate(buttonPrefab, buttonContainer);
                buttonView.labelText.text = buttonData.label;
                buttonView.button.onClick.AddListener(buttonData.action);
            }
        }

        closeButton.gameObject.SetActive(showCloseButton);

        if (closeAction != null)
        {
            closeButton.onClick.AddListener(closeAction);
        }
        else
        {
            closeButton.onClick.AddListener(()=> Hide());
        }

        Show();
    }

    public bool IsFullScreen { get => false; set { } }
}

public class ButtonData
{
    public string label;
    public UnityAction action;

    public ButtonData(string label, UnityAction action)
    {
        this.label = label;
        this.action = action;
    }
}
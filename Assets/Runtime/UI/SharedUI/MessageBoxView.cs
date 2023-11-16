using TMPro;
using UnityEngine;

public class MessageBoxView : MonoBehaviour
{
    public DialogBaseView baseView;
    public TextMeshProUGUI messageText;
    public void SetTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            baseView.titleText.gameObject.SetActive(false);
        }
        else
        {
            baseView.titleText.gameObject.SetActive(true);
            baseView.titleText.text = title;
        }
    }

    public void SetMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            messageText.gameObject.SetActive(false);
        }
        else
        {
            messageText.gameObject.SetActive(true);
            messageText.text = message;
        }
    }

    public void show(bool show, float fadeTime = -1f, bool destroyOnComplete = true)
    {
        baseView.fade(!show, fadeTime, destroyOnComplete);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxView : MonoBehaviour
{
    public MessageBoxView baseView;
    public Transform buttonContainer;

    public void enableButtons(bool enable)
    {
        Button[] buttons = buttonContainer.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = enable;
        }
    }

    public void clearButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void showBackground(bool show)
    {
        baseView.baseView.showBackground(show);
    }

    public void show(bool show)
    {
        baseView.show(show);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DialogBoxWithImageView : MonoBehaviour
{
    public MessageBoxView baseView;
    public Transform buttonContainer;
    public Image image;
    public Image imageHighlight;
    private Vector3 _rotationTarget = new Vector3(0f, 0f, -360f);

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

    public void showSprite(Sprite sprite)
    {
        image.sprite = sprite;

        //imageHighlight.transform.DORotate(_rotationTarget, 1f).SetLoops(-1, LoopType.Incremental);
        imageHighlight.transform.DORotate(_rotationTarget, 8f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear).SetRelative();
    }
}

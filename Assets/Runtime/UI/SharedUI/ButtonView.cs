using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;

public class ButtonView : MonoBehaviour 
{
    public TextMeshProUGUI labelText;
	public Image iconImage;
	public Image buttonImage;
	public Image buttonOutline;
	public Button button;
	public Sprite whiteButtonImage;
	public Sprite blueButtonImage; 
	public Sprite disabledButtonImage;
	public CanvasGroup canvasGroup;
    private Sequence _outlineFadeSequence;

    public void showHighlight(bool show)
    {
        _outlineFadeSequence?.Kill(true);
        _outlineFadeSequence = null;

        if (show)
        {
            buttonOutline.gameObject.SetActive(true);
            buttonOutline.DOFade(0f, 0f);

            startHighlightAfterDelay();
        }
        else
        {
            buttonOutline.gameObject.SetActive(false);
        }
    }

    private void startHighlight()
    {
        if (buttonOutline == null || buttonOutline.gameObject == null || !buttonOutline.gameObject.activeSelf)
        {
            return;
        }

        _outlineFadeSequence = DOTween.Sequence();
        _outlineFadeSequence.Append(buttonOutline.DOFade(.6f, 1f));
        _outlineFadeSequence.SetEase(Ease.Linear);
        _outlineFadeSequence.SetLoops(-1, LoopType.Yoyo);
        _outlineFadeSequence.SetDelay(1f);
    }

    private async void startHighlightAfterDelay()
    {
        await Task.Delay(2000);
        startHighlight();
    }
}
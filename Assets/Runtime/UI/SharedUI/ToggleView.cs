using signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ToggleView : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI toggleLabelText;
    public Button button;
    public Signal<bool> toggled = new Signal<bool>();
    public Image toggleOnImage;
    public Image toggleOffImage;
    public string onLabel;
    public string offLabel;
    private bool _toggleOn;

    private void Start()
    {
        button.onClick.AddListener(handleButtonClicked);
        // Forces layout of layoutgroup
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private void handleButtonClicked()
    {
        toggleOn = !toggleOn;
        toggled.Dispatch(_toggleOn);
    }

    public bool toggleOn
    {
        get
        {
            return _toggleOn;
        }

        set
        {
            _toggleOn = value;

            DOTween.Complete(toggleOnImage);
            DOTween.Complete(toggleOffImage);

            if (_toggleOn)
            {
                toggleLabelText.text = onLabel;
                toggleOnImage.DOFade(1f, .2f);
                toggleOffImage.DOFade(0f, .2f);
            }
            else
            {
                toggleLabelText.text = offLabel;
                toggleOnImage.DOFade(0f, .2f);
                toggleOffImage.DOFade(1f, .2f);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(handleButtonClicked);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderView : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI handleText;
    public Slider slider;

    private void Awake()
    {
        handleValueChange(slider.value);
        slider.onValueChanged.AddListener(handleValueChange);
    }

    private void handleValueChange(float value)
    {
        float formattedValue = value * 100f;
        handleText.text = formattedValue.ToString("N0");
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(handleValueChange);
    }
}

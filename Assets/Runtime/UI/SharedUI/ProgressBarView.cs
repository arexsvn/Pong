using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ProgressBarView : MonoBehaviour 
{
    public TextMeshProUGUI labelText;
	public Slider slider;

    public void setProgress(int current, int max, bool instant = true)
    {
        float targetValue = (float)current / (float)max;
        if (instant)
        {
            slider.value = targetValue;
        }
        else
        {
            slider.DOValue(targetValue, .8f);
        }

        labelText.text = current + " / " + max;
    }
}
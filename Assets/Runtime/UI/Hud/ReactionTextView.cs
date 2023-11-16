using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReactionTextView : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMP_FontAsset reactionTextFont;
    public TMP_FontAsset reactionText2Font;
    public TextMeshProUGUI reactionText;
    public TextMeshProUGUI reactionText2;
    public Text reactionTextFallback;
    public Text reactionText2Fallback;
    private List<VertexGradient> _colors;
    private System.Random _rand;

    public void Awake()
    {
        _rand = new System.Random();

        _colors = new List<VertexGradient>();

        Color color1 = StringUtils.getColorFromHex("#D7FF00FF");
        Color color2 = StringUtils.getColorFromHex("#FF00B9FF");
        _colors.Add(new VertexGradient(color1, color1, color2, color2));

        color1 = StringUtils.getColorFromHex("#00ABFFFF");
        color2 = StringUtils.getColorFromHex("#00FFF8FF");
        _colors.Add(new VertexGradient(color1, color1, color2, color2));

        color1 = StringUtils.getColorFromHex("#FAFF00FF");
        color2 = StringUtils.getColorFromHex("#08C009FF");
        _colors.Add(new VertexGradient(color1, color1, color2, color2));

        color1 = StringUtils.getColorFromHex("#FF9F00FF");
        color2 = StringUtils.getColorFromHex("#A112F8FF");
        _colors.Add(new VertexGradient(color1, color1, color2, color2));

        color1 = StringUtils.getColorFromHex("#00D8FFFF");
        color2 = StringUtils.getColorFromHex("#BA12F8FF");
        _colors.Add(new VertexGradient(color1, color1, color2, color2));

        color1 = StringUtils.getColorFromHex("#FFDE00FF");
        color2 = StringUtils.getColorFromHex("#F81612FF");
        _colors.Add(new VertexGradient(color1, color1, color2, color2));
    }

    public void show(string text, float intensity)
    {
        text = text[0].ToString().ToUpper() + text.Substring(1).ToLower();

        DOTween.Kill(transform);
        DOTween.Kill(canvasGroup);

        reactionText.gameObject.SetActive(false);
        reactionText2.gameObject.SetActive(false);
        reactionTextFallback.gameObject.SetActive(false);
        reactionText2Fallback.gameObject.SetActive(false);

        canvasGroup.alpha = 0;
        gameObject.SetActive(true);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.zero;

        float scaleOffset = .5f + Mathf.Max(intensity, .5f);
        float scaleTimeOffset = .5f - Mathf.Min(intensity, .3f);

        VertexGradient vertexGradient = _colors[_rand.Next(_colors.Count)];

        if (intensity > .7f)
        {
            if (reactionText2Font.HasCharacters(text))
            {
                reactionText2.gameObject.SetActive(true);
                reactionText2.text = text + "!";
                reactionText2.colorGradient = vertexGradient;
            }
            else
            {
                reactionText2Fallback.gameObject.SetActive(true);
                reactionText2Fallback.text = text + "!";
            }

            transform.DOScale(scaleOffset, scaleTimeOffset + .4f).SetEase(Ease.OutElastic);
        }
        else
        {
            if (reactionTextFont.HasCharacters(text))
            {
                reactionText.gameObject.SetActive(true);
                reactionText.text = text + "!";
                reactionText.colorGradient = vertexGradient;
            }
            else
            {
                reactionTextFallback.gameObject.SetActive(true);
                reactionTextFallback.text = text + "!";
            }

            transform.DOScale(scaleOffset, scaleTimeOffset).SetEase(Ease.InOutQuad);
        }

        canvasGroup.DOFade(1, .4f);

        transform.DOLocalMoveY(transform.localPosition.y + 80f, 1f).SetDelay(.6f).SetEase(Ease.InQuad);
        canvasGroup.DOFade(0, .6f).SetDelay(.8f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public bool isShowing()
    {
        return gameObject.activeSelf;
    }
}

public class ReactionTextColor
{
    public Color top;
    public Color bottom;

    public ReactionTextColor(string topHex, string bottomHex)
    {
        top = StringUtils.getColorFromHex(topHex);
        bottom = StringUtils.getColorFromHex(bottomHex);
    }
}

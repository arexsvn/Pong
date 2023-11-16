using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImage : MonoBehaviour
{
    public Image Image;
    public string ImageName;
    public CanvasGroup canvasGroup;
    public AspectRatioFitter aspectRatioFitter;

    public void LoadImage(string imageName)
    {
        ImageName = imageName;
        
        Sprite newSprite = Resources.Load<Sprite>("Images/" + imageName);

        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        // Calculate aspect ratio BEFORE setting sprite to image.sprite to get the original image size.
        aspectRatioFitter.aspectRatio = newSprite.rect.width / newSprite.rect.height;
        
        Image.sprite = newSprite;
    }

    public void Cleanup()
    {
        //Destroy(Image.sprite.texture);
        Image.sprite = null;
        Image = null;
        Resources.UnloadUnusedAssets();
        Destroy(gameObject);
    }
}

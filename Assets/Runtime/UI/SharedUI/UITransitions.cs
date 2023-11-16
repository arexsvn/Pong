using UnityEngine;
using DG.Tweening;

public class UITransitions
{
	public const float FADE_TIME = .2f;
	public const float LONG_FADE_TIME = .5f;

	public static void fade(GameObject target, CanvasGroup canvasGroup, bool fadeOut = true, bool destroyOnComplete = false, float fadeTime = -1f, System.Action<GameObject> complete = null)
	{
		if (fadeTime == -1f)
		{
			fadeTime = FADE_TIME;
		}

		canvasGroup.DOComplete(false);

		if (fadeOut)
		{
			TweenCallback fadeComplete = () =>
			{
				if (target != null)
                {
					if (destroyOnComplete)
					{
						Object.Destroy(target);
					}
					else
					{
						target.SetActive(false);
					}
				}

				complete?.Invoke(target);
			};

			canvasGroup.alpha = 1;
			canvasGroup.DOComplete(false);
			canvasGroup.DOFade(0, fadeTime).OnComplete(fadeComplete);
		}
		else
		{
			if (target != null)
            {
				target.SetActive(true);
			}

			canvasGroup.alpha = 0;
			canvasGroup.DOComplete(false);

			if (complete != null)
            {
				canvasGroup.DOFade(1, fadeTime).OnComplete(()=>complete(target));
			}
			else
            {
				canvasGroup.DOFade(1, fadeTime);
			}	
		}
	}
}
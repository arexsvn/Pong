using UnityEngine;
using UnityEngine.UI;

public class EntryContainerView : MonoBehaviour 
{
	public GameObject entryContainer;
	public CanvasGroup canvasGroup;
	public RectTransform parentRect;
	public CanvasScaler canvasScaler;
	public ScrollRect scrollRect;
	public GameObject walletContainer;

	public void clearContainer()
	{
		foreach (Transform child in entryContainer.transform)
		{
			Object.Destroy(child.gameObject);
		}
	}

	public void scrollToTop()
	{
		scrollRect.verticalNormalizedPosition = 1f;
	}
}

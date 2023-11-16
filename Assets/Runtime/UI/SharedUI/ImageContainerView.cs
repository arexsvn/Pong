using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ImageContainerView : MonoBehaviour 
{
	public Image image;
	public CanvasRenderer imageRenderer;
	public GameObject counterContainer;
    private Texture _currentTexture;

	public void Awake()
	{
		if (counterContainer != null)
		{
			counterContainer.SetActive(false);
		}
	}

	public void setCount(string count)
	{
		counterContainer.SetActive(true);
		counterContainer.GetComponentInChildren<Text>().text = count;
	}

    public void loadImage(string assetId, RemoteAssetLoader bpRemoteAssetLoader)
    {
        RemoteAssetRequest request = new RemoteAssetRequest();
        request.assetId = assetId;
        request.complete.AddOnce((UnityWebRequest assetRequest) => addTexture(assetRequest));

        bpRemoteAssetLoader.sendRequest(request);
    }

    public void clearImage()
    {
        _currentTexture = null;
        imageRenderer.Clear();
    }

    private void addTexture(UnityWebRequest request)
    {
		setTexture(DownloadHandlerTexture.GetContent(request));
    }

    public void setTexture(Texture texture)
    {
        _currentTexture = texture;
        imageRenderer.SetTexture(_currentTexture);
    }

    /***/
    // Some code to deal with canvas renderers losing their textures when disabled/enabled.
    private bool _textureRefresh = false;

    private void refreshTexture()
    {
		if (_currentTexture != null)
		{
			imageRenderer.SetTexture(_currentTexture);
		}
    }

    void OnEnable()
    {
		_textureRefresh = true;
    }

    void LateUpdate()
    {
		if (_textureRefresh)
		{
			refreshTexture();
			_textureRefresh = false;
		}
    }
    /***/
}
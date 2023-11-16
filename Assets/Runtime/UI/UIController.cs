using UnityEngine;
using signals;
using UnityEngine.UI;
using DG.Tweening;

public class UIController
{
    public Signal startGame;
    public Signal fadeComplete;
    private TextOverlayView _textOverlay;
    private GameObject _fadeScreen;
    private const float FADE_TIME = 0.5f;
    private static string BACKGROUND_CONTAINER_PREFAB = "UI/BackgroundContainer";
    readonly UICreator _uiCreator;
    readonly MainMenuController _mainMenuController;

    public UIController(UICreator uiCreator, MainMenuController mainMenuController, VisualThemeController visualThemeController)
    {
        _uiCreator = uiCreator;
        _mainMenuController = mainMenuController;
        uiCreator.visualThemeController = visualThemeController; 

        startGame = new Signal();
        fadeComplete = new Signal();
    }

    public void showMainMenu(bool useLongFade, bool fadeFromBlack = true)
    {
        if (fadeFromBlack)
        {
            float delay = 1f;

            if (useLongFade)
            {
                delay = 1f;
                fade(false, default(Color), FADE_TIME, delay);
            }

            fade(false, default(Color), FADE_TIME, delay);
        }

        if (!_mainMenuController.ready)
        {
            _mainMenuController.init();
            _mainMenuController.playGame.Add(handlePlayButtonClicked);
            _mainMenuController.show();
        }
        else
        {
            _mainMenuController.show();
        }
    }

    public void showBackground(VisualTheme visualTheme)
    {
        GameObject prefab = Object.Instantiate(Resources.Load(BACKGROUND_CONTAINER_PREFAB)) as GameObject;
        VisualThemeContainer visualThemeContainer = prefab.GetComponent<VisualThemeContainer>();
        visualThemeContainer.Apply(visualTheme);
    }

    public void showTextOverlay(string text)
    {
        if (_textOverlay == null)
        {
            _textOverlay = _uiCreator.createTextOverlay().GetComponent<TextOverlayView>();
        }
        _textOverlay.textField.text = text;
    }

    public void hideTextOverlay()
    {
        if (_textOverlay != null)
        {
            Object.Destroy(_textOverlay.gameObject);
            _textOverlay = null;
        }
    }

	public void fade(bool fadeIn, Color color = default(Color), float fadeTime = FADE_TIME, float delay = 0f)
    {
		if (_fadeScreen == null)
        {
            _fadeScreen = _uiCreator.createFadeScreen();
        }
        else
        {
            _fadeScreen.SetActive(true);
        }

        int initAlpha = 0;
        int finalAlpha = 1;

        if (!fadeIn)
        {
            initAlpha = 1;
            finalAlpha = 0;
        }

        color.a = initAlpha;

        Image image =_fadeScreen.GetComponentInChildren<Image>();
        image.color = color;
		image.DOFade(finalAlpha, fadeTime).SetDelay(delay).OnComplete(()=>fadeCompleted(finalAlpha));
    }

    private void fadeCompleted(int finalAlpha)
    {
        if (finalAlpha == 0) { _fadeScreen.SetActive(false); }
        fadeComplete.Dispatch();
    }

    public void hideMainMenu()
    {
        _mainMenuController.hide();
    }

    public DialogBoxView showConfirmationDialog(string titleString, string messageString,
                                             System.Action confirmButtonAction = null, string confirmButtonText = null,
                                             System.Action cancelButtonAction = null, string cancelButtonText = null)
    {
        return _uiCreator.showConfirmationDialog(titleString, messageString, confirmButtonAction, confirmButtonText, cancelButtonAction, cancelButtonText);
    }

    private void handlePlayButtonClicked()
    {
        startGame.Dispatch();
    }

    private void fitToScreen(GameObject container)
    {
        SpriteRenderer spriteRenderer = container.GetComponent<SpriteRenderer>();

        container.transform.localScale = new Vector3(1, 1, 1);

        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;

        float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        container.transform.localScale = new Vector3(worldScreenWidth / width, worldScreenHeight / height, 1);
    }
}

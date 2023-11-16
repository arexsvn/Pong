using VContainer.Unity;

public class GameController : ITickable, ILateTickable, IFixedTickable, IInitializable
{
    private bool _gamePaused = true;
    readonly UIController _uiController;
    readonly SceneController _sceneController;
    readonly SaveStateController _saveGameController;
    readonly CoroutineRunner _coroutineRunner;
    readonly HudController _hudController;
    readonly AudioController _audioController;
    readonly LocaleManager _localeManager;
    readonly VisualThemeController _visualThemeController;

    public GameController(UIController uiController, 
                          SceneController sceneController, 
                          SaveStateController saveGameController, 
                          CoroutineRunner coroutineRunner,
                          HudController hudController,
                          AudioController audioController,
                          LocaleManager localeManager,
                          VisualThemeController visualThemeController) 
    {
        _uiController = uiController;
        _sceneController = sceneController;
        _saveGameController = saveGameController;
        _coroutineRunner = coroutineRunner;
        _hudController = hudController;
        _audioController = audioController;
        _localeManager = localeManager;
        _visualThemeController = visualThemeController;
    }

    public void Initialize()
    {
        _saveGameController.init();

        _localeManager.init();
        _localeManager.SetLanguage(_saveGameController.CurrentSave.language);

        _visualThemeController.init();

        _hudController.init();
        _audioController.init();
        _sceneController.init(exitGame);

        _uiController.startGame.Add(handleStartGame);
        _uiController.showBackground(_visualThemeController.CurrentTheme);
        showMainMenu(true);

        DG.Tweening.DOTween.SetTweensCapacity(500, 125);
    }

    private void startMainMenuMusic()
    {
        _audioController.play("city_lights", AudioType.Music);
    }

    private void startGameMusic()
    {
        _audioController.fade(AudioType.Music, 0f);

        _coroutineRunner.delayAction(() => { _audioController.play("skyline", AudioType.Music); }, 1.8f);
    }

    private void showMainMenu(bool addFadeDelay)
    {
        _uiController.showMainMenu(addFadeDelay, addFadeDelay);

        if (addFadeDelay)
        {
            _coroutineRunner.delayAction(startMainMenuMusic, 1f);
        }
        else
        {
            startMainMenuMusic();
        }
    }
       
    private void handleStartGame()
    {
        _uiController.hideMainMenu();
        startGame();
    }

    private void exitGame()
    {
        showMainMenu(false);
    }

    private void startGame()
    {
        startGameMusic();

        _sceneController.start();
        _gamePaused = false;
    }

    public void Tick()
    {
        if (_gamePaused)
        {
            return;
        }
    }

    public void FixedTick()
    {
        if (_gamePaused)
        {
            return;
        }
    }

    public void LateTick()
    {
        if (_gamePaused)
        {
            return;
        }
    }
}
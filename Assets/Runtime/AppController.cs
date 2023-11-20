using VContainer.Unity;

public class AppController : ITickable, ILateTickable, IFixedTickable, IInitializable
{
    private bool _gamePaused = true;
    readonly UIController _uiController;
    readonly GameController _gameController;
    readonly SaveStateController _saveGameController;
    readonly CoroutineRunner _coroutineRunner;
    readonly HudController _hudController;
    readonly AudioController _audioController;
    readonly LocaleManager _localeManager;
    readonly VisualThemeController _visualThemeController;

    public AppController(UIController uiController, 
                          GameController gameController, 
                          SaveStateController saveGameController, 
                          CoroutineRunner coroutineRunner,
                          HudController hudController,
                          AudioController audioController,
                          LocaleManager localeManager,
                          VisualThemeController visualThemeController) 
    {
        _uiController = uiController;
        _gameController = gameController;
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

        _gameController.init();
        _gameController.enterGame.Add(gameStarted);
        _gameController.exitGame.Add(exitGame);

        _uiController.initBackground(_visualThemeController.CurrentTheme);
        _uiController.startGame.Add(showGameTypes);

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
       
    private void exitGame()
    {
        showMainMenu(false);
    }

    private void showGameTypes()
    {
        _gameController.showGameTypes();
    }

    private void gameStarted()
    {
        _uiController.hideMainMenu();
        _uiController.hideBackground();

        startGameMusic();

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
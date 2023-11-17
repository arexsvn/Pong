using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LevelController
{
    readonly CourtController _courtController;
    readonly HudController _hudController;
    readonly UICreator _uiCreator;
    readonly SaveStateController _saveStateController;
    readonly CoroutineRunner _coroutineRunner;
    readonly ParticleController _particleController;
    readonly AudioController _audioController;
    readonly VisualThemeController _visualThemeController;
    readonly LocaleManager _localeManager;
    readonly UIController _uiController;
    private System.Action _exit;
    private MessageBoxView _levelDialog;
    private System.Random _rand;
    private Coroutine _fireworksTimer;
    private List<string> _fireworksColors;
    private bool _levelInProgress;

    public LevelController(CourtController courtController, 
                           HudController hudController, 
                           UICreator uiCreator, 
                           SaveStateController saveStateController,
                           CoroutineRunner coroutineRunner,
                           ParticleController particleController,
                           AudioController audioController,
                           VisualThemeController visualThemeController,
                           LocaleManager localeManager,
                           UIController uIController)
    {
        _courtController = courtController;
        _hudController = hudController;
        _uiCreator = uiCreator;
        _saveStateController = saveStateController;
        _coroutineRunner = coroutineRunner;
        _particleController = particleController;
        _audioController = audioController;
        _visualThemeController = visualThemeController;
        _localeManager = localeManager;
        _uiController = uIController;
    }
    public void init(System.Action exit)
    {
        _exit = exit;

        _rand = new System.Random();

        _fireworksColors = new List<string> { "#f200ff", "#00d0ff", "#ffe600", "#0099ff", "#ff00e1", "#b7ff00" };

        _courtController.init();
        _courtController.ready.Add(handleCourtReady);
        _courtController.error.Add(handleCourtError);
        _courtController.playerWon.Add(showEndLevel);

        _hudController.view.backButton.onClick.AddListener(handleBack);
        _hudController.view.themeButton.onClick.AddListener(showTheme);

        _levelDialog = _uiCreator.showMessageBox(null, null, false);
        _levelDialog.baseView.showDialogBackground(false);
        _levelDialog.baseView.fade(true, 0f);

        _localeManager.LanguageChanged.Add(handleLanguageChanged);
    }

    public void start(GameType gameType, string ipAddress = null, string host = null)
    {
        if (_levelInProgress)
        {
            showCourt(true);
        }
        else
        {
            startNextLevel();
        }

        _uiController.hideBackground();
    }
    
    private void showTheme()
    {
        _audioController.play("click", AudioType.Sfx);
        pauseGame(true);
        _visualThemeController.screenClosed.AddOnce(() => pauseGame(false));
        _visualThemeController.show(true);
    }

    private void updateUIVisibility()
    {

    }

    private void pauseGame(bool pause)
    {
        _courtController.gamePaused = pause;
    }

    private void showHud()
    {
        //_hudController.view.themeText.text = theme.title.ToUpperInvariant();
        //_hudController.view.levelText.text = $"{_localeManager.lookup("level")} {_saveStateController.CurrentSave.currentLevel}";
        _hudController.view.show(true, false);
    }

    private void startCourt()
    {
        _coroutineRunner.delayAction(() =>_levelDialog.baseView.fade(true), .3f);

        _courtController.showBoard();
        _courtController.startLevel();

        _coroutineRunner.delayAction(showHud, 1f);
    }

    private string addTextHighlight(string text)
    {
        return $"<size=120%><color=#8cd1ff>{text.ToUpper()}</color></size>";
    }

    private void handleLanguageChanged(string language)
    {

    }

    private void startNextLevel()
    {
        updateUIVisibility();
        
        _levelDialog.baseView.SetTitle($"<size=120%>Next Level</size>");
        _levelDialog.show(true);

        _coroutineRunner.delayAction(startCourt, 2f);

        _audioController.play("level_start", AudioType.Sfx);

        _coroutineRunner.stop(_fireworksTimer);
    }

    private void handleCourtReady()
    {
        if (_courtController.showing)
        {
            pauseGame(false);
            _levelInProgress = true;
        }
    }

    private void handleCourtError(string errorType, string errorMessage)
    {

    }

    private void showCourt(bool show)
    {
        pauseGame(!show);
        _courtController.showBoard(show);
        _hudController.view.show(show, false);
    }

    private void handleBack()
    {
        _uiController.showBackground();
        _audioController.play("click", AudioType.Sfx);
        showCourt(false);
        _exit();
    }

    private void showEndLevel(int winner)
    {
        _levelInProgress = false;

        _hudController.view.show(false, false);

        _saveStateController.save();

        showFireworks();

        string result = "Top Player Won!";
        if (winner < 0) 
        {
            result = "Bottom Player Won!";
        }

        _levelDialog.baseView.SetTitle($"<size=120%>{result}</size>");
        _levelDialog.show(true);
    }

    private void showFireworks()
    {
        string color = _fireworksColors[_rand.Next(_fireworksColors.Count)];
        float randomX = _rand.Next(120, 600);
        float randomY = _rand.Next(600, 1200);
        _particleController.add("fireworks", new Vector2(randomX, randomY), StringUtils.getColorFromHex(color));
        _audioController.play("fireworks", AudioType.Sfx);

        _fireworksTimer = _coroutineRunner.delayAction(showFireworks, (float)GetRandomNumber(0.2, 1.0));
    }

    public double GetRandomNumber(double minimum, double maximum)
    {
        return _rand.NextDouble() * (maximum - minimum) + minimum;
    }
}
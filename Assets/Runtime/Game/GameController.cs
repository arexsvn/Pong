using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController
{
    public signals.Signal exitGame;
    public signals.Signal enterGame;
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
    private DialogBoxView _levelDialog;
    private System.Random _rand;
    private Coroutine _fireworksTimer;
    private List<string> _fireworksColors;
    private bool _waitingForGame;
    private bool _gameFinished;
    readonly NetworkGameplayManager _networkGameplayManager;
    private bool _offline = false;
    private BasicListView _startGameDialog;
    private BasicListView _selectMultiplayerGameTypeDialog;
    private JoinGameView _joinGameView;
    private static string JOIN_GAME_PREFAB = "UI/JoinGame";
    private DialogBoxView _waitingForGameStartDialog;
    private int _maxScore = 3;
    private Dictionary<int, PlayerData> _players;

    public GameController(CourtController courtController, 
                           HudController hudController, 
                           UICreator uiCreator, 
                           SaveStateController saveStateController,
                           CoroutineRunner coroutineRunner,
                           ParticleController particleController,
                           AudioController audioController,
                           VisualThemeController visualThemeController,
                           LocaleManager localeManager,
                           UIController uIController,
                           NetworkGameplayManager networkGameplayManager)
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
        _networkGameplayManager = networkGameplayManager;
    }
    public void init()
    {
        exitGame = new signals.Signal();
        enterGame = new signals.Signal();

        _rand = new System.Random();

        _fireworksColors = new List<string> { "#f200ff", "#00d0ff", "#ffe600", "#0099ff", "#ff00e1", "#b7ff00" };

        _players = new Dictionary<int, PlayerData>();

        _courtController.init();
        _courtController.error.Add(handleCourtError);
        _courtController.playerWon.Add(showEndLevel);

        _networkGameplayManager.Init();
        _networkGameplayManager.playerScored += handlePlayerScore;
        _networkGameplayManager.clientDisconnected += handleClientDisconnected;
        _networkGameplayManager.gameStarted += handleGameStarted;
        _networkGameplayManager.serverStopped += handleServerStopped;
        _networkGameplayManager.gameRestarted += handleGameRestarted;

        _hudController.view.backButton.onClick.AddListener(handleConfirmEndGame);
        _hudController.view.themeButton.onClick.AddListener(showTheme);

        _levelDialog = _uiCreator.showDialog("Match Complete", "Game Over Man!", new List<ButtonVO> {
                                             new ButtonVO(handlePlayAgain, "Play Again"),
                                             new ButtonVO(handleCancelGame, "Exit") });
        _levelDialog.baseView.baseView.showBackground(false);
        _levelDialog.show(false, 0f);

        _localeManager.LanguageChanged.Add(handleLanguageChanged);
    }

    private void showTheme()
    {
        _audioController.play("click", AudioType.Sfx);
        pauseGame(true);
        _visualThemeController.screenClosed.AddOnce(() => pauseGame(false));
        _visualThemeController.show(true);
    }

    private void pauseGame(bool pause)
    {
        _courtController.gamePaused = pause;
    }

    private void showHud()
    {
        //_hudController.view.themeText.text = theme.title.ToUpperInvariant();
        //_hudController.view.levelText.text = $"{_localeManager.lookup("level")} {_saveStateController.CurrentSave.currentLevel}";
        _hudController.setScoreTop(0);
        _hudController.setScoreBottom(0);
        _hudController.view.show(true, false);
    }

    public void showGameTypes()
    {
        _audioController.play("click", AudioType.Sfx);

        if (_offline)
        {
            startAIGame();
            return;
        }

        if (_startGameDialog != null)
        {
            _startGameDialog.Show();
            return;
        }

        _startGameDialog = _uiCreator.showBasicListView("Start Game", new List<ButtonData>
        {
            new ButtonData("versus ai", startAIGame),
            new ButtonData("versus human", showMultiplayerGameTypeSelection)
        });
    }

    private void showMultiplayerGameTypeSelection()
    {
        if (_selectMultiplayerGameTypeDialog != null)
        {
            _selectMultiplayerGameTypeDialog.Show();
            return;
        }

        _selectMultiplayerGameTypeDialog = _uiCreator.showBasicListView("Select Multiplayer Game Type", new List<ButtonData>
        {
            new ButtonData("join game", showJoinGame),
            new ButtonData("host game", startHostGame),
            new ButtonData("start dedicated server", startDedicatedServerGame)
        });
    }

    private void startDedicatedServerGame()
    {
        _networkGameplayManager.StartServer();
        hideDialogs();
        startCourt();
        showWaitingForGameStart();
    }

    private void startHostGame()
    {
        _networkGameplayManager.StartHost();
        hideDialogs();
        startCourt();
        showWaitingForGameStart();
    }

    private void startAIGame()
    {
        hideDialogs();
    }

    private void hideDialogs()
    {
        _startGameDialog.Hide();

        if (_selectMultiplayerGameTypeDialog != null)
        {
            _selectMultiplayerGameTypeDialog.Hide();
        }

        if (_joinGameView != null)
        {
            _joinGameView.fade(true, UITransitions.FADE_TIME);
        }
    }

    private void showWaitingForGameStart()
    {
        _waitingForGame = true;
        if (_waitingForGameStartDialog == null)
        {
            _waitingForGameStartDialog = _uiCreator.showConfirmationDialog("Hold On", "Waiting for game to start...", handleCancelGame, "Cancel");
        }
        _waitingForGameStartDialog.show(true);
    }

    private void hideWaitingForGameStart()
    {
        _waitingForGame = false;
        _waitingForGameStartDialog.show(false);
    }

    private void showJoinGame()
    {
        if (_joinGameView != null)
        {
            _joinGameView.fade(false, UITransitions.FADE_TIME);
            return;
        }

        _joinGameView = Object.Instantiate(Resources.Load<JoinGameView>(JOIN_GAME_PREFAB));
        _joinGameView.ApplyTheme(_uiCreator.visualThemeController.CurrentTheme);
        _joinGameView.joinButton.button.onClick.AddListener(handleJoinGame);
        _joinGameView.backButton.onClick.AddListener(() =>
        {
            _joinGameView.fade(true, UITransitions.FADE_TIME);
        });

        string defaultIp = _networkGameplayManager.CurrentIpAddress;
        if (!string.IsNullOrEmpty(_saveStateController.CurrentSave.lastIpAddress) && _saveStateController.CurrentSave.lastIpAddress.Length == 9)
        {
            defaultIp = _saveStateController.CurrentSave.lastIpAddress;
        }
        _joinGameView.ipAddressText.text = defaultIp;

        _joinGameView.fade(true, 0f);
        _joinGameView.fade(false, UITransitions.FADE_TIME);
    }

    private void handleJoinGame()
    {
        _saveStateController.CurrentSave.lastIpAddress = _joinGameView.ipAddressText.text;
        _saveStateController.save();

        _networkGameplayManager.SetIpAddress(_joinGameView.ipAddressText.text);
        _networkGameplayManager.StartClient();

        hideDialogs();
        startCourt();
        showWaitingForGameStart();
    }

    private void startCourt()
    {
        //_coroutineRunner.delayAction(() =>_levelDialog.baseView.fade(true), .3f);

        enterGame.Dispatch();
        _courtController.showBoard();
        _coroutineRunner.delayAction(showHud, 1f);
    }

    private string addTextHighlight(string text)
    {
        return $"<size=120%><color=#8cd1ff>{text.ToUpper()}</color></size>";
    }

    private void handleLanguageChanged(string language)
    {

    }

    /*
    private void startNextLevel()
    {        
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
            _gameInProgress = true;
        }
    }
    */

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
        _levelDialog.show(false);
        _uiController.showBackground();
        _audioController.play("click", AudioType.Sfx);
        showCourt(false);
        exitGame.Dispatch();
    }

    private void handlePlayAgain()
    {
        _networkGameplayManager.RestartGame();
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

    private void handlePlayerScore(int playerNumber)
    {
        _players[playerNumber].score++;

        _hudController.setScoreTop(_players[0].score);
        _hudController.setScoreBottom(_players[1].score);

        if (_players[playerNumber].score == _maxScore)
        {
            showEndLevel(playerNumber);
        }
    }

    private void showEndLevel(int winner)
    {
        _gameFinished = true;

        _hudController.setScoreTop(0);
        _hudController.setScoreBottom(0);
        _hudController.view.show(false, false);

        _saveStateController.save();

        _networkGameplayManager.EndGame();
        showFireworks();
        _uiController.showBackground();

        string result = "Top Player Won!";
        if (winner > 0)
        {
            result = "Bottom Player Won!";
        }

        _levelDialog.baseView.SetMessage(result);
        _levelDialog.show(true);
    }

    private void handleConfirmEndGame()
    {
        _uiCreator.showConfirmationDialog("Leave Game", "Are you sure you want to leave? This will end the game for all players.", handleEndGame, "Leave", true);
    }

    private void handleEndGame()
    {
        handleClientDisconnected(default);
    }

    private void handleClientDisconnected(ulong clientId)
    {
        if (_waitingForGame || !_gameFinished)
        {
            _uiCreator.showConfirmationDialog("Game Over", "Client Disconnected.");
            cleanupMultiplayerGame();
            handleBack();
        }
    }

    private void handleCancelGame()
    {
        cleanupMultiplayerGame();
        handleBack();
    }

    private void handleGameStarted(List<string> playerUids)
    {
        for(int index = 0; index < playerUids.Count; index++)
        {
            ulong.TryParse(playerUids[index], out ulong uid);
            _players[index] = new PlayerData(index, uid);
        }

        _gameFinished = false;
        hideWaitingForGameStart();
    }

    private void handleGameRestarted()
    {
        _hudController.setScoreTop(0);
        _hudController.setScoreBottom(0);

        foreach (var kvp in _players)
        {
            kvp.Value.score = 0;
        }

        _coroutineRunner.stop(_fireworksTimer);
        _hudController.view.show(true, false);
        _uiController.hideBackground();
        _levelDialog.show(false);

        _gameFinished = false;
    }

    private void handleServerStopped()
    {
        if (_waitingForGame || !_gameFinished)
        {
            _uiCreator.showConfirmationDialog("Game Over", "Server Disconnected.");
            cleanupMultiplayerGame();
            handleBack();
        }
    }

    private void cleanupMultiplayerGame()
    {
        _gameFinished = true;
        _players.Clear();
        _networkGameplayManager.Shutdown();
        hideWaitingForGameStart();
    }
}

public class PlayerData
{
    public ulong clientId;
    public int score;
    public int playerNumber;

    public PlayerData(int playerNumber, ulong clientId = default)
    {
        this.playerNumber = playerNumber;
        this.clientId = clientId;
    }
}
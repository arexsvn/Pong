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
    readonly LocalGameplayManager _localGameplayManager;
    private bool _offline;
    private bool _localGameActive;
    private bool _networkGameActive;
    private BasicListView _startGameDialog;
    private BasicListView _selectMultiplayerGameTypeDialog;
    private JoinGameView _joinGameView;
    private JoinGameRelayView _joinGameRelayView;
    private static string JOIN_GAME_PREFAB = "UI/JoinGame";
    private static string JOIN_GAME_RELAY_PREFAB = "UI/JoinGameRelay";
    private DialogBoxView _waitingForGameStartDialog;
    private int _maxScore = 10;
    private Dictionary<int, PlayerData> _players;
    private string _roomCode;

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
                           NetworkGameplayManager networkGameplayManager,
                           LocalGameplayManager localGameplayManager)
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
        _localGameplayManager = localGameplayManager;
    }
    public void init()
    {
        exitGame = new signals.Signal();
        enterGame = new signals.Signal();

        _rand = new System.Random();

        _fireworksColors = new List<string> { "#f200ff", "#00d0ff", "#ffe600", "#0099ff", "#ff00e1", "#b7ff00" };

        _players = new Dictionary<int, PlayerData>();

        _courtController.init();

        _networkGameplayManager.Init();
        _networkGameplayManager.playerScored += handlePlayerScore;
        _networkGameplayManager.clientDisconnected += handleClientDisconnected;
        _networkGameplayManager.gameStarted += handleGameStarted;
        _networkGameplayManager.serverStopped += handleServerStopped;
        _networkGameplayManager.gameRestarted += handleGameRestarted;

        _localGameplayManager.Init();
        _localGameplayManager.playerScored += handlePlayerScore;
        _localGameplayManager.gameStarted += handleGameStarted;
        _localGameplayManager.gameRestarted += handleGameRestarted;

        _particleController.init();

        _hudController.view.backButton.onClick.AddListener(handleConfirmEndGame);

        _levelDialog = _uiCreator.showDialog("Match Complete", "Game Over Man!", new List<ButtonVO> {
                                             new ButtonVO(handlePlayAgain, "Play Again"),
                                             new ButtonVO(handleCancelGame, "Exit") });
        _levelDialog.baseView.baseView.showBackground(false);
        _levelDialog.show(false, 0f);
    }

    private void showHud()
    {
        _hudController.setScoreTop(0);
        _hudController.setScoreBottom(0);
        _hudController.view.show(true, false);
    }

    public void showGameTypes()
    {
        _audioController.play("snap", AudioType.Sfx);

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
            new ButtonData("versus human", showMultiplayerGameTypeSelection),
#if ALLOW_NON_RELAY_MULTIPLAYER
            new ButtonData("start dedicated server (non-relay)", ()=>startDedicatedServerGame(false)),
#endif
            new ButtonData("start dedicated server", ()=>startDedicatedServerGame(true))
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
#if ALLOW_NON_RELAY_MULTIPLAYER
            new ButtonData("join game (non-relay)", showJoinGame),
            new ButtonData("host game (non-relay)", ()=>startHostGame(false)),
#endif
            new ButtonData("join game", showJoinGameRelay),
            new ButtonData("host game", ()=>startHostGame(true))
        });
    }

    private async void startDedicatedServerGame(bool useRelay)
    {
        setNetworkGameActive();

        if (useRelay) 
        {
            showWaitingForGameStart();
            _roomCode = await _networkGameplayManager.StartServerWithRelay();
        }
        else
        {
            _networkGameplayManager.StartServer();
        }

        hideDialogs();
        startCourt();
        showWaitingForGameStart();
    }

    private async void startHostGame(bool useRelay)
    {
        setNetworkGameActive();

        if (useRelay)
        {
            showWaitingForGameStart();
            _roomCode = await _networkGameplayManager.StartHostWithRelay();
        }
        else
        {
            _networkGameplayManager.StartHost();
        }

        hideDialogs();
        startCourt();
        showWaitingForGameStart();
    }

    private void startAIGame()
    {
        setLocalGameActive();
        hideDialogs();
        startCourt();
        _localGameplayManager.Start();
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

        if (_joinGameRelayView != null)
        {
            _joinGameRelayView.fade(true, UITransitions.FADE_TIME);
        }
    }

    private void showWaitingForGameStart()
    {
        _waitingForGame = true;

        string message = "Waiting for game to start...";

        if (!string.IsNullOrEmpty(_roomCode))
        {
            message = $"Waiting for other players, use room code:\n{addTextHighlight(_roomCode)}";
        }

        if (_waitingForGameStartDialog == null)
        {
            _waitingForGameStartDialog = _uiCreator.showConfirmationDialog("Entered Game", message, handleCancelGame, "Cancel");
        }

        _waitingForGameStartDialog.baseView.SetMessage(message);
        _waitingForGameStartDialog.show(true);
    }

    private string addTextHighlight(string text)
    {
        return $"<size=120%><color=#8cd1ff>{text.ToUpper()}</color></size>";
    }

    private void hideWaitingForGameStart()
    {
        _waitingForGame = false;
        if (_waitingForGameStartDialog != null)
        {
            _waitingForGameStartDialog.show(false);
        }
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
        setNetworkGameActive();

        if (!string.IsNullOrEmpty(_joinGameView.ipAddressText.text))
        {
            _saveStateController.CurrentSave.lastIpAddress = _joinGameView.ipAddressText.text;
            _saveStateController.save();
        }

        _networkGameplayManager.SetIpAddress(_saveStateController.CurrentSave.lastIpAddress);
        _networkGameplayManager.StartClient();

        hideDialogs();
        startCourt();
        showWaitingForGameStart();
    }

    private void showJoinGameRelay()
    {
        if (_joinGameRelayView != null)
        {
            _joinGameRelayView.fade(false, UITransitions.FADE_TIME);
            return;
        }

        _joinGameRelayView = Object.Instantiate(Resources.Load<JoinGameRelayView>(JOIN_GAME_RELAY_PREFAB));
        _joinGameRelayView.ApplyTheme(_uiCreator.visualThemeController.CurrentTheme);
        _joinGameRelayView.joinButton.button.onClick.AddListener(handleJoinGameRelay);
        _joinGameRelayView.backButton.onClick.AddListener(() =>
        {
            _joinGameRelayView.fade(true, UITransitions.FADE_TIME);
        });

        _joinGameRelayView.fade(true, 0f);
        _joinGameRelayView.fade(false, UITransitions.FADE_TIME);
    }

    private async void handleJoinGameRelay()
    {
        if (string.IsNullOrEmpty(_joinGameRelayView.roomNameTextLegacy.text))
        {
            _uiCreator.showErrorDialog("Enter a room code to join game.");
            return;
        }
        
        setNetworkGameActive();
        showWaitingForGameStart();

        bool result = await _networkGameplayManager.StartClientWithRelay(_joinGameRelayView.roomNameTextLegacy.text.ToUpper());

        if (result)
        {
            hideDialogs();
            startCourt();
            showWaitingForGameStart();
        }
        else
        {
            _uiCreator.showErrorDialog("Error joining relay game.");
        }

    }

    private void setNetworkGameActive()
    {
        _roomCode = null;
        _networkGameActive = true;

        if (_localGameActive) 
        {
            _localGameActive = false;
            _localGameplayManager.Cleanup();
        }
    }

    private void setLocalGameActive()
    {
        _localGameActive = true;

        if (_networkGameActive)
        {
            _networkGameActive = false;
            cleanupMultiplayerGame();
        }
    }

    private void startCourt()
    {
        enterGame.Dispatch();
        _courtController.showBoard();
        _coroutineRunner.delayAction(showHud, 1f);
    }

    private void showCourt(bool show)
    {
        _courtController.showBoard(show);
        _hudController.view.show(show, false);
    }

    private void handleBack()
    {
        _levelDialog.show(false);
        _uiController.showBackground();
        _audioController.play("snap", AudioType.Sfx);
        showCourt(false);
        stopFireworks();
        exitGame.Dispatch();
    }

    private void handlePlayAgain()
    {
        if (_localGameActive)
        {
            _localGameplayManager.RestartGame();
        }
        else if (_networkGameplayManager.IsServer)
        {
            _networkGameplayManager.RestartGame();
        }
        else 
        {
            _networkGameplayManager.RequestRestart();
        }
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

    private void stopFireworks()
    {
        _coroutineRunner.stop(_fireworksTimer);
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

        if (_localGameActive)
        {
            _localGameplayManager.EndGame();
        }
        else
        {
            _networkGameplayManager.EndGame();
        }

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
        _uiCreator.showConfirmationDialog("Leave Game", "Are you sure you want to leave? This will end the game for all players.", handleCancelGame, "Leave", true);
    }

    private void handleEndGame()
    {
        if (_localGameActive)
        {
            _localGameActive = false;
            _localGameplayManager.Cleanup();
        }
        if (_networkGameActive)
        {
            _networkGameActive = false;
            cleanupMultiplayerGame();
        }
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
        handleEndGame();
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

        if (_networkGameActive && _networkGameplayManager.IsDedicatedServer)
        {
            _hudController.view.serverStatus.text = "Dedicated Server";
        }
        else
        {
            _hudController.view.serverStatus.text = "";
        }

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

        stopFireworks();
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
        _roomCode = null;
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

public enum GameType
{
    AI,
    Join,
    Host,
    DedicatedServer
}
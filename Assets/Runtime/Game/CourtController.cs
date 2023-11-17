using System.Collections.Generic;
using UnityEngine;
using signals;
using Unity.Netcode;

public class CourtController
{
    public Signal<string, string> error;
    public Signal<int> playerWon;
    public Signal ready;
    private string PREFAB = "Gameplay/Court";
    private string NETWORK_MANAGER_PREFAB = "Gameplay/NetworkManager";
    private string GAMEPLAY_CONTAINER_PREFAB = "Gameplay/GameplayContainer";
    private CourtView _view;
    private NetworkManager _networkManager;
    private GameManager _gameManager;
    private int _maxScore = 3;
    private int _topPlayerScore = 0;
    private int _bottomPlayerScore = 0;

    public List<string> wordsAdded { get; private set; }
    public bool gamePaused { get; set; } = true;

    readonly InputController _inputController;
    readonly UICreator _uiCreator;
    readonly AudioController _audioController;
    readonly ParticleController _particleController;
    readonly CoroutineRunner _coroutineRunner;

    public CourtController(InputController inputController, UICreator uiCreator, AudioController audioController, ParticleController particleController, CoroutineRunner coroutineRunner)
    {
        _inputController = inputController;
        _uiCreator = uiCreator;
        _audioController = audioController;
        _particleController = particleController;
        _coroutineRunner = coroutineRunner;
    }

    public void init()
    {
        error = new Signal<string, string>();
        ready = new Signal();
        playerWon = new Signal<int>();

        create();
    }

    public void startLevel()
    {
        _networkManager = Object.Instantiate(Resources.Load<NetworkManager>(NETWORK_MANAGER_PREFAB));
        _gameManager = Object.Instantiate(Resources.Load<GameManager>(GAMEPLAY_CONTAINER_PREFAB));
        _gameManager.playerScored += handlePlayerScore;

        _coroutineRunner.delayAction(ready.Dispatch, 1f);
    }

    private void handlePlayerScore(int direction)
    {
        if (direction > 0)
        {
            _topPlayerScore++;
        }
        else
        {
            _bottomPlayerScore++;
        }

        if (_topPlayerScore == _maxScore || _bottomPlayerScore == _maxScore)
        {
            playerWon.Dispatch(direction);
        }
    }

    public void showBoard(bool show = true)
    {
        showing = show;
        _view.show(show);
    }

    public bool showing { get; private set; }

    public void Tick()
    {
        if (gamePaused)
        {
            return;
        }
    }


    private void create()
    {
        _view = Object.Instantiate(Resources.Load<CourtView>(PREFAB));
        _particleController.init();
    }
}

public enum GameType
{
    AI,
    Join,
    Host,
    DedicatedServer
}
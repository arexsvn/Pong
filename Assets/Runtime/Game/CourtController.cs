using System.Collections.Generic;
using UnityEngine;
using signals;

public class CourtController
{
    public Signal<string, string> error;
    public Signal<int> playerWon;
    private string PREFAB = "Gameplay/Court";
    private CourtView _view;

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
        playerWon = new Signal<int>();

        create();
    }

    public void showBoard(bool show = true)
    {
        showing = show;
        _view.show(show);
    }

    public bool showing { get; private set; }
    public CourtView View { get => _view; }

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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using signals;
using DG.Tweening;
using System.Text;

public class BoardError
{
    public const string NO_VALID_WORDS = "NO_VALID_WORDS";
}

public class CourtController
{
    public Signal<string, string> error;
    public Signal ready;
    private string PREFAB = "Court/Court";
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
        ready = new Signal();

        create();
    }

    public void startLevel()
    {
        _coroutineRunner.delayAction(ready.Dispatch, 1f);
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
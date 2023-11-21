using UnityEngine;

public class CourtController
{
    private string PREFAB = "Gameplay/Court";
    private CourtView _view;

    public CourtController()
    {

    }

    public void init()
    {
        create();
    }

    public void showBoard(bool show = true)
    {
        showing = show;
        _view.show(show);
    }

    public bool showing { get; private set; }
    public CourtView View { get => _view; }

    private void create()
    {
        _view = Object.Instantiate(Resources.Load<CourtView>(PREFAB));
    }
}
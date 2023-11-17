using UnityEngine;

public class HudController
{
    private static string MAIN_PREFAB = "UI/Hud";
    readonly UICreator _uiCreator;

    public HudController(UICreator uiCreator)
    {
        _uiCreator = uiCreator;
    }

    public void init()
    {
        view = Object.Instantiate(Resources.Load<HudView>(MAIN_PREFAB));
        view.show(false);
        VisualThemeContainer[] visualThemeContainers = view.gameObject.GetComponentsInChildren<VisualThemeContainer>();
        foreach(VisualThemeContainer visualThemeContainer in visualThemeContainers)
        {
            visualThemeContainer.Apply(_uiCreator.visualThemeController.CurrentTheme);
        }
    }

    public void setScoreTop(int score)
    {
        view.scoreTop.text = score.ToString();
    }

    public void setScoreBottom(int score)
    {
        view.scoreBottom.text = score.ToString();
    }

    public HudView view { get; private set; }
}
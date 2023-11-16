using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI.Extensions;
using DG.Tweening;
using System.Linq;

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

    public HudView view { get; private set; }
}
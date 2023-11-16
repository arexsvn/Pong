using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using signals;

public class MainMenuController
{
    public Signal playGame = new Signal();
    private MainMenuView _view;
    private ButtonView _playButtonView;
    private static string MAIN_MENU_PREFAB = "UI/MainMenu";
    readonly UICreator _uiCreator;
    readonly VisualThemeController _themeController;
    readonly SaveStateController _saveStateController;
    readonly SettingsController _settingsController;
    readonly AudioController _audioController;
    readonly LocaleManager _localeManager;

    public MainMenuController(UICreator uiCreator, VisualThemeController themeController, SaveStateController saveStateController, SettingsController settingsController, AudioController audioController, LocaleManager localeManager)
    {
        _uiCreator = uiCreator;
        _themeController = themeController;
        _saveStateController = saveStateController;
        _settingsController = settingsController;
        _audioController = audioController;
        _localeManager = localeManager;
    }

    public void init()
    {
        if (_view != null)
        {
            Object.Destroy(_view.gameObject);
        }

        _localeManager.LanguageChanged.Add(setText);

        _view = Object.Instantiate(Resources.Load<MainMenuView>(MAIN_MENU_PREFAB));

        VisualThemeContainer[] visualThemeContainers = _view.gameObject.GetComponentsInChildren<VisualThemeContainer>();
        foreach (VisualThemeContainer visualThemeContainer in visualThemeContainers)
        {
            visualThemeContainer.Apply(_uiCreator.visualThemeController.CurrentTheme);
        }

        _view.title.text = "";

        _playButtonView = _uiCreator.addButton(_view.buttonContainer, new ButtonVO(handlePlayGame));

        _settingsController.init();

        _view.settingsButton.onClick.AddListener(handleOpenSettings);
        _view.themeButton.onClick.AddListener(handleSelectTheme);
    }

    private void setText(string languageCode = null)
    {
        _playButtonView.labelText.text = $"{_localeManager.lookup("play_game")}";
    }

    public void show()
    {
        setText();

        _view.show(true, false);
        _playButtonView.showHighlight(true);
    }

    public void hide()
    {
        _view.show(false, false);
        _playButtonView.showHighlight(false);
    }

    public void handlePlayGame()
    {
        _audioController.play("click", AudioType.Sfx);
        playGame.Dispatch();
    }

    public void handleSelectTheme()
    {
        _audioController.play("click", AudioType.Sfx);
        _themeController.show();
    }

    public void handleOpenSettings()
    {
        _audioController.play("click", AudioType.Sfx);
        _settingsController.show();
    }

    public bool ready
    {
        get => _view != null;
    }
}
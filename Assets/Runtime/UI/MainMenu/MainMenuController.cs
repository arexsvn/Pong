using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using signals;

public class MainMenuController
{
    public Signal<GameType> startGame = new Signal<GameType>();
    public Signal<string, string> joinGame = new Signal<string, string>();
    private MainMenuView _view;
    private ButtonView _playButtonView;
    private static string MAIN_MENU_PREFAB = "UI/MainMenu";
    private static string JOIN_GAME_PREFAB = "UI/JoinGame";
    readonly UICreator _uiCreator;
    readonly VisualThemeController _themeController;
    readonly SaveStateController _saveStateController;
    readonly SettingsController _settingsController;
    readonly AudioController _audioController;
    readonly LocaleManager _localeManager;
    private bool _offline = false;
    private BasicListView _startGameDialog;
    private BasicListView _selectMultiplayerGameTypeDialog;
    private JoinGameView _joinGameView;

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
        hideDialogs();
        startGame.Dispatch(GameType.DedicatedServer);
    }

    private void startHostGame()
    {
        hideDialogs();
        startGame.Dispatch(GameType.Host);
    }

    private void startAIGame()
    {
        hideDialogs();
        startGame.Dispatch(GameType.AI);
    }

    private void hideDialogs()
    {
        _startGameDialog.Hide();
        _selectMultiplayerGameTypeDialog.Hide();
        _joinGameView.fade(true, UITransitions.FADE_TIME);
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
    }

    private void handleJoinGame()
    {
        hideDialogs();
        joinGame.Dispatch(_joinGameView.ipAddressText.text, _joinGameView.hostText.text);
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
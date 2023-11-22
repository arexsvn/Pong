using UnityEngine;
using signals;

public class SettingsController
{
    public Signal screenClosed;
    private SettingsView _view;
    private LanguageSelectionView _languageSelectionView;
    private const string MAIN_PREFAB = "UI/Settings";
    private const string LANGUAGE_SELECTION_PREFAB = "UI/LanguageSelection";
    private const string LANGUAGE_SELECTION_LISTITEM_PREFAB = "UI/LanguageListItem";
    readonly SaveStateController _saveStateController;
    readonly UICreator _uiCreator;
    readonly AudioController _audioController;
    readonly LocaleManager _localeManager;

    public SettingsController(SaveStateController saveStateController, 
                              UICreator uiCreator, 
                              AudioController audioController, 
                              LocaleManager localeManager)
    {
        _saveStateController = saveStateController;
        _uiCreator = uiCreator;
        _audioController = audioController;
        _localeManager = localeManager;
    }

    public void init()
    {
        screenClosed = new Signal();

        _localeManager.LanguageChanged.Add(setText);

        _view = Object.Instantiate(Resources.Load<SettingsView>(MAIN_PREFAB));
        _view.baseView.backButton.onClick.AddListener(() => show(false));

        _view.musicVolumeSlider.slider.value = _saveStateController.CurrentSave.musicVolume;
        _view.musicVolumeSlider.slider.onValueChanged.AddListener(handleMusicVolumeChanged);

        _view.sfxVolumeSlider.slider.value = _saveStateController.CurrentSave.sfxVolume;
        _view.sfxVolumeSlider.slider.onValueChanged.AddListener(handleSfxVolumeChanged);

        _view.languageSelectionButton.button.onClick.AddListener(showLanguageSelection);

        VisualThemeContainer[] visualThemeContainers = _view.GetComponentsInChildren<VisualThemeContainer>();
        foreach (VisualThemeContainer visualThemeContainer in visualThemeContainers)
        {
            visualThemeContainer.Apply(_uiCreator.visualThemeController.CurrentTheme);
        }

        setupLanguageSelection();

        setText();

        _view.baseView.fade(true, 0f);
    }

    private void setText(string languageCode = null)
    {
        _view.baseView.SetTitle(_localeManager.lookup("settings"));
        _view.musicVolumeSlider.labelText.text = _localeManager.lookup("music_volume");
        _view.sfxVolumeSlider.labelText.text = _localeManager.lookup("sound_effects_volume");
        _view.languageSelectionButton.labelText.text = _localeManager.lookup("language");
        _languageSelectionView.dialogWithItemScrollView.baseView.SetTitle(_localeManager.lookup("select_language"));
    }

    private void setupLanguageSelection()
    {
        _languageSelectionView = Object.Instantiate(Resources.Load<LanguageSelectionView>(LANGUAGE_SELECTION_PREFAB));
        _languageSelectionView.dialogWithItemScrollView.baseView.backButton.onClick.AddListener(closeLanguageSelection);

        foreach (Language language in _localeManager.Locale.languages)
        {
            LanguageListItemView languageListItem = Object.Instantiate(Resources.Load<LanguageListItemView>(LANGUAGE_SELECTION_LISTITEM_PREFAB), _languageSelectionView.dialogWithItemScrollView.itemContainer.transform);
            languageListItem.Set(language, handleLanguageSelection, _localeManager.LanguageCode == language.code);
        }

        _languageSelectionView.dialogWithItemScrollView.baseView.fade(true, 0f);
    }

    public void show(bool show = true)
    {
        _view.baseView.fade(!show);
        _audioController.play("snap", AudioType.Sfx);

        if (!show)
        {
            closeLanguageSelection();
            screenClosed.Dispatch();
        }
    }

    private void handleMusicVolumeChanged(float value)
    {
        _audioController.setVolume(AudioType.Music, value);

        _saveStateController.CurrentSave.musicVolume = value;
        _saveStateController.save();
    }

    private void handleSfxVolumeChanged(float value)
    {
        _audioController.setVolume(AudioType.Sfx, value);

        _saveStateController.CurrentSave.sfxVolume = value;
        _saveStateController.save();
    }

    private void handleLanguageSelection(Language language)
    {
        if (language.code != _localeManager.LanguageCode)
        {
            _audioController.play("snap", AudioType.Sfx);

            LanguageListItemView[] views = _languageSelectionView.dialogWithItemScrollView.itemContainer.transform.GetComponentsInChildren<LanguageListItemView>();
            foreach (LanguageListItemView view in views)
            {
                view.select(view.currentLanguage.code == language.code);
            }

            _localeManager.SetLanguage(language.code);
            _saveStateController.CurrentSave.language = language.code;
            _saveStateController.save();
        }
    }

    private void closeLanguageSelection()
    {
        _audioController.play("snap", AudioType.Sfx);
        _languageSelectionView.dialogWithItemScrollView.baseView.fade(true);
    }
    private void showLanguageSelection()
    {
        _audioController.play("snap", AudioType.Sfx);
        _languageSelectionView.dialogWithItemScrollView.baseView.fade(false);
    }
}

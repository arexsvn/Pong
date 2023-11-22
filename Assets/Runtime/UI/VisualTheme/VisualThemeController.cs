using signals;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VisualThemeController
{
    public Signal screenClosed;
    public Signal<VisualTheme> themeSelected;
    private VisualThemeSelectionView _view;
    private static string MAIN_PREFAB = "UI/VisualThemeSelection";
    private static string ITEM_PREFAB = "UI/VisualThemeListItem";
    private Dictionary<string, VisualTheme> _themes;
    private List<string> _themeOrder;
    private List<int> _levelThemeUnlock;
    public static float TRANSITION_TIME = .8f;
    readonly SaveStateController _saveGameController;
    readonly UICreator _uiCreator;
    readonly AudioController _audioController;
    readonly LocaleManager _localeManager;

    public VisualThemeController(SaveStateController saveGameController, 
                                 UICreator uiCreator, 
                                 AudioController audioController,
                                 LocaleManager localeManager)
    {
        _saveGameController = saveGameController;
        _uiCreator = uiCreator;
        _audioController = audioController;
        _localeManager = localeManager;
    }

    public void init()
    {
        screenClosed = new Signal();
        themeSelected = new Signal<VisualTheme>();
        loadThemes();

        _localeManager.LanguageChanged.Add(setText);

        show(false);
    }

    public void show(bool show = true)
    {
        if (_view == null)
        {
            GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(MAIN_PREFAB));
            _view = prefab.GetComponent<VisualThemeSelectionView>();
            _view.dialogWithItemScrollView.baseView.backButton.onClick.AddListener(close);
            _view.dialogWithItemScrollView.baseView.canvasGroup.alpha = 0f;

            VisualThemeContainer[] visualThemeContainers = prefab.GetComponentsInChildren<VisualThemeContainer>();
            foreach(VisualThemeContainer visualThemeContainer in visualThemeContainers)
            {
                visualThemeContainer.Apply(CurrentTheme);
            }

            setText();

            foreach (string themeId in _themeOrder)
            {
                displayThemeItem(_themes[themeId]);
            }
        }

        _view.dialogWithItemScrollView.baseView.fade(!show);

        if (!show)
        {
            screenClosed.Dispatch();
        }
        else
        {
            VisualThemeListItemView[] views = _view.dialogWithItemScrollView.itemContainer.transform.GetComponentsInChildren<VisualThemeListItemView>();
            /*
            foreach (VisualThemeListItemView view in views)
            {
                bool locked = _themeOrder.IndexOf(view.visualTheme.id) > highestThemeIndex;
                view.selectButton.interactable = !locked;
            }
            */
        }
    }

    public void close()
    {
        show(false);
        _audioController.play("snap", AudioType.Sfx);
    }

    public VisualTheme DefaultTheme
    {
        get
        {
            return _themes[_themeOrder[0]];
        }
    }

    public VisualTheme CurrentTheme
    {
        get
        {
            VisualTheme visualTheme = DefaultTheme;
            if (!string.IsNullOrEmpty(_saveGameController.CurrentSave.currentTheme) && _themes.ContainsKey(_saveGameController.CurrentSave.currentTheme))
            {
                visualTheme = _themes[_saveGameController.CurrentSave.currentTheme];
            }
            else
            {
                _saveGameController.CurrentSave.currentTheme = visualTheme.id;
                _saveGameController.save();
            }
            return visualTheme;
        }
    }

    public void switchTheme(string theme)
    {
        selectTheme(_themes[theme]);
    }

    public int getHighestThemeUnlockIndex(int level)
    {
        int totalThemes = _themeOrder.Count;

        for (int n = 0; n < totalThemes; n++)
        {
            if (_levelThemeUnlock[n] > level)
            {
                return n - 1;
            }
        }

        return totalThemes - 1;
    }

    public int getNextUnlockLevel(int level)
    {
        int nextUnlockLevel = 0;
        int totalThemes = _themeOrder.Count;

        for (int n = 0; n < totalThemes; n++)
        {
            if (_levelThemeUnlock[n] >= level)
            {
                nextUnlockLevel = _levelThemeUnlock[n];
                break;
            }
        }

        return nextUnlockLevel;
    }

    public VisualTheme getNextTheme(int level)
    {
        string nextThemeId = _themeOrder[_levelThemeUnlock.IndexOf(getNextUnlockLevel(level))];
        return _themes[nextThemeId];
    }

    private void setText(string languageCode = null)
    {
        _view.dialogWithItemScrollView.baseView.SetTitle(_localeManager.lookup("destinations"));

        VisualThemeListItemView[] views = _view.dialogWithItemScrollView.itemContainer.transform.GetComponentsInChildren<VisualThemeListItemView>();
        foreach (VisualThemeListItemView view in views)
        {
            view.SetLabel(_localeManager.lookup(view.visualTheme.labelLocaleKey));
        }
    }

    private void displayThemeItem(VisualTheme visualTheme)
    {
        bool selected = CurrentTheme.id == visualTheme.id;

        VisualThemeListItemView themeListItemView = Object.Instantiate(Resources.Load<VisualThemeListItemView>(ITEM_PREFAB), _view.dialogWithItemScrollView.itemContainer.transform);
        themeListItemView.Set(visualTheme, _localeManager.lookup(visualTheme.labelLocaleKey), handleSelectTheme, selected);

        VisualThemeContainer visualThemeContainer = themeListItemView.gameObject.GetComponent<VisualThemeContainer>();
        visualThemeContainer.Apply(CurrentTheme);
    }

    private void handleSelectTheme(VisualTheme visualTheme)
    {
        _audioController.play("snap", AudioType.Sfx);
        selectTheme(visualTheme);
    }

    private void selectTheme(VisualTheme visualTheme)
    {
        if (!string.IsNullOrEmpty(_saveGameController.CurrentSave.currentTheme) && visualTheme.id == _saveGameController.CurrentSave.currentTheme)
        {
            return;
        }

        _saveGameController.CurrentSave.currentTheme = visualTheme.id;
        _saveGameController.save();

        VisualThemeListItemView[] views = _view.dialogWithItemScrollView.itemContainer.transform.GetComponentsInChildren<VisualThemeListItemView>();
        foreach (VisualThemeListItemView view in views)
        {
            view.select(view.visualTheme.id == visualTheme.id);
        }

        applyTheme(visualTheme);

        themeSelected.Dispatch(visualTheme);
    }

    public void applyTheme(VisualTheme visualTheme)
    {
        Scene scene = SceneManager.GetActiveScene();
        List<GameObject> rootObjectsInScene = new List<GameObject>();
        scene.GetRootGameObjects(rootObjectsInScene);

        foreach (GameObject gameObject in rootObjectsInScene)
        {
            VisualThemeContainer[] visualThemeContainers = gameObject.GetComponentsInChildren<VisualThemeContainer>(true);
            foreach (VisualThemeContainer visualThemeContainer in visualThemeContainers)
            {
                visualThemeContainer.Apply(visualTheme, TRANSITION_TIME);
            }
        }
    }

    // TODO - move to data?
    private void loadThemes()
    {
        _levelThemeUnlock = new List<int> { 0, 5, 10, 15, 20, 40, 60, 80, 100, 140, 180, 220, 260, 300, 400, 500, 600 };
        _themeOrder = new List<string> { "finland", "rome", "china", "arizona", "greece", "istanbul", "london", "machu_picchu", "moscow", "paris", "phuket", "san_francisco", "tokyo", "bali" };
        _themes = new Dictionary<string, VisualTheme>();

        ColorTheme colorThemeBlue = new ColorTheme();
        colorThemeBlue.id = "blue";
        colorThemeBlue.primaryColor = "#003B8A";
        colorThemeBlue.highlightColor = "#00b7ff";
        colorThemeBlue.borderColor = "#152C7Eb3";// "#000D3F";
        colorThemeBlue.backgroundColor = "#0F236D";
        colorThemeBlue.disabledColor = "#d3d3d3";
        colorThemeBlue.primaryTextColor = "#ffffff";// "#92D3FF";//#0099FF";
        colorThemeBlue.highlightTextColor = "#ffffff";//"#58d7ff";
        colorThemeBlue.titleTextColor = "#ffffff";//"#9ED8FF";

        ColorTheme colorThemeGreen = new ColorTheme();
        colorThemeGreen.id = "green";
        colorThemeGreen.primaryColor = "#0d851f";
        colorThemeGreen.highlightColor = "#35C600";
        colorThemeGreen.borderColor = "#22722Eb3";//"#0d2e12";
        colorThemeGreen.backgroundColor = "#156321";//"#0d2e12";
        colorThemeGreen.disabledColor = "#d3d3d3";
        colorThemeGreen.primaryTextColor = "#ffffff";//"#A8EFB3";
        colorThemeGreen.highlightTextColor = "#ffffff";//"#56FF70";
        colorThemeGreen.titleTextColor = "#ffffff";//"#35DB4A";//"#7CFF8C";

        ColorTheme colorThemeSea = new ColorTheme();
        colorThemeSea.id = "sea";
        colorThemeSea.primaryColor = "#10959e";
        colorThemeSea.highlightColor = "#00D99D";
        colorThemeSea.borderColor = "#007079b3";
        colorThemeSea.backgroundColor = "#0F5D63";
        colorThemeSea.disabledColor = "#d3d3d3";
        colorThemeSea.primaryTextColor = "#ffffff";//"#BBFBFF";
        colorThemeSea.highlightTextColor = "#ffffff";//"#31FBE8";
        colorThemeSea.titleTextColor = "#ffffff";//"#84F8FF";

        ColorTheme colorThemePurple = new ColorTheme();
        colorThemePurple.id = "purple";
        colorThemePurple.primaryColor = "#8D109E";
        colorThemePurple.highlightColor = "#d900ff";
        colorThemePurple.borderColor = "#900096b3";
        colorThemePurple.backgroundColor = "#6F1072";
        colorThemePurple.disabledColor = "#d3d3d3";
        colorThemePurple.primaryTextColor = "#ffffff";//"#F8BBFF";
        colorThemePurple.highlightTextColor = "#ffffff";//"#FF84EB";
        colorThemePurple.titleTextColor = "#ffffff";//"#D484FF";

        ColorTheme colorThemeRed = new ColorTheme();
        colorThemeRed.id = "red";
        colorThemeRed.primaryColor = "#d43a17";
        colorThemeRed.highlightColor = "#FF7E00";
        colorThemeRed.borderColor = "#8a1e06b3";
        colorThemeRed.backgroundColor = "#741E10";// "#5e1606";
        colorThemeRed.disabledColor = "#d3d3d3";
        colorThemeRed.primaryTextColor = "#ffffff";//"#ffcdbb";
        colorThemeRed.highlightTextColor = "#ffffff";//"#ffc900";// "#FFB300";
        colorThemeRed.titleTextColor = "#ffffff";//"#FF7E32";

        VisualTheme visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_bali";
        visualTheme.id = "bali";
        visualTheme.backgroundImage = "bali_alt";
        visualTheme.colorTheme = colorThemeBlue;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_china";
        visualTheme.id = "china";
        visualTheme.backgroundImage = "china_alt";
        visualTheme.colorTheme = colorThemeGreen;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_finland";
        visualTheme.id = "finland";
        visualTheme.backgroundImage = "finland_alt";
        visualTheme.colorTheme = colorThemeBlue;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_arizona";
        visualTheme.id = "arizona";
        visualTheme.backgroundImage = "grand_canyon_alt";
        visualTheme.colorTheme = colorThemeRed;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_greece";
        visualTheme.id = "greece";
        visualTheme.backgroundImage = "greece_alt";
        visualTheme.colorTheme = colorThemeBlue;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_istanbul";
        visualTheme.id = "istanbul";
        visualTheme.backgroundImage = "istanbul_alt";
        visualTheme.colorTheme = colorThemePurple;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_london";
        visualTheme.id = "london";
        visualTheme.backgroundImage = "london_alt";
        visualTheme.colorTheme = colorThemeBlue;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_machu_picchu";
        visualTheme.id = "machu_picchu";
        visualTheme.backgroundImage = "machu_picchu_alt";
        visualTheme.colorTheme = colorThemeGreen;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_moscow";
        visualTheme.id = "moscow";
        visualTheme.backgroundImage = "moscow_alt";
        visualTheme.colorTheme = colorThemeRed;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_paris";
        visualTheme.id = "paris";
        visualTheme.backgroundImage = "paris_alt";
        visualTheme.colorTheme = colorThemePurple;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_phuket";
        visualTheme.id = "phuket";
        visualTheme.backgroundImage = "phuket_alt";
        visualTheme.colorTheme = colorThemeSea;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_rome";
        visualTheme.id = "rome";
        visualTheme.backgroundImage = "rome_alt";
        visualTheme.colorTheme = colorThemeSea;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_san_francisco";
        visualTheme.id = "san_francisco";
        visualTheme.backgroundImage = "san_francisco_alt";
        visualTheme.colorTheme = colorThemeRed;
        _themes[visualTheme.id] = visualTheme;

        visualTheme = new VisualTheme();
        visualTheme.labelLocaleKey = "destination_tokyo";
        visualTheme.id = "tokyo";
        visualTheme.backgroundImage = "tokyo_alt";
        visualTheme.colorTheme = colorThemePurple;
        _themes[visualTheme.id] = visualTheme;
    }
}

[System.Serializable]
public class VisualTheme
{
    public string id;
    public string labelLocaleKey;
    public string backgroundImage;
    public ColorTheme colorTheme;

    public void setColorThemeColors(ColorThemeColors colors)
    {
        colorTheme = new ColorTheme();
        colorTheme.id = colors.id;
        colorTheme.primaryColor = "#" + ColorUtility.ToHtmlStringRGBA(colors.primaryColor);
        colorTheme.highlightColor = "#" + ColorUtility.ToHtmlStringRGBA(colors.highlightColor);
        colorTheme.borderColor = "#" + ColorUtility.ToHtmlStringRGBA(colors.borderColor);
        colorTheme.backgroundColor = "#" + ColorUtility.ToHtmlStringRGBA(colors.backgroundColor);
        colorTheme.disabledColor = "#" + ColorUtility.ToHtmlStringRGBA(colors.disabledColor);
        colorTheme.primaryTextColor = "#" + ColorUtility.ToHtmlStringRGBA(colors.primaryTextColor);
        colorTheme.highlightTextColor = "#" + ColorUtility.ToHtmlStringRGBA(colors.highlightTextColor);
        colorTheme.titleTextColor = "#" + ColorUtility.ToHtmlStringRGBA(colors.titleTextColor);
    }

    public ColorThemeColors getColorThemeColors()
    {
        ColorThemeColors colors = new ColorThemeColors();
        colors.id = colorTheme.id;
        colors.primaryColor = StringUtils.getColorFromHex(colorTheme.primaryColor);
        colors.highlightColor = StringUtils.getColorFromHex(colorTheme.highlightColor);
        colors.borderColor = StringUtils.getColorFromHex(colorTheme.borderColor);
        colors.backgroundColor = StringUtils.getColorFromHex(colorTheme.backgroundColor);
        colors.disabledColor = StringUtils.getColorFromHex(colorTheme.disabledColor);
        colors.primaryTextColor = StringUtils.getColorFromHex(colorTheme.primaryTextColor);
        colors.highlightTextColor = StringUtils.getColorFromHex(colorTheme.highlightTextColor);
        colors.titleTextColor = StringUtils.getColorFromHex(colorTheme.titleTextColor);

        return colors;
    }
}

[System.Serializable]
public class ColorTheme
{
    public string id;
    public string primaryColor;
    public string highlightColor;
    public string borderColor;
    public string backgroundColor;
    public string disabledColor;
    public string primaryTextColor;
    public string highlightTextColor;
    public string titleTextColor;
}

[System.Serializable]
public class ColorThemeColors
{
    public string id;
    public Color primaryColor;
    public Color highlightColor;
    public Color borderColor;
    public Color backgroundColor;
    public Color disabledColor;
    public Color primaryTextColor;
    public Color highlightTextColor;
    public Color titleTextColor;
}
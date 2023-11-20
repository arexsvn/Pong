using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class UICreator
{
	public VisualThemeController visualThemeController;
	public const string BUTTON_PATH = "UI/Button";
	public const string TEXT_AREA_PATH = "UI/TextArea";
	public const string DIALOG_PATH = "UI/DialogBox";
	public const string DIALOG_WITH_IMAGE_PATH = "UI/DialogBoxWithImage";
	public const string MESSAGE_BOX = "UI/MessageBox";
	public const string TOOLTIP = "UI/Tooltip";
    private static string FADE_SCREEN_PREFAB = "UI/ScreenFade";
    private static string TEXT_OVERLAY_PREFAB = "UI/TextOverlay";
    public const string LIST_VIEW_PATH = "UI/SimpleListView";

    public ButtonView addButton(Transform container, ButtonVO vo)
    {
        string buttonPrefabResourceKey = BUTTON_PATH;

        if (vo.prefabResourceKey != null)
        {
            buttonPrefabResourceKey = vo.prefabResourceKey;
        }

		ButtonView buttonView = Object.Instantiate(Resources.Load<ButtonView>(buttonPrefabResourceKey), container);
		VisualThemeContainer visualThemeContainer = buttonView.gameObject.GetComponent<VisualThemeContainer>();
		visualThemeContainer.Apply(visualThemeController.CurrentTheme);
		string textColor = null;
        buttonView.button.onClick.RemoveAllListeners();
        buttonView.button.onClick.AddListener(vo.action);
        buttonView.labelText.text = vo.label;

        if (vo.textColor != null)
        {
            textColor = vo.textColor;
        }

        Color newColor;

        if (textColor != null)
        {
            ColorUtility.TryParseHtmlString(textColor, out newColor);
            buttonView.labelText.color = newColor;
        }

        if (vo.backgroundColor != null)
        {
            ColorUtility.TryParseHtmlString(vo.backgroundColor, out newColor);
            buttonView.buttonImage.color = newColor;
        }

        buttonView.button.interactable = vo.interactable;

        return buttonView;
    }

	public MessageBoxView showTooltip(string message, string prefabResourceKey = TOOLTIP)
	{
		MessageBoxView view = Object.Instantiate(Resources.Load<MessageBoxView>(prefabResourceKey));

		view.SetMessage(message);
		view.baseView.fade(false);
		view.baseView.ApplyTheme(visualThemeController.CurrentTheme);

		return view;
	}

	public MessageBoxView showMessageBox(string title, string description, bool showBack, string prefabResourceKey = MESSAGE_BOX)
	{
		MessageBoxView view = Object.Instantiate(Resources.Load<MessageBoxView>(prefabResourceKey));

		view.baseView.SetTitle(title);
		view.SetMessage(description);
		view.baseView.fade(false);
		view.baseView.ApplyTheme(visualThemeController.CurrentTheme);
		view.baseView.ShowBack(showBack);

		return view;
	}

	public DialogBoxView showDialog(string title, string description, List<ButtonVO> buttons = null, string prefabResourceKey = DIALOG_PATH)
	{
		DialogBoxView view = Object.Instantiate(Resources.Load<DialogBoxView>(prefabResourceKey));
		updateDialog(view, title, description, buttons);
		view.baseView.baseView.fade(false);
		view.baseView.baseView.ApplyTheme(visualThemeController.CurrentTheme);

		return view;
	}

	public BasicListView showBasicListView(string title, List<ButtonData> buttons = null, string prefabResourceKey = LIST_VIEW_PATH)
	{
		BasicListView view = Object.Instantiate(Resources.Load<BasicListView>(prefabResourceKey));
        view.Init();
		view.Set(title, buttons);
        view.ApplyTheme(visualThemeController.CurrentTheme);

        return view;
    }

	public DialogBoxWithImageView showDialogWithImage(string title, string description, Sprite sprite, List<ButtonVO> buttons = null, string prefabResourceKey = DIALOG_WITH_IMAGE_PATH)
	{
		DialogBoxView view = showDialog(title, description, buttons, prefabResourceKey);
		view.baseView.baseView.ShowBack(false);

		DialogBoxWithImageView dialogBoxWithImageView = view.gameObject.GetComponent<DialogBoxWithImageView>();
		dialogBoxWithImageView.showSprite(sprite);

		return dialogBoxWithImageView;
	}

	public void updateDialog(DialogBoxView dialogBoxView, string title, string description, List<ButtonVO> buttons = null)
    {
		dialogBoxView.baseView.baseView.SetTitle(title);
		dialogBoxView.baseView.SetMessage(description);
		dialogBoxView.clearButtons();

		bool showBack = false;

		if (buttons != null)
        {
            foreach (ButtonVO vo in buttons)
            {
				if (vo.action == null)
                {
					vo.action = () => dialogBoxView.baseView.baseView.fade(true, -1, true);
				}

				if (vo.isBack)
                {
					showBack = true;
					dialogBoxView.baseView.baseView.backButton.onClick.RemoveAllListeners();
					dialogBoxView.baseView.baseView.backButton.onClick.AddListener(vo.action);
				}
				else
                {
					addButton(dialogBoxView.buttonContainer, vo);
				}
            }
        }

		dialogBoxView.baseView.baseView.ShowBack(showBack);
	}

    public DialogBoxView showErrorDialog(string errorString, string errorTitle = null, System.Action actionToRetry = null, string buttonText = null, bool allowCancel = false, System.Action actionOnClose = null)
	{
		DialogBoxView dialogBoxView = null;
		List<ButtonVO> actions = new List<ButtonVO>();

        UnityAction closeDialog = () =>
		{
            actionOnClose?.Invoke();
			dialogBoxView.baseView.baseView.fade(true, -1, true);
		};

		if (errorTitle == null)
        {
			errorTitle = "ERROR";
		}

		if (actionToRetry != null)
		{
			if (buttonText == null)
			{
				buttonText = "TRY AGAIN";
			}

			UnityAction retryAction = () =>
			{
				dialogBoxView.baseView.baseView.fade(true, -1, true);
				actionToRetry();
			};
			actions.Add(new ButtonVO(retryAction, buttonText));
		}
		else
		{
			if (buttonText == null)
			{
				buttonText = "OK";
			}

			actions.Add(new ButtonVO(closeDialog, buttonText));
		}

		if (allowCancel)
		{
			actions.Add(new ButtonVO(closeDialog, "CANCEL"));
		}

		dialogBoxView = showDialog(errorTitle, errorString, actions);

		return dialogBoxView;
	}

	public DialogBoxView showConfirmationDialog(string titleString, string messageString, 
											 System.Action confirmButtonAction = null, string confirmButtonText = null, 
		                                     bool showCancel = false, System.Action cancelButtonAction = null, string cancelButtonText = null)
	{
		DialogBoxView dialogBoxView = null;
		List<ButtonVO> actions = new List<ButtonVO>();

		if (confirmButtonText == null)
		{
			confirmButtonText = "OK";
		}

		if (confirmButtonAction != null)
		{
            UnityAction doOkAction = delegate
			{
				dialogBoxView.baseView.baseView.fade(true, -1, true);
				confirmButtonAction();
			};
			actions.Add(new ButtonVO(doOkAction, confirmButtonText));
		}
		else
		{
			actions.Add(new ButtonVO(() => dialogBoxView.baseView.baseView.fade(true, -1, true), confirmButtonText));
		}

		if (showCancel)
		{
			if (cancelButtonText == null)
			{
				cancelButtonText = "CANCEL";
			}

            UnityAction doCancelAction = delegate
			{
				dialogBoxView.baseView.baseView.fade(true, -1, true);
				cancelButtonAction?.Invoke();
			};
			actions.Add(new ButtonVO(doCancelAction, cancelButtonText));
		}

		dialogBoxView = showDialog(titleString, messageString, actions);

		return dialogBoxView;
	}

	public GameObject loadTextAreaPrefab(Transform parent = null)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load(TEXT_AREA_PATH), parent);

		return gameObject;
	}

    public GameObject createTextOverlay()
    {
        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(TEXT_OVERLAY_PREFAB));

        return prefab;
    }

    public GameObject createFadeScreen()
    {
        GameObject prefab = (GameObject)Object.Instantiate(Resources.Load(FADE_SCREEN_PREFAB));

        return prefab;
    }
}

public class ButtonVO
{
    public UnityAction action;
    public string label;
	public string prefabResourceKey;
	public string iconResourceKey;
	public bool interactable = true;
	public string textColor;
	public string backgroundColor;
	public bool isBack = false;
	public const string GOLD_TEXT_COLOR = "#724B1AFF";
    public const string GOLD_BG_COLOR = "#FFD96FFF";
    public const string DISABLED_TEXT_COLOR = "#808080FF";
    public const string DISABLED_BG_COLOR = "#2F3C45FF";
    public const string BLUE_BG_COLOR = "#445F80C8";
    public const string WHITE_BG_COLOR = "#FDFDFDFF";
	public const string BLUE_TEXT_COLOR = "#284461FF";
	public const string WHITE_TEXT_COLOR = "#FDFDFDFF";

	public ButtonVO(UnityAction action, string label = null, string iconResourceKey = null)
	{
		this.action = action;
		this.label = label;
		this.iconResourceKey = iconResourceKey;
	}
}
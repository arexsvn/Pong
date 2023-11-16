using UnityEngine;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine.UI;
using System;

public class DebugSettingsItemView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private TMP_InputField _currentValueInput;
    [SerializeField] private Button _button;
    private FieldInfo _fieldInfo;
    private Action<DebugSettingsItemView> _clickedCallback;

    public FieldInfo FieldInfo
    {
        get => _fieldInfo;
    }

    public object CurrentValue
    {
        get => _fieldInfo.GetValue(DebugSettingsController.instance.settings);
    }

    void Start()
    {
        _button.onClick.AddListener(HandleClicked);
        _currentValueInput.onEndEdit.AddListener(HandleTextEdit);
    }

    public void EnableTextInput(bool enable)
    {
        _currentValueInput.enabled = enable;
    }

    public void SetTextInput()
    {
        _currentValueInput.ActivateInputField();
    }

    public void Set(FieldInfo fieldInfo, object value, Action<DebugSettingsItemView> action)
    {
        _fieldInfo = fieldInfo;

        _label.text = fieldInfo.Name;

        UpdateValue(value);

        _clickedCallback = action;
    }

    public void UpdateValue(object value)
    {
        _fieldInfo.SetValue(DebugSettingsController.instance.settings, value);
        string valueString = value?.ToString();

        if (string.IsNullOrEmpty(valueString))
        {
            _currentValueInput.text = "Undefined";
        }
        else
        {
            _currentValueInput.text = valueString;
        }
    }

    private void HandleTextEdit(string text)
    {
        object newValue = null;

        if (int.TryParse(text, out int parsedInt) && _fieldInfo.GetValue(DebugSettingsController.instance.settings) is int)
        {
            newValue = parsedInt;
        }
        else if (float.TryParse(text, out float parsedFloat) && _fieldInfo.GetValue(DebugSettingsController.instance.settings) is float)
        {
            newValue = parsedFloat;
        }
        else if (_fieldInfo.GetValue(DebugSettingsController.instance.settings) is string)
        {
            newValue = text;
        }
        else
        {
            Debug.LogError("Invalid type for setting " + _fieldInfo.Name + ". Expected type : "  + _fieldInfo.GetValue(DebugSettingsController.instance.settings).GetType().Name);
        }

        UpdateValue(newValue);
    }

    private void HandleClicked()
    {
        _clickedCallback(this);
    }
}

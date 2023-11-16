using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Reflection;

public class DebugSettingsController
{
    private DebugSettings _settings;
    private static DebugSettingsController _instance;
    private const string SAVED_SETTINGS_FILE_NAME = "DebugSettings.json";
    private System.Collections.Generic.Dictionary<string, IList> _settingsValues;

    public DebugSettings settings
    {
        get => _settings;
    }

    public static DebugSettingsController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DebugSettingsController();
            }

            return _instance;
        }
    }

    public DebugSettingsController()
    {
        Init();
    }

    public void Save()
    {
        string settingsPath = Path.Combine(Application.persistentDataPath, SAVED_SETTINGS_FILE_NAME);
        string settingsString = JsonUtility.ToJson(_settings);

        File.WriteAllText(settingsPath, settingsString);
    }

    public bool HasNextValue(FieldInfo fieldInfo)
    {
        return _settingsValues.ContainsKey(fieldInfo.Name) || (fieldInfo.GetValue(settings) is bool);
    }

    // This method returns the 'next' value for settings which can cycle through multiple values when tapped.
    public object GetNextValue(FieldInfo fieldInfo)
    {
        object currentValue = fieldInfo.GetValue(settings);
        object updatedValue = null;

        // bool values can just get toggled between 'true' and 'false'
        if (currentValue is bool)
        {
            if ((bool)currentValue)
            {
                updatedValue = false;
            }
            else
            {
                updatedValue = true;
            }
        }
        else if (_settingsValues.ContainsKey(fieldInfo.Name))
        {
            // If a MaintenanceAppSetting has a corresponding list of permitted values in MaintenanceAppSettingsValues cycle through them.
            IList settingsValues = _settingsValues[fieldInfo.Name];
            int currentValueIndex = settingsValues.IndexOf(currentValue);

            if (currentValueIndex < settingsValues.Count - 1)
            {
                currentValueIndex++;
            }
            else
            {
                currentValueIndex = 0;
            }

            updatedValue = settingsValues[currentValueIndex];
        }

        return updatedValue;
    }

    private void Init()
    {
        LoadSavedSettings();

        _settingsValues = new System.Collections.Generic.Dictionary<string, IList>();
        // cache all the settings which have predefined values.
        DebugSettingsValues settingsValues = new DebugSettingsValues();
        FieldInfo[] settingsValuesFields = typeof(DebugSettingsValues).GetFields();
        foreach (FieldInfo settingsValueField in settingsValuesFields)
        {
            _settingsValues[settingsValueField.Name] = settingsValueField.GetValue(settingsValues) as IList;

            ValidateCurrentSettingValue(settingsValueField);
        }
    }

    private void ValidateCurrentSettingValue(FieldInfo settingsValueField)
    {
        // If the value saved for a particular setting is no longer valid, replace it with the first allowed value for that setting.
        //   This will fix any settings saved to json that are no longer allowed.
        FieldInfo currentSettingFieldInfo = typeof(DebugSettings).GetField(settingsValueField.Name);
        var currentValue = currentSettingFieldInfo.GetValue(_settings);
        if (_settingsValues[settingsValueField.Name].IndexOf(currentValue) == -1)
        {
            var defaultValue = _settingsValues[settingsValueField.Name][0];
            currentSettingFieldInfo.SetValue(_settings, defaultValue);
            Debug.LogWarning("SettingsManager :: Invalid value '" + currentValue + "' for setting '" + settingsValueField.Name + "'. Updating to default value '" + defaultValue + "'.");
        }
    }

    private void LoadSavedSettings()
    {
        string settingsPath = Path.Combine(Application.persistentDataPath, SAVED_SETTINGS_FILE_NAME);

        if (File.Exists(settingsPath))
        {
            string settingsString = File.ReadAllText(settingsPath);
            _settings = JsonUtility.FromJson<DebugSettings>(settingsString);
        }
        else
        {
            _settings = new DebugSettings();
        }
    }
}

[Serializable]
public class DebugSettings
{
    //public string DefaultAtomType = AtomType.LOCATABLE_NOTE;
}

// This class optionally defines a list of values that are allowed for a DebugSettings field. The field names in DebugSettings and DebugSettingsValues
//   must match for the validation and lookup to work.
public class DebugSettingsValues
{
    //public System.Collections.Generic.List<string> DefaultAtomType = new System.Collections.Generic.List<string>(){ AtomType.LOCATABLE_NOTE, AtomType.NONE };
}
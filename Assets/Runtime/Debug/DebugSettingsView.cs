using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DebugSettingsView : MonoBehaviour
{
    public Action cardClosedCallback;
    [SerializeField] private Transform _itemContainer;
    private string PREFAB_PATH = "SettingsItem";
    private Dictionary<string, DebugSettingsItemView> _settingsItemsByFieldName = new Dictionary<string, DebugSettingsItemView>();

    private void OnEnable()
    {
        UpdateSettings();
    }

    private void UpdateSettings()
    {
        DebugSettings currentSettings = DebugSettingsController.instance.settings;
        FieldInfo[] settingsFields = typeof(DebugSettings).GetFields();
        foreach (FieldInfo settingsField in settingsFields)
        {
            var currentValue = settingsField.GetValue(currentSettings);

            DebugSettingsItemView settingsItem;

            // If a settingItem already exists just uses the cached version and update it with any changes.
            if (_settingsItemsByFieldName.ContainsKey(settingsField.Name))
            {
                settingsItem = _settingsItemsByFieldName[settingsField.Name];
            }
            else
            {
                settingsItem = GameObject.Instantiate(Resources.Load<DebugSettingsItemView>(PREFAB_PATH), _itemContainer);
                _settingsItemsByFieldName[settingsField.Name] = settingsItem;
            }

            settingsItem.Set(settingsField, currentValue, HandleSettingsItemClicked);
            settingsItem.EnableTextInput(!DebugSettingsController.instance.HasNextValue(settingsField));
        }
    }

    private void HandleSettingsItemClicked(DebugSettingsItemView settingsItem)
    {
        if (DebugSettingsController.instance.HasNextValue(settingsItem.FieldInfo))
        {
            object updatedValue = DebugSettingsController.instance.GetNextValue(settingsItem.FieldInfo);

            if (updatedValue != null)
            {
                settingsItem.UpdateValue(updatedValue);
            }
        }
        else
        {
            settingsItem.SetTextInput();
        }
    }

    // Save settings to device when the menu is closed.
    private void OnDisable()
    {
        DebugSettingsController.instance.Save();
    }

    public void CloseButtonClicked()
    {
        gameObject.SetActive(false);
        cardClosedCallback?.Invoke();
    }
}
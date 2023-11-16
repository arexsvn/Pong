using UnityEngine;
using System;
using System.IO;
using LitJson;
using signals;

public class SaveStateController
{
    public SaveState CurrentSave { get; private set; }
    public AppConfig AppConfig { get; private set; }

    public Signal newConfigCreated = new Signal();
    public Signal newSaveCreated = new Signal();
    public Signal<string, string> versionUpgraded = new Signal<string, string>();

    private string _appConfigFileName = "config.txt";
    private string _saveSuffix = "_save.txt";
    private string _saveFolderPath = "saveStates/";

    public SaveStateController()
    {

    }

    public void init()
    {
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, _saveFolderPath);
        if (!Directory.Exists(saveDirectoryPath))
        {
            Directory.CreateDirectory(saveDirectoryPath);
        }

        loadCurrentSave();
    }

    public void createNewSave()
    {
        CurrentSave = new SaveState();
        CurrentSave.id = getTotalSaves().ToString();
        CurrentSave.version = Application.version;
        CurrentSave.creationTime = TimeUtils.currentTime;

        System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
        string countryCode = cultureInfo.Name.Substring(0, 2);
        if (countryCode == "ru")
        {
            CurrentSave.language = "ru";
        }
        else
        {
            CurrentSave.language = "en";
        }

        AppConfig.currentSaveId = CurrentSave.id;
        AppConfig.lastUpdateTime = TimeUtils.currentTime;

        save();

        newSaveCreated.Dispatch();
    }

    private bool loadCurrentSave()
    {
        string configPath = Path.Combine(Application.persistentDataPath, _appConfigFileName);
        bool newGame = true;

        if (File.Exists(configPath))
        {
            using (StreamReader streamReader = File.OpenText(configPath))
            {
                string jsonString = streamReader.ReadToEnd();
                AppConfig = JsonMapper.ToObject<AppConfig>(jsonString);
            }
            load(AppConfig.currentSaveId);
            newGame = !isSaveGameValid(CurrentSave);
        }

        if (newGame)
        {
            createNewGameConfig();
            newConfigCreated.Dispatch();
        }

        if (CurrentSave == null)
        {
            createNewSave();
        }
        else if (CurrentSave.version != Application.version)
        {
            //  TODO : Add 'upgrade' support to get older versions up to snuff.
            versionUpgraded.Dispatch(CurrentSave.version, Application.version);
            CurrentSave.version = Application.version;

            // Update sfx volume to new base setting if it is unchanged from last version.
            if (Mathf.Approximately(CurrentSave.sfxVolume, .2f))
            {
                SaveState saveState = new SaveState();
                CurrentSave.sfxVolume = saveState.sfxVolume;
            }

            save();
        }

        return newGame;
    }

    private void createNewGameConfig()
    {
        AppConfig = new AppConfig();
        AppConfig.version = Application.version;
        AppConfig.creationTime = TimeUtils.currentTime;
    }

    private bool isSaveGameValid(SaveState saveGame)
    {
        return saveGame != null;
    }

    private void saveConfig()
    {
        AppConfig.lastUpdateTime = TimeUtils.currentTime;

        string configPath = Path.Combine(Application.persistentDataPath, _appConfigFileName);
        string jsonString = JsonMapper.ToJson(AppConfig);
        using (StreamWriter streamWriter = File.CreateText(configPath))
        {
            streamWriter.Write(jsonString);
        }
    }

    public void save()
    {
        CurrentSave.lastSaveTime = TimeUtils.currentTime;

        string saveFileName = Path.Combine(_saveFolderPath, CurrentSave.id + _saveSuffix);
        string currentSavePath = Path.Combine(Application.persistentDataPath, saveFileName);
        string jsonString = JsonMapper.ToJson(CurrentSave);
        using (StreamWriter streamWriter = File.CreateText(currentSavePath))
        {
            streamWriter.Write(jsonString);
        }

        saveConfig();
    }

    public int getTotalSaves()
    {
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, _saveFolderPath);
        string[] filePaths = Directory.GetFiles(saveDirectoryPath);

        return filePaths.Length;
    }

    public SaveState load(string id)
    {
        string saveFileName = Path.Combine(_saveFolderPath, id + _saveSuffix);
        string currentSavePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (!File.Exists(currentSavePath))
        {
            return null;
        }

        using (StreamReader streamReader = File.OpenText(currentSavePath))
        {
            string jsonString = streamReader.ReadToEnd();
            CurrentSave = JsonMapper.ToObject<SaveState>(jsonString);
            return CurrentSave;
        }
    }

    public void delete(string id)
    {
        if (CurrentSave.id == id)
        {
            createNewSave();
        }

        string saveFileName = Path.Combine(_saveFolderPath, id + _saveSuffix);
        string savePathToDelete = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(savePathToDelete))
        {
            File.Delete(savePathToDelete);
        }
    }

    public long getTotalTimePlaying()
    {
        return TimeUtils.currentTime - AppConfig.creationTime;
    }
}

[Serializable]
public class SaveState
{
    public string id;
    public long lastSaveTime = 0;
    public long creationTime = 0;
    public string version;
    public string language = "en";
    public float musicVolume = .4f;
    public float sfxVolume = .6f;
    public string currentTheme;
}

[Serializable]
public class AppConfig
{
    public long lastUpdateTime;
    public long creationTime = 0;
    public string currentSaveId;
    public string version;
}

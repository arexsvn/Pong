using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController
{
    readonly AudioSources _audioSources;
    readonly SaveStateController _saveStateController;
    private System.Random _rand;

    public AudioController(AudioSources audioSources, SaveStateController saveStateController)
    {
        _audioSources = audioSources;
        _saveStateController = saveStateController;
    }

    public void init()
    {
        _audioSources.init();
        _rand = new System.Random();
    }

    public void play(string fileName, string type, float volumeScale = 1f)
    {
        float volumeModifier = 1f;

        if (type == AudioType.Music)
        {
            volumeModifier = _saveStateController.CurrentSave.musicVolume;
        }
        else if (type == AudioType.Sfx)
        {
            volumeModifier = _saveStateController.CurrentSave.sfxVolume;
        }

        _audioSources.play(fileName, type, volumeScale * volumeModifier);
    }

    public void fade(string type, float targetVolume)
    {
        float duration = 2.0f;
        _audioSources.fade(type, duration, targetVolume);
    }

    public void setVolume(string type, float volume)
    {
        _audioSources.setVolume(type, volume);
    }
    public void playSelected(int total)
    {
        if (total < 1)
        {
            return;
        }

        total = Mathf.Min(total, 15);
        string fileName = "tile_selected/selected_" + total;
        // TODO - volume level should be corrected in audio file.
        float volumeScale = 0.4f;

        play(fileName, AudioType.Sfx, volumeScale);
    }

    public void playLand()
    {
        int index = _rand.Next(0, 8);
        string fileName = "tile_land/land_" + index;
        // TODO - volume level should be corrected in audio file.
        float volumeScale = 0.8f;

        play(fileName, AudioType.Sfx, volumeScale);
    }
}

public class AudioType
{
    public const string Music = "Music";
    public const string Sfx = "Sfx";
}

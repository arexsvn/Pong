using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG;

public class AudioSources : MonoBehaviour
{
    public AudioMixer mixer;
    private Dictionary<string, Coroutine> _coroutines;
    private Dictionary<string, AudioSource> _sources;
    private Dictionary<string, AudioClip> _cache;
    private const string MUSIC_PATH = "Audio/music/";
    private const string SFX_PATH = "Audio/sfx/";
    private const string SOURCE_MUSIC = "music";
    private const string SOURCE_MUSIC2 = "music2";
    private const string SOURCE_SFX = "sfx";

    public void init()
    {
        _cache = new Dictionary<string, AudioClip>();
        _sources = new Dictionary<string, AudioSource>();
        _coroutines = new Dictionary<string, Coroutine>();

        addSource(SOURCE_MUSIC, true);
        addSource(SOURCE_MUSIC2, true);
        addSource(SOURCE_SFX, false);
    }

    public void play(string fileName, string type, float volumeScale = 1f)
    {
        AudioClip clip;
        string path;

        if (type == AudioType.Music)
        {
            path = System.IO.Path.Combine(MUSIC_PATH, fileName);
        }
        else
        {
            path = System.IO.Path.Combine(SFX_PATH, fileName);
        }

        if (_cache.ContainsKey(fileName))
        {
            clip = _cache[path];
        }
        else
        {
            clip = Resources.Load<AudioClip>(path);
            _cache[path] = clip;
        }

        if (type == AudioType.Music)
        {
            AudioSource music = _sources[SOURCE_MUSIC];
            AudioSource music2 = _sources[SOURCE_MUSIC2];

            if (!music.isPlaying && !music2.isPlaying)
            {
                music.volume = 0f;
                music.clip = clip;
                music.Play();

                // Don't need to fade the sound if the target volume level is zero.
                if (volumeScale != 0f)
                {
                    fadeSource(SOURCE_MUSIC, 2f, volumeScale);
                }
            }
            else
            {
                AudioSource newSource = music;
                AudioSource oldSource = music2;

                // If there is music currently playing on an AudioSource, fade it out as the new music fades in.
                if (newSource.isPlaying)
                {
                    oldSource = newSource;
                    newSource = music2;
                }

                newSource.volume = 0f;
                newSource.clip = clip;
                newSource.Play();

                // Don't need to fade the sound if the target volume level is zero.
                if (volumeScale != 0f)
                {
                    fadeSource(newSource.name, 4f, volumeScale);
                }
                
                fadeSource(oldSource.name, 2f, 0f);
            }
        }
        else
        {
            AudioSource sfx = _sources[SOURCE_SFX];
            sfx.PlayOneShot(clip, volumeScale);
        }
    }

    public void fade(string type, float duration, float targetVolume)
    {
        if (type == AudioType.Music)
        {
            if (_sources[SOURCE_MUSIC].isPlaying)
            {
                fadeSource(SOURCE_MUSIC, duration, targetVolume);
            }

            if (_sources[SOURCE_MUSIC2].isPlaying)
            {
                fadeSource(SOURCE_MUSIC2, duration, targetVolume);
            }
        }
        else
        {
            fadeSource(SOURCE_SFX, duration, targetVolume);
        }
    }

    public void setVolume(string type, float volume)
    {
        if (type == AudioType.Music)
        {
            _sources[SOURCE_MUSIC].volume = volume;
            _sources[SOURCE_MUSIC2].volume = volume;
        }
        else
        {
            _sources[SOURCE_SFX].volume = volume;
        }
    }

    private void fadeSource(string audioSourceName, float duration, float targetVolume)
    {
        if (_coroutines.ContainsKey(audioSourceName) && _coroutines[audioSourceName] != null)
        {
            StopCoroutine(_coroutines[audioSourceName]);
            _coroutines[audioSourceName] = null;
        }

        AudioSource audioSource = _sources[audioSourceName];
        _coroutines[audioSourceName] = StartCoroutine(fadeAsync(audioSource, duration, targetVolume));
    }

    private AudioSource addSource(string name, bool loop)
    {
        GameObject prefab = new GameObject(name);
        prefab.transform.parent = gameObject.transform;

        AudioSource audioSource = prefab.AddComponent<AudioSource>();
        audioSource.loop = loop;

        _sources[name] = audioSource;

        return audioSource;
    }

    private IEnumerator fadeAsync(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }

        if (targetVolume == 0f)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        _coroutines[audioSource.name] = null;

        yield break;
    }

    private IEnumerator fade(float duration, float targetVolume)
    {
        string exposedParam = "MasterVolume";
        float currentTime = 0;
        float currentVol;
        mixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            mixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }
}

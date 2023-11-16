using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemView : MonoBehaviour
{
    public ParticleSystem system;
    public UnityEngine.UI.Extensions.UIParticleSystem uiParticleSystem;

    public void stop()
    {
        system.Stop();
        uiParticleSystem.StopParticleEmission();
        //gameObject.SetActive(false);
        playing = false;
    }

    public void play()
    {
        //gameObject.SetActive(true);
        system.time = 0f;
        uiParticleSystem.StartParticleEmission();
        system.Play();
        playing = true;
    }

    public void setPosition(Vector2 position)
    {
        system.transform.position = position;
    }

    public void setColor(Color color)
    {
        //system.startColor = color;
        ParticleSystem.MainModule settings = system.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color);
    }

    public bool playing { get; private set; } = false;
}

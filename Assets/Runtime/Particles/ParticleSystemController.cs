using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController
{
    private static string MAIN_PREFAB = "Particles/ParticleSystemContainer";

    private ParticleSystemContainer _container;
    public ParticleController()
    {
        
    }

    public void init()
    {
        _container = Object.Instantiate(Resources.Load<ParticleSystemContainer>(MAIN_PREFAB));
        _container.init();
    }

    public void add(string name, Vector2 position)
    {
        _container.add(name, position);
    }

    public void add(string name, Vector2 position, Color color)
    {
        ParticleSystemView view = _container.add(name, position);
        view.setColor(color);
    }
}

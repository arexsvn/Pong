using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemContainer : MonoBehaviour
{
    public Transform root;
    private static string SYSTEM_PREFABS = "Particles/ParticleSystems/";
    private Dictionary<string, ParticleSystemView> _prefabs;
    private Dictionary<string, List<ParticleSystemView>> _pool;

    public void init()
    {
        _prefabs = new Dictionary<string, ParticleSystemView>();
        _pool = new Dictionary<string, List<ParticleSystemView>>();
    }

    public ParticleSystemView add(string fileName, Vector2 position)
    {
        if (!_prefabs.ContainsKey(fileName))
        {
            string path = System.IO.Path.Combine(SYSTEM_PREFABS, fileName);
            _prefabs[fileName] = Resources.Load<ParticleSystemView>(path);
            _pool[fileName] = new List<ParticleSystemView>();
        }

        foreach(ParticleSystemView next in _pool[fileName])
        {
            if (!next.playing)
            {
                //next.system.transform.position = position;
                next.setPosition(position);
                next.play();
                //next.system.time = 0f;
                //next.system.enableEmission = true;
                //next.system.Play();
                //next.system.enableEmission = true;
                //next.uiParticleSystem.StartParticleEmission();
                StartCoroutine(resetView(next, next.system.main.duration));
                //next.system.Simulate(0f, true, true); 
                return next;
            }
        }

        ParticleSystemView view = Object.Instantiate(_prefabs[fileName], root);
        StartCoroutine(resetView(view, view.system.main.duration));
        view.setPosition(position);
        view.play();
        _pool[fileName].Add(view);
        return view;
    }

    IEnumerator resetView(ParticleSystemView view, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        view.stop();
        /*
        view.system.Stop();
        view.system.Clear();
        view.uiParticleSystem.StopParticleEmission();
        */

        /*
        view.system.Stop();
        view.system.time = 0;
        */
        //view.system.enableEmission = false;
    }
}

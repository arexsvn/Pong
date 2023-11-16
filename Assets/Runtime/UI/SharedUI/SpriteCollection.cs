using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteCollection", menuName = "ScriptableObjects/SpriteCollection", order = 1)]
public class SpriteCollection : ScriptableObject
{
    [SerializeField] private GenericDictionary<string, Sprite> _sprites;

    public Sprite get(string id)
    {
        if (_sprites.ContainsKey(id))
        {
            return _sprites[id];
        }

        Debug.LogError("SpriteCollection :: Sprite '" + id + "' not found.");

        return null;
    }

    public bool contains(string id)
    {
        return _sprites.ContainsKey(id);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectCollection<T> : Singleton<GameObjectCollection<T>> where T : MonoBehaviour
{
    public bool logMissing = true;
    public List<T> prefabs;

    public T GetPrefab(string name)
    {
        foreach(var prefab in prefabs)
        {
            if(prefab.name == name)
            {
                return prefab;
            }
        }
        if(logMissing)
        {
            Debug.LogWarningFormat("Missing {0} prefab {1}", typeof(T).Name, name);
        }
        return null;
    }
}

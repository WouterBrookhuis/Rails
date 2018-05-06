using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {

    public static PoolManager Instance { get; private set; }

    private Dictionary<string, SimplePool> pools = new Dictionary<string, SimplePool>();

    private void Awake()
    {
        Instance = this;
        var poolComponents = GetComponentsInChildren<SimplePool>();
        foreach(var pool in poolComponents)
        {
            pools.Add(pool.prefab.name, pool);
        }
    }

    public SimplePool GetPool(string prefabName)
    {
        SimplePool pool;
        pools.TryGetValue(prefabName, out pool);
        return pool;
    }
}

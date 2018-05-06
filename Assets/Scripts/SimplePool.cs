using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour {

    public GameObject prefab;

    private Stack<GameObject> freeInstances = new Stack<GameObject>();

    public GameObject GetInstance()
    {
        GameObject go;
        if(freeInstances.Count > 0)
        {
            go = freeInstances.Pop();
        }
        else
        {
            go = Instantiate(prefab);
        }
        go.SetActive(true);
        return go;
    }

    public void ReleaseInstance(GameObject instance)
    {
        freeInstances.Push(instance);
        instance.SetActive(false);
    }
}

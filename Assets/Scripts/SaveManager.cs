using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public bool autoSave = true;
    public string autoSaveName = "autosave.rail";

    public AbstractSerializer[] serializers = new AbstractSerializer[0];

    private void Start()
    {
        if(autoSave)
        {
            StartCoroutine(AutoLoad());
        }
    }

    private void OnApplicationQuit()
    {
        if(!autoSave)
        {
            return;
        }

        try
        {
            using(var stream = File.Open(Application.dataPath + "/" + autoSaveName, FileMode.OpenOrCreate))
            {
                using(var wr = new BinaryWriter(stream))
                {
                    for(int i = 0; i < serializers.Length; i++)
                    {
                        serializers[i].SaveWorld(wr);
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

    IEnumerator AutoLoad()
    {
        yield return new WaitForSeconds(1.0f);

        try
        {
            if(File.Exists(Application.dataPath + "/" + autoSaveName))
            {
                using(var stream = File.Open(Application.dataPath + "/" + autoSaveName, FileMode.Open))
                {
                    using(var rd = new BinaryReader(stream))
                    {
                        for(int i = 0; i < serializers.Length; i++)
                        {
                            serializers[i].LoadWorld(rd);
                        }
                    }
                }
            }

        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }
}

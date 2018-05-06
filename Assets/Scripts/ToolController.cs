using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : Singleton<ToolController> {

    private DefaultTool defaultTool;
    private Tool activeTool;

    public Tool ActiveTool {
        get
        {
            return activeTool;
        }
        set
        {
            activeTool.enabled = false;
            if(value == null)
            {
                value = defaultTool;
            }
            activeTool = value;
            activeTool.enabled = true;
        }
    }

	// Use this for initialization
	void Awake () {
        var tools = GetComponents<Tool>();
        foreach(var tool in tools)
        {
            tool.enabled = false;
        }
        defaultTool = gameObject.GetComponent<DefaultTool>();
        activeTool = defaultTool;
        defaultTool.enabled = true;
	}
	
	public void SetTool(Type type)
    {
        var tool = GetComponent(type) as Tool;
        if(tool == null)
        {
            Debug.LogErrorFormat("Unable to find tool of type {0}", type.Name);
            return;
        }
        ActiveTool = tool;
    }
}

using UnityEngine;
using System.Collections.Generic;

public class GlowObject : MonoBehaviour
{
	public Color glowColor;
	public float lerpFactor = 10;

    private Renderer[] renderers;
	private List<Material> materials = new List<Material>();
	private Color currentColor;
	private Color targetColor;

	void Start()
	{
        renderers = GetComponentsInChildren<Renderer>();

		foreach (var renderer in renderers)
		{
            materials.AddRange(renderer.materials);
		}
	}

	public void ActivateGlow()
	{
        targetColor = glowColor;
		enabled = true;
	}

    public void DeactivateGlow ()
	{
        targetColor = Color.black;
		enabled = true;
	}

	private void Update()
	{
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * lerpFactor);

		for (int i = 0; i < materials.Count; i++)
		{
            materials[i].SetColor("_GlowColor", currentColor);
		}

		if (currentColor.Equals(targetColor))
		{
			enabled = false;
		}
	}
}

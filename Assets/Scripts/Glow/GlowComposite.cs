using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlowComposite : MonoBehaviour
{
	[Range (0, 10)]
	public float intensity = 2;

	private Material compositeMaterial;

	void OnEnable()
	{
		compositeMaterial = new Material(Shader.Find("Hidden/GlowComposite"));
    }

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		compositeMaterial.SetFloat("_Intensity", intensity);
        Graphics.Blit(src, dst, compositeMaterial, 0);
	}
}

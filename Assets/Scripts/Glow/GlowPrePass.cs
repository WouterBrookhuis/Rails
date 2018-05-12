using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlowPrePass : MonoBehaviour
{
	private RenderTexture prePass;
	private RenderTexture blurred;
	private Material blurMaterial;

	void OnEnable()
	{
        prePass = new RenderTexture(Screen.width, Screen.height, 24);
        prePass.antiAliasing = QualitySettings.antiAliasing;
        blurred = new RenderTexture(Screen.width >> 1, Screen.height >> 1, 0);

		var camera = GetComponent<Camera>();
		var glowShader = Shader.Find("Hidden/GlowReplace");
		camera.targetTexture = prePass;
		camera.SetReplacementShader(glowShader, "Glowable");
		Shader.SetGlobalTexture("_GlowPrePassTex", prePass);

		Shader.SetGlobalTexture("_GlowBlurredTex", blurred);

		blurMaterial = new Material(Shader.Find("Hidden/Blur"));
		blurMaterial.SetVector("_BlurSize", new Vector2(blurred.texelSize.x * 1.5f, blurred.texelSize.y * 1.5f));
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit(src, dst);

		Graphics.SetRenderTarget(blurred);
		GL.Clear(false, true, Color.clear);

		Graphics.Blit(src, blurred);
		
		for (int i = 0; i < 4; i++)
		{
			var temp = RenderTexture.GetTemporary(blurred.width, blurred.height);
			Graphics.Blit(blurred, temp, blurMaterial, 0);
			Graphics.Blit(temp, blurred, blurMaterial, 1);
			RenderTexture.ReleaseTemporary(temp);
		}
	}
}

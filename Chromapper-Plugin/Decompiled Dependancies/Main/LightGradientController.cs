using Beatmap.Shared;
using UnityEngine;

public class LightGradientController : MonoBehaviour
{
	private static readonly int colorA = Shader.PropertyToID("_ColorA");

	private static readonly int colorB = Shader.PropertyToID("_ColorB");

	private static readonly int easingId = Shader.PropertyToID("_EasingID");

	[SerializeField]
	private MeshRenderer meshRenderer;

	private MaterialPropertyBlock materialPropertyBlock;

	public void UpdateGradientData(ChromaLightGradient gradient)
	{
		if (materialPropertyBlock == null)
		{
			materialPropertyBlock = new MaterialPropertyBlock();
		}
		materialPropertyBlock.SetColor(colorA, gradient.StartColor);
		materialPropertyBlock.SetColor(colorB, gradient.EndColor);
		materialPropertyBlock.SetInt(easingId, Easing.EasingShaderId(gradient.EasingType));
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
	}

	public void UpdateDuration(float duration)
	{
		base.transform.localScale = new Vector3(duration * EditorScaleController.EditorScale * 1.3333334f, 1f, 1f);
	}

	public void SetVisible(bool visible)
	{
		meshRenderer.enabled = visible;
	}
}

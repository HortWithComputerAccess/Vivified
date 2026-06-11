using UnityEngine;

public class SpectrogramShellTextureController : MonoBehaviour
{
	private static readonly int valueCutoff = Shader.PropertyToID("_ValueCutoff");

	[SerializeField]
	private Renderer _spectrogram;

	private void Start()
	{
		Transform parent = _spectrogram.transform.parent;
		Vector3 localPosition = _spectrogram.transform.localPosition;
		int spectrogramSlices = Settings.Instance.SpectrogramSlices;
		for (int i = 0; i < spectrogramSlices; i++)
		{
			float num = (float)i / (float)spectrogramSlices;
			GameObject obj = Object.Instantiate(_spectrogram.gameObject, parent);
			obj.transform.localPosition = localPosition + num * Settings.Instance.SpectrogramHeight * Vector3.up;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetFloat(valueCutoff, num);
			obj.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
		}
	}
}

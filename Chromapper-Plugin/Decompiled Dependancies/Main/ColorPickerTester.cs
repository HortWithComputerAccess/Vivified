using UnityEngine;
using UnityEngine.Serialization;

public class ColorPickerTester : MonoBehaviour
{
	[FormerlySerializedAs("renderer")]
	public Renderer Renderer;

	[FormerlySerializedAs("picker")]
	public ColorPicker Picker;

	public Color Color = Color.red;

	private void Start()
	{
		Picker.ONValueChanged.AddListener(delegate(Color color)
		{
			Renderer.material.color = color;
			Color = color;
		});
		Renderer.material.color = Picker.CurrentColor;
		Picker.CurrentColor = Color;
	}
}

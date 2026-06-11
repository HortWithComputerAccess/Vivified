using TMPro;
using UnityEngine;

public class FloatValueController : DisableActionsField
{
	[PickerChoice("Mapper", "bar.events.floatValue")]
	[SerializeField]
	private TMP_InputField floatValue;

	[SerializeField]
	private EventPlacement eventPlacement;

	public void UpdateManualFloatValue(string result)
	{
		if (int.TryParse(result, out var result2))
		{
			eventPlacement.UpdateFloatValue((float)result2 / 100f);
		}
	}
}

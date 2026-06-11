using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BasicStrobePassUI : StrobeGeneratorPassUIController
{
	[FormerlySerializedAs("EventTypes")]
	[SerializeField]
	private StrobeGeneratorEventSelector[] eventTypes;

	[SerializeField]
	private Toggle dynamicallyChangeTypeA;

	[SerializeField]
	private Toggle swapColors;

	[FormerlySerializedAs("Values")]
	[SerializeField]
	private StrobeGeneratorEventSelector values;

	[SerializeField]
	private TMP_InputField strobeInterval;

	[SerializeField]
	private TMP_Dropdown regularEventEasings;

	[SerializeField]
	private Toggle easingTime;

	[SerializeField]
	private Toggle easingFloatValue;

	private readonly string[] filteredEasings = new string[3] { "Back", "Elastic", "Bounce" };

	private new void Start()
	{
		base.Start();
		regularEventEasings.ClearOptions();
		regularEventEasings.AddOptions(Easing.DisplayNameToInternalName.Keys.Where((string x) => !filteredEasings.Any((string y) => x.Contains(y))).ToList());
		regularEventEasings.value = 0;
	}

	public override StrobeGeneratorPass GetPassForGeneration()
	{
		List<int> list = new List<int>();
		StrobeGeneratorEventSelector[] array = eventTypes;
		foreach (StrobeGeneratorEventSelector strobeGeneratorEventSelector in array)
		{
			list.Add(GetTypeFromEventIds(strobeGeneratorEventSelector.SelectedNum, values.SelectedNum));
		}
		float strobePrecision = float.Parse(strobeInterval.text);
		string strobeEasing = Easing.DisplayNameToInternalName[regularEventEasings.captionText.text];
		return new StrobeLightingPass(list, swapColors.isOn, dynamicallyChangeTypeA.isOn, strobePrecision, strobeEasing, easingTime.isOn, easingFloatValue.isOn);
	}

	private static int GetTypeFromEventIds(int eventValue, int eventColor)
	{
		switch (eventValue)
		{
		case 1:
			switch (eventColor)
			{
			case 0:
				return 5;
			case 1:
				return 1;
			case 2:
				return 9;
			}
			break;
		case 2:
			switch (eventColor)
			{
			case 0:
				return 6;
			case 1:
				return 2;
			case 2:
				return 10;
			}
			break;
		case 3:
			switch (eventColor)
			{
			case 0:
				return 7;
			case 1:
				return 3;
			case 2:
				return 11;
			}
			break;
		case 4:
			switch (eventColor)
			{
			case 0:
				return 8;
			case 1:
				return 4;
			case 2:
				return 12;
			}
			break;
		case 0:
			return 0;
		}
		return -1;
	}
}

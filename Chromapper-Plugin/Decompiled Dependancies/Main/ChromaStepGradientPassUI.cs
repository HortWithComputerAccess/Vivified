using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChromaStepGradientPassUI : StrobeGeneratorPassUIController
{
	private static readonly System.Random rand = new System.Random();

	private static bool flicker;

	[FormerlySerializedAs("EventType")]
	[SerializeField]
	private StrobeGeneratorEventSelector eventType;

	[FormerlySerializedAs("Values")]
	[SerializeField]
	private StrobeGeneratorEventSelector values;

	[SerializeField]
	private Toggle swapColors;

	[SerializeField]
	private TMP_InputField strobeInterval;

	[SerializeField]
	private TMP_Dropdown chromaEventEasings;

	private readonly Dictionary<string, Func<float, float>> extraEasings = new Dictionary<string, Func<float, float>>
	{
		{
			"Random",
			(float f) => (float)rand.NextDouble()
		},
		{
			"Flicker",
			delegate(float f)
			{
				flicker = f != 0f && !flicker;
				return flicker ? 1 : 0;
			}
		}
	};

	private new void Start()
	{
		base.Start();
		chromaEventEasings.ClearOptions();
		chromaEventEasings.AddOptions(Easing.DisplayNameToInternalName.Keys.ToList());
		chromaEventEasings.AddOptions(extraEasings.Keys.ToList());
		chromaEventEasings.value = 0;
	}

	public override StrobeGeneratorPass GetPassForGeneration()
	{
		string text = chromaEventEasings.captionText.text;
		Func<float, float> easing = (extraEasings.ContainsKey(text) ? extraEasings[text] : Easing.Named(Easing.DisplayNameToInternalName[text]));
		return new StrobeStepGradientPass(GetTypeFromEventIds(eventType.SelectedNum, values.SelectedNum), swapColors.isOn, float.Parse(strobeInterval.text), easing);
	}

	private int GetTypeFromEventIds(int eventValue, int eventColor)
	{
		return eventValue switch
		{
			0 => 0, 
			1 => (eventColor != 0) ? 1 : 5, 
			2 => (eventColor == 0) ? 6 : 2, 
			3 => (eventColor == 0) ? 7 : 3, 
			4 => (eventColor == 0) ? 8 : 4, 
			_ => -1, 
		};
	}
}

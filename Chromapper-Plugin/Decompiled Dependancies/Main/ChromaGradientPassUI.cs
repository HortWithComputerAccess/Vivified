using System.Linq;
using TMPro;
using UnityEngine;

public class ChromaGradientPassUI : StrobeGeneratorPassUIController
{
	[SerializeField]
	private TMP_Dropdown chromaEventEasings;

	[SerializeField]
	private TMP_Dropdown chromaLerpTypes;

	private new void Start()
	{
		base.Start();
		chromaEventEasings.ClearOptions();
		chromaEventEasings.AddOptions(Easing.DisplayNameToInternalName.Keys.ToList());
		chromaEventEasings.value = 0;
		chromaLerpTypes.value = 0;
	}

	public override StrobeGeneratorPass GetPassForGeneration()
	{
		return new StrobeTransitionPass(Easing.DisplayNameToInternalName[chromaEventEasings.captionText.text], chromaLerpTypes.captionText.text);
	}
}

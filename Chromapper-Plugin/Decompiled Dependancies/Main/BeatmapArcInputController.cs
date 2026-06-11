using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Helper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BeatmapArcInputController : BeatmapInputController<ArcContainer>, CMInput.IArcObjectsActions
{
	public const float MuChangeSpeed = 0.1f;

	[FormerlySerializedAs("arcAppearanceSO")]
	[SerializeField]
	private ArcAppearanceSO arcAppearanceSo;

	public void OnChangingMu(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			RaycastFirstObject(out var firstObject);
			if (!(firstObject == null) && !firstObject.Dragging && context.performed)
			{
				float num = context.ReadValue<float>();
				num = (((num > 0f) ^ Settings.Instance.InvertScrollArcMultiplier) ? 0.1f : (-0.1f));
				ChangeMu(firstObject, num);
			}
		}
	}

	public void ChangeMu(ArcContainer s, float modifier)
	{
		BaseArc originalData = BeatmapFactory.Clone(s.ArcData);
		s.ChangeHeadMultiplier(modifier);
		s.NotifySplineChanged();
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(s.ObjectData, s.ObjectData, originalData, "No comment.", keepSelection: false, ActionMergeType.ArcHeadMultTweak));
	}

	public void OnInvertArcColor(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) && KeybindsController.IsMouseInWindow && context.performed)
		{
			RaycastFirstObject(out var firstObject);
			if (firstObject != null && !firstObject.Dragging)
			{
				InvertArc(firstObject);
			}
		}
	}

	public void InvertArc(ArcContainer arc)
	{
		BaseArc originalData = BeatmapFactory.Clone(arc.ArcData);
		int color = ((arc.ArcData.Color == 0) ? 1 : 0);
		arc.ArcData.Color = color;
		arcAppearanceSo.SetArcAppearance(arc);
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(arc.ObjectData, arc.ObjectData, originalData, "invert arc color"));
	}

	public void OnChangingTmu(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			RaycastFirstObject(out var firstObject);
			if (!(firstObject == null) && !firstObject.Dragging && context.performed)
			{
				float num = context.ReadValue<float>();
				num = (((num > 0f) ^ Settings.Instance.InvertScrollArcMultiplier) ? 0.1f : (-0.1f));
				ChangeTmu(firstObject, num);
			}
		}
	}

	public void ChangeTmu(ArcContainer s, float modifier)
	{
		BaseArc originalData = BeatmapFactory.Clone(s.ArcData);
		s.ChangeTailMultiplier(modifier);
		s.NotifySplineChanged();
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(s.ObjectData, s.ObjectData, originalData, "No comment.", keepSelection: false, ActionMergeType.ArcTailMultTweak));
	}
}

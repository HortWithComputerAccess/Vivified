using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Helper;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BeatmapChainInputController : BeatmapInputController<ChainContainer>, CMInput.IChainObjectsActions
{
	private const int minChainCount = 1;

	private const int maxChainCount = 999;

	private const float minChainSquish = 0.1f;

	private const float maxChainSquish = 999f;

	private const float squishChangeSpeed = 0.1f;

	[FormerlySerializedAs("chainAppearanceSO")]
	[SerializeField]
	private ChainAppearanceSO chainAppearanceSo;

	public void OnTweakChainCount(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			RaycastFirstObject(out var firstObject);
			if (!(firstObject == null) && !firstObject.Dragging && context.performed)
			{
				int modifier = (((context.ReadValue<float>() > 0f) ^ Settings.Instance.InvertScrollChainSegmentCount) ? 1 : (-1));
				TweakValue(firstObject, modifier);
			}
		}
	}

	public void TweakValue(ChainContainer c, int modifier)
	{
		BaseObject baseObject = BeatmapFactory.Clone(c.ObjectData);
		c.ChainData.SliceCount += modifier;
		c.ChainData.SliceCount = Mathf.Clamp(c.ChainData.SliceCount, 1, 999);
		if (c.ChainData.CompareTo(baseObject) != 0)
		{
			c.GenerateChain();
			BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(c.ObjectData, c.ObjectData, baseObject, "No comment.", keepSelection: false, ActionMergeType.ChainSliceCountTweak));
		}
	}

	public void OnInvertChainColor(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true) && KeybindsController.IsMouseInWindow && context.performed)
		{
			RaycastFirstObject(out var firstObject);
			if (firstObject != null && !firstObject.Dragging)
			{
				InvertChain(firstObject);
			}
		}
	}

	public void InvertChain(ChainContainer chain)
	{
		BaseObject originalData = BeatmapFactory.Clone(chain.ObjectData);
		int color = ((chain.ChainData.Color == 0) ? 1 : 0);
		chain.ChainData.Color = color;
		chainAppearanceSo.SetChainAppearance(chain);
		BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(chain.ObjectData, chain.ObjectData, originalData));
	}

	public void OnTweakChainSquish(InputAction.CallbackContext context)
	{
		if (!CustomStandaloneInputModule.IsPointerOverGameObject<GraphicRaycaster>(0, includeDerived: true))
		{
			RaycastFirstObject(out var firstObject);
			if (!(firstObject == null) && !firstObject.Dragging && context.performed)
			{
				float modifier = (((context.ReadValue<float>() > 0f) ^ Settings.Instance.InvertScrollChainSquish) ? 0.1f : (-0.1f));
				TweakChainSquish(firstObject, modifier);
			}
		}
	}

	public void TweakChainSquish(ChainContainer c, float modifier)
	{
		BaseObject baseObject = BeatmapFactory.Clone(c.ObjectData);
		c.ChainData.Squish += modifier;
		c.ChainData.Squish = Mathf.Clamp(c.ChainData.Squish, 0.1f, 999f);
		if (c.ChainData.CompareTo(baseObject) != 0)
		{
			c.GenerateChain();
			BeatmapActionContainer.AddAction(new BeatmapObjectModifiedAction(c.ObjectData, c.ObjectData, baseObject, "No comment.", keepSelection: false, ActionMergeType.ChainSquishTweak));
		}
	}
}

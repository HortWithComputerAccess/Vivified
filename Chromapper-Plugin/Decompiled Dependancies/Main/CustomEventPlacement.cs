using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Containers;
using SimpleJSON;
using UnityEngine;

public class CustomEventPlacement : PlacementController<BaseCustomEvent, CustomEventContainer, CustomEventGridContainer>
{
	private readonly List<TextAsset> customEventDataPresets = new List<TextAsset>();

	public override int PlacementXMax => objectContainerCollection.CustomEventTypes.Count;

	protected override bool CanClickAndDrag { get; set; }

	protected override Vector2 vanillaOffset { get; } = new Vector2(0f, -1.1f);

	internal override void Start()
	{
		base.gameObject.SetActive(Settings.Instance.AdvancedShit);
		TextAsset[] array = Resources.LoadAll<TextAsset>("Custom Event Presets");
		foreach (TextAsset item in array)
		{
			customEventDataPresets.Add(item);
		}
		Debug.Log($"Loaded {customEventDataPresets.Count} presets for custom events.");
		base.Start();
	}

	public override BeatmapAction GenerateAction(BaseObject spawned, IEnumerable<BaseObject> conflicting)
	{
		return new BeatmapObjectPlacementAction(spawned, conflicting, "Placed a Custom Event.");
	}

	public override BaseCustomEvent GenerateOriginalData()
	{
		return new BaseCustomEvent();
	}

	public override void OnPhysicsRaycast(Intersections.IntersectionHit _, Vector3 __)
	{
		Vector3 localPosition = instantiatedContainer.transform.localPosition;
		instantiatedContainer.transform.localPosition = new Vector3(localPosition.x, 0.5f, localPosition.z);
		int num = Mathf.CeilToInt(instantiatedContainer.transform.localPosition.x);
		if (num < objectContainerCollection.CustomEventTypes.Count && num >= 0)
		{
			queuedData.Type = objectContainerCollection.CustomEventTypes[num];
		}
	}

	internal override void ApplyToMap()
	{
		queuedData.Data = new JSONObject();
		base.ApplyToMap();
	}

	public override void TransferQueuedToDraggedObject(ref BaseCustomEvent dragged, BaseCustomEvent queued)
	{
	}
}

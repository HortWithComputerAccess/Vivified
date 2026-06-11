using System.Globalization;
using Beatmap.Base;
using TMPro;
using UnityEngine;

namespace Beatmap.Containers;

public class BpmEventContainer : ObjectContainer
{
	[SerializeField]
	private TextMeshProUGUI bpmText;

	public BaseBpmEvent BpmData;

	public override BaseObject ObjectData
	{
		get
		{
			return BpmData;
		}
		set
		{
			BpmData = (BaseBpmEvent)value;
		}
	}

	public static BpmEventContainer SpawnBpmChange(BaseBpmEvent data, ref GameObject prefab)
	{
		BpmEventContainer component = Object.Instantiate(prefab).GetComponent<BpmEventContainer>();
		component.BpmData = data;
		return component;
	}

	public void UpdateBpmText()
	{
		bpmText.text = BpmData.Bpm.ToString(CultureInfo.InvariantCulture);
	}

	public override void UpdateGridPosition()
	{
		base.transform.localPosition = new Vector3(0.5f, 0.5f, BpmData.SongBpmTime * EditorScaleController.EditorScale);
		bpmText.text = BpmData.Bpm.ToString(CultureInfo.InvariantCulture);
		UpdateCollisionGroups();
	}
}

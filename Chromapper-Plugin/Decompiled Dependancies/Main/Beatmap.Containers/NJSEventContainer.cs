using System.Globalization;
using Beatmap.Base;
using TMPro;
using UnityEngine;

namespace Beatmap.Containers;

public class NJSEventContainer : ObjectContainer
{
	[SerializeField]
	private TextMeshProUGUI njsText;

	public BaseNJSEvent NJSData;

	public override BaseObject ObjectData
	{
		get
		{
			return NJSData;
		}
		set
		{
			NJSData = (BaseNJSEvent)value;
		}
	}

	public static NJSEventContainer SpawnNJSEvent(BaseNJSEvent data, ref GameObject prefab)
	{
		NJSEventContainer component = Object.Instantiate(prefab).GetComponent<NJSEventContainer>();
		component.NJSData = data;
		return component;
	}

	public void UpdateNJSText()
	{
		float num = BeatSaberSongContainer.Instance.MapDifficultyInfo.NoteJumpSpeed + NJSData.RelativeNJS;
		njsText.text = num.ToString(CultureInfo.InvariantCulture);
		njsText.enabled = NJSData.UsePrevious != 1;
	}

	public override void UpdateGridPosition()
	{
		base.transform.localPosition = new Vector3(0.5f, 0.5f, NJSData.SongBpmTime * EditorScaleController.EditorScale);
		UpdateNJSText();
		UpdateCollisionGroups();
	}
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiTimelineContainer : MonoBehaviour
{
	[SerializeField]
	private Graphic coloredGraphic;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Outline outline;

	private MultiTimelineController source;

	private MapperPosePacket pose;

	public void Init(MultiTimelineController source, MapperIdentityPacket identity)
	{
		this.source = source;
		coloredGraphic.color = identity.Color;
		nameText.text = identity.Name;
		HsvColor hsvColor = HSVUtil.ConvertRgbToHsv(identity.Color);
		hsvColor.V = 0.5;
		outline.effectColor = HSVUtil.ConvertHsvToRgb(hsvColor.H, hsvColor.S, hsvColor.V, 1f);
	}

	public void RefreshPosition(MapperPosePacket pose, float width, float songLength)
	{
		this.pose = pose;
		float num = width / songLength;
		((RectTransform)base.transform).anchoredPosition = new Vector2(num * pose.SongPosition, 50f);
	}

	public void JumpToMapper()
	{
		source.JumpTo(pose);
	}
}

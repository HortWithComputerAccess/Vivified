using System.Globalization;
using Beatmap.Base;
using Beatmap.Containers;
using UnityEngine;

namespace Beatmap.Appearances;

[CreateAssetMenu(menuName = "Beatmap/Appearance/Event Appearance SO", fileName = "EventAppearanceSO")]
public class EventAppearanceSO : ScriptableObject
{
	[Space(5f)]
	[SerializeField]
	private GameObject laserSpeedPrefab;

	[Space(5f)]
	[Header("Default Colors")]
	public Color RedColor;

	public Color BlueColor;

	public Color WhiteColor = new Color(77f / 106f, 77f / 106f, 77f / 106f);

	public Color RedBoostColor;

	public Color BlueBoostColor;

	public Color WhiteBoostColor = new Color(77f / 106f, 77f / 106f, 77f / 106f);

	[SerializeField]
	private Color offColor;

	[Header("Other Event Colors")]
	[SerializeField]
	private Color ringEventsColor;

	[Tooltip("Example: Ring rotate/Ring zoom/Light speed change events")]
	[SerializeField]
	private Color otherColor;

	public void SetEventAppearance(EventContainer e, bool final = true, bool boost = false)
	{
		Color color = Color.white;
		string text = EnvironmentInfoHelper.GetName();
		e.UpdateOffset(Vector3.zero, updateMaterials: false);
		e.UpdateAlpha(final ? 1f : 0.6f, updateMaterials: false);
		e.UpdateScale(final ? 0.75f : 0.6f);
		e.ChangeSpotlightSize(1f, updateMaterials: false);
		if (e.EventData.IsLaneRotationEvent() || e.EventData.IsLaserRotationEvent(text) || e.EventData.IsUtilityEvent(text))
		{
			if (e.EventData.IsLaneRotationEvent())
			{
				float rotation = e.EventData.Rotation;
				e.UpdateTextDisplay(visible: true, $"{rotation}°");
			}
			else if (e.EventData.IsLaserRotationEvent(text) || e.EventData.IsUtilityEvent(text))
			{
				float num = e.EventData.Value;
				if (e.EventData.CustomSpeed.HasValue)
				{
					num = e.EventData.CustomSpeed.Value;
				}
				e.UpdateTextDisplay(visible: true, num.ToString(CultureInfo.InvariantCulture));
			}
		}
		else
		{
			e.UpdateTextDisplay(visible: false);
		}
		if (!e.EventData.IsLightEvent(text))
		{
			e.EventModel = EventModelType.Block;
			if (!(text == "InterscopeEnvironment"))
			{
				if (text == "BillieEnvironment" && e.EventData.Type == 8)
				{
					e.UpdateTextDisplay(visible: true, e.EventData.Value.ToString());
				}
			}
			else
			{
				int type = e.EventData.Type;
				if (type == 8 || type == 17 || type == 18)
				{
					e.UpdateTextDisplay(visible: true, e.EventData.Value.ToString());
				}
			}
			if (e.EventData.IsRingEvent(text))
			{
				e.ChangeColor(ringEventsColor, updateMaterials: false);
				e.ChangeBaseColor(ringEventsColor, updateMaterials: false);
			}
			else
			{
				if (e.EventData.Type == 5)
				{
					if (e.EventData.Value == 1)
					{
						e.ChangeBaseColor(RedBoostColor, updateMaterials: false);
						e.ChangeColor(BlueBoostColor, updateMaterials: false);
					}
					else
					{
						e.ChangeBaseColor(RedColor, updateMaterials: false);
						e.ChangeColor(BlueColor, updateMaterials: false);
					}
					e.UpdateOffset(Vector3.forward * 1.05f, updateMaterials: false);
					e.ChangeFadeSize(e.BoostEventFadeSize, updateMaterials: false);
					e.UpdateGradientRendering();
					e.UpdateMaterials();
					return;
				}
				e.ChangeColor(otherColor, updateMaterials: false);
				e.ChangeBaseColor(otherColor, updateMaterials: false);
			}
			e.UpdateOffset(Vector3.zero, updateMaterials: false);
			e.UpdateGradientRendering();
			e.UpdateMaterials();
			return;
		}
		if (e.EventData.Value >= 2000000000)
		{
			color = ColourManager.ColourFromInt(e.EventData.Value);
			e.UpdateAlpha(final ? 0.9f : 0.6f, updateMaterials: false);
		}
		else if (e.EventData.IsOff)
		{
			color = offColor;
		}
		else if (e.EventData.IsBlue)
		{
			color = (boost ? BlueBoostColor : BlueColor);
		}
		else if (e.EventData.IsRed)
		{
			color = (boost ? RedBoostColor : RedColor);
		}
		else if (e.EventData.IsWhite)
		{
			color = (boost ? WhiteBoostColor : WhiteColor);
		}
		if (Settings.Instance.EmulateChromaLite && e.EventData.CustomColor.HasValue && !e.EventData.IsOff && !e.EventData.IsWhite)
		{
			color = e.EventData.CustomColor.Value;
		}
		if (e.EventData.IsLightEvent(text) && e.EventData.Value != 0)
		{
			if (Settings.Instance.DisplayFloatValueText)
			{
				string text2 = (e.EventData.IsTransition ? $"T{Mathf.RoundToInt(e.EventData.FloatValue * 100f)}" : $"{Mathf.RoundToInt(e.EventData.FloatValue * 100f)}");
				e.UpdateTextDisplay(visible: true, text2);
			}
			color = Color.Lerp(Color.Lerp(offColor, color, 0.25f), color, e.EventData.FloatValue);
		}
		e.EventModel = Settings.Instance.EventModel;
		e.ChangeColor(color, updateMaterials: false);
		e.ChangeBaseColor(Color.black, updateMaterials: false);
		switch (e.EventData.Value)
		{
		case 0:
			e.ChangeColor(offColor, updateMaterials: false);
			e.ChangeBaseColor(offColor, updateMaterials: false);
			e.UpdateOffset(Vector3.zero, updateMaterials: false);
			break;
		case 1:
		case 5:
		case 9:
			e.UpdateOffset(Vector3.zero, updateMaterials: false);
			e.ChangeBaseColor(color, updateMaterials: false);
			break;
		case 2:
		case 6:
		case 10:
			e.UpdateOffset(e.FlashShaderOffset, updateMaterials: false);
			break;
		case 3:
		case 7:
		case 11:
			e.UpdateOffset(e.FadeShaderOffset, updateMaterials: false);
			break;
		case 4:
		case 8:
		case 12:
			e.ChangeBaseColor(color, updateMaterials: false);
			break;
		}
		e.ChangeFadeSize(e.DefaultFadeSize, updateMaterials: false);
		Color? endColor = null;
		BaseEvent next = e.EventData.Next;
		if (!e.EventData.IsFade && !e.EventData.IsFlash && next != null && next.IsTransition)
		{
			if (next.IsBlue)
			{
				endColor = (boost ? BlueBoostColor : BlueColor);
			}
			else if (next.IsRed)
			{
				endColor = (boost ? RedBoostColor : RedColor);
			}
			else if (next.IsWhite)
			{
				endColor = (boost ? WhiteBoostColor : WhiteColor);
			}
			if (Settings.Instance.EmulateChromaLite && next.CustomColor.HasValue && !next.IsWhite)
			{
				endColor = next.CustomColor.Value;
			}
			Color a = Color.Lerp(offColor, endColor.Value, 0.25f);
			endColor = Color.Lerp(a, endColor.Value, next.FloatValue);
		}
		if (Settings.Instance.VisualizeChromaGradients)
		{
			e.UpdateGradientRendering(color, endColor, e.EventData?.CustomEasing ?? "easeLinear");
		}
		e.UpdateMaterials();
	}
}

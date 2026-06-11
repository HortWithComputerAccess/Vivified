using System;
using System.Collections.Generic;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Shared;
using TMPro;
using UnityEngine;

namespace Beatmap.Containers;

public class EventContainer : ObjectContainer
{
	public static int ModifyTypeMode = 0;

	private static readonly int colorBase = Shader.PropertyToID("_ColorBase");

	private static readonly int colorTint = Shader.PropertyToID("_ColorTint");

	private static readonly int position = Shader.PropertyToID("_Position");

	private static readonly int mainAlpha = Shader.PropertyToID("_MainAlpha");

	private static readonly int fadeSize = Shader.PropertyToID("_FadeSize");

	private static readonly int spotlightSize = Shader.PropertyToID("_SpotlightSize");

	[SerializeField]
	private EventGridContainer EventGridContainer;

	[SerializeField]
	private EventAppearanceSO eventAppearance;

	[SerializeField]
	private List<Renderer> eventRenderer;

	[SerializeField]
	private TracksManager tracksManager;

	[SerializeField]
	private TextMeshPro valueDisplay;

	[SerializeField]
	private LightGradientController lightGradientController;

	[SerializeField]
	private GameObject[] eventModels;

	[SerializeField]
	private CreateEventTypeLabels labels;

	[SerializeField]
	public BaseEvent EventData;

	private int eventModel;

	private float oldAlpha = -1f;

	public EventModelType EventModel
	{
		get
		{
			return (EventModelType)eventModel;
		}
		set
		{
			for (int i = 0; i < eventModels.Length; i++)
			{
				eventModels[i].SetActive(i == (int)value);
			}
			eventModel = (int)value;
		}
	}

	public Vector3 FlashShaderOffset => eventModels[eventModel].GetComponent<MaterialParameters>().FlashShaderOffset;

	public Vector3 FadeShaderOffset => eventModels[eventModel].GetComponent<MaterialParameters>().FadeShaderOffset;

	public float DefaultFadeSize => eventModels[eventModel].GetComponent<MaterialParameters>().DefaultFadeSize;

	public float BoostEventFadeSize => eventModels[eventModel].GetComponent<MaterialParameters>().BoostEventFadeSize;

	public override BaseObject ObjectData
	{
		get
		{
			return EventData;
		}
		set
		{
			EventData = (BaseEvent)value;
		}
	}

	public static EventContainer SpawnEvent(EventGridContainer eventsContainer, BaseEvent data, ref GameObject prefab, ref EventAppearanceSO eventAppearanceSO, ref CreateEventTypeLabels labels)
	{
		EventContainer component = UnityEngine.Object.Instantiate(prefab).GetComponent<EventContainer>();
		component.EventData = data;
		component.EventGridContainer = eventsContainer;
		component.eventAppearance = eventAppearanceSO;
		component.labels = labels;
		component.transform.localEulerAngles = Vector3.zero;
		return component;
	}

	public override void Setup()
	{
		if (MaterialPropertyBlock == null)
		{
			MaterialPropertyBlock = new MaterialPropertyBlock();
			modelRenderers.AddRange(eventRenderer);
		}
	}

	public override void UpdateGridPosition()
	{
		Vector2? vector = EventData.GetPosition(labels, EventGridContainer.PropagationEditing, EventGridContainer.EventTypeToPropagate);
		if (!vector.HasValue)
		{
			base.transform.localPosition = new Vector3(-0.5f, 0.5f, EventData.SongBpmTime * EditorScaleController.EditorScale);
			SafeSetActive(active: false);
		}
		else
		{
			base.transform.localPosition = new Vector3(vector.Value.x, vector.Value.y, EventData.SongBpmTime * EditorScaleController.EditorScale);
		}
		base.transform.localEulerAngles = Vector3.zero;
		if (EventData.CustomLightGradient != null && Settings.Instance.VisualizeChromaGradients)
		{
			lightGradientController.UpdateDuration(EventData.CustomLightGradient.Duration);
		}
		if (Settings.Instance.VisualizeChromaAlpha)
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y + (GetHeight() - 1f) / 2.775f, base.transform.localPosition.z);
		}
		UpdateCollisionGroups();
	}

	public void ChangeColor(Color c, bool updateMaterials = true)
	{
		MaterialPropertyBlock.SetColor(colorTint, c);
		if (updateMaterials)
		{
			UpdateMaterials();
		}
	}

	public void ChangeBaseColor(Color c, bool updateMaterials = true)
	{
		MaterialPropertyBlock.SetColor(colorBase, c);
		if (updateMaterials)
		{
			UpdateMaterials();
		}
	}

	public void ChangeFadeSize(float size, bool updateMaterials = true)
	{
		MaterialPropertyBlock.SetFloat(fadeSize, size);
		if (updateMaterials)
		{
			UpdateMaterials();
		}
	}

	public void ChangeSpotlightSize(float size, bool updateMaterials = true)
	{
		MaterialPropertyBlock.SetFloat(spotlightSize, size);
		if (updateMaterials)
		{
			UpdateMaterials();
		}
	}

	public void UpdateOffset(Vector3 offset, bool updateMaterials = true)
	{
		MaterialPropertyBlock.SetVector(position, offset);
		if (updateMaterials)
		{
			UpdateMaterials();
		}
	}

	public void UpdateAlpha(float alpha, bool updateMaterials = true)
	{
		float num = MaterialPropertyBlock.GetFloat(mainAlpha);
		if (num > 0f)
		{
			oldAlpha = num;
		}
		if (oldAlpha != alpha)
		{
			MaterialPropertyBlock.SetFloat(mainAlpha, (alpha == -1f) ? oldAlpha : alpha);
			if (updateMaterials)
			{
				UpdateMaterials();
			}
		}
	}

	public void UpdateScale(float scale)
	{
		base.transform.localScale = new Vector3(1f, Settings.Instance.VisualizeChromaAlpha ? GetHeight() : 1f, 1f) * scale;
	}

	private float GetHeight()
	{
		if (!EventData.IsLightEvent())
		{
			return 1f;
		}
		float num = EventData.FloatValue;
		if (EventData.CustomColor.HasValue && (double)Math.Abs(EventData.CustomColor.Value.a - 1f) > 0.001)
		{
			num *= EventData.CustomColor.Value.a;
		}
		else if (EventData.CustomLightGradient != null && (double)Math.Abs(EventData.CustomLightGradient.StartColor.a - 1f) > 0.001)
		{
			num *= EventData.CustomLightGradient.StartColor.a;
		}
		return Mathf.Clamp(num, 0.1f, 1.5f);
	}

	public void UpdateGradientRendering(Color? startColor = null, Color? endColor = null, string easing = "easeLinear")
	{
		if (!EventData.IsLightEvent(BeatSaberSongContainer.Instance.Info.EnvironmentName))
		{
			lightGradientController.SetVisible(visible: false);
		}
		else if (EventData.CustomLightGradient != null)
		{
			if (Settings.Instance.EmulateChromaLite && EventData.Value != 0)
			{
				ChangeColor(EventData.CustomLightGradient.StartColor);
				ChangeBaseColor(EventData.CustomLightGradient.StartColor);
			}
			lightGradientController.SetVisible(visible: true);
			lightGradientController.UpdateGradientData(EventData.CustomLightGradient);
		}
		else if (!startColor.HasValue || !endColor.HasValue)
		{
			lightGradientController.SetVisible(visible: false);
		}
		else
		{
			ChromaLightGradient chromaLightGradient = new ChromaLightGradient(startColor.Value, endColor.Value, EventData.Next.SongBpmTime - EventData.SongBpmTime, easing);
			lightGradientController.SetVisible(visible: true);
			lightGradientController.UpdateGradientData(chromaLightGradient);
			lightGradientController.UpdateDuration(chromaLightGradient.Duration);
		}
	}

	public void UpdateTextDisplay(bool visible, string text = "")
	{
		if (visible != valueDisplay.gameObject.activeSelf)
		{
			valueDisplay.gameObject.SetActive(visible);
		}
		valueDisplay.text = text;
	}

	public void RefreshAppearance()
	{
		eventAppearance.SetEventAppearance(this);
	}
}

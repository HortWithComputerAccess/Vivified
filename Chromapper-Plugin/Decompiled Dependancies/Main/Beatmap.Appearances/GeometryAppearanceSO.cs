using System;
using Beatmap.Base.Customs;
using Beatmap.Containers;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Appearances;

[CreateAssetMenu(menuName = "Beatmap/Appearance/Geometry Appearance SO", fileName = "GeometryAppearanceSO")]
public class GeometryAppearanceSO : ScriptableObject
{
	private enum ShaderType
	{
		Standard,
		OpaqueLight,
		TransparentLight,
		BaseWater,
		BillieWater,
		BTSPillar,
		InterscopeConcrete,
		InterscopeCar,
		Obstacle,
		WaterfallMirror
	}

	[SerializeField]
	private Material lightMaterial;

	[SerializeField]
	private Material shinyMaterial;

	[SerializeField]
	private Material obstacleMaterial;

	[SerializeField]
	private Material regularMaterial;

	private static BaseMaterial standard;

	public void OnEnable()
	{
		standard = new BaseMaterial
		{
			Shader = "Standard"
		};
	}

	public void SetGeometryAppearance(GeometryContainer container)
	{
		BaseEnvironmentEnhancement environmentEnhancement = container.EnvironmentEnhancement;
		BaseMaterial value = standard;
		JSONNode jSONNode = environmentEnhancement.Geometry[environmentEnhancement.GeometryKeyMaterial];
		if (!(jSONNode is JSONString jSONString))
		{
			if (jSONNode is JSONObject node)
			{
				value = new BaseMaterial(node);
			}
			else
			{
				Debug.LogError("Geometry with invalid material!");
			}
		}
		else if (jSONString.Value != "standard" && !BeatSaberSongContainer.Instance.Map.Materials.TryGetValue(jSONString.Value, out value))
		{
			Debug.LogError("Missing material \"" + jSONString.Value + "\"!");
			value = standard;
		}
		ShaderType result = ShaderType.Standard;
		if (!Enum.TryParse<ShaderType>(value.Shader ?? "Standard", out result))
		{
			Debug.LogError("Invalid shader '" + value.Shader + "'!");
		}
		MeshRenderer component = container.Shape.GetComponent<MeshRenderer>();
		Material sharedMaterial = result switch
		{
			ShaderType.OpaqueLight => lightMaterial, 
			ShaderType.TransparentLight => lightMaterial, 
			ShaderType.BaseWater => shinyMaterial, 
			ShaderType.BillieWater => shinyMaterial, 
			ShaderType.WaterfallMirror => shinyMaterial, 
			ShaderType.Obstacle => obstacleMaterial, 
			_ => regularMaterial, 
		};
		string colorKeyword = result switch
		{
			ShaderType.OpaqueLight => "_EmissionColor", 
			ShaderType.TransparentLight => "_EmissionColor", 
			ShaderType.Obstacle => "_ColorTint", 
			_ => "_Color", 
		};
		Color? color = value.Color;
		if (color.HasValue)
		{
			Color valueOrDefault = color.GetValueOrDefault();
			container.MaterialPropertyBlock.SetColor(colorKeyword, valueOrDefault);
		}
		string track = value.Track;
		if (track != null)
		{
			container.MaterialAnimator.AttachToMaterial(container, track, colorKeyword);
		}
		component.sharedMaterial = sharedMaterial;
		component.SetPropertyBlock(container.MaterialPropertyBlock);
		JSONNode components = environmentEnhancement.Components;
		if ((object)components != null && components.HasKey("ILightWithId"))
		{
			LightingEvent lightingEvent = container.Shape.AddComponent<LightingEvent>();
			lightingEvent.OverrideLightGroup = true;
			lightingEvent.OverrideLightGroupID = environmentEnhancement.LightType.GetValueOrDefault();
			lightingEvent.LightID = environmentEnhancement.LightID.GetValueOrDefault();
			lightingEvent.PropGroup = -1;
		}
	}

	private static bool IsLightType(ShaderType shaderType)
	{
		if (shaderType != ShaderType.OpaqueLight && shaderType != ShaderType.TransparentLight)
		{
			return shaderType == ShaderType.BillieWater;
		}
		return true;
	}
}

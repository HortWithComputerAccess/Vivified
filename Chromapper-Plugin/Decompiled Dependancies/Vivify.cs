// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.

// C:\Users\ninja\BSManager\BSInstances\1.40.8\Plugins\Vivify.dll
// Vivify, Version=1.0.5.0, Culture=neutral, PublicKeyToken=null
// Global type: <Module>
// Architecture: AnyCPU (64-bit preferred)
// Runtime: v4.0.30319
// This assembly was compiled using the /deterministic option.
// Hash algorithm: SHA1

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using Heck.Deserialize;
using Heck.Event;
using Heck.Module;
using Heck.Patcher;
using Heck.PlayView;
using Heck.ReLoad;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Logging;
using IPA.Utilities;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using SiraUtil.Affinity;
using SiraUtil.Extras;
using SiraUtil.Logging;
using SiraUtil.Sabers;
using SiraUtil.Zenject;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using Vivify.Controllers;
using Vivify.Controllers.Sync;
using Vivify.Events;
using Vivify.Extras;
using Vivify.HarmonyPatches;
using Vivify.Installers;
using Vivify.Managers;
using Vivify.ObjectPrefab.Collections;
using Vivify.ObjectPrefab.Hijackers;
using Vivify.ObjectPrefab.Managers;
using Vivify.ObjectPrefab.Pools;
using Vivify.PostProcessing;
using Vivify.Settings;
using Vivify.TrackGameObject;
using Zenject;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: InternalsVisibleTo("IPA.Config.Generated")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("DataModels")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("HMRendering")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("Main")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("Rendering")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("SaberTrail")]
[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("SiraUtil")]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyCompany("Vivify")]
[assembly: AssemblyConfiguration("Release-1.40.3")]
[assembly: AssemblyDescription("Bring your map to life!")]
[assembly: AssemblyFileVersion("1.0.5.0")]
[assembly: AssemblyInformationalVersion("1.0.5+5a8b77dd8a1da58b6051599310e5197b298a9aac")]
[assembly: AssemblyProduct("Vivify")]
[assembly: AssemblyTitle("Vivify")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.5.0")]
[module: UnverifiableCode]
[module: System.Runtime.CompilerServices.RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace Vivify
{
	[UsedImplicitly(/*Could not decode attribute arguments.*/)]
	public class Config
	{
		public int MaxCamera2Cams { get; set; } = 2;

		public bool AllowDownload { get; set; }

		public string BundleRepository { get; set; } = "https://repo.totalbs.dev/api/v1/bundles/";
	}
	internal class CustomDataDeserializer : IEarlyDeserializer, IObjectsDeserializer, ICustomEventsDeserializer
	{
		private readonly CustomBeatmapData _beatmapData;

		private readonly float _bpm;

		private readonly Dictionary<string, List<object>> _pointDefinitions;

		private readonly TrackBuilder _trackBuilder;

		private readonly Dictionary<string, Track> _tracks;

		private CustomDataDeserializer(TrackBuilder trackBuilder, CustomBeatmapData beatmapData, Dictionary<string, List<object>> pointDefinitions, Dictionary<string, Track> tracks, float bpm)
		{
			_trackBuilder = trackBuilder;
			_beatmapData = beatmapData;
			_pointDefinitions = pointDefinitions;
			_tracks = tracks;
			_bpm = bpm;
		}

		public Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents()
		{
			Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new Dictionary<CustomEventData, ICustomEventCustomData>();
			foreach (CustomEventData customEventData in _beatmapData.customEventDatas)
			{
				try
				{
					CustomData customData = customEventData.customData;
					string eventType = customEventData.eventType;
					if (eventType == null)
					{
						continue;
					}
					switch (eventType.Length)
					{
					case 19:
						switch (eventType[3])
						{
						case 'a':
							if (eventType == "CreateScreenTexture")
							{
								dictionary.Add(customEventData, new CreateScreenTextureData(customData));
							}
							break;
						case 'M':
							if (eventType == "SetMaterialProperty")
							{
								dictionary.Add(customEventData, new SetMaterialPropertyData(customData, _pointDefinitions));
							}
							break;
						case 'A':
							if (eventType == "SetAnimatorProperty")
							{
								dictionary.Add(customEventData, new SetAnimatorPropertyData(customData, _pointDefinitions));
							}
							break;
						}
						break;
					case 17:
						switch (eventType[3])
						{
						case 't':
							if (eventType == "InstantiatePrefab")
							{
								dictionary.Add(customEventData, new InstantiatePrefabData(customData, _tracks));
							}
							break;
						case 'G':
							if (eventType == "SetGlobalProperty")
							{
								dictionary.Add(customEventData, new SetGlobalPropertyData(customData, _pointDefinitions));
							}
							break;
						case 'C':
							if (eventType == "SetCameraProperty")
							{
								dictionary.Add(customEventData, new SetCameraPropertyData(customData, _tracks));
							}
							break;
						}
						break;
					case 4:
						if (eventType == "Blit")
						{
							dictionary.Add(customEventData, new ApplyPostProcessingData(customData, _pointDefinitions));
						}
						break;
					case 18:
						if (eventType == "AssignObjectPrefab")
						{
							dictionary.Add(customEventData, new AssignObjectPrefabData(customData, _tracks));
						}
						break;
					case 12:
						if (eventType == "CreateCamera")
						{
							dictionary.Add(customEventData, new CreateCameraData(customData, _tracks));
						}
						break;
					case 13:
						if (eventType == "DestroyObject")
						{
							dictionary.Add(customEventData, new DestroyObjectData(customData));
						}
						break;
					case 20:
						if (eventType == "SetRenderingSettings")
						{
							dictionary.Add(customEventData, new SetRenderingSettingsData(customData, _pointDefinitions));
						}
						break;
					}
				}
				catch (Exception e)
				{
					Plugin.Log.DeserializeFailure(e, customEventData, _bpm);
				}
			}
			return dictionary;
		}

		public void DeserializeEarly()
		{
			foreach (CustomEventData customEventData in _beatmapData.customEventDatas)
			{
				try
				{
					if (customEventData.eventType == "InstantiatePrefab")
					{
						_trackBuilder.AddFromCustomData(customEventData.customData, v2: false, required: false);
					}
				}
				catch (Exception e)
				{
					Plugin.Log.DeserializeFailure(e, customEventData, _bpm);
				}
			}
		}

		public Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects()
		{
			Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new Dictionary<BeatmapObjectData, IObjectCustomData>();
			foreach (BeatmapObjectData beatmapObjectData in _beatmapData.beatmapObjectDatas)
			{
				try
				{
					CustomData customData = ((ICustomData)beatmapObjectData).customData;
					dictionary.Add(beatmapObjectData, new VivifyObjectData(customData, _tracks));
				}
				catch (Exception e)
				{
					Plugin.Log.DeserializeFailure(e, beatmapObjectData, _bpm);
				}
			}
			return dictionary;
		}
	}
	internal class AnimatedMaterialProperty<T> : MaterialProperty where T : struct
	{
		internal PointDefinition<T> PointDefinition { get; }

		internal AnimatedMaterialProperty(CustomData rawData, MaterialPropertyType materialPropertyType, object value, Dictionary<string, List<object>> pointDefinitions)
			: base(rawData, materialPropertyType, value)
		{
			PointDefinition = rawData.GetPointData<T>("value", pointDefinitions) ?? throw new JsonNotDefinedException("value");
		}
	}
	internal class MaterialProperty
	{
		internal object Id { get; }

		internal MaterialPropertyType Type { get; }

		internal object Value { get; }

		internal MaterialProperty(CustomData rawData, MaterialPropertyType materialPropertyType, object value)
		{
			string required = rawData.GetRequired<string>("id");
			Id = ((materialPropertyType != MaterialPropertyType.Keyword) ? ((object)Shader.PropertyToID(required)) : required);
			Type = materialPropertyType;
			Value = value;
		}

		internal static MaterialProperty CreateMaterialProperty(CustomData rawData, Dictionary<string, List<object>> pointDefinitions)
		{
			MaterialPropertyType stringToEnumRequired = rawData.GetStringToEnumRequired<MaterialPropertyType>("type");
			object required = rawData.GetRequired<object>("value");
			if (required is List<object>)
			{
				return stringToEnumRequired switch
				{
					MaterialPropertyType.Color => new AnimatedMaterialProperty<Vector4>(rawData, stringToEnumRequired, required, pointDefinitions), 
					MaterialPropertyType.Float => new AnimatedMaterialProperty<float>(rawData, stringToEnumRequired, required, pointDefinitions), 
					MaterialPropertyType.Vector => new AnimatedMaterialProperty<Vector4>(rawData, stringToEnumRequired, required, pointDefinitions), 
					MaterialPropertyType.Keyword => new AnimatedMaterialProperty<float>(rawData, stringToEnumRequired, required, pointDefinitions), 
					_ => throw new ArgumentOutOfRangeException("type", stringToEnumRequired, "Type not currently supported."), 
				};
			}
			return new MaterialProperty(rawData, stringToEnumRequired, required);
		}
	}
	internal class CameraProperty
	{
		internal class CullingData
		{
			internal IEnumerable<Track> Tracks { get; }

			internal bool Whitelist { get; }

			internal CullingData(CustomData customData, Dictionary<string, Track> tracks)
			{
				Tracks = customData.GetTrackArray(tracks, v2: false);
				Whitelist = customData.Get<bool?>("whitelist") == true;
			}
		}

		internal bool HasDepthTextureMode { get; }

		internal bool HasClearFlags { get; }

		internal bool HasBackgroundColor { get; }

		internal bool HasCulling { get; }

		internal bool HasBloomPrePass { get; }

		internal bool HasMainEffect { get; }

		internal DepthTextureMode? DepthTextureMode { get; }

		internal CameraClearFlags? ClearFlags { get; }

		internal Color? BackgroundColor { get; }

		internal CullingData? Culling { get; }

		internal bool? BloomPrePass { get; }

		internal bool? MainEffect { get; }

		internal CameraProperty(bool hasDepthTextureMode, bool hasClearFlags, bool hasBackgroundColor, bool hasCulling, bool hasBloomPrePass, bool hasMainEffect, DepthTextureMode? depthTextureMode, CameraClearFlags? clearFlags, Color? backgroundColor, CullingData? culling, bool? bloomPrePass, bool? mainEffect)
		{
			HasDepthTextureMode = hasDepthTextureMode;
			HasClearFlags = hasClearFlags;
			HasBackgroundColor = hasBackgroundColor;
			HasCulling = hasCulling;
			HasBloomPrePass = hasBloomPrePass;
			HasMainEffect = hasMainEffect;
			DepthTextureMode = depthTextureMode;
			ClearFlags = clearFlags;
			BackgroundColor = backgroundColor;
			Culling = culling;
			BloomPrePass = bloomPrePass;
			MainEffect = mainEffect;
		}

		internal static CameraProperty CreateCameraProperty(CustomData customData, Dictionary<string, Track> tracks)
		{
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			object value;
			bool num = customData.TryGetValue("depthTextureMode", out value);
			DepthTextureMode? depthTextureMode = null;
			if (num && value != null)
			{
				depthTextureMode = ((List<object>)value).Aggregate((DepthTextureMode)0, (DepthTextureMode current, object depthTextureModeString) => (DepthTextureMode)(current | (DepthTextureMode)Enum.Parse(typeof(DepthTextureMode), (string)depthTextureModeString)));
			}
			object value2;
			bool flag = customData.TryGetValue("clearFlags", out value2);
			CameraClearFlags? clearFlags = ((flag && value2 != null) ? new CameraClearFlags?((CameraClearFlags)Enum.Parse(typeof(CameraClearFlags), (string)value2)) : ((CameraClearFlags?)null));
			object value3;
			bool flag2 = customData.TryGetValue("backgroundColor", out value3);
			Color? backgroundColor = null;
			if (flag2 && value3 != null)
			{
				List<float> list = ((List<object>)value3).Select(Convert.ToSingle).ToList();
				backgroundColor = new Color(list[0], list[1], list[2], (list.Count > 3) ? list[3] : 1f);
			}
			object value4;
			bool flag3 = customData.TryGetValue("culling", out value4);
			CullingData culling = ((flag3 && value4 != null) ? new CullingData((CustomData)value4, tracks) : null);
			object value5;
			bool flag4 = customData.TryGetValue("bloomPrePass", out value5);
			bool? bloomPrePass = ((flag4 && value5 != null) ? new bool?((bool)value5) : ((bool?)null));
			object value6;
			bool flag5 = customData.TryGetValue("mainEffect", out value6);
			return new CameraProperty(mainEffect: (flag5 && value6 != null) ? new bool?((bool)value6) : ((bool?)null), hasDepthTextureMode: num, hasClearFlags: flag, hasBackgroundColor: flag2, hasCulling: flag3, hasBloomPrePass: flag4, hasMainEffect: flag5, depthTextureMode: depthTextureMode, clearFlags: clearFlags, backgroundColor: backgroundColor, culling: culling, bloomPrePass: bloomPrePass);
		}
	}
	internal class AnimatedAnimatorProperty : AnimatorProperty
	{
		internal PointDefinition<float> PointDefinition { get; }

		internal AnimatedAnimatorProperty(CustomData rawData, AnimatorPropertyType animatorPropertyType, object value, Dictionary<string, List<object>> pointDefinitions)
			: base(rawData, animatorPropertyType, value)
		{
			PointDefinition = rawData.GetPointData<float>("value", pointDefinitions) ?? throw new JsonNotDefinedException("value");
		}
	}
	internal class AnimatorProperty
	{
		internal string Name { get; }

		internal AnimatorPropertyType Type { get; }

		internal object Value { get; }

		internal AnimatorProperty(CustomData rawData, AnimatorPropertyType animatorPropertyType, object value)
		{
			Name = rawData.GetRequired<string>("id");
			Type = animatorPropertyType;
			Value = value;
		}

		internal static AnimatorProperty CreateAnimatorProperty(CustomData rawData, Dictionary<string, List<object>> pointDefinitions)
		{
			AnimatorPropertyType stringToEnumRequired = rawData.GetStringToEnumRequired<AnimatorPropertyType>("type");
			object obj = ((stringToEnumRequired != AnimatorPropertyType.Trigger) ? rawData.GetRequired<object>("value") : (rawData.Get<object>("value") ?? ((object)true)));
			if (!(obj is List<object>))
			{
				return new AnimatorProperty(rawData, stringToEnumRequired, obj);
			}
			return new AnimatedAnimatorProperty(rawData, stringToEnumRequired, obj, pointDefinitions);
		}
	}
	internal abstract class RenderingSettingsProperty
	{
		internal string Name { get; }

		internal RenderingSettingsProperty(string name)
		{
			Name = name;
		}

		internal static RenderingSettingsProperty CreateRenderSettingProperty<T>(string name, object value, CustomData rawData, Dictionary<string, List<object>> pointDefinitions) where T : struct
		{
			if (!(value is List<object>))
			{
				return new RenderingSettingsProperty<T>(name, rawData.GetRequired<T>(name));
			}
			return new AnimatedRenderingSettingsProperty<T>(name, rawData, pointDefinitions);
		}
	}
	internal class AnimatedRenderingSettingsProperty<T> : RenderingSettingsProperty where T : struct
	{
		internal PointDefinition<T> PointDefinition { get; }

		internal AnimatedRenderingSettingsProperty(string name, CustomData rawData, Dictionary<string, List<object>> pointDefinitions)
			: base(name)
		{
			PointDefinition = rawData.GetPointData<T>(name, pointDefinitions) ?? throw new JsonNotDefinedException(name);
		}
	}
	internal class RenderingSettingsProperty<T> : RenderingSettingsProperty
	{
		internal T Value { get; }

		internal RenderingSettingsProperty(string name, T value)
			: base(name)
		{
			Value = value;
		}
	}
	internal class SetMaterialPropertyData : ICustomEventCustomData
	{
		internal string Asset { get; }

		internal float Duration { get; }

		internal Functions Easing { get; }

		internal List<MaterialProperty> Properties { get; }

		internal SetMaterialPropertyData(CustomData customData, Dictionary<string, List<object>> pointDefinitions)
		{
			Easing = customData.GetStringToEnum<Functions?>("easing").GetValueOrDefault();
			Duration = customData.Get<float?>("duration").GetValueOrDefault();
			Asset = customData.GetRequired<string>("asset");
			Properties = (from n in customData.GetRequired<List<object>>("properties")
				select MaterialProperty.CreateMaterialProperty((CustomData)n, pointDefinitions)).ToList();
		}
	}
	internal class SetGlobalPropertyData : ICustomEventCustomData
	{
		internal float Duration { get; }

		internal Functions Easing { get; }

		internal List<MaterialProperty> Properties { get; }

		internal SetGlobalPropertyData(CustomData customData, Dictionary<string, List<object>> pointDefinitions)
		{
			Easing = customData.GetStringToEnum<Functions?>("easing").GetValueOrDefault();
			Duration = customData.Get<float?>("duration").GetValueOrDefault();
			Properties = (from n in customData.GetRequired<List<object>>("properties")
				select MaterialProperty.CreateMaterialProperty((CustomData)n, pointDefinitions)).ToList();
		}
	}
	internal class SetCameraPropertyData : ICustomEventCustomData
	{
		internal string Id { get; }

		internal CameraProperty Property { get; }

		internal SetCameraPropertyData(CustomData customData, Dictionary<string, Track> tracks)
		{
			Id = customData.Get<string>("id") ?? "_Main";
			Property = CameraProperty.CreateCameraProperty(customData.GetRequired<CustomData>("properties"), tracks);
		}
	}
	internal class SetAnimatorPropertyData : ICustomEventCustomData
	{
		internal float Duration { get; }

		internal Functions Easing { get; }

		internal string Id { get; }

		internal List<AnimatorProperty> Properties { get; }

		internal SetAnimatorPropertyData(CustomData customData, Dictionary<string, List<object>> pointDefinitions)
		{
			Easing = customData.GetStringToEnum<Functions?>("easing").GetValueOrDefault();
			Duration = customData.Get<float?>("duration").GetValueOrDefault();
			Id = customData.GetRequired<string>("id");
			Properties = (from n in customData.GetRequired<List<object>>("properties")
				select AnimatorProperty.CreateAnimatorProperty((CustomData)n, pointDefinitions)).ToList();
		}
	}
	internal enum PostProcessingOrder
	{
		BeforeMainEffect,
		AfterMainEffect
	}
	internal enum MaterialPropertyType
	{
		Texture,
		Color,
		Float,
		FloatArray,
		Int,
		Vector,
		VectorArray,
		Keyword
	}
	internal enum AnimatorPropertyType
	{
		Bool,
		Float,
		Integer,
		Trigger
	}
	internal class VivifyObjectData : IObjectCustomData, ICopyable<IObjectCustomData>
	{
		internal IReadOnlyList<Track>? Track { get; }

		internal VivifyObjectData(CustomData customData, Dictionary<string, Track> beatmapTracks)
		{
			Track = customData.GetNullableTrackArray(beatmapTracks, v2: false)?.ToList();
		}

		internal VivifyObjectData(VivifyObjectData original)
		{
			Track = original.Track;
		}

		public IObjectCustomData Copy()
		{
			return new VivifyObjectData(this);
		}
	}
	internal class ApplyPostProcessingData : ICustomEventCustomData
	{
		internal string? Asset { get; }

		internal float Duration { get; }

		internal Functions Easing { get; }

		internal int? Pass { get; }

		internal PostProcessingOrder Order { get; }

		internal int Priority { get; }

		internal List<MaterialProperty>? Properties { get; }

		internal string? Source { get; }

		internal string[]? Target { get; }

		internal ApplyPostProcessingData(CustomData customData, Dictionary<string, List<object>> pointDefinitions)
		{
			Easing = customData.GetStringToEnum<Functions?>("easing").GetValueOrDefault();
			Duration = customData.Get<float?>("duration").GetValueOrDefault();
			Priority = customData.Get<int?>("priority").GetValueOrDefault();
			Source = customData.Get<string>("source");
			Asset = customData.Get<string>("asset");
			Pass = customData.Get<int?>("pass");
			Order = customData.GetStringToEnum<PostProcessingOrder?>("order") ?? PostProcessingOrder.AfterMainEffect;
			List<object> list = customData.Get<List<object>>("properties");
			if (list != null)
			{
				Properties = list.Select((object n) => MaterialProperty.CreateMaterialProperty((CustomData)n, pointDefinitions)).ToList();
			}
			object obj = customData.Get<object>("destination");
			string[] array;
			if (obj != null)
			{
				if (!(obj is string text))
				{
					if (!(obj is List<object> source))
					{
						throw new InvalidCastException("[destination] was not an allowable type. Was [" + obj.GetType().FullName + "].");
					}
					array = source.Select((object n) => (string)n).ToArray();
				}
				else
				{
					array = new string[1] { text };
				}
			}
			else
			{
				array = null;
			}
			Target = array;
		}
	}
	internal class SetRenderingSettingsData : ICustomEventCustomData
	{
		internal float Duration { get; }

		internal Functions Easing { get; }

		internal List<RenderingSettingsProperty> Properties { get; } = new List<RenderingSettingsProperty>();

		internal SetRenderingSettingsData(CustomData customData, Dictionary<string, List<object>> pointDefinitions)
		{
			Duration = customData.Get<float?>("duration").GetValueOrDefault();
			Easing = customData.GetStringToEnum<Functions?>("easing").GetValueOrDefault();
			string[] excludedStrings = new string[2] { "duration", "easing" };
			string text = default(string);
			object obj = default(object);
			foreach (KeyValuePair<string, object> item2 in customData.Where<KeyValuePair<string, object>>((KeyValuePair<string, object> n) => excludedStrings.All((string m) => m != n.Key)).ToList())
			{
				Utils.Deconstruct<string, object>(item2, ref text, ref obj);
				string text2 = text;
				CustomData customData2 = ((CustomData)obj) ?? throw new InvalidOperationException();
				foreach (KeyValuePair<string, object> item3 in customData2)
				{
					Utils.Deconstruct<string, object>(item3, ref text, ref obj);
					string text3 = text;
					object obj2 = obj;
					if (obj2 == null)
					{
						continue;
					}
					RenderingSettingsProperty item;
					switch (text2)
					{
					case "renderSettings":
						switch (text3)
						{
						case "ambientIntensity":
						case "fogStartDistance":
						case "flareFadeSpeed":
						case "fogEndDistance":
						case "fog":
						case "haloStrength":
						case "reflectionIntensity":
						case "ambientMode":
						case "defaultReflectionMode":
						case "defaultReflectionResolution":
						case "flareStrength":
						case "fogDensity":
						case "fogMode":
						case "reflectionBounces":
							item = RenderingSettingsProperty.CreateRenderSettingProperty<float>(text3, obj2, customData2, pointDefinitions);
							break;
						case "ambientLight":
						case "ambientEquatorColor":
						case "ambientGroundColor":
						case "ambientSkyColor":
						case "fogColor":
						case "subtractiveShadowColor":
							item = RenderingSettingsProperty.CreateRenderSettingProperty<Vector4>(text3, obj2, customData2, pointDefinitions);
							break;
						case "sun":
						case "skybox":
							item = new RenderingSettingsProperty<string>(text3, customData2.GetRequired<string>(text3));
							break;
						default:
							continue;
						}
						break;
					case "qualitySettings":
						switch (text3)
						{
						case "shadowCascades":
						case "shadowDistance":
						case "shadowmaskMode":
						case "shadowProjection":
						case "shadowResolution":
						case "anisotropicFiltering":
						case "antiAliasing":
						case "pixelLightCount":
						case "realtimeReflectionProbes":
						case "shadowNearPlaneOffset":
						case "shadows":
						case "softParticles":
							break;
						default:
							continue;
						}
						item = RenderingSettingsProperty.CreateRenderSettingProperty<float>(text3, obj2, customData2, pointDefinitions);
						break;
					case "xrSettings":
						if (!(text3 == "useOcclusionMesh"))
						{
							continue;
						}
						item = RenderingSettingsProperty.CreateRenderSettingProperty<float>(text3, obj2, customData2, pointDefinitions);
						break;
					default:
						continue;
					}
					Properties.Add(item);
				}
			}
		}
	}
	internal class CreateCameraData : ICustomEventCustomData
	{
		internal string Name { get; }

		internal string? Texture { get; }

		internal string? DepthTexture { get; }

		internal CameraProperty? Property { get; }

		internal CreateCameraData(CustomData customData, Dictionary<string, Track> tracks)
		{
			Name = customData.GetRequired<string>("id");
			Texture = customData.Get<string>("texture");
			DepthTexture = customData.Get<string>("depthTexture");
			CustomData customData2 = customData.Get<CustomData>("properties");
			if (customData2 != null)
			{
				Property = CameraProperty.CreateCameraProperty(customData2, tracks);
			}
		}
	}
	internal class CreateScreenTextureData : ICustomEventCustomData
	{
		internal FilterMode? FilterMode { get; }

		internal RenderTextureFormat? Format { get; }

		internal int? Height { get; }

		internal string Name { get; }

		internal int PropertyId { get; }

		internal int? Width { get; }

		internal float XRatio { get; }

		internal float YRatio { get; }

		internal CreateScreenTextureData(CustomData customData)
		{
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			Name = customData.GetRequired<string>("id");
			PropertyId = Shader.PropertyToID(Name);
			XRatio = customData.Get<float?>("xRatio") ?? 1f;
			YRatio = customData.Get<float?>("yRatio") ?? 1f;
			Width = customData.Get<int?>("width");
			Height = customData.Get<int?>("height");
			Format = customData.GetStringToEnum<RenderTextureFormat?>("colorFormat");
			FilterMode = customData.GetStringToEnum<FilterMode?>("filterMode");
			if (Format.HasValue && !SystemInfo.SupportsRenderTextureFormat(Format.Value))
			{
				Plugin.Log.Warn($"Current graphics card does not support [{Format.Value}]");
			}
		}
	}
	internal class DestroyObjectData : ICustomEventCustomData
	{
		internal string[] Id { get; }

		internal DestroyObjectData(CustomData customData)
		{
			object required = customData.GetRequired<object>("id");
			string[] array;
			if (!(required is string text))
			{
				if (!(required is List<object> source))
				{
					throw new InvalidCastException("[id] was not an allowable type. Was [" + required.GetType().FullName + "].");
				}
				array = source.Select((object n) => (string)n).ToArray();
			}
			else
			{
				array = new string[1] { text };
			}
			Id = array;
		}
	}
	internal class InstantiatePrefabData : ICustomEventCustomData
	{
		internal string Asset { get; }

		internal string? Id { get; }

		internal List<Track>? Track { get; }

		internal TransformData TransformData { get; }

		internal InstantiatePrefabData(CustomData customData, Dictionary<string, Track> beatmapTracks)
		{
			Asset = customData.GetRequired<string>("asset");
			Id = customData.Get<string>("id");
			TransformData = new TransformData(customData);
			Track = customData.GetNullableTrackArray(beatmapTracks, v2: false)?.ToList();
		}
	}
	internal class AssignObjectPrefabData : ICustomEventCustomData
	{
		internal interface IPrefabInfo
		{
		}

		internal struct ObjectPrefabInfo : IPrefabInfo
		{
			internal string? Asset { get; }

			internal string? DebrisAsset { get; }

			internal string? AnyDirectionAsset { get; }

			internal IReadOnlyList<Track> Track { get; }

			internal ObjectPrefabInfo(string? asset, string? debrisAsset, string? anyDirectionAsset, IReadOnlyList<Track> track)
			{
				Asset = asset;
				DebrisAsset = debrisAsset;
				AnyDirectionAsset = anyDirectionAsset;
				Track = track;
			}
		}

		internal struct SaberPrefabInfo : IPrefabInfo
		{
			[Flags]
			internal enum SaberType
			{
				Left = 1,
				Right = 2,
				Both = 3
			}

			internal SaberType Type { get; }

			internal string? Asset { get; }

			internal string? TrailAsset { get; }

			internal Vector3? TopPos { get; }

			internal Vector3? BottomPos { get; }

			internal float? Duration { get; }

			internal int? SamplingFrequency { get; }

			internal int? Granularity { get; }

			internal SaberPrefabInfo(SaberType type, string? asset, string? trailAsset, Vector3? topPos, Vector3? bottomPos, float? duration, int? samplingFrequency, int? granularity)
			{
				Type = type;
				Asset = asset;
				TrailAsset = trailAsset;
				TopPos = topPos;
				BottomPos = bottomPos;
				Duration = duration;
				SamplingFrequency = samplingFrequency;
				Granularity = granularity;
			}
		}

		internal Dictionary<string, IPrefabInfo> Assets { get; } = new Dictionary<string, IPrefabInfo>();

		internal LoadMode LoadMode { get; }

		internal AssignObjectPrefabData(CustomData customData, Dictionary<string, Track> beatmapTracks)
		{
			LoadMode = customData.GetStringToEnum<LoadMode>("loadMode");
			string text = default(string);
			object obj = default(object);
			foreach (KeyValuePair<string, object> item in customData.Where<KeyValuePair<string, object>>((KeyValuePair<string, object> n) => n.Key != "loadMode"))
			{
				Utils.Deconstruct<string, object>(item, ref text, ref obj);
				string text2 = text;
				CustomData customData2 = ((CustomData)obj) ?? throw new InvalidOperationException("Null value for [" + text2 + "]");
				string asset = string.Empty;
				if (customData2.TryGetValue("asset", out object value))
				{
					asset = (string)value;
				}
				if (text2 == "saber")
				{
					string trailAsset = string.Empty;
					if (customData2.TryGetValue("trailAsset", out object value2))
					{
						trailAsset = (string)value2;
					}
					Assets.Add(text2, new SaberPrefabInfo(customData2.GetStringToEnumRequired<SaberPrefabInfo.SaberType>("type"), asset, trailAsset, customData2.GetVector3("trailTopPos"), customData2.GetVector3("trailBottomPos"), customData2.Get<float?>("trailDuration"), customData2.Get<int?>("trailSamplingFrequency"), customData2.Get<int?>("trailGranularity")));
					continue;
				}
				string debrisAsset = string.Empty;
				if (customData2.TryGetValue("debrisAsset", out object value3))
				{
					debrisAsset = (string)value3;
				}
				string anyDirectionAsset = string.Empty;
				if (customData2.TryGetValue("anyDirectionAsset", out object value4))
				{
					anyDirectionAsset = (string)value4;
				}
				List<Track> track = customData2.GetTrackArray(beatmapTracks, v2: false).ToList();
				Assets.Add(text2, new ObjectPrefabInfo(asset, debrisAsset, anyDirectionAsset, track));
			}
		}
	}
	[Module("Vivify", 2, LoadType.Active, new string[] { "Heck" }, null)]
	[ModulePatcher("aeroluna.VivifyFeatures", PatchType.Features)]
	[ModuleDataDeserializer("Vivify", typeof(CustomDataDeserializer))]
	internal class FeaturesModule : IModule
	{
		internal bool Active { get; private set; }

		[ModuleCondition]
		private static bool Condition(Capabilities capabilities)
		{
			return capabilities.Requirements.Contains<string>("Vivify");
		}

		[ModuleCallback]
		private void Callback(bool value)
		{
			Active = value;
		}
	}
	[Plugin(/*Could not decode attribute arguments.*/)]
	internal class Plugin
	{
		internal static Config Config { get; private set; }

		internal static Logger Log { get; private set; }

		[UsedImplicitly]
		[Init]
		public Plugin(Logger pluginLogger, Config conf, Zenjector zenjector)
		{
			Log = pluginLogger;
			Config config = (Config = GeneratedStore.Generated<Config>(conf, true));
			zenjector.Install<VivifyAppInstaller>(Location.App, new object[1] { config });
			zenjector.Install<VivifyPlayerInstaller>(Location.Player, Array.Empty<object>());
			zenjector.Install<VivifyMenuInstaller>(Location.Menu, Array.Empty<object>());
			zenjector.UseLogger(pluginLogger);
			HeckPatchManager.Register("aeroluna.Vivify");
		}

		[UsedImplicitly]
		[OnEnable]
		public void OnEnable()
		{
			VivifyController.Capability.Register();
		}

		[UsedImplicitly]
		[OnDisable]
		public void OnDisable()
		{
			VivifyController.Capability.Deregister();
		}
	}
	internal enum PatchType
	{
		Features
	}
	public static class VivifyController
	{
		internal const string VALUE = "value";

		internal const string PASS = "pass";

		internal const string PRIORITY = "priority";

		internal const string SOURCE = "source";

		internal const string DESTINATION = "destination";

		internal const string ASSET = "asset";

		internal const string PROPERTIES = "properties";

		internal const string TEXTURE = "texture";

		internal const string DEPTH_TEXTURE = "depthTexture";

		internal const string ID_FIELD = "id";

		internal const string ORDER = "order";

		internal const string X_RATIO = "xRatio";

		internal const string Y_RATIO = "yRatio";

		internal const string WIDTH = "width";

		internal const string HEIGHT = "height";

		internal const string FORMAT = "colorFormat";

		internal const string FILTER = "filterMode";

		internal const string CAMERA_DEPTH_TEXTURE_MODE = "depthTextureMode";

		internal const string CAMERA_CLEAR_FLAGS = "clearFlags";

		internal const string CAMERA_BACKGROUND_COLOR = "backgroundColor";

		internal const string CULLING = "culling";

		internal const string WHITELIST = "whitelist";

		internal const string MAIN_EFFECT = "mainEffect";

		internal const string BLOOMPREPASS = "bloomPrePass";

		internal const string ASSIGN_PREFAB_LOAD_MODE = "loadMode";

		internal const string NOTE_PREFAB = "colorNotes";

		internal const string BOMB_PREFAB = "bombNotes";

		internal const string CHAIN_PREFAB = "burstSliders";

		internal const string CHAIN_ELEMENT_PREFAB = "burstSliderElements";

		internal const string DEBRIS_ASSET = "debrisAsset";

		internal const string ANY_ASSET = "anyDirectionAsset";

		internal const string SABER_PREFAB = "saber";

		internal const string SABER_TYPE = "type";

		internal const string SABER_TRAIL_ASSET = "trailAsset";

		internal const string SABER_TRAIL_TOP_POS = "trailTopPos";

		internal const string SABER_TRAIL_BOTTOM_POS = "trailBottomPos";

		internal const string SABER_TRAIL_DURATION = "trailDuration";

		internal const string SABER_TRAIL_SAMPLE_FREQ = "trailSamplingFrequency";

		internal const string SABER_TRAIL_GRANULARITY = "trailGranularity";

		internal const string RENDER_SETTINGS = "renderSettings";

		internal const string QUALITY_SETTINGS = "qualitySettings";

		internal const string XR_SETTINGS = "xrSettings";

		internal const string APPLY_POST_PROCESSING = "Blit";

		internal const string ASSIGN_OBJECT_PREFAB = "AssignObjectPrefab";

		internal const string DECLARE_CULLING_TEXTURE = "CreateCamera";

		internal const string DECLARE_TEXTURE = "CreateScreenTexture";

		internal const string DESTROY_PREFAB = "DestroyObject";

		internal const string INSTANTIATE_PREFAB = "InstantiatePrefab";

		internal const string SET_MATERIAL_PROPERTY = "SetMaterialProperty";

		internal const string SET_ANIMATOR_PROPERTY = "SetAnimatorProperty";

		internal const string SET_RENDERING_SETTINGS = "SetRenderingSettings";

		internal const string SET_GLOBAL_PROPERTY = "SetGlobalProperty";

		internal const string SET_CAMERA_PROPERTY = "SetCameraProperty";

		internal const string CAMERA_TARGET = "_Main";

		internal const string ASSET_BUNDLE = "_assetBundle";

		internal const string BUNDLE_FILE = "bundleWindows2021.vivify";

		internal const string BUNDLE_SUFFIX = "Windows2021";

		internal const string BUNDLE_CHECKSUM = "_windows2021";

		internal const string CAPABILITY = "Vivify";

		internal const string ID = "Vivify";

		internal const string HARMONY_ID = "aeroluna.Vivify";

		internal const int CULLING_LAYER = 22;

		internal static Capability Capability { get; } = new Capability("Vivify");
	}
}
namespace Vivify.TrackGameObject
{
	internal sealed class CullingTextureTracker : TrackGameObjectTracker
	{
		private readonly HashSet<RendererController> _maskRenderers = new HashSet<RendererController>();

		private GameObject[] _gameObjects = Array.Empty<GameObject>();

		private bool _gameObjectsDirty;

		internal GameObject[] GameObjects
		{
			get
			{
				if (_gameObjectsDirty)
				{
					_gameObjects = (from n in _maskRenderers.SelectMany((RendererController n) => n.ChildRenderers)
						where (Object)(object)n != (Object)null
						select ((Component)n).gameObject).ToArray();
					_gameObjectsDirty = false;
				}
				return _gameObjects;
			}
		}

		internal bool Whitelist { get; }

		internal CullingTextureTracker(IEnumerable<Track> tracks, bool whitelist)
			: base(tracks)
		{
			Whitelist = whitelist;
			UpdateGameObjects();
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach (RendererController maskRenderer in _maskRenderers)
			{
				maskRenderer.OnDestroyed -= OnMaskRendererDestroyed;
				maskRenderer.OnTransformChanged -= UpdateGameObjects;
			}
		}

		protected override void OnGameObjectAdded(GameObject gameObject)
		{
			RendererController rendererController = gameObject.GetComponent<RendererController>();
			if (rendererController == null)
			{
				rendererController = gameObject.AddComponent<RendererController>();
			}
			_maskRenderers.Add(rendererController);
			rendererController.OnDestroyed += OnMaskRendererDestroyed;
			rendererController.OnTransformChanged += UpdateGameObjects;
			UpdateGameObjects();
		}

		protected override void OnGameObjectRemoved(GameObject gameObject)
		{
			RendererController component = gameObject.GetComponent<RendererController>();
			if ((Object)(object)component != (Object)null)
			{
				OnMaskRendererDestroyed(component);
			}
		}

		private void OnMaskRendererDestroyed(RendererController rendererController)
		{
			_maskRenderers.Remove(rendererController);
			rendererController.OnDestroyed -= OnMaskRendererDestroyed;
			rendererController.OnTransformChanged -= UpdateGameObjects;
			UpdateGameObjects();
		}

		private void UpdateGameObjects()
		{
			_gameObjectsDirty = true;
		}
	}
	internal abstract class TrackGameObjectTracker : IDisposable
	{
		private readonly IEnumerable<Track> _tracks;

		internal TrackGameObjectTracker(IEnumerable<Track> tracks)
		{
			_tracks = tracks.ToList();
			foreach (Track track in _tracks)
			{
				foreach (GameObject gameObject in track.GameObjects)
				{
					OnGameObjectAdded(gameObject);
				}
				track.GameObjectAdded += OnGameObjectAdded;
				track.GameObjectRemoved += OnGameObjectRemoved;
			}
		}

		public virtual void Dispose()
		{
			foreach (Track track in _tracks)
			{
				if (track != null)
				{
					track.GameObjectAdded -= OnGameObjectAdded;
					track.GameObjectRemoved -= OnGameObjectRemoved;
				}
			}
		}

		protected abstract void OnGameObjectAdded(GameObject gameObject);

		protected abstract void OnGameObjectRemoved(GameObject gameObject);
	}
}
namespace Vivify.Settings
{
	internal class SettingsMenu : IInitializable, IDisposable
	{
		private readonly Config _config;

		private readonly BSMLSettings _bsmlSettings;

		[UsedImplicitly]
		[UIValue("ints")]
		private readonly List<object> _intChoices = new List<object>(21)
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
			20
		};

		[UsedImplicitly]
		[UIValue("max-camera2-cams")]
		public int MaxCamera2Cams
		{
			get
			{
				return _config.MaxCamera2Cams;
			}
			set
			{
				_config.MaxCamera2Cams = value;
			}
		}

		private SettingsMenu(BSMLSettings bsmlSettings, Config config)
		{
			_config = config;
			_bsmlSettings = bsmlSettings;
		}

		public void Initialize()
		{
			_bsmlSettings.AddSettingsMenu("Vivify", "Vivify.Resources.Settings.bsml", this);
		}

		public void Dispose()
		{
			_bsmlSettings.RemoveSettingsMenu(this);
		}
	}
}
namespace Vivify.PostProcessing
{
	internal abstract class CullingCameraController : MonoBehaviour
	{
		private readonly HashSet<(GameObject, int)> _cachedLayers = new HashSet<(GameObject, int)>();

		private Camera? _camera;

		private int? _cachedMask;

		internal bool MainEffect { get; set; }

		internal Camera Camera => _camera ?? (_camera = ((Component)this).GetComponent<Camera>());

		internal CullingTextureTracker? CullingTextureData { get; set; }

		protected virtual void OnPreCull()
		{
			CullingTextureTracker? cullingTextureData = CullingTextureData;
			if (cullingTextureData != null && cullingTextureData.Whitelist)
			{
				_cachedMask = Camera.cullingMask;
				Camera.cullingMask = 4194304;
			}
			if (CullingTextureData == null)
			{
				return;
			}
			GameObject[] gameObjects = CullingTextureData.GameObjects;
			int num = gameObjects.Length;
			for (int i = 0; i < num; i++)
			{
				GameObject val = gameObjects[i];
				if (!((Object)(object)val == (Object)null))
				{
					_cachedLayers.Add((val, val.layer));
					val.layer = 22;
				}
			}
		}

		private void OnPostRender()
		{
			if (_cachedMask.HasValue)
			{
				Camera.cullingMask = _cachedMask.Value;
				_cachedMask = null;
			}
			if (_cachedLayers.Count == 0)
			{
				return;
			}
			foreach (var (val, layer) in _cachedLayers)
			{
				val.layer = layer;
			}
			_cachedLayers.Clear();
		}
	}
	internal class LateBloomPrePass : BloomPrePass
	{
		private void Awake()
		{
		}

		private void Start()
		{
			((BloomPrePass)this).Awake();
		}
	}
	internal class MainEffectRenderer
	{
		private readonly MainEffectContainerSO _mainEffectContainer;

		private readonly MainEffectController _mainEffectController;

		internal MainEffectRenderer(MainEffectController mainEffectController)
		{
			_mainEffectController = mainEffectController;
			_mainEffectContainer = _mainEffectController._mainEffectContainer;
		}

		internal void Render(RenderTexture src, RenderTexture dest)
		{
			MainEffectSO mainEffect = _mainEffectContainer.mainEffect;
			if (mainEffect.hasPostProcessEffect)
			{
				_mainEffectController.OnPreRender();
				mainEffect.Render(src, dest, ObservableVariableSO<float>.op_Implicit((ObservableVariableSO<float>)(object)_mainEffectController._fadeValue));
				_mainEffectController.OnPostRender();
			}
			else
			{
				Graphics.Blit((Texture)(object)src, dest);
			}
		}
	}
	[RequireComponent(typeof(Camera))]
	internal class PostProcessingController : CullingCameraController
	{
		private readonly Dictionary<CreateCameraData, string> _activeCreateCameraDatas = new Dictionary<CreateCameraData, string>();

		private readonly Dictionary<CreateScreenTextureData, string> _activeDeclaredTextures = new Dictionary<CreateScreenTextureData, string>();

		private readonly Dictionary<string, CullingCameraController> _cullingCameraControllers = new Dictionary<string, CullingCameraController>();

		private readonly Dictionary<string, RenderTextureHolder> _declaredTextures = new Dictionary<string, RenderTextureHolder>();

		private readonly Stack<CullingTextureController> _disabledCullingCameraControllers = new Stack<CullingTextureController>();

		private readonly List<CreateCameraData> _reusableCameraKeys = new List<CreateCameraData>();

		private readonly List<CreateScreenTextureData> _reusableDeclaredKeys = new List<CreateScreenTextureData>();

		private SiraLog _log;

		private IInstantiator _instantiator;

		private ImageEffectController _imageEffectController;

		internal Dictionary<string, CreateCameraData> CameraDatas { get; set; } = new Dictionary<string, CreateCameraData>();

		internal Dictionary<string, CreateScreenTextureData> DeclaredTextureDatas { get; set; } = new Dictionary<string, CreateScreenTextureData>();

		internal List<MaterialData> PreEffects { get; set; } = new List<MaterialData>();

		internal List<MaterialData> PostEffects { get; set; } = new List<MaterialData>();

		internal void PrewarmCameras(int count)
		{
			count -= _disabledCullingCameraControllers.Count + _cullingCameraControllers.Count;
			for (int i = 0; i < count; i++)
			{
				_disabledCullingCameraControllers.Push(CreateCamera());
			}
		}

		protected override void OnPreCull()
		{
			CreateCameraData createCameraData = default(CreateCameraData);
			string text = default(string);
			foreach (KeyValuePair<CreateCameraData, string> activeCreateCameraData in _activeCreateCameraDatas)
			{
				Utils.Deconstruct<CreateCameraData, string>(activeCreateCameraData, ref createCameraData, ref text);
				CreateCameraData createCameraData2 = createCameraData;
				string key = text;
				if (!CameraDatas.ContainsValue(createCameraData2))
				{
					if (_cullingCameraControllers.TryGetValue(key, out CullingCameraController value) && value is CullingTextureController cullingTextureController)
					{
						((Component)cullingTextureController).gameObject.SetActive(false);
						_disabledCullingCameraControllers.Push(cullingTextureController);
					}
					_cullingCameraControllers.Remove(key);
					_reusableCameraKeys.Add(createCameraData2);
				}
			}
			foreach (CreateCameraData reusableCameraKey in _reusableCameraKeys)
			{
				_activeCreateCameraDatas.Remove(reusableCameraKey);
			}
			_reusableCameraKeys.Clear();
			foreach (KeyValuePair<string, CreateCameraData> cameraData in CameraDatas)
			{
				Utils.Deconstruct<string, CreateCameraData>(cameraData, ref text, ref createCameraData);
				string text2 = text;
				CreateCameraData createCameraData3 = createCameraData;
				if (!_activeCreateCameraDatas.ContainsKey(createCameraData3))
				{
					_activeCreateCameraDatas[createCameraData3] = text2;
					CullingTextureController cullingTextureController2 = ((_disabledCullingCameraControllers.Count > 0) ? _disabledCullingCameraControllers.Pop() : CreateCamera());
					cullingTextureController2.Init(createCameraData3);
					((Component)cullingTextureController2).gameObject.SetActive(true);
					_cullingCameraControllers[text2] = cullingTextureController2;
				}
			}
			CreateScreenTextureData createScreenTextureData = default(CreateScreenTextureData);
			foreach (KeyValuePair<CreateScreenTextureData, string> activeDeclaredTexture in _activeDeclaredTextures)
			{
				Utils.Deconstruct<CreateScreenTextureData, string>(activeDeclaredTexture, ref createScreenTextureData, ref text);
				CreateScreenTextureData createScreenTextureData2 = createScreenTextureData;
				string key2 = text;
				if (DeclaredTextureDatas.ContainsValue(createScreenTextureData2))
				{
					continue;
				}
				foreach (RenderTexture value2 in _declaredTextures[key2].Textures.Values)
				{
					if ((Object)(object)value2 != (Object)null)
					{
						value2.Release();
					}
				}
				_declaredTextures.Remove(key2);
				_reusableDeclaredKeys.Add(createScreenTextureData2);
			}
			foreach (CreateScreenTextureData reusableDeclaredKey in _reusableDeclaredKeys)
			{
				_activeDeclaredTextures.Remove(reusableDeclaredKey);
			}
			_reusableDeclaredKeys.Clear();
			foreach (KeyValuePair<string, CreateScreenTextureData> declaredTextureData in DeclaredTextureDatas)
			{
				Utils.Deconstruct<string, CreateScreenTextureData>(declaredTextureData, ref text, ref createScreenTextureData);
				string text3 = text;
				CreateScreenTextureData createScreenTextureData3 = createScreenTextureData;
				if (!_activeDeclaredTextures.ContainsKey(createScreenTextureData3))
				{
					_declaredTextures.Add(text3, new RenderTextureHolder(createScreenTextureData3));
					_activeDeclaredTextures.Add(createScreenTextureData3, text3);
				}
			}
			base.OnPreCull();
		}

		private static void CopyComponent<T, TDerived>(T original, GameObject destination) where T : MonoBehaviour where TDerived : T
		{
			Type typeFromHandle = typeof(T);
			MonoBehaviour obj = (MonoBehaviour)(object)destination.AddComponent<TDerived>();
			FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (Attribute.IsDefined(fieldInfo, typeof(SerializeField)))
				{
					fieldInfo.SetValue(obj, fieldInfo.GetValue(original));
				}
			}
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dst)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			RenderTextureDescriptor descriptor = src.descriptor;
			((RenderTextureDescriptor)(ref descriptor)).msaaSamples = 1;
			CreateDeclaredTextures(descriptor);
			RenderTexture val = RenderTexture.GetTemporary(descriptor);
			RenderImage(src, val, PreEffects);
			RenderImageCallback renderImageCallback = _imageEffectController._renderImageCallback;
			if (renderImageCallback != null && ((Behaviour)_imageEffectController).isActiveAndEnabled)
			{
				RenderTexture temporary = RenderTexture.GetTemporary(descriptor);
				renderImageCallback.Invoke(val, temporary);
				RenderTexture.ReleaseTemporary(val);
				val = temporary;
			}
			RenderImage(val, dst, PostEffects);
			RenderTexture.ReleaseTemporary(val);
		}

		private void OnPreRender()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			MonoOrStereoscopicEye stereoActiveEye = base.Camera.stereoActiveEye;
			foreach (CullingCameraController value4 in _cullingCameraControllers.Values)
			{
				if (value4 is CullingTextureController cullingTextureController)
				{
					Camera camera = cullingTextureController.Camera;
					if (!((Behaviour)camera).enabled)
					{
						camera.Render();
					}
					if (cullingTextureController.Key.HasValue && cullingTextureController.RenderTextures.TryGetValue(stereoActiveEye, out RenderTexture value))
					{
						Shader.SetGlobalTexture(cullingTextureController.Key.Value, (Texture)(object)value);
					}
					if (cullingTextureController.DepthKey.HasValue && cullingTextureController.RenderTexturesDepth.TryGetValue(stereoActiveEye, out RenderTexture value2))
					{
						Shader.SetGlobalTexture(cullingTextureController.DepthKey.Value, (Texture)(object)value2);
					}
				}
			}
			foreach (RenderTextureHolder value5 in _declaredTextures.Values)
			{
				CreateScreenTextureData data = value5.Data;
				if (value5.Textures.TryGetValue(stereoActiveEye, out RenderTexture value3))
				{
					Shader.SetGlobalTexture(data.PropertyId, (Texture)(object)value3);
				}
			}
		}

		private void CreateDeclaredTextures(RenderTextureDescriptor descriptor)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Expected O, but got Unknown
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			MonoOrStereoscopicEye stereoActiveEye = base.Camera.stereoActiveEye;
			string text = default(string);
			RenderTextureHolder renderTextureHolder = default(RenderTextureHolder);
			foreach (KeyValuePair<string, RenderTextureHolder> declaredTexture in _declaredTextures)
			{
				Utils.Deconstruct<string, RenderTextureHolder>(declaredTexture, ref text, ref renderTextureHolder);
				string text2 = text;
				RenderTextureHolder renderTextureHolder2 = renderTextureHolder;
				if (!renderTextureHolder2.Textures.ContainsKey(stereoActiveEye))
				{
					CreateScreenTextureData data = renderTextureHolder2.Data;
					RenderTextureDescriptor val = descriptor;
					((RenderTextureDescriptor)(ref val)).width = (int)((float)(data.Width ?? ((RenderTextureDescriptor)(ref descriptor)).width) / data.XRatio);
					((RenderTextureDescriptor)(ref val)).height = (int)((float)(data.Height ?? ((RenderTextureDescriptor)(ref descriptor)).height) / data.YRatio);
					if (data.Format.HasValue)
					{
						RenderTextureFormat value = data.Format.Value;
						((RenderTextureDescriptor)(ref val)).colorFormat = value;
					}
					RenderTexture val2 = new RenderTexture(val);
					if (data.FilterMode.HasValue)
					{
						((Texture)val2).filterMode = data.FilterMode.Value;
					}
					renderTextureHolder2.Textures[stereoActiveEye] = val2;
					_log.Debug($"Created texture for [{((Object)((Component)this).gameObject).name}] [{stereoActiveEye}]: {text2}, {((Texture)val2).width} : {((Texture)val2).height} : {((Texture)val2).filterMode} : {val2.format}");
				}
			}
		}

		private void RenderImage(RenderTexture src, RenderTexture dst, List<MaterialData> materials)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_033b: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_037d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0239: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			RenderTextureDescriptor descriptor = src.descriptor;
			MonoOrStereoscopicEye stereoActiveEye = base.Camera.stereoActiveEye;
			if (materials.Count == 0)
			{
				Graphics.Blit((Texture)(object)src, dst);
				return;
			}
			RenderTexture val = RenderTexture.GetTemporary(descriptor);
			Graphics.Blit((Texture)(object)src, val);
			for (int num = materials.Count - 1; num >= 0; num--)
			{
				MaterialData materialData = materials[num];
				if (materialData.Frame.HasValue && materialData.Frame != Time.frameCount)
				{
					materials.RemoveAt(num);
				}
				else
				{
					Material material = materialData.Material;
					RenderTextureHolder value3;
					CullingCameraController value7;
					if (materialData.Source == "_Main")
					{
						string[] targets = materialData.Targets;
						foreach (string text in targets)
						{
							RenderTextureHolder value;
							RenderTexture value2;
							if (text == "_Main")
							{
								if (!((Object)(object)material == (Object)null))
								{
									RenderTexture temporary = RenderTexture.GetTemporary(descriptor);
									Graphics.Blit((Texture)(object)val, temporary, material, materialData.Pass);
									RenderTexture.ReleaseTemporary(val);
									val = temporary;
								}
							}
							else if (_declaredTextures.TryGetValue(text, out value) && value.Textures.TryGetValue(stereoActiveEye, out value2))
							{
								Blit(val, value2, material, materialData.Pass);
							}
							else
							{
								_log.Warn("[" + ((Object)((Component)this).gameObject).name + "] Unable to find destination [" + text + "]");
							}
						}
					}
					else if (_declaredTextures.TryGetValue(materialData.Source, out value3))
					{
						string[] targets = materialData.Targets;
						foreach (string text2 in targets)
						{
							value3.Textures.TryGetValue(stereoActiveEye, out RenderTexture value4);
							RenderTextureHolder value5;
							if (text2 == "_Main")
							{
								Blit(value4, val, material, materialData.Pass);
							}
							else if (_declaredTextures.TryGetValue(text2, out value5))
							{
								RenderTexture value6;
								if (value3 == value5)
								{
									if ((Object)(object)material == (Object)null)
									{
										_log.Warn("[" + text2 + "] Attempting to blit to self without material");
										continue;
									}
									RenderTexture temporary2 = RenderTexture.GetTemporary(value4.descriptor);
									((Texture)temporary2).filterMode = ((Texture)value4).filterMode;
									Graphics.Blit((Texture)(object)value4, temporary2, material, materialData.Pass);
									Graphics.Blit((Texture)(object)temporary2, value4);
									RenderTexture.ReleaseTemporary(temporary2);
								}
								else if (value5.Textures.TryGetValue(stereoActiveEye, out value6))
								{
									Blit(value4, value6, material, materialData.Pass);
								}
							}
							else
							{
								_log.Warn("[" + ((Object)((Component)this).gameObject).name + "] Unable to find destination [" + text2 + "]");
							}
						}
					}
					else if (_cullingCameraControllers.TryGetValue(materialData.Source, out value7) && value7 is CullingTextureController cullingTextureController)
					{
						string[] targets = materialData.Targets;
						foreach (string text3 in targets)
						{
							cullingTextureController.RenderTextures.TryGetValue(stereoActiveEye, out RenderTexture value8);
							if (text3 == "_Main")
							{
								Blit(value8, val, material, materialData.Pass);
								continue;
							}
							if (_declaredTextures.TryGetValue(text3, out RenderTextureHolder value9) && value9.Textures.TryGetValue(stereoActiveEye, out RenderTexture value10))
							{
								Blit(value8, value10, material, materialData.Pass);
								continue;
							}
							_log.Warn("[" + ((Object)((Component)this).gameObject).name + "] Unable to find destination [" + text3 + "]");
						}
					}
					else
					{
						_log.Warn("[" + ((Object)((Component)this).gameObject).name + "] Unable to find source [" + materialData.Source + "]");
					}
				}
			}
			Graphics.Blit((Texture)(object)val, dst);
			RenderTexture.ReleaseTemporary(val);
			static void Blit(RenderTexture? blitSrc, RenderTexture? blitDst, Material? blitMat, int blitPass)
			{
				if (!((Object)(object)blitDst == (Object)null) && !((Object)(object)blitSrc == (Object)null))
				{
					if ((Object)(object)blitMat != (Object)null)
					{
						Graphics.Blit((Texture)(object)blitSrc, blitDst, blitMat, blitPass);
					}
					else
					{
						Graphics.Blit((Texture)(object)blitSrc, blitDst);
					}
				}
			}
		}

		private CullingTextureController CreateCamera()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			GameObject val = new GameObject("VivifyCamera");
			val.transform.SetParent(((Component)this).transform, false);
			val.AddComponent<Camera>();
			CopyComponent<BloomPrePass, LateBloomPrePass>(((Component)this).gameObject.GetComponent<BloomPrePass>(), val);
			return _instantiator.InstantiateComponent<CullingTextureController>(val, (IEnumerable<object>)new global::_003C_003Ez__ReadOnlySingleElementList<object>(this));
		}

		[UsedImplicitly]
		[Inject]
		private void Construct(SiraLog log, IInstantiator instantiator)
		{
			_log = log;
			_instantiator = instantiator;
		}

		private void Awake()
		{
			_imageEffectController = ((Component)this).GetComponent<ImageEffectController>();
		}

		private void OnDestroy()
		{
			foreach (RenderTexture item in _declaredTextures.Values.SelectMany((RenderTextureHolder n) => n.Textures.Values))
			{
				if ((Object)(object)item != (Object)null)
				{
					item.Release();
				}
			}
			_declaredTextures.Clear();
			foreach (CullingCameraController item2 in _cullingCameraControllers.Values.Concat(_disabledCullingCameraControllers))
			{
				if (item2 is CullingTextureController)
				{
					Object.Destroy((Object)(object)((Component)item2).gameObject);
				}
			}
			_cullingCameraControllers.Clear();
			_disabledCullingCameraControllers.Clear();
		}
	}
	internal readonly record struct MaterialData : IComparable<MaterialData>
	{
		internal int? Frame { get; }

		internal Material? Material { get; }

		internal int Pass { get; }

		internal int Priority { get; }

		internal string Source { get; }

		internal string[] Targets { get; }

		internal MaterialData(Material? material, int priority, string? source, string[]? targets, int? pass, int? frame = null)
		{
			Material = material;
			Priority = priority;
			Source = source ?? "_Main";
			Targets = targets ?? new string[1] { "_Main" };
			Pass = pass ?? (-1);
			Frame = frame;
		}

		public int CompareTo(MaterialData other)
		{
			if (Priority.CompareTo(other.Priority) >= 0)
			{
				return 1;
			}
			return -1;
		}
	}
	internal class RenderTextureHolder
	{
		internal CreateScreenTextureData Data { get; }

		internal Dictionary<MonoOrStereoscopicEye, RenderTexture> Textures { get; } = new Dictionary<MonoOrStereoscopicEye, RenderTexture>();

		internal RenderTextureHolder(CreateScreenTextureData data)
		{
			Data = data;
		}
	}
	internal class CullingTextureController : CullingCameraController
	{
		private static readonly int _arraySliceIndex = Shader.PropertyToID("_ArraySliceIndex");

		private MainEffectRenderer? _mainEffectRenderer;

		private PostProcessingController _postProcessingController;

		private DepthShaderManager _depthShaderManager;

		private CameraPropertyController _cameraPropertyController;

		private bool _ready;

		internal int? Key { get; private set; }

		internal int? DepthKey { get; private set; }

		internal Dictionary<MonoOrStereoscopicEye, RenderTexture> RenderTextures { get; } = new Dictionary<MonoOrStereoscopicEye, RenderTexture>();

		internal Dictionary<MonoOrStereoscopicEye, RenderTexture> RenderTexturesDepth { get; } = new Dictionary<MonoOrStereoscopicEye, RenderTexture>();

		internal void Init(CreateCameraData cameraData)
		{
			_cameraPropertyController.Id = cameraData.Name;
			if (cameraData.Texture != null)
			{
				Key = Shader.PropertyToID(cameraData.Texture);
			}
			if (cameraData.DepthTexture != null)
			{
				DepthKey = Shader.PropertyToID(cameraData.DepthTexture);
			}
			RefreshCamera();
		}

		protected override void OnPreCull()
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			Camera camera = base.Camera;
			Camera camera2 = _postProcessingController.Camera;
			if (!CamEquals(camera, camera2))
			{
				RefreshCamera();
			}
			Transform transform = ((Component)this).transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			base.OnPreCull();
			if ((int)OpenXRSettings.Instance.renderMode != 0)
			{
				camera.cullingMatrix = camera2.projectionMatrix * camera2.worldToCameraMatrix;
				camera.projectionMatrix = camera2.projectionMatrix;
				camera.nonJitteredProjectionMatrix = camera2.nonJitteredProjectionMatrix;
				camera.worldToCameraMatrix = camera2.worldToCameraMatrix;
			}
		}

		private static bool RTEquals(RenderTexture lhs, RenderTexture rhs)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			if (lhs.vrUsage == rhs.vrUsage && ((Texture)lhs).width == ((Texture)rhs).width)
			{
				return ((Texture)lhs).height == ((Texture)rhs).height;
			}
			return false;
		}

		private static bool CamEquals(Camera lhs, Camera rhs)
		{
			if (lhs.stereoEnabled == rhs.stereoEnabled && lhs.cullingMask == rhs.cullingMask && Mathf.Approximately(lhs.fieldOfView, rhs.fieldOfView) && Mathf.Approximately(lhs.nearClipPlane, rhs.nearClipPlane))
			{
				return Mathf.Approximately(lhs.farClipPlane, rhs.farClipPlane);
			}
			return false;
		}

		[UsedImplicitly]
		[Inject]
		private void Construct(IInstantiator instantiator, PostProcessingController postProcessingController, DepthShaderManager depthShaderManager)
		{
			_postProcessingController = postProcessingController;
			_depthShaderManager = depthShaderManager;
			_cameraPropertyController = instantiator.InstantiateComponent<CameraPropertyController>(((Component)this).gameObject);
			((Behaviour)_cameraPropertyController).enabled = false;
			base.Camera.CopyFrom(_postProcessingController.Camera);
		}

		private void RefreshCamera()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Camera camera = _postProcessingController.Camera;
			base.Camera.stereoTargetEye = camera.stereoTargetEye;
			base.Camera.fieldOfView = camera.fieldOfView;
			base.Camera.aspect = camera.aspect;
			base.Camera.depth = camera.depth - 1f;
			base.Camera.nearClipPlane = camera.nearClipPlane;
			base.Camera.farClipPlane = camera.farClipPlane;
			base.Camera.layerCullDistances = camera.layerCullDistances;
			base.Camera.targetTexture = null;
			base.Camera.cullingMask = camera.cullingMask;
		}

		private void OnDestroy()
		{
			CollectionExtensions.Do<RenderTexture>((IEnumerable<RenderTexture>)RenderTextures.Values, (Action<RenderTexture>)delegate(RenderTexture n)
			{
				n.Release();
			});
			RenderTextures.Clear();
			CollectionExtensions.Do<RenderTexture>((IEnumerable<RenderTexture>)RenderTexturesDepth.Values, (Action<RenderTexture>)delegate(RenderTexture n)
			{
				n.Release();
			});
			RenderTexturesDepth.Clear();
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dst)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Invalid comparison between Unknown and I4
			MonoOrStereoscopicEye stereoActiveEye = base.Camera.stereoActiveEye;
			RenderTextureDescriptor descriptor = src.descriptor;
			((RenderTextureDescriptor)(ref descriptor)).msaaSamples = 1;
			if (!_ready)
			{
				GetRenderTexture(RenderTextures, descriptor, depth: false, create: true);
				GetRenderTexture(RenderTexturesDepth, descriptor, depth: true, create: true);
				((Behaviour)_cameraPropertyController).enabled = true;
				_ready = true;
			}
			if (Key.HasValue)
			{
				RenderTexture val = GetRenderTexture(RenderTextures, descriptor, depth: false, create: false);
				if (base.MainEffect)
				{
					(_mainEffectRenderer ?? (_mainEffectRenderer = new MainEffectRenderer(((Component)((Component)this).gameObject.transform.parent).GetComponent<MainEffectController>()))).Render(src, val);
				}
				else
				{
					Graphics.Blit((Texture)(object)src, val);
				}
			}
			if (DepthKey.HasValue)
			{
				RenderTexture val2 = GetRenderTexture(RenderTexturesDepth, descriptor, depth: true, create: false);
				if ((int)((Texture)val2).dimension == 5)
				{
					Material depthArrayMaterial = _depthShaderManager.DepthArrayMaterial;
					if ((Object)(object)depthArrayMaterial == (Object)null)
					{
						return;
					}
					depthArrayMaterial.SetFloat(_arraySliceIndex, 0f);
					Graphics.Blit((Texture)null, val2, depthArrayMaterial, -1, 0);
					depthArrayMaterial.SetFloat(_arraySliceIndex, 1f);
					Graphics.Blit((Texture)null, val2, depthArrayMaterial, -1, 1);
				}
				else
				{
					Material depthMaterial = _depthShaderManager.DepthMaterial;
					if ((Object)(object)depthMaterial == (Object)null)
					{
						return;
					}
					Graphics.Blit((Texture)null, val2, depthMaterial);
				}
			}
			if (!Key.HasValue && !DepthKey.HasValue)
			{
				((Component)this).gameObject.SetActive(false);
			}
			RenderTexture GetRenderTexture(Dictionary<MonoOrStereoscopicEye, RenderTexture> dictionary, RenderTextureDescriptor renderTextureDescriptor, bool depth, bool create)
			{
				//IL_0003: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Expected O, but got Unknown
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				if (!dictionary.TryGetValue(stereoActiveEye, out RenderTexture value) || !RTEquals(value, src))
				{
					if (value != null)
					{
						value.Release();
					}
					if (depth)
					{
						((RenderTextureDescriptor)(ref renderTextureDescriptor)).colorFormat = (RenderTextureFormat)14;
					}
					value = new RenderTexture(renderTextureDescriptor);
					dictionary[stereoActiveEye] = value;
					if (create)
					{
						value.Create();
					}
				}
				return value;
			}
		}
	}
}
namespace Vivify.ObjectPrefab
{
	internal class FollowedSaberTrail : SaberTrail
	{
		private class SimpleOffsetMovementData : IBladeMovementData
		{
			private Vector3 _bottomPos;

			private IBladeMovementData? _followed;

			private Transform? _parent;

			private Vector3 _topPos;

			public float bladeSpeed => 0f;

			public BladeMovementDataElement lastAddedData
			{
				get
				{
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					//IL_000f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0014: Unknown result type (might be due to invalid IL or missing references)
					if (_followed != null)
					{
						return Modify(_followed.lastAddedData);
					}
					return default(BladeMovementDataElement);
				}
			}

			public BladeMovementDataElement prevAddedData
			{
				get
				{
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					//IL_000f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0014: Unknown result type (might be due to invalid IL or missing references)
					if (_followed != null)
					{
						return Modify(_followed.prevAddedData);
					}
					return default(BladeMovementDataElement);
				}
			}

			internal void Init(IBladeMovementData followed, Transform parent)
			{
				_followed = followed;
				_parent = parent;
			}

			internal void InitProperties(Vector3 topPos, Vector3 bottomPos)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				_topPos = topPos;
				_bottomPos = bottomPos;
			}

			private BladeMovementDataElement Modify(BladeMovementDataElement original)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0057: Unknown result type (might be due to invalid IL or missing references)
				//IL_005c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_006b: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)_parent == (Object)null)
				{
					return original;
				}
				return new BladeMovementDataElement
				{
					time = original.time,
					topPos = original.bottomPos + _parent.TransformVector(_topPos),
					bottomPos = original.bottomPos + _parent.TransformVector(_bottomPos)
				};
			}
		}

		private static readonly int _colorId = Shader.PropertyToID("_Color");

		private readonly SimpleOffsetMovementData _simpleOffsetMovementData = new SimpleOffsetMovementData();

		private SaberTrail? _followed;

		private MaterialPropertyBlock? _materialPropertyBlock;

		internal Material Material { get; set; }

		internal void Init(SaberTrail followed, Transform parent)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)base._trailRenderer == (Object)null)
			{
				base._trailRenderer = Object.Instantiate<SaberTrailRenderer>(followed._trailRendererPrefab, Vector3.zero, Quaternion.identity);
				((Component)base._trailRenderer).transform.SetParent(((Component)followed._trailRenderer).transform.parent);
				((Renderer)base._trailRenderer._meshRenderer).material = Material;
			}
			_followed = followed;
			if (base._movementData == null)
			{
				base._movementData = (IBladeMovementData)(object)_simpleOffsetMovementData;
			}
			_simpleOffsetMovementData.Init(followed._movementData, parent);
			((SaberTrail)this).Init();
		}

		internal void InitProperties(TrailProperties trailProperties)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			_simpleOffsetMovementData.InitProperties((Vector3)(((_003F?)trailProperties.TopPos) ?? Vector3.forward), (Vector3)(((_003F?)trailProperties.BottomPos) ?? Vector3.zero));
			base._trailDuration = trailProperties.Duration ?? 0.4f;
			base._samplingFrequency = trailProperties.SamplingFrequency ?? 50;
			base._granularity = trailProperties.Granularity ?? 60;
		}

		private void Awake()
		{
		}

		private void Update()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			if (!((Object)(object)_followed == (Object)null) && !(base._color == _followed._color))
			{
				base._color = _followed._color;
				if (_materialPropertyBlock == null)
				{
					_materialPropertyBlock = new MaterialPropertyBlock();
				}
				_materialPropertyBlock.SetColor(_colorId, base._color);
				((Renderer)base._trailRenderer._meshRenderer).SetPropertyBlock(_materialPropertyBlock);
			}
		}
	}
	[HeckPatch(PatchType.Features)]
	internal class SetSaberManyColor : SetSaberGlowColor
	{
		private static readonly int _color = Shader.PropertyToID("_Color");

		private Renderer?[] _renderers;

		private static bool SetAllRendererColors(SetSaberGlowColor setSaberGlowColor, Color color)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0023: Expected O, but got Unknown
			if (!(setSaberGlowColor is SetSaberManyColor setSaberManyColor))
			{
				return true;
			}
			MaterialPropertyBlock val = setSaberGlowColor._materialPropertyBlock;
			if (val == null)
			{
				MaterialPropertyBlock val2 = new MaterialPropertyBlock();
				val = val2;
				setSaberGlowColor._materialPropertyBlock = val2;
			}
			val.SetColor(_color, color);
			Renderer[] renderers = setSaberManyColor._renderers;
			foreach (Renderer obj in renderers)
			{
				if (obj != null)
				{
					obj.SetPropertyBlock(val);
				}
			}
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SetSaberGlowColor), "SetColors")]
		private static bool SetSaberGlowColorOverride(SetSaberGlowColor __instance)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			Color color = __instance._colorManager.ColorForSaberType(__instance._saberType);
			return SetAllRendererColors(__instance, color);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SiraUtil.Extras.SaberExtensions), "SetColors", new Type[]
		{
			typeof(SetSaberGlowColor),
			typeof(Color)
		})]
		private static bool SiraSetColorsOverride(SetSaberGlowColor setSaberGlowColor, Color color)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return SetAllRendererColors(setSaberGlowColor, color);
		}

		private void Awake()
		{
			_renderers = ((Component)this).GetComponentsInChildren<Renderer>();
		}
	}
}
namespace Vivify.ObjectPrefab.Pools
{
	internal interface IPrefabPool : IDisposable
	{
		void Despawn(Component component);
	}
	internal interface IPrefabPool<out T> : IPrefabPool, IDisposable
	{
		T Spawn(Component component, float startTime);
	}
	internal class PrefabPool : IPrefabPool<GameObject>, IPrefabPool, IDisposable
	{
		private readonly Dictionary<Component, GameObject> _active = new Dictionary<Component, GameObject>();

		private readonly Stack<GameObject> _inactive = new Stack<GameObject>();

		private readonly IInstantiator _instantiator;

		private readonly GameObject _original;

		internal PrefabPool(GameObject original, IInstantiator instantiator)
		{
			_original = original;
			_instantiator = instantiator;
		}

		public void Despawn(Component component)
		{
			if (_active.TryGetValue(component, out GameObject value))
			{
				value.SetActive(false);
				_inactive.Push(value);
				_active.Remove(component);
				value.transform.SetParent((Transform)null, false);
			}
		}

		public void Dispose()
		{
			CollectionExtensions.Do<GameObject>((IEnumerable<GameObject>)_inactive, (Action<GameObject>)Object.Destroy);
			CollectionExtensions.Do<GameObject>((IEnumerable<GameObject>)_active.Values, (Action<GameObject>)Object.Destroy);
		}

		public GameObject Spawn(Component component, float startTime)
		{
			if (_active.TryGetValue(component, out GameObject value))
			{
				return value;
			}
			if (_inactive.Count == 0)
			{
				value = _instantiator.InstantiatePrefab((Object)(object)_original);
			}
			else
			{
				value = _inactive.Pop();
				value.SetActive(true);
			}
			Animator[] components = value.GetComponents<Animator>();
			foreach (Animator obj in components)
			{
				obj.Rebind();
				obj.Update(0.01f);
			}
			_active.Add(component, value);
			_instantiator.SongSynchronize(value, startTime);
			return value;
		}

		internal void Prewarm(int count)
		{
			for (int i = 0; i < count; i++)
			{
				GameObject val = _instantiator.InstantiatePrefab((Object)(object)_original);
				val.SetActive(false);
				_inactive.Push(val);
				_instantiator.SongSynchronize(val, 0f);
			}
		}
	}
	internal class TrailPool : IPrefabPool<FollowedSaberTrail>, IPrefabPool, IDisposable
	{
		private readonly Dictionary<Component, FollowedSaberTrail> _active = new Dictionary<Component, FollowedSaberTrail>();

		private readonly Stack<FollowedSaberTrail> _inactive = new Stack<FollowedSaberTrail>();

		private readonly Material _material;

		private readonly TrailProperties _trailProperties;

		internal TrailPool(Material material, TrailProperties trailProperties)
		{
			_material = material;
			_trailProperties = trailProperties;
		}

		public void Despawn(Component component)
		{
			if (_active.TryGetValue(component, out FollowedSaberTrail value))
			{
				((Component)value).gameObject.SetActive(false);
				_inactive.Push(value);
				_active.Remove(component);
				((Component)value).transform.SetParent((Transform)null, false);
			}
		}

		public void Dispose()
		{
			CollectionExtensions.Do<FollowedSaberTrail>((IEnumerable<FollowedSaberTrail>)_inactive, (Action<FollowedSaberTrail>)Object.Destroy);
			CollectionExtensions.Do<FollowedSaberTrail>((IEnumerable<FollowedSaberTrail>)_active.Values, (Action<FollowedSaberTrail>)Object.Destroy);
		}

		public FollowedSaberTrail Spawn(Component component, float _)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (_active.TryGetValue(component, out FollowedSaberTrail value))
			{
				return value;
			}
			if (_inactive.Count == 0)
			{
				value = new GameObject("FollowedSaberTrail").AddComponent<FollowedSaberTrail>();
				value.Material = _material;
			}
			else
			{
				value = _inactive.Pop();
				((Component)value).gameObject.SetActive(true);
			}
			value.InitProperties(_trailProperties);
			_active.Add(component, value);
			return value;
		}
	}
	internal readonly struct TrailProperties : IEquatable<TrailProperties>
	{
		internal Vector3? TopPos { get; }

		internal Vector3? BottomPos { get; }

		internal float? Duration { get; }

		internal int? SamplingFrequency { get; }

		internal int? Granularity { get; }

		internal TrailProperties(Vector3? topPos, Vector3? bottomPos, float? duration, int? samplingFrequency, int? granularity)
		{
			TopPos = topPos;
			BottomPos = bottomPos;
			Duration = duration;
			SamplingFrequency = samplingFrequency;
			Granularity = granularity;
		}

		public static bool operator ==(TrailProperties lhs, TrailProperties rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(TrailProperties lhs, TrailProperties rhs)
		{
			return !lhs.Equals(rhs);
		}

		public override bool Equals(object? obj)
		{
			if (obj is TrailProperties other)
			{
				return Equals(other);
			}
			return false;
		}

		public bool Equals(TrailProperties other)
		{
			if (Nullable.Equals<Vector3>(TopPos, other.TopPos) && Nullable.Equals<Vector3>(BottomPos, other.BottomPos) && Nullable.Equals(Duration, other.Duration) && SamplingFrequency == other.SamplingFrequency)
			{
				return Granularity == other.Granularity;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((TopPos.GetHashCode() * 397) ^ BottomPos.GetHashCode()) * 397) ^ Duration.GetHashCode()) * 397) ^ SamplingFrequency.GetHashCode()) * 397) ^ Granularity.GetHashCode();
		}
	}
}
namespace Vivify.ObjectPrefab.Managers
{
	[UsedImplicitly(/*Could not decode attribute arguments.*/)]
	internal enum LoadMode
	{
		Single,
		Additive
	}
	internal class BeatmapObjectPrefabManager : IDisposable
	{
		private readonly AssetBundleManager _assetBundleManager;

		private readonly IInstantiator _instantiator;

		private readonly SiraLog _log;

		private readonly Dictionary<string, IPrefabPool> _prefabPools = new Dictionary<string, IPrefabPool>();

		private readonly ReLoader? _reLoader;

		private readonly Dictionary<(string, TrailProperties), TrailPool> _trailPools = new Dictionary<(string, TrailProperties), TrailPool>();

		internal Dictionary<Component, HashSet<IPrefabPool?>> ActivePools { get; } = new Dictionary<Component, HashSet<IPrefabPool>>();

		internal Dictionary<Component, IHijacker> Hijackers { get; } = new Dictionary<Component, IHijacker>();

		[UsedImplicitly]
		private BeatmapObjectPrefabManager(SiraLog log, AssetBundleManager assetBundleManager, IInstantiator instantiator, [InjectOptional] ReLoader? reLoader)
		{
			_log = log;
			_assetBundleManager = assetBundleManager;
			_instantiator = instantiator;
			_reLoader = reLoader;
			if (reLoader != null)
			{
				reLoader.Rewinded += OnRewind;
			}
		}

		public void Dispose()
		{
			if (_reLoader != null)
			{
				_reLoader.Rewinded -= OnRewind;
			}
			CollectionExtensions.Do<IPrefabPool>((IEnumerable<IPrefabPool>)_prefabPools.Values, (Action<IPrefabPool>)delegate(IPrefabPool n)
			{
				n.Dispose();
			});
		}

		internal void PrewarmGameObjectPrefabPool(string assetName, int count)
		{
			GetGameObjectPrefabPool(assetName)?.Prewarm(count);
		}

		internal void AssignGameObjectPrefab(PrefabList prefabList, string? assetName, LoadMode loadMode, float time)
		{
			PrefabPool gameObjectPrefabPool = GetGameObjectPrefabPool(assetName);
			if (!prefabList.AddPool(gameObjectPrefabPool, loadMode, time))
			{
				_log.Error("Could not assign [" + assetName + "], already assigned");
			}
		}

		internal void AssignTrackPrefab(PrefabDictionary prefabDictionary, IReadOnlyList<Track> tracks, string? assetName, LoadMode loadMode)
		{
			PrefabPool gameObjectPrefabPool = GetGameObjectPrefabPool(assetName);
			foreach (Track track in tracks)
			{
				if (!prefabDictionary.AddPrefabPool(track, gameObjectPrefabPool, loadMode))
				{
					_log.Error("Could not assign [" + assetName + "], is already on track");
				}
			}
		}

		internal void AssignTrail(TrailList trailList, string? assetName, TrailProperties trailProperties, LoadMode loadMode, float time)
		{
			TrailPool trailPool = GetTrailPool(assetName, trailProperties);
			if (!trailList.AddPool(trailPool, loadMode, time))
			{
				_log.Error("Could not assign [" + assetName + "], already assigned");
			}
		}

		internal void Despawn(Component component)
		{
			if (!ActivePools.TryGetValue(component, out HashSet<IPrefabPool> value))
			{
				return;
			}
			if (Hijackers.TryGetValue(component, out IHijacker value2))
			{
				value2.Deactivate();
			}
			foreach (IPrefabPool item in value)
			{
				item?.Despawn(component);
			}
			ActivePools.Remove(component);
		}

		internal void Spawn(IEnumerable<Track> tracks, PrefabDictionary prefabDictionary, Component component, float startTime)
		{
			IHijacker hijacker = null;
			HashSet<IPrefabPool> activePool = null;
			List<GameObject> spawned = null;
			bool hideOriginal = false;
			foreach (Track track in tracks)
			{
				if (prefabDictionary.TryGetValue(track, out HashSet<PrefabPool> value))
				{
					Spawn(value, component, startTime, ref hijacker, ref activePool, ref spawned, ref hideOriginal);
				}
			}
			if (spawned != null)
			{
				((IHijacker<GameObject>)hijacker)?.Activate(spawned, hideOriginal);
			}
		}

		internal void Spawn<TPool, TSpawned>(PrefabList<TPool> prefabList, Component component, float startTime) where TPool : class, IPrefabPool<TSpawned>
		{
			IHijacker hijacker = null;
			HashSet<IPrefabPool> activePool = null;
			List<TSpawned> spawned = null;
			bool hideOriginal = false;
			Spawn(prefabList.HashSet, component, startTime, ref hijacker, ref activePool, ref spawned, ref hideOriginal);
			if (spawned != null)
			{
				((IHijacker<TSpawned>)hijacker)?.Activate(spawned, hideOriginal);
			}
		}

		private PrefabPool? GetGameObjectPrefabPool(string? assetName)
		{
			if (assetName == null)
			{
				return null;
			}
			if (!_prefabPools.TryGetValue(assetName, out IPrefabPool value))
			{
				if (!_assetBundleManager.TryGetAsset<GameObject>(assetName, out GameObject asset))
				{
					return null;
				}
				value = (_prefabPools[assetName] = new PrefabPool(asset, _instantiator));
			}
			return (PrefabPool)value;
		}

		private TrailPool? GetTrailPool(string? assetName, TrailProperties trailProperties)
		{
			if (assetName == null)
			{
				return null;
			}
			if (!_trailPools.TryGetValue((assetName, trailProperties), out TrailPool value))
			{
				if (!_assetBundleManager.TryGetAsset<Material>(assetName, out Material asset))
				{
					return null;
				}
				value = (_trailPools[(assetName, trailProperties)] = new TrailPool(asset, trailProperties));
			}
			return value;
		}

		private void OnRewind()
		{
			Component[] array = ActivePools.Keys.ToArray();
			foreach (Component component in array)
			{
				Despawn(component);
			}
		}

		private void Spawn<TPool, TSpawned>(HashSet<TPool?> prefabPools, Component component, float startTime, ref IHijacker? hijacker, ref HashSet<IPrefabPool?>? activePool, ref List<TSpawned>? spawned, ref bool hideOriginal) where TPool : IPrefabPool<TSpawned>
		{
			if (prefabPools.Count == 0)
			{
				return;
			}
			if (hijacker == null && !Hijackers.TryGetValue(component, out hijacker))
			{
				Dictionary<Component, IHijacker> hijackers = Hijackers;
				SaberTrail val = (SaberTrail)(object)((component is SaberTrail) ? component : null);
				IHijacker hijacker2;
				if (val == null)
				{
					SaberModelController val2 = (SaberModelController)(object)((component is SaberModelController) ? component : null);
					hijacker2 = ((val2 == null) ? ((IHijacker<GameObject>)_instantiator.Instantiate<MpbControllerHijacker>((IEnumerable<object>)new global::_003C_003Ez__ReadOnlySingleElementList<object>(component))) : ((IHijacker<GameObject>)_instantiator.Instantiate<SaberModelControllerHijacker>((IEnumerable<object>)new global::_003C_003Ez__ReadOnlySingleElementList<object>(val2))));
				}
				else
				{
					hijacker2 = _instantiator.Instantiate<SaberTrailHijacker>((IEnumerable<object>)new global::_003C_003Ez__ReadOnlySingleElementList<object>(val));
				}
				hijackers[component] = (hijacker = hijacker2);
			}
			if (activePool == null && !ActivePools.TryGetValue(component, out activePool))
			{
				ActivePools[component] = (activePool = new HashSet<IPrefabPool>());
			}
			if (spawned == null)
			{
				spawned = new List<TSpawned>(prefabPools.Count);
			}
			bool flag = false;
			foreach (TPool prefabPool in prefabPools)
			{
				if (prefabPool == null)
				{
					flag = true;
					continue;
				}
				spawned.Add(prefabPool.Spawn(component, startTime));
				activePool.Add(prefabPool);
			}
			if (!flag)
			{
				hideOriginal = true;
			}
		}
	}
	internal class DebrisPrefabManager : IAffinity, IDisposable
	{
		private readonly AudioTimeSyncController _audioTimeSyncController;

		private readonly BeatmapObjectPrefabManager _beatmapObjectPrefabManager;

		private readonly DeserializedData _deserializedData;

		private readonly ReLoader? _reLoader;

		private NoteData? _noteData;

		internal PrefabDictionary BurstSliderDebrisPrefabs { get; } = new PrefabDictionary();

		internal PrefabDictionary BurstSliderElementDebrisPrefabs { get; } = new PrefabDictionary();

		internal PrefabDictionary ColorNoteDebrisPrefabs { get; } = new PrefabDictionary();

		[UsedImplicitly]
		private DebrisPrefabManager(BeatmapObjectPrefabManager beatmapObjectPrefabManager, AudioTimeSyncController audioTimeSyncController, [Inject(Id = "Vivify")] DeserializedData deserializedData, [InjectOptional] ReLoader? reLoader)
		{
			_beatmapObjectPrefabManager = beatmapObjectPrefabManager;
			_audioTimeSyncController = audioTimeSyncController;
			_deserializedData = deserializedData;
			_reLoader = reLoader;
			if (reLoader != null)
			{
				reLoader.Rewinded += OnRewind;
			}
		}

		public void Dispose()
		{
			if (_reLoader != null)
			{
				_reLoader.Rewinded -= OnRewind;
			}
		}

		[AffinityPostfix]
		[AffinityPatch(typeof(NoteDebrisSpawner), "HandleNoteDebrisDidFinish", AffinityMethodType.Normal, null, new Type[] { })]
		private void DespawnPrefab(NoteDebris noteDebris)
		{
			_beatmapObjectPrefabManager.Despawn((Component)(object)noteDebris);
		}

		private void OnRewind()
		{
			ColorNoteDebrisPrefabs.Clear();
			BurstSliderDebrisPrefabs.Clear();
			BurstSliderElementDebrisPrefabs.Clear();
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(NoteCutCoreEffectsSpawner), "SpawnNoteCutEffect", AffinityMethodType.Normal, null, new Type[] { })]
		private void SetNoteData(NoteController noteController)
		{
			_noteData = ((NoteControllerBase)noteController).noteData;
		}

		[AffinityPostfix]
		[AffinityPatch(typeof(NoteDebris), "Init", AffinityMethodType.Normal, null, new Type[] { })]
		private void SpawnPrefab(NoteDebris __instance)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected I4, but got Unknown
			if (_noteData != null && _deserializedData.Resolve<VivifyObjectData>((BeatmapObjectData)(object)_noteData, out VivifyObjectData result) && result.Track != null)
			{
				GameplayType gameplayType = _noteData.gameplayType;
				PrefabDictionary prefabDictionary = (int)gameplayType switch
				{
					0 => ColorNoteDebrisPrefabs, 
					2 => BurstSliderDebrisPrefabs, 
					3 => BurstSliderElementDebrisPrefabs, 
					_ => null, 
				};
				if (prefabDictionary != null)
				{
					_beatmapObjectPrefabManager.Spawn(result.Track, prefabDictionary, (Component)(object)__instance, _audioTimeSyncController.songTime);
				}
			}
		}
	}
	internal class NotePrefabManager : IDisposable
	{
		private readonly BasicBeatmapObjectManager? _basicBeatmapObjectManager;

		private readonly BeatmapObjectManager _beatmapObjectManager;

		private readonly DeserializedData _deserializedData;

		private readonly BeatmapObjectPrefabManager _prefabManager;

		private readonly ReLoader? _reLoader;

		internal PrefabDictionary AnyDirectionNotePrefabs { get; } = new PrefabDictionary();

		internal PrefabDictionary BombNotePrefabs { get; } = new PrefabDictionary();

		internal PrefabDictionary BurstSliderElementPrefabs { get; } = new PrefabDictionary();

		internal PrefabDictionary BurstSliderPrefabs { get; } = new PrefabDictionary();

		internal PrefabDictionary ColorNotePrefabs { get; } = new PrefabDictionary();

		[UsedImplicitly]
		private NotePrefabManager(BeatmapObjectPrefabManager prefabManager, BeatmapObjectManager beatmapObjectManager, [Inject(Id = "Vivify")] DeserializedData deserializedData, [InjectOptional] ReLoader? reLoader)
		{
			_prefabManager = prefabManager;
			_beatmapObjectManager = beatmapObjectManager;
			_basicBeatmapObjectManager = (BasicBeatmapObjectManager?)(object)((beatmapObjectManager is BasicBeatmapObjectManager) ? beatmapObjectManager : null);
			_deserializedData = deserializedData;
			_reLoader = reLoader;
			if (reLoader != null)
			{
				reLoader.Rewinded += OnRewind;
			}
			if (_basicBeatmapObjectManager != null)
			{
				AnyDirectionNotePrefabs.Changed += OnAnyDirectionNotePrefabsChanges;
				BombNotePrefabs.Changed += OnBombNotePrefabsChanged;
				BurstSliderElementPrefabs.Changed += OnBurstSliderElementPrefabsChanged;
				BurstSliderPrefabs.Changed += OnBurstSliderPrefabsChanged;
				ColorNotePrefabs.Changed += OnColorNotePrefabsChanges;
			}
			beatmapObjectManager.noteWasSpawnedEvent += HandleNoteWasSpawned;
			beatmapObjectManager.noteWasDespawnedEvent += HandleNoteWasDespawned;
		}

		public void Dispose()
		{
			if (_reLoader != null)
			{
				_reLoader.Rewinded -= OnRewind;
			}
			AnyDirectionNotePrefabs.Changed -= OnAnyDirectionNotePrefabsChanges;
			BombNotePrefabs.Changed -= OnBombNotePrefabsChanged;
			BurstSliderElementPrefabs.Changed -= OnBurstSliderElementPrefabsChanged;
			BurstSliderPrefabs.Changed -= OnBurstSliderPrefabsChanged;
			ColorNotePrefabs.Changed -= OnColorNotePrefabsChanges;
			_beatmapObjectManager.noteWasSpawnedEvent -= HandleNoteWasSpawned;
			_beatmapObjectManager.noteWasDespawnedEvent -= HandleNoteWasDespawned;
		}

		private void HandleNoteWasDespawned(NoteController noteController)
		{
			_prefabManager.Despawn((Component)(object)noteController);
		}

		private void HandleNoteWasSpawned(NoteController noteController)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected I4, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Invalid comparison between Unknown and I4
			NoteData noteData = ((NoteControllerBase)noteController).noteData;
			if (_deserializedData.Resolve<VivifyObjectData>((BeatmapObjectData)(object)noteData, out VivifyObjectData result) && result.Track != null)
			{
				GameplayType gameplayType = noteData.gameplayType;
				PrefabDictionary prefabDictionary = (int)gameplayType switch
				{
					0 => ((int)noteData.cutDirection == 8) ? AnyDirectionNotePrefabs : ColorNotePrefabs, 
					1 => BombNotePrefabs, 
					2 => BurstSliderPrefabs, 
					3 => BurstSliderElementPrefabs, 
					_ => null, 
				};
				if (prefabDictionary != null)
				{
					_prefabManager.Spawn(result.Track, prefabDictionary, (Component)(object)noteController, noteController._noteMovement._floorMovement._beatTime);
				}
			}
		}

		private void OnAnyDirectionNotePrefabsChanges(Track track)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Invalid comparison between Unknown and I4
			foreach (GameNoteController activeItem in _basicBeatmapObjectManager._basicGameNotePoolContainer.activeItems)
			{
				if ((int)((NoteControllerBase)activeItem).noteData.cutDirection == 8)
				{
					RefreshObjects(track, (NoteController)(object)activeItem, AnyDirectionNotePrefabs);
				}
			}
		}

		private void OnBombNotePrefabsChanged(Track track)
		{
			foreach (BombNoteController activeItem in _basicBeatmapObjectManager._bombNotePoolContainer.activeItems)
			{
				RefreshObjects(track, (NoteController)(object)activeItem, BombNotePrefabs);
			}
		}

		private void OnBurstSliderElementPrefabsChanged(Track track)
		{
			foreach (BurstSliderGameNoteController activeItem in _basicBeatmapObjectManager._burstSliderGameNotePoolContainer.activeItems)
			{
				RefreshObjects(track, (NoteController)(object)activeItem, BurstSliderElementPrefabs);
			}
		}

		private void OnBurstSliderPrefabsChanged(Track track)
		{
			foreach (GameNoteController activeItem in _basicBeatmapObjectManager._burstSliderHeadGameNotePoolContainer.activeItems)
			{
				RefreshObjects(track, (NoteController)(object)activeItem, BurstSliderPrefabs);
			}
		}

		private void OnColorNotePrefabsChanges(Track track)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Invalid comparison between Unknown and I4
			foreach (GameNoteController activeItem in _basicBeatmapObjectManager._basicGameNotePoolContainer.activeItems)
			{
				if ((int)((NoteControllerBase)activeItem).noteData.cutDirection != 8)
				{
					RefreshObjects(track, (NoteController)(object)activeItem, ColorNotePrefabs);
				}
			}
		}

		private void OnRewind()
		{
			ColorNotePrefabs.Clear();
			BombNotePrefabs.Clear();
			BurstSliderPrefabs.Clear();
			BurstSliderElementPrefabs.Clear();
		}

		private void RefreshObjects(Track track, NoteController noteController, PrefabDictionary prefabDictionary)
		{
			if (_deserializedData.Resolve<VivifyObjectData>((BeatmapObjectData)(object)((NoteControllerBase)noteController).noteData, out VivifyObjectData result) && result.Track != null && result.Track.Contains<Track>(track))
			{
				_prefabManager.Despawn((Component)(object)noteController);
				_prefabManager.Spawn(result.Track, prefabDictionary, (Component)(object)noteController, noteController._noteMovement._floorMovement._beatTime);
			}
		}
	}
	internal class SaberPrefabManager : IDisposable
	{
		private readonly BeatmapObjectPrefabManager _beatmapObjectPrefabManager;

		private readonly ReLoader? _reLoader;

		private readonly Saber _saberA;

		private readonly Saber _saberB;

		private readonly SaberModelManager _saberModelManager;

		private SaberModelController? _saberModelControllerA;

		private SaberModelController? _saberModelControllerB;

		internal PrefabList SaberAPrefabs { get; } = new PrefabList();

		internal TrailList SaberATrailMaterials { get; } = new TrailList();

		internal PrefabList SaberBPrefabs { get; } = new PrefabList();

		internal TrailList SaberBTrailMaterials { get; } = new TrailList();

		private SaberModelController SaberModelControllerA
		{
			get
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				SaberModelController? obj = _saberModelControllerA ?? (_saberModelControllerA = _saberModelManager.GetSaberModelController(_saberA));
				if (obj == null)
				{
					throw new InvalidOperationException($"Could not find SaberModelController for [{_saberA.saberType}]");
				}
				return obj;
			}
		}

		private SaberModelController SaberModelControllerB
		{
			get
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				SaberModelController? obj = _saberModelControllerB ?? (_saberModelControllerB = _saberModelManager.GetSaberModelController(_saberB));
				if (obj == null)
				{
					throw new InvalidOperationException($"Could not find SaberModelController for [{_saberB.saberType}]");
				}
				return obj;
			}
		}

		[UsedImplicitly]
		internal SaberPrefabManager(BeatmapObjectPrefabManager beatmapObjectPrefabManager, SaberManager saberManager, SaberModelManager saberModelManager, [InjectOptional] ReLoader? reLoader)
		{
			_beatmapObjectPrefabManager = beatmapObjectPrefabManager;
			_saberModelManager = saberModelManager;
			_saberA = saberManager.SaberForType((SaberType)0);
			_saberB = saberManager.SaberForType((SaberType)1);
			_reLoader = reLoader;
			if (reLoader != null)
			{
				reLoader.Rewinded += OnRewind;
			}
			SaberAPrefabs.Changed += OnSaberAChanged;
			SaberBPrefabs.Changed += OnSaberBChanged;
			SaberATrailMaterials.Changed += OnSaberATrailChanged;
			SaberBTrailMaterials.Changed += OnSaberBTrailChanged;
		}

		public void Dispose()
		{
			if (_reLoader != null)
			{
				_reLoader.Rewinded -= OnRewind;
			}
			SaberAPrefabs.Changed -= OnSaberAChanged;
			SaberBPrefabs.Changed -= OnSaberBChanged;
			SaberATrailMaterials.Changed -= OnSaberATrailChanged;
			SaberBTrailMaterials.Changed -= OnSaberBTrailChanged;
		}

		private void OnRewind()
		{
			SaberAPrefabs.Clear();
			SaberBPrefabs.Clear();
		}

		private void OnSaberAChanged(float time)
		{
			_beatmapObjectPrefabManager.Despawn((Component)(object)SaberModelControllerA);
			_beatmapObjectPrefabManager.Spawn<PrefabPool, GameObject>(SaberAPrefabs, (Component)(object)SaberModelControllerA, time);
		}

		private void OnSaberATrailChanged(float time)
		{
			_beatmapObjectPrefabManager.Despawn((Component)(object)SaberModelControllerA._saberTrail);
			_beatmapObjectPrefabManager.Spawn<TrailPool, FollowedSaberTrail>(SaberATrailMaterials, (Component)(object)SaberModelControllerA._saberTrail, time);
		}

		private void OnSaberBChanged(float time)
		{
			_beatmapObjectPrefabManager.Despawn((Component)(object)SaberModelControllerB);
			_beatmapObjectPrefabManager.Spawn<PrefabPool, GameObject>(SaberBPrefabs, (Component)(object)SaberModelControllerB, time);
		}

		private void OnSaberBTrailChanged(float time)
		{
			_beatmapObjectPrefabManager.Despawn((Component)(object)SaberModelControllerB._saberTrail);
			_beatmapObjectPrefabManager.Spawn<TrailPool, FollowedSaberTrail>(SaberBTrailMaterials, (Component)(object)SaberModelControllerB._saberTrail, time);
		}
	}
}
namespace Vivify.ObjectPrefab.Hijackers
{
	internal interface IHijacker
	{
		void Deactivate();
	}
	internal interface IHijacker<TSpawned> : IHijacker
	{
		void Activate(List<TSpawned> spawned, bool hideOriginal);
	}
	internal class MpbControllerHijacker : IHijacker<GameObject>, IHijacker
	{
		private readonly Transform _child;

		private readonly MaterialPropertyBlockController _materialPropertyBlockController;

		private readonly Renderer?[] _originalRenderers;

		private Renderer[]? _cachedRenderers;

		[UsedImplicitly]
		internal MpbControllerHijacker(Component component)
		{
			_originalRenderers = component.GetComponentsInChildren<Renderer>();
			_child = component.transform.GetChild(0);
			if ((component is GameNoteController || component is BurstSliderGameNoteController) ? true : false)
			{
				_materialPropertyBlockController = ((Component)component.transform.GetChild(0)).GetComponent<MaterialPropertyBlockController>();
			}
			else
			{
				_materialPropertyBlockController = component.GetComponent<MaterialPropertyBlockController>();
			}
		}

		public void Activate(List<GameObject> gameObjects, bool hideOriginal)
		{
			foreach (GameObject gameObject in gameObjects)
			{
				gameObject.transform.SetParent(_child, false);
			}
			_cachedRenderers = _materialPropertyBlockController._renderers;
			IEnumerable<Renderer> enumerable = gameObjects.SelectMany((GameObject n) => n.GetComponentsInChildren<Renderer>(true));
			if (hideOriginal)
			{
				Renderer[] originalRenderers = _originalRenderers;
				foreach (Renderer val in originalRenderers)
				{
					if ((Object)(object)val != (Object)null)
					{
						val.enabled = false;
					}
				}
				_materialPropertyBlockController._renderers = enumerable.ToArray();
			}
			else
			{
				_materialPropertyBlockController._renderers = _cachedRenderers.Concat<Renderer>(enumerable).ToArray();
			}
			_materialPropertyBlockController.ApplyChanges();
		}

		public void Deactivate()
		{
			if (_cachedRenderers != null)
			{
				_materialPropertyBlockController._renderers = _cachedRenderers;
				_cachedRenderers = null;
			}
			Renderer[] originalRenderers = _originalRenderers;
			foreach (Renderer val in originalRenderers)
			{
				if ((Object)(object)val != (Object)null)
				{
					val.enabled = true;
				}
			}
		}
	}
	internal class SaberModelControllerHijacker : IHijacker<GameObject>, IHijacker
	{
		private class LiteModelContract : MonoBehaviour
		{
			private SaberModelControllerHijacker _hijacker;

			private FieldInfo? _liteSaberInstanceField;

			private SaberModelController? _saberModelController;

			internal void Init(Assembly assembly, SaberModelControllerHijacker hijacker)
			{
				_saberModelController = hijacker._saberModelController;
				Type type = ((object)_saberModelController).GetType();
				if (type.Assembly != assembly)
				{
					Object.Destroy((Object)(object)this);
					return;
				}
				_liteSaberInstanceField = type.GetField("liteSaberInstance", AccessTools.all);
				if (_liteSaberInstanceField == null || GetLiteSaberInstance() != null)
				{
					Object.Destroy((Object)(object)this);
					return;
				}
				hijacker._log.Warn("CustomSabersLite model not yet created, deferring renderer fetching");
				_hijacker = hijacker;
			}

			private object? GetLiteSaberInstance()
			{
				return _liteSaberInstanceField?.GetValue(_saberModelController);
			}

			private void OnTransformChildrenChanged()
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Expected O, but got Unknown
				object liteSaberInstance = GetLiteSaberInstance();
				if (liteSaberInstance == null)
				{
					return;
				}
				Object.Destroy((Object)(object)this);
				GameObject val = (GameObject)(liteSaberInstance.GetType().GetProperty("GameObject", AccessTools.all)?.GetValue(liteSaberInstance));
				if ((Object)(object)val == (Object)null)
				{
					return;
				}
				HashSet<Renderer> originalRenderers = _hijacker._originalRenderers;
				Renderer[] componentsInChildren = val.GetComponentsInChildren<Renderer>();
				originalRenderers.UnionWith(componentsInChildren);
				if (_hijacker._shouldHide)
				{
					Renderer[] array = componentsInChildren;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].enabled = false;
					}
				}
				_hijacker._log.Info("Fetched CustomSabersLite model");
			}
		}

		private readonly IInstantiator _instantiator;

		private readonly HashSet<Renderer> _originalRenderers;

		private readonly Saber _saber;

		private readonly SiraLog _log;

		private readonly SaberModelController _saberModelController;

		private SetSaberGlowColor[]? _cachedSetSaberGlowColors;

		private bool _shouldHide;

		[UsedImplicitly]
		internal SaberModelControllerHijacker(SiraLog log, SaberModelController saberModelController, IInstantiator instantiator)
		{
			_log = log;
			_saberModelController = saberModelController;
			_instantiator = instantiator;
			Transform parent = ((Component)saberModelController).transform.parent;
			_originalRenderers = ((Component)parent).GetComponentsInChildren<Renderer>().ToHashSet();
			_saber = ((Component)parent).GetComponent<Saber>();
			PluginMetadata plugin = PluginManager.GetPlugin("CustomSabersLite");
			Assembly assembly = ((plugin != null) ? plugin.Assembly : null);
			if (!(assembly == null))
			{
				((Component)saberModelController).gameObject.AddComponent<LiteModelContract>().Init(assembly, this);
			}
		}

		public void Activate(List<GameObject> gameObjects, bool hideOriginal)
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			foreach (GameObject gameObject in gameObjects)
			{
				gameObject.transform.SetParent(((Component)_saber).transform, false);
			}
			_cachedSetSaberGlowColors = _saberModelController._setSaberGlowColors;
			SetSaberGlowColor[] array = (SetSaberGlowColor[])(object)new SetSaberGlowColor[gameObjects.Count];
			for (int i = 0; i < gameObjects.Count; i++)
			{
				GameObject val = gameObjects[i];
				SetSaberManyColor setSaberManyColor = val.GetComponent<SetSaberManyColor>();
				if ((Object)(object)setSaberManyColor == (Object)null)
				{
					setSaberManyColor = _instantiator.InstantiateComponent<SetSaberManyColor>(val);
				}
				array[i] = (SetSaberGlowColor)(object)setSaberManyColor;
				((SetSaberGlowColor)setSaberManyColor).saberType = _saber.saberType;
			}
			if (hideOriginal)
			{
				_shouldHide = true;
				foreach (Renderer originalRenderer in _originalRenderers)
				{
					originalRenderer.enabled = false;
				}
				_saberModelController._setSaberGlowColors = array;
			}
			else
			{
				_saberModelController._setSaberGlowColors = _cachedSetSaberGlowColors.Concat<SetSaberGlowColor>(array).ToArray();
			}
		}

		public void Deactivate()
		{
			if (_cachedSetSaberGlowColors != null)
			{
				_saberModelController._setSaberGlowColors = _cachedSetSaberGlowColors;
				_cachedSetSaberGlowColors = null;
			}
			_shouldHide = false;
			foreach (Renderer originalRenderer in _originalRenderers)
			{
				originalRenderer.enabled = true;
			}
		}
	}
	internal class SaberTrailHijacker : IHijacker<FollowedSaberTrail>, IHijacker
	{
		private class LiteTrailContract : MonoBehaviour
		{
			private SaberTrailHijacker _hijacker;

			internal void Init(SaberTrailHijacker hijacker)
			{
				_hijacker = hijacker;
			}

			private void OnTransformChildrenChanged()
			{
				SaberTrail saberTrail = _hijacker._saberTrail;
				SaberTrail[] array = (from n in ((Component)saberTrail).GetComponentsInChildren<SaberTrail>()
					where (Object)(object)n != (Object)(object)saberTrail
					select n).ToArray();
				if (array.Length == 0)
				{
					return;
				}
				HashSet<Renderer> originalRenderers = _hijacker._originalRenderers;
				MeshRenderer[] array2 = array.Select((SaberTrail n) => n._trailRenderer._meshRenderer).ToArray();
				originalRenderers.UnionWith((IEnumerable<Renderer>)(object)array2);
				if (_hijacker._shouldHide)
				{
					MeshRenderer[] array3 = array2;
					for (int num = 0; num < array3.Length; num++)
					{
						((Renderer)array3[num]).enabled = false;
					}
				}
				_hijacker._log.Info("Fetched CustomSabersLite trail");
				Object.Destroy((Object)(object)this);
			}
		}

		private static Type? _sfTrailType;

		private static FieldInfo? _vertexPoolField;

		private static FieldInfo? _vertexPoolMeshRendererField;

		private static Type? _vertexPoolType;

		private readonly SiraLog _log;

		private readonly HashSet<Renderer> _originalRenderers = new HashSet<Renderer>();

		private readonly Transform _parent;

		private readonly SaberTrail _saberTrail;

		private bool _shouldHide;

		[UsedImplicitly]
		internal SaberTrailHijacker(SiraLog log, SaberTrail saberTrail, ColorManager colorManager)
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			SaberTrailHijacker saberTrailHijacker = this;
			_log = log;
			_saberTrail = saberTrail;
			_parent = ((Component)saberTrail).transform.parent;
			if (!((Behaviour)saberTrail).enabled)
			{
				Saber componentInParent = ((Component)saberTrail).GetComponentInParent<Saber>();
				Color val = colorManager.ColorForSaberType(componentInParent.saberType);
				saberTrail.Setup(val, (IBladeMovementData)(object)componentInParent._movementData);
				((Behaviour)saberTrail).enabled = true;
				((Renderer)saberTrail._trailRenderer._meshRenderer).enabled = false;
			}
			else
			{
				_originalRenderers.Add((Renderer)(object)saberTrail._trailRenderer._meshRenderer);
			}
			CheckSaberFactory();
			CheckCustomSabersLite();
			void CheckCustomSabersLite()
			{
				if (PluginManager.GetPlugin("CustomSabersLite") != null)
				{
					SaberTrail[] array = (from n in ((Component)saberTrail).GetComponentsInChildren<SaberTrail>()
						where (Object)(object)n != (Object)(object)saberTrail
						select n).ToArray();
					if (array.Length == 0)
					{
						log.Warn("CustomSabersLite trail not yet created, deferring renderer fetching");
						((Component)saberTrail).gameObject.AddComponent<LiteTrailContract>().Init(saberTrailHijacker);
					}
					else
					{
						saberTrailHijacker._originalRenderers.UnionWith((IEnumerable<Renderer>)array.Select((SaberTrail n) => n._trailRenderer._meshRenderer));
					}
				}
			}
			void CheckSaberFactory()
			{
				PluginMetadata plugin = PluginManager.GetPlugin("Saber Factory");
				Assembly assembly = ((plugin != null) ? plugin.Assembly : null);
				if (!(assembly == null))
				{
					if ((object)_sfTrailType == null)
					{
						_sfTrailType = assembly.GetType("SaberFactory.Instances.Trail.SFTrail");
					}
					if ((object)_vertexPoolField == null)
					{
						_vertexPoolField = _sfTrailType?.GetField("_vertexPool", AccessTools.all);
					}
					if ((object)_vertexPoolType == null)
					{
						_vertexPoolType = assembly.GetType("SaberFactory.Misc.VertexPool");
					}
					if ((object)_vertexPoolMeshRendererField == null)
					{
						_vertexPoolMeshRendererField = _vertexPoolType.GetField("MeshRenderer", AccessTools.all);
					}
					object componentInChildren = ((Component)saberTrail).GetComponentInChildren(_sfTrailType);
					if (componentInChildren != null)
					{
						object obj = _vertexPoolField?.GetValue(componentInChildren);
						if (obj != null)
						{
							object obj2 = _vertexPoolMeshRendererField?.GetValue(obj);
							MeshRenderer val2 = (MeshRenderer)((obj2 is MeshRenderer) ? obj2 : null);
							if (val2 != null)
							{
								saberTrailHijacker._originalRenderers.Add((Renderer)(object)val2);
								return;
							}
						}
					}
					log.Error("Could not fetch Saber Factory trail");
				}
			}
		}

		public void Activate(List<FollowedSaberTrail> followedSaberTrails, bool hideOriginal)
		{
			foreach (FollowedSaberTrail followedSaberTrail in followedSaberTrails)
			{
				((Component)followedSaberTrail).transform.SetParent(_parent, false);
				followedSaberTrail.Init(_saberTrail, _parent);
			}
			if (!hideOriginal)
			{
				return;
			}
			_shouldHide = true;
			foreach (Renderer originalRenderer in _originalRenderers)
			{
				originalRenderer.enabled = false;
			}
		}

		public void Deactivate()
		{
			_shouldHide = false;
			foreach (Renderer originalRenderer in _originalRenderers)
			{
				originalRenderer.enabled = true;
			}
		}
	}
}
namespace Vivify.ObjectPrefab.Collections
{
	internal interface IPrefabCollection
	{
	}
	internal class PrefabDictionary : IPrefabCollection
	{
		private readonly Dictionary<Track, HashSet<PrefabPool?>> _dictionary = new Dictionary<Track, HashSet<PrefabPool>>();

		internal event Action<Track>? Changed;

		internal bool AddPrefabPool(Track key, PrefabPool? prefabPool, LoadMode loadMode)
		{
			if (!_dictionary.TryGetValue(key, out HashSet<PrefabPool> value))
			{
				Dictionary<Track, HashSet<PrefabPool?>> dictionary = _dictionary;
				HashSet<PrefabPool> obj = new HashSet<PrefabPool> { null };
				value = obj;
				dictionary[key] = obj;
			}
			if (loadMode == LoadMode.Single)
			{
				value.Clear();
			}
			bool result = value.Add(prefabPool);
			Action<Track>? action = this.Changed;
			if (action != null)
			{
				action(key);
				return result;
			}
			return result;
		}

		internal void Clear()
		{
			_dictionary.Clear();
		}

		internal bool TryGetValue(Track key, out HashSet<PrefabPool?> value)
		{
			return _dictionary.TryGetValue(key, out value);
		}
	}
	internal abstract class PrefabList<T> : IPrefabCollection where T : class
	{
		internal HashSet<T?> HashSet { get; } = new HashSet<T> { null };

		internal event Action<float>? Changed;

		internal bool AddPool(T? pool, LoadMode loadMode, float time)
		{
			if (loadMode == LoadMode.Single)
			{
				HashSet.Clear();
			}
			bool result = HashSet.Add(pool);
			Action<float>? action = this.Changed;
			if (action != null)
			{
				action(time);
				return result;
			}
			return result;
		}

		internal void Clear()
		{
			HashSet.Clear();
		}
	}
	internal class PrefabList : PrefabList<PrefabPool>
	{
	}
	internal class TrailList : PrefabList<TrailPool>
	{
	}
}
namespace Vivify.Managers
{
	internal class AssetBundleManager : IDisposable
	{
		private readonly Dictionary<string, Object> _assets = new Dictionary<string, Object>();

		private readonly SiraLog _log;

		private readonly AssetBundle? _mainBundle;

		[UsedImplicitly]
		private AssetBundleManager(SiraLog log, IReadonlyBeatmapData beatmapData, BeatmapLevel beatmapLevel, Config config)
		{
			IPreviewMediaData previewMediaData = beatmapLevel.previewMediaData;
			IPreviewMediaData obj = ((previewMediaData is FileSystemPreviewMediaData) ? previewMediaData : null) ?? throw new ArgumentException("Was not correct type. Expected: FileSystemPreviewMediaData, was: " + ((object)beatmapLevel.previewMediaData).GetType().Name + ".", "beatmapLevel");
			if (!(beatmapData is CustomBeatmapData customBeatmapData))
			{
				throw new ArgumentException("Was not correct type. Expected: CustomBeatmapData, was: " + ((object)beatmapData).GetType().Name + ".", "beatmapData");
			}
			_log = log;
			string text = Path.Combine(Path.GetDirectoryName(((FileSystemPreviewMediaData)obj)._previewAudioClipPath), "bundleWindows2021.vivify");
			if (!File.Exists(text))
			{
				_log.Error("[bundleWindows2021.vivify] not found");
				return;
			}
			if (HeckController.DebugMode)
			{
				_mainBundle = AssetBundle.LoadFromFile(text);
			}
			else
			{
				uint? num = customBeatmapData.levelCustomData.Get<CustomData>("_assetBundle")?.Get<uint>("_windows2021");
				if (num.HasValue)
				{
					_mainBundle = AssetBundle.LoadFromFile(text, num.Value);
				}
				else
				{
					_log.Error("Checksum not defined");
				}
			}
			if ((Object)(object)_mainBundle == (Object)null)
			{
				_log.Error("Failed to load [" + text + "]");
				return;
			}
			string[] allAssetNames = _mainBundle.GetAllAssetNames();
			foreach (string text2 in allAssetNames)
			{
				Object value = _mainBundle.LoadAsset(text2);
				_assets.Add(text2, value);
			}
		}

		public void Dispose()
		{
			if ((Object)(object)_mainBundle != (Object)null)
			{
				_mainBundle.Unload(true);
			}
		}

		internal bool TryGetAsset<T>(string assetName, [NotNullWhen(true)] out T? asset)
		{
			if (_assets.TryGetValue(assetName, out Object value))
			{
				if (value is T val)
				{
					asset = val;
					return true;
				}
				_log.Error("Found " + assetName + ", but was null or not [" + typeof(T).FullName + "]");
			}
			else
			{
				_log.Error("Could not find " + typeof(T).FullName + " [" + assetName + "]");
			}
			asset = default(T);
			return false;
		}
	}
	[UsedImplicitly]
	internal class CameraPropertyManager : IInitializable, IDisposable
	{
		internal class CameraProperties : IDisposable
		{
			private readonly HashSet<CameraPropertyController> _controllers = new HashSet<CameraPropertyController>();

			private DepthTextureMode? _depthTextureMode;

			private CameraClearFlags? _clearFlags;

			private Color? _backgroundColor;

			private CullingTextureTracker? _cullingTextureData;

			private bool? _bloomPrePass;

			private bool? _mainEffect;

			internal DepthTextureMode? DepthTextureMode
			{
				set
				{
					_depthTextureMode = value;
					foreach (CameraPropertyController controller in _controllers)
					{
						controller.DepthTextureMode = value;
					}
				}
			}

			internal CameraClearFlags? ClearFlags
			{
				set
				{
					_clearFlags = value;
					foreach (CameraPropertyController controller in _controllers)
					{
						controller.ClearFlags = value;
					}
				}
			}

			internal Color? BackgroundColor
			{
				set
				{
					_backgroundColor = value;
					foreach (CameraPropertyController controller in _controllers)
					{
						controller.BackgroundColor = value;
					}
				}
			}

			internal CullingTextureTracker? CullingTextureData
			{
				set
				{
					_cullingTextureData = value;
					foreach (CameraPropertyController controller in _controllers)
					{
						controller.CullingTextureData = value;
					}
				}
			}

			internal bool? BloomPrePass
			{
				set
				{
					_bloomPrePass = value;
					foreach (CameraPropertyController controller in _controllers)
					{
						controller.BloomPrePass = value;
					}
				}
			}

			internal bool? MainEffect
			{
				set
				{
					_mainEffect = value;
					foreach (CameraPropertyController controller in _controllers)
					{
						controller.MainEffect = value;
					}
				}
			}

			public void Dispose()
			{
				_cullingTextureData?.Dispose();
			}

			internal void AddController(CameraPropertyController controller)
			{
				controller.DepthTextureMode = _depthTextureMode;
				controller.ClearFlags = _clearFlags;
				controller.BackgroundColor = _backgroundColor;
				controller.CullingTextureData = _cullingTextureData;
				controller.BloomPrePass = _bloomPrePass;
				controller.MainEffect = _mainEffect;
				_controllers.Add(controller);
			}

			internal void RemoveController(CameraPropertyController controller)
			{
				_controllers.Remove(controller);
				controller.Reset();
			}
		}

		private static readonly HashSet<CameraPropertyController> _allControllers = new HashSet<CameraPropertyController>();

		internal Dictionary<string, CameraProperties> Properties { get; } = new Dictionary<string, CameraProperties>();

		internal static event Action<CameraPropertyController>? ControllerAdded;

		internal static event Action<CameraPropertyController>? ControllerRemoved;

		public void Initialize()
		{
			foreach (CameraPropertyController allController in _allControllers)
			{
				OnControllerAdded(allController);
			}
			ControllerAdded += OnControllerAdded;
			ControllerRemoved += OnControllerRemoved;
		}

		public void Dispose()
		{
			foreach (CameraProperties value in Properties.Values)
			{
				value.Dispose();
			}
			foreach (CameraPropertyController allController in _allControllers)
			{
				OnControllerRemoved(allController);
			}
			ControllerAdded -= OnControllerAdded;
			ControllerRemoved -= OnControllerRemoved;
		}

		internal static void AddControllerStatic(CameraPropertyController controller)
		{
			_allControllers.Add(controller);
			CameraPropertyManager.ControllerAdded?.Invoke(controller);
		}

		internal static void RemoveControllerStatic(CameraPropertyController controller)
		{
			_allControllers.Remove(controller);
			CameraPropertyManager.ControllerRemoved?.Invoke(controller);
		}

		private void OnControllerAdded(CameraPropertyController controller)
		{
			string key = controller.Id ?? "_Main";
			if (!Properties.TryGetValue(key, out CameraProperties value))
			{
				value = (Properties[key] = new CameraProperties());
			}
			value.AddController(controller);
		}

		private void OnControllerRemoved(CameraPropertyController controller)
		{
			if (Properties.TryGetValue(controller.Id ?? "_Main", out CameraProperties value))
			{
				value.RemoveController(controller);
			}
		}
	}
	internal class DepthShaderManager : IInitializable
	{
		private const string PATH = "Vivify.Resources.DepthBlit";

		internal Material? DepthArrayMaterial { get; private set; }

		internal Material? DepthMaterial { get; private set; }

		public void Initialize()
		{
			Load();
		}

		private static async Task<AssetBundle?> LoadFromMemoryAsync(byte[] binary, uint crc)
		{
			TaskCompletionSource<AssetBundle> taskCompletionSource = new TaskCompletionSource<AssetBundle>();
			AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromMemoryAsync(binary, crc);
			((AsyncOperation)bundleRequest).completed += delegate
			{
				taskCompletionSource.SetResult(bundleRequest.assetBundle);
			};
			return await taskCompletionSource.Task;
		}

		private static async Task<T?> LoadAssetAsync<T>(AssetBundle assetBundle, string path) where T : Object
		{
			TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
			AssetBundleRequest assetRequest = assetBundle.LoadAssetAsync<T>(path);
			((AsyncOperation)assetRequest).completed += delegate
			{
				taskCompletionSource.SetResult((T)(object)assetRequest.asset);
			};
			return await taskCompletionSource.Task;
		}

		private async Task Load()
		{
			byte[] binary;
			using (Stream stream = typeof(DepthShaderManager).Assembly.GetManifestResourceStream("Vivify.Resources.DepthBlit"))
			{
				using MemoryStream memoryStream = new MemoryStream();
				await stream.CopyToAsync(memoryStream);
				binary = memoryStream.ToArray();
			}
			AssetBundle bundle = await LoadFromMemoryAsync(binary, 1746663828u);
			if (!((Object)(object)bundle == (Object)null))
			{
				Task task = LoadAssetAsync<Material>(bundle, "assets/depthblit.mat").ContinueWith((Task<Material?> n) => DepthMaterial = n.Result);
				Task task2 = LoadAssetAsync<Material>(bundle, "assets/depthblitarrayslice.mat").ContinueWith((Task<Material?> n) => DepthArrayMaterial = n.Result);
				await Task.WhenAll(task, task2);
				bundle.UnloadAsync(false);
			}
		}
	}
	internal class PrefabManager : IDisposable
	{
		private readonly SiraLog _log;

		private readonly Dictionary<string, InstantiatedPrefab> _prefabs = new Dictionary<string, InstantiatedPrefab>();

		private readonly ReLoader? _reLoader;

		[UsedImplicitly]
		private PrefabManager(SiraLog log, [InjectOptional] ReLoader? reLoader)
		{
			_log = log;
			_reLoader = reLoader;
			if (reLoader != null)
			{
				reLoader.Rewinded += DestroyAllPrefabs;
			}
		}

		public void Dispose()
		{
			if (_reLoader != null)
			{
				_reLoader.Rewinded -= DestroyAllPrefabs;
			}
		}

		internal void Add(string id, GameObject prefab, List<Track>? track)
		{
			_prefabs.Add(id, new InstantiatedPrefab(prefab, track));
		}

		internal bool Destroy(string id)
		{
			if (!_prefabs.TryGetValue(id, out InstantiatedPrefab value))
			{
				return false;
			}
			List<Track> track = value.Track;
			if (track != null)
			{
				foreach (Track item in track)
				{
					item.RemoveGameObject(value.GameObject);
				}
			}
			Object.Destroy((Object)(object)value.GameObject);
			_prefabs.Remove(id);
			return true;
		}

		internal bool TryGetPrefab(string id, [NotNullWhen(true)] out InstantiatedPrefab? prefab)
		{
			bool num = _prefabs.TryGetValue(id, out prefab);
			if (!num)
			{
				_log.Error("No prefab with id [" + id + "] detected");
			}
			return num;
		}

		private void DestroyAllPrefabs()
		{
			string text = default(string);
			InstantiatedPrefab instantiatedPrefab = default(InstantiatedPrefab);
			foreach (KeyValuePair<string, InstantiatedPrefab> prefab in _prefabs)
			{
				Utils.Deconstruct<string, InstantiatedPrefab>(prefab, ref text, ref instantiatedPrefab);
				InstantiatedPrefab instantiatedPrefab2 = instantiatedPrefab;
				List<Track> track = instantiatedPrefab2.Track;
				if (track != null)
				{
					foreach (Track item in track)
					{
						item.RemoveGameObject(instantiatedPrefab2.GameObject);
					}
				}
				Object.Destroy((Object)(object)instantiatedPrefab2.GameObject);
			}
			_prefabs.Clear();
		}
	}
	internal class InstantiatedPrefab
	{
		internal Animator[] Animators { get; }

		internal GameObject GameObject { get; }

		internal List<Track>? Track { get; }

		internal InstantiatedPrefab(GameObject gameObject, List<Track>? track)
		{
			GameObject = gameObject;
			Track = track;
			Animators = gameObject.GetComponentsInChildren<Animator>();
		}
	}
}
namespace Vivify.Installers
{
	[UsedImplicitly]
	internal class VivifyAppInstaller : Installer
	{
		private readonly Config _config;

		private VivifyAppInstaller(Config config)
		{
			_config = config;
		}

		public override void InstallBindings()
		{
			((InstallerBase)this).Container.BindInstance<Config>(_config);
			((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<DepthShaderManager>()).AsSingle();
			((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<AddComponentsToCamera>()).AsSingle();
			((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<FeaturesModule>()).AsSingle();
		}
	}
	[UsedImplicitly]
	internal class VivifyMenuInstaller : Installer
	{
		public override void InstallBindings()
		{
			((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<SettingsMenu>()).AsSingle();
			((ScopeConcreteIdArgConditionCopyNonLazyBinder)((FromBinder)(object)((InstallerBase)this).Container.Bind<AssetBundleDownloadViewController.AssetDownloader>()).FromNewComponentOnNewGameObject()).AsSingle();
			((FromBinder)(object)((InstallerBase)this).Container.BindInterfacesTo<AssetBundleDownloadViewController>()).FromNewComponentAsViewController().AsSingle();
		}
	}
	[UsedImplicitly]
	internal class VivifyPlayerInstaller : Installer
	{
		private readonly FeaturesModule _featuresModule;

		private VivifyPlayerInstaller(FeaturesModule featuresModule)
		{
			_featuresModule = featuresModule;
		}

		public override void InstallBindings()
		{
			if (_featuresModule.Active)
			{
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<AssetBundleManager>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<PrefabManager>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<NotePrefabManager>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<DebrisPrefabManager>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<SaberPrefabManager>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<BeatmapObjectPrefabManager>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<CameraPropertyManager>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<CameraEffectApplier>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<ApplyPostProcessing>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<AssignObjectPrefab>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<DeclareCullingTexture>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<DeclareRenderTexture>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<DestroyPrefab>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<InstantiatePrefab>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<SetAnimatorProperty>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<SetCameraProperty>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<SetGlobalProperty>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesAndSelfTo<SetMaterialProperty>()).AsSingle();
				((ScopeConcreteIdArgConditionCopyNonLazyBinder)((InstallerBase)this).Container.BindInterfacesTo<SetRenderingSettings>()).AsSingle();
			}
		}
	}
}
namespace Vivify.HarmonyPatches
{
	internal class AddComponentsToCamera : IAffinity
	{
		private readonly SiraLog _log;

		private readonly DiContainer _container;

		private readonly List<Component> _injected = new List<Component>();

		private readonly Dictionary<ImageEffectController, PostProcessingController> _postProcessingControllers = new Dictionary<ImageEffectController, PostProcessingController>();

		[UsedImplicitly]
		private AddComponentsToCamera(SiraLog log, DiContainer container)
		{
			_log = log;
			_container = container;
		}

		[AffinityPostfix]
		[AffinityPatch(typeof(MainEffectController), "LazySetupImageEffectController", AffinityMethodType.Normal, null, new Type[] { })]
		private void AddComponents(MainEffectController __instance)
		{
			GameObject gameObject = ((Component)__instance).gameObject;
			SafeAddComponent<MultipassKeywordController>(gameObject);
			SafeAddComponent<PostProcessingController>(gameObject);
			SafeAddComponent<CameraPropertyController>(gameObject);
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(ImageEffectController), "OnRenderImage", AffinityMethodType.Normal, null, new Type[] { })]
		private bool StopRenderImage(ImageEffectController __instance, RenderTexture src, RenderTexture dest)
		{
			if (!_postProcessingControllers.TryGetValue(__instance, out PostProcessingController value))
			{
				value = ((Component)__instance).GetComponent<PostProcessingController>();
				if (!((Object)(object)value != (Object)null))
				{
					return true;
				}
				_postProcessingControllers[__instance] = value;
			}
			if (!((Behaviour)value).enabled)
			{
				return true;
			}
			Graphics.Blit((Texture)(object)src, dest);
			return false;
		}

		private void SafeAddComponent<T>(GameObject gameObject) where T : Component
		{
			string name = typeof(T).Name;
			T component = gameObject.GetComponent<T>();
			if ((Object)(object)component != (Object)null)
			{
				if (!_injected.Contains((Component)(object)component))
				{
					_log.Debug("Injected [" + name + "] for [" + ((Object)gameObject).name + "]");
					_container.Inject((object)component);
					_injected.Add((Component)(object)component);
				}
			}
			else
			{
				_log.Debug("Created [" + name + "] for [" + ((Object)gameObject).name + "]");
				_injected.Add((Component)(object)_container.InstantiateComponent<T>(gameObject));
			}
		}
	}
	[HeckPatch(null)]
	internal static class Camera2PriorityActivateCam
	{
		private static MethodBase? _setCameraActive;

		[UsedImplicitly]
		[HarmonyPrepare]
		private static bool Prepare()
		{
			PluginMetadata pluginFromId = PluginManager.GetPluginFromId("Camera2");
			Assembly assembly = ((pluginFromId != null) ? pluginFromId.Assembly : null);
			if (assembly != null)
			{
				_setCameraActive = assembly.GetType("Camera2.SDK.Cameras")?.GetMethod("SetCameraActive", AccessTools.all);
				if (_setCameraActive != null)
				{
					return true;
				}
				Plugin.Log.Warn("Could not find [Camera2.SDK.Cameras.SetCameraActive].");
				return false;
			}
			return _setCameraActive != null;
		}

		[UsedImplicitly]
		[HarmonyTargetMethod]
		private static MethodBase TargetMethod()
		{
			return _setCameraActive ?? throw new InvalidOperationException();
		}

		[UsedImplicitly]
		[HarmonyPostfix]
		private static void ActivateCam(string cameraName, bool active)
		{
			if (active)
			{
				if (!Camera2PriorityScene.ActiveCams.Add(cameraName))
				{
					return;
				}
			}
			else if (!Camera2PriorityScene.ActiveCams.Remove(cameraName))
			{
				return;
			}
			Camera2PriorityScene.UpdateMainCam();
		}
	}
	[HeckPatch(null)]
	internal static class Camera2PriorityScene
	{
		private static readonly HashSet<(string Name, MonoBehaviour MonoBehaviour)> _nonAllocCams = new HashSet<(string, MonoBehaviour)>();

		private static MethodBase? _cameraGetter;

		private static MethodBase? _switchToCamlist;

		private static IDictionary? _cams;

		public static HashSet<string> ActiveCams { get; } = new HashSet<string>();

		internal static void UpdateMainCam()
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected O, but got Unknown
			_nonAllocCams.Clear();
			foreach (DictionaryEntry item in _cams ?? throw new InvalidOperationException())
			{
				_nonAllocCams.Add(((string)item.Key, (MonoBehaviour)item.Value));
			}
			if (_cameraGetter == null)
			{
				throw new InvalidOperationException();
			}
			HashSet<string> activeCams = ActiveCams;
			string[] array = (from n in (from n in _nonAllocCams
					where activeCams.Contains(n.Name)
					orderby ((Camera)_cameraGetter.Invoke(n.MonoBehaviour, null)).depth
					select n).Take(Plugin.Config.MaxCamera2Cams)
				select n.Name).ToArray();
			string text = ((array.Length != 0) ? string.Join(", ", array) : "null");
			Plugin.Log.Info("Enabling Vivify Cam2 post-processing on [" + text + "]");
			foreach (var nonAllocCam in _nonAllocCams)
			{
				PostProcessingController componentInChildren = ((Component)nonAllocCam.MonoBehaviour).GetComponentInChildren<PostProcessingController>();
				if ((Object)(object)componentInChildren != (Object)null)
				{
					((Behaviour)componentInChildren).enabled = array.Contains<string>(nonAllocCam.Name);
				}
			}
		}

		[UsedImplicitly]
		[HarmonyPrepare]
		private static bool Prepare()
		{
			PluginMetadata pluginFromId = PluginManager.GetPluginFromId("Camera2");
			Assembly assembly = ((pluginFromId != null) ? pluginFromId.Assembly : null);
			if (assembly != null)
			{
				_switchToCamlist = assembly.GetType("Camera2.Managers.ScenesManager")?.GetMethod("SwitchToCamlist", AccessTools.all);
				if (_switchToCamlist == null)
				{
					Plugin.Log.Warn("Could not find [Camera2.Managers.ScenesManager.SwitchToCamlist].");
					return false;
				}
				_cams = (IDictionary)(assembly.GetType("Camera2.Managers.CamManager")?.GetProperty("cams", AccessTools.all)?.GetValue(null));
				if (_cams == null)
				{
					Plugin.Log.Warn("Could not find [Camera2.Managers.CamManager.cams].");
					return false;
				}
				_cameraGetter = assembly.GetType("Camera2.Behaviours.Cam2")?.GetProperty("UCamera", AccessTools.all)?.GetGetMethod(nonPublic: true);
				if (_cameraGetter == null)
				{
					Plugin.Log.Warn("Could not find [Camera2.Behaviours.Cam2.UCamera].");
					return false;
				}
			}
			return _switchToCamlist != null;
		}

		[UsedImplicitly]
		[HarmonyTargetMethod]
		private static MethodBase TargetMethod()
		{
			return _switchToCamlist ?? throw new InvalidOperationException();
		}

		[UsedImplicitly]
		[HarmonyPostfix]
		private static void SwapMainCam(List<string>? cams)
		{
			ActiveCams.Clear();
			if (cams != null)
			{
				ActiveCams.UnionWith(cams);
			}
			UpdateMainCam();
		}
	}
	internal class CameraEffectApplier : IAffinity, IDisposable
	{
		private readonly Dictionary<MainEffectController, PostProcessingController> _postProcessingControllers = new Dictionary<MainEffectController, PostProcessingController>();

		private readonly ReLoader? _reLoader;

		private readonly int _prewarmCount;

		internal Dictionary<string, CreateCameraData> CameraDatas { get; } = new Dictionary<string, CreateCameraData>();

		internal Dictionary<string, CreateScreenTextureData> DeclaredTextureDatas { get; } = new Dictionary<string, CreateScreenTextureData>();

		internal List<MaterialData> PreEffects { get; } = new List<MaterialData>();

		internal List<MaterialData> PostEffects { get; } = new List<MaterialData>();

		[UsedImplicitly]
		private CameraEffectApplier(IReadonlyBeatmapData beatmapData, [InjectOptional] ReLoader? reLoader)
		{
			_reLoader = reLoader;
			if (reLoader != null)
			{
				reLoader.Rewinded += OnRewind;
			}
			_prewarmCount = (((CustomBeatmapData)(object)beatmapData).customEventDatas.Any((CustomEventData n) => n.eventType == "CreateCamera") ? 1 : 0);
		}

		public void Dispose()
		{
			Reset();
			if (_reLoader != null)
			{
				_reLoader.Rewinded -= OnRewind;
			}
		}

		private void OnRewind()
		{
			Reset();
		}

		private void Reset()
		{
			CameraDatas.Clear();
			DeclaredTextureDatas.Clear();
			PreEffects.Clear();
			PostEffects.Clear();
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(MainEffectController), "ImageEffectControllerCallback", AffinityMethodType.Normal, null, new Type[] { })]
		private void ApplyVivifyEffect(MainEffectController __instance, RenderTexture src, RenderTexture dest)
		{
			if (!_postProcessingControllers.TryGetValue(__instance, out PostProcessingController value))
			{
				value = ((Component)__instance).GetComponent<PostProcessingController>();
				if (!((Object)(object)value == (Object)null))
				{
					_postProcessingControllers[__instance] = value;
					value.CameraDatas = CameraDatas;
					value.DeclaredTextureDatas = DeclaredTextureDatas;
					value.PreEffects = PreEffects;
					value.PostEffects = PostEffects;
					value.PrewarmCameras(_prewarmCount);
				}
			}
		}
	}
	[HeckPatch(PatchType.Features)]
	[HarmonyPatch(typeof(MirrorRendererSO))]
	internal static class IndexMirrorByHash
	{
		private static readonly MethodInfo _fieldOfViewGetter = AccessTools.PropertyGetter(typeof(Camera), "fieldOfView");

		private static readonly MethodInfo _getFloatHash = AccessTools.Method(typeof(IndexMirrorByHash), "GetFloatHash", (Type[])null, (Type[])null);

		private static float GetFloatHash(Camera camera)
		{
			return ((object)camera).GetHashCode();
		}

		[HarmonyTranspiler]
		[HarmonyPatch("RenderMirrorTexture")]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			return new CodeMatcher(instructions, (ILGenerator)null).MatchForward(false, (CodeMatch[])(object)new CodeMatch[1]
			{
				new CodeMatch((OpCode?)OpCodes.Callvirt, (object)_fieldOfViewGetter, (string)null)
			}).Set(OpCodes.Call, (object)_getFloatHash).InstructionEnumeration();
		}
	}
	[HeckPatch(PatchType.Features)]
	internal static class MoveAlwaysVisibleQuad
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(AlwaysVisibleQuad), "OnEnable")]
		private static void MoveIt(AlwaysVisibleQuad __instance)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			((Component)__instance).transform.position = new Vector3(0f, -1000f, 0f);
		}
	}
	[HeckPatch(PatchType.Features)]
	internal static class SaberTrailRendererEnabler
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(SaberTrailRenderer), "OnEnable")]
		[HarmonyPatch(typeof(SaberTrailRenderer), "OnDisable")]
		private static bool DisableDisable()
		{
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SaberTrail), "OnDisable")]
		private static bool DisableFix(SaberTrailRenderer ____trailRenderer)
		{
			if (Object.op_Implicit((Object)(object)____trailRenderer))
			{
				((Component)____trailRenderer).gameObject.SetActive(false);
			}
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SaberTrail), "OnEnable")]
		private static bool EnableFix(SaberTrail __instance, bool ____inited, TrailElementCollection ____trailElementCollection, Color ____color, SaberTrailRenderer ____trailRenderer)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (____inited)
			{
				__instance.ResetTrailData();
				____trailRenderer.UpdateMesh(____trailElementCollection, ____color);
			}
			if (Object.op_Implicit((Object)(object)____trailRenderer))
			{
				((Component)____trailRenderer).gameObject.SetActive(true);
			}
			return false;
		}
	}
}
namespace Vivify.Extras
{
	internal static class SortedListExtensions
	{
		internal static void InsertIntoSortedList<T>(this IList<T> list, T value) where T : IComparable<T>
		{
			int num = 0;
			int num2 = list.Count;
			while (num2 > num)
			{
				int num3 = num2 - num;
				int num4 = num + num3 / 2;
				int num5 = list[num4].CompareTo(value);
				if (num5 >= 0)
				{
					if (num5 == 0)
					{
						list.Insert(num4, value);
						return;
					}
					num2 = num4;
				}
				else
				{
					num = num4 + 1;
				}
			}
			list.Insert(num, value);
		}
	}
	[UsedImplicitly(/*Could not decode attribute arguments.*/)]
	public class XRSettingsSetter
	{
		public static bool useOcclusionMesh
		{
			get
			{
				return XRSettings.useOcclusionMesh;
			}
			set
			{
				XRSettings.useOcclusionMesh = value;
			}
		}
	}
}
namespace Vivify.Events
{
	[CustomEvent(new string[] { "Blit" })]
	internal class ApplyPostProcessing : ICustomEvent
	{
		private readonly SiraLog _log;

		private readonly AssetBundleManager _assetBundleManager;

		private readonly DeserializedData _deserializedData;

		private readonly BeatmapCallbacksController _beatmapCallbacksController;

		private readonly IBpmController _bpmController;

		private readonly SetMaterialProperty _setMaterialProperty;

		private readonly CameraEffectApplier _cameraEffectApplier;

		private readonly CoroutineDummy _coroutineDummy;

		private ApplyPostProcessing(SiraLog log, AssetBundleManager assetBundleManager, [Inject(Id = "Vivify")] DeserializedData deserializedData, BeatmapCallbacksController beatmapCallbacksController, IBpmController bpmController, SetMaterialProperty setMaterialProperty, CameraEffectApplier cameraEffectApplier, CoroutineDummy coroutineDummy)
		{
			_log = log;
			_assetBundleManager = assetBundleManager;
			_deserializedData = deserializedData;
			_beatmapCallbacksController = beatmapCallbacksController;
			_bpmController = bpmController;
			_setMaterialProperty = setMaterialProperty;
			_cameraEffectApplier = cameraEffectApplier;
			_coroutineDummy = coroutineDummy;
		}

		public void Callback(CustomEventData customEventData)
		{
			if (!_deserializedData.Resolve<ApplyPostProcessingData>(customEventData, out ApplyPostProcessingData result))
			{
				return;
			}
			float num = 60f * result.Duration / _bpmController.currentBpm;
			string asset = result.Asset;
			Material asset2 = null;
			if (asset != null)
			{
				if (!_assetBundleManager.TryGetAsset<Material>(asset, out asset2))
				{
					return;
				}
				List<MaterialProperty> properties = result.Properties;
				if (properties != null)
				{
					_setMaterialProperty.SetMaterialProperties(asset2, properties, num, result.Easing, ((BeatmapDataItem)customEventData).time);
				}
			}
			List<MaterialData> list = result.Order switch
			{
				PostProcessingOrder.BeforeMainEffect => _cameraEffectApplier.PreEffects, 
				PostProcessingOrder.AfterMainEffect => _cameraEffectApplier.PostEffects, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			if (num == 0f)
			{
				list.InsertIntoSortedList(new MaterialData(asset2, result.Priority, result.Source, result.Target, result.Pass, Time.frameCount));
				_log.Debug("Applied material [" + asset + "] for single frame");
			}
			else if (!(num <= 0f) && !(_beatmapCallbacksController.songTime > ((BeatmapDataItem)customEventData).time + num))
			{
				MaterialData materialData = new MaterialData(asset2, result.Priority, result.Source, result.Target, result.Pass);
				list.InsertIntoSortedList(materialData);
				_log.Debug($"Applied material [{asset}] for [{num}] seconds");
				((MonoBehaviour)_coroutineDummy).StartCoroutine(KillPostProcessingCoroutine(list, materialData, num, ((BeatmapDataItem)customEventData).time));
			}
		}

		internal IEnumerator KillPostProcessingCoroutine(List<MaterialData> effects, MaterialData data, float duration, float startTime)
		{
			while (true)
			{
				float num = _beatmapCallbacksController.songTime - startTime;
				if (!(num < 0f))
				{
					if (num < duration)
					{
						yield return null;
						continue;
					}
					effects.Remove(data);
					break;
				}
				break;
			}
		}
	}
	[CustomEvent(new string[] { "AssignObjectPrefab" })]
	internal class AssignObjectPrefab : IInitializable, ICustomEvent
	{
		private readonly BeatmapObjectPrefabManager _beatmapObjectPrefabManager;

		private readonly DebrisPrefabManager _debrisPrefabManager;

		private readonly DeserializedData _deserializedData;

		private readonly SiraLog _log;

		private readonly NotePrefabManager _notePrefabManager;

		private readonly SaberPrefabManager _saberPrefabManager;

		private AssignObjectPrefab(SiraLog log, [Inject(Id = "Vivify")] DeserializedData deserializedData, BeatmapObjectPrefabManager beatmapObjectPrefabManager, NotePrefabManager notePrefabManager, DebrisPrefabManager debrisPrefabManager, SaberPrefabManager saberPrefabManager)
		{
			_log = log;
			_deserializedData = deserializedData;
			_beatmapObjectPrefabManager = beatmapObjectPrefabManager;
			_notePrefabManager = notePrefabManager;
			_debrisPrefabManager = debrisPrefabManager;
			_saberPrefabManager = saberPrefabManager;
		}

		public void Initialize()
		{
			string text = default(string);
			AssignObjectPrefabData.IPrefabInfo prefabInfo = default(AssignObjectPrefabData.IPrefabInfo);
			foreach (ICustomEventCustomData value in _deserializedData.CustomEventCustomDatas.Values)
			{
				if (!(value is AssignObjectPrefabData assignObjectPrefabData))
				{
					continue;
				}
				foreach (KeyValuePair<string, AssignObjectPrefabData.IPrefabInfo> asset2 in assignObjectPrefabData.Assets)
				{
					Utils.Deconstruct<string, AssignObjectPrefabData.IPrefabInfo>(asset2, ref text, ref prefabInfo);
					string text2 = text;
					AssignObjectPrefabData.IPrefabInfo prefabInfo2 = prefabInfo;
					if (text2 != null && prefabInfo2 is AssignObjectPrefabData.ObjectPrefabInfo objectPrefabInfo)
					{
						string asset = objectPrefabInfo.Asset;
						if (!string.IsNullOrEmpty(asset))
						{
							_beatmapObjectPrefabManager.PrewarmGameObjectPrefabPool(asset, 10);
						}
						if (!string.IsNullOrEmpty(objectPrefabInfo.DebrisAsset))
						{
							_beatmapObjectPrefabManager.PrewarmGameObjectPrefabPool(asset, 10);
						}
						if (!string.IsNullOrEmpty(objectPrefabInfo.AnyDirectionAsset) && text2 == "colorNotes")
						{
							_beatmapObjectPrefabManager.PrewarmGameObjectPrefabPool(asset, 10);
						}
						break;
					}
				}
			}
		}

		public void Callback(CustomEventData customEventData)
		{
			if (!_deserializedData.Resolve<AssignObjectPrefabData>(customEventData, out AssignObjectPrefabData result))
			{
				return;
			}
			string text = default(string);
			AssignObjectPrefabData.IPrefabInfo prefabInfo = default(AssignObjectPrefabData.IPrefabInfo);
			foreach (KeyValuePair<string, AssignObjectPrefabData.IPrefabInfo> asset3 in result.Assets)
			{
				Utils.Deconstruct<string, AssignObjectPrefabData.IPrefabInfo>(asset3, ref text, ref prefabInfo);
				string text2 = text;
				AssignObjectPrefabData.IPrefabInfo prefabInfo2 = prefabInfo;
				if (!(prefabInfo2 is AssignObjectPrefabData.ObjectPrefabInfo objectPrefabInfo))
				{
					if (!(prefabInfo2 is AssignObjectPrefabData.SaberPrefabInfo saberPrefabInfo))
					{
						continue;
					}
					string asset = saberPrefabInfo.Asset;
					if (asset != string.Empty)
					{
						if ((saberPrefabInfo.Type & AssignObjectPrefabData.SaberPrefabInfo.SaberType.Left) != 0)
						{
							_log.Debug(string.Format("Assigned prefab [{0}] for left saber with load mode [{1}]", asset ?? "null", result.LoadMode));
							_beatmapObjectPrefabManager.AssignGameObjectPrefab(_saberPrefabManager.SaberAPrefabs, asset, result.LoadMode, ((BeatmapDataItem)customEventData).time);
						}
						if ((saberPrefabInfo.Type & AssignObjectPrefabData.SaberPrefabInfo.SaberType.Right) != 0)
						{
							_log.Debug(string.Format("Assigned prefab [{0}] for right saber with load mode [{1}]", asset ?? "null", result.LoadMode));
							_beatmapObjectPrefabManager.AssignGameObjectPrefab(_saberPrefabManager.SaberBPrefabs, asset, result.LoadMode, ((BeatmapDataItem)customEventData).time);
						}
					}
					string trailAsset = saberPrefabInfo.TrailAsset;
					if (trailAsset != string.Empty)
					{
						TrailProperties trailProperties = new TrailProperties(saberPrefabInfo.TopPos, saberPrefabInfo.BottomPos, saberPrefabInfo.Duration, saberPrefabInfo.SamplingFrequency, saberPrefabInfo.Granularity);
						if ((saberPrefabInfo.Type & AssignObjectPrefabData.SaberPrefabInfo.SaberType.Left) != 0)
						{
							_log.Debug(string.Format("Assigned trail material [{0}] for left saber with load mode [{1}]", trailAsset ?? "null", result.LoadMode));
							_beatmapObjectPrefabManager.AssignTrail(_saberPrefabManager.SaberATrailMaterials, trailAsset, trailProperties, result.LoadMode, ((BeatmapDataItem)customEventData).time);
						}
						if ((saberPrefabInfo.Type & AssignObjectPrefabData.SaberPrefabInfo.SaberType.Right) != 0)
						{
							_log.Debug(string.Format("Assigned trail material [{0}] for right saber with load mode [{1}]", trailAsset ?? "null", result.LoadMode));
							_beatmapObjectPrefabManager.AssignTrail(_saberPrefabManager.SaberBTrailMaterials, trailAsset, trailProperties, result.LoadMode, ((BeatmapDataItem)customEventData).time);
						}
					}
					continue;
				}
				IPrefabCollection prefabCollection = text2 switch
				{
					"colorNotes" => _notePrefabManager.ColorNotePrefabs, 
					"bombNotes" => _notePrefabManager.BombNotePrefabs, 
					"burstSliders" => _notePrefabManager.BurstSliderPrefabs, 
					"burstSliderElements" => _notePrefabManager.BurstSliderElementPrefabs, 
					_ => null, 
				};
				if (prefabCollection == null)
				{
					_log.Error("[" + text2 + "] not recognized");
					continue;
				}
				if (objectPrefabInfo.Track == null)
				{
					_log.Error("No track defined");
					continue;
				}
				string asset2 = objectPrefabInfo.Asset;
				if (asset2 != string.Empty)
				{
					_log.Debug(string.Format("Assigned track prefab: [{0}] for [{1}] with load mode [{2}]", asset2 ?? "null", text2, result.LoadMode));
					_beatmapObjectPrefabManager.AssignTrackPrefab((PrefabDictionary)prefabCollection, objectPrefabInfo.Track, asset2, result.LoadMode);
				}
				string debrisAsset = objectPrefabInfo.DebrisAsset;
				if (debrisAsset != string.Empty)
				{
					PrefabDictionary prefabDictionary = text2 switch
					{
						"colorNotes" => _debrisPrefabManager.ColorNoteDebrisPrefabs, 
						"burstSliders" => _debrisPrefabManager.BurstSliderDebrisPrefabs, 
						"burstSliderElements" => _debrisPrefabManager.BurstSliderElementDebrisPrefabs, 
						_ => null, 
					};
					if (prefabDictionary == null)
					{
						_log.Error("[" + text2 + "] debris not recognized");
						continue;
					}
					_log.Debug(string.Format("Assigned debris track prefab [{0}] for [{1}] with load mode [{2}]", debrisAsset ?? "null", text2, result.LoadMode));
					_beatmapObjectPrefabManager.AssignTrackPrefab(prefabDictionary, objectPrefabInfo.Track, debrisAsset, result.LoadMode);
				}
				string anyDirectionAsset = objectPrefabInfo.AnyDirectionAsset;
				if (anyDirectionAsset != string.Empty && text2 == "colorNotes")
				{
					_log.Debug(string.Format("Assigned any direction track prefab [{0}] for [{1}] with load mode [{2}]", debrisAsset ?? "null", text2, result.LoadMode));
					_beatmapObjectPrefabManager.AssignTrackPrefab(_notePrefabManager.AnyDirectionNotePrefabs, objectPrefabInfo.Track, anyDirectionAsset, result.LoadMode);
				}
			}
		}
	}
	[CustomEvent(new string[] { "CreateCamera" })]
	internal class DeclareCullingTexture : ICustomEvent
	{
		private readonly SiraLog _log;

		private readonly SetCameraProperty _setCameraProperty;

		private readonly CameraEffectApplier _cameraEffectApplier;

		private readonly DeserializedData _deserializedData;

		private DeclareCullingTexture(SiraLog log, SetCameraProperty setCameraProperty, CameraEffectApplier cameraEffectApplier, [Inject(Id = "Vivify")] DeserializedData deserializedData)
		{
			_log = log;
			_setCameraProperty = setCameraProperty;
			_cameraEffectApplier = cameraEffectApplier;
			_deserializedData = deserializedData;
		}

		public void Callback(CustomEventData customEventData)
		{
			if (_deserializedData.Resolve<CreateCameraData>(customEventData, out CreateCameraData result))
			{
				string name = result.Name;
				_cameraEffectApplier.CameraDatas.Add(name, result);
				_log.Debug("Created camera [" + name + "]");
				if (result.Property != null)
				{
					_setCameraProperty.SetCameraProperties(name, result.Property);
				}
			}
		}
	}
	[CustomEvent(new string[] { "CreateScreenTexture" })]
	internal class DeclareRenderTexture : ICustomEvent
	{
		private readonly SiraLog _log;

		private readonly DeserializedData _deserializedData;

		private readonly CameraEffectApplier _cameraEffectApplier;

		private DeclareRenderTexture(SiraLog log, [Inject(Id = "Vivify")] DeserializedData deserializedData, CameraEffectApplier cameraEffectApplier)
		{
			_log = log;
			_deserializedData = deserializedData;
			_cameraEffectApplier = cameraEffectApplier;
		}

		public void Callback(CustomEventData customEventData)
		{
			if (_deserializedData.Resolve<CreateScreenTextureData>(customEventData, out CreateScreenTextureData result))
			{
				_cameraEffectApplier.DeclaredTextureDatas.Add(result.Name, result);
				_log.Debug("Created texture [" + result.Name + "]");
			}
		}
	}
	[CustomEvent(new string[] { "DestroyObject" })]
	internal class DestroyPrefab : ICustomEvent
	{
		private readonly DeserializedData _deserializedData;

		private readonly SiraLog _log;

		private readonly PrefabManager _prefabManager;

		private readonly CameraEffectApplier _cameraEffectApplier;

		private DestroyPrefab(SiraLog log, PrefabManager prefabManager, CameraEffectApplier cameraEffectApplier, [Inject(Id = "Vivify")] DeserializedData deserializedData)
		{
			_log = log;
			_prefabManager = prefabManager;
			_cameraEffectApplier = cameraEffectApplier;
			_deserializedData = deserializedData;
		}

		public void Callback(CustomEventData customEventData)
		{
			if (!_deserializedData.Resolve<DestroyObjectData>(customEventData, out DestroyObjectData result))
			{
				return;
			}
			string[] id = result.Id;
			foreach (string text in id)
			{
				if (_cameraEffectApplier.CameraDatas.Remove(text))
				{
					_log.Debug("Destroyed camera [" + text + "]");
				}
				else if (_cameraEffectApplier.DeclaredTextureDatas.Remove(text))
				{
					_log.Debug("Destroyed screen texture [" + text + "]");
				}
				else if (_prefabManager.Destroy(text))
				{
					_log.Debug("Destroyed prefab [" + text + "]");
				}
				else
				{
					_log.Error("Could not find [" + text + "]");
				}
			}
		}
	}
	[CustomEvent(new string[] { "InstantiatePrefab" })]
	internal class InstantiatePrefab : ICustomEvent, IInitializable, IDisposable
	{
		private readonly AssetBundleManager _assetBundleManager;

		private readonly DeserializedData _deserializedData;

		private readonly IInstantiator _instantiator;

		private readonly ReLoader? _reLoader;

		private readonly SiraLog _log;

		private readonly PrefabManager _prefabManager;

		private readonly IReadonlyBeatmapData _readonlyBeatmapData;

		private readonly TransformControllerFactory _transformControllerFactory;

		private readonly bool _leftHanded;

		private readonly Dictionary<InstantiatePrefabData, GameObject> _loadedPrefabs = new Dictionary<InstantiatePrefabData, GameObject>();

		private Transform? _mirroredParent;

		private InstantiatePrefab(SiraLog log, IInstantiator instantiator, AssetBundleManager assetBundleManager, PrefabManager prefabManager, IReadonlyBeatmapData readonlyBeatmapData, [Inject(Id = "Vivify")] DeserializedData deserializedData, TransformControllerFactory transformControllerFactory, [Inject(Id = "leftHanded")] bool leftHanded, [InjectOptional] ReLoader? reLoader)
		{
			_log = log;
			_instantiator = instantiator;
			_assetBundleManager = assetBundleManager;
			_prefabManager = prefabManager;
			_readonlyBeatmapData = readonlyBeatmapData;
			_deserializedData = deserializedData;
			_transformControllerFactory = transformControllerFactory;
			_leftHanded = leftHanded;
			_reLoader = reLoader;
			if (reLoader != null)
			{
				reLoader.Rewinded += OnRewind;
			}
		}

		public void Dispose()
		{
			if (_reLoader != null)
			{
				_reLoader.Rewinded -= OnRewind;
			}
			DestroyAllPrefabs();
		}

		public void Initialize()
		{
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			if (!(_readonlyBeatmapData is CustomBeatmapData customBeatmapData))
			{
				return;
			}
			foreach (CustomEventData customEventData in customBeatmapData.customEventDatas)
			{
				if (!(customEventData.eventType != "InstantiatePrefab") && _deserializedData.Resolve<InstantiatePrefabData>(customEventData, out InstantiatePrefabData result))
				{
					string asset = result.Asset;
					if (_assetBundleManager.TryGetAsset<GameObject>(asset, out GameObject asset2))
					{
						GameObject val = Object.Instantiate<GameObject>(asset2);
						val.SetActive(false);
						_loadedPrefabs.Add(result, val);
					}
				}
			}
			if (_leftHanded)
			{
				_mirroredParent = new GameObject("LeftHandPrefabParent").transform;
				_mirroredParent.localScale = _mirroredParent.localScale.Mirror();
			}
		}

		public void Callback(CustomEventData customEventData)
		{
			if (!_deserializedData.Resolve<InstantiatePrefabData>(customEventData, out InstantiatePrefabData result) || !_loadedPrefabs.TryGetValue(result, out GameObject value))
			{
				return;
			}
			value.SetActive(true);
			Transform transform = value.transform;
			result.TransformData.Apply(transform, leftHanded: false);
			if ((Object)(object)_mirroredParent != (Object)null)
			{
				transform.SetParent(_mirroredParent);
			}
			if (result.Track != null)
			{
				foreach (Track item in result.Track)
				{
					item.AddGameObject(value);
				}
				_transformControllerFactory.Create(value, result.Track);
			}
			_instantiator.SongSynchronize(value, ((BeatmapDataItem)customEventData).time);
			string id = result.Id;
			if (id != null)
			{
				_log.Debug("Enabled [" + result.Asset + "] with id [" + id + "]");
				_prefabManager.Add(id, value, result.Track);
			}
			else
			{
				string id2 = ((object)value).GetHashCode().ToString();
				_log.Debug("Enabled [" + result.Asset + "] without id");
				_prefabManager.Add(id2, value, result.Track);
			}
		}

		private void OnRewind()
		{
			DestroyAllPrefabs();
			Initialize();
		}

		private void DestroyAllPrefabs()
		{
			CollectionExtensions.Do<GameObject>((IEnumerable<GameObject>)_loadedPrefabs.Values, (Action<GameObject>)Object.Destroy);
			_loadedPrefabs.Clear();
		}
	}
	public class RenderEnumCapturedSetting<TEnum> : EnumCapturedSetting<RenderSettings, TEnum> where TEnum : struct, Enum
	{
		internal RenderEnumCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class RenderColorCapturedSetting : ColorCapturedSetting<RenderSettings>
	{
		internal RenderColorCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class RenderFloatCapturedSetting : FloatCapturedSetting<RenderSettings>
	{
		internal RenderFloatCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class RenderIntCapturedSetting : IntCapturedSetting<RenderSettings>
	{
		internal RenderIntCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class RenderBoolCapturedSetting : BoolCapturedSetting<RenderSettings>
	{
		internal RenderBoolCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class RenderMaterialCapturedSetting : CapturedSettings<RenderSettings, Material>
	{
		internal RenderMaterialCapturedSetting(string property, AssetBundleManager assetBundleManager)
			: base(property, (Func<object, Material?>)((object obj) => (!assetBundleManager.TryGetAsset<Material>((string)obj, out Material asset)) ? null : asset))
		{
		}
	}
	public class RenderLightCapturedSetting : CapturedSettings<RenderSettings, Light>
	{
		internal RenderLightCapturedSetting(string property, PrefabManager prefabManager)
			: base(property, (Func<object, Light?>)((object obj) => (!prefabManager.TryGetPrefab((string)obj, out InstantiatedPrefab prefab)) ? null : prefab.GameObject.GetComponents<Light>().FirstOrDefault((Light n) => (int)n.type == 1)))
		{
		}
	}
	public class QualityEnumCapturedSetting<TEnum> : EnumCapturedSetting<QualitySettings, TEnum> where TEnum : struct, Enum
	{
		internal QualityEnumCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class QualityIntCapturedSetting : IntCapturedSetting<QualitySettings>
	{
		internal QualityIntCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class QualityFloatCapturedSetting : FloatCapturedSetting<QualitySettings>
	{
		internal QualityFloatCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class QualityBoolCapturedSetting : BoolCapturedSetting<QualitySettings>
	{
		internal QualityBoolCapturedSetting(string property)
			: base(property)
		{
		}
	}
	public class EnumCapturedSetting<TClass, TEnum> : CapturedSettings<TClass, TEnum> where TClass : class where TEnum : struct, Enum
	{
		internal EnumCapturedSetting(string property)
			: base(property, (Func<object, TEnum?>)Convert)
		{
		}

		private static TEnum Convert(object obj)
		{
			return (TEnum)Enum.ToObject(typeof(TEnum), (int)(float)obj);
		}
	}
	public class IntCapturedSetting<TClass> : CapturedSettings<TClass, int> where TClass : class
	{
		internal IntCapturedSetting(string property)
			: base(property, (Func<object, int>)Convert.ToInt32)
		{
		}
	}
	public class FloatCapturedSetting<TClass> : CapturedSettings<TClass, float> where TClass : class
	{
		internal FloatCapturedSetting(string property)
			: base(property, (Func<object, float>)Convert.ToSingle)
		{
		}
	}
	public class BoolCapturedSetting<TClass> : CapturedSettings<TClass, bool> where TClass : class
	{
		internal BoolCapturedSetting(string property)
			: base(property, (Func<object, bool>)Convert)
		{
		}

		private static bool Convert(object obj)
		{
			return System.Convert.ToBoolean(obj);
		}
	}
	public class ColorCapturedSetting<TClass> : CapturedSettings<TClass, Color> where TClass : class
	{
		internal ColorCapturedSetting(string property)
			: base(property, (Func<object, Color>)Convert)
		{
		}

		private static Color Convert(object obj)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Color.op_Implicit((Vector4)obj);
		}
	}
	public class CapturedSettings<TClass, TProperty> where TClass : class
	{
		private readonly Func<object, TProperty?> _convert;

		private readonly Func<TProperty> _get;

		private readonly Action<TProperty?> _set;

		private TProperty? _captured;

		internal CapturedSettings(string property, Func<object, TProperty?> convert)
		{
			PropertyInfo propertyInfo = AccessTools.Property(typeof(TClass), property);
			_get = (Func<TProperty>)Delegate.CreateDelegate(typeof(Func<TProperty>), propertyInfo.GetMethod);
			_set = (Action<TProperty>)Delegate.CreateDelegate(typeof(Action<TProperty>), propertyInfo.SetMethod);
			_convert = convert;
		}

		public void Capture()
		{
			_captured = _get();
		}

		public void Reset()
		{
			_set(_captured);
		}

		public void Set(object value)
		{
			_set(_convert(value));
		}
	}
	[CustomEvent(new string[] { "SetAnimatorProperty" })]
	internal class SetAnimatorProperty : ICustomEvent
	{
		private readonly IAudioTimeSource _audioTimeSource;

		private readonly IBpmController _bpmController;

		private readonly CoroutineDummy _coroutineDummy;

		private readonly DeserializedData _deserializedData;

		private readonly SiraLog _log;

		private readonly PrefabManager _prefabManager;

		private SetAnimatorProperty(SiraLog log, PrefabManager prefabManager, [Inject(Id = "Vivify")] DeserializedData deserializedData, IAudioTimeSource audioTimeSource, IBpmController bpmController, CoroutineDummy coroutineDummy)
		{
			_log = log;
			_prefabManager = prefabManager;
			_deserializedData = deserializedData;
			_audioTimeSource = audioTimeSource;
			_bpmController = bpmController;
			_coroutineDummy = coroutineDummy;
		}

		public void Callback(CustomEventData customEventData)
		{
			if (_deserializedData.Resolve<SetAnimatorPropertyData>(customEventData, out SetAnimatorPropertyData result))
			{
				float duration = result.Duration;
				duration = 60f * duration / _bpmController.currentBpm;
				if (_prefabManager.TryGetPrefab(result.Id, out InstantiatedPrefab prefab))
				{
					List<AnimatorProperty> properties = result.Properties;
					SetAnimatorProperties(prefab.Animators, properties, duration, result.Easing, ((BeatmapDataItem)customEventData).time);
				}
			}
		}

		internal void SetAnimatorProperties(Animator[] animators, List<AnimatorProperty> properties, float duration, Functions easing, float startTime)
		{
			foreach (AnimatorProperty property in properties)
			{
				string name = property.Name;
				AnimatorPropertyType type = property.Type;
				object value = property.Value;
				bool flag = duration == 0f || startTime + duration < _audioTimeSource.songTime;
				AnimatedAnimatorProperty animatedAnimatorProperty = property as AnimatedAnimatorProperty;
				switch (type)
				{
				case AnimatorPropertyType.Bool:
					if (animatedAnimatorProperty != null)
					{
						if (flag)
						{
							Animator[] array = animators;
							for (int i = 0; i < array.Length; i++)
							{
								array[i].SetBool(name, animatedAnimatorProperty.PointDefinition.Interpolate(1f) >= 1f);
							}
						}
						else
						{
							StartCoroutine(animatedAnimatorProperty.PointDefinition, animators, name, AnimatorPropertyType.Bool, duration, startTime, easing);
						}
					}
					else
					{
						Animator[] array = animators;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].SetBool(name, (bool)value);
						}
					}
					break;
				case AnimatorPropertyType.Float:
					if (animatedAnimatorProperty != null)
					{
						if (flag)
						{
							Animator[] array = animators;
							for (int i = 0; i < array.Length; i++)
							{
								array[i].SetFloat(name, animatedAnimatorProperty.PointDefinition.Interpolate(1f));
							}
						}
						else
						{
							StartCoroutine(animatedAnimatorProperty.PointDefinition, animators, name, AnimatorPropertyType.Float, duration, startTime, easing);
						}
					}
					else
					{
						Animator[] array = animators;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].SetFloat(name, Convert.ToSingle(value));
						}
					}
					break;
				case AnimatorPropertyType.Integer:
					if (animatedAnimatorProperty != null)
					{
						if (flag)
						{
							Animator[] array = animators;
							for (int i = 0; i < array.Length; i++)
							{
								array[i].SetFloat(name, animatedAnimatorProperty.PointDefinition.Interpolate(1f));
							}
						}
						else
						{
							StartCoroutine(animatedAnimatorProperty.PointDefinition, animators, name, AnimatorPropertyType.Float, duration, startTime, easing);
						}
					}
					else
					{
						Animator[] array = animators;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].SetFloat(name, Convert.ToSingle(value));
						}
					}
					break;
				case AnimatorPropertyType.Trigger:
				{
					bool flag2 = (bool)value;
					Animator[] array = animators;
					foreach (Animator val in array)
					{
						if (flag2)
						{
							val.SetTrigger(name);
						}
						else
						{
							val.ResetTrigger(name);
						}
					}
					break;
				}
				default:
					_log.Error($"[{type}] invalid");
					break;
				}
			}
		}

		private IEnumerator AnimatePropertyCoroutine(PointDefinition<float> points, Animator[] animators, string name, AnimatorPropertyType type, float duration, float startTime, Functions easing)
		{
			while (true)
			{
				float num = _audioTimeSource.songTime - startTime;
				if (!(num < duration))
				{
					break;
				}
				float time = Easings.Interpolate(Mathf.Min(num / duration, 1f), easing);
				switch (type)
				{
				default:
					yield break;
				case AnimatorPropertyType.Bool:
				{
					bool flag = points.Interpolate(time) >= 1f;
					Animator[] array = animators;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetBool(name, flag);
					}
					break;
				}
				case AnimatorPropertyType.Float:
				{
					float num3 = points.Interpolate(time);
					Animator[] array = animators;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetFloat(name, num3);
					}
					break;
				}
				case AnimatorPropertyType.Integer:
				{
					float num2 = points.Interpolate(time);
					Animator[] array = animators;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetInteger(name, (int)num2);
					}
					break;
				}
				}
				yield return null;
			}
		}

		private void StartCoroutine(PointDefinition<float> points, Animator[] animators, string name, AnimatorPropertyType type, float duration, float startTime, Functions easing)
		{
			((MonoBehaviour)_coroutineDummy).StartCoroutine(AnimatePropertyCoroutine(points, animators, name, type, duration, startTime, easing));
		}
	}
	[CustomEvent(new string[] { "SetCameraProperty" })]
	internal class SetCameraProperty : ICustomEvent
	{
		private readonly CameraPropertyManager _cameraPropertyManager;

		private readonly DeserializedData _deserializedData;

		private SetCameraProperty(CameraPropertyManager cameraPropertyManager, [Inject(Id = "Vivify")] DeserializedData deserializedData)
		{
			_cameraPropertyManager = cameraPropertyManager;
			_deserializedData = deserializedData;
		}

		public void Callback(CustomEventData customEventData)
		{
			if (_deserializedData.Resolve<SetCameraPropertyData>(customEventData, out SetCameraPropertyData result))
			{
				SetCameraProperties(result.Id, result.Property);
			}
		}

		public void SetCameraProperties(string id, CameraProperty property)
		{
			if (!_cameraPropertyManager.Properties.TryGetValue(id, out CameraPropertyManager.CameraProperties value))
			{
				value = (_cameraPropertyManager.Properties[id] = new CameraPropertyManager.CameraProperties());
			}
			if (property.HasDepthTextureMode)
			{
				value.DepthTextureMode = property.DepthTextureMode;
			}
			if (property.HasClearFlags)
			{
				value.ClearFlags = property.ClearFlags;
			}
			if (property.HasBackgroundColor)
			{
				value.BackgroundColor = property.BackgroundColor;
			}
			if (property.HasCulling)
			{
				CameraProperty.CullingData culling = property.Culling;
				value.CullingTextureData = ((culling != null) ? new CullingTextureTracker(culling.Tracks, culling.Whitelist) : null);
			}
			if (property.HasBloomPrePass)
			{
				value.BloomPrePass = property.BloomPrePass;
			}
			if (property.HasMainEffect)
			{
				value.MainEffect = property.MainEffect;
			}
		}
	}
	[CustomEvent(new string[] { "SetGlobalProperty" })]
	internal class SetGlobalProperty : ICustomEvent, IInitializable, IDisposable
	{
		private readonly struct ResettableProperty
		{
			internal object Id { get; }

			internal MaterialPropertyType Type { get; }

			internal object Value { get; }

			internal ResettableProperty(object id, MaterialPropertyType type, object value)
			{
				Id = id;
				Type = type;
				Value = value;
			}
		}

		private readonly AssetBundleManager _assetBundleManager;

		private readonly IAudioTimeSource _audioTimeSource;

		private readonly IBpmController _bpmController;

		private readonly CoroutineDummy _coroutineDummy;

		private readonly DeserializedData _deserializedData;

		private readonly List<ResettableProperty> _resettableProperties = new List<ResettableProperty>();

		[UsedImplicitly]
		private SetGlobalProperty(AssetBundleManager assetBundleManager, [Inject(Id = "Vivify")] DeserializedData deserializedData, IAudioTimeSource audioTimeSource, IBpmController bpmController, CoroutineDummy coroutineDummy)
		{
			_assetBundleManager = assetBundleManager;
			_deserializedData = deserializedData;
			_audioTimeSource = audioTimeSource;
			_bpmController = bpmController;
			_coroutineDummy = coroutineDummy;
		}

		public void Initialize()
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			foreach (ICustomEventCustomData value2 in _deserializedData.CustomEventCustomDatas.Values)
			{
				if (!(value2 is SetGlobalPropertyData setGlobalPropertyData))
				{
					continue;
				}
				foreach (MaterialProperty property in setGlobalPropertyData.Properties)
				{
					object id = property.Id;
					object obj2;
					if (!(id is int num))
					{
						if (!(id is string text))
						{
							throw new ArgumentOutOfRangeException();
						}
						object obj = ((property.Type != MaterialPropertyType.Keyword) ? new ArgumentOutOfRangeException() : ((object)Shader.IsKeywordEnabled(text)));
						obj2 = obj;
					}
					else
					{
						obj2 = property.Type switch
						{
							MaterialPropertyType.Texture => Shader.GetGlobalTexture(num), 
							MaterialPropertyType.Color => Shader.GetGlobalColor(num), 
							MaterialPropertyType.Float => Shader.GetGlobalFloat(num), 
							MaterialPropertyType.Vector => Shader.GetGlobalVector(num), 
							_ => new ArgumentOutOfRangeException(), 
						};
					}
					object value = obj2;
					_resettableProperties.Add(new ResettableProperty(property.Id, property.Type, value));
				}
			}
		}

		public void Dispose()
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Expected O, but got Unknown
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			foreach (ResettableProperty resettableProperty in _resettableProperties)
			{
				object value = resettableProperty.Value;
				object id = resettableProperty.Id;
				if (!(id is int num))
				{
					if (id is string keyword)
					{
						if (resettableProperty.Type == MaterialPropertyType.Keyword)
						{
							SetGlobalKeyword(keyword, (bool)value);
							continue;
						}
						throw new ArgumentOutOfRangeException();
					}
					throw new ArgumentOutOfRangeException();
				}
				switch (resettableProperty.Type)
				{
				case MaterialPropertyType.Texture:
					Shader.SetGlobalTexture(num, (Texture)value);
					break;
				case MaterialPropertyType.Color:
					Shader.SetGlobalColor(num, (Color)value);
					break;
				case MaterialPropertyType.Float:
					Shader.SetGlobalFloat(num, (float)value);
					break;
				case MaterialPropertyType.Vector:
					Shader.SetGlobalVector(num, (Vector4)value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void Callback(CustomEventData customEventData)
		{
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			if (!_deserializedData.Resolve<SetGlobalPropertyData>(customEventData, out SetGlobalPropertyData result))
			{
				return;
			}
			float duration = result.Duration;
			duration = 60f * duration / _bpmController.currentBpm;
			List<MaterialProperty> properties = result.Properties;
			Functions easing = result.Easing;
			float time = ((BeatmapDataItem)customEventData).time;
			foreach (MaterialProperty item in properties)
			{
				MaterialPropertyType type = item.Type;
				object value = item.Value;
				bool flag = duration == 0f || time + duration < _audioTimeSource.songTime;
				object id = item.Id;
				if (!(id is int num))
				{
					if (id is string text && type == MaterialPropertyType.Keyword)
					{
						if (item is AnimatedMaterialProperty<float> animatedMaterialProperty)
						{
							if (flag)
							{
								SetGlobalKeyword(text, animatedMaterialProperty.PointDefinition.Interpolate(1f) >= 1f);
							}
							else
							{
								StartCoroutine(animatedMaterialProperty.PointDefinition, text, MaterialPropertyType.Float, duration, time, easing);
							}
						}
						else
						{
							SetGlobalKeyword(text, (bool)value);
						}
						continue;
					}
				}
				else
				{
					switch (type)
					{
					case MaterialPropertyType.Texture:
					{
						string assetName = Convert.ToString(value);
						if (_assetBundleManager.TryGetAsset<Texture>(assetName, out Texture asset))
						{
							Shader.SetGlobalTexture(num, asset);
						}
						continue;
					}
					case MaterialPropertyType.Color:
						if (item is AnimatedMaterialProperty<Vector4> animatedMaterialProperty4)
						{
							if (flag)
							{
								Shader.SetGlobalColor(num, Color.op_Implicit(animatedMaterialProperty4.PointDefinition.Interpolate(1f)));
							}
							else
							{
								StartCoroutine<Vector4>(animatedMaterialProperty4.PointDefinition, (object)num, MaterialPropertyType.Color, duration, time, easing);
							}
						}
						else
						{
							List<float> list2 = ((List<object>)value).Select(Convert.ToSingle).ToList();
							Shader.SetGlobalColor(num, new Color(list2[0], list2[1], list2[2], (list2.Count > 3) ? list2[3] : 1f));
						}
						continue;
					case MaterialPropertyType.Float:
						if (item is AnimatedMaterialProperty<float> animatedMaterialProperty3)
						{
							if (flag)
							{
								Shader.SetGlobalFloat(num, animatedMaterialProperty3.PointDefinition.Interpolate(1f));
							}
							else
							{
								StartCoroutine(animatedMaterialProperty3.PointDefinition, num, MaterialPropertyType.Float, duration, time, easing);
							}
						}
						else
						{
							Shader.SetGlobalFloat(num, Convert.ToSingle(value));
						}
						continue;
					case MaterialPropertyType.Vector:
						if (item is AnimatedMaterialProperty<Vector4> animatedMaterialProperty2)
						{
							if (flag)
							{
								Shader.SetGlobalVector(num, animatedMaterialProperty2.PointDefinition.Interpolate(1f));
							}
							else
							{
								StartCoroutine<Vector4>(animatedMaterialProperty2.PointDefinition, (object)num, MaterialPropertyType.Vector, duration, time, easing);
							}
						}
						else
						{
							List<float> list = ((List<object>)value).Select(Convert.ToSingle).ToList();
							Shader.SetGlobalVector(num, new Vector4(list[0], list[1], list[2], list[3]));
						}
						continue;
					}
				}
				throw new ArgumentOutOfRangeException("type", type, "Type not currently supported.");
			}
		}

		private static void SetGlobalKeyword(string keyword, bool value)
		{
			if (value)
			{
				Shader.EnableKeyword(keyword);
			}
			else
			{
				Shader.DisableKeyword(keyword);
			}
		}

		private IEnumerator AnimateGlobalPropertyCoroutine<T>(PointDefinition<T> points, object id, MaterialPropertyType type, float duration, float startTime, Functions easing) where T : struct
		{
			while (true)
			{
				float num = _audioTimeSource.songTime - startTime;
				if (!(num < duration))
				{
					break;
				}
				float time = Easings.Interpolate(Mathf.Min(num / duration, 1f), easing);
				if (!(id is int num2))
				{
					if (id is string keyword && type == MaterialPropertyType.Keyword)
					{
						SetGlobalKeyword(keyword, (points as PointDefinition<float>).Interpolate(time) >= 1f);
					}
				}
				else
				{
					switch (type)
					{
					case MaterialPropertyType.Color:
						Shader.SetGlobalColor(num2, Color.op_Implicit((points as PointDefinition<Vector4>).Interpolate(time)));
						break;
					case MaterialPropertyType.Float:
						Shader.SetGlobalFloat(num2, (points as PointDefinition<float>).Interpolate(time));
						break;
					case MaterialPropertyType.Vector:
						Shader.SetGlobalVector(num2, (points as PointDefinition<Vector4>).Interpolate(time));
						break;
					}
				}
				yield return null;
			}
		}

		private void StartCoroutine<T>(PointDefinition<T> points, object id, MaterialPropertyType type, float duration, float startTime, Functions easing) where T : struct
		{
			((MonoBehaviour)_coroutineDummy).StartCoroutine(AnimateGlobalPropertyCoroutine(points, id, type, duration, startTime, easing));
		}
	}
	[CustomEvent(new string[] { "SetMaterialProperty" })]
	internal class SetMaterialProperty : ICustomEvent
	{
		private readonly AssetBundleManager _assetBundleManager;

		private readonly IAudioTimeSource _audioTimeSource;

		private readonly IBpmController _bpmController;

		private readonly CoroutineDummy _coroutineDummy;

		private readonly DeserializedData _deserializedData;

		private SetMaterialProperty(AssetBundleManager assetBundleManager, [Inject(Id = "Vivify")] DeserializedData deserializedData, IAudioTimeSource audioTimeSource, IBpmController bpmController, CoroutineDummy coroutineDummy)
		{
			_assetBundleManager = assetBundleManager;
			_deserializedData = deserializedData;
			_audioTimeSource = audioTimeSource;
			_bpmController = bpmController;
			_coroutineDummy = coroutineDummy;
		}

		public void Callback(CustomEventData customEventData)
		{
			if (_deserializedData.Resolve<SetMaterialPropertyData>(customEventData, out SetMaterialPropertyData result))
			{
				float duration = result.Duration;
				duration = 60f * duration / _bpmController.currentBpm;
				if (_assetBundleManager.TryGetAsset<Material>(result.Asset, out Material asset))
				{
					List<MaterialProperty> properties = result.Properties;
					SetMaterialProperties(asset, properties, duration, result.Easing, ((BeatmapDataItem)customEventData).time);
				}
			}
		}

		internal void SetMaterialProperties(Material material, List<MaterialProperty> properties, float duration, Functions easing, float startTime)
		{
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			foreach (MaterialProperty property in properties)
			{
				MaterialPropertyType type = property.Type;
				object value = property.Value;
				bool flag = duration == 0f || startTime + duration < _audioTimeSource.songTime;
				object id = property.Id;
				if (!(id is int num))
				{
					if (id is string text && type == MaterialPropertyType.Keyword)
					{
						if (property is AnimatedMaterialProperty<float> animatedMaterialProperty)
						{
							if (flag)
							{
								SetKeyword(material, text, animatedMaterialProperty.PointDefinition.Interpolate(1f) >= 1f);
							}
							else
							{
								StartCoroutine(animatedMaterialProperty.PointDefinition, material, text, MaterialPropertyType.Float, duration, startTime, easing);
							}
						}
						else
						{
							SetKeyword(material, text, (bool)value);
						}
						continue;
					}
				}
				else
				{
					switch (type)
					{
					case MaterialPropertyType.Texture:
					{
						string assetName = Convert.ToString(value);
						if (_assetBundleManager.TryGetAsset<Texture>(assetName, out Texture asset))
						{
							material.SetTexture(num, asset);
						}
						continue;
					}
					case MaterialPropertyType.Color:
						if (property is AnimatedMaterialProperty<Vector4> animatedMaterialProperty4)
						{
							if (flag)
							{
								material.SetColor(num, Color.op_Implicit(animatedMaterialProperty4.PointDefinition.Interpolate(1f)));
							}
							else
							{
								StartCoroutine<Vector4>(animatedMaterialProperty4.PointDefinition, material, (object)num, MaterialPropertyType.Color, duration, startTime, easing);
							}
						}
						else
						{
							List<float> list2 = ((List<object>)value).Select(Convert.ToSingle).ToList();
							material.SetColor(num, new Color(list2[0], list2[1], list2[2], (list2.Count > 3) ? list2[3] : 1f));
						}
						continue;
					case MaterialPropertyType.Float:
						if (property is AnimatedMaterialProperty<float> animatedMaterialProperty3)
						{
							if (flag)
							{
								material.SetFloat(num, animatedMaterialProperty3.PointDefinition.Interpolate(1f));
							}
							else
							{
								StartCoroutine(animatedMaterialProperty3.PointDefinition, material, num, MaterialPropertyType.Float, duration, startTime, easing);
							}
						}
						else
						{
							material.SetFloat(num, Convert.ToSingle(value));
						}
						continue;
					case MaterialPropertyType.Vector:
						if (property is AnimatedMaterialProperty<Vector4> animatedMaterialProperty2)
						{
							if (flag)
							{
								material.SetVector(num, animatedMaterialProperty2.PointDefinition.Interpolate(1f));
							}
							else
							{
								StartCoroutine<Vector4>(animatedMaterialProperty2.PointDefinition, material, (object)num, MaterialPropertyType.Vector, duration, startTime, easing);
							}
						}
						else
						{
							List<float> list = ((List<object>)value).Select(Convert.ToSingle).ToList();
							material.SetVector(num, new Vector4(list[0], list[1], list[2], list[3]));
						}
						continue;
					}
				}
				throw new ArgumentOutOfRangeException("type", type, "Type not currently supported.");
			}
		}

		private static void SetKeyword(Material material, string keyword, bool value)
		{
			if (value)
			{
				material.EnableKeyword(keyword);
			}
			else
			{
				material.DisableKeyword(keyword);
			}
		}

		private IEnumerator AnimatePropertyCoroutine<T>(PointDefinition<T> points, Material material, object id, MaterialPropertyType type, float duration, float startTime, Functions easing) where T : struct
		{
			while (true)
			{
				float num = _audioTimeSource.songTime - startTime;
				if (!(num < duration))
				{
					break;
				}
				float time = Easings.Interpolate(Mathf.Min(num / duration, 1f), easing);
				if (!(id is int num2))
				{
					if (id is string keyword && type == MaterialPropertyType.Keyword)
					{
						SetKeyword(material, keyword, (points as PointDefinition<float>).Interpolate(time) >= 1f);
					}
				}
				else
				{
					switch (type)
					{
					case MaterialPropertyType.Color:
						material.SetColor(num2, Color.op_Implicit((points as PointDefinition<Vector4>).Interpolate(time)));
						break;
					case MaterialPropertyType.Float:
						material.SetFloat(num2, (points as PointDefinition<float>).Interpolate(time));
						break;
					case MaterialPropertyType.Vector:
						material.SetVector(num2, (points as PointDefinition<Vector4>).Interpolate(time));
						break;
					}
				}
				yield return null;
			}
		}

		private void StartCoroutine<T>(PointDefinition<T> points, Material material, object id, MaterialPropertyType type, float duration, float startTime, Functions easing) where T : struct
		{
			((MonoBehaviour)_coroutineDummy).StartCoroutine(AnimatePropertyCoroutine(points, material, id, type, duration, startTime, easing));
		}
	}
	[CustomEvent(new string[] { "SetRenderingSettings" })]
	internal class SetRenderingSettings : ICustomEvent, IInitializable, IDisposable
	{
		private interface ISettingHandler
		{
			void Capture();

			void Handle(SetRenderingSettings instance, RenderingSettingsProperty property, bool noDuration, float duration, Functions easing, float startTime);

			void Reset();
		}

		private class StructSettingHandler<TSettings, THandled, TProperty> : ISettingHandler where TSettings : class where THandled : struct
		{
			private readonly CapturedSettings<TSettings, TProperty> _capturedSetting;

			internal StructSettingHandler(CapturedSettings<TSettings, TProperty> capturedSetting)
			{
				_capturedSetting = capturedSetting;
			}

			public void Capture()
			{
				_capturedSetting.Capture();
			}

			public void Handle(SetRenderingSettings instance, RenderingSettingsProperty property, bool noDuration, float duration, Functions easing, float startTime)
			{
				if (!(property is AnimatedRenderingSettingsProperty<THandled> animatedRenderingSettingsProperty))
				{
					if (!(property is RenderingSettingsProperty<THandled> renderingSettingsProperty))
					{
						throw new InvalidOperationException("Could not handle type [" + property.GetType().FullName + "].");
					}
					_capturedSetting.Set(renderingSettingsProperty.Value);
					DynamicGI.UpdateEnvironment();
				}
				else if (noDuration)
				{
					_capturedSetting.Set(animatedRenderingSettingsProperty.PointDefinition.Interpolate(1f));
				}
				else
				{
					AnimatedRenderingSettingsProperty<THandled> animatedRenderingSettingsProperty2 = animatedRenderingSettingsProperty;
					instance.StartCoroutine(animatedRenderingSettingsProperty2.PointDefinition, _capturedSetting.Set, duration, startTime, easing);
				}
			}

			public void Reset()
			{
				_capturedSetting.Reset();
			}
		}

		private class ClassSettingHandler<THandled, TProperty> : ISettingHandler where THandled : class
		{
			private readonly CapturedSettings<RenderSettings, TProperty> _capturedSetting;

			internal ClassSettingHandler(CapturedSettings<RenderSettings, TProperty> capturedSetting)
			{
				_capturedSetting = capturedSetting;
			}

			public void Capture()
			{
				_capturedSetting.Capture();
			}

			public void Handle(SetRenderingSettings instance, RenderingSettingsProperty property, bool noDuration, float duration, Functions easing, float startTime)
			{
				if (property is RenderingSettingsProperty<THandled> renderingSettingsProperty)
				{
					_capturedSetting.Set(renderingSettingsProperty.Value);
					DynamicGI.UpdateEnvironment();
					return;
				}
				throw new InvalidOperationException("Could not handle type [" + property.GetType().FullName + "].");
			}

			public void Reset()
			{
				_capturedSetting.Reset();
			}
		}

		private readonly IAudioTimeSource _audioTimeSource;

		private readonly IBpmController _bpmController;

		private readonly CoroutineDummy _coroutineDummy;

		private readonly DeserializedData _deserializedData;

		private readonly SiraLog _log;

		private readonly Dictionary<string, ISettingHandler> _settings = new Dictionary<string, ISettingHandler>
		{
			{
				"ambientEquatorColor",
				new StructSettingHandler<RenderSettings, Vector4, Color>(new RenderColorCapturedSetting("ambientEquatorColor"))
			},
			{
				"ambientGroundColor",
				new StructSettingHandler<RenderSettings, Vector4, Color>(new RenderColorCapturedSetting("ambientGroundColor"))
			},
			{
				"ambientIntensity",
				new StructSettingHandler<RenderSettings, float, float>(new RenderFloatCapturedSetting("ambientIntensity"))
			},
			{
				"ambientLight",
				new StructSettingHandler<RenderSettings, Vector4, Color>(new RenderColorCapturedSetting("ambientLight"))
			},
			{
				"ambientMode",
				new StructSettingHandler<RenderSettings, float, AmbientMode>(new RenderEnumCapturedSetting<AmbientMode>("ambientMode"))
			},
			{
				"ambientSkyColor",
				new StructSettingHandler<RenderSettings, Vector4, Color>(new RenderColorCapturedSetting("ambientSkyColor"))
			},
			{
				"defaultReflectionMode",
				new StructSettingHandler<RenderSettings, float, DefaultReflectionMode>(new RenderEnumCapturedSetting<DefaultReflectionMode>("defaultReflectionMode"))
			},
			{
				"defaultReflectionResolution",
				new StructSettingHandler<RenderSettings, float, int>(new RenderIntCapturedSetting("defaultReflectionResolution"))
			},
			{
				"flareFadeSpeed",
				new StructSettingHandler<RenderSettings, float, float>(new RenderFloatCapturedSetting("flareFadeSpeed"))
			},
			{
				"flareStrength",
				new StructSettingHandler<RenderSettings, float, float>(new RenderFloatCapturedSetting("flareStrength"))
			},
			{
				"fog",
				new StructSettingHandler<RenderSettings, float, bool>(new RenderBoolCapturedSetting("fog"))
			},
			{
				"fogColor",
				new StructSettingHandler<RenderSettings, Vector4, Color>(new RenderColorCapturedSetting("fogColor"))
			},
			{
				"fogDensity",
				new StructSettingHandler<RenderSettings, float, float>(new RenderFloatCapturedSetting("fogDensity"))
			},
			{
				"fogEndDistance",
				new StructSettingHandler<RenderSettings, float, float>(new RenderFloatCapturedSetting("fogEndDistance"))
			},
			{
				"fogMode",
				new StructSettingHandler<RenderSettings, float, FogMode>(new RenderEnumCapturedSetting<FogMode>("fogMode"))
			},
			{
				"fogStartDistance",
				new StructSettingHandler<RenderSettings, float, float>(new RenderFloatCapturedSetting("fogStartDistance"))
			},
			{
				"haloStrength",
				new StructSettingHandler<RenderSettings, float, float>(new RenderFloatCapturedSetting("haloStrength"))
			},
			{
				"reflectionBounces",
				new StructSettingHandler<RenderSettings, float, int>(new RenderIntCapturedSetting("reflectionBounces"))
			},
			{
				"reflectionIntensity",
				new StructSettingHandler<RenderSettings, float, float>(new RenderFloatCapturedSetting("reflectionIntensity"))
			},
			{
				"subtractiveShadowColor",
				new StructSettingHandler<RenderSettings, Vector4, Color>(new RenderColorCapturedSetting("subtractiveShadowColor"))
			},
			{
				"anisotropicFiltering",
				new StructSettingHandler<QualitySettings, float, AnisotropicFiltering>(new QualityEnumCapturedSetting<AnisotropicFiltering>("anisotropicFiltering"))
			},
			{
				"antiAliasing",
				new StructSettingHandler<QualitySettings, float, int>(new QualityIntCapturedSetting("antiAliasing"))
			},
			{
				"pixelLightCount",
				new StructSettingHandler<QualitySettings, float, int>(new QualityIntCapturedSetting("pixelLightCount"))
			},
			{
				"realtimeReflectionProbes",
				new StructSettingHandler<QualitySettings, float, bool>(new QualityBoolCapturedSetting("realtimeReflectionProbes"))
			},
			{
				"shadowCascades",
				new StructSettingHandler<QualitySettings, float, int>(new QualityIntCapturedSetting("shadowCascades"))
			},
			{
				"shadowDistance",
				new StructSettingHandler<QualitySettings, float, float>(new QualityFloatCapturedSetting("shadowDistance"))
			},
			{
				"shadowmaskMode",
				new StructSettingHandler<QualitySettings, float, ShadowmaskMode>(new QualityEnumCapturedSetting<ShadowmaskMode>("shadowmaskMode"))
			},
			{
				"shadowNearPlaneOffset",
				new StructSettingHandler<QualitySettings, float, float>(new QualityFloatCapturedSetting("shadowNearPlaneOffset"))
			},
			{
				"shadowProjection",
				new StructSettingHandler<QualitySettings, float, ShadowProjection>(new QualityEnumCapturedSetting<ShadowProjection>("shadowProjection"))
			},
			{
				"shadowResolution",
				new StructSettingHandler<QualitySettings, float, ShadowResolution>(new QualityEnumCapturedSetting<ShadowResolution>("shadowResolution"))
			},
			{
				"shadows",
				new StructSettingHandler<QualitySettings, float, ShadowQuality>(new QualityEnumCapturedSetting<ShadowQuality>("shadows"))
			},
			{
				"softParticles",
				new StructSettingHandler<QualitySettings, float, bool>(new QualityBoolCapturedSetting("softParticles"))
			},
			{
				"useOcclusionMesh",
				new StructSettingHandler<XRSettingsSetter, float, bool>(new BoolCapturedSetting<XRSettingsSetter>("useOcclusionMesh"))
			}
		};

		private SetRenderingSettings(SiraLog log, [Inject(Id = "Vivify")] DeserializedData deserializedData, IAudioTimeSource audioTimeSource, IBpmController bpmController, CoroutineDummy coroutineDummy, AssetBundleManager assetBundleManager, PrefabManager prefabManager)
		{
			_log = log;
			_deserializedData = deserializedData;
			_audioTimeSource = audioTimeSource;
			_bpmController = bpmController;
			_coroutineDummy = coroutineDummy;
			_settings.Add("skybox", new ClassSettingHandler<string, Material>(new RenderMaterialCapturedSetting("skybox", assetBundleManager)));
			_settings.Add("sun", new ClassSettingHandler<string, Light>(new RenderLightCapturedSetting("sun", prefabManager)));
		}

		public void Callback(CustomEventData customEventData)
		{
			if (_deserializedData.Resolve<SetRenderingSettingsData>(customEventData, out SetRenderingSettingsData result))
			{
				float duration = result.Duration;
				duration = 60f * duration / _bpmController.currentBpm;
				List<RenderingSettingsProperty> properties = result.Properties;
				SetRenderSettings(properties, duration, result.Easing, ((BeatmapDataItem)customEventData).time);
			}
		}

		public void Dispose()
		{
			foreach (ISettingHandler value in _settings.Values)
			{
				value.Reset();
			}
		}

		public void Initialize()
		{
			foreach (ISettingHandler value in _settings.Values)
			{
				value.Capture();
			}
		}

		internal void SetRenderSettings(List<RenderingSettingsProperty> properties, float duration, Functions easing, float startTime)
		{
			foreach (RenderingSettingsProperty property in properties)
			{
				string name = property.Name;
				_log.Debug("Setting [" + name + "]");
				bool noDuration = duration == 0f || startTime + duration < _audioTimeSource.songTime;
				if (_settings.TryGetValue(name, out ISettingHandler value))
				{
					value.Handle(this, property, noDuration, duration, easing, startTime);
				}
			}
		}

		private IEnumerator AnimatePropertyCoroutine<T>(PointDefinition<T> points, Action<object> set, float duration, float startTime, Functions easing) where T : struct
		{
			while (true)
			{
				float num = _audioTimeSource.songTime - startTime;
				if (num < duration)
				{
					float time = Easings.Interpolate(Mathf.Min(num / duration, 1f), easing);
					set(points.Interpolate(time));
					yield return null;
					continue;
				}
				break;
			}
		}

		private void StartCoroutine<T>(PointDefinition<T> points, Action<object> set, float duration, float startTime, Functions easing) where T : struct
		{
			((MonoBehaviour)_coroutineDummy).StartCoroutine(AnimatePropertyCoroutine(points, set, duration, startTime, easing));
		}
	}
}
namespace Vivify.Controllers
{
	[PlayViewControllerSettings(100, "vivify")]
	internal class AssetBundleDownloadViewController : BSMLResourceViewController, IPlayViewController
	{
		private enum View
		{
			None,
			Tos,
			Downloading,
			Error
		}

		internal class AssetDownloader : MonoBehaviour
		{
		}

		[Serializable]
		private class RepoJson
		{
			public string downloadUrl;
		}

		[UIComponent("loadingbar")]
		private VerticalLayoutGroup _barGroup;

		private Config _config;

		private AssetDownloader _assetDownloader;

		private View _currentView;

		private bool _doAbort;

		private uint _downloadChecksum;

		private bool _downloadFinished;

		[UIComponent("downloading")]
		private VerticalLayoutGroup _downloadingGroup;

		private string? _downloadPath;

		private float _downloadProgress;

		private Coroutine? _downloadWaiter;

		[UIComponent("error")]
		private VerticalLayoutGroup _error;

		[UIComponent("errortext")]
		private TMP_Text _errorText;

		private string _lastError = string.Empty;

		private Image _loadingBar;

		private SiraLog _log;

		private View _newView;

		[UIComponent("percentage")]
		private TMP_Text _percentageText;

		[UIComponent("tos")]
		private VerticalLayoutGroup _tosGroup;

		public override string ResourceName => "Vivify.Resources.AssetBundleDownloading.bsml";

		public event Action? Finished;

		public bool Init(StartStandardLevelParameters standardLevelParameters)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			IPreviewMediaData previewMediaData = standardLevelParameters.BeatmapLevel.previewMediaData;
			FileSystemPreviewMediaData val = (FileSystemPreviewMediaData)(object)((previewMediaData is FileSystemPreviewMediaData) ? previewMediaData : null);
			if (val == null)
			{
				return false;
			}
			CustomData beatmapCustomData = standardLevelParameters.BeatmapLevel.GetBeatmapCustomData(standardLevelParameters.BeatmapKey);
			CustomData levelCustomData = standardLevelParameters.BeatmapLevel.GetLevelCustomData();
			if (!(beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>().ToArray() ?? Array.Empty<string>()).Contains("Vivify"))
			{
				return false;
			}
			string text = Path.Combine(Path.GetDirectoryName(val._previewAudioClipPath), "bundleWindows2021.vivify");
			if (File.Exists(text))
			{
				return false;
			}
			uint? num = levelCustomData.Get<CustomData>("_assetBundle")?.Get<uint?>("_windows2021");
			if (!num.HasValue)
			{
				_lastError = "This maps is missing required assets for your game version.\nPlease contact the mapper to update their map to include the assets.";
				_newView = View.Error;
			}
			else
			{
				_log.Error("[" + text + "] not found, attempting to download remotely");
				uint value = num.Value;
				_doAbort = false;
				_downloadFinished = false;
				if (_config.AllowDownload)
				{
					((MonoBehaviour)_assetDownloader).StartCoroutine(DownloadAndSave(text, value));
				}
				else
				{
					_downloadPath = text;
					_downloadChecksum = value;
				}
			}
			return true;
		}

		[UsedImplicitly]
		[Inject]
		private void Construct(SiraLog log, Config config, AssetDownloader assetDownloader)
		{
			_log = log;
			_config = config;
			_assetDownloader = assetDownloader;
			_newView = ((!config.AllowDownload) ? View.Tos : View.Downloading);
		}

		private IEnumerator DownloadAndSave(string savePath, uint checksum)
		{
			_newView = View.Downloading;
			string text = _config.BundleRepository + checksum;
			_log.Debug("Fetching asset bundle info from [" + text + "]");
			UnityWebRequest apiRequest = UnityWebRequest.Get(text);
			try
			{
				apiRequest.SendWebRequest();
				while (!apiRequest.isDone)
				{
					if (!_doAbort)
					{
						yield return null;
						continue;
					}
					apiRequest.Abort();
					_log.Debug("Fetch cancelled");
					yield break;
				}
				if (apiRequest.isNetworkError || apiRequest.isHttpError)
				{
					if (apiRequest.isNetworkError)
					{
						_lastError = "Network error while fetching bundle.\n" + apiRequest.error;
					}
					else if (apiRequest.isHttpError)
					{
						_lastError = $"Server sent error response code while fetching bundle.\n({apiRequest.responseCode})";
					}
					_log.Error(_lastError);
					_newView = View.Error;
					yield break;
				}
				string downloadUrl = JsonUtility.FromJson<RepoJson>(apiRequest.downloadHandler.text).downloadUrl;
				_log.Debug("Attempting to download asset bundle from [" + downloadUrl + "]");
				UnityWebRequest www = UnityWebRequest.Get(downloadUrl);
				try
				{
					www.SendWebRequest();
					while (!www.isDone)
					{
						if (!_doAbort)
						{
							_downloadProgress = www.downloadProgress;
							yield return null;
							continue;
						}
						www.Abort();
						_log.Debug("Download cancelled");
						yield break;
					}
					if (www.isNetworkError || www.isHttpError)
					{
						if (www.isNetworkError)
						{
							_lastError = "Network error while downloading bundle.\n" + www.error;
						}
						else if (www.isHttpError)
						{
							_lastError = $"Server sent error response code while downloading bundle.\n({www.responseCode})";
						}
						_log.Error(_lastError);
						_newView = View.Error;
					}
					else
					{
						_downloadProgress = 1f;
						File.WriteAllBytes(savePath, www.downloadHandler.data);
						_log.Debug("Successfully downloaded bundle to [" + savePath + "]");
						_downloadFinished = true;
					}
				}
				finally
				{
					((IDisposable)www)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)apiRequest)?.Dispose();
			}
		}

		[UsedImplicitly]
		[UIAction("accept-click")]
		private void OnAcceptClick()
		{
			_config.AllowDownload = true;
			if (_downloadPath != null)
			{
				((MonoBehaviour)_assetDownloader).StartCoroutine(DownloadAndSave(_downloadPath, _downloadChecksum));
			}
		}

		[UsedImplicitly]
		private void OnEarlyDismiss()
		{
			_doAbort = true;
			if (_downloadWaiter != null)
			{
				((MonoBehaviour)_assetDownloader).StopCoroutine(_downloadWaiter);
			}
		}

		[UsedImplicitly]
		private void OnShow()
		{
			if (!_downloadFinished)
			{
				((MonoBehaviour)_assetDownloader).StartCoroutine(WaitForDownload());
			}
			else
			{
				this.Finished?.Invoke();
			}
		}

		private void Start()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			Vector2 sizeDelta = default(Vector2);
			((Vector2)(ref sizeDelta))._002Ector(0f, 8f);
			_loadingBar = new GameObject("Loading Bar").AddComponent<Image>();
			RectTransform val = (RectTransform)((Component)_loadingBar).transform;
			((Transform)val).SetParent(((Component)_barGroup).transform, false);
			val.sizeDelta = sizeDelta;
			Texture2D whiteTexture = Texture2D.whiteTexture;
			Sprite sprite = Sprite.Create(whiteTexture, new Rect(0f, 0f, (float)((Texture)whiteTexture).width, (float)((Texture)whiteTexture).height), Vector2.one * 0.5f, 100f, 1u);
			_loadingBar.sprite = sprite;
			_loadingBar.type = (Type)3;
			_loadingBar.fillMethod = (FillMethod)0;
			((Graphic)_loadingBar).color = new Color(1f, 1f, 1f, 0.5f);
			Image obj = new GameObject("Background").AddComponent<Image>();
			RectTransform val2 = (RectTransform)((Component)obj).transform;
			val2.sizeDelta = sizeDelta;
			((Transform)val2).SetParent(((Component)_barGroup).transform, false);
			((Graphic)obj).color = new Color(0f, 0f, 0f, 0.2f);
		}

		private void Update()
		{
			if (_currentView != _newView)
			{
				_currentView = _newView;
				switch (_currentView)
				{
				case View.Tos:
					((Component)_tosGroup).gameObject.SetActive(true);
					((Component)_downloadingGroup).gameObject.SetActive(false);
					((Component)_error).gameObject.SetActive(false);
					break;
				case View.Downloading:
					((Component)_tosGroup).gameObject.SetActive(false);
					((Component)_downloadingGroup).gameObject.SetActive(true);
					((Component)_error).gameObject.SetActive(false);
					break;
				case View.Error:
					((Component)_tosGroup).gameObject.SetActive(false);
					((Component)_downloadingGroup).gameObject.SetActive(false);
					((Component)_error).gameObject.SetActive(true);
					_errorText.text = _lastError;
					break;
				}
			}
			if (_currentView == View.Downloading)
			{
				_loadingBar.fillAmount = _downloadProgress;
				float num = _downloadProgress * 100f;
				_percentageText.text = $"{num:0.0}%";
			}
		}

		private IEnumerator WaitForDownload()
		{
			while (!_downloadFinished)
			{
				yield return null;
			}
			this.Finished?.Invoke();
		}
	}
	[RequireComponent(typeof(Camera))]
	internal class CameraPropertyController : MonoBehaviour
	{
		private Camera _camera;

		private DepthTextureMode _cachedDepthTextureMode;

		private CameraClearFlags _cachedClearFlags;

		private Color _cachedBackgroundColor;

		private CullingCameraController _cullingCameraController;

		private BloomPrePass _bloomPrePass;

		private SettingsManager _settingsManager;

		private bool _injected;

		private DepthTextureController? _depthTextureController;

		internal DepthTextureMode? DepthTextureMode
		{
			set
			{
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_004e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				if (!value.HasValue)
				{
					if ((Object)(object)_depthTextureController != (Object)null)
					{
						_depthTextureController.Init(_settingsManager);
					}
					else
					{
						_camera.depthTextureMode = _cachedDepthTextureMode;
					}
				}
				else
				{
					_camera.depthTextureMode = (DepthTextureMode)(value.Value | _cachedDepthTextureMode);
				}
			}
		}

		internal CameraClearFlags? ClearFlags
		{
			set
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				_camera.clearFlags = (CameraClearFlags)(((_003F?)value) ?? _cachedClearFlags);
			}
		}

		internal Color? BackgroundColor
		{
			set
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				_camera.backgroundColor = (Color)(((_003F?)value) ?? _cachedBackgroundColor);
			}
		}

		internal CullingTextureTracker? CullingTextureData
		{
			set
			{
				_cullingCameraController.CullingTextureData = value;
			}
		}

		internal bool? BloomPrePass
		{
			set
			{
				((Behaviour)_bloomPrePass).enabled = value ?? true;
			}
		}

		internal bool? MainEffect
		{
			set
			{
				_cullingCameraController.MainEffect = value ?? true;
			}
		}

		internal string? Id { get; set; }

		internal void Reset()
		{
			DepthTextureMode = null;
			ClearFlags = null;
			BackgroundColor = null;
			CullingTextureData = null;
			BloomPrePass = null;
			MainEffect = null;
		}

		[UsedImplicitly]
		[Inject]
		private void Construct(SettingsManager settingsManager)
		{
			_settingsManager = settingsManager;
			_injected = true;
			if (((Behaviour)this).isActiveAndEnabled)
			{
				OnEnable();
			}
		}

		private void Awake()
		{
			_camera = ((Component)this).GetComponent<Camera>();
			_depthTextureController = ((Component)this).GetComponent<DepthTextureController>();
			_cullingCameraController = ((Component)this).GetComponent<CullingCameraController>();
			_bloomPrePass = ((Component)this).GetComponent<BloomPrePass>();
		}

		private void OnEnable()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if (_injected)
			{
				_cachedDepthTextureMode = _camera.depthTextureMode;
				_cachedClearFlags = _camera.clearFlags;
				_cachedBackgroundColor = _camera.backgroundColor;
				CameraPropertyManager.AddControllerStatic(this);
			}
		}

		private void OnDisable()
		{
			CameraPropertyManager.RemoveControllerStatic(this);
		}
	}
	[RequireComponent(typeof(Camera))]
	internal class MultipassKeywordController : MonoBehaviour
	{
		private const string MULTIPASS_KEYWORD = "MULTIPASS_ENABLED";

		private const string MULTIPASS_EYE_KEY = "_StereoActiveEye";

		private static readonly int _multipassEye = Shader.PropertyToID("_StereoActiveEye");

		private Camera _camera;

		private void Awake()
		{
			_camera = ((Component)this).GetComponent<Camera>();
		}

		private void OnPreRender()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Invalid comparison between Unknown and I4
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected I4, but got Unknown
			if (_camera.stereoEnabled && (int)_camera.stereoTargetEye != 0 && (int)OpenXRSettings.Instance.renderMode == 0)
			{
				Shader.EnableKeyword("MULTIPASS_ENABLED");
			}
			else
			{
				Shader.DisableKeyword("MULTIPASS_ENABLED");
			}
			Shader.SetGlobalInt(_multipassEye, (int)_camera.stereoActiveEye);
		}
	}
	internal class RendererController : MonoBehaviour
	{
		internal Renderer[] ChildRenderers { get; private set; } = Array.Empty<Renderer>();

		internal event Action<RendererController>? OnDestroyed;

		internal event Action? OnTransformChanged;

		private void OnDestroy()
		{
			this.OnDestroyed?.Invoke(this);
		}

		private void OnEnable()
		{
			UpdateChildRenderers();
		}

		private void OnTransformChildrenChanged()
		{
			UpdateChildRenderers();
			this.OnTransformChanged?.Invoke();
		}

		private void UpdateChildRenderers()
		{
			ChildRenderers = ((Component)((Component)this).transform).GetComponentsInChildren<Renderer>(true);
		}
	}
}
namespace Vivify.Controllers.Sync
{
	[RequireComponent(typeof(Animator))]
	internal class AnimatorSyncController : SyncController
	{
		private Animator _animator;

		public override void Sync(float speed)
		{
			_animator.speed = speed;
		}

		private void Awake()
		{
			_animator = ((Component)this).GetComponent<Animator>();
			_animator.updateMode = (AnimatorUpdateMode)0;
			_animator.Update(base.SongTime);
		}
	}
	internal interface ISync
	{
		void SetStartTime(float time);
	}
	[RequireComponent(typeof(ParticleSystem))]
	internal class ParticleSystemSyncController : SyncController
	{
		private ParticleSystem _particleSystem;

		public override void Sync(float speed)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			MainModule main = _particleSystem.main;
			((MainModule)(ref main)).simulationSpeed = speed;
		}

		private void Awake()
		{
			_particleSystem = ((Component)this).GetComponent<ParticleSystem>();
		}
	}
	internal abstract class SyncController : MonoBehaviour, ISync
	{
		private AudioTimeSyncController _audioTimeSyncController;

		protected float SongTime { get; private set; }

		public void SetStartTime(float time)
		{
			SongTime = time;
		}

		public abstract void Sync(float speed);

		[Inject]
		[UsedImplicitly]
		private void Construct(float startTime, AudioTimeSyncController audioTimeSyncController)
		{
			SongTime = startTime;
			_audioTimeSyncController = audioTimeSyncController;
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			float songTime = _audioTimeSyncController.songTime;
			float num = songTime - SongTime;
			SongTime = songTime;
			if (deltaTime > 0f && num > 0f)
			{
				Sync(num / deltaTime);
			}
			else
			{
				Sync(0f);
			}
		}
	}
	internal static class SyncExtensions
	{
		internal static void SongSynchronize(this IInstantiator instantiator, GameObject gameObject, float startTime)
		{
			ISync[] componentsInChildren = gameObject.GetComponentsInChildren<ISync>();
			if (componentsInChildren.Length != 0)
			{
				ISync[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetStartTime(startTime);
				}
				return;
			}
			CollectionExtensions.Do<Animator>((IEnumerable<Animator>)gameObject.GetComponentsInChildren<Animator>(), (Action<Animator>)delegate(Animator n)
			{
				instantiator.InstantiateComponent<AnimatorSyncController>(((Component)n).gameObject, (IEnumerable<object>)new global::_003C_003Ez__ReadOnlySingleElementList<object>(startTime));
			});
			CollectionExtensions.Do<ParticleSystem>((IEnumerable<ParticleSystem>)gameObject.GetComponentsInChildren<ParticleSystem>(), (Action<ParticleSystem>)delegate(ParticleSystem n)
			{
				instantiator.InstantiateComponent<ParticleSystemSyncController>(((Component)n).gameObject, (IEnumerable<object>)new global::_003C_003Ez__ReadOnlySingleElementList<object>(startTime));
			});
			CollectionExtensions.Do<VideoPlayer>((IEnumerable<VideoPlayer>)gameObject.GetComponentsInChildren<VideoPlayer>(), (Action<VideoPlayer>)delegate(VideoPlayer n)
			{
				if (n.playOnAwake)
				{
					instantiator.InstantiateComponent<VideoPlayerSyncController>(((Component)n).gameObject, (IEnumerable<object>)new global::_003C_003Ez__ReadOnlySingleElementList<object>(startTime));
				}
			});
		}
	}
	[RequireComponent(typeof(VideoPlayer))]
	internal class VideoPlayerSyncController : MonoBehaviour, ISync
	{
		private AudioTimeSyncController _audioTimeSyncController;

		private SiraLog _log;

		private bool _seeking;

		private float _startTime;

		private VideoPlayer _videoPlayer;

		private float SongTime => _audioTimeSyncController.songTime - _startTime;

		public void SetStartTime(float time)
		{
			_startTime = time;
		}

		private void Awake()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			_videoPlayer = ((Component)this).GetComponent<VideoPlayer>();
			_videoPlayer.errorReceived += new ErrorEventHandler(OnErrorRecieved);
			_videoPlayer.prepareCompleted += new EventHandler(OnPrepareCompleted);
			_videoPlayer.skipOnDrop = false;
		}

		[Inject]
		[UsedImplicitly]
		private void Construct(SiraLog log, float startTime, AudioTimeSyncController audioTimeSyncController)
		{
			_log = log;
			_startTime = startTime;
			_audioTimeSyncController = audioTimeSyncController;
			audioTimeSyncController.stateChangedEvent += OnStateChange;
		}

		private void OnDestroy()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			if ((Object)(object)_videoPlayer != (Object)null)
			{
				_videoPlayer.errorReceived -= new ErrorEventHandler(OnErrorRecieved);
				_videoPlayer.prepareCompleted -= new EventHandler(OnPrepareCompleted);
			}
			if ((Object)(object)_audioTimeSyncController != (Object)null)
			{
				_audioTimeSyncController.stateChangedEvent -= OnStateChange;
			}
		}

		private void OnEnable()
		{
			((MonoBehaviour)this).StartCoroutine(Prepare());
		}

		private void OnErrorRecieved(VideoPlayer _, string error)
		{
			_log.Error(error);
		}

		private void OnPrepareCompleted(VideoPlayer _)
		{
			OnStateChange();
		}

		private void OnSeekCompleted(VideoPlayer _)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			_videoPlayer.seekCompleted -= new EventHandler(OnSeekCompleted);
			((MonoBehaviour)this).StartCoroutine(SeekCompleteDelay());
		}

		private void OnStateChange()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected I4, but got Unknown
			if (_videoPlayer.isPrepared)
			{
				State state = _audioTimeSyncController.state;
				switch ((int)state)
				{
				case 0:
					ResyncTime();
					_videoPlayer.Play();
					break;
				case 1:
					_videoPlayer.Pause();
					break;
				case 2:
					_videoPlayer.Stop();
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		private IEnumerator Prepare()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)_videoPlayer != (Object)null && ((Behaviour)_videoPlayer).isActiveAndEnabled));
			_videoPlayer.Prepare();
		}

		private void ResyncTime()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			if (!_seeking)
			{
				_seeking = true;
				_videoPlayer.playbackSpeed = _audioTimeSyncController.timeScale;
				_videoPlayer.seekCompleted += new EventHandler(OnSeekCompleted);
				_videoPlayer.time = SongTime;
			}
		}

		private IEnumerator SeekCompleteDelay()
		{
			yield return (object)new WaitForEndOfFrame();
			_seeking = false;
		}

		private void Update()
		{
			if (_videoPlayer.isPrepared && Math.Abs(_videoPlayer.time - (double)SongTime) > 0.2)
			{
				ResyncTime();
			}
		}
	}
}
namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class IgnoresAccessChecksToAttribute : Attribute
	{
		public IgnoresAccessChecksToAttribute(string assemblyName)
		{
		}
	}
}
[CompilerGenerated]
internal sealed class _003C_003Ez__ReadOnlySingleElementList<T> : IEnumerable, ICollection, IList, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection<T>, IList<T>
{
	private sealed class Enumerator : IDisposable, IEnumerator, IEnumerator<T>
	{
		[CompilerGenerated]
		private readonly T _item;

		[CompilerGenerated]
		private bool _moveNextCalled;

		object IEnumerator.Current => _item;

		T IEnumerator<T>.Current => _item;

		public Enumerator(T item)
		{
			_item = item;
		}

		bool IEnumerator.MoveNext()
		{
			if (!_moveNextCalled)
			{
				return _moveNextCalled = true;
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			_moveNextCalled = false;
		}

		void IDisposable.Dispose()
		{
		}
	}

	[CompilerGenerated]
	private readonly T _item;

	int ICollection.Count => 1;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	object IList.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	int IReadOnlyCollection<T>.Count => 1;

	T IReadOnlyList<T>.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
	}

	int ICollection<T>.Count => 1;

	bool ICollection<T>.IsReadOnly => true;

	T IList<T>.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public _003C_003Ez__ReadOnlySingleElementList(T item)
	{
		_item = item;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_item);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		array.SetValue(_item, index);
	}

	int IList.Add(object value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object value)
	{
		return EqualityComparer<T>.Default.Equals(_item, (T)value);
	}

	int IList.IndexOf(object value)
	{
		if (!EqualityComparer<T>.Default.Equals(_item, (T)value))
		{
			return -1;
		}
		return 0;
	}

	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException();
	}

	void IList.Remove(object value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(_item);
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Contains(T item)
	{
		return EqualityComparer<T>.Default.Equals(_item, item);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		array[arrayIndex] = _item;
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	int IList<T>.IndexOf(T item)
	{
		if (!EqualityComparer<T>.Default.Equals(_item, item))
		{
			return -1;
		}
		return 0;
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}
}

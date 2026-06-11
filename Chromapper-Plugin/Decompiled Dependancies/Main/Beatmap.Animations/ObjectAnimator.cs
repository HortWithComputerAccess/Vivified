using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Containers;
using Beatmap.Enums;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Animations;

public class ObjectAnimator : MonoBehaviour
{
	public enum TargetTypes
	{
		None,
		GameplayObject,
		Transform,
		Material
	}

	public class Aggregator<T> where T : struct
	{
		public int Count;

		public Func<T, T, T> func;

		public T _default;

		public int Keep;

		private T[] items = new T[4];

		public Aggregator(T _default, Func<T, T, T> func)
		{
			this._default = _default;
			this.func = func;
		}

		public void Add(T v)
		{
			if (Count < 4)
			{
				items[Count] = v;
				Count++;
			}
		}

		public void Preload(T v)
		{
			Add(v);
			Keep++;
		}

		public T Get()
		{
			if (Count == 0)
			{
				return _default;
			}
			T val = items[0];
			for (int i = 1; i < Count; i++)
			{
				val = func(val, items[i]);
			}
			Count = Keep;
			return val;
		}
	}

	[SerializeField]
	public GameObject AnimationThis;

	[SerializeField]
	private ObjectContainer container;

	public Track AnimationTrack;

	public AudioTimeSyncController Atsc;

	public TracksManager TracksManager;

	[SerializeField]
	public Transform LocalTarget;

	public Transform WorldTarget;

	public Aggregator<Quaternion> LocalRotation;

	public Aggregator<Quaternion> WorldRotation;

	public Aggregator<Vector3> OffsetPosition;

	public Aggregator<Vector3> WorldPosition;

	public Aggregator<Vector3> Scale;

	public Aggregator<Color> Colors;

	public Aggregator<float> Opacity;

	public Aggregator<float> OpacityArrow;

	public bool ShouldRecycle;

	public TargetTypes TargetType;

	public string ColorKeyword = "_Color";

	private List<TrackAnimator> tracks = new List<TrackAnimator>();

	public Dictionary<string, IAnimateProperty> AnimatedProperties = new Dictionary<string, IAnimateProperty>();

	private IAnimateProperty[] properties = new IAnimateProperty[0];

	private static readonly int opaqueAlpha = Shader.PropertyToID("_OpaqueAlpha");

	private static readonly int animationSpawned = Shader.PropertyToID("_AnimationSpawned");

	private static readonly int alwaysOpaque = Shader.PropertyToID("_AlwaysOpaque");

	private float? _time;

	private float time_begin;

	private float time_end;

	private static float minWall = 0.06f;

	public bool AnimatedTrack { get; private set; }

	public bool AnimatedLife { get; private set; }

	public void ResetData()
	{
		AnimatedProperties = new Dictionary<string, IAnimateProperty>();
		properties = new IAnimateProperty[0];
		TargetType = TargetTypes.None;
		OnDisable();
		if (AnimatedTrack)
		{
			if (container.transform.IsChildOf(AnimationTrack.transform))
			{
				TracksManager.GetTrackAtTime(container.ObjectData?.SongBpmTime ?? 0f).AttachContainer(container);
			}
			UnityEngine.Object.Destroy(AnimationTrack.gameObject);
			AnimationTrack = null;
			AnimatedTrack = false;
		}
		LocalRotation = new Aggregator<Quaternion>(Quaternion.identity, (Quaternion a, Quaternion b) => a * b);
		WorldRotation = new Aggregator<Quaternion>(Quaternion.identity, (Quaternion a, Quaternion b) => a * b);
		OffsetPosition = new Aggregator<Vector3>(Vector3.zero, (Vector3 a, Vector3 b) => a + b);
		WorldPosition = new Aggregator<Vector3>(Vector3.zero, (Vector3 a, Vector3 b) => a + b);
		Scale = new Aggregator<Vector3>(Vector3.one, (Vector3 a, Vector3 b) => Vector3.Scale(a, b));
		Colors = new Aggregator<Color>(container?.MaterialPropertyBlock?.GetColor(ColorKeyword) ?? Color.white, (Color a, Color b) => a * b);
		Opacity = new Aggregator<float>(1f, (float a, float b) => a * b);
		OpacityArrow = new Aggregator<float>(1f, (float a, float b) => a * b);
		_time = null;
		AnimatedLife = false;
		ShouldRecycle = false;
		if (LocalTarget != null)
		{
			LocalTarget.localEulerAngles = Vector3.zero;
			LocalTarget.localPosition = Vector3.zero;
			LocalTarget.localScale = Vector3.one;
		}
		if (container != null && !(container is GeometryContainer))
		{
			container.UpdateGridPosition();
			container.MaterialPropertyBlock.SetFloat(opaqueAlpha, 1f);
			container.MaterialPropertyBlock.SetFloat(animationSpawned, 0f);
			container.MaterialPropertyBlock.SetFloat(alwaysOpaque, 0f);
			if (container is NoteContainer noteContainer)
			{
				noteContainer.ArrowMaterialPropertyBlock.SetFloat(opaqueAlpha, 1f);
				noteContainer.DirectionTarget.localPosition = Vector3.zero;
			}
			container.UpdateMaterials();
		}
	}

	private void OnDisable()
	{
		if (Atsc != null)
		{
			AudioTimeSyncController atsc = Atsc;
			atsc.TimeChanged = (Action)Delegate.Remove(atsc.TimeChanged, new Action(OnTimeChanged));
		}
		foreach (TrackAnimator track in tracks)
		{
			track.RemoveChild(this);
		}
		tracks.Clear();
	}

	public void AttachToObject(BaseGrid obj)
	{
		ResetData();
		TargetType = TargetTypes.GameplayObject;
		ColorKeyword = ((container is ObstacleContainer) ? "_ColorTint" : "_Color");
		base.enabled = UIMode.AnimationMode && TracksManager != null;
		if (!base.enabled)
		{
			return;
		}
		obj.RecomputeSpawnParameters();
		float num = 0f;
		if (container is ObstacleContainer obstacleContainer)
		{
			num = obstacleContainer.ObstacleData.DurationSongBpm;
			var (vector, v) = obstacleContainer.ReadSizePosition();
			v -= new Vector3(0f, 0f, 0.4f);
			OffsetPosition.Preload(v);
			Scale.Preload(new Vector3(WallClamp(vector.x), WallClamp(vector.y), WallClamp(vector.z)));
		}
		JSONNode customLocalRotation = obj.CustomLocalRotation;
		if ((object)customLocalRotation != null)
		{
			LocalRotation.Preload(Quaternion.Euler(customLocalRotation.ReadVector3()));
		}
		if (obj.CustomWorldRotation is JSONArray jSONArray)
		{
			WorldRotation.Preload(Quaternion.Euler(jSONArray.ReadVector3()));
		}
		if (obj.CustomWorldRotation is JSONNumber jSONNumber)
		{
			WorldRotation.Preload(Quaternion.Euler(0f, jSONNumber, 0f));
		}
		time_begin = obj.SpawnSongBpmTime;
		time_end = obj.SongBpmTime + num + obj.Hjd;
		RequireAnimationTrack();
		WorldTarget = AnimationTrack.transform;
		bool flag = false;
		if (obj.CustomTrack != null)
		{
			JSONNode customTrack = obj.CustomTrack;
			List<string> list = ((customTrack is JSONString jSONString) ? new List<string> { jSONString } : ((!(customTrack is JSONArray jSONArray2)) ? new List<string>() : new List<string>(jSONArray2.Children.Select((Func<JSONNode, string>)((JSONNode c) => c)))));
			List<string> list2 = list;
			foreach (string item in list2)
			{
				AddParent(item);
				List<BaseCustomEvent> value = null;
				BeatmapObjectContainerCollection.GetCollectionForType<CustomEventGridContainer>(ObjectType.CustomEvent).EventsByTrack?.TryGetValue(item, out value);
				if (value == null)
				{
					continue;
				}
				BaseDifficulty map = BeatSaberSongContainer.Instance.Map;
				foreach (BaseCustomEvent item2 in value.Where((BaseCustomEvent ev) => ev.Type == "AssignPathAnimation"))
				{
					JSONNode.Enumerator enumerator3 = item2.Data.GetEnumerator();
					while (enumerator3.MoveNext())
					{
						KeyValuePair<string, JSONNode> current3 = enumerator3.Current;
						if (current3.Key == "_definitePosition" || current3.Key == "definitePosition")
						{
							flag = true;
						}
						IPointDefinition.UntypedParams p = new IPointDefinition.UntypedParams
						{
							Key = "track_" + current3.Key,
							Overwrite = false,
							Points = current3.Value,
							Easing = item2.DataEasing,
							Time = item2.SongBpmTime,
							Transition = item2.DataDuration.GetValueOrDefault(),
							TimeBegin = time_begin,
							TimeEnd = time_end
						};
						if (p.Transition != 0f)
						{
							p.Transition = map.JsonTimeToSongBpmTime(item2.JsonTime + p.Transition).Value - item2.SongBpmTime;
						}
						AddPointDef(p, current3.Key, item2);
					}
				}
			}
			if (list2.Count > 0)
			{
				AnimationTrack.transform.SetParent(tracks[0].Track.ObjectParentTransform, worldPositionStays: false);
			}
		}
		if (obj.CustomAnimation != null)
		{
			JSONNode.Enumerator enumerator3 = obj.CustomAnimation.AsObject.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				KeyValuePair<string, JSONNode> current4 = enumerator3.Current;
				if (current4.Key == "_definitePosition" || current4.Key == "definitePosition")
				{
					flag = true;
				}
				IPointDefinition.UntypedParams p2 = new IPointDefinition.UntypedParams
				{
					Key = current4.Key,
					Overwrite = true,
					Points = current4.Value,
					Easing = null,
					TimeBegin = time_begin,
					TimeEnd = time_end
				};
				AddPointDef(p2, current4.Key, null);
			}
		}
		if (flag)
		{
			JSONNode jSONNode = obj.CustomData["_disableNoteGravity"];
			bool asBool;
			if ((object)jSONNode == null)
			{
				JSONNode jSONNode2 = obj.CustomData["disableNoteGravity"];
				if ((object)jSONNode2 == null)
				{
					goto IL_058f;
				}
				asBool = jSONNode2.AsBool;
			}
			else
			{
				asBool = jSONNode.AsBool;
			}
			if (asBool)
			{
				Debug.LogError("disableNoteGravity is bugged when combined with definitePosition, please remove it!");
				Vector3 localPosition = AnimationTrack.ObjectParentTransform.localPosition;
				localPosition.y = localPosition.y * -0.1f + 1f;
				AnimationTrack.ObjectParentTransform.localPosition = localPosition;
			}
		}
		goto IL_058f;
		IL_058f:
		properties = new IAnimateProperty[AnimatedProperties.Count];
		int num2 = 0;
		foreach (KeyValuePair<string, IAnimateProperty> animatedProperty in AnimatedProperties)
		{
			animatedProperty.Value.Sort();
			properties[num2++] = animatedProperty.Value;
		}
		Update();
		AudioTimeSyncController atsc = Atsc;
		atsc.TimeChanged = (Action)Delegate.Combine(atsc.TimeChanged, new Action(OnTimeChanged));
	}

	public void AttachToGeometry(BaseEnvironmentEnhancement eh)
	{
		bool flag = false;
		ResetData();
		TargetType = TargetTypes.Transform;
		LocalTarget = AnimationThis.transform;
		WorldTarget = AnimationThis.transform;
		WorldRotation = LocalRotation;
		Vector3? scale = eh.Scale;
		if (scale.HasValue)
		{
			Vector3 valueOrDefault = scale.GetValueOrDefault();
			Scale._default = valueOrDefault;
		}
		scale = eh.Position;
		if (scale.HasValue)
		{
			Vector3 valueOrDefault2 = scale.GetValueOrDefault();
			OffsetPosition._default = (flag ? 1f : 1.667f) * valueOrDefault2;
		}
		scale = eh.LocalPosition;
		if (scale.HasValue)
		{
			Vector3 valueOrDefault3 = scale.GetValueOrDefault();
			OffsetPosition._default = (flag ? 1f : 1.667f) * valueOrDefault3;
		}
		scale = eh.Rotation;
		if (scale.HasValue)
		{
			Vector3 valueOrDefault4 = scale.GetValueOrDefault();
			LocalRotation._default = Quaternion.Euler(valueOrDefault4.x, valueOrDefault4.y, valueOrDefault4.z);
		}
		scale = eh.LocalRotation;
		if (scale.HasValue)
		{
			Vector3 valueOrDefault5 = scale.GetValueOrDefault();
			LocalRotation._default = Quaternion.Euler(valueOrDefault5.x, valueOrDefault5.y, valueOrDefault5.z);
		}
		if (eh.Track != null)
		{
			AddParent(eh.Track);
			container.transform.SetParent(tracks[0].Track.ObjectParentTransform, worldPositionStays: false);
		}
		AudioTimeSyncController atsc = Atsc;
		atsc.TimeChanged = (Action)Delegate.Combine(atsc.TimeChanged, new Action(OnTimeChanged));
		OnTimeChanged();
	}

	public void AttachToTrack(Track track, string name)
	{
		ResetData();
		TargetType = TargetTypes.Transform;
		LocalTarget = track.ObjectParentTransform;
		WorldTarget = track.transform;
		AudioTimeSyncController atsc = Atsc;
		atsc.TimeChanged = (Action)Delegate.Combine(atsc.TimeChanged, new Action(OnTimeChanged));
	}

	public void AttachToMaterial(GeometryContainer con, string track, string colorKeyword)
	{
		ResetData();
		TargetType = TargetTypes.Material;
		container = con;
		ColorKeyword = colorKeyword;
		base.enabled = true;
		AddParent(track);
	}

	public void AddParent(string name)
	{
		TrackAnimator animationTrack = TracksManager.GetAnimationTrack(name);
		animationTrack.AddChild(this);
		tracks.Add(animationTrack);
	}

	public void Update()
	{
		float num = _time ?? Atsc?.CurrentSongBpmTime ?? 0f;
		if (container?.ObjectData is BaseGrid baseGrid)
		{
			int num2 = ((!(num > time_end)) ? 1 : (-1));
			if (!(container is ChainContainer))
			{
				container?.MaterialPropertyBlock.SetFloat(animationSpawned, num2);
				if (container is NoteContainer noteContainer)
				{
					noteContainer.ArrowMaterialPropertyBlock.SetFloat(animationSpawned, num2);
				}
			}
			AnimatedLife = (_time.HasValue && _time < baseGrid.SongBpmTime) || WorldPosition.Count > 0 || (baseGrid.CustomFake && num < time_end);
			if (ShouldRecycle)
			{
				float num3 = ((WorldPosition.Count == 0 && !baseGrid.CustomFake) ? baseGrid.SongBpmTime : time_end);
				if (num > num3)
				{
					BeatmapObjectContainerCollection.GetCollectionForType(container.ObjectData.ObjectType).RecycleContainer(container.ObjectData);
					AnimatedLife = false;
					return;
				}
			}
		}
		int num4 = properties.Length;
		for (int i = 0; i < num4; i++)
		{
			IAnimateProperty animateProperty = properties[i];
			if (num >= animateProperty.StartTime)
			{
				animateProperty.UpdateProperty(num);
			}
		}
		if (AnimatedTrack)
		{
			AnimationTrack.UpdateTime(num);
		}
	}

	public void LateUpdate()
	{
		if (TargetType == TargetTypes.Material)
		{
			if (Colors.Count > 0)
			{
				Color value = Colors.Get();
				container.MaterialPropertyBlock.SetColor(ColorKeyword, value);
				container.UpdateMaterials();
			}
			return;
		}
		if (LocalRotation.Count > 0)
		{
			LocalTarget.localRotation = LocalRotation.Get();
		}
		if (OffsetPosition.Count > 0)
		{
			LocalTarget.localPosition = OffsetPosition.Get();
		}
		if (Scale.Count > 0)
		{
			LocalTarget.localScale = Scale.Get();
		}
		if ((object)WorldTarget != null && WorldRotation.Count > 0 && !(container is GeometryContainer))
		{
			WorldTarget.localRotation = WorldRotation.Get();
		}
		float num = _time ?? Atsc?.CurrentSongBpmTime ?? 0f;
		if (WorldPosition.Count > 0)
		{
			if (time_begin < num && num < time_end)
			{
				AnimationTrack.UpdatePosition(0f);
			}
			ObjectContainer objectContainer = container;
			if ((object)objectContainer != null && !(objectContainer is GeometryContainer))
			{
				container.transform.localPosition = WorldPosition.Get();
			}
			else
			{
				WorldTarget.localPosition = WorldPosition.Get();
			}
		}
		if ((object)container != null && (Colors.Count > 0 || OpacityArrow.Count > 0 || Opacity.Count > 0))
		{
			if (Colors.Count > 0)
			{
				Color value2 = Colors.Get();
				container.MaterialPropertyBlock.SetColor(ColorKeyword, value2);
			}
			if (container is NoteContainer noteContainer)
			{
				noteContainer.ArrowMaterialPropertyBlock.SetFloat(opaqueAlpha, OpacityArrow.Get());
			}
			container.MaterialPropertyBlock.SetFloat(opaqueAlpha, Opacity.Get());
			container.UpdateMaterials();
		}
	}

	public void SetLifeTime(float normalTime)
	{
		_time = ((normalTime < 0f) ? ((float?)null) : new float?(Mathf.LerpUnclamped(time_begin, time_end, normalTime)));
	}

	private void OnTimeChanged()
	{
		if (!Atsc.IsPlaying)
		{
			LocalTarget.localRotation = LocalRotation.Get();
			LocalTarget.localPosition = OffsetPosition.Get();
			LocalTarget.localScale = Scale.Get();
			if ((object)WorldTarget != null && !(container is GeometryContainer))
			{
				WorldTarget.localRotation = WorldRotation.Get();
			}
		}
	}

	private void RequireAnimationTrack()
	{
		if (AnimationTrack == null)
		{
			AnimationTrack = TracksManager.CreateIndividualTrack(container.ObjectData as BaseGrid);
			AnimationTrack.AttachContainer(container);
			AnimationTrack.ObjectParentTransform.localPosition = new Vector3(container.transform.localPosition.x, container.transform.localPosition.y, 0f);
			AnimationTrack.transform.localPosition = Vector3.zero;
			container.transform.localPosition = Vector3.zero;
			AnimatedTrack = true;
		}
	}

	private void AddPointDef(IPointDefinition.UntypedParams p, string key, BaseCustomEvent source)
	{
		switch (key)
		{
		case "_dissolve":
		case "dissolve":
			AddPointDef(source, delegate(float f)
			{
				Opacity.Add(f);
			}, PointDataParsers.ParseFloat, p, 1f);
			break;
		case "_dissolveArrow":
		case "dissolveArrow":
			AddPointDef(source, delegate(float f)
			{
				OpacityArrow.Add(f);
			}, PointDataParsers.ParseFloat, p, 1f);
			break;
		case "_localRotation":
		case "localRotation":
			AddPointDef(source, delegate(Quaternion q)
			{
				LocalRotation.Add(q);
			}, PointDataParsers.ParseQuaternion, p, Quaternion.identity);
			break;
		case "_rotation":
		case "offsetWorldRotation":
			AddPointDef(source, delegate(Quaternion v)
			{
				WorldRotation.Add(v);
			}, PointDataParsers.ParseQuaternion, p, Quaternion.identity);
			break;
		case "_position":
		case "offsetPosition":
			AddPointDef(source, delegate(Vector3 v)
			{
				OffsetPosition.Add(v);
			}, PointDataParsers.ParseVector3, p, Vector3.zero);
			break;
		case "_definitePosition":
		case "definitePosition":
			AddPointDef(source, delegate(Vector3 v)
			{
				WorldPosition.Add(v);
			}, PointDataParsers.ParseVector3, p, Vector3.zero);
			break;
		case "_scale":
		case "scale":
			AddPointDef(source, delegate(Vector3 v)
			{
				Scale.Add(v);
			}, PointDataParsers.ParseVector3, p, Vector3.one);
			break;
		case "_color":
		case "color":
			AddPointDef(source, delegate(Color c)
			{
				Colors.Add(c);
			}, PointDataParsers.ParseColor, p, Color.white);
			break;
		}
	}

	private void AddPointDef<T>(BaseCustomEvent source, Action<T> setter, PointDefinition<T>.Parser parser, IPointDefinition.UntypedParams p, T _default) where T : struct
	{
		try
		{
			if (p.Overwrite)
			{
				AnimatedProperties[p.Key] = new AnimateProperty<T>(new List<PointDefinition<T>>(), setter, _default);
			}
			GetAnimateProperty(p.Key, setter, _default).AddPointDef(parser, p, source);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private AnimateProperty<T> GetAnimateProperty<T>(string key, Action<T> setter, T _default) where T : struct
	{
		if (!AnimatedProperties.ContainsKey(key))
		{
			AnimatedProperties[key] = new AnimateProperty<T>(new List<PointDefinition<T>>(), setter, _default);
		}
		return AnimatedProperties[key] as AnimateProperty<T>;
	}

	private static float WallClamp(float a)
	{
		if (0f - minWall < a && a < minWall)
		{
			return minWall;
		}
		return a;
	}
}

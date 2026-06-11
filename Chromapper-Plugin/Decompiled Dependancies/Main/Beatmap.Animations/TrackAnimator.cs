using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Animations;

public class TrackAnimator : MonoBehaviour
{
	public AudioTimeSyncController Atsc;

	public Track Track;

	public ObjectAnimator Animator;

	public Dictionary<string, IAnimateProperty> AnimatedProperties = new Dictionary<string, IAnimateProperty>();

	private IAnimateProperty[] properties = new IAnimateProperty[0];

	public List<TrackAnimator> Parents = new List<TrackAnimator>();

	public List<ObjectAnimator> Children = new List<ObjectAnimator>();

	public ObjectAnimator[] CachedChildren = new ObjectAnimator[0];

	private bool preload;

	public void AddEvent(BaseCustomEvent ev)
	{
		JSONNode.Enumerator enumerator = ev.Data.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, JSONNode> current = enumerator.Current;
			IPointDefinition.UntypedParams p = new IPointDefinition.UntypedParams
			{
				Key = current.Key,
				Points = current.Value,
				Easing = ev.DataEasing,
				Time = ev.JsonTime,
				Duration = ev.DataDuration.GetValueOrDefault(),
				TimeBegin = ev.JsonTime,
				TimeEnd = ev.JsonTime + ev.DataDuration.GetValueOrDefault(),
				Repeat = ev.DataRepeat.GetValueOrDefault()
			};
			AddPointDef(p, current.Key, ev);
		}
		RefreshProperties();
	}

	public void RemoveEvent(BaseCustomEvent ev)
	{
		foreach (string item in AnimatedProperties.Keys.ToList())
		{
			AnimatedProperties[item].RemoveEvent(ev);
			if (AnimatedProperties[item].IsEmpty())
			{
				AnimatedProperties.Remove(item);
			}
		}
		RefreshProperties();
	}

	private void RefreshProperties()
	{
		properties = new IAnimateProperty[AnimatedProperties.Count];
		int num = 0;
		foreach (KeyValuePair<string, IAnimateProperty> animatedProperty in AnimatedProperties)
		{
			animatedProperty.Value.Sort();
			properties[num++] = animatedProperty.Value;
		}
		Update();
	}

	public void Update()
	{
		float num = Atsc?.CurrentJsonTime ?? 0f;
		if (CachedChildren.Length == 0)
		{
			base.enabled = false;
			if (Animator != null)
			{
				Animator.enabled = false;
			}
			return;
		}
		for (int i = 0; i < properties.Length; i++)
		{
			IAnimateProperty animateProperty = properties[i];
			if (num >= animateProperty.StartTime)
			{
				animateProperty.UpdateProperty(num);
			}
		}
	}

	public void AddChild(ObjectAnimator oa)
	{
		Children.Add(oa);
		OnChildrenChanged();
	}

	public void RemoveChild(ObjectAnimator oa)
	{
		Children.Remove(oa);
		OnChildrenChanged();
	}

	public void OnChildrenChanged()
	{
		CachedChildren = Children.Where((ObjectAnimator o) => o.enabled).ToArray();
		base.enabled = CachedChildren.Length != 0;
		if (Animator != null)
		{
			Animator.enabled = base.enabled;
		}
		Parents.ForEach(delegate(TrackAnimator t)
		{
			t.OnChildrenChanged();
		});
	}

	private void AddPointDef(IPointDefinition.UntypedParams p, string key, BaseCustomEvent source)
	{
		switch (key)
		{
		case "_dissolve":
		case "dissolve":
			AddPointDef(source, delegate(ObjectAnimator animator, float f)
			{
				animator.Opacity.Add(f);
			}, PointDataParsers.ParseFloat, p, 1f);
			break;
		case "_dissolveArrow":
		case "dissolveArrow":
			AddPointDef(source, delegate(ObjectAnimator animator, float f)
			{
				animator.OpacityArrow.Add(f);
			}, PointDataParsers.ParseFloat, p, 1f);
			break;
		case "_localRotation":
		case "localRotation":
			AddPointDef(source, delegate(ObjectAnimator animator, Quaternion v)
			{
				animator.LocalRotation.Add(v);
			}, PointDataParsers.ParseQuaternion, p, Quaternion.identity);
			break;
		case "rotation":
			AddPointDef(source, delegate(ObjectAnimator animator, Quaternion v)
			{
				if (animator.TargetType == ObjectAnimator.TargetTypes.Transform)
				{
					animator.WorldRotation.Add(v);
				}
			}, PointDataParsers.ParseQuaternion, p, Quaternion.identity);
			break;
		case "_rotation":
		case "offsetWorldRotation":
			AddPointDef(source, delegate(ObjectAnimator animator, Quaternion v)
			{
				animator.WorldRotation.Add(v);
			}, PointDataParsers.ParseQuaternion, p, Quaternion.identity);
			break;
		case "_position":
			AddPointDef(source, delegate(ObjectAnimator animator, Vector3 v)
			{
				animator.OffsetPosition.Add(v);
			}, PointDataParsers.ParseVector3, p, Vector3.zero);
			break;
		case "offsetPosition":
			AddPointDef(source, delegate(ObjectAnimator animator, Vector3 v)
			{
				if (animator.TargetType == ObjectAnimator.TargetTypes.GameplayObject)
				{
					animator.OffsetPosition.Add(v);
				}
			}, PointDataParsers.ParseVector3, p, Vector3.zero);
			break;
		case "localPosition":
			AddPointDef(source, delegate(ObjectAnimator animator, Vector3 v)
			{
				if (animator.TargetType == ObjectAnimator.TargetTypes.Transform)
				{
					animator.OffsetPosition.Add(v * 1.667f);
				}
			}, PointDataParsers.ParseVector3, p, Vector3.zero);
			break;
		case "position":
			AddPointDef(source, delegate(ObjectAnimator animator, Vector3 v)
			{
				if (animator.TargetType == ObjectAnimator.TargetTypes.Transform)
				{
					animator.WorldPosition.Add(v * 1.667f);
				}
			}, PointDataParsers.ParseVector3, p, Vector3.zero);
			break;
		case "_scale":
		case "scale":
			AddPointDef(source, delegate(ObjectAnimator animator, Vector3 v)
			{
				animator.Scale.Add(v);
			}, PointDataParsers.ParseVector3, p, Vector3.one);
			break;
		case "_color":
		case "color":
			AddPointDef(source, delegate(ObjectAnimator animator, Color v)
			{
				if (animator.TargetType != ObjectAnimator.TargetTypes.Transform)
				{
					animator.Colors.Add(v);
				}
			}, PointDataParsers.ParseColor, p, Color.white);
			break;
		case "_time":
		case "time":
			AddPointDef(source, delegate(ObjectAnimator animator, float f)
			{
				animator.SetLifeTime(f);
			}, PointDataParsers.ParseFloat, p, -1f);
			break;
		}
	}

	private void AddPointDef<T>(BaseCustomEvent source, Action<ObjectAnimator, T> _setter, PointDefinition<T>.Parser parser, IPointDefinition.UntypedParams p, T _default) where T : struct
	{
		Action<T> setter = delegate(T v)
		{
			for (int i = 0; i < CachedChildren.Length; i++)
			{
				_setter(CachedChildren[i], v);
			}
		};
		GetAnimateProperty(p.Key, setter, _default).AddPointDef(parser, p, source);
	}

	private AnimateProperty<T> GetAnimateProperty<T>(string key, Action<T> setter, T _default) where T : struct
	{
		if (!AnimatedProperties.ContainsKey(key))
		{
			AnimatedProperties[key] = new AnimateProperty<T>(new List<PointDefinition<T>>(), setter, _default);
		}
		return AnimatedProperties[key] as AnimateProperty<T>;
	}
}

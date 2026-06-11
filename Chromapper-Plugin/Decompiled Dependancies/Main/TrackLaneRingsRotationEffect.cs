using System.Collections.Generic;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class TrackLaneRingsRotationEffect : MonoBehaviour
{
	private class RingRotationEffect
	{
		public float ProgressPos;

		public float RotationAngle;

		public float RotationFlexySpeed;

		public float RotationPropagationSpeed;

		public float RotationStep;
	}

	[FormerlySerializedAs("manager")]
	public TrackLaneRingsManager Manager;

	[FormerlySerializedAs("mirrorManager")]
	public TrackLaneRingsManager MirrorManager;

	[FormerlySerializedAs("startupRotationAngle")]
	public float StartupRotationAngle = 45f;

	[FormerlySerializedAs("startupRotationStep")]
	public float StartupRotationStep = 5f;

	[FormerlySerializedAs("startupRotationPropagationSpeed")]
	public float StartupRotationPropagationSpeed = 1f;

	[FormerlySerializedAs("startupRotationFlexySpeed")]
	public float StartupRotationFlexySpeed = 1f;

	[FormerlySerializedAs("rotationStep")]
	public float RotationStep = 90f;

	[FormerlySerializedAs("counterSpin")]
	public bool CounterSpin;

	private List<RingRotationEffect> activeEffects;

	private List<RingRotationEffect> effectsPool;

	private void Awake()
	{
		activeEffects = new List<RingRotationEffect>(20);
		effectsPool = new List<RingRotationEffect>(20);
		for (int i = 0; i < effectsPool.Capacity; i++)
		{
			effectsPool.Add(new RingRotationEffect());
		}
	}

	public void Reset()
	{
		for (int num = activeEffects.Count - 1; num >= 0; num--)
		{
			RecycleRingRotationEffect(activeEffects[num]);
			activeEffects.RemoveAt(num);
		}
		TrackLaneRing[] rings = Manager.Rings;
		for (int i = 0; i < rings.Length; i++)
		{
			rings[i].Reset();
		}
		if (!(MirrorManager == null))
		{
			rings = MirrorManager.Rings;
			for (int i = 0; i < rings.Length; i++)
			{
				rings[i].Reset();
			}
		}
	}

	private void Start()
	{
		AddRingRotationEvent(StartupRotationAngle, StartupRotationStep, StartupRotationPropagationSpeed, StartupRotationFlexySpeed, new BaseEvent());
	}

	private void FixedUpdate()
	{
		TrackLaneRing[] rings = Manager.Rings;
		for (int num = activeEffects.Count - 1; num >= 0; num--)
		{
			RingRotationEffect ringRotationEffect = activeEffects[num];
			for (int i = (int)ringRotationEffect.ProgressPos; (float)i < ringRotationEffect.ProgressPos + ringRotationEffect.RotationPropagationSpeed && i < rings.Length; i++)
			{
				float destinationZ = ringRotationEffect.RotationAngle + (float)i * ringRotationEffect.RotationStep;
				rings[i].SetRotation(destinationZ, ringRotationEffect.RotationFlexySpeed);
				if (MirrorManager != null)
				{
					MirrorManager.Rings[i].SetRotation(destinationZ, ringRotationEffect.RotationFlexySpeed);
				}
			}
			ringRotationEffect.ProgressPos += ringRotationEffect.RotationPropagationSpeed;
			if (ringRotationEffect.ProgressPos >= (float)rings.Length)
			{
				RecycleRingRotationEffect(activeEffects[num]);
				activeEffects.RemoveAt(num);
			}
		}
	}

	public void AddRingRotationEvent(float angle, float step, float propagationSpeed, float flexySpeed, float rotation, bool clockwise, bool counterSpinEvent)
	{
		RingRotationEffect ringRotationEffect = SpawnRingRotationEffect();
		int num = (clockwise ? 1 : (-1));
		ringRotationEffect.ProgressPos = 0f;
		ringRotationEffect.RotationStep = step;
		ringRotationEffect.RotationPropagationSpeed = propagationSpeed;
		ringRotationEffect.RotationFlexySpeed = flexySpeed;
		if (CounterSpin && counterSpinEvent)
		{
			num *= -1;
		}
		ringRotationEffect.RotationAngle = angle + rotation * (float)num;
		activeEffects.Add(ringRotationEffect);
	}

	public void AddRingRotationEvent(float angle, float step, float propagationSpeed, float flexySpeed, BaseEvent evt)
	{
		bool clockwise = Random.value < 0.5f;
		float rotation = RotationStep;
		bool flag = false;
		if (evt.CustomData != null)
		{
			if (evt.CustomStep.HasValue)
			{
				step = evt.CustomStep.Value;
			}
			if (evt.CustomProp.HasValue)
			{
				propagationSpeed = evt.CustomProp.Value;
			}
			if (evt.CustomSpeed.HasValue)
			{
				flexySpeed = evt.CustomSpeed.Value;
			}
			if (evt.CustomRingRotation.HasValue)
			{
				rotation = evt.CustomRingRotation.Value;
			}
			if (evt.CustomStepMult.HasValue)
			{
				step *= evt.CustomStepMult.Value;
			}
			if (evt.CustomPropMult.HasValue)
			{
				propagationSpeed *= evt.CustomPropMult.Value;
			}
			if (evt.CustomSpeedMult.HasValue)
			{
				flexySpeed *= evt.CustomSpeedMult.Value;
			}
			if (evt.CustomDirection.HasValue)
			{
				clockwise = evt.CustomDirection.Value == 0;
			}
			flag = evt.CustomData.HasKey("_counterSpin") && evt.CustomData["_counterSpin"].AsBool;
		}
		if (evt.CustomData != null && evt.CustomData.HasKey("_reset") && evt.CustomData["_reset"] == true)
		{
			AddRingRotationEvent(angle, 0f, 50f, 50f, 90f, flag, counterSpinEvent: false);
		}
		else
		{
			AddRingRotationEvent(angle, step, propagationSpeed, flexySpeed, rotation, clockwise, flag);
		}
	}

	private void RecycleRingRotationEffect(RingRotationEffect effect)
	{
		effectsPool.Add(effect);
	}

	private RingRotationEffect SpawnRingRotationEffect()
	{
		RingRotationEffect result;
		if (effectsPool.Count > 0)
		{
			result = effectsPool[0];
			effectsPool.RemoveAt(0);
		}
		else
		{
			result = new RingRotationEffect();
		}
		return result;
	}
}

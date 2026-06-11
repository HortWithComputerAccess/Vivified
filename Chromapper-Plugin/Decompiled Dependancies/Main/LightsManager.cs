using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class LightsManager : MonoBehaviour
{
	[Serializable]
	public class LightGroup
	{
		public List<LightingEvent> Lights = new List<LightingEvent>();
	}

	public static readonly float FadeTime = 1.5f;

	public static readonly float FlashTime = 0.6f;

	public static readonly float HDRIntensity = Mathf.GammaToLinearSpace(2.4169f);

	public static readonly float HDRFlashIntensity = Mathf.GammaToLinearSpace(3f);

	[FormerlySerializedAs("disableCustomInitialization")]
	public bool DisableCustomInitialization;

	public List<LightingEvent> ControllingLights = new List<LightingEvent>();

	public LightGroup[] LightsGroupedByZ = new LightGroup[0];

	public List<RotatingLightsBase> RotatingLights = new List<RotatingLightsBase>();

	public float GroupingMultiplier = 1f;

	public float GroupingOffset = 0.001f;

	public Dictionary<int, int> LightIDPlacementMap;

	public Dictionary<int, int> LightIDPlacementMapReverse;

	public Dictionary<int, LightingEvent> LightIDMap;

	private int previousValue;

	private void Start()
	{
		LoadOldLightOrder();
	}

	public void LoadOldLightOrder()
	{
		if (DisableCustomInitialization)
		{
			return;
		}
		LightingEvent[] componentsInChildren = GetComponentsInChildren<LightingEvent>();
		foreach (LightingEvent lightingEvent in componentsInChildren)
		{
			if (!lightingEvent.OverrideLightGroup)
			{
				ControllingLights.Add(lightingEvent);
			}
		}
		RotatingLightsBase[] componentsInChildren2 = GetComponentsInChildren<RotatingLightsBase>();
		foreach (RotatingLightsBase rotatingLightsBase in componentsInChildren2)
		{
			if (!rotatingLightsBase.IsOverrideLightGroup())
			{
				RotatingLights.Add(rotatingLightsBase);
			}
		}
		List<LightingEvent> lightIdOrder = (from x in ControllingLights
			orderby x.LightID
			group x by x.LightID into x
			select x.First()).ToList();
		LightIDPlacementMap = lightIdOrder.ToDictionary((LightingEvent x) => lightIdOrder.IndexOf(x), (LightingEvent x) => x.LightID);
		LightIDPlacementMapReverse = lightIdOrder.ToDictionary((LightingEvent x) => x.LightID, (LightingEvent x) => lightIdOrder.IndexOf(x));
		LightIDMap = lightIdOrder.ToDictionary((LightingEvent x) => x.LightID, (LightingEvent x) => x);
		LightsGroupedByZ = GroupLightsBasedOnZ();
		RotatingLights = RotatingLights.OrderBy((RotatingLightsBase x) => x.transform.localPosition.z).ToList();
	}

	public LightGroup[] GroupLightsBasedOnZ()
	{
		return (from x in ControllingLights
			where x.gameObject.activeInHierarchy
			where x.PropGroup >= 0
			group x by Mathf.RoundToInt(x.PropGroup) into x
			orderby x.Key
			select new LightGroup
			{
				Lights = x.ToList()
			}).ToArray();
	}

	public void ChangeAlpha(float alpha, float time, IEnumerable<LightingEvent> lights)
	{
		foreach (LightingEvent light in lights)
		{
			light.UpdateTargetAlpha(alpha, time);
		}
	}

	public void ChangeMultiplierAlpha(float alpha, IEnumerable<LightingEvent> lights)
	{
		foreach (LightingEvent light in lights)
		{
			light.UpdateMultiplyAlpha(alpha);
		}
	}

	public void ChangeColor(Color color, float time, IEnumerable<LightingEvent> lights)
	{
		foreach (LightingEvent light in lights)
		{
			light.UpdateTargetColor(color * HDRIntensity, time);
		}
	}

	public void Fade(Color color, IEnumerable<LightingEvent> lights)
	{
		foreach (LightingEvent light in lights)
		{
			light.UpdateTargetAlpha(1f, 0f);
			light.UpdateTargetColor(color * HDRFlashIntensity, 0f);
			if (light.CanBeTurnedOff)
			{
				light.UpdateTargetAlpha(0f, FadeTime);
				light.UpdateTargetColor(Color.black, FadeTime);
			}
			else
			{
				light.UpdateTargetColor(color * HDRIntensity, FadeTime);
			}
		}
	}

	public void Flash(Color color, IEnumerable<LightingEvent> lights)
	{
		foreach (LightingEvent light in lights)
		{
			light.UpdateTargetAlpha(1f, 0f);
			light.UpdateTargetColor(color * HDRFlashIntensity, 0f);
			light.UpdateTargetColor(color * HDRIntensity, FlashTime);
		}
	}

	public void SetValue(int value)
	{
		if (value < 255)
		{
			previousValue = value;
		}
	}

	public void Boost(bool boost, Color redColor, Color blueColor, Color whiteColor)
	{
		if (previousValue == 0)
		{
			return;
		}
		Color a = previousValue switch
		{
			1 => blueColor, 
			2 => blueColor, 
			3 => blueColor, 
			4 => blueColor, 
			5 => redColor, 
			6 => redColor, 
			7 => redColor, 
			8 => redColor, 
			9 => whiteColor, 
			10 => whiteColor, 
			11 => whiteColor, 
			12 => whiteColor, 
			_ => Color.white, 
		};
		foreach (LightingEvent controllingLight in ControllingLights)
		{
			controllingLight.UpdateBoostState(boost);
			if (!controllingLight.UseInvertedPlatformColors)
			{
				SetTargets(controllingLight, a);
			}
		}
	}

	private void SetTargets(LightingEvent light, Color a)
	{
		if (previousValue == 3 || previousValue == 7)
		{
			light.UpdateCurrentColor(a * HDRFlashIntensity);
			light.UpdateTargetAlpha(0f);
		}
		else
		{
			light.UpdateTargetColor(a * HDRIntensity, 0f);
		}
	}
}

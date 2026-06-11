using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridRenderingController : MonoBehaviour
{
	private static readonly int offset = Shader.PropertyToID("_Offset");

	private static readonly int gridSpacing = Shader.PropertyToID("_GridSpacing");

	private static readonly int mainColor = Shader.PropertyToID("_Color");

	private static readonly int gridThickness = Shader.PropertyToID("_GridThickness");

	private static readonly Color mainColorDefault = new Color(0.33f, 0.33f, 0.33f, 1f);

	private static readonly Color mainColorHighContrast = new Color(0f, 0f, 0f, 1f);

	private static MaterialPropertyBlock oneBeatPropertyBlock;

	private static MaterialPropertyBlock smallBeatPropertyBlock;

	private static MaterialPropertyBlock detailedBeatPropertyBlock;

	private static MaterialPropertyBlock preciseBeatPropertyBlock;

	private static MaterialPropertyBlock beatColorPropertyBlock;

	[SerializeField]
	private AudioTimeSyncController atsc;

	[SerializeField]
	private Renderer[] oneBeat;

	[SerializeField]
	private Renderer[] smallBeatSegment;

	[SerializeField]
	private Renderer[] detailedBeatSegment;

	[SerializeField]
	private Renderer[] preciseBeatSegment;

	[SerializeField]
	private Renderer[] opaqueGrids;

	[SerializeField]
	private Renderer[] transparentGrids;

	[SerializeField]
	private Transform[] gridFrontTransforms;

	private readonly List<Renderer> allRenderers = new List<Renderer>();

	private void Awake()
	{
		oneBeatPropertyBlock = new MaterialPropertyBlock();
		smallBeatPropertyBlock = new MaterialPropertyBlock();
		detailedBeatPropertyBlock = new MaterialPropertyBlock();
		preciseBeatPropertyBlock = new MaterialPropertyBlock();
		beatColorPropertyBlock = new MaterialPropertyBlock();
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.GridMeasureSnappingChanged = (Action<int>)Delegate.Combine(audioTimeSyncController.GridMeasureSnappingChanged, new Action<int>(GridMeasureSnappingChanged));
		allRenderers.AddRange(oneBeat);
		allRenderers.AddRange(smallBeatSegment);
		allRenderers.AddRange(detailedBeatSegment);
		allRenderers.AddRange(preciseBeatSegment);
		Settings.NotifyBySettingName("HighContrastGrids", UpdateGridColors);
		Settings.NotifyBySettingName("GridTransparency", UpdateGridColors);
		Settings.NotifyBySettingName("TrackLength", UpdateTrackLength);
		Settings.NotifyBySettingName("OneBeatWidth", UpdateOneBeat);
		UpdateOneBeat(Settings.Instance.OneBeatWidth);
	}

	private void OnDestroy()
	{
		AudioTimeSyncController audioTimeSyncController = atsc;
		audioTimeSyncController.GridMeasureSnappingChanged = (Action<int>)Delegate.Remove(audioTimeSyncController.GridMeasureSnappingChanged, new Action<int>(GridMeasureSnappingChanged));
		Settings.ClearSettingNotifications("HighContrastGrids");
		Settings.ClearSettingNotifications("GridTransparency");
		Settings.ClearSettingNotifications("TrackLength");
		Settings.ClearSettingNotifications("OneBeatWidth");
	}

	public void UpdateOffset(float offset)
	{
		Shader.SetGlobalFloat(GridRenderingController.offset, offset);
		if (!atsc.IsPlaying)
		{
			GridMeasureSnappingChanged(atsc.GridMeasureSnapping);
		}
	}

	private void GridMeasureSnappingChanged(int snapping)
	{
		float num = GetLowestDenominator(snapping);
		if (num < 3f)
		{
			num = 4f;
		}
		oneBeatPropertyBlock.SetFloat(gridSpacing, EditorScaleController.EditorScale / 4f);
		Renderer[] array = oneBeat;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(oneBeatPropertyBlock);
		}
		smallBeatPropertyBlock.SetFloat(gridSpacing, EditorScaleController.EditorScale / 4f / num);
		array = smallBeatSegment;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(smallBeatPropertyBlock);
		}
		bool flag = num < (float)snapping;
		num *= (float)GetLowestDenominator(Mathf.FloorToInt((float)snapping / num));
		detailedBeatPropertyBlock.SetFloat(gridSpacing, EditorScaleController.EditorScale / 4f / num);
		array = detailedBeatSegment;
		foreach (Renderer obj in array)
		{
			obj.enabled = flag;
			obj.SetPropertyBlock(detailedBeatPropertyBlock);
		}
		bool flag2 = num < (float)snapping;
		num *= (float)GetLowestDenominator(Mathf.FloorToInt((float)snapping / num));
		preciseBeatPropertyBlock.SetFloat(gridSpacing, EditorScaleController.EditorScale / 4f / num);
		array = preciseBeatSegment;
		foreach (Renderer obj2 in array)
		{
			obj2.enabled = flag2;
			obj2.SetPropertyBlock(preciseBeatPropertyBlock);
		}
		UpdateGridColors();
	}

	private void UpdateGridColors(object _ = null)
	{
		float gridTransparency = Settings.Instance.GridTransparency;
		Color value = (Settings.Instance.HighContrastGrids ? mainColorHighContrast : mainColorDefault);
		value.a = 1f - gridTransparency;
		beatColorPropertyBlock.SetColor(mainColor, value);
		Renderer[] array = transparentGrids;
		foreach (Renderer obj in array)
		{
			obj.SetPropertyBlock(beatColorPropertyBlock);
			obj.enabled = value.a != 1f;
		}
		array = opaqueGrids;
		foreach (Renderer obj2 in array)
		{
			obj2.SetPropertyBlock(beatColorPropertyBlock);
			obj2.enabled = value.a == 1f;
		}
	}

	private void UpdateTrackLength(object _)
	{
		Transform[] array = gridFrontTransforms;
		foreach (Transform obj in array)
		{
			Vector3 localScale = obj.localScale;
			obj.localScale = new Vector3(localScale.x, localScale.y, (float)Settings.Instance.TrackLength * EditorScaleController.EditorScale);
		}
	}

	private void UpdateOneBeat(object value)
	{
		oneBeatPropertyBlock.SetFloat(gridThickness, (float)value);
		Renderer[] array = oneBeat;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(oneBeatPropertyBlock);
		}
	}

	private int GetLowestDenominator(int a)
	{
		if (a <= 1)
		{
			return 2;
		}
		IEnumerable<int> source = PrimeFactors(a);
		if (source.Any())
		{
			return source.Max();
		}
		return a;
	}

	public static List<int> PrimeFactors(int a)
	{
		List<int> list = new List<int>();
		int num = 2;
		while (a > 1)
		{
			while (a % num == 0)
			{
				a /= num;
				list.Add(num);
			}
			num++;
		}
		return list;
	}
}

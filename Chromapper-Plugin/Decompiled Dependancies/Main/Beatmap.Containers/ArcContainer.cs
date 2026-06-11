using System;
using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Enums;
using UnityEngine;

namespace Beatmap.Containers;

public class ArcContainer : ObjectContainer
{
	private const float splineControlPointScaleFactor = 4.1666665f;

	internal const float arcEmissionIntensity = 6f;

	private const int numSamples = 30;

	private static readonly int emissionColor = Shader.PropertyToID("_ColorTint");

	private static readonly int lit = Shader.PropertyToID("_Lit");

	private static readonly int translucentAlpha = Shader.PropertyToID("_TranslucentAlpha");

	[SerializeField]
	private TracksManager manager;

	[SerializeField]
	private GameObject indicatorMu;

	[SerializeField]
	private GameObject indicatorTmu;

	[SerializeField]
	private List<GameObject> indicators;

	[SerializeField]
	public BaseArc ArcData;

	private MaterialPropertyBlock indicatorMaterialPropertyBlock;

	[SerializeField]
	private LineRenderer splineRenderer;

	public override BaseObject ObjectData
	{
		get
		{
			return ArcData;
		}
		set
		{
			ArcData = (BaseArc)value;
		}
	}

	public Vector3 p0()
	{
		Vector2 position = ArcData.GetPosition();
		return new Vector3(position.x, position.y + ObjectContainer.offsetY, 0f);
	}

	public Vector3 p1()
	{
		Vector2 position = ArcData.GetPosition();
		if (ArcData.CutDirection == 8)
		{
			return new Vector3(position.x, position.y + ObjectContainer.offsetY, 0f);
		}
		float f = MathF.PI / 180f * NoteContainer.Directionalize(ArcData.CutDirection).z;
		Vector2 vector = new Vector2(Mathf.Sin(f), 0f - Mathf.Cos(f));
		Vector2 vector2 = position + vector * ArcData.HeadControlPointLengthMultiplier * 4.1666665f;
		return new Vector3(vector2.x, vector2.y + ObjectContainer.offsetY, 0f);
	}

	public Vector3 p2()
	{
		Vector2 tailPosition = ArcData.GetTailPosition();
		if (ArcData.TailCutDirection == 8)
		{
			return new Vector3(tailPosition.x, tailPosition.y + ObjectContainer.offsetY, (ArcData.TailSongBpmTime - ArcData.SongBpmTime) * EditorScaleController.EditorScale);
		}
		float f = MathF.PI / 180f * NoteContainer.Directionalize(ArcData.TailCutDirection).z;
		Vector2 vector = new Vector2(Mathf.Sin(f), 0f - Mathf.Cos(f));
		Vector2 vector2 = tailPosition - vector * ArcData.TailControlPointLengthMultiplier * 4.1666665f;
		return new Vector3(vector2.x, vector2.y + ObjectContainer.offsetY, (ArcData.TailSongBpmTime - ArcData.SongBpmTime) * EditorScaleController.EditorScale);
	}

	public Vector3 p3()
	{
		Vector2 tailPosition = ArcData.GetTailPosition();
		return new Vector3(tailPosition.x, tailPosition.y + ObjectContainer.offsetY, (ArcData.TailSongBpmTime - ArcData.SongBpmTime) * EditorScaleController.EditorScale);
	}

	private Vector3 SampleCubicBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		float num = 1f - t;
		return Mathf.Pow(num, 3f) * p0 + 3f * Mathf.Pow(num, 2f) * t * p1 + 3f * num * Mathf.Pow(t, 2f) * p2 + Mathf.Pow(t, 3f) * p3;
	}

	public static ArcContainer SpawnArc(BaseArc data, ref GameObject prefab)
	{
		ArcContainer component = UnityEngine.Object.Instantiate(prefab).GetComponent<ArcContainer>();
		component.ArcData = data;
		return component;
	}

	public override void Setup()
	{
		base.Setup();
		MaterialPropertyBlock.SetFloat(lit, 1f);
		MaterialPropertyBlock.SetFloat(translucentAlpha, 1f);
		foreach (GameObject indicator in indicators)
		{
			indicator.GetComponent<ArcIndicatorContainer>().Setup();
		}
		UpdateMaterials();
	}

	public override void UpdateGridPosition()
	{
		RecomputePosition();
		foreach (GameObject indicator in indicators)
		{
			indicator.GetComponent<ArcIndicatorContainer>().UpdateGridPosition();
		}
		UpdateCollisionGroups();
	}

	public void SetScale(Vector3 scale)
	{
		base.transform.localScale = scale;
	}

	public void NotifySplineChanged(BaseArc arcData = null)
	{
		if (arcData != null)
		{
			ArcData = arcData;
		}
		(BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Arc) as ArcGridContainer).RequestForSplineRecompute(this);
	}

	public void RecomputePosition()
	{
		if (ArcData == null)
		{
			return;
		}
		base.transform.localPosition = new Vector3(0f, 0f, ArcData.SongBpmTime * EditorScaleController.EditorScale);
		splineRenderer.positionCount = 31;
		Vector3 vector = p0();
		Vector3 vector2 = p1();
		Vector3 vector3 = p2();
		Vector3 vector4 = p3();
		if (ArcData.MidAnchorMode != 0 && ArcData.CutDirection != 8 && ArcData.PosX == ArcData.TailPosX && (ArcData.CutDirection == ArcData.TailCutDirection || Mathf.Approximately(180f, Mathf.Abs(NoteContainer.Directionalize(ArcData.CutDirection).z - NoteContainer.Directionalize(ArcData.TailCutDirection).z))))
		{
			(Vector3 headToMidControl, Vector3 midPoint, Vector3 midToTailControl) midAnchorPoints = GetMidAnchorPoints(vector, vector2, vector3, vector4);
			Vector3 item = midAnchorPoints.headToMidControl;
			Vector3 item2 = midAnchorPoints.midPoint;
			Vector3 item3 = midAnchorPoints.midToTailControl;
			for (int i = 0; i <= 30; i++)
			{
				splineRenderer.SetPosition(i, (i <= 15) ? SampleCubicBezierPoint(vector, vector2, item, item2, (float)i / 30f * 2f) : SampleCubicBezierPoint(item2, item3, vector3, vector4, (float)i / 30f * 2f - 1f));
			}
		}
		else
		{
			for (int j = 0; j <= 30; j++)
			{
				splineRenderer.SetPosition(j, SampleCubicBezierPoint(vector, vector2, vector3, vector4, (float)j / 30f));
			}
		}
		splineRenderer.enabled = true;
		ResetIndicatorsPosition();
	}

	private (Vector3 headToMidControl, Vector3 midPoint, Vector3 midToTailControl) GetMidAnchorPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 vector = (p0 + p3) / 2f;
		float num = ((ArcData.MidAnchorMode == 1) ? (-90f) : 90f);
		float z = NoteContainer.Directionalize(ArcData.CutDirection).z;
		Vector2 vector2 = new Vector2(Mathf.Sin((z + num) * (MathF.PI / 180f)), 0f - Mathf.Cos((z + num) * (MathF.PI / 180f)));
		vector += (Vector3)vector2 * 4.1666665f;
		Vector3 vector3 = new Vector3(Mathf.Abs(vector.x - p1.x), Mathf.Abs(vector.y - p1.y), Mathf.Abs(vector.z - p1.z));
		Vector3 vector4 = new Vector3(Mathf.Abs(vector.x - p2.x), Mathf.Abs(vector.y - p2.y), Mathf.Abs(vector.z - p2.z));
		bool num2 = Mathf.Approximately(p1.x, p2.x);
		bool flag = Mathf.Approximately(p1.y, p2.y);
		float num3 = (num2 ? 0f : ((vector3.x + vector4.x) * 0.25f));
		float num4 = (flag ? 0f : ((vector3.y + vector4.y) * 0.25f));
		float num5 = (vector3.z + vector4.z) * 0.15f;
		if (p1.x < p2.x)
		{
			num3 = 0f - num3;
		}
		if (p1.y < p2.y)
		{
			num4 = 0f - num4;
		}
		Vector3 vector5 = new Vector3(num3, num4, 0f - num5);
		return new ValueTuple<Vector3, Vector3, Vector3>(vector + vector5, item3: vector - vector5, item2: vector);
	}

	private void ResetIndicatorsPosition()
	{
		foreach (GameObject indicator in indicators)
		{
			indicator.GetComponent<ArcIndicatorContainer>().UpdateGridPosition();
		}
	}

	public void SetColor(Color c)
	{
		MaterialPropertyBlock.SetColor(ObjectContainer.color, c);
		MaterialPropertyBlock.SetColor(emissionColor, c * 6f);
		UpdateMaterials();
	}

	internal override void UpdateMaterials()
	{
		foreach (GameObject indicator in indicators)
		{
			indicator.GetComponent<ArcIndicatorContainer>().UpdateMaterials(MaterialPropertyBlock);
		}
		foreach (GameObject indicator2 in indicators)
		{
			indicator2.GetComponent<ArcIndicatorContainer>().OutlineVisible = base.OutlineVisible;
		}
		splineRenderer.SetPropertyBlock(MaterialPropertyBlock);
		foreach (Renderer selectionRenderer in SelectionRenderers)
		{
			selectionRenderer.SetPropertyBlock(MaterialPropertyBlock);
		}
	}

	public void SetIndicatorBlocksActive(bool visible)
	{
		foreach (GameObject indicator in indicators)
		{
			indicator.SetActive(visible);
		}
	}

	public void ChangeHeadMultiplier(float modifier)
	{
		ArcData.HeadControlPointLengthMultiplier += modifier;
	}

	public void ChangeTailMultiplier(float modifier)
	{
		ArcData.TailControlPointLengthMultiplier += modifier;
	}
}

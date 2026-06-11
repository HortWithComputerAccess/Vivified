using System;
using Beatmap.Base;
using Beatmap.Enums;
using UnityEngine;

namespace Beatmap.Containers;

public class ArcIndicatorContainer : ObjectContainer
{
	public IndicatorType IndicatorType;

	public ArcContainer ParentArc;

	private static readonly int lit = Shader.PropertyToID("_Lit");

	private static readonly int translucentAlpha = Shader.PropertyToID("_TranslucentAlpha");

	private static readonly int opaqueAlpha = Shader.PropertyToID("_OpaqueAlpha");

	public override BaseObject ObjectData
	{
		get
		{
			return ParentArc.ArcData;
		}
		set
		{
			ParentArc.ArcData = (BaseArc)value;
		}
	}

	public override void UpdateGridPosition()
	{
		if (IndicatorType == IndicatorType.Head)
		{
			float f = MathF.PI / 180f * NoteContainer.Directionalize(ParentArc.ArcData.CutDirection).z;
			Vector3 vector = new Vector3(Mathf.Sin(f), 0f - Mathf.Cos(f), 0f);
			base.transform.localPosition = ParentArc.p0() + vector / 2f;
			base.transform.localEulerAngles = new Vector3(NoteContainer.Directionalize(ParentArc.ArcData.CutDirection).z + 90f, -90f, 0f);
		}
		else if (IndicatorType == IndicatorType.Tail)
		{
			float f2 = MathF.PI / 180f * NoteContainer.Directionalize(ParentArc.ArcData.TailCutDirection).z;
			Vector3 vector2 = new Vector3(Mathf.Sin(f2), 0f - Mathf.Cos(f2), 0f);
			base.transform.localPosition = ParentArc.p3() - vector2 * 1.5f;
			base.transform.localEulerAngles = new Vector3(NoteContainer.Directionalize(ParentArc.ArcData.TailCutDirection).z + 90f, -90f, 0f);
		}
	}

	public void UpdateMaterials(MaterialPropertyBlock materialPropertyBlock)
	{
		Color value = materialPropertyBlock.GetColor(ObjectContainer.color);
		MaterialPropertyBlock.SetColor(ObjectContainer.color, value);
		UpdateMaterials();
	}

	public override void Setup()
	{
		base.Setup();
		MaterialPropertyBlock.SetFloat(lit, (!Settings.Instance.SimpleBlocks) ? 1 : 0);
		MaterialPropertyBlock.SetFloat(translucentAlpha, 0.6f);
		MaterialPropertyBlock.SetFloat(opaqueAlpha, 0.6f);
		UpdateMaterials();
	}
}

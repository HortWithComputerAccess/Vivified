using Beatmap.Base;
using Beatmap.Enums;
using UnityEngine;

namespace Beatmap.Containers;

public class ChainIndicatorContainer : ObjectContainer
{
	public IndicatorType IndicatorType;

	public ChainContainer ParentChain;

	private static readonly int lit = Shader.PropertyToID("_Lit");

	private static readonly int translucentAlpha = Shader.PropertyToID("_TranslucentAlpha");

	private static readonly int opaqueAlpha = Shader.PropertyToID("_OpaqueAlpha");

	public override BaseObject ObjectData
	{
		get
		{
			return ParentChain.ChainData;
		}
		set
		{
			ParentChain.ChainData = (BaseChain)value;
		}
	}

	public override void UpdateGridPosition()
	{
		BaseChain baseChain = (BaseChain)ObjectData;
		if (IndicatorType == IndicatorType.Head)
		{
			base.transform.localPosition = (Vector3)baseChain.GetPosition() + new Vector3(1.5f, 0f, 0f);
			base.transform.localEulerAngles = new Vector3(NoteContainer.Directionalize(ParentChain.ChainData.CutDirection).z + 90f, -90f, 0f);
		}
		else if (IndicatorType == IndicatorType.Tail)
		{
			float z = (baseChain.TailSongBpmTime - baseChain.SongBpmTime) * EditorScaleController.EditorScale;
			base.transform.localPosition = (Vector3)baseChain.GetTailPosition() + new Vector3(1.5f, 0f, z);
			base.transform.rotation = ParentChain.GetTailNodeRotation();
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

using System;
using System.Collections.Generic;
using Beatmap.Base;
using UnityEngine;

namespace Beatmap.Containers;

public class NoteContainer : ObjectContainer
{
	private static readonly int colorMultiplier = Shader.PropertyToID("_ColorMult");

	private static readonly int objectTime = Shader.PropertyToID("_ObjectTime");

	private static readonly int lit = Shader.PropertyToID("_Lit");

	private static readonly int translucentAlpha = Shader.PropertyToID("_TranslucentAlpha");

	private static readonly Color unassignedColor = new Color(0.1544118f, 0.1544118f, 0.1544118f);

	[SerializeField]
	private GameObject simpleBlock;

	[SerializeField]
	private GameObject simpleChainHead;

	[SerializeField]
	private GameObject complexBlock;

	[SerializeField]
	private GameObject complexChainHead;

	[SerializeField]
	public Transform DirectionTarget;

	[SerializeField]
	private List<MeshRenderer> noteRenderer;

	[SerializeField]
	private List<MeshRenderer> chainRenderer;

	[SerializeField]
	private MeshRenderer bombRenderer;

	[SerializeField]
	private MeshRenderer dotRenderer;

	[SerializeField]
	private MeshRenderer arrowRenderer;

	[SerializeField]
	private SpriteRenderer swingArcRenderer;

	public BaseNote NoteData;

	public MaterialPropertyBlock ArrowMaterialPropertyBlock;

	[NonSerialized]
	public Vector3 DirectionTargetEuler = Vector3.zero;

	public override BaseObject ObjectData
	{
		get
		{
			return NoteData;
		}
		set
		{
			NoteData = (BaseNote)value;
		}
	}

	public override void Setup()
	{
		base.Setup();
		SetModelInfer();
		MaterialPropertyBlock.SetFloat(lit, (!Settings.Instance.SimpleBlocks) ? 1 : 0);
		MaterialPropertyBlock.SetFloat(translucentAlpha, Settings.Instance.PastNoteModelAlpha);
		UpdateMaterials();
		if (ArrowMaterialPropertyBlock == null)
		{
			ArrowMaterialPropertyBlock = new MaterialPropertyBlock();
		}
		SetArcVisible(NoteGridContainer.ShowArcVisualizer);
	}

	internal static Vector3 Directionalize(BaseNote noteData)
	{
		if (noteData == null)
		{
			return Vector3.zero;
		}
		int cutDirection = noteData.CutDirection;
		Vector3 result = Directionalize(cutDirection);
		if (noteData.CustomDirection.HasValue)
		{
			result = new Vector3(0f, 0f, noteData.CustomDirection.GetValueOrDefault());
		}
		else if (noteData != null && noteData.AngleOffset != 0)
		{
			result += new Vector3(0f, 0f, noteData.AngleOffset);
		}
		else if (cutDirection >= 1000)
		{
			result += new Vector3(0f, 0f, 360 - (cutDirection - 1000));
		}
		return result;
	}

	internal static Vector3 Directionalize(int cutDirection)
	{
		Vector3 zero = Vector3.zero;
		switch (cutDirection)
		{
		case 0:
			zero += new Vector3(0f, 0f, 180f);
			break;
		case 1:
			zero += new Vector3(0f, 0f, 0f);
			break;
		case 2:
			zero += new Vector3(0f, 0f, -90f);
			break;
		case 3:
			zero += new Vector3(0f, 0f, 90f);
			break;
		case 5:
			zero += new Vector3(0f, 0f, 135f);
			break;
		case 4:
			zero += new Vector3(0f, 0f, -135f);
			break;
		case 6:
			zero += new Vector3(0f, 0f, -45f);
			break;
		case 7:
			zero += new Vector3(0f, 0f, 45f);
			break;
		}
		return zero;
	}

	public void SetDotVisible(bool b)
	{
		dotRenderer.enabled = b;
	}

	public void SetArrowVisible(bool b)
	{
		arrowRenderer.enabled = b;
	}

	public void SetModelInfer()
	{
		if (NoteData != null)
		{
			if (NoteData.Type == 3)
			{
				SetBombModel();
			}
			else
			{
				SetNoteModel();
			}
			IntersectionCollider component = DirectionTarget.GetComponent<IntersectionCollider>();
			component.BoundsRenderer = simpleBlock.GetComponent<MeshRenderer>();
			component.Mesh = component.transform.GetChild(component.transform.childCount - 1).GetComponent<MeshFilter>().mesh;
		}
	}

	public void SetNoteModel()
	{
		simpleBlock.SetActive(Settings.Instance.SimpleBlocks);
		complexBlock.SetActive(!Settings.Instance.SimpleBlocks);
		simpleChainHead.SetActive(value: false);
		complexChainHead.SetActive(value: false);
		bombRenderer.gameObject.SetActive(value: false);
		bombRenderer.enabled = false;
	}

	public void SetBombModel()
	{
		simpleBlock.SetActive(value: false);
		complexBlock.SetActive(value: false);
		simpleChainHead.SetActive(value: false);
		complexChainHead.SetActive(value: false);
		bombRenderer.gameObject.SetActive(value: true);
		bombRenderer.enabled = true;
	}

	public void SetChainHeadModel()
	{
		if (NoteData.Type != 3)
		{
			IntersectionCollider component = DirectionTarget.GetComponent<IntersectionCollider>();
			component.BoundsRenderer = simpleChainHead.GetComponent<MeshRenderer>();
			component.Mesh = component.transform.GetChild(component.transform.childCount - 2).GetComponent<MeshFilter>().mesh;
			simpleBlock.SetActive(value: false);
			complexBlock.SetActive(value: false);
			simpleChainHead.SetActive(Settings.Instance.SimpleBlocks);
			complexChainHead.SetActive(!Settings.Instance.SimpleBlocks);
			bombRenderer.gameObject.SetActive(value: false);
			bombRenderer.enabled = false;
		}
	}

	public void SetArcVisible(bool showArcVisualizer)
	{
		if (swingArcRenderer != null)
		{
			swingArcRenderer.enabled = showArcVisualizer;
		}
	}

	public static NoteContainer SpawnBeatmapNote(BaseNote noteData, ref GameObject notePrefab)
	{
		NoteContainer component = UnityEngine.Object.Instantiate(notePrefab).GetComponent<NoteContainer>();
		component.NoteData = noteData;
		component.DirectionTarget.localEulerAngles = Directionalize(noteData);
		return component;
	}

	public override void UpdateGridPosition()
	{
		if (!(Animator != null) || !Animator.AnimatedTrack)
		{
			base.transform.localPosition = (Vector3)NoteData.GetPosition() + new Vector3(0f, ObjectContainer.offsetY, NoteData.SongBpmTime * EditorScaleController.EditorScale);
		}
		base.transform.localScale = NoteData.GetScale();
		DirectionTarget.localScale = Vector3.one;
		DirectionTarget.localEulerAngles = DirectionTargetEuler;
		if (!Settings.Instance.AccurateNoteSize && NoteData.Type != 3)
		{
			DirectionTarget.localScale *= 0.9f;
		}
		if (NoteData.Type != 3)
		{
			DirectionTarget.localPosition = Vector3.zero;
		}
		UpdateCollisionGroups();
		MaterialPropertyBlock.SetFloat(objectTime, NoteData.SongBpmTime);
		ArrowMaterialPropertyBlock.SetFloat(objectTime, NoteData.SongBpmTime);
		SetRotation((base.AssignedTrack != null) ? base.AssignedTrack.RotationValue.y : 0f);
		UpdateMaterials();
	}

	public void SetColor(Color? c)
	{
		MaterialPropertyBlock.SetColor(ObjectContainer.color, c ?? unassignedColor);
		Color value = Color.Lerp(c ?? unassignedColor, Color.white, Settings.Instance.ArrowColorWhiteBlend);
		ArrowMaterialPropertyBlock.SetColor(ObjectContainer.color, value);
		MaterialPropertyBlock.SetFloat(colorMultiplier, Settings.Instance.NoteColorMultiplier);
		ArrowMaterialPropertyBlock.SetFloat(colorMultiplier, Settings.Instance.ArrowColorMultiplier);
		UpdateMaterials();
	}

	internal override void UpdateMaterials()
	{
		foreach (MeshRenderer item in noteRenderer)
		{
			item.SetPropertyBlock(MaterialPropertyBlock);
		}
		foreach (MeshRenderer item2 in chainRenderer)
		{
			item2.SetPropertyBlock(MaterialPropertyBlock);
		}
		foreach (Renderer selectionRenderer in SelectionRenderers)
		{
			selectionRenderer.SetPropertyBlock(MaterialPropertyBlock);
		}
		bombRenderer.SetPropertyBlock(MaterialPropertyBlock);
		if (dotRenderer != null)
		{
			dotRenderer.SetPropertyBlock(ArrowMaterialPropertyBlock);
			arrowRenderer.SetPropertyBlock(ArrowMaterialPropertyBlock);
		}
	}
}

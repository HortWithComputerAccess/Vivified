using System;
using System.Collections.Generic;
using Beatmap.Base;
using Beatmap.Enums;
using Beatmap.Shared;
using UnityEngine;

namespace Beatmap.Containers;

public class ChainContainer : ObjectContainer
{
	private static readonly int colorMultiplier = Shader.PropertyToID("_ColorMult");

	private static readonly int objectTime = Shader.PropertyToID("_ObjectTime");

	private static readonly int lit = Shader.PropertyToID("_Lit");

	private static readonly int translucentAlpha = Shader.PropertyToID("_TranslucentAlpha");

	[SerializeField]
	private GameObject mainObject;

	[SerializeField]
	private GameObject simpleLink;

	[SerializeField]
	private GameObject simpleLinkSolid;

	[SerializeField]
	private GameObject complexLink;

	[SerializeField]
	private GameObject complexLinkSolid;

	public NoteContainer AttachedHead;

	private readonly List<GameObject> nodes = new List<GameObject>();

	[SerializeField]
	public BaseChain ChainData;

	[SerializeField]
	private List<GameObject> indicators;

	[SerializeField]
	private GameObject tailLinkIndicator;

	[SerializeField]
	private GameObject tailSphereIndicator;

	private Vector3 headDirection;

	private bool headPointsToTail;

	private Vector3 interPoint;

	private MaterialPropertyBlock arrowMaterialPropertyBlock;

	public static readonly float PosOffsetFactor = 5f / 6f * (1f - BaseChain.ChainHeadScale.y) / 2f;

	public override BaseObject ObjectData
	{
		get
		{
			return ChainData;
		}
		set
		{
			ChainData = (BaseChain)value;
		}
	}

	public static ChainContainer SpawnChain(BaseChain data, ref GameObject prefab)
	{
		ChainContainer component = UnityEngine.Object.Instantiate(prefab).GetComponent<ChainContainer>();
		component.ChainData = data;
		return component;
	}

	public override void Setup()
	{
		base.Setup();
		SetModel();
		MaterialPropertyBlock.SetFloat(lit, (!Settings.Instance.SimpleBlocks) ? 1 : 0);
		MaterialPropertyBlock.SetFloat(translucentAlpha, Settings.Instance.PastNoteModelAlpha);
		if (arrowMaterialPropertyBlock == null)
		{
			arrowMaterialPropertyBlock = new MaterialPropertyBlock();
		}
		foreach (GameObject indicator in indicators)
		{
			indicator.GetComponent<ChainIndicatorContainer>().Setup();
		}
		UpdateMaterials();
	}

	private void SetModel()
	{
		simpleLink.SetActive(Settings.Instance.SimpleBlocks && !Settings.Instance.SolidChainLink);
		simpleLinkSolid.SetActive(Settings.Instance.SimpleBlocks && Settings.Instance.SolidChainLink);
		complexLink.SetActive(!Settings.Instance.SimpleBlocks && !Settings.Instance.SolidChainLink);
		complexLinkSolid.SetActive(!Settings.Instance.SimpleBlocks && Settings.Instance.SolidChainLink);
	}

	public void AdjustTimePlacement()
	{
		if (!(Animator != null) || !Animator.AnimatedTrack)
		{
			base.transform.localPosition = new Vector3(-1.5f, ObjectContainer.offsetY, ChainData.SongBpmTime * EditorScaleController.EditorScale);
		}
	}

	public override void UpdateGridPosition()
	{
		AdjustTimePlacement();
		GenerateChain();
		UpdateCollisionGroups();
		if (!(AttachedHead == null) && !AttachedHead.Animator.AnimatedTrack && !IsHeadNote(AttachedHead.NoteData))
		{
			AttachedHead = null;
			DetectHeadNote();
		}
	}

	public void GenerateChain(BaseChain chainData = null)
	{
		if (chainData != null)
		{
			ChainData = chainData;
		}
		Vector3 vector = (Vector3)ChainData.GetPosition() + new Vector3(1.5f, 0f, 0f);
		Vector3 vector2 = (Vector3)ChainData.GetTailPosition() + new Vector3(1.5f, 0f, 0f);
		Vector3 head = vector;
		Quaternion headRot = Quaternion.Euler(NoteContainer.Directionalize(ChainData.CutDirection));
		mainObject.transform.localPosition = vector2 + new Vector3(0f, 0f, (ChainData.TailSongBpmTime - ChainData.SongBpmTime) * EditorScaleController.EditorScale);
		float f = MathF.PI / 180f * NoteContainer.Directionalize(ChainData.CutDirection).z;
		headDirection = new Vector3(Mathf.Sin(f), 0f - Mathf.Cos(f), 0f);
		float num = (vector - vector2).magnitude / 2f;
		interPoint = vector + num * headDirection;
		Colliders.Clear();
		SelectionRenderers.Clear();
		ComputeHeadPointsToTail();
		int i;
		for (i = 0; i < ChainData.SliceCount - 2 && i < nodes.Count; i++)
		{
			nodes[i].SetActive(value: true);
			Interpolate(ChainData.SliceCount - 1, i + 1, in head, in headRot, in mainObject, nodes[i]);
			Colliders.Add(nodes[i].GetComponent<IntersectionCollider>());
			nodes[i].GetComponent<ChainComponentsFetcher>().SelectionRenderer.ForEach(SelectionRenderers.Add);
		}
		for (; i < nodes.Count; i++)
		{
			nodes[i].SetActive(value: false);
		}
		for (; i < ChainData.SliceCount - 2; i++)
		{
			GameObject linkSegment = UnityEngine.Object.Instantiate(mainObject, Animator.AnimationThis.transform);
			linkSegment.SetActive(value: true);
			ChainComponentsFetcher component = mainObject.GetComponent<ChainComponentsFetcher>();
			ChainComponentsFetcher component2 = linkSegment.GetComponent<ChainComponentsFetcher>();
			for (int j = 0; j < component.NoteRenderer.Count; j++)
			{
				component.NoteRenderer[j].sharedMaterial = component2.NoteRenderer[j].sharedMaterial;
			}
			Interpolate(ChainData.SliceCount - 1, i + 1, in head, in headRot, in mainObject, in linkSegment);
			nodes.Add(linkSegment);
			Colliders.Add(nodes[i].GetComponent<IntersectionCollider>());
			nodes[i].GetComponent<ChainComponentsFetcher>().SelectionRenderer.ForEach(SelectionRenderers.Add);
		}
		if (ChainData.SliceCount == 1)
		{
			mainObject.SetActive(value: false);
		}
		else
		{
			mainObject.SetActive(value: true);
			Interpolate(ChainData.SliceCount - 1, ChainData.SliceCount - 1, in head, in headRot, in mainObject, in mainObject);
			Colliders.Add(mainObject.GetComponent<IntersectionCollider>());
			mainObject.GetComponent<ChainComponentsFetcher>().SelectionRenderer.ForEach(SelectionRenderers.Add);
		}
		Vector3 one = Vector3.one;
		if (!Settings.Instance.AccurateNoteSize)
		{
			one *= 0.9f;
		}
		foreach (GameObject node in nodes)
		{
			node.transform.localScale = one;
		}
		mainObject.transform.localScale = one;
		tailLinkIndicator.transform.localScale = one;
		UpdateMaterials();
		ResetIndicatorsPosition();
	}

	private void ComputeHeadPointsToTail()
	{
		Vector2 to = ChainData.GetTailPosition() - ChainData.GetPosition() + new Vector2(1.5f, 0f);
		float num = Vector2.SignedAngle(Vector2.down, to);
		float z = NoteContainer.Directionalize(ChainData.CutDirection).z;
		headPointsToTail = Mathf.Abs(num - z) < 0.01f;
	}

	private void Interpolate(int n, int i, in Vector3 head, in Quaternion headRot, in GameObject tail, in GameObject linkSegment)
	{
		float num = ((ChainData.Squish < 0.001f) ? 1f : ChainData.Squish);
		float num2 = (float)i / (float)n;
		float num3 = num2 * num;
		Vector3 vector = head;
		Vector3 vector2 = interPoint;
		Vector3 localPosition = tail.transform.localPosition;
		float z = Mathf.Lerp(head.z, tail.transform.localPosition.z, num2);
		if (headPointsToTail)
		{
			Vector3 vector3 = Vector3.LerpUnclamped(head, tail.transform.localPosition, num3);
			linkSegment.transform.localPosition = new Vector3(vector3.x, vector3.y, z);
			linkSegment.transform.localRotation = headRot;
		}
		else
		{
			Vector3 vector4 = Mathf.Pow(1f - num3, 2f) * vector + 2f * (1f - num3) * num3 * vector2 + Mathf.Pow(num3, 2f) * localPosition;
			linkSegment.transform.localPosition = new Vector3(vector4.x, vector4.y, z);
			Vector3 vector5 = 2f * (1f - num3) * (vector2 - vector) + 2f * num3 * (localPosition - vector2);
			linkSegment.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f + 57.29578f * Mathf.Atan2(vector5.y, vector5.x)));
		}
	}

	public void SetColor(Color c)
	{
		MaterialPropertyBlock.SetColor(ObjectContainer.color, c);
		Color value = Color.Lerp(c, Color.white, Settings.Instance.ArrowColorWhiteBlend);
		arrowMaterialPropertyBlock.SetColor(ObjectContainer.color, value);
		MaterialPropertyBlock.SetFloat(colorMultiplier, Settings.Instance.NoteColorMultiplier);
		arrowMaterialPropertyBlock.SetFloat(colorMultiplier, Settings.Instance.ArrowColorMultiplier);
		UpdateMaterials();
	}

	internal override void UpdateMaterials()
	{
		foreach (IntersectionCollider collider in Colliders)
		{
			ChainComponentsFetcher component = collider.GetComponent<ChainComponentsFetcher>();
			MeshRenderer dotRenderer = component.DotRenderer;
			float value = ChainData.SongBpmTime + collider.transform.localPosition.z / EditorScaleController.EditorScale;
			MaterialPropertyBlock.SetFloat(objectTime, value);
			arrowMaterialPropertyBlock.SetFloat(objectTime, value);
			float value2 = ((UIMode.SelectedMode == UIModeType.Preview || UIMode.SelectedMode == UIModeType.Playing) ? 0f : Settings.Instance.PastNoteModelAlpha);
			MaterialPropertyBlock.SetFloat(translucentAlpha, value2);
			arrowMaterialPropertyBlock.SetFloat(translucentAlpha, value2);
			component.NoteRenderer.ForEach(delegate(MeshRenderer r)
			{
				r.SetPropertyBlock(MaterialPropertyBlock);
			});
			dotRenderer.SetPropertyBlock(arrowMaterialPropertyBlock);
		}
		foreach (Renderer selectionRenderer in SelectionRenderers)
		{
			selectionRenderer.SetPropertyBlock(MaterialPropertyBlock);
		}
		foreach (GameObject indicator in indicators)
		{
			indicator.GetComponent<ChainIndicatorContainer>().UpdateMaterials(MaterialPropertyBlock);
		}
		foreach (GameObject indicator2 in indicators)
		{
			indicator2.GetComponent<ChainIndicatorContainer>().OutlineVisible = base.OutlineVisible;
		}
	}

	public void DetectHeadNote(bool detect = true)
	{
		if (ChainData == null)
		{
			return;
		}
		if (detect && AttachedHead == null)
		{
			NoteGridContainer collectionForType = BeatmapObjectContainerCollection.GetCollectionForType<NoteGridContainer>(ObjectType.Note);
			Span<BaseNote> between = collectionForType.GetBetween(ChainData.JsonTime - 0.1f, ChainData.JsonTime + 0.1f);
			for (int i = 0; i < between.Length; i++)
			{
				BaseNote baseNote = between[i];
				if (baseNote.ObjectType == ObjectType.Note && baseNote.HasAttachedContainer && IsHeadNote(baseNote))
				{
					collectionForType.LoadedContainers.TryGetValue(baseNote, out var value);
					AttachedHead = value as NoteContainer;
					AttachedHead.SetChainHeadModel();
					break;
				}
			}
		}
		else
		{
			if (!(AttachedHead != null))
			{
				return;
			}
			if (!IsHeadNote(AttachedHead.NoteData))
			{
				if (AttachedHead.NoteData != null)
				{
					AttachedHead.SetModelInfer();
				}
				AttachedHead = null;
				DetectHeadNote();
			}
			else
			{
				AttachedHead.SetChainHeadModel();
			}
		}
	}

	public void DetachHeadNote()
	{
		if (!(AttachedHead == null) && AttachedHead.NoteData != null)
		{
			AttachedHead.SetModelInfer();
			AttachedHead = null;
		}
	}

	public bool IsHeadNote(BaseNote baseNote)
	{
		if (baseNote == null)
		{
			return false;
		}
		Vector2 position = baseNote.GetPosition();
		Vector2 position2 = ChainData.GetPosition();
		if (Mathf.Abs(baseNote.JsonTime - ChainData.JsonTime) < BeatmapObjectContainerCollection.Epsilon && (double)Vector2.Distance(position, position2) < 0.1)
		{
			return baseNote.Type == ChainData.Color;
		}
		return false;
	}

	public void SetIndicatorBlocksActive(bool visible)
	{
		indicators[0].SetActive(visible);
		tailSphereIndicator.SetActive(visible && ChainData.SliceCount == 1);
		tailLinkIndicator.SetActive(visible && ChainData.SliceCount != 1);
	}

	private void ResetIndicatorsPosition()
	{
		tailSphereIndicator.SetActive(ChainData.SliceCount == 1);
		tailLinkIndicator.SetActive(ChainData.SliceCount != 1);
		foreach (GameObject indicator in indicators)
		{
			if (indicator.activeSelf)
			{
				indicator.GetComponent<ChainIndicatorContainer>().UpdateGridPosition();
			}
		}
	}

	public Quaternion GetTailNodeRotation()
	{
		return mainObject.transform.rotation;
	}
}

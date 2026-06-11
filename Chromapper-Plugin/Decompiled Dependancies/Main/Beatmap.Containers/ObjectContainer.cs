using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Animations;
using Beatmap.Base;
using UnityEngine;

namespace Beatmap.Containers;

public abstract class ObjectContainer : MonoBehaviour
{
	public static Action<ObjectContainer, bool, string> FlaggedForDeletionEvent;

	internal static readonly int color = Shader.PropertyToID("_Color");

	internal static readonly int rotation = Shader.PropertyToID("_Rotation");

	internal static readonly int outline = Shader.PropertyToID("_Outline");

	internal static readonly int outlineColor = Shader.PropertyToID("_OutlineColor");

	protected static readonly float offsetY = 1.1f;

	public bool Dragging;

	[SerializeField]
	protected List<IntersectionCollider> Colliders;

	[SerializeField]
	protected List<Renderer> SelectionRenderers = new List<Renderer>();

	[SerializeField]
	protected BoxCollider BoxCollider;

	[SerializeField]
	public ObjectAnimator Animator;

	protected readonly List<Renderer> modelRenderers = new List<Renderer>();

	public MaterialPropertyBlock MaterialPropertyBlock;

	internal bool selectionStateChanged;

	public bool OutlineVisible
	{
		get
		{
			return MaterialPropertyBlock.GetFloat(outline) != 0f;
		}
		set
		{
			SelectionRenderers.ForEach(delegate(Renderer r)
			{
				r.enabled = value;
			});
			MaterialPropertyBlock.SetFloat(outline, value ? 0.05f : 0f);
			UpdateMaterials();
		}
	}

	public Track AssignedTrack { get; private set; }

	public abstract BaseObject ObjectData { get; set; }

	public int ChunkID => (int)(ObjectData.JsonTime / 1f);

	public abstract void UpdateGridPosition();

	public virtual void Setup()
	{
		if (MaterialPropertyBlock == null)
		{
			MaterialPropertyBlock = new MaterialPropertyBlock();
			modelRenderers.AddRange(from x in GetComponentsInChildren<Renderer>(includeInactive: true)
				where !(x is SpriteRenderer)
				select x);
		}
	}

	internal virtual void SafeSetActive(bool active)
	{
		if (active != base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(active);
		}
	}

	internal void SafeSetBoxCollider(bool con)
	{
		if (!(BoxCollider == null) && con != BoxCollider.isTrigger)
		{
			BoxCollider.isTrigger = con;
		}
	}

	internal virtual void UpdateMaterials()
	{
		foreach (Renderer modelRenderer in modelRenderers)
		{
			modelRenderer.SetPropertyBlock(MaterialPropertyBlock);
		}
	}

	public void SetRotation(float rot)
	{
		MaterialPropertyBlock.SetFloat(rotation, rot);
		UpdateMaterials();
	}

	public void SetOutlineColor(Color color, bool automaticallyShowOutline = true)
	{
		if (automaticallyShowOutline)
		{
			OutlineVisible = true;
		}
		MaterialPropertyBlock.SetColor(outlineColor, color);
		UpdateMaterials();
	}

	public virtual void AssignTrack(Track track)
	{
		AssignedTrack = track;
	}

	protected virtual void UpdateCollisionGroups()
	{
		int chunkID = ChunkID;
		foreach (IntersectionCollider collider in Colliders)
		{
			bool flag = Intersections.UnregisterColliderFromGroups(collider);
			collider.CollisionGroups.Clear();
			collider.CollisionGroups.Add(chunkID);
			if (flag)
			{
				Intersections.RegisterColliderToGroups(collider);
			}
		}
	}
}

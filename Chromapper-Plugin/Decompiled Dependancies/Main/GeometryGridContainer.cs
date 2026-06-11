using System;
using System.Linq;
using Beatmap.Appearances;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Containers;
using Beatmap.Enums;
using UnityEngine;

public class GeometryGridContainer : BeatmapObjectContainerCollection<BaseEnvironmentEnhancement>
{
	[SerializeField]
	private GameObject geometryPrefab;

	[SerializeField]
	private GeometryAppearanceSO geometryAppearanceSo;

	[SerializeField]
	private TracksManager tracksManager;

	public override ObjectType ContainerType => ObjectType.EnvironmentEnhancement;

	protected override void OnObjectSpawned(BaseObject obj, bool inCollection = false)
	{
		BaseEnvironmentEnhancement baseEnvironmentEnhancement = obj as BaseEnvironmentEnhancement;
		if ((object)baseEnvironmentEnhancement.Geometry == null)
		{
			return;
		}
		try
		{
			GeometryContainer geometryContainer = GeometryContainer.SpawnGeometry(baseEnvironmentEnhancement, ref geometryPrefab);
			if (!(geometryContainer == null))
			{
				geometryContainer.Setup();
				LoadedContainers.Add(baseEnvironmentEnhancement, geometryContainer);
				ObjectsWithContainers.Add(baseEnvironmentEnhancement);
				geometryAppearanceSo.SetGeometryAppearance(geometryContainer);
				geometryContainer.OutlineVisible = SelectionController.IsObjectSelected(obj);
			}
		}
		catch (Exception exception)
		{
			Debug.LogError("Error in geometry:");
			Debug.LogException(exception);
		}
	}

	protected override void OnObjectDelete(BaseObject obj, bool inCollection = false)
	{
		BaseEnvironmentEnhancement baseEnvironmentEnhancement = obj as BaseEnvironmentEnhancement;
		if (LoadedContainers.ContainsKey(baseEnvironmentEnhancement))
		{
			UnityEngine.Object.DestroyImmediate(LoadedContainers[baseEnvironmentEnhancement].gameObject);
			LoadedContainers.Remove(baseEnvironmentEnhancement);
			ObjectsWithContainers.Remove(baseEnvironmentEnhancement);
		}
	}

	public override void RefreshPool(bool force)
	{
		if (!force)
		{
			return;
		}
		foreach (BaseObject item in LoadedContainers.Keys.ToList())
		{
			OnObjectDelete(item);
		}
		foreach (BaseEnvironmentEnhancement mapObject in MapObjects)
		{
			if (mapObject.HasMatchingTrack(BeatmapObjectContainerCollection.TrackFilterID))
			{
				OnObjectSpawned(mapObject);
			}
		}
	}

	public override void RefreshPool(float lowerBound, float upperBound, bool forceRefresh = false)
	{
	}

	internal override void SubscribeToCallbacks()
	{
	}

	internal override void UnsubscribeToCallbacks()
	{
	}

	public override ObjectContainer CreateContainer()
	{
		return null;
	}
}

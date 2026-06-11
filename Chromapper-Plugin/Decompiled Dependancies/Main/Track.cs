using System;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.V2;
using SimpleJSON;
using UnityEngine;

public class Track : MonoBehaviour
{
	public Transform ObjectParentTransform;

	public Vector3 RotationValue = Vector3.zero;

	public Action TimeChanged;

	private readonly Vector3 rotationPoint = LoadInitialMap.PlatformOffset;

	public BaseGrid Object;

	private float spawnPosition;

	private float despawnPosition;

	private float despawnTime;

	private const float JUMP_FAR = 500f;

	public const float JUMP_TIME = 2f;

	public void AssignRotationValue(Vector3 rotation)
	{
		RotationValue = rotation;
		base.transform.RotateAround(rotationPoint, Vector3.right, RotationValue.x);
		base.transform.RotateAround(rotationPoint, Vector3.up, RotationValue.y);
		base.transform.RotateAround(rotationPoint, Vector3.forward, RotationValue.z);
	}

	public void UpdatePosition(float position)
	{
		ObjectParentTransform.localPosition = new Vector3(ObjectParentTransform.localPosition.x, ObjectParentTransform.localPosition.y, position);
		TimeChanged?.Invoke();
	}

	public void UpdateTime(float time)
	{
		float num = 0f;
		bool flag = Object is V2Object;
		Vector3 localPosition = ObjectParentTransform.localPosition;
		num = ((time < Object.SpawnSongBpmTime) ? (((bool)(Object.CustomSpawnEffect ?? ((JSONNode)(!flag))) ^ flag) ? Mathf.Lerp(spawnPosition, 500f, (Object.SpawnSongBpmTime - time) / 2f) : 500f) : ((!(time < despawnTime)) ? Mathf.Lerp(despawnPosition, -500f, (time - despawnTime) / 2f) : Mathf.Lerp(spawnPosition, despawnPosition, (time - Object.SpawnSongBpmTime) / (despawnTime - Object.SpawnSongBpmTime))));
		localPosition.z = num;
		if (Object is BaseNote baseNote)
		{
			float num2 = Mathf.Clamp01(Mathf.InverseLerp(Object.DespawnSongBpmTime, Object.SpawnSongBpmTime, time));
			float num3 = Mathf.Clamp01(1f - (num2 - 0.5f) * 2f);
			float k = Mathf.Clamp01(num3 / 0.3f);
			float t = Easing.Quadratic.Out(num3);
			float t2 = Easing.Quadratic.Out(k);
			localPosition.y = Mathf.Lerp(1.1f, baseNote.GetPosition().y + 1.1f, t);
			if (num2 >= 0.5f && BeatmapObjectContainerCollection.GetCollectionForType(baseNote.ObjectType).LoadedContainers.TryGetValue(Object, out var value) && value is NoteContainer noteContainer)
			{
				Quaternion b = Quaternion.Euler(noteContainer.DirectionTargetEuler);
				noteContainer.DirectionTarget.localRotation = Quaternion.Lerp(Quaternion.identity, b, t2);
			}
		}
		ObjectParentTransform.localPosition = localPosition;
	}

	public void AttachContainer(ObjectContainer obj)
	{
		UpdateMaterialRotation(obj);
		if (obj.transform.parent == ObjectParentTransform)
		{
			return;
		}
		obj.transform.SetParent(ObjectParentTransform, worldPositionStays: false);
		obj.AssignTrack(this);
		if (obj.ObjectData is BaseGrid baseGrid)
		{
			Object = baseGrid;
			spawnPosition = Object.Jd;
			if (Object is BaseObstacle baseObstacle)
			{
				despawnPosition = 0f - Object.Jd * 0.5f - baseObstacle.DurationSongBpm * baseObstacle.EditorScale;
				despawnTime = baseObstacle.SongBpmTime + baseObstacle.DurationSongBpm + baseObstacle.Hjd * 0.5f;
			}
			else
			{
				despawnPosition = 0f - Object.Jd;
				despawnTime = Object.DespawnSongBpmTime;
			}
		}
	}

	public void UpdateMaterialRotation(ObjectContainer obj)
	{
		if (obj is ObstacleContainer || obj is NoteContainer)
		{
			obj.SetRotation(RotationValue.y);
		}
	}
}

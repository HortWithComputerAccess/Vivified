using Beatmap.Base;
using Beatmap.Shared;
using UnityEngine;

namespace Beatmap.Containers;

public class ObstacleContainer : ObjectContainer
{
	private static readonly int colorTint = Shader.PropertyToID("_ColorTint");

	private static readonly int shaderScale = Shader.PropertyToID("_WorldScale");

	private static readonly int handleScale = Shader.PropertyToID("_HandleScale");

	[SerializeField]
	private TracksManager manager;

	[SerializeField]
	public BaseObstacle ObstacleData;

	public override BaseObject ObjectData
	{
		get
		{
			return ObstacleData;
		}
		set
		{
			ObstacleData = (BaseObstacle)value;
		}
	}

	public bool IsRotatedByNoodleExtensions => ObstacleData.CustomWorldRotation != null;

	public static ObstacleContainer SpawnObstacle(BaseObstacle data, TracksManager manager, ref GameObject prefab)
	{
		ObstacleContainer component = Object.Instantiate(prefab).GetComponent<ObstacleContainer>();
		component.ObstacleData = data;
		component.manager = manager;
		return component;
	}

	public void SetColor(Color c)
	{
		MaterialPropertyBlock.SetColor(colorTint, c);
		UpdateMaterials();
	}

	public void SetScale(Vector3 scale)
	{
		Animator.LocalTarget.localScale = scale;
		MaterialPropertyBlock.SetVector(shaderScale, scale);
		MaterialPropertyBlock.SetFloat(handleScale, 1f);
		UpdateMaterials();
	}

	public Vector3 GetScale()
	{
		return Animator.LocalTarget.localScale;
	}

	public float GetLength()
	{
		if (ObstacleData.CustomSize != null && ObstacleData.CustomSize.IsArray && ObstacleData.CustomSize[2].IsNumber)
		{
			return ObstacleData.CustomSize[2];
		}
		float num = ObstacleData.DurationSongBpm;
		if (ObstacleData.Duration < 0f && Settings.Instance.ShowMoreAccurateFastWalls && !UIMode.AnimationMode)
		{
			num -= num * Mathf.Abs(num / ObstacleData.Hjd);
		}
		return num * (UIMode.AnimationMode ? ObstacleData.EditorScale : EditorScaleController.EditorScale);
	}

	public (Vector3 size, Vector3 position) ReadSizePosition()
	{
		float z = Mathf.Abs(GetLength());
		ObstacleBounds shape = ObstacleData.GetShape();
		return (size: new Vector3(Mathf.Abs(shape.Width), Mathf.Abs(shape.Height), z), position: new Vector3(shape.Position + shape.Width / 2f, shape.StartHeight + ((shape.Height < 0f) ? shape.Height : 0f), 0f));
	}

	public override void UpdateGridPosition()
	{
		Vector3 vector = Vector3.zero;
		float length = GetLength();
		var (scale, localPosition) = ReadSizePosition();
		if (ObstacleData.CustomLocalRotation != null)
		{
			vector = ObstacleData.CustomLocalRotation.ReadVector3();
		}
		if (ObstacleData.CustomWorldRotation != null && !Animator.AnimatedTrack)
		{
			if (ObstacleData.CustomWorldRotation.IsNumber)
			{
				manager.CreateTrack(new Vector3(0f, ObstacleData.CustomWorldRotation, 0f)).AttachContainer(this);
			}
			else
			{
				manager.CreateTrack(ObstacleData.CustomWorldRotation.ReadVector3()).AttachContainer(this);
			}
		}
		base.transform.localPosition = new Vector3(0f, 0.1f, ObstacleData.SongBpmTime * EditorScaleController.EditorScale + ((length < 0f) ? length : 0f));
		Animator.LocalTarget.localPosition = localPosition;
		SetScale(scale);
		if (vector != Vector3.zero)
		{
			Animator.LocalTarget.localEulerAngles = vector;
		}
		UpdateCollisionGroups();
	}
}

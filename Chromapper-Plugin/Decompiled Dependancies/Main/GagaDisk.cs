using UnityEngine;

public class GagaDisk : MonoBehaviour
{
	public int HeightEventType;

	private Vector3 basePosition;

	private float prevPosY;

	private float destPosY;

	private float startTime;

	private float destTime;

	public void Init(float positionY = 20f)
	{
		destPosY = (prevPosY = positionY);
		basePosition = base.gameObject.transform.position;
	}

	public void LateUpdateDisk(float jsonTime)
	{
		float t = Easing.Cubic.InOut(LerpTime(startTime, destTime, jsonTime));
		base.transform.position = new Vector3(basePosition.x, Mathf.Lerp(prevPosY, destPosY, t), basePosition.z);
	}

	public void SetPosition(int startValue, int destinationValue, float timeStart, float timeDest)
	{
		prevPosY = GetPositionForValue(startValue);
		destPosY = GetPositionForValue(destinationValue);
		startTime = timeStart;
		destTime = timeDest;
	}

	private float LerpTime(float timeStart, float targetTime, float x)
	{
		float num = Mathf.Clamp01((x - timeStart) / (targetTime - timeStart));
		if (float.IsNaN(num))
		{
			return 0f;
		}
		return num;
	}

	private int GetPositionForValue(int value)
	{
		return value * 6 - 4;
	}
}

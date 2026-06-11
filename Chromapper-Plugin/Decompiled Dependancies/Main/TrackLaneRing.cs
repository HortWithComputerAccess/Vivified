using UnityEngine;

public class TrackLaneRing : MonoBehaviour
{
	private float destPosZ;

	private float destRotZ;

	private float moveSpeed;

	private Vector3 positionOffset;

	private float posZ;

	private float prevPosZ;

	private float prevRotZ;

	private float rotateSpeed;

	private float rotZ;

	public void Reset()
	{
		rotZ = 0f;
		prevRotZ = 0f;
		destRotZ = 0f;
		rotateSpeed = 0f;
	}

	public void Init(Vector3 pos, Vector3 posOffset)
	{
		positionOffset = posOffset;
		base.transform.localPosition = pos + positionOffset;
		prevPosZ = (posZ = pos.z + positionOffset.z);
		rotZ = (destRotZ = base.transform.localPosition.z);
	}

	public void FixedUpdateRing(float fixedDeltaTime)
	{
		prevRotZ = rotZ;
		rotZ = Mathf.Lerp(rotZ, destRotZ, fixedDeltaTime * rotateSpeed);
		prevPosZ = posZ;
		posZ = Mathf.Lerp(posZ, positionOffset.z + destPosZ, fixedDeltaTime * moveSpeed);
	}

	public void LateUpdateRing(float interpolationFactor)
	{
		base.transform.localEulerAngles = new Vector3(0f, 0f, prevRotZ + (rotZ - prevRotZ) * interpolationFactor);
		base.transform.localPosition = new Vector3(positionOffset.x, positionOffset.y, prevPosZ + (posZ - prevPosZ) * interpolationFactor);
	}

	public void SetRotation(float destinationZ, float rotateSpeed)
	{
		destRotZ = destinationZ;
		this.rotateSpeed = rotateSpeed;
	}

	public float GetRotation()
	{
		return rotZ;
	}

	public float GetDestinationRotation()
	{
		return destRotZ;
	}

	public void SetPosition(float destinationZ, float moveSpeed)
	{
		destPosZ = destinationZ;
		this.moveSpeed = moveSpeed;
	}
}

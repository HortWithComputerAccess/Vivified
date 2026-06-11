using UnityEngine;
using UnityEngine.Serialization;

public class RotatingWheel : MonoBehaviour
{
	[FormerlySerializedAs("spinSpeed")]
	public float SpinSpeed = 25f;

	private void Update()
	{
		base.transform.localEulerAngles += Vector3.back * (SpinSpeed * Time.deltaTime);
	}
}

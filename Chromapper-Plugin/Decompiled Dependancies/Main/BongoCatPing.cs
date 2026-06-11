using UnityEngine;

public class BongoCatPing : MonoBehaviour
{
	private CanvasGroup canvasGroup;

	private void Start()
	{
		canvasGroup = base.gameObject.GetComponent<CanvasGroup>();
		base.transform.localPosition = new Vector3(Random.Range(-1f, 1f) * (1f / base.transform.lossyScale.x), 0f, Random.Range(-0.25f, 0.25f) + 1f);
		base.transform.localEulerAngles = Vector3.right * 90f;
		base.transform.localScale = new Vector3(1f, base.transform.localScale.y, base.transform.localScale.z);
	}

	private void Update()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.z += Time.deltaTime;
		base.transform.localPosition = localPosition;
		canvasGroup.alpha -= Time.deltaTime;
		if (canvasGroup.alpha <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}

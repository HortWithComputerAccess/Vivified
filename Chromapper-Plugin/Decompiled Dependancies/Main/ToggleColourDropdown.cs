using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ToggleColourDropdown : MonoBehaviour
{
	[FormerlySerializedAs("ColourDropdown")]
	[SerializeField]
	private RectTransform colourDropdown;

	public float YTop = 90f;

	public float YBottom = -50f;

	public bool Visible;

	public void ToggleDropdown(bool visible)
	{
		base.gameObject.SetActive(value: true);
		StopAllCoroutines();
		Visible = visible;
		StartCoroutine(UpdateGroup(visible, colourDropdown));
	}

	private IEnumerator UpdateGroup(bool enabled, RectTransform group)
	{
		float dest = (enabled ? YBottom : YTop);
		float og = group.anchoredPosition.y;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			group.anchoredPosition = new Vector2(group.anchoredPosition.x, Mathf.Lerp(og, dest, t));
			og = group.anchoredPosition.y;
			yield return new WaitForEndOfFrame();
		}
		group.anchoredPosition = new Vector2(group.anchoredPosition.x, dest);
		if (!enabled)
		{
			group.gameObject.SetActive(value: false);
		}
	}
}

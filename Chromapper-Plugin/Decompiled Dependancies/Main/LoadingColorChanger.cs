using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingColorChanger : MonoBehaviour
{
	public Color[] Colors;

	public float TimeBetweenColorChanges = 1f;

	public float ColorFadeTime = 1f;

	private Image image;

	private Color oldColor;

	private void Start()
	{
		image = GetComponent<Image>();
		oldColor = image.color;
		StartCoroutine(ChangeColors());
	}

	private IEnumerator ChangeColors()
	{
		while (true)
		{
			yield return new WaitForSeconds(TimeBetweenColorChanges);
			yield return StartCoroutine(ChangeColor());
			oldColor = image.color;
		}
	}

	private IEnumerator ChangeColor()
	{
		Color color = oldColor;
		while (color == oldColor)
		{
			color = Colors[Random.Range(0, Colors.Length)];
		}
		float t = 0f;
		while (t < ColorFadeTime)
		{
			Color color2 = Color.Lerp(oldColor, color, t / ColorFadeTime);
			t += Time.deltaTime;
			image.color = color2;
			yield return new WaitForEndOfFrame();
		}
		image.color = color;
	}
}

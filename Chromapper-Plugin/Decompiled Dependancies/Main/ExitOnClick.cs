using System.Collections;
using UnityEngine;

public class ExitOnClick : MonoBehaviour
{
	public void OnClick()
	{
		StartCoroutine(Exit());
	}

	private IEnumerator Exit()
	{
		yield return PersistentUI.Instance.FadeInLoadingScreen();
		yield return new WaitForSeconds(1f);
		Application.Quit();
	}
}

using System.Collections;
using QuestDumper;
using UnityEngine;

[RequireComponent(typeof(BetterToggle))]
public class AdbHandler : MonoBehaviour
{
	private BetterToggle betterToggle;

	private void Start()
	{
		betterToggle = GetComponent<BetterToggle>();
		betterToggle.IsOn = Adb.IsAdbInstalled(out var _);
		betterToggle.UpdateUI();
	}

	public void ToggleADB()
	{
		StartCoroutine(ToggleADBCoroutine(Adb.IsAdbInstalled(out var _) ? AdbUI.DoRemove() : AdbUI.DoDownload()));
	}

	private IEnumerator ToggleADBCoroutine(IEnumerator enumerator)
	{
		yield return enumerator;
		betterToggle.SetUiOn(Adb.IsAdbInstalled(out var _), notifyChange: false);
	}
}

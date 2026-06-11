using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisableActionsField : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public void OnDeselect(BaseEventData eventData)
	{
		OnDeselect();
	}

	public void OnSelect(BaseEventData eventData)
	{
		OnSelect();
	}

	public void OnSelect()
	{
		StartCoroutine(WaitToEnable());
	}

	public void OnDeselect()
	{
		CMInputCallbackInstaller.ClearDisabledActionMaps(GetType(), from x in typeof(CMInput).GetNestedTypes()
			where x.IsInterface
			select x);
	}

	private IEnumerator WaitToEnable()
	{
		yield return new WaitForEndOfFrame();
		CMInputCallbackInstaller.DisableActionMaps(GetType(), from x in typeof(CMInput).GetNestedTypes()
			where x.IsInterface
			select x);
	}
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class CustomStandaloneInputModule : InputSystemUIInputModule
{
	public bool IsPointerOverGameObject<T>(int pointerId, bool includeDerived = false) where T : BaseRaycaster
	{
		Debug.unityLogger.logEnabled = false;
		bool num = IsPointerOverGameObject(pointerId);
		Debug.unityLogger.logEnabled = true;
		if (!num)
		{
			return false;
		}
		RaycastResult lastRaycastResult = GetLastRaycastResult(pointerId);
		if (!includeDerived)
		{
			return lastRaycastResult.module.GetType() == typeof(T);
		}
		return lastRaycastResult.module is T;
	}
}

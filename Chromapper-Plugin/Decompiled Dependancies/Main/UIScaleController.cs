using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScaleController : MonoBehaviour
{
	private readonly Dictionary<CanvasScaler, Vector2> scalers = new Dictionary<CanvasScaler, Vector2>();

	private void Start()
	{
		CanvasScaler[] componentsInChildren = GetComponentsInChildren<CanvasScaler>();
		foreach (CanvasScaler canvasScaler in componentsInChildren)
		{
			scalers.Add(canvasScaler, canvasScaler.referenceResolution);
		}
		Settings.NotifyBySettingName("UIScale", RecalculateScale);
		RecalculateScale(Settings.Instance.UIScale);
	}

	private void RecalculateScale(object obj)
	{
		float num = Convert.ToSingle(obj);
		foreach (KeyValuePair<CanvasScaler, Vector2> scaler in scalers)
		{
			scaler.Key.referenceResolution = scaler.Value * num;
		}
	}
}

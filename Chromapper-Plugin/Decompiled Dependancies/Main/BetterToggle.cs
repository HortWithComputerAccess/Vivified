using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BetterToggle : UIBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[Serializable]
	public class ToggleEvent : UnityEvent<bool>
	{
	}

	private const float slideSpeed = 0.2f;

	[FormerlySerializedAs("background")]
	public Image Background;

	[FormerlySerializedAs("switchTransform")]
	public RectTransform SwitchTransform;

	[FormerlySerializedAs("description")]
	public TextMeshProUGUI Description;

	[FormerlySerializedAs("isOn")]
	public bool IsOn;

	[FormerlySerializedAs("OnColor")]
	[HideInInspector]
	public Color Color;

	[HideInInspector]
	public Color OffColor;

	[FormerlySerializedAs("onValueChanged")]
	public ToggleEvent OnValueChanged = new ToggleEvent();

	private readonly Vector3 offPos = new Vector3(-35f, 0f, 0f);

	private readonly Vector3 onPos = new Vector3(-15f, 0f, 0f);

	private Coroutine slideButtonCoroutine;

	private Coroutine slideColorCoroutine;

	protected override void Start()
	{
		if (TryGetComponent<SettingsBinder>(out var component))
		{
			IsOn = (bool?)component.RetrieveValueFromSettings() == true;
			UpdateUI();
		}
		base.Start();
	}

	public void UpdateUI()
	{
		SwitchTransform.localPosition = (IsOn ? onPos : offPos);
		Background.color = (IsOn ? Color : OffColor);
	}

	public void SetUiOn(bool isOn, bool notifyChange = true)
	{
		IsOn = isOn;
		slideButtonCoroutine = StartCoroutine(SlideToggle());
		slideColorCoroutine = StartCoroutine(SlideColor());
		if (notifyChange)
		{
			OnValueChanged?.Invoke(IsOn);
			SendMessage("SendValueToSettings", IsOn, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		SetUiOn(!IsOn);
	}

	private IEnumerator SlideToggle()
	{
		if (slideButtonCoroutine != null)
		{
			StopCoroutine(slideButtonCoroutine);
		}
		float startTime = Time.time;
		while (true)
		{
			Vector3 localPosition = SwitchTransform.localPosition;
			localPosition = Vector3.Lerp(localPosition, IsOn ? onPos : offPos, Time.time / startTime * 0.2f);
			SwitchTransform.localPosition = localPosition;
			if (!(SwitchTransform.localPosition == onPos) && !(SwitchTransform.localPosition == offPos))
			{
				yield return new WaitForFixedUpdate();
				continue;
			}
			break;
		}
	}

	private IEnumerator SlideColor()
	{
		if (slideColorCoroutine != null)
		{
			StopCoroutine(slideColorCoroutine);
		}
		float startTime = Time.time;
		while (true)
		{
			Color color = Background.color;
			color = Color.Lerp(color, IsOn ? Color : OffColor, Time.time / startTime * 0.2f);
			Background.color = color;
			if (!(Background.color == Color) && !(Background.color == OffColor))
			{
				yield return new WaitForFixedUpdate();
				continue;
			}
			break;
		}
	}
}

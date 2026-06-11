using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI;

[AddComponentMenu("UI/BoxSlider", 35)]
[RequireComponent(typeof(RectTransform))]
public class BoxSlider : Selectable, IDragHandler, IEventSystemHandler, IInitializePotentialDragHandler, ICanvasElement
{
	public enum Direction
	{
		LeftToRight,
		RightToLeft,
		BottomToTop,
		TopToBottom
	}

	[Serializable]
	public class BoxSliderEvent : UnityEvent<float, float>
	{
	}

	private enum Axis
	{
		Horizontal,
		Vertical
	}

	[FormerlySerializedAs("m_HandleRect")]
	[SerializeField]
	private RectTransform mHandleRect;

	[FormerlySerializedAs("m_MinValue")]
	[Space(6f)]
	[SerializeField]
	private float mMinValue;

	[FormerlySerializedAs("m_MaxValue")]
	[SerializeField]
	private float mMaxValue = 1f;

	[FormerlySerializedAs("m_WholeNumbers")]
	[SerializeField]
	private bool mWholeNumbers;

	[FormerlySerializedAs("m_Value")]
	[SerializeField]
	private float mValue = 1f;

	[FormerlySerializedAs("m_ValueY")]
	[SerializeField]
	private float mValueY = 1f;

	[FormerlySerializedAs("m_OnValueChanged")]
	[Space(6f)]
	[SerializeField]
	private BoxSliderEvent mOnValueChanged = new BoxSliderEvent();

	private RectTransform mHandleContainerRect;

	private Transform mHandleTransform;

	private Vector2 mOffset = Vector2.zero;

	private DrivenRectTransformTracker mTracker;

	public RectTransform HandleRect
	{
		get
		{
			return mHandleRect;
		}
		set
		{
			if (SetClass(ref mHandleRect, value))
			{
				UpdateCachedReferences();
				UpdateVisuals();
			}
		}
	}

	public float MINValue
	{
		get
		{
			return mMinValue;
		}
		set
		{
			if (SetStruct(ref mMinValue, value))
			{
				Set(mValue);
				SetY(mValueY);
				UpdateVisuals();
			}
		}
	}

	public float MAXValue
	{
		get
		{
			return mMaxValue;
		}
		set
		{
			if (SetStruct(ref mMaxValue, value))
			{
				Set(mValue);
				SetY(mValueY);
				UpdateVisuals();
			}
		}
	}

	public bool WholeNumbers
	{
		get
		{
			return mWholeNumbers;
		}
		set
		{
			if (SetStruct(ref mWholeNumbers, value))
			{
				Set(mValue);
				SetY(mValueY);
				UpdateVisuals();
			}
		}
	}

	public float Value
	{
		get
		{
			if (WholeNumbers)
			{
				return Mathf.Round(mValue);
			}
			return mValue;
		}
		set
		{
			Set(value);
		}
	}

	public float NormalizedValue
	{
		get
		{
			if (Mathf.Approximately(MINValue, MAXValue))
			{
				return 0f;
			}
			return Mathf.InverseLerp(MINValue, MAXValue, Value);
		}
		set
		{
			Value = Mathf.Lerp(MINValue, MAXValue, value);
		}
	}

	public float ValueY
	{
		get
		{
			if (WholeNumbers)
			{
				return Mathf.Round(mValueY);
			}
			return mValueY;
		}
		set
		{
			SetY(value);
		}
	}

	public float NormalizedValueY
	{
		get
		{
			if (Mathf.Approximately(MINValue, MAXValue))
			{
				return 0f;
			}
			return Mathf.InverseLerp(MINValue, MAXValue, ValueY);
		}
		set
		{
			ValueY = Mathf.Lerp(MINValue, MAXValue, value);
		}
	}

	public BoxSliderEvent ONValueChanged
	{
		get
		{
			return mOnValueChanged;
		}
		set
		{
			mOnValueChanged = value;
		}
	}

	protected BoxSlider()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateCachedReferences();
		Set(mValue, sendCallback: false);
		SetY(mValueY, sendCallback: false);
		UpdateVisuals();
	}

	protected override void OnDisable()
	{
		mTracker.Clear();
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		UpdateVisuals();
	}

	public virtual void Rebuild(CanvasUpdate executing)
	{
	}

	public void LayoutComplete()
	{
	}

	public void GraphicUpdateComplete()
	{
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData))
		{
			UpdateDrag(eventData, eventData.pressEventCamera);
		}
	}

	public virtual void OnInitializePotentialDrag(PointerEventData eventData)
	{
		eventData.useDragThreshold = false;
	}

	public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
	{
		if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
		{
			return false;
		}
		currentValue = newValue;
		return true;
	}

	public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
	{
		if (currentValue.Equals(newValue))
		{
			return false;
		}
		currentValue = newValue;
		return true;
	}

	private void UpdateCachedReferences()
	{
		if ((bool)mHandleRect)
		{
			mHandleTransform = mHandleRect.transform;
			if (mHandleTransform.parent != null)
			{
				mHandleContainerRect = mHandleTransform.parent.GetComponent<RectTransform>();
			}
		}
		else
		{
			mHandleContainerRect = null;
		}
	}

	private void Set(float input)
	{
		Set(input, sendCallback: true);
	}

	private void Set(float input, bool sendCallback)
	{
		float num = Mathf.Clamp(input, MINValue, MAXValue);
		if (WholeNumbers)
		{
			num = Mathf.Round(num);
		}
		if (!mValue.Equals(num))
		{
			mValue = num;
			UpdateVisuals();
			if (sendCallback)
			{
				mOnValueChanged.Invoke(num, ValueY);
			}
		}
	}

	private void SetY(float input)
	{
		SetY(input, sendCallback: true);
	}

	private void SetY(float input, bool sendCallback)
	{
		float num = Mathf.Clamp(input, MINValue, MAXValue);
		if (WholeNumbers)
		{
			num = Mathf.Round(num);
		}
		if (!mValueY.Equals(num))
		{
			mValueY = num;
			UpdateVisuals();
			if (sendCallback)
			{
				mOnValueChanged.Invoke(Value, num);
			}
		}
	}

	private void UpdateVisuals()
	{
		mTracker.Clear();
		if (mHandleContainerRect != null)
		{
			mTracker.Add(this, mHandleRect, DrivenTransformProperties.Anchors);
			Vector2 zero = Vector2.zero;
			Vector2 one = Vector2.one;
			float value = (one[0] = NormalizedValue);
			zero[0] = value;
			value = (one[1] = NormalizedValueY);
			zero[1] = value;
			mHandleRect.anchorMin = zero;
			mHandleRect.anchorMax = one;
		}
	}

	private void UpdateDrag(PointerEventData eventData, Camera cam)
	{
		RectTransform rectTransform = mHandleContainerRect;
		if (rectTransform != null && rectTransform.rect.size[0] > 0f && RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, cam, out var localPoint))
		{
			localPoint -= rectTransform.rect.position;
			float normalizedValue = Mathf.Clamp01((localPoint - mOffset)[0] / rectTransform.rect.size[0]);
			NormalizedValue = normalizedValue;
			float normalizedValueY = Mathf.Clamp01((localPoint - mOffset)[1] / rectTransform.rect.size[1]);
			NormalizedValueY = normalizedValueY;
		}
	}

	private bool MayDrag(PointerEventData eventData)
	{
		if (IsActive() && IsInteractable())
		{
			return eventData.button == PointerEventData.InputButton.Left;
		}
		return false;
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (!MayDrag(eventData))
		{
			return;
		}
		base.OnPointerDown(eventData);
		mOffset = Vector2.zero;
		if (mHandleContainerRect != null && RectTransformUtility.RectangleContainsScreenPoint(mHandleRect, eventData.position, eventData.enterEventCamera))
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(mHandleRect, eventData.position, eventData.pressEventCamera, out var localPoint))
			{
				mOffset = localPoint;
			}
			mOffset.y = 0f - mOffset.y;
		}
		else
		{
			UpdateDrag(eventData, eventData.pressEventCamera);
		}
	}
}

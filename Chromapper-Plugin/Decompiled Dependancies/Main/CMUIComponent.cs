using System;
using UnityEngine;

public abstract class CMUIComponent<T> : CMUIComponentBase
{
	private Func<T> valueAccessor;

	private Action<T> onValueChanged;

	private T internalValue;

	public T Value
	{
		get
		{
			return internalValue;
		}
		set
		{
			internalValue = ValidateValue(value);
			onValueChanged?.Invoke(internalValue);
			OnValueUpdated(internalValue);
		}
	}

	internal void SetValueAccessor(Func<T> valueAccessor)
	{
		if (this.valueAccessor != null)
		{
			throw new InvalidOperationException("valueAccessor has already been assigned.");
		}
		this.valueAccessor = valueAccessor;
	}

	internal void SetOnValueChanged(Action<T> onValueChanged)
	{
		if (this.onValueChanged != null)
		{
			throw new InvalidOperationException("onValueChanged has already been assigned.");
		}
		this.onValueChanged = onValueChanged;
	}

	protected virtual void OnValueUpdated(T updatedValue)
	{
	}

	protected virtual T ValidateValue(T rawValue)
	{
		return rawValue;
	}

	private void Awake()
	{
		if (valueAccessor != null)
		{
			internalValue = valueAccessor();
		}
		else
		{
			Debug.LogWarning("Value accessor was not assigned. Is this intentional?");
		}
	}
}

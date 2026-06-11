using System;
using UnityEngine;

public abstract class CMUIComponentBase : MonoBehaviour
{
	internal virtual void SetLabelEnabled(bool enabled)
	{
		throw new InvalidOperationException("This component has no label.");
	}

	internal virtual void SetLabelText(string text)
	{
		throw new InvalidOperationException("This component has no label.");
	}
}

namespace Beatmap.Base;

public abstract class FxEventBase<T> : BaseItem where T : struct
{
	public float JsonTime;

	public int UsePreviousEventValue;

	public T Value;
}

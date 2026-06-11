using SimpleJSON;

namespace Beatmap.Animations;

public interface IPointDefinition
{
	public struct UntypedParams
	{
		public string Key;

		public bool Overwrite;

		public JSONNode Points;

		public string Easing;

		public float Time;

		public float Transition;

		public float Duration;

		public float TimeBegin;

		public float TimeEnd;

		public int Repeat;
	}
}

using Beatmap.Base.Customs;

namespace Beatmap.Animations;

public interface IAnimateProperty
{
	float StartTime { get; }

	bool IsEmpty();

	void UpdateProperty(float time);

	void Sort();

	void RemoveEvent(BaseCustomEvent ev);
}

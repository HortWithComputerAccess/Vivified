using UnityEngine;

namespace Beatmap.Base.Customs;

public interface IChromaObject
{
	Color? CustomColor { get; set; }

	string CustomKeyColor { get; }
}

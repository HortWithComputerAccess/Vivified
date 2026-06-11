using Beatmap.Base;
using Beatmap.Enums;

namespace Beatmap.V2;

public class V2ChromaNote : BaseNote
{
	public const int Monochrome = 0;

	public const int Bidirectional = 2;

	public const int Duochrome = 3;

	public const int HotGarbage = 7;

	public const int Alternate = 1;

	public const int Deflect = 5;

	public int BombRotation;

	public BaseNote OriginalNote;

	public override ObjectType ObjectType
	{
		get
		{
			return ObjectType.CustomNote;
		}
		set
		{
			base.ObjectType = value;
		}
	}

	public V2ChromaNote(BaseNote baseNote)
	{
		OriginalNote = baseNote;
		base.Type = baseNote.Type;
		base.CutDirection = baseNote.CutDirection;
		base.PosX = baseNote.PosX;
		PosY = baseNote.PosY;
		base.JsonTime = baseNote.JsonTime;
		base.Type = baseNote.Type;
	}
}

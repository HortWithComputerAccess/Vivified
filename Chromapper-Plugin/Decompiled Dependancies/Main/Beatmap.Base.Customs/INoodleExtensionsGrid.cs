using SimpleJSON;

namespace Beatmap.Base.Customs;

public interface INoodleExtensionsGrid
{
	JSONNode CustomAnimation { get; set; }

	JSONNode CustomCoordinate { get; set; }

	JSONNode CustomWorldRotation { get; set; }

	JSONNode CustomLocalRotation { get; set; }

	JSONNode CustomSpawnEffect { get; set; }

	JSONNode CustomNoteJumpMovementSpeed { get; set; }

	JSONNode CustomNoteJumpStartBeatOffset { get; set; }

	bool CustomFake { get; set; }

	string CustomKeyAnimation { get; }

	string CustomKeyCoordinate { get; }

	string CustomKeyWorldRotation { get; }

	string CustomKeyLocalRotation { get; }

	string CustomKeySpawnEffect { get; }

	string CustomKeyNoteJumpMovementSpeed { get; }

	string CustomKeyNoteJumpStartBeatOffset { get; }
}

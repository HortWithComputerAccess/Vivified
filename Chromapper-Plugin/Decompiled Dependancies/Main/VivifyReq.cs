using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Info;

public class VivifyReq : RequirementCheck
{
	private static readonly string[] vivifyEventTypes = new string[11]
	{
		"SetMaterialProperty", "SetGlobalProperty", "Blit", "CreateCamera", "CreateScreenTexture", "InstantiatePrefab", "DestroyObject", "SetAnimatorProperty", "SetCameraProperty", "AssignObjectPrefab",
		"SetRenderingSettings"
	};

	public override string Name => "Vivify";

	public override RequirementType IsRequiredOrSuggested(InfoDifficulty infoDifficulty, BaseDifficulty map)
	{
		if (!MapHasVivifyBundles(infoDifficulty) && !MapHasVivifyEvents(map))
		{
			return RequirementType.None;
		}
		return RequirementType.Requirement;
	}

	private bool MapHasVivifyEvents(BaseDifficulty map)
	{
		return map.CustomEvents.Any((BaseCustomEvent ev) => vivifyEventTypes.Contains(ev.Type));
	}

	private bool MapHasVivifyBundles(InfoDifficulty infoDifficulty)
	{
		if (infoDifficulty.CustomData != null)
		{
			return infoDifficulty.CustomData.HasKey("_assetBundle");
		}
		return false;
	}
}

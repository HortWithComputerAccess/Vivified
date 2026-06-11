using Beatmap.Info;
using UnityEngine;
using UnityEngine.UI;

public class NotePlacementUI : MonoBehaviour
{
	[SerializeField]
	private NotePlacement notePlacement;

	[SerializeField]
	private BombPlacement bombPlacement;

	[SerializeField]
	private ObstaclePlacement obstaclePlacement;

	[SerializeField]
	private CustomStandaloneInputModule customStandaloneInputModule;

	[SerializeField]
	private DeleteToolController deleteToolController;

	[SerializeField]
	private Toggle[] chromaToggles;

	[SerializeField]
	private Toggle[] singleSaberDisabledToggles;

	private void Start()
	{
		InfoDifficulty mapDifficultyInfo = BeatSaberSongContainer.Instance.MapDifficultyInfo;
		Toggle[] array = chromaToggles;
		foreach (Toggle toggle in array)
		{
			if (mapDifficultyInfo.Characteristic != "ChromaToggle")
			{
				toggle.interactable = false;
				Tooltip component = toggle.GetComponent<Tooltip>();
				if (component != null)
				{
					component.TooltipOverride = "ChromaToggle coming soon!";
				}
			}
		}
		array = singleSaberDisabledToggles;
		foreach (Toggle toggle2 in array)
		{
			if (mapDifficultyInfo.Characteristic == "OneSaber")
			{
				toggle2.interactable = false;
				Tooltip component2 = toggle2.GetComponent<Tooltip>();
				if (component2 != null)
				{
					component2.TooltipOverride = "Single Saber only allows the right saber!";
				}
			}
		}
	}

	public void RedNote(bool active)
	{
		if (active)
		{
			UpdateValue(0);
		}
	}

	public void BlueNote(bool active)
	{
		if (active)
		{
			UpdateValue(1);
		}
	}

	public void Bomb(bool active)
	{
		if (active)
		{
			notePlacement.IsActive = false;
			bombPlacement.IsActive = true;
			obstaclePlacement.IsActive = false;
			deleteToolController.UpdateDeletion(enabled: false);
		}
	}

	public void Wall(bool active)
	{
		if (active)
		{
			notePlacement.IsActive = false;
			bombPlacement.IsActive = false;
			obstaclePlacement.IsActive = true;
			deleteToolController.UpdateDeletion(enabled: false);
		}
	}

	public void RedAlt(bool active)
	{
	}

	public void BlueAlt(bool active)
	{
	}

	public void Mono(bool active)
	{
	}

	public void Duo(bool active)
	{
	}

	public void UpdateValue(int v)
	{
		notePlacement.IsActive = true;
		bombPlacement.IsActive = false;
		obstaclePlacement.IsActive = false;
		notePlacement.UpdateType(v);
		deleteToolController.UpdateDeletion(enabled: false);
	}
}

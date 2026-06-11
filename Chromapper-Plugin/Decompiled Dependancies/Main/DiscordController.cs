using System;
using Beatmap.Info;
using Discord;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordController : MonoBehaviour
{
	public static bool IsActive = true;

	public static ImageManager ImageManager = null;

	public static UserManager UserManager = null;

	public ActivityManager ActivityManager;

	public global::Discord.Discord Discord;

	private Activity activity;

	[SerializeField]
	private TextAsset clientIDTextAsset;

	private void Start()
	{
		if (!Settings.Instance.DiscordRPCEnabled)
		{
			IsActive = false;
			return;
		}
		try
		{
			if (long.TryParse(clientIDTextAsset.text, out var result) && Application.internetReachability != NetworkReachability.NotReachable)
			{
				Discord = new global::Discord.Discord(result, 1uL);
				ImageManager = Discord.GetImageManager();
				UserManager = Discord.GetUserManager();
				ActivityManager = Discord.GetActivityManager();
				ActivityManager.ClearActivity(delegate
				{
				});
				SceneManager.activeSceneChanged += SceneUpdated;
				LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Combine(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(LoadPlatform));
				LoadedDifficultySelectController.LoadedDifficultyChangedEvent = (Action)Delegate.Combine(LoadedDifficultySelectController.LoadedDifficultyChangedEvent, new Action(LoadedDifficultyChanged));
			}
			else
			{
				HandleException("No internet connection, or invalid Client ID.");
			}
		}
		catch (ResultException ex)
		{
			HandleException(ex.Message + " (Perhaps Discord is not open?)");
		}
		catch (DllNotFoundException ex2)
		{
			HandleException(ex2.Message + " Dll missing?");
		}
	}

	private void Update()
	{
		try
		{
			if (IsActive)
			{
				Discord?.RunCallbacks();
			}
		}
		catch (ResultException ex)
		{
			HandleException(ex.Message);
		}
	}

	private void OnDestroy()
	{
		SceneManager.activeSceneChanged -= SceneUpdated;
		LoadInitialMap.PlatformLoadedEvent = (Action<PlatformDescriptor>)Delegate.Remove(LoadInitialMap.PlatformLoadedEvent, new Action<PlatformDescriptor>(LoadPlatform));
		LoadedDifficultySelectController.LoadedDifficultyChangedEvent = (Action)Delegate.Remove(LoadedDifficultySelectController.LoadedDifficultyChangedEvent, new Action(LoadedDifficultyChanged));
	}

	private void OnApplicationQuit()
	{
		Discord?.Dispose();
	}

	private void LoadPlatform(PlatformDescriptor platform)
	{
		string largeImage = platform.gameObject.name.Replace("(Clone)", "").Replace(" ", "").ToLowerInvariant()
			.Trim();
		activity.Assets.LargeImage = largeImage;
		string jsonEnvironmentName = BeatSaberSongContainer.Instance.Info.EnvironmentName;
		string largeText = SongInfoEditUI.VanillaEnvironments.Find((SongInfoEditUI.Environment x) => x.JsonName == jsonEnvironmentName)?.HumanName ?? jsonEnvironmentName;
		activity.Assets.LargeText = largeText;
		UpdatePresence();
	}

	private void LoadedDifficultyChanged()
	{
		InfoDifficulty mapDifficultyInfo = BeatSaberSongContainer.Instance.MapDifficultyInfo;
		activity.State = mapDifficultyInfo.Characteristic + " " + mapDifficultyInfo.Difficulty;
		UpdatePresence();
	}

	private void SceneUpdated(Scene from, Scene to)
	{
		StopAllCoroutines();
		string details = "Invalid!";
		string state = "";
		switch (to.name)
		{
		case "00_FirstBoot":
			details = "Selecting install folder...";
			break;
		case "01_SongSelectMenu":
			details = "Viewing song list.";
			break;
		case "02_SongEditMenu":
			details = BeatSaberSongContainer.Instance.Info.SongName;
			state = "Viewing song info.";
			break;
		case "03_Mapper":
		{
			BeatSaberSongContainer instance = BeatSaberSongContainer.Instance;
			BaseInfo info = instance.Info;
			InfoDifficulty mapDifficultyInfo = instance.MapDifficultyInfo;
			details = "Editing " + info.SongName;
			state = mapDifficultyInfo.Characteristic + " " + mapDifficultyInfo.Difficulty;
			break;
		}
		case "04_Options":
			details = "Editing ChroMapper options";
			break;
		}
		activity = new Activity
		{
			Details = details,
			State = state,
			Timestamps = new ActivityTimestamps
			{
				Start = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
			},
			Assets = new ActivityAssets
			{
				SmallImage = "newlogo",
				SmallText = "ChroMapper v" + Application.version,
				LargeImage = "newlogo_glow",
				LargeText = "In Menus"
			}
		};
		UpdatePresence();
	}

	private void UpdatePresence()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			return;
		}
		ActivityManager?.UpdateActivity(activity, delegate(Result res)
		{
			if (res == Result.Ok)
			{
				Debug.Log("Discord Presence updated!");
			}
			else
			{
				Debug.LogWarning($"Discord Presence failed! {res}");
			}
		});
	}

	private void HandleException(string msg)
	{
		PersistentUI.Instance.ShowDialogBox("PersistentUI", "discord.error", null, PersistentUI.DialogBoxPresetType.Ok, new object[1] { msg });
		IsActive = false;
	}
}

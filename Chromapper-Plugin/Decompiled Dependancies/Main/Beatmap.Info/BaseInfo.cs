using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Info;

public class BaseInfo
{
	public class CustomEditorsMetadata
	{
		private readonly JSONNode editorsNode;

		public JSONNode MetadataNode = new JSONObject();

		public CustomEditorsMetadata(JSONNode obj)
		{
			if ((object)obj == null || !obj.Children.Any())
			{
				editorsNode = new JSONObject();
				return;
			}
			editorsNode = obj;
			if (editorsNode.HasKey(editorName))
			{
				MetadataNode = editorsNode[editorName];
			}
		}

		public JSONNode ToJson()
		{
			MetadataNode["version"] = editorVersion;
			int? obj = BeatSaberSongContainer.Instance?.Info.MajorVersion;
			editorsNode[obj switch
			{
				2 => "_lastEditedBy", 
				4 => "lastEditedBy", 
				_ => "lastEditedBy", 
			}] = editorName;
			editorsNode[editorName] = MetadataNode;
			return editorsNode;
		}
	}

	private string directory;

	private static string editorName;

	private static string editorVersion;

	private bool isFavourite;

	public JSONNode CustomData = new JSONObject();

	public List<BaseContributor> CustomContributors = new List<BaseContributor>();

	public CustomEditorsMetadata CustomEditorsData = new CustomEditorsMetadata(null);

	public CustomEnvironmentMetadata CustomEnvironmentMetadata;

	public string Directory
	{
		get
		{
			return directory;
		}
		set
		{
			LastWriteTime = File.GetLastWriteTime(Path.Combine(value, "Info.dat"));
			isFavourite = File.Exists(Path.Combine(value, ".favourite"));
			directory = value;
		}
	}

	public DateTime LastWriteTime { get; private set; }

	public bool IsFavourite
	{
		get
		{
			return isFavourite;
		}
		set
		{
			string path = Path.Combine(Directory, ".favourite");
			lock (this)
			{
				if (value)
				{
					File.Create(path).Dispose();
					File.SetAttributes(path, FileAttributes.Hidden);
				}
				else
				{
					File.Delete(path);
				}
			}
			isFavourite = value;
		}
	}

	public string Version { get; set; } = "4.0.1";

	public int MajorVersion
	{
		get
		{
			if (string.IsNullOrEmpty(Version))
			{
				return -1;
			}
			return (int)char.GetNumericValue(Version[0]);
		}
	}

	public string SongName { get; set; } = "New Song";

	public string CleanSongName => Path.GetInvalidFileNameChars().Aggregate(SongName, (string res, char el) => res.Replace(el.ToString(), string.Empty)).Trim('.');

	public string SongSubName { get; set; } = "";

	public string SongAuthorName { get; set; } = "";

	public string LevelAuthorName { get; set; } = "";

	public float SongTimeOffset { get; set; }

	public float Shuffle { get; set; }

	public float ShufflePeriod { get; set; }

	public float BeatsPerMinute { get; set; } = 100f;

	public float PreviewStartTime { get; set; } = 12f;

	public float PreviewDuration { get; set; } = 10f;

	public string SongFilename { get; set; } = "song.ogg";

	public string SongPreviewFilename { get; set; } = "song.ogg";

	public string AudioDataFilename { get; set; } = "AudioData.dat";

	public float SongDurationMetadata { get; set; }

	public float Lufs { get; set; }

	public string CoverImageFilename { get; set; } = "cover.png";

	public string EnvironmentName => EnvironmentNames.FirstOrDefault() ?? "DefaultEnvironment";

	public string AllDirectionsEnvironmentName => "GlassDesertEnvironment";

	public List<string> EnvironmentNames { get; set; } = new List<string> { "DefaultEnvironment" };

	public List<InfoColorScheme> ColorSchemes { get; set; } = new List<InfoColorScheme>();

	public List<InfoDifficultySet> DifficultySets { get; set; } = new List<InfoDifficultySet>();

	public BaseInfo()
	{
		if (string.IsNullOrEmpty(editorName))
		{
			editorName = Application.productName;
		}
		if (string.IsNullOrEmpty(editorVersion))
		{
			editorVersion = Application.version;
		}
	}

	public bool Save()
	{
		if (!System.IO.Directory.Exists(Directory))
		{
			System.IO.Directory.CreateDirectory(Directory);
		}
		JSONNode jSONNode = Version[0] switch
		{
			'2' => V2Info.GetOutputJson(this), 
			'4' => V4Info.GetOutputJson(this), 
			_ => null, 
		};
		if (jSONNode == null)
		{
			return false;
		}
		File.WriteAllText(Path.Combine(Directory, "Info.dat"), jSONNode.ToString(2));
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using UnityEngine;

public class SwingsPerSecond
{
	public class Stats
	{
		public readonly float Median;

		public readonly float Overall;

		public readonly float Peak;

		public Stats(float overall, float peak, double median)
		{
			Overall = overall;
			Peak = peak;
			Median = (float)median;
		}
	}

	private readonly float maximumTolerance = 0.06f;

	private readonly float maximumWindowTolerance = 0.07f;

	private readonly NoteGridContainer noteGrid;

	private readonly ObstacleGridContainer obstacleGrid;

	private int NotesCount => noteGrid.MapObjects.Count;

	public Stats Blue { get; private set; }

	public Stats Red { get; private set; }

	public Stats Total { get; private set; }

	public SwingsPerSecond(NoteGridContainer noteGrid, ObstacleGridContainer obstacleGrid)
	{
		this.noteGrid = noteGrid;
		this.obstacleGrid = obstacleGrid;
	}

	private float LastInteractiveObjectTime(float songBpm)
	{
		float b = 0f;
		if (NotesCount > 0 && noteGrid.MapObjects.Count != 0)
		{
			b = noteGrid.MapObjects[^1].SongBpmTime / songBpm * 60f;
		}
		float a = 0f;
		foreach (BaseObstacle mapObject in obstacleGrid.MapObjects)
		{
			if (mapObject.Width >= 2 || mapObject.PosX == 1 || mapObject.PosX == 2)
			{
				float b2 = (mapObject.SongBpmTime + mapObject.DurationSongBpm) / songBpm * 60f;
				a = Mathf.Max(a, b2);
			}
		}
		return Mathf.Max(a, b);
	}

	private float FirstInteractiveObjectTime(float songBpm)
	{
		float b = float.MaxValue;
		if (NotesCount > 0 && noteGrid.MapObjects.Count != 0)
		{
			b = noteGrid.MapObjects[0].SongBpmTime / songBpm * 60f;
		}
		float a = float.MaxValue;
		foreach (BaseObstacle mapObject in obstacleGrid.MapObjects)
		{
			if (mapObject.Width >= 2 || mapObject.PosX == 1 || mapObject.PosX == 2)
			{
				a = (mapObject.SongBpmTime + mapObject.DurationSongBpm) / songBpm * 60f;
				break;
			}
		}
		return Mathf.Min(a, b);
	}

	private bool MaybeWindowed(BaseNote note1, BaseNote note2)
	{
		return Mathf.Max(Mathf.Abs(note1.PosX - note2.PosX), Mathf.Abs(note1.PosY - note2.PosY)) >= 2;
	}

	private void CheckWindow(BaseNote note, ref BaseNote lastNote, int[] swingCount, float realTime, float songBpm)
	{
		if (lastNote != null)
		{
			if ((MaybeWindowed(note, lastNote) && (note.SongBpmTime - lastNote.SongBpmTime) / songBpm * 60f > maximumWindowTolerance) || (note.SongBpmTime - lastNote.SongBpmTime) / songBpm * 60f > maximumTolerance)
			{
				swingCount[Mathf.FloorToInt(realTime)]++;
			}
		}
		else
		{
			swingCount[Mathf.FloorToInt(realTime)]++;
		}
		lastNote = note;
	}

	private int[][] SwingCount(float songBpm)
	{
		if (NotesCount != 0)
		{
			float f = LastInteractiveObjectTime(songBpm);
			int[] array = new int[Mathf.FloorToInt(f) + 1];
			int[] array2 = new int[Mathf.FloorToInt(f) + 1];
			BaseNote lastNote = null;
			BaseNote lastNote2 = null;
			foreach (BaseNote mapObject in noteGrid.MapObjects)
			{
				if (!(mapObject.JsonTime < 0f))
				{
					float realTime = mapObject.SongBpmTime / songBpm * 60f;
					if (mapObject.Type == 0)
					{
						CheckWindow(mapObject, ref lastNote, array, realTime, songBpm);
					}
					else if (mapObject.Type == 1)
					{
						CheckWindow(mapObject, ref lastNote2, array2, realTime, songBpm);
					}
				}
			}
			return new int[2][] { array, array2 };
		}
		return new int[2][]
		{
			Array.Empty<int>(),
			Array.Empty<int>()
		};
	}

	public void Update()
	{
		int num = 10;
		float beatsPerMinute = BeatSaberSongContainer.Instance.Info.BeatsPerMinute;
		int[][] array = SwingCount(beatsPerMinute);
		int[] array2 = array[0];
		int[] array3 = array[1];
		int[] array4 = new int[array2.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array4[i] = array2[i] + array3[i];
		}
		if (num < 1)
		{
			Debug.LogWarning("Interval cannot be less than 1");
			return;
		}
		if (array4.Sum() == 0)
		{
			Total = new Stats(0f, 0f, 0.0);
			return;
		}
		List<double> list = new List<double>();
		List<double> list2 = new List<double>();
		List<double> list3 = new List<double>();
		for (int j = 0; j < array4.Length; j += num)
		{
			double num2 = ((j + num > array4.Length) ? (array4.Length - j) : num);
			double item = (double)array4.Skip(j).Take(num).Sum() / num2;
			double item2 = (double)array2.Skip(j).Take(num).Sum() / num2;
			double item3 = (double)array3.Skip(j).Take(num).Sum() / num2;
			list2.Add(item3);
			list.Add(item2);
			list3.Add(item);
		}
		float num3 = FirstInteractiveObjectTime(beatsPerMinute);
		float num4 = LastInteractiveObjectTime(beatsPerMinute);
		Red = new Stats((float)array2.Sum() / (num4 - num3), CalculateMaxRollingSps(array2, num), Median(list));
		Blue = new Stats((float)array3.Sum() / (num4 - num3), CalculateMaxRollingSps(array3, num), Median(list2));
		Total = new Stats((float)array4.Sum() / (num4 - num3), CalculateMaxRollingSps(array4, num), Median(list3));
	}

	public void Log()
	{
		Debug.Log("-----------------------------------------");
		Debug.LogFormat("[ Overall | Peak | Median ] Red SPS:\n[ {0:0.00} | {1:0.00} | {2:0.00} ]", Red.Overall, Red.Peak, Red.Median);
		Debug.LogFormat("[ Overall | Peak | Median ] Blue SPS:\n[ {0:0.00} | {1:0.00} | {2:0.00} ]", Blue.Overall, Blue.Peak, Blue.Median);
		Debug.LogFormat("[ Overall | Peak | Median ] Combined SPS:\n[ {0:0.00} | {1:0.00} | {2:0.00} ]", Total.Overall, Total.Peak, Total.Median);
		Debug.LogFormat("Overall Combined SPS: {0:0.00}", Total.Overall);
	}

	private double Median(List<double> xs)
	{
		if (xs.Count == 0)
		{
			return 0.0;
		}
		List<double> list = xs.OrderBy((double x) => x).ToList();
		double num = (double)(list.Count - 1) / 2.0;
		return (list[(int)num] + list[(int)(num + 0.5)]) / 2.0;
	}

	private float CalculateMaxRollingSps(int[] spsList, int interval)
	{
		if (spsList.Length == 0)
		{
			return 0f;
		}
		if (spsList.Length < interval)
		{
			return spsList.Sum() / spsList.Length;
		}
		int num = spsList.Take(interval).Sum();
		int num2 = num;
		for (int i = 0; i < spsList.Length - interval; i++)
		{
			num = num - spsList[i] + spsList[i + interval];
			num2 = Mathf.Max(num2, num);
		}
		return (float)num2 / (float)interval;
	}
}

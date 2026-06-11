using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using SimpleJSON;

public abstract class HeckRequirementCheck : RequirementCheck
{
	private static readonly HashSet<string> heckCustomEventTypes = new HashSet<string> { "AnimateTrack", "AssignPathAnimation", "AssignTrackParent", "AssignPlayerToTrack", "AnimateComponent" };

	protected bool HasAnimationsFromMod(BaseDifficulty map, ICollection<string> modSpecificTrackTypes, ICollection<string> modAnimationKeys)
	{
		Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
		foreach (BaseCustomEvent item in GetHeckCustomEventsFromMap(map))
		{
			if (modSpecificTrackTypes.Contains(item.Type))
			{
				return true;
			}
			if (!(item.Type == "AnimateTrack") && !(item.Type == "AssignPathAnimation"))
			{
				continue;
			}
			if (dictionary.TryGetValue(item.Data[item.CustomKeyTrack], out var value))
			{
				value.UnionWith(item.Data.Linq.Select((KeyValuePair<string, JSONNode> x) => x.Key));
			}
			else
			{
				dictionary.Add(item.Data[item.CustomKeyTrack].Value, new HashSet<string>(item.Data.Linq.Select((KeyValuePair<string, JSONNode> x) => x.Key)));
			}
		}
		(IEnumerable<string>, IEnumerable<string>) heckDataFromMap = GetHeckDataFromMap(map);
		if (heckDataFromMap.Item2.Intersect(modAnimationKeys).Any())
		{
			return true;
		}
		foreach (string item2 in heckDataFromMap.Item1)
		{
			if (dictionary.TryGetValue(item2, out var value2) && value2.Intersect(modAnimationKeys).Any())
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerable<BaseCustomEvent> GetHeckCustomEventsFromMap(BaseDifficulty map)
	{
		return map.CustomEvents.Where((BaseCustomEvent customEvent) => heckCustomEventTypes.Contains(customEvent.Type) && (customEvent.CustomTrack != null || customEvent.DataParentTrack != null || customEvent.DataChildrenTracks != null));
	}

	private static (IEnumerable<string> tracks, IEnumerable<string> animationKeys) GetHeckDataFromMap(BaseDifficulty map)
	{
		HashSet<string> hashSet = new HashSet<string>();
		HashSet<string> hashSet2 = new HashSet<string>();
		AddAnimationDataFromHeckGameplayObjects(hashSet2, hashSet, map.Notes);
		AddAnimationDataFromHeckGameplayObjects(hashSet2, hashSet, map.Obstacles);
		AddAnimationDataFromHeckGameplayObjects(hashSet2, hashSet, map.Arcs);
		AddAnimationDataFromHeckGameplayObjects(hashSet2, hashSet, map.Chains);
		return (tracks: hashSet, animationKeys: hashSet2);
	}

	private static void AddAnimationDataFromHeckGameplayObjects(ICollection<string> animations, ICollection<string> tracks, IEnumerable<BaseGrid> heckObjects)
	{
		foreach (BaseGrid heckObject in heckObjects)
		{
			if (heckObject.CustomAnimation != null && heckObject.CustomAnimation.IsObject)
			{
				foreach (string item in heckObject.CustomAnimation.AsObject.Linq.Select((KeyValuePair<string, JSONNode> x) => x.Key))
				{
					animations.Add(item);
				}
			}
			if (!(heckObject.CustomTrack != null))
			{
				continue;
			}
			if (heckObject.CustomTrack.IsString)
			{
				tracks.Add(heckObject.CustomTrack.Value);
			}
			else
			{
				if (!heckObject.CustomTrack.IsArray)
				{
					continue;
				}
				foreach (JSONNode child in heckObject.CustomTrack.Children)
				{
					if (child.IsString)
					{
						tracks.Add(child);
					}
				}
			}
		}
	}
}

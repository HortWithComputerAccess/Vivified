using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Patreon Supporters", menuName = "Patreon Supporter List")]
public class PatreonSupporters : ScriptableObject
{
	public List<string> HighTierPatrons = new List<string>();

	public List<string> RegularPatrons = new List<string>();

	public void AddSupporter(string patron, bool isCmSupporter)
	{
		if (isCmSupporter)
		{
			if (!HighTierPatrons.Contains(patron))
			{
				HighTierPatrons.Add(patron);
			}
		}
		else if (!RegularPatrons.Contains(patron))
		{
			RegularPatrons.Add(patron);
		}
	}

	public IEnumerable<string> GetAllSupporters()
	{
		List<string> list = new List<string>(HighTierPatrons);
		list.AddRange(RegularPatrons);
		return list.OrderBy((string x) => x);
	}
}

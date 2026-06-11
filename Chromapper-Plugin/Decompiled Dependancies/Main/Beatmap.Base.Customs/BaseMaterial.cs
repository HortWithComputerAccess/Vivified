using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Beatmap.V2.Customs;
using Beatmap.V3.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Base.Customs;

public class BaseMaterial : BaseItem
{
	public Color? Color { get; set; }

	public string Shader { get; set; }

	public string? Track { get; set; }

	public List<string> ShaderKeywords { get; set; }

	public string KeyColor
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_color";
			case 3:
			case 4:
				return "color";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyShader
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_shader";
			case 3:
			case 4:
				return "shader";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyTrack
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_track";
			case 3:
			case 4:
				return "track";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public string KeyShaderKeywords
	{
		get
		{
			int mapVersion = Settings.Instance.MapVersion;
			switch (mapVersion)
			{
			case 2:
				return "_shaderKeywords";
			case 3:
			case 4:
				return "shaderKeywords";
			default:
				throw new SwitchExpressionException(mapVersion);
			}
		}
	}

	public BaseMaterial()
	{
	}

	public BaseMaterial(BaseMaterial other)
	{
		Color = other.Color;
		Shader = other.Shader;
		Track = other.Track;
		ShaderKeywords = other.ShaderKeywords;
	}

	public BaseMaterial(JSONNode node)
	{
		Color = ((node[KeyColor] is JSONArray jSONArray) ? new Color?(jSONArray.ReadColor()) : ((Color?)null));
		Shader = RetrieveRequiredNode(node, KeyShader);
		Track = ((node[KeyTrack] is JSONString jSONString) ? ((string)jSONString) : null);
		ShaderKeywords = new List<string>();
		if (node[KeyShaderKeywords] is JSONArray jSONArray2)
		{
			JSONNode.Enumerator enumerator = jSONArray2.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JSONNode> current = enumerator.Current;
				ShaderKeywords.Add(current.Value);
			}
		}
	}

	public override JSONNode ToJson()
	{
		int mapVersion = Settings.Instance.MapVersion;
		switch (mapVersion)
		{
		case 2:
			return V2Material.ToJson(this);
		case 3:
		case 4:
			return V3Material.ToJson(this);
		default:
			throw new SwitchExpressionException(mapVersion);
		}
	}

	public override BaseItem Clone()
	{
		return new BaseMaterial(this);
	}
}

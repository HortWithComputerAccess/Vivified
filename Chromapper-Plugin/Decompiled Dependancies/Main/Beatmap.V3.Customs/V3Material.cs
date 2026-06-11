using Beatmap.Base.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.V3.Customs;

public class V3Material
{
	public const string KeyColor = "color";

	public const string KeyShader = "shader";

	public const string KeyTrack = "track";

	public const string KeyShaderKeywords = "shaderKeywords";

	public static BaseMaterial GetFromJson(JSONNode node)
	{
		return new BaseMaterial(node);
	}

	public static JSONNode ToJson(BaseMaterial material)
	{
		JSONObject jSONObject = new JSONObject();
		if (material.Color.HasValue)
		{
			Color? color = material.Color;
			jSONObject["color"] = (color.HasValue ? ((JSONNode)color.GetValueOrDefault()) : null);
		}
		jSONObject["shader"] = material.Shader;
		if (material.Track != null)
		{
			jSONObject["track"] = material.Track;
		}
		if (material.ShaderKeywords.Count > 0)
		{
			JSONArray jSONArray = new JSONArray();
			foreach (string shaderKeyword in material.ShaderKeywords)
			{
				jSONArray.Add(shaderKeyword);
			}
			jSONObject["shaderKeywords"] = jSONArray;
		}
		return jSONObject;
	}
}

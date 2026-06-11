using Beatmap.Base.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.V2.Customs;

public class V2Material
{
	public const string KeyColor = "_color";

	public const string KeyShader = "_shader";

	public const string KeyTrack = "_track";

	public const string KeyShaderKeywords = "_shaderKeywords";

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
			jSONObject["_color"] = (color.HasValue ? ((JSONNode)color.GetValueOrDefault()) : null);
		}
		jSONObject["_shader"] = material.Shader;
		if (material.Track != null)
		{
			jSONObject["_track"] = material.Track;
		}
		if (material.ShaderKeywords.Count > 0)
		{
			JSONArray jSONArray = new JSONArray();
			foreach (string shaderKeyword in material.ShaderKeywords)
			{
				jSONArray.Add(shaderKeyword);
			}
			jSONObject["_shaderKeywords"] = jSONArray;
		}
		return jSONObject;
	}
}

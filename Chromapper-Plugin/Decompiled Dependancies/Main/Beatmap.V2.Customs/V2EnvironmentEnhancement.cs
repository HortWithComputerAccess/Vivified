using Beatmap.Base.Customs;
using SimpleJSON;

namespace Beatmap.V2.Customs;

public static class V2EnvironmentEnhancement
{
	public const string KeyID = "_id";

	public const string KeyLookupMethod = "_lookupMethod";

	public const string KeyGeometry = "_geometry";

	public const string KeyTrack = "_track";

	public const string KeyDuplicate = "_duplicate";

	public const string KeyActive = "_active";

	public const string KeyScale = "_scale";

	public const string KeyPosition = "_position";

	public const string KeyRotation = "_rotation";

	public const string KeyLocalPosition = "_localPosition";

	public const string KeyLocalRotation = "_localRotation";

	public const string KeyComponents = "_components";

	public const string KeyLightID = "_lightID";

	public const string KeyLightType = "_type";

	public const string GeometryKeyType = "_type";

	public const string GeometryKeyMaterial = "_material";

	public static BaseEnvironmentEnhancement GetFromJson(JSONNode node)
	{
		return new BaseEnvironmentEnhancement(node);
	}

	public static JSONNode ToJson(BaseEnvironmentEnhancement environment)
	{
		JSONObject jSONObject = new JSONObject();
		if (environment.Geometry != null)
		{
			jSONObject["_geometry"] = environment.Geometry;
		}
		else
		{
			jSONObject["_id"] = environment.ID;
			jSONObject["_lookupMethod"] = environment.LookupMethod.ToString();
		}
		if (!string.IsNullOrEmpty(environment.Track))
		{
			jSONObject["_track"] = environment.Track;
		}
		if (environment.Duplicate > 0)
		{
			int? duplicate = environment.Duplicate;
			jSONObject["_duplicate"] = (duplicate.HasValue ? ((JSONNode)duplicate.GetValueOrDefault()) : null);
		}
		if (environment.Active != null)
		{
			jSONObject["_active"] = environment.Active;
		}
		if (environment.Scale.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "_scale", environment.Scale);
		}
		if (environment.Position.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "_position", environment.Position);
		}
		if (environment.Rotation.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "_rotation", environment.Rotation);
		}
		if (environment.LocalPosition.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "_localPosition", environment.LocalPosition);
		}
		if (environment.LocalRotation.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "_localRotation", environment.LocalRotation);
		}
		if (environment.LightID > 0)
		{
			int? duplicate = environment.LightID;
			jSONObject["_lightID"] = (duplicate.HasValue ? ((JSONNode)duplicate.GetValueOrDefault()) : null);
		}
		return jSONObject;
	}
}

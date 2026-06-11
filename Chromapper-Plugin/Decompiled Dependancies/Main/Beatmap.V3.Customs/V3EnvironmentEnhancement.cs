using Beatmap.Base.Customs;
using SimpleJSON;

namespace Beatmap.V3.Customs;

public static class V3EnvironmentEnhancement
{
	public const string KeyID = "id";

	public const string KeyLookupMethod = "lookupMethod";

	public const string KeyGeometry = "geometry";

	public const string KeyTrack = "track";

	public const string KeyDuplicate = "duplicate";

	public const string KeyActive = "active";

	public const string KeyScale = "scale";

	public const string KeyPosition = "position";

	public const string KeyRotation = "rotation";

	public const string KeyLocalPosition = "localPosition";

	public const string KeyLocalRotation = "localRotation";

	public const string KeyComponents = "components";

	public const string KeyLightID = "lightID";

	public const string KeyLightType = "type";

	public const string GeometryKeyType = "type";

	public const string GeometryKeyMaterial = "material";

	public static BaseEnvironmentEnhancement GetFromJson(JSONNode node)
	{
		return new BaseEnvironmentEnhancement(node);
	}

	public static JSONNode ToJson(BaseEnvironmentEnhancement environment)
	{
		JSONObject jSONObject = new JSONObject();
		if (environment.Geometry != null)
		{
			jSONObject["geometry"] = environment.Geometry;
		}
		else
		{
			jSONObject["id"] = environment.ID;
			jSONObject["lookupMethod"] = environment.LookupMethod.ToString();
		}
		if (!string.IsNullOrEmpty(environment.Track))
		{
			jSONObject["track"] = environment.Track;
		}
		if (environment.Duplicate > 0)
		{
			int? duplicate = environment.Duplicate;
			jSONObject["duplicate"] = (duplicate.HasValue ? ((JSONNode)duplicate.GetValueOrDefault()) : null);
		}
		if (environment.Active != null)
		{
			jSONObject["active"] = environment.Active;
		}
		if (environment.Scale.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "scale", environment.Scale);
		}
		if (environment.Position.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "position", environment.Position);
		}
		if (environment.Rotation.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "rotation", environment.Rotation);
		}
		if (environment.LocalPosition.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "localPosition", environment.LocalPosition);
		}
		if (environment.LocalRotation.HasValue)
		{
			BaseEnvironmentEnhancement.WriteVector3(jSONObject, "localRotation", environment.LocalRotation);
		}
		if (environment.Components != null)
		{
			jSONObject["components"] = environment.Components;
		}
		return jSONObject;
	}
}

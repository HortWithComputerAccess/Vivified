using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.V2.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Converters;

public static class V3ToV2
{
	public static JSONObject Geometry(JSONObject other)
	{
		if (other == null)
		{
			return null;
		}
		JSONObject jSONObject = new JSONObject();
		if (other["type"] == (object)"CUSTOM")
		{
			jSONObject["_type"] = other["type"];
			jSONObject["_mesh"] = Mesh(jSONObject["mesh"]?.AsObject);
			jSONObject["_material"] = (other["material"].IsString ? other["material"] : Material(jSONObject["material"]?.AsObject));
			jSONObject["_collision"] = other["collision"];
		}
		else
		{
			jSONObject["_type"] = other["type"];
			jSONObject["_material"] = (other["material"].IsString ? other["material"] : Material(jSONObject["material"]?.AsObject));
			jSONObject["_collision"] = other["collision"];
		}
		return jSONObject;
	}

	public static JSONObject Mesh(JSONObject other)
	{
		if (other == null)
		{
			return null;
		}
		JSONObject jSONObject = new JSONObject { ["_vertices"] = other["vertices"] };
		if (other.HasKey("uv"))
		{
			jSONObject["_uv"] = other["uv"];
		}
		if (other.HasKey("triangles"))
		{
			jSONObject["_triangles"] = other["triangles"];
		}
		return jSONObject;
	}

	public static JSONObject Material(JSONObject other)
	{
		if (other == null)
		{
			return null;
		}
		JSONObject jSONObject = new JSONObject { ["_shader"] = other["shader"] };
		if (other.HasKey("shaderKeywords"))
		{
			jSONObject["_shaderKeywords"] = other["shaderKeywords"];
		}
		if (other.HasKey("collision"))
		{
			jSONObject["_collision"] = other["collision"];
		}
		if (other.HasKey("track"))
		{
			jSONObject["_track"] = other["track"];
		}
		if (other.HasKey("color"))
		{
			jSONObject["_color"] = other["color"];
		}
		return jSONObject;
	}

	public static Vector3? RescaleVector3(Vector3? vec3)
	{
		if (vec3.HasValue)
		{
			Vector3 valueOrDefault = vec3.GetValueOrDefault();
			return new Vector3(valueOrDefault.x / 0.6f, valueOrDefault.y / 0.6f, valueOrDefault.z / 0.6f);
		}
		return null;
	}

	public static JSONNode CustomDataObject(JSONNode node)
	{
		if (node == null)
		{
			return null;
		}
		if (!node.Children.Any())
		{
			return null;
		}
		JSONNode jSONNode = node.Clone();
		if (jSONNode.HasKey("color"))
		{
			jSONNode["_color"] = (jSONNode.HasKey("_color") ? jSONNode["_color"] : jSONNode["color"]);
		}
		if (jSONNode.HasKey("coordinates"))
		{
			jSONNode["_position"] = (jSONNode.HasKey("_position") ? jSONNode["_position"] : jSONNode["coordinates"]);
		}
		if (jSONNode.HasKey("disableNoteGravity"))
		{
			jSONNode["_disableNoteGravity"] = (jSONNode.HasKey("_disableNoteGravity") ? jSONNode["_disableNoteGravity"] : jSONNode["disableNoteGravity"]);
		}
		if (jSONNode.HasKey("disableNoteLook"))
		{
			jSONNode["_disableNoteLook"] = (jSONNode.HasKey("_disableNoteLook") ? jSONNode["_disableNoteLook"] : jSONNode["disableNoteLook"]);
		}
		if (jSONNode.HasKey("flip"))
		{
			jSONNode["_flip"] = (jSONNode.HasKey("_flip") ? jSONNode["_flip"] : jSONNode["flip"]);
		}
		if (jSONNode.HasKey("localRotation"))
		{
			jSONNode["_localRotation"] = (jSONNode.HasKey("_localRotation") ? jSONNode["_localRotation"] : jSONNode["localRotation"]);
		}
		if (jSONNode.HasKey("noteJumpMovementSpeed"))
		{
			jSONNode["_noteJumpMovementSpeed"] = (jSONNode.HasKey("_noteJumpMovementSpeed") ? jSONNode["_noteJumpMovementSpeed"] : jSONNode["noteJumpMovementSpeed"]);
		}
		if (jSONNode.HasKey("noteJumpStartBeatOffset"))
		{
			jSONNode["_noteJumpStartBeatOffset"] = (jSONNode.HasKey("_noteJumpStartBeatOffset") ? jSONNode["_noteJumpStartBeatOffset"] : jSONNode["noteJumpStartBeatOffset"]);
		}
		if (jSONNode.HasKey("spawnEffect") && !jSONNode.HasKey("_disableSpawnEffect"))
		{
			jSONNode["_disableSpawnEffect"] = !jSONNode["spawnEffect"];
		}
		if (jSONNode.HasKey("size"))
		{
			jSONNode["_scale"] = (jSONNode.HasKey("_scale") ? jSONNode["_scale"] : jSONNode["size"]);
		}
		if (jSONNode.HasKey("track"))
		{
			jSONNode["_track"] = (jSONNode.HasKey("_track") ? jSONNode["_track"] : jSONNode["track"]);
		}
		if (jSONNode.HasKey("uninteractable") && !jSONNode.HasKey("_interactable"))
		{
			jSONNode["_interactable"] = !jSONNode["uninteractable"];
		}
		if (jSONNode.HasKey("worldRotation"))
		{
			jSONNode["_rotation"] = (jSONNode.HasKey("_rotation") ? jSONNode["_rotation"] : jSONNode["worldRotation"]);
		}
		if (jSONNode.HasKey("animation") && !jSONNode.HasKey("_animation"))
		{
			JSONObject jSONObject = new JSONObject();
			if (jSONNode["animation"].HasKey("color"))
			{
				jSONObject["_color"] = jSONNode["animation"]["color"];
			}
			if (jSONNode["animation"].HasKey("definitePosition"))
			{
				jSONObject["_definitePosition"] = jSONNode["animation"]["definitePosition"];
			}
			if (jSONNode["animation"].HasKey("dissolve"))
			{
				jSONObject["_dissolve"] = jSONNode["animation"]["dissolve"];
			}
			if (jSONNode["animation"].HasKey("dissolveArrow"))
			{
				jSONObject["_dissolveArrow"] = jSONNode["animation"]["dissolveArrow"];
			}
			if (jSONNode["animation"].HasKey("interactable"))
			{
				jSONObject["_interactable"] = jSONNode["animation"]["interactable"];
			}
			if (jSONNode["animation"].HasKey("localRotation"))
			{
				jSONObject["_localRotation"] = jSONNode["animation"]["localRotation"];
			}
			if (jSONNode["animation"].HasKey("offsetPosition"))
			{
				jSONObject["_position"] = jSONNode["animation"]["offsetPosition"];
			}
			if (jSONNode["animation"].HasKey("offsetRotation"))
			{
				jSONObject["_rotation"] = jSONNode["animation"]["offsetRotation"];
			}
			if (jSONNode["animation"].HasKey("scale"))
			{
				jSONObject["_scale"] = jSONNode["animation"]["scale"];
			}
			if (jSONNode["animation"].HasKey("time"))
			{
				jSONObject["_time"] = jSONNode["animation"]["time"];
			}
			if (jSONObject.Children.Any())
			{
				jSONNode["_animation"] = jSONObject;
			}
		}
		if (jSONNode.HasKey("color"))
		{
			jSONNode.Remove("color");
		}
		if (jSONNode.HasKey("coordinates"))
		{
			jSONNode.Remove("coordinates");
		}
		if (jSONNode.HasKey("disableNoteGravity"))
		{
			jSONNode.Remove("disableNoteGravity");
		}
		if (jSONNode.HasKey("disableNoteLook"))
		{
			jSONNode.Remove("disableNoteLook");
		}
		if (jSONNode.HasKey("flip"))
		{
			jSONNode.Remove("flip");
		}
		if (jSONNode.HasKey("localRotation"))
		{
			jSONNode.Remove("localRotation");
		}
		if (jSONNode.HasKey("noteJumpMovementSpeed"))
		{
			jSONNode.Remove("noteJumpMovementSpeed");
		}
		if (jSONNode.HasKey("noteJumpStartBeatOffset"))
		{
			jSONNode.Remove("noteJumpStartBeatOffset");
		}
		if (jSONNode.HasKey("spawnEffect"))
		{
			jSONNode.Remove("spawnEffect");
		}
		if (jSONNode.HasKey("size"))
		{
			jSONNode.Remove("size");
		}
		if (jSONNode.HasKey("track"))
		{
			jSONNode.Remove("track");
		}
		if (jSONNode.HasKey("uninteractable"))
		{
			jSONNode.Remove("uninteractable");
		}
		if (jSONNode.HasKey("worldRotation"))
		{
			jSONNode.Remove("worldRotation");
		}
		if (jSONNode.HasKey("animation"))
		{
			jSONNode.Remove("animation");
		}
		return jSONNode;
	}

	public static JSONNode CustomDataEvent(JSONNode node)
	{
		if (node == null)
		{
			return null;
		}
		if (!node.Children.Any())
		{
			return null;
		}
		JSONNode jSONNode = node.Clone();
		if (jSONNode.HasKey("color"))
		{
			jSONNode["_color"] = (jSONNode.HasKey("_color") ? jSONNode["_color"] : jSONNode["color"]);
		}
		if (jSONNode.HasKey("lightID"))
		{
			jSONNode["_lightID"] = (jSONNode.HasKey("_lightID") ? jSONNode["_lightID"] : jSONNode["lightID"]);
		}
		if (jSONNode.HasKey("easing"))
		{
			jSONNode["_easing"] = (jSONNode.HasKey("_easing") ? jSONNode["_easing"] : jSONNode["easing"]);
		}
		if (jSONNode.HasKey("lerpType"))
		{
			jSONNode["_lerpType"] = (jSONNode.HasKey("_lerpType") ? jSONNode["_lerpType"] : jSONNode["lerpType"]);
		}
		if (jSONNode.HasKey("nameFilter"))
		{
			jSONNode["_nameFilter"] = (jSONNode.HasKey("_nameFilter") ? jSONNode["_nameFilter"] : jSONNode["nameFilter"]);
		}
		if (jSONNode.HasKey("rotation"))
		{
			jSONNode["_rotation"] = (jSONNode.HasKey("_rotation") ? jSONNode["_rotation"] : jSONNode["rotation"]);
		}
		if (jSONNode.HasKey("step"))
		{
			jSONNode["_step"] = (jSONNode.HasKey("_step") ? jSONNode["_step"] : jSONNode["step"]);
		}
		if (jSONNode.HasKey("prop"))
		{
			jSONNode["_prop"] = (jSONNode.HasKey("_prop") ? jSONNode["_prop"] : jSONNode["prop"]);
		}
		if (jSONNode.HasKey("speed"))
		{
			jSONNode["_speed"] = (jSONNode.HasKey("_speed") ? jSONNode["_speed"] : jSONNode["speed"]);
		}
		if (jSONNode.HasKey("direction"))
		{
			jSONNode["_direction"] = (jSONNode.HasKey("_direction") ? jSONNode["_direction"] : jSONNode["direction"]);
		}
		if (jSONNode.HasKey("lockRotation"))
		{
			jSONNode["_lockPosition"] = (jSONNode.HasKey("_lockPosition") ? jSONNode["_lockPosition"] : jSONNode["lockRotation"]);
		}
		if (jSONNode.HasKey("color"))
		{
			jSONNode.Remove("color");
		}
		if (jSONNode.HasKey("lightID"))
		{
			jSONNode.Remove("lightID");
		}
		if (jSONNode.HasKey("easing"))
		{
			jSONNode.Remove("easing");
		}
		if (jSONNode.HasKey("lerpType"))
		{
			jSONNode.Remove("lerpType");
		}
		if (jSONNode.HasKey("nameFilter"))
		{
			jSONNode.Remove("nameFilter");
		}
		if (jSONNode.HasKey("rotation"))
		{
			jSONNode.Remove("rotation");
		}
		if (jSONNode.HasKey("step"))
		{
			jSONNode.Remove("step");
		}
		if (jSONNode.HasKey("prop"))
		{
			jSONNode.Remove("prop");
		}
		if (jSONNode.HasKey("speed"))
		{
			jSONNode.Remove("speed");
		}
		if (jSONNode.HasKey("direction"))
		{
			jSONNode.Remove("direction");
		}
		if (jSONNode.HasKey("lockRotation"))
		{
			jSONNode.Remove("lockRotation");
		}
		return jSONNode;
	}

	public static JSONNode CustomEventData(JSONNode node)
	{
		if (node == null)
		{
			return null;
		}
		if (!node.Children.Any())
		{
			return null;
		}
		JSONNode jSONNode = node.Clone();
		if (jSONNode.HasKey("track"))
		{
			jSONNode["_track"] = jSONNode["track"];
			jSONNode.Remove("track");
		}
		if (jSONNode.HasKey("duration"))
		{
			jSONNode["_duration"] = jSONNode["duration"];
			jSONNode.Remove("duration");
		}
		if (jSONNode.HasKey("easing"))
		{
			jSONNode["_easing"] = jSONNode["easing"];
			jSONNode.Remove("easing");
		}
		if (jSONNode.HasKey("repeat"))
		{
			jSONNode["_repeat"] = jSONNode["repeat"];
			jSONNode.Remove("repeat");
		}
		if (jSONNode.HasKey("childrenTracks"))
		{
			jSONNode["_childrenTracks"] = jSONNode["childrenTracks"];
			jSONNode.Remove("childrenTracks");
		}
		if (jSONNode.HasKey("parentTrack"))
		{
			jSONNode["_parentTrack"] = jSONNode["parentTrack"];
			jSONNode.Remove("parentTrack");
		}
		if (jSONNode.HasKey("worldPositionStays"))
		{
			jSONNode["_worldPositionStays"] = jSONNode["worldPositionStays"];
			jSONNode.Remove("worldPositionStays");
		}
		return jSONNode;
	}

	public static JSONNode CustomDataRoot(JSONNode node, BaseDifficulty difficulty)
	{
		if (node == null)
		{
			return null;
		}
		if (!node.Children.Any())
		{
			return null;
		}
		JSONNode jSONNode = node.Clone();
		if (jSONNode.HasKey("time"))
		{
			jSONNode["_time"] = jSONNode["time"];
			jSONNode.Remove("time");
		}
		if (jSONNode.HasKey("bookmarks"))
		{
			jSONNode["_bookmarks"] = jSONNode["bookmarks"];
			jSONNode["_bookmarks"].Remove("bookmarksUseOfficialBpmEvents");
			jSONNode.Remove("bookmarks");
		}
		if (jSONNode.HasKey("customEvents"))
		{
			JSONArray jSONArray = new JSONArray();
			foreach (BaseCustomEvent customEvent in difficulty.CustomEvents)
			{
				jSONArray.Add(V2CustomEvent.ToJson(customEvent));
			}
			jSONNode["_customEvents"] = jSONArray;
			jSONNode.Remove("customEvents");
		}
		if (jSONNode.HasKey("environment"))
		{
			JSONArray jSONArray2 = new JSONArray();
			foreach (BaseEnvironmentEnhancement environmentEnhancement in difficulty.EnvironmentEnhancements)
			{
				jSONArray2.Add(V2EnvironmentEnhancement.ToJson(environmentEnhancement));
			}
			jSONNode["_environment"] = jSONArray2;
			jSONNode.Remove("environment");
		}
		if (jSONNode.HasKey("pointDefinitions"))
		{
			JSONArray jSONArray3 = new JSONArray();
			foreach (KeyValuePair<string, JSONArray> pointDefinition in difficulty.PointDefinitions)
			{
				JSONObject aItem = new JSONObject
				{
					["_name"] = pointDefinition.Key,
					["_points"] = pointDefinition.Value
				};
				jSONArray3.Add(aItem);
			}
			jSONNode["_pointDefinitions"] = jSONArray3;
			jSONNode.Remove("pointDefinitions");
		}
		if (jSONNode.HasKey("materials"))
		{
			jSONNode["_materials"] = new JSONObject();
			foreach (KeyValuePair<string, BaseMaterial> material in difficulty.Materials)
			{
				jSONNode["_materials"][material.Key] = V2Material.ToJson(material.Value);
			}
			jSONNode.Remove("materials");
		}
		jSONNode.Remove("fakeColorNotes");
		jSONNode.Remove("fakeBombNotes");
		jSONNode.Remove("fakeObstacles");
		jSONNode.Remove("fakeBurstSliders");
		return jSONNode;
	}
}

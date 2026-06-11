using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.V3;
using Beatmap.V3.Customs;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.Converters;

public static class V2ToV3
{
	public static JSONObject Geometry(JSONObject other)
	{
		if (other == null)
		{
			return null;
		}
		JSONObject jSONObject = new JSONObject();
		if (other["_type"] == (object)"CUSTOM")
		{
			jSONObject["type"] = other["_type"];
			jSONObject["mesh"] = Mesh(jSONObject["_mesh"]?.AsObject);
			jSONObject["material"] = (other["_material"].IsString ? other["_material"] : Material(jSONObject["_material"]?.AsObject));
			jSONObject["collision"] = other["_collision"];
		}
		else
		{
			jSONObject["type"] = other["_type"];
			jSONObject["material"] = (other["_material"].IsString ? other["_material"] : Material(jSONObject["material"]?.AsObject));
			jSONObject["collision"] = other["_collision"];
		}
		return jSONObject;
	}

	public static JSONObject Mesh(JSONObject other)
	{
		if (other == null)
		{
			return null;
		}
		JSONObject jSONObject = new JSONObject { ["vertices"] = other["_vertices"] };
		if (other.HasKey("_uv"))
		{
			jSONObject["uv"] = other["_uv"];
		}
		if (other.HasKey("_triangles"))
		{
			jSONObject["triangles"] = other["_triangles"];
		}
		return jSONObject;
	}

	public static JSONObject Material(JSONObject other)
	{
		if (other == null)
		{
			return null;
		}
		JSONObject jSONObject = new JSONObject { ["shader"] = other["_shader"] };
		if (other.HasKey("_shaderKeywords"))
		{
			jSONObject["shaderKeywords"] = other["_shaderKeywords"];
		}
		if (other.HasKey("_collision"))
		{
			jSONObject["collision"] = other["_collision"];
		}
		if (other.HasKey("_track"))
		{
			jSONObject["track"] = other["_track"];
		}
		if (other.HasKey("_color"))
		{
			jSONObject["color"] = other["_color"];
		}
		return jSONObject;
	}

	public static Vector3? RescaleVector3(Vector3? vec3)
	{
		if (vec3.HasValue)
		{
			Vector3 valueOrDefault = vec3.GetValueOrDefault();
			return new Vector3(valueOrDefault.x * 0.6f, valueOrDefault.y * 0.6f, valueOrDefault.z * 0.6f);
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
		if (jSONNode.HasKey("_color"))
		{
			jSONNode["color"] = (jSONNode.HasKey("color") ? jSONNode["color"] : jSONNode["_color"]);
		}
		if (jSONNode.HasKey("_position"))
		{
			jSONNode["coordinates"] = (jSONNode.HasKey("coordinates") ? jSONNode["coordinates"] : jSONNode["_position"]);
		}
		if (jSONNode.HasKey("_disableNoteGravity"))
		{
			jSONNode["disableNoteGravity"] = (jSONNode.HasKey("disableNoteGravity") ? jSONNode["disableNoteGravity"] : jSONNode["_disableNoteGravity"]);
		}
		if (jSONNode.HasKey("_disableNoteLook"))
		{
			jSONNode["disableNoteLook"] = (jSONNode.HasKey("disableNoteLook") ? jSONNode["disableNoteLook"] : jSONNode["_disableNoteLook"]);
		}
		if (jSONNode.HasKey("_flip"))
		{
			jSONNode["flip"] = (jSONNode.HasKey("flip") ? jSONNode["flip"] : jSONNode["_flip"]);
		}
		if (jSONNode.HasKey("_localRotation"))
		{
			jSONNode["localRotation"] = (jSONNode.HasKey("localRotation") ? jSONNode["localRotation"] : jSONNode["_localRotation"]);
		}
		if (jSONNode.HasKey("_noteJumpMovementSpeed"))
		{
			jSONNode["noteJumpMovementSpeed"] = (jSONNode.HasKey("noteJumpMovementSpeed") ? jSONNode["noteJumpMovementSpeed"] : jSONNode["_noteJumpMovementSpeed"]);
		}
		if (jSONNode.HasKey("_noteJumpStartBeatOffset"))
		{
			jSONNode["noteJumpStartBeatOffset"] = (jSONNode.HasKey("noteJumpStartBeatOffset") ? jSONNode["noteJumpStartBeatOffset"] : jSONNode["_noteJumpStartBeatOffset"]);
		}
		if (jSONNode.HasKey("_disableSpawnEffect") && !jSONNode.HasKey("spawnEffect"))
		{
			jSONNode["spawnEffect"] = !jSONNode["_disableSpawnEffect"];
		}
		if (jSONNode.HasKey("_scale"))
		{
			jSONNode["size"] = (jSONNode.HasKey("size") ? jSONNode["size"] : jSONNode["_scale"]);
		}
		if (jSONNode.HasKey("_track"))
		{
			jSONNode["track"] = (jSONNode.HasKey("track") ? jSONNode["track"] : jSONNode["_track"]);
		}
		if (jSONNode.HasKey("_interactable") && !jSONNode.HasKey("uninteractable"))
		{
			jSONNode["uninteractable"] = !jSONNode["_interactable"];
		}
		if (jSONNode.HasKey("_rotation"))
		{
			jSONNode["worldRotation"] = (jSONNode.HasKey("worldRotation") ? jSONNode["worldRotation"] : jSONNode["_rotation"]);
		}
		if (jSONNode.HasKey("_animation") && !jSONNode.HasKey("animation"))
		{
			JSONObject jSONObject = new JSONObject();
			if (jSONNode["_animation"].HasKey("_color"))
			{
				jSONObject["color"] = jSONNode["_animation"]["_color"];
			}
			if (jSONNode["_animation"].HasKey("_definitePosition"))
			{
				jSONObject["definitePosition"] = jSONNode["_animation"]["_definitePosition"];
			}
			if (jSONNode["_animation"].HasKey("_dissolve"))
			{
				jSONObject["dissolve"] = jSONNode["_animation"]["_dissolve"];
			}
			if (jSONNode["_animation"].HasKey("_dissolveArrow"))
			{
				jSONObject["dissolveArrow"] = jSONNode["_animation"]["_dissolveArrow"];
			}
			if (jSONNode["_animation"].HasKey("_interactable"))
			{
				jSONObject["interactable"] = jSONNode["_animation"]["_interactable"];
			}
			if (jSONNode["_animation"].HasKey("_localRotation"))
			{
				jSONObject["localRotation"] = jSONNode["_animation"]["_localRotation"];
			}
			if (jSONNode["_animation"].HasKey("_position"))
			{
				jSONObject["offsetPosition"] = jSONNode["_animation"]["_position"];
			}
			if (jSONNode["_animation"].HasKey("_rotation"))
			{
				jSONObject["offsetRotation"] = jSONNode["_animation"]["_rotation"];
			}
			if (jSONNode["_animation"].HasKey("_scale"))
			{
				jSONObject["scale"] = jSONNode["_animation"]["_scale"];
			}
			if (jSONNode["_animation"].HasKey("_time"))
			{
				jSONObject["time"] = jSONNode["_animation"]["_time"];
			}
			if (jSONObject.Children.Any())
			{
				jSONNode["animation"] = jSONObject;
			}
		}
		if (jSONNode.HasKey("_color"))
		{
			jSONNode.Remove("_color");
		}
		if (jSONNode.HasKey("_fake"))
		{
			jSONNode.Remove("_fake");
		}
		if (jSONNode.HasKey("_position"))
		{
			jSONNode.Remove("_position");
		}
		if (jSONNode.HasKey("_disableNoteGravity"))
		{
			jSONNode.Remove("_disableNoteGravity");
		}
		if (jSONNode.HasKey("_disableNoteLook"))
		{
			jSONNode.Remove("_disableNoteLook");
		}
		if (jSONNode.HasKey("_flip"))
		{
			jSONNode.Remove("_flip");
		}
		if (jSONNode.HasKey("_cutDirection"))
		{
			jSONNode.Remove("_cutDirection");
		}
		if (jSONNode.HasKey("_localRotation"))
		{
			jSONNode.Remove("_localRotation");
		}
		if (jSONNode.HasKey("_noteJumpMovementSpeed"))
		{
			jSONNode.Remove("_noteJumpMovementSpeed");
		}
		if (jSONNode.HasKey("_noteJumpStartBeatOffset"))
		{
			jSONNode.Remove("_noteJumpStartBeatOffset");
		}
		if (jSONNode.HasKey("_disableSpawnEffect"))
		{
			jSONNode.Remove("_disableSpawnEffect");
		}
		if (jSONNode.HasKey("_scale"))
		{
			jSONNode.Remove("_scale");
		}
		if (jSONNode.HasKey("_track"))
		{
			jSONNode.Remove("_track");
		}
		if (jSONNode.HasKey("_interactable"))
		{
			jSONNode.Remove("_interactable");
		}
		if (jSONNode.HasKey("_rotation"))
		{
			jSONNode.Remove("_rotation");
		}
		if (jSONNode.HasKey("_animation"))
		{
			jSONNode.Remove("_animation");
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
		if (jSONNode.HasKey("_preciseSpeed"))
		{
			jSONNode["speed"] = jSONNode["_preciseSpeed"];
		}
		else if (jSONNode.HasKey("_speed"))
		{
			jSONNode["speed"] = (jSONNode.HasKey("speed") ? jSONNode["speed"] : jSONNode["_speed"]);
		}
		if (jSONNode.HasKey("_color"))
		{
			jSONNode["color"] = (jSONNode.HasKey("color") ? jSONNode["color"] : jSONNode["_color"]);
		}
		if (jSONNode.HasKey("_lightID"))
		{
			jSONNode["lightID"] = (jSONNode.HasKey("lightID") ? jSONNode["lightID"] : jSONNode["_lightID"]);
		}
		if (jSONNode.HasKey("_easing"))
		{
			jSONNode["easing"] = (jSONNode.HasKey("easing") ? jSONNode["easing"] : jSONNode["_easing"]);
		}
		if (jSONNode.HasKey("_lerpType"))
		{
			jSONNode["lerpType"] = (jSONNode.HasKey("lerpType") ? jSONNode["lerpType"] : jSONNode["_lerpType"]);
		}
		if (jSONNode.HasKey("_nameFilter"))
		{
			jSONNode["nameFilter"] = (jSONNode.HasKey("nameFilter") ? jSONNode["nameFilter"] : jSONNode["_nameFilter"]);
		}
		if (jSONNode.HasKey("_rotation"))
		{
			jSONNode["rotation"] = (jSONNode.HasKey("rotation") ? jSONNode["rotation"] : jSONNode["_rotation"]);
		}
		if (jSONNode.HasKey("_step"))
		{
			jSONNode["step"] = (jSONNode.HasKey("step") ? jSONNode["step"] : jSONNode["_step"]);
		}
		if (jSONNode.HasKey("_prop"))
		{
			jSONNode["prop"] = (jSONNode.HasKey("prop") ? jSONNode["prop"] : jSONNode["_prop"]);
		}
		if (jSONNode.HasKey("_direction"))
		{
			jSONNode["direction"] = (jSONNode.HasKey("direction") ? jSONNode["direction"] : jSONNode["_direction"]);
		}
		if (jSONNode.HasKey("_lockPosition"))
		{
			jSONNode["lockRotation"] = (jSONNode.HasKey("lockRotation") ? jSONNode["lockRotation"] : jSONNode["_lockPosition"]);
		}
		if (jSONNode.HasKey("_color"))
		{
			jSONNode.Remove("_color");
		}
		if (jSONNode.HasKey("_lightID"))
		{
			jSONNode.Remove("_lightID");
		}
		if (jSONNode.HasKey("_easing"))
		{
			jSONNode.Remove("_easing");
		}
		if (jSONNode.HasKey("_lerpType"))
		{
			jSONNode.Remove("_lerpType");
		}
		if (jSONNode.HasKey("_propID"))
		{
			jSONNode.Remove("_propID");
		}
		if (jSONNode.HasKey("_lightGradient"))
		{
			jSONNode.Remove("_lightGradient");
		}
		if (jSONNode.HasKey("_nameFilter"))
		{
			jSONNode.Remove("_nameFilter");
		}
		if (jSONNode.HasKey("_rotation"))
		{
			jSONNode.Remove("_rotation");
		}
		if (jSONNode.HasKey("_step"))
		{
			jSONNode.Remove("_step");
		}
		if (jSONNode.HasKey("_prop"))
		{
			jSONNode.Remove("_prop");
		}
		if (jSONNode.HasKey("_speed"))
		{
			jSONNode.Remove("_speed");
		}
		if (jSONNode.HasKey("_preciseSpeed"))
		{
			jSONNode.Remove("_preciseSpeed");
		}
		if (jSONNode.HasKey("_direction"))
		{
			jSONNode.Remove("_direction");
		}
		if (jSONNode.HasKey("_reset"))
		{
			jSONNode.Remove("_reset");
		}
		if (jSONNode.HasKey("_counterSpin"))
		{
			jSONNode.Remove("_counterSpin");
		}
		if (jSONNode.HasKey("_stepMult"))
		{
			jSONNode.Remove("_stepMult");
		}
		if (jSONNode.HasKey("_propMult"))
		{
			jSONNode.Remove("_propMult");
		}
		if (jSONNode.HasKey("_speedMult"))
		{
			jSONNode.Remove("_speedMult");
		}
		if (jSONNode.HasKey("_lockPosition"))
		{
			jSONNode.Remove("_lockPosition");
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
		if (jSONNode.HasKey("_track"))
		{
			jSONNode["track"] = jSONNode["_track"];
			jSONNode.Remove("_track");
		}
		if (jSONNode.HasKey("_duration"))
		{
			jSONNode["duration"] = jSONNode["_duration"];
			jSONNode.Remove("_duration");
		}
		if (jSONNode.HasKey("_easing"))
		{
			jSONNode["easing"] = jSONNode["_easing"];
			jSONNode.Remove("_easing");
		}
		if (jSONNode.HasKey("_repeat"))
		{
			jSONNode["repeat"] = jSONNode["_repeat"];
			jSONNode.Remove("_repeat");
		}
		if (jSONNode.HasKey("_childrenTracks"))
		{
			jSONNode["childrenTracks"] = jSONNode["_childrenTracks"];
			jSONNode.Remove("_childrenTracks");
		}
		if (jSONNode.HasKey("_parentTrack"))
		{
			jSONNode["parentTrack"] = jSONNode["_parentTrack"];
			jSONNode.Remove("_parentTrack");
		}
		if (jSONNode.HasKey("_worldPositionStays"))
		{
			jSONNode["worldPositionStays"] = jSONNode["_worldPositionStays"];
			jSONNode.Remove("_worldPositionStays");
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
		if (jSONNode.HasKey("_time"))
		{
			jSONNode["time"] = jSONNode["_time"];
			jSONNode.Remove("_time");
		}
		if (jSONNode.HasKey("_bookmarks"))
		{
			jSONNode["bookmarks"] = jSONNode["_bookmarks"];
			jSONNode["bookmarks"].Remove("_bookmarksUseOfficialBpmEvents");
			jSONNode.Remove("_bookmarks");
		}
		if (jSONNode.HasKey("_customEvents"))
		{
			JSONArray jSONArray = new JSONArray();
			foreach (BaseCustomEvent customEvent in difficulty.CustomEvents)
			{
				jSONArray.Add(V3CustomEvent.ToJson(customEvent));
			}
			jSONNode["customEvents"] = jSONArray;
			jSONNode.Remove("_customEvents");
		}
		if (jSONNode.HasKey("_environment"))
		{
			JSONArray jSONArray2 = new JSONArray();
			foreach (KeyValuePair<string, BaseMaterial> material in difficulty.Materials)
			{
				jSONArray2.Add(V3Material.ToJson(material.Value));
			}
			foreach (BaseEnvironmentEnhancement environmentEnhancement in difficulty.EnvironmentEnhancements)
			{
				jSONArray2.Add(V3EnvironmentEnhancement.ToJson(environmentEnhancement));
			}
			jSONNode["environment"] = jSONArray2;
			jSONNode.Remove("_environment");
		}
		if (jSONNode.HasKey("_pointDefinitions"))
		{
			jSONNode["pointDefinitions"] = new JSONObject();
			foreach (KeyValuePair<string, JSONArray> pointDefinition in difficulty.PointDefinitions)
			{
				jSONNode["pointDefinitions"][pointDefinition.Key] = pointDefinition.Value;
			}
			jSONNode.Remove("_pointDefinitions");
		}
		if (jSONNode.HasKey("_materials"))
		{
			jSONNode["materials"] = new JSONObject();
			foreach (KeyValuePair<string, BaseMaterial> material2 in difficulty.Materials)
			{
				jSONNode["materials"][material2.Key] = V3Material.ToJson(material2.Value);
			}
			jSONNode.Remove("_materials");
		}
		JSONArray jSONArray3 = new JSONArray();
		JSONArray jSONArray4 = new JSONArray();
		foreach (BaseNote item in difficulty.Notes.Where((BaseNote note) => note.CustomFake))
		{
			if (item.Type == 3)
			{
				jSONArray3.Add(V3BombNote.ToJson(item));
			}
			else
			{
				jSONArray4.Add(V3ColorNote.ToJson(item));
			}
		}
		if (jSONArray4.Count > 0)
		{
			jSONNode["fakeColorNotes"] = jSONArray4;
		}
		if (jSONArray3.Count > 0)
		{
			jSONNode["fakeBombNotes"] = jSONArray3;
		}
		JSONArray jSONArray5 = new JSONArray();
		foreach (BaseObstacle item2 in difficulty.Obstacles.Where((BaseObstacle obstacle) => obstacle.CustomFake))
		{
			jSONArray5.Add(V3Obstacle.ToJson(item2));
		}
		if (jSONArray5.Count > 0)
		{
			jSONNode["fakeObstacles"] = jSONArray5;
		}
		JSONArray jSONArray6 = new JSONArray();
		foreach (BaseChain item3 in difficulty.Chains.Where((BaseChain chain) => chain.CustomFake))
		{
			jSONArray6.Add(V3Chain.ToJson(item3));
		}
		if (jSONArray6.Count > 0)
		{
			jSONNode["fakeBurstSliders"] = jSONArray6;
		}
		return jSONNode;
	}
}

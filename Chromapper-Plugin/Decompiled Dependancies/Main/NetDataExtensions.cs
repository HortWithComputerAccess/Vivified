using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Beatmap.Base;
using Beatmap.Base.Customs;
using Beatmap.Enums;
using LiteNetLib;
using LiteNetLib.Utils;

public static class NetDataExtensions
{
	private static Dictionary<string, Func<object>> compiledActionCtors = new Dictionary<string, Func<object>>();

	public static BaseObject GetBeatmapObject(this NetDataReader reader)
	{
		return (ObjectType)reader.GetByte() switch
		{
			ObjectType.Note => reader.Get<BaseNote>(), 
			ObjectType.Event => reader.Get<BaseEvent>(), 
			ObjectType.Obstacle => reader.Get<BaseObstacle>(), 
			ObjectType.CustomNote => throw new NotImplementedException(), 
			ObjectType.CustomEvent => reader.Get<BaseCustomEvent>(), 
			ObjectType.BpmChange => reader.Get<BaseBpmEvent>(), 
			ObjectType.Arc => reader.Get<BaseArc>(), 
			ObjectType.Chain => reader.Get<BaseChain>(), 
			ObjectType.Bookmark => reader.Get<BaseBookmark>(), 
			ObjectType.Waypoint => reader.Get<BaseWaypoint>(), 
			_ => throw new InvalidPacketException("Attempting to parse an invalid object type"), 
		};
	}

	public static void PutBeatmapObject(this NetDataWriter writer, BaseObject obj)
	{
		writer.Put((byte)obj.ObjectType);
		writer.Put(obj);
	}

	public static BeatmapAction GetBeatmapAction(this NetDataReader reader, MapperIdentityPacket identity)
	{
		string text = reader.GetString();
		string input = reader.GetString();
		if (!compiledActionCtors.TryGetValue(text, out var value))
		{
			Type type = Type.GetType(text);
			if (type != null && typeof(BeatmapAction).IsAssignableFrom(type))
			{
				ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
				DynamicMethod dynamicMethod = new DynamicMethod("Create" + text, type, Type.EmptyTypes);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Newobj, constructor);
				iLGenerator.Emit(OpCodes.Ret);
				value = dynamicMethod.CreateDelegate(typeof(Func<object>)) as Func<object>;
				compiledActionCtors.Add(text, value);
			}
		}
		BeatmapAction obj = value() as BeatmapAction;
		obj.Identity = identity;
		obj.Guid = Guid.Parse(input);
		obj.Comment = "[" + identity.Name + "] " + reader.GetString();
		obj.Deserialize(reader);
		return obj;
	}

	public static void PutBeatmapAction(this NetDataWriter writer, BeatmapAction action)
	{
		writer.Put(action.GetType().Name);
		writer.Put(action.Guid.ToString());
		writer.Put(action.Comment);
		writer.Put(action);
	}
}

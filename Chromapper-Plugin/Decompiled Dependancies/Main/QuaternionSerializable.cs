using LiteNetLib.Utils;
using UnityEngine;

public class QuaternionSerializable : INetSerializable
{
	public float X;

	public float Y;

	public float Z;

	public float W;

	public static implicit operator Quaternion(QuaternionSerializable serializable)
	{
		return new Quaternion(serializable.X, serializable.Y, serializable.Z, serializable.W);
	}

	public static implicit operator QuaternionSerializable(Quaternion quaternion)
	{
		return new QuaternionSerializable
		{
			X = quaternion.x,
			Y = quaternion.y,
			Z = quaternion.z,
			W = quaternion.w
		};
	}

	public void Serialize(NetDataWriter writer)
	{
		writer.Put(X);
		writer.Put(Y);
		writer.Put(Z);
		writer.Put(W);
	}

	public void Deserialize(NetDataReader reader)
	{
		X = reader.GetFloat();
		Y = reader.GetFloat();
		Z = reader.GetFloat();
		W = reader.GetFloat();
	}
}

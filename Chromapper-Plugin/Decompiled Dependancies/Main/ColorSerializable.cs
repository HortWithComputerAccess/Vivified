using LiteNetLib.Utils;
using UnityEngine;

public class ColorSerializable : INetSerializable
{
	public float R;

	public float G;

	public float B;

	public static implicit operator Color(ColorSerializable serializable)
	{
		return new Color(serializable.R, serializable.G, serializable.B, 1f);
	}

	public static implicit operator ColorSerializable(Color color)
	{
		return new ColorSerializable
		{
			R = color.r,
			G = color.g,
			B = color.b
		};
	}

	public void Serialize(NetDataWriter writer)
	{
		writer.Put(R);
		writer.Put(G);
		writer.Put(B);
	}

	public void Deserialize(NetDataReader reader)
	{
		R = reader.GetFloat();
		G = reader.GetFloat();
		B = reader.GetFloat();
	}
}

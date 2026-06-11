using System;
using System.IO;
using UnityEngine;

public static class FileContentValidationHelper
{
	private const int vorbisIdHeaderOffset = 28;

	private static readonly int oggVorbisSignatureSize = 28 + "?vorbis".Length;

	private static readonly int wavSignatureSize = "RIFF????WAVE".Length;

	public static AudioType GetAudioType(string filePath)
	{
		using FileStream fileStream = new FileStream(filePath, FileMode.Open);
		int num = Math.Max(oggVorbisSignatureSize, wavSignatureSize);
		byte[] buffer = new byte[num];
		fileStream.Read(buffer, 0, num);
		if (IsOggVorbisFileSignature(buffer))
		{
			return AudioType.OGGVORBIS;
		}
		if (IsWavFileSignature(buffer))
		{
			return AudioType.WAV;
		}
		return AudioType.UNKNOWN;
	}

	public static bool IsSupportedAudioFormat(string filePath)
	{
		return GetAudioType(filePath) != AudioType.UNKNOWN;
	}

	private static bool IsWavFileSignature(byte[] buffer)
	{
		if (buffer.Length < wavSignatureSize)
		{
			return false;
		}
		if (buffer[0] == 82 && buffer[1] == 73 && buffer[2] == 70 && buffer[3] == 70 && buffer[8] == 87 && buffer[9] == 65 && buffer[10] == 86)
		{
			return buffer[11] == 69;
		}
		return false;
	}

	private static bool IsOggVorbisFileSignature(byte[] buffer)
	{
		if (buffer.Length < oggVorbisSignatureSize)
		{
			return false;
		}
		bool num = buffer[0] == 79 && buffer[1] == 103 && buffer[2] == 103 && buffer[3] == 83;
		bool flag = buffer[29] == 118 && buffer[30] == 111 && buffer[31] == 114 && buffer[32] == 98 && buffer[33] == 105 && buffer[34] == 115;
		return num && flag;
	}
}

using System.Collections.Generic;
using UnityEngine;

public class AudioUtil : MonoBehaviour
{
	private readonly List<AudioSource> oneShotPool = new List<AudioSource>();

	private AudioSource ambianceSource;

	private AudioSource AvailableOneShot
	{
		get
		{
			for (int i = 0; i < oneShotPool.Count; i++)
			{
				if (!oneShotPool[i].isPlaying)
				{
					return oneShotPool[i];
				}
			}
			AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
			MakeSourceNonDimensional(audioSource, loop: false);
			oneShotPool.Add(audioSource);
			return audioSource;
		}
	}

	private void Start()
	{
		ambianceSource = base.gameObject.AddComponent<AudioSource>();
		MakeSourceNonDimensional(ambianceSource, loop: true);
	}

	public AudioSource PlayOneShotSound(AudioClip clip, float volume = 1f, float pitch = 1f, float delay = 0f)
	{
		AudioSource availableOneShot = AvailableOneShot;
		PlayOneShotSound(clip, availableOneShot, volume, pitch, delay);
		return availableOneShot;
	}

	public void PlayOneShotSound(AudioClip clip, AudioSource oneShotSource, float volume = 1f, float pitch = 1f, float delay = 0f)
	{
		oneShotSource.volume = volume;
		oneShotSource.pitch = pitch;
		oneShotSource.clip = clip;
		oneShotSource.PlayScheduled(AudioSettings.dspTime + (double)delay);
	}

	public void StopOneShot()
	{
		foreach (AudioSource item in oneShotPool)
		{
			item.Stop();
		}
	}

	public void StopAmbianceSound()
	{
		ambianceSource.Stop();
	}

	public static void MakeSourceNonDimensional(AudioSource source, bool loop)
	{
		source.loop = loop;
		source.bypassEffects = true;
		source.bypassListenerEffects = true;
		source.bypassReverbZones = true;
		source.spatialBlend = 0f;
		source.spatialize = false;
		source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
	}

	public static void MakeSourceNonDimensional(AudioSource source)
	{
		MakeSourceNonDimensional(source, source.loop);
	}
}

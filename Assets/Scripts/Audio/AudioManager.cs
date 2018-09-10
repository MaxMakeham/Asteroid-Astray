﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public static AudioManager singleton;
	public AudioMixerGroup masterMixer, musicMixer, sfxMixer;

	private const int poolReserve = 400;
	private Queue<AudioSource> pool = new Queue<AudioSource>(poolReserve);
	private List<AudioSource> active = new List<AudioSource>(poolReserve);
	private const float MIN_DISTANCE = 1f / 3f, MAX_DISTANCE = 30f;
	public static Transform holder;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		holder = new GameObject("Audio Holder").transform;
		SetUpPoolReserve();
	}

	private void Update()
	{
		masterMixer.audioMixer.SetFloat("Pitch", Time.timeScale);
	}

	public static void PlaySFX(AudioClip clip, Vector3 position, Transform parent = null, float volume = 1f,
		float pitch = 1f)
	{
		AudioSource src = singleton.pool.Dequeue();
		singleton.active.Add(src);
		GameObject obj = src.gameObject;
		obj.SetActive(true);
		obj.transform.position = position;
		obj.transform.parent = parent == null ? holder : parent;
		src.clip = clip;
		src.volume = volume;
		src.pitch = pitch;
		src.Play();
		singleton.StartCoroutine(Lifetime(src, clip.length));
	}

	private static IEnumerator Lifetime(AudioSource src, float time)
	{
		while (time > 0f)
		{
			time -= Time.deltaTime;
			yield return null;
		}
		src.gameObject.SetActive(false);
		src.transform.parent = holder;
		singleton.active.Remove(src);
		singleton.pool.Enqueue(src);
	}

	private void SetUpPoolReserve()
	{
		for (int i = 0; i < poolReserve; i++)
		{
			GameObject go = new GameObject();
			go.SetActive(false);
			go.transform.parent = holder;
			AudioSource src = go.AddComponent<AudioSource>();
			src.outputAudioMixerGroup = sfxMixer;
			src.spatialBlend = 1f;
			src.minDistance = MIN_DISTANCE;
			src.maxDistance = MAX_DISTANCE;
			pool.Enqueue(src);
		}
	}
}
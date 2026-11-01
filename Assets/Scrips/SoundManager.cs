
using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Range(0.1f, 3f)]
        public float pitch = 1f;

        public bool loop;
        public bool randomPitch;
        public bool preventOverlap;

        [Range(0.1f, 3f)]
        public float minPitch = 0.9f;

        [Range(0.1f, 3f)]
        public float maxPitch = 1.1f;

        [HideInInspector]
        public AudioSource source;
    }

    [SerializeField] private List<Sound> sounds = new List<Sound>();

    private readonly Dictionary<string, Sound> soundDictionary =
        new Dictionary<string, Sound>(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        soundDictionary.Clear();

        foreach (Sound sound in sounds)
        {
            if (sound == null || string.IsNullOrWhiteSpace(sound.name))
                continue;

            if (soundDictionary.ContainsKey(sound.name))
            {
                Debug.LogWarning("Duplicate sound name: " + sound.name, this);
                continue;
            }

            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = false;

            soundDictionary.Add(sound.name, sound);
        }
    }

    public void Play(string soundName)
    {
        if (!TryGetSound(soundName, out Sound sound))
            return;

        if (sound.clip == null)
        {
            Debug.LogWarning("No AudioClip assigned to: " + soundName, this);
            return;
        }

        if (sound.preventOverlap && sound.source.isPlaying)
            return;

        sound.source.pitch = sound.randomPitch
            ? UnityEngine.Random.Range(sound.minPitch, sound.maxPitch)
            : sound.pitch;

        sound.source.PlayOneShot(sound.clip, sound.volume);
    }

    public void PlaySoundEvent(string soundName)
    {
        Play(soundName);
    }

    public void PlayLoop(string soundName)
    {
        if (!TryGetSound(soundName, out Sound sound))
            return;

        if (sound.clip == null)
            return;

        sound.source.Stop();
        sound.source.clip = sound.clip;
        sound.source.volume = sound.volume;
        sound.source.pitch = sound.pitch;
        sound.source.loop = true;
        sound.source.Play();
    }

    public void Stop(string soundName)
    {
        if (!TryGetSound(soundName, out Sound sound))
            return;

        sound.source.Stop();
        sound.source.loop = false;
    }

    public void StopAll()
    {
        foreach (Sound sound in sounds)
        {
            if (sound == null || sound.source == null)
                continue;

            sound.source.Stop();
            sound.source.loop = false;
        }
    }

    public void SetVolume(string soundName, float volume)
    {
        if (!TryGetSound(soundName, out Sound sound))
            return;

        sound.volume = Mathf.Clamp01(volume);
        sound.source.volume = sound.volume;
    }

    private bool TryGetSound(string soundName, out Sound sound)
    {
        if (!string.IsNullOrWhiteSpace(soundName) &&
            soundDictionary.TryGetValue(soundName, out sound))
        {
            return true;
        }

        sound = null;
        Debug.LogWarning("Sound not found: " + soundName, this);
        return false;
    }
}

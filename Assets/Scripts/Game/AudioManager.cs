using System.Collections.Generic;
using UnityEngine;

namespace WarmFamilyPuzzle
{
    // Plays the ElevenLabs-generated clips from Resources/Audio.
    public static class AudioManager
    {
        static AudioSource musicSource;
        static AudioSource sfxSource;
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        public static void Init(Transform parent)
        {
            if (musicSource != null)
            {
                return;
            }

            var go = new GameObject("Audio");
            go.transform.SetParent(parent, false);

            musicSource = go.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.3f;
            musicSource.playOnAwake = false;

            sfxSource = go.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            var music = Load("music_warm");
            if (music != null)
            {
                musicSource.clip = music;
                musicSource.Play();
            }
        }

        public static void Play(string name, float volume = 1f)
        {
            AudioClip clip = Load(name);
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        static AudioClip Load(string name)
        {
            if (!clips.TryGetValue(name, out AudioClip clip))
            {
                clip = Resources.Load<AudioClip>("Audio/" + name);
                clips[name] = clip;
            }

            return clip;
        }
    }
}

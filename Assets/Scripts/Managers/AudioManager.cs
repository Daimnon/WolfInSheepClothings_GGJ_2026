using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance; // singleton
        public static AudioManager Instance => instance;
        [SerializeField] private List<Sound> musicList; // custom class for a single sound in a list
        [SerializeField] private List<Sound> sfxList; // custom class for a single sound in a list

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;


            foreach (Sound s in musicList) // instantiates all sounds 
            {
                s.audioSource = gameObject.AddComponent<AudioSource>();
                s.audioSource.clip = s.clip;

                s.audioSource.volume = s.volume;
                s.audioSource.pitch = s.pitch;
                s.audioSource.loop = s.loop;
            }

            foreach (Sound s in sfxList) // instantiates all sounds 
            {
                s.audioSource = gameObject.AddComponent<AudioSource>();
                s.audioSource.clip = s.clip;

                s.audioSource.volume = s.volume;
                s.audioSource.pitch = s.pitch;
                s.audioSource.loop = s.loop;
            }

            DontDestroyOnLoad(gameObject);
        }

        public void PlaySound(SoundType st, string soundName) // a function to play a sound from anywhere in the script
        {
            var sounds = st == SoundType.Music ? musicList : sfxList;
            foreach (Sound s in sounds)
            {
                if (s.name == soundName)
                {
                    s.audioSource.Play();
                }
            }
        }

        public void SFXEnabled(bool mute)
        {
            foreach (Sound s in sfxList)
            {
                s.audioSource.mute = !mute;
            }
        }

        public void MusicEnabled(bool mute)
        {
            foreach (Sound s in musicList)
            {
                s.audioSource.mute = !mute;
            }
        }
    }

    public enum SoundType
    {
        Music,
        SFX
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BEKStudio
{
    public class AudioController : MonoBehaviour
    {
        public static AudioController Instance; // Singleton Instance

        [Header("Audio Sources")]
        [SerializeField] private AudioSource MusicSource;
        [SerializeField] private AudioSource SFXSource;

        [Header("Audio Clips")]
        public AudioClip pawnMoveClip;
        public AudioClip diceClip;
        public AudioClip ClickButtonClip;
        public AudioClip CloseClip;
        public AudioClip pawnkillClip;
        public AudioClip winningClip;
        public AudioClip SafeClip;
        public AudioClip MouseClickClip;
        public AudioClip ClosePanelClip;
        public AudioClip SwipeClip;
        public AudioClip backgroundClip;



        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
            }
            else
            {
                Destroy(gameObject); // Prevent duplicates
            }

            SceneManager.sceneLoaded += OnSceneChanged; // Listen for scene changes
        }

        private void Start()
        {
            if (MusicSource != null && backgroundClip != null)
            {
                MusicSource.clip = backgroundClip;
                MusicSource.loop = true;
                MusicSource.Play();
            }
        }

        //Play Sound Effects Anywhere
        public void PlaySFX(AudioClip clip)
        {
            if (clip != null && SFXSource != null)
            {
                SFXSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("SFX Clip or Source is missing!");
            }
        }

        // Play Background Music
        public void PlayMusic(AudioClip clip)
        {
            if (clip != null && MusicSource != null)
            {
                MusicSource.clip = clip;
                MusicSource.loop = true;
                MusicSource.Play();
            }
            else
            {
                Debug.LogWarning("Music Clip or Source is missing!");
            }
        }

        // Adjust Music Volume
        public void SetMusicVolume(float volume)
        {
            MusicSource.volume = Mathf.Clamp01(volume);
        }

        //Adjust SFX Volume
        public void SetSFXVolume(float volume)
        {
            SFXSource.volume = Mathf.Clamp01(volume);
        }

        private void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Game")
            {
                SetMusicVolume(0.3f); // Lower music in Game scene
            }
            else
            {
                SetMusicVolume(1f); // Restore normal volume in other scenes
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneChanged; // Unsubscribe event
        }
    }
}


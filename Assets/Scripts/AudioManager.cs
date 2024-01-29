using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance { get; private set; }


    private void CreateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    private AudioSource audioSource;
    
    private void Awake()
    {
        CreateSingleton();
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio(AudioClip audio)
    {
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(audio);
    }
    
    public void PlayAudio(AudioClip audio, Vector3 position)
    {
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        AudioSource.PlayClipAtPoint(audio,position);
    }
}

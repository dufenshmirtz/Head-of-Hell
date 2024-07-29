using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSliderScript : MonoBehaviour
{
    public AudioSource menuMusicSource;
    public AudioSource menuSFXSource;
    public Slider volumeSlider;
    public static SoundSliderScript instance;

    private const string VolumePrefKey = "Volume";

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load volume settings
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1.0f);
        volumeSlider.value = savedVolume;
        UpdateVolume(volumeSlider.value);
    }

    public void ChangeVolume()
    {
        UpdateVolume(volumeSlider.value);
        // Save volume settings
        PlayerPrefs.SetFloat(VolumePrefKey, volumeSlider.value);
    }

    private void UpdateVolume(float volume)
    {
        menuMusicSource.volume = volume;
        menuSFXSource.volume = volume;
    }
}


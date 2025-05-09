using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public AudioSource music;
    public AudioSource SFX;

    public List<AudioClip> playlist;
    public List<AudioClip> menuPlaylist;

    //clips
    public AudioClip death;
    public AudioClip heavyattack;
    public AudioClip lightattack;
    public AudioClip jump;
    public AudioClip land;
    public AudioClip backround;
    public AudioClip swoosh;
    public AudioClip heavyswoosh;
    public AudioClip run;
    public AudioClip dash;
    public AudioClip dashHit;

    public AudioClip counterScream;
    public AudioClip counterClong;
    public AudioClip counterSucces;

    public AudioClip stabHit;
    public AudioClip stab;
    public AudioClip grab;
    public AudioClip klong;
    public AudioClip lullaby;
    public AudioClip growl;
    public AudioClip knockback;
    public AudioClip buttonClick;
    public AudioClip smash;
    public AudioClip charged;
    public AudioClip shoot;
    public AudioClip reload;
    public AudioClip fuse;
    public AudioClip explosion;
    public AudioClip lighter;
    public AudioClip katanaSeath;
    public AudioClip katanaHit;
    public AudioClip katanaHit2;
    public AudioClip katanaSwoosh;
    public AudioClip sworDashin;
    public AudioClip sworDashHit;
    public AudioClip sworDashMiss;
    public AudioClip sworDashTada;
    public AudioClip roll;
    public AudioClip rollReady;
    public AudioClip skiplaHeavyHit;
    public AudioClip skiplaHeavyCharge;
    public AudioClip BigusHeavy;

    public AudioClip bellPunch;
    public AudioClip bellDash;
    public AudioClip bellDashHit;
    public AudioClip bellSpell;

    public AudioClip sytheDash;
    public AudioClip sytheSlash;
    public AudioClip sytheHit;
    public AudioClip fireblast;
    public AudioClip beam;
    public AudioClip beamHit;
    public AudioClip bigExplosion;
    public AudioClip poison;
    public AudioClip sip;
    public AudioClip powerup;
    public AudioClip powerupSpawned;
    public AudioClip trailerSound;
    public AudioClip dearth;
    public AudioClip dramaticDrums;

    public AudioClip idleGlitch;
    public AudioClip jumpGlitch;
    public AudioClip dashGlitch;
    public AudioClip quickGlitch;
    public AudioClip chargeGlitch;
    public AudioClip heavyGlitch;
    public AudioClip heavyGlitchHit;
    public AudioClip nothitGlitch;

    public AudioClip volchBite;
    public AudioClip volchSpit;
    public AudioClip volchBiteSuccess;
    public AudioClip volchBiteExtra;

    public AudioClip skipWin;
    public AudioClip skipPick;
    public AudioClip gabaWin;
    public AudioClip gabaPick;

    public AudioClip portal;

    public AudioClip incense;
    public AudioClip waterSplash;

    public AudioClip transformation;
    public AudioClip whip;
    public AudioClip coinSound;



    public AudioClip shotgunBlast,alarm;

    //volumes
    public float deathVolume = 1.0f;
    public float heavyAttackVolume = 1.0f;
    public float lightAttackVolume = 1.0f;
    public float jumpVolume = 1.0f;
    public float landVolume = 1.0f;
    public float swooshVolume = 1.0f;
    public float heavySwooshVolume = 1.0f;
    public float runVolume = 1.0f;
    public float counterVol = 0.4f;
    public float counterClongVol = 1.0f;
    public float doubleVol = 2.0f;
    public float lessVol = 0.8f;
    public float lullaVol = 0.8f;
    public float normalVol = 1f;





    //settings
    private static string musicPref = "MusicPref";
    private static string sfxPref = "SfxPref";
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;

    private Dictionary<AudioClip, AudioSource> activeSoundEffects = new Dictionary<AudioClip, AudioSource>();


    private void Start()
    {
              
    }

    public void StopMusic()
    {
        music.Stop();
    }

    public void PauseMusic()
    {
        music.Pause();
    }

    public void StartMusic()
    {
        music.UnPause();
    }


    public void PlaySFX(AudioClip sfx,float volume)
    {
        SFX.PlayOneShot(sfx,volume);
    }

    public void StopSFX()
    {
            SFX.Stop();
    }

    private void Awake()
    {
        ContinueSettings();
    }

    private void ContinueSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(musicPref);
        sfxVolume = PlayerPrefs.GetFloat(sfxPref);

        music.volume = musicVolume;
        SFX.volume = sfxVolume;
    }

    public void ButtonSound()
    {
        SFX.PlayOneShot(buttonClick, 0.2f);
    }

    public void BoomSound()
    {
        SFX.PlayOneShot(explosion, lessVol);
    }

    public void PlayMusic()
    {
        if (playlist.Count > 0)
        {
            // Select a random index from the playlist
            int randomIndex = Random.Range(0, playlist.Count);

            // Get the randomly selected AudioClip
            AudioClip selectedSong = playlist[randomIndex];

            // Assign the selected song to the music AudioSource
            music.clip = selectedSong;

            // Play the selected song
            music.Play();
        }
        else
        {
            Debug.LogError("Playlist is empty. Add AudioClips to the playlist in the Inspector.");
        }
    }

    public void PlayAndStoreSFX(AudioClip sfx, float volume)
    {
        // Create a new AudioSource to play the sound effect
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.volume = volume;
        tempSource.clip = sfx;
        tempSource.Play();

        // Remove the AudioSource after the clip is done playing
        StartCoroutine(RemoveAfterFinish(tempSource));

        // Store this AudioSource in the dictionary
        activeSoundEffects[sfx] = tempSource;
    }

    public void StopAndStoreSFX(AudioClip sfx)
    {
        // Check if the sound effect is currently playing
        if (activeSoundEffects.ContainsKey(sfx))
        {
            AudioSource source = activeSoundEffects[sfx];

            // Stop the AudioSource and remove it
            if (source != null)
            {
                source.Stop();
                Destroy(source); // Clean up the AudioSource after stopping
            }

            activeSoundEffects.Remove(sfx);
        }
    }

    private IEnumerator RemoveAfterFinish(AudioSource source)
    {
        // Wait until the sound effect finishes playing
        yield return new WaitForSeconds(source.clip.length);

        // Remove the AudioSource from the dictionary and destroy it
        Destroy(source);
    }
}


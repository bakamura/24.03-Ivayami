using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManagaer : MonoBehaviour
{
    Bus Music;
    Bus SFX;
    Bus Master;
    float musicVolume = 0.5f;
    float SFXVolume=0.5f;
    float masterVolume=0.5f;

    public EventReference teste;
    EventInstance testeSom;
    public EventReference musica;
    EventInstance musicaSom;
    // Start is called before the first frame update
    void Start()
    {
        musicaSom = RuntimeManager.CreateInstance(musica);
        musicaSom.start();
        testeSom = RuntimeManager.CreateInstance(teste);
        Music = RuntimeManager.GetBus("bus:/Master/Music");
        SFX= RuntimeManager.GetBus("bus:/Master/SFX");
        Master= RuntimeManager.GetBus("bus:/Master");
    }

    // Update is called once per frame
    void Update()
    {
        Music.setVolume(musicVolume);
        SFX.setVolume(SFXVolume);
        Master.setVolume(masterVolume);
    } 

    public void SFXVolumeLevel(float newSFXVolume) 
    {
        SFXVolume = newSFXVolume;
    }

    public void musicVolumeLevel(float newMusicVolume)
    {
        musicVolume = newMusicVolume;
    }

    public void materVolumeLevel(float newMasterVolume)
    {
        masterVolume = newMasterVolume;
    }

    public void playButton()
    {
        RuntimeManager.PlayOneShot(teste);
    }
}

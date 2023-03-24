using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsManager : MonoBehaviour
{

    public Slider sliderBgm;
    public Slider sliderSfx;
    public AudioMixer mixer;

    // Start is called before the first frame update
    void Start()
    {
        LoadBGM_SFX();
    }

    public void LoadBGM_SFX()
    {
        if (PlayerPrefs.HasKey("BGM_VOL"))
        {
            sliderBgm.value = PlayerPrefs.GetFloat("BGM_VOL");
        }
        if (PlayerPrefs.HasKey("SFX_VOL"))
        {
            sliderSfx.value = PlayerPrefs.GetFloat("SFX_VOL");
        }
    }

    public void SetBGMVol(float vol)
    {
        mixer.SetFloat("BGM_VOL", vol);
        PlayerPrefs.SetFloat("BGM_VOL", vol);
    }

    public void SetSFXVol(float vol)
    {
        mixer.SetFloat("SFX_VOL", vol);
        PlayerPrefs.SetFloat("SFX_VOL", vol);
    }

}
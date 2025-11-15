using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsScript : MonoBehaviour
{
    //public mainmenu menu;
    public AudioMixer theMixer;
    public TMP_Text masterlabel, musiclabel, soundlabel, voicelabel, fullscreenlabel;
    public Slider masterslider, musicslider, soundslider, voiceslider, fullscreenSlider; // 0 = Windowed, 1 = Borderless, 2 = Fullscreen

    void Start()
    {
        // Load saved audio settings
        if (PlayerPrefs.HasKey("Master"))
        {
            theMixer.SetFloat("Master", PlayerPrefs.GetFloat("Master"));
        }
        if (PlayerPrefs.HasKey("MUSIC"))
        {
            theMixer.SetFloat("MUSIC", PlayerPrefs.GetFloat("MUSIC"));
        }
        if (PlayerPrefs.HasKey("SFX"))
        {
            theMixer.SetFloat("SFX", PlayerPrefs.GetFloat("SFX"));
        }
        if (PlayerPrefs.HasKey("VOICE"))
        {
            theMixer.SetFloat("VOICE", PlayerPrefs.GetFloat("VOICE"));
        }

        // Load volume sliders
        float vol = 0f;
        theMixer.GetFloat("Master", out vol);
        masterslider.value = vol;
        theMixer.GetFloat("MUSIC", out vol);
        musicslider.value = vol;
        theMixer.GetFloat("SFX", out vol);
        soundslider.value = vol;
        theMixer.GetFloat("VOICE", out vol);
        voiceslider.value = vol;

        masterlabel.text = (masterslider.value + 80).ToString();
        musiclabel.text = (musicslider.value + 80).ToString();
        soundlabel.text = (soundslider.value + 80).ToString();
        voicelabel.text = (voiceslider.value + 80).ToString();

        // Load Fullscreen Mode preference
        if (PlayerPrefs.HasKey("FullscreenMode"))
        {
            fullscreenSlider.value = PlayerPrefs.GetInt("FullscreenMode");
        }
        else
        {
            fullscreenSlider.value = GetCurrentFullscreenModeIndex();
        }

        ApplyGraphics();
    }

    void Update()
    {
        setmastervol();
    }

    public void ApplyGraphics()
    {
        int mode = Mathf.RoundToInt(fullscreenSlider.value); // Ensure we get an integer value
        switch (mode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.MaximizedWindow; // Borderless
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }

        PlayerPrefs.SetInt("FullscreenMode", mode);
        PlayerPrefs.Save();
    }

    private int GetCurrentFullscreenModeIndex()
    {
        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.Windowed:
                return 0;
            case FullScreenMode.MaximizedWindow:
                return 1;
            case FullScreenMode.FullScreenWindow:
                return 2;
            default:
                return 1; // Default to borderless if unknown
        }
    }

    public void setmastervol()
    {
        masterlabel.text = (masterslider.value + 80).ToString();
        theMixer.SetFloat("Master", masterslider.value);
        PlayerPrefs.SetFloat("Master", masterslider.value);
    }

    public void setmusicvol()
    {
        musiclabel.text = (musicslider.value + 80).ToString();
        theMixer.SetFloat("MUSIC", musicslider.value);
        PlayerPrefs.SetFloat("MUSIC", musicslider.value);
    }

    public void setsoundvol()
    {
        soundlabel.text = (soundslider.value + 80).ToString();
        theMixer.SetFloat("SFX", soundslider.value);
        PlayerPrefs.SetFloat("SFX", soundslider.value);
    }
    
    public void setvoicevol()
    {
        voicelabel.text = (voiceslider.value + 80).ToString();
        theMixer.SetFloat("VOICE", voiceslider.value);
        PlayerPrefs.SetFloat("VOICE", voiceslider.value);
    }

    public void setfullscreenlabel()
    {
        int mode = Mathf.RoundToInt(fullscreenSlider.value); // Get the selected mode
        string fullscreenModeName = "";

        switch (mode)
        {
            case 0:
                fullscreenModeName = "Windowed";
                break;
            case 1:
                fullscreenModeName = "Borderless Window";
                break;
            case 2:
                fullscreenModeName = "Fullscreen";
                break;
        }

        fullscreenlabel.text = fullscreenModeName;
    }
}

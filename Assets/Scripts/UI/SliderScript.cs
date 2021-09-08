using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider slider;


	private void Start()
	{
        slider.value = PlayerPrefs.GetFloat("Volume", 0.75f);
	}

	public void SetLevel(float sliderValue)
    {
        mixer.SetFloat("Volume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("Volume", sliderValue);
    }
}

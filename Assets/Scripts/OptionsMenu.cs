using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Slider audioSlider;
    public AudioSource audioSource;

    void Start()
    {
        // Load the saved volume level, default to 1.0 if no saved value exists
        float savedVolume = PlayerPrefs.GetFloat("audioVolume", 1f);
        audioSlider.value = savedVolume;
        audioSource.volume = savedVolume;

        // Attach the OnVolumeChanged method to the slider's value change event
        audioSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    public void OnVolumeChanged(float volume)
    {
        // Update the audio source's volume and save the new value
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("audioVolume", volume);
    }
}

using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton instance of GameManager for global access
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        #region Singleton Pattern Setup

        // Enforce a unique instance: if one already exists, self-destruct.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Establish this instance as the global instance and persist across scene loads.
        Instance = this;

        #endregion
    }

    public void PlayTempSFX(AudioClip clip)
    {
        float minPitch = 0.85f;
        float maxPitch = 1.15f;

        GameObject tempSoundPlayer = new GameObject($"OneShot_{clip.name}");
        tempSoundPlayer.transform.SetParent(transform, false);

        AudioSource audioSource = tempSoundPlayer.AddComponent<AudioSource>();

        //Apply a random pitch variation
        // Unlike integers, float parameters in Random.Range are inclusive for both min and max
        float randomPitch = Random.Range(minPitch, maxPitch);

        audioSource.pitch = randomPitch;

        audioSource.clip = clip;
        audioSource.Play();

        Destroy(audioSource, clip.length);
    }
}

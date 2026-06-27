using UnityEngine;

public class SimpleSceneMusic : MonoBehaviour
{
    [Header("Audio Component")]
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Clip")]
    [SerializeField] private AudioClip musikSceneIni;

    void Start()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        // Putar musik jika clip sudah dimasukkan
        if (musicSource != null && musikSceneIni != null)
        {
            musicSource.clip = musikSceneIni;
            musicSource.loop = true; // Karena cuma 1 lagu, kita set loop terus menerus
            musicSource.Play();
        }
    }
}
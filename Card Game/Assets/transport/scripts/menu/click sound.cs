using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class clicksound : MonoBehaviour
{
    private AudioSource audioSource;
    void start()
    {
        audioSource = GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }
    void PlaySound()
    {
        audioSource.Play();
    }
}
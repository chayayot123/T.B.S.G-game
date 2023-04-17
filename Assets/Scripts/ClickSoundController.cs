using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickSoundController : MonoBehaviour
{
    private AudioSource clickAudioSource;

    void Start()
    {
        clickAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detect left mouse button click
        {
            clickAudioSource.Play();
        }
    }
}
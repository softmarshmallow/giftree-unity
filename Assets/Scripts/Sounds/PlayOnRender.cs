using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class PlayOnRender : MonoBehaviour
{
    private bool isSoundPlayed = false;
    Renderer m_Renderer;
    private AudioSource _audioSource;
    public AudioClip Clip;
    public float Delay = 0;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Renderer.isVisible)
        {
            if (!isSoundPlayed)
            {
                PlaySound();
            }
        }
    }

    void PlaySound()
    {
        isSoundPlayed = true;
        StartCoroutine(PlaySoundAfterDelay());
    }

    IEnumerator PlaySoundAfterDelay()
    {
        yield return new WaitForSeconds(Delay);
        _audioSource.PlayOneShot(Clip);
    }
}

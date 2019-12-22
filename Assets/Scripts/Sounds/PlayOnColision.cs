using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(AudioSource))]
public class PlayOnColision : MonoBehaviour
{

    private Collider2D _collider2D;
    private AudioSource _audioSource;
    public AudioClip Clip;
    public float Delay = 0;
    // Start is called before the first frame update
    void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("OnTriggerEnter2D:: Sound play");
        StartCoroutine(PlaySoundAfterDelay());
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("OnCollisionEnter2D:: Sound play");
        StartCoroutine(PlaySoundAfterDelay());
    }

    IEnumerator PlaySoundAfterDelay()
    {
        yield return new WaitForSeconds(Delay);
        _audioSource.PlayOneShot(Clip);
    }
}

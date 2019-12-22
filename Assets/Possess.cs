using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Possess : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite onSprite;

    public Transform eyePos;

    private SpriteRenderer spriteRenderer;
    public Transform secend;
    public GameObject particle;
    public Transform particlePos;
    private bool isElec;

    private void Start()
    {
        eyePos = transform.GetChild(0);
        spriteRenderer = GetComponent<SpriteRenderer>();
        eyePos.gameObject.SetActive(false);
    }

    public void PossessObject()
    {
        
        if(onSprite!=null)
        spriteRenderer.sprite = onSprite;
        if(eyePos!=null)
        eyePos.gameObject.SetActive(true);
        if (particle != null)
        {
            GameObject parti = Instantiate(particle, transform);
            parti.transform.position = particlePos.position;
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electronic : MonoBehaviour
{
    public Transform startPos;
    public Transform endPos;

    public GameObject pos;

    private Vector2 movePos;
    private SpriteRenderer eysSR;
    private SpriteRenderer bodySR;
    
    // Start is called before the first frame update
    void Start()
    {
        movePos = endPos.position - startPos.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveElec(GameObject go,SpriteRenderer esp,SpriteRenderer bsp)
    {
        pos = go;
        pos.transform.position = startPos.position;
        eysSR = esp;
        bodySR = bsp;
        eysSR.enabled = false;
        bodySR.enabled = false;
        StartCoroutine(move());
        
    }

    IEnumerator move()
    {
        float t = Time.time;
        while (t>Time.time-5)
        {
            pos.transform.position += (Vector3)movePos*Time.deltaTime/5;
            
            yield return null;
        }

        eysSR.enabled = true;
        bodySR.enabled = true;
    }
}

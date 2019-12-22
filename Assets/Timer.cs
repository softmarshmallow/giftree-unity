using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
public float time;
private float maxTime;
public Image image;
private bool castDie;
public playerScript ps;
// Start is called before the first frame update
    void Start()
    {
        maxTime = time;
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        image.fillAmount = time / maxTime;
        if (time < 0)
        {
            if (!castDie)
            {
                //Do SomeThing
                ps.DiePlayer();
                castDie = true;
            }
            
        }
    }
}

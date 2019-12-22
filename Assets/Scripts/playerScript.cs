using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum State
{
    Idle,Pressed,FlyStart,Flying,FlyEnd,FormChange
}

[RequireComponent(typeof(AudioSource))]
public class playerScript : MonoBehaviour
{
    private AudioSource _audioSource;
    private Vector3 startPos;
    private Vector3 endPos;
    public Vector3 initPos;
    private Rigidbody2D rigidbody;
    private Vector3 forceAtPlayer;
    public float forceFactor;
    private Progress pg;
    public GameObject trajectoryDot;
    private GameObject[] trajectoryDots;
    public int number;
    private bool isPressed;
    public SpriteRenderer sp;
    public SpriteRenderer eyeSP;
    public GameObject eff;
    private Possess possess;
    public int stage;

    public GameObject dieEffect;
    public AudioClip dieSound;
    public Transform endingPos;
    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        trajectoryDots = new GameObject[number];
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) { 
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray ray = new Ray(startPos,Vector3.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    isPressed = true;
                    rigidbody.gravityScale = 0;
                    rigidbody.velocity = Vector2.zero;
                    for (int i = 0; i < number; i++)
                    {
                        trajectoryDots[i] = Instantiate(trajectoryDot, gameObject.transform);
                    }
                }
            }
        }
        if(isPressed) { //drag
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
            //gameObject.transform.position = endPos;
            forceAtPlayer = endPos - startPos;
            for (int i = 0; i < number; i++)
            {
                trajectoryDots[i].transform.position = calculatePosition(i * 0.1f);
            }
        }
        if(Input.GetMouseButtonUp(0)) { //leave
            if (isPressed)
            {
                sp.enabled = true;
                rigidbody.gravityScale = 1;
                rigidbody.velocity = new Vector2(-forceAtPlayer.x * forceFactor, -forceAtPlayer.y * forceFactor);
                for (int i = 0; i < number; i++)
                {
                    Destroy(trajectoryDots[i]);
                }

                isPressed = false;
                if (pg != null)
                {
                    pg.bPossess = false;
                    pg = null;
                    eyeSP.enabled = true;

                }
            }
        }
        if(Input.GetKey(KeyCode.Space)) {
            rigidbody.gravityScale = 0;
            rigidbody.velocity = Vector2.zero;
            gameObject.transform.position = initPos;
            
        }
    }

    public void DiePlayer()
    {
        _audioSource.PlayOneShot(dieSound);
        GameObject particle = Instantiate(dieEffect);
        particle.transform.position = transform.position;
        Destroy(particle,3f);
        eyeSP.enabled = false;
        rigidbody.velocity = Vector2.zero;
        rigidbody.gravityScale = 0;
        eff.SetActive(false);
        GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(Restart());
    }
    private Vector2 calculatePosition(float elapsedTime) {
        return new Vector2(endPos.x, endPos.y) + //X0
                new Vector2(-forceAtPlayer.x * forceFactor, -forceAtPlayer.y * forceFactor) * elapsedTime + //ut
                0.5f * Physics2D.gravity * elapsedTime * elapsedTime ;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fire"))
        {
            sp.enabled = false;
            transform.position = other.transform.position;
            rigidbody.gravityScale = 0;
            rigidbody.velocity = Vector2.zero;
            Possess _possess = other.GetComponent<Possess>();
            if (_possess != null)
            {
                possess = _possess;
                possess.PossessObject();
                transform.position = (Vector2)possess.eyePos.position+Vector2.down;
            }

            pg = other.GetComponent<Progress>();
            if (pg != null)
            {
                pg.possessedPos = transform;
                pg.bPossess = true;
                transform.position = pg.startPos.position;
                eyeSP.enabled = false;
            }

            Electronic et = other.GetComponent<Electronic>();
            if (et != null)
            {
                et.MoveElec(gameObject,eyeSP,sp);
            }
        }
        if(other.CompareTag("Danger"))
        {
            DiePlayer();
            
        }

        if (other.CompareTag("Finish"))
        {
            if (stage <= 2)
            {
                //암전 코드
                SceneManager.LoadScene("Stage" + (stage + 1));
            }

            if (stage >= 3)
            {
                StartCoroutine(EndingEnding());
            }
        }
    }

    IEnumerator Restart()
    {
        yield return new WaitForSeconds(3.5f);
        SceneManager.LoadScene("Stage" + stage);
    }
    //IEnumerator StatePattern()
   //{
   //    while (true)
   //    {
   //        switch (curState)
   //        {
   //            case State.Idle:

   //                break;
   //            case State.Pressed:

   //                break;
   //            case State.FlyStart:

   //                break;
   //            case State.Flying:

   //                break;
   //            case State.FlyEnd:

   //                break;
   //            case State.FormChange:

   //                break;
   //        }
   //        yield return null;
   //    }
   //}
   IEnumerator EndingEnding()
   {
       float f = Time.time;
       rigidbody.velocity = Vector2.zero;
       rigidbody.gravityScale = 0;
       Vector2 movedir = endingPos.position - transform.position;
       while (f+3>Time.time)
       {
           transform.position +=(Vector3) movedir * Time.deltaTime / 3;
           yield return null;
       }
       SceneManager.LoadScene("Ending");
   }
}
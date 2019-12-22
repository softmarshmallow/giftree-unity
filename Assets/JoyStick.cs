using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyStick : MonoBehaviour
{
    private Camera mainCamera;

    private bool isTouchedPlayer;
    public GameObject player;
    private float jumpPower;

    private Vector2 jumpDir;

    private Rigidbody2D rd;

    public SpriteRenderer sp;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("k");
        rd = player.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        DetectPlayer();
        Debug.Log(rd.velocity);
    }

    void DetectPlayer()
    {
        #if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPos = mainCamera.ScreenToWorldPoint(touch.position);
            touchPos.z = -1;
            Debug.Log(touch.position);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                {
                    Ray ray = new Ray(touchPos, Vector3.forward);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            isTouchedPlayer = true;
                        }
                    }

                    break;
                }
                case TouchPhase.Moved:
                    if (isTouchedPlayer)
                    {
                        Debug.DrawLine(touchPos,Vector3.forward);
                        var position = player.transform.position;
                        jumpPower = (touchPos.x - position.x) * (touchPos.x - position.x) +
                                    (touchPos.y - position.y) * (touchPos.y - position.y);
                        jumpDir = ((Vector2)touchPos - (Vector2) position).normalized;
                        Debug.Log($"JumpPower = {jumpPower}, JumpDir = {jumpDir}");
                    }

                    break;
                case TouchPhase.Ended:
                    isTouchedPlayer = false;
                    break;
            }
        }
        #endif
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 touchPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Ray ray = new Ray(touchPos, Vector3.forward);
            Debug.DrawRay(touchPos,Vector3.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    isTouchedPlayer = true;
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (isTouchedPlayer)
            {
                Vector3 touchPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                var position = player.transform.position;
                jumpPower = (touchPos.x - position.x) * (touchPos.x - position.x) +
                            (touchPos.y - position.y) * (touchPos.y - position.y);
                jumpDir = ( (Vector2) position-(Vector2)touchPos);
                rd.velocity = Vector2.zero;
                rd.gravityScale = 0;
                Debug.DrawLine(transform.position,(Vector2)transform.position+jumpDir,Color.yellow);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isTouchedPlayer = false;
            rd.gravityScale = 1;
            rd.velocity = jumpDir ;
            Debug.Log($"JumpPower = {jumpPower}, JumpDir = {jumpDir}");
            
        }
        #endif
    }
}

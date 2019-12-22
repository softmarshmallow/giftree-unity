using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Progress : MonoBehaviour
{
    private Material mt;

    private float t = 1;
    public bool bPossess;
    private static readonly int Progress1 = Shader.PropertyToID("_Progress");
    public GameObject go;

    public Vector3 targetPos;

    public Transform possessedPos;
    public Transform startPos;
    public Transform endPos;
    private Vector2 distance;
    private float rate;
    public GameObject wirePrame;
    private GameObject wp;

    private Vector3 movePos;
    // Start is called before the first frame update
    void Start()
    {
        mt = GetComponent<SpriteRenderer>().material;
        distance = endPos.position - startPos.position;
        Debug.Log(distance);
        wp = Instantiate(wirePrame);
        wp.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (bPossess)
        {
            wp.SetActive(true);
            t -= Time.deltaTime / 5;
            mt.SetFloat(Progress1,t);
            possessedPos.position=new Vector3(possessedPos.position.x,possessedPos.position.y+distance.y*Time.deltaTime/5);
            wp.transform.position=new Vector3(possessedPos.position.x,possessedPos.position.y+distance.y*Time.deltaTime/5);
        }
    }

    public void OnPossess()
    {
        
    }
}

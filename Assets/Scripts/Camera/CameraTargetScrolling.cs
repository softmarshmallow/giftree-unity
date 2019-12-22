using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetScrolling : MonoBehaviour
{
    public List<TransformMap> durationByTargetMap;
    private Transform currentTarget;
    // Start is called before the first frame update
    void Awake()
    {
    }

    void getNextTarget()
    {

    }
    float t;


    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime/durationByTargetMap[1].duration;
        transform.position = Vector3.Lerp(durationByTargetMap[0].transform.position, durationByTargetMap[1].transform.position, t);
    }
}

[Serializable]
public class TransformMap
{
    public Transform transform;
    public float duration;
}
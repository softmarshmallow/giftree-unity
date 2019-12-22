using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class PeopleGenerator : MonoBehaviour
{
    public Transform[] genpos;

    public GameObject people;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Generator");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Generator()
    {
        while (true)
        {
            int rand = UnityEngine.Random.Range(0, 101);
            if (rand < 50)
            {
                Instantiate(people).transform.position = genpos[UnityEngine.Random.Range(0, genpos.Length)].position;
                yield return new WaitForSeconds(2f);
            }
            yield return new WaitForSeconds(2f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BirthSceneManager : MonoBehaviour
{
    public GameObject Cutscenes;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveNextSceneAfterDelay());
    }

    IEnumerator MoveNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(6.4f);
        Cutscenes.SetActive(true);
        yield return new WaitForSeconds(6f);
        SceneManager.LoadScene("Stage1");
    }

}

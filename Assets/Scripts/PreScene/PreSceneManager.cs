using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreSceneManager : MonoBehaviour
{
    public Animator lifeAnimator;

    public Animator camAnimator;

    public Animator logoAnimator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStartClick()
    {
        StartPreAnimationBeforeNextScene();
    }

    void StartPreAnimationBeforeNextScene()
    {
        StartCoroutine(MoveNextSceneAfterAnimation());
    }

    IEnumerator MoveNextSceneAfterAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        logoAnimator.Play("logo-fadeout", 0, 0);
        yield return new WaitForSeconds(0.5f);
        lifeAnimator.Play("pre-scene-life", 0, 0);
        yield return new WaitForSeconds(2.8f);
        camAnimator.Play("fade-out", 0, 0);
        // play animation here
        yield return new WaitForSeconds(2.2f);
        SceneManager.LoadScene("Birth", LoadSceneMode.Single);
    }
}

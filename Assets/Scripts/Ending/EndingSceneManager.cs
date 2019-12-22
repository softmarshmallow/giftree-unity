using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EndingSceneManager : MonoBehaviour
{
    public PostProcessVolume PostProcessVolume;

    private Bloom bloom;
    // Start is called before the first frame update
    void Start()
    {
        StartExitGameSequence();
        // bloom.intensity =
    }


    private bool doStartEndingSequence = false;
    float currentTime = 0f;
    float timeToMove = 3f;

    void Update() {

        if (doStartEndingSequence)
        {
            if (currentTime <= timeToMove)
            {
                currentTime += Time.deltaTime;
                bloom.intensity.value = Mathf.Lerp(0, 250, currentTime / timeToMove);
            }
            else
            {
                currentTime = 0f;
                doStartEndingSequence = false;
            }
        }

    }


    public void StartExitGameSequence()
    {
        Debug.Log("StartExitGameSequence");
        bloom = PostProcessVolume.profile.GetSetting<Bloom>();
        StartCoroutine(PostProcessingSequence());
    }

    IEnumerator PostProcessingSequence()
    {
        yield return new WaitForSeconds(20);
        doStartEndingSequence = true;
        // Debug.Log("yield return new WaitForSeconds(20);");
        // while (bloom.intensity == 280)
        // {
        //     bloom.intensity.value += 0.1f;
        //     yield return new WaitForSeconds(0.01f);
        // }
    }
}
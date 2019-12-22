using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class KillTimerDisplay : MonoBehaviour
{
    private Text _text;
    void Awake()
    {
        _text = GetComponent<Text>();
    }
    // Start is called before the first frame update
    void Start()
    {
        KillTimer.Instance.AddTimerCallback(OnTimerUpdate);
    }

    void OnTimerUpdate(int seconds)
    {
        _text.text = seconds.ToString();
    }
}

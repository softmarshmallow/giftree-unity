using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTimer : MonoBehaviour
{
    public static KillTimer Instance;
    public int initialTime;
    private Action<int> TimerCallback;
    private int _time;

    public int Time
    {
        get => _time;
        set
        {
            _time = value;
            TimerCallback?.Invoke(value);
        }
    }

    void Awake()
    {
        Instance = this;
    }



    public void AddTimerCallback(Action<int> callback)
    {
        TimerCallback += callback;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time = initialTime;
        StartTimer();
    }

    private Coroutine timerCoroutine;
    public void StartTimer()
    {
        timerCoroutine = StartCoroutine(CountDownSequence());
    }

    IEnumerator CountDownSequence()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            Time -= 1;
        }
    }

    public void EndTimer()
    {
        StopCoroutine(timerCoroutine);
    }

    public void RestartTimer()
    {
        EndTimer();
        Time = initialTime;
        StartTimer();
    }
}

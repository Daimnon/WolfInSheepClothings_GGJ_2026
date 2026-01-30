using System;
using System.Collections;
using UnityEngine;

public enum ShootableType
{
    Wolf,
    Sheep,
    BloodySheep
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager Instance => _instance;

    [Header("Timer")]
    [SerializeField] private float _timerDuration = 300.0f; // 300 = 5 mins
    private float _currentTimeLeft = 0.0f;
    private Coroutine _timerCoroutine = null;
    public static Action OnGameTimerEnd;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    #region Timer Methods
    public void StartTimer()
    {
        _timerCoroutine = StartCoroutine(TimerRoutine(_timerDuration));
    }
    private IEnumerator TimerRoutine(float duration)
    {
        float timeRemaining = duration;

        while (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;

            timeRemaining = Mathf.Max(timeRemaining, 0f);
            _currentTimeLeft = timeRemaining;
            Debug.Log(timeRemaining);

            yield return null; // wait one frame
        }

        InvokeOnGameTimerEnd();
    }
    public static void InvokeOnGameTimerEnd()
    {
        OnGameTimerEnd?.Invoke();
        Debug.Log("Event: OnGameTimerEnd");
    }
    #endregion
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum ShootableType
{
    Wolf,
    Sheep,
    BloodySheep,
    Hidden
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager Instance => _instance;

    [SerializeField] private ShepherdAI _shepherd;

    [Header("Timer")]
    [SerializeField] private GameObject _directionalLight;
    [SerializeField] private float _timerDuration = 300.0f; // 300 = 5 mins
    private float _currentTimeLeft = 0.0f;
    private Coroutine _timerCoroutine = null;
    public static Action OnGameTimerEnd;

    [Header("Events")]
    public static Action<Vector3> OnSheepKilled;
    public static Action UpdateSheepCount;
    public static Action OnPlayerAteSheep;
    
    public static int sheepCount = 0;

    private void OnEnable()
    {
        UpdateSheepCount += HandleSheepKilled;
    }

    private void OnDisable()
    {
        UpdateSheepCount -= HandleSheepKilled;
    }

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
    
    private void Start()
    {
        StartTimer();
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
            //Debug.Log(timeRemaining);

            yield return null; // wait one frame
        }

        InvokeOnGameTimerEnd();
    }
    public void InvokeOnGameTimerEnd()
    {
        OnGameTimerEnd?.Invoke();
        Debug.Log("Event: OnGameTimerEnd");
    }
    public void StopTimer()
    {
        StopCoroutine(_timerCoroutine);
        _timerCoroutine = null;
    }

    private void HandleSheepKilled()
    {
        sheepCount++;
    }
    
    #endregion
}

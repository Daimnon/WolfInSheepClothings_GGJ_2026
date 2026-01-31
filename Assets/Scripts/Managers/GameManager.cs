using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ShootableType
{
    Wolf,
    Sheep,
    BloodySheep,
    Hidden
}

public enum KilledBy
{
    Wolf,
    Shepherd
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
    public static Action<Vector3, KilledBy> OnSheepKilled;
    public static Action OnUpdateSheepCount;
    public static Action<int> OnUpdateSheepCountUI;
    public static Action OnPlayerAteSheep;
    
    public static int sheepCount = 0;

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

    private void OnEnable()
    {
        OnUpdateSheepCount += UpdateSheepCount;
    }
    private void OnDisable()
    {
        OnUpdateSheepCount -= UpdateSheepCount;
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
    #endregion

    #region SheepCount
    public void InvokeUpdateSheepCount()
    {
        OnUpdateSheepCount?.Invoke();
        Debug.Log("Event: OnUpdateSheepCount");
    }
    public void InvokeUpdateSheepCountUI(int sheepCount)
    {
        OnUpdateSheepCountUI?.Invoke(sheepCount);
        Debug.Log("Event: OnUpdateSheepCount");
    }
    private void UpdateSheepCount()
    {
        sheepCount++;
        InvokeUpdateSheepCountUI(sheepCount);
    }
    //?
    public void InvokeOnPlayerAteSheep()
    {
        OnPlayerAteSheep?.Invoke();
        Debug.Log("Event: OnPlayerAteSheep");
    }
    #endregion
}

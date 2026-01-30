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

    [Header("Timer")]
    [SerializeField] private float _timerDuration = 300.0f; // 300 = 5 mins
    [SerializeField] private ShepherdAI _shepherd;
    private float _currentTimeLeft = 0.0f;
    private Coroutine _timerCoroutine = null;
    public static Action OnGameTimerEnd;

    [Header("Events")]
    public static Action<Vector3> OnSheepKilled;
    public static Action UpdateSheepCount;
    
    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text sheepCountText;
    public TMP_Text aggroMeter;
    
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
        sheepCountText.text = ("Sheep killed: " + sheepCount);
        aggroMeter.text = ("Aggro meter: " + _shepherd.AggroMeter.ToString("F0") + "%");
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
    public static void InvokeOnGameTimerEnd()
    {
        OnGameTimerEnd?.Invoke();
        Debug.Log("Event: OnGameTimerEnd");
    }

    private void Update()
    {
        timerText.text = ("Timer: " + _currentTimeLeft.ToString("0.0"));
        aggroMeter.text = ("Aggro meter: " + _shepherd.AggroMeter.ToString("F0") + "%");
    }

    private void HandleSheepKilled()
    {
        sheepCount++;
        sheepCountText.text = ("Sheep killed: " + sheepCount);
    }
    
    #endregion
}

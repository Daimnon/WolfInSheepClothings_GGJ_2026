using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    [Header("SheepCount")]
    [SerializeField] private RectTransform _sheepCountIcon;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private TextMeshProUGUI _sheepCountTMP;
    [SerializeField] private float _sheepCountAnimDuration = 1.0f;
    private Vector3 _startScale;
    private Coroutine _sheepCountAnimation;

    private void OnEnable()
    {
        GameManager.OnUpdateSheepCountUI += UpdateSheepCount;
    }
    private void OnDisable()
    {
        GameManager.OnUpdateSheepCountUI -= UpdateSheepCount;
    }

    private void Start()
    {
        _sheepCountTMP.text = 0.ToString();
        _startScale = transform.localScale;
        gameObject.SetActive(false);
    }

    private void UpdateSheepCount(int sheepCount)
    {
        StartSheepCountAnimation();
        _sheepCountTMP.text = sheepCount.ToString();
    }

    private IEnumerator GrowShrink()
    {
        float t = 0f;

        while (t < _sheepCountAnimDuration)
        {
            t += Time.deltaTime;
            float normalized = t / _sheepCountAnimDuration;
            float scale = scaleCurve.Evaluate(normalized);
            _sheepCountIcon.localScale = _startScale * scale;
            yield return null;
        }

        _sheepCountIcon.localScale = _startScale;
    }

    private void StartSheepCountAnimation()
    {
        StopSheepCountAnimation();
        _sheepCountAnimation = StartCoroutine(GrowShrink());
    }
    private void StopSheepCountAnimation()
    {
        if (_sheepCountAnimation != null) StopCoroutine(_sheepCountAnimation);
        _sheepCountAnimation = null;
    }
}

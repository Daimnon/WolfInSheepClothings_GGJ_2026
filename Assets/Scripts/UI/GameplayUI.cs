using TMPro;
using UnityEngine;
using System.Collections;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    [Header("SheepCount")]
    [SerializeField] private GameObject _sheepCountIcon;
    [SerializeField] private TextMeshProUGUI _sheepCountTMP;
    [SerializeField] private float _sheepCountAnimDuration = 1.0f;
    [SerializeField] private float _sheepCountAnimMaxScale = 2.0f;
    private Coroutine _sheepCountAnimation;



    private void OnEnable()
    {
        GameManager.OnUpdateSheepCountUI += UpdateSheepCount;
    }
    private void OnDisable()
    {
        GameManager.OnUpdateSheepCountUI -= UpdateSheepCount;
    }

    private void UpdateSheepCount(int sheepCount)
    {
        StartSheepCountAnimation();
        _sheepCountTMP.text = sheepCount.ToString();
    }

    private IEnumerator GrowShrink()
    {
        Vector3 startScale = transform.localScale;
        float half = _sheepCountAnimDuration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, startScale * _sheepCountAnimMaxScale, t / half);
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale * _sheepCountAnimMaxScale, startScale, t / half);
            yield return null;
        }

        transform.localScale = startScale;
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

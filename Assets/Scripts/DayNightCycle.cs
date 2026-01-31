using System.Collections;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Light directionalLight;
    [SerializeField] private GameObject dayNightImg;
    [SerializeField] private float maxRotationX = 180f;
    [SerializeField] private Vector3 startRotation = new(50.0f, -30.0f, 0.0f);
    [SerializeField] private Vector3 startImgRotation = new(0.0f, 0.0f, -30.0f);
    private Coroutine _rotationCoroutine;

    public void StartDayNightCycleRotation()
    {
        if (_rotationCoroutine != null) StopCoroutine(_rotationCoroutine);
        _rotationCoroutine = StartCoroutine(RotateLightWithTimer());
    }
    private IEnumerator RotateLightWithTimer()
    {
        while (GameManager.Instance.CurrentTimeLeft > 0f)
        {
            float progress = 1f - (GameManager.Instance.CurrentTimeLeft / GameManager.Instance.TimerDuration);
            float xRotation = Mathf.Lerp(startRotation.x, maxRotationX, progress);
            float zImgRotation = Mathf.Lerp(startImgRotation.x, maxRotationX, progress);
            directionalLight.transform.rotation = Quaternion.Euler(xRotation, -30.0f, 0.0f);
            dayNightImg.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -zImgRotation);
            yield return null;
        }

        // Ensure exact final rotation
        directionalLight.transform.rotation = Quaternion.Euler(maxRotationX, -30.0f, 0.0f);
        dayNightImg.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -maxRotationX);
    }
}

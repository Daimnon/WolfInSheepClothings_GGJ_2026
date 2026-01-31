using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _cameraToDisable;
    [SerializeField] private GameObject _gameTitle;
    [SerializeField] private GameObject _pressAnyKey;
    [SerializeField] private float _fadeDuration = 1.0f;
    [SerializeField] private float _lensDistortionIntensity = 0.446f;

    private LensDistortion _lensDistortion;

    public static Action FinishedCameraPan;

    private void Awake()
    {
        var volumeSettings = _cameraToDisable.GetComponent<CinemachineVolumeSettings>();
        if (volumeSettings == null)
        {
            Debug.LogError("CinemachineVolumeSettings missing");
            return;
        }

        volumeSettings.Profile.TryGet(out _lensDistortion);
    }

    private IEnumerator FadeOutRoutine()
    {
        float start = _lensDistortion.intensity.value;
        float time = 0f;

        while (time < _fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / _fadeDuration;

            _lensDistortion.intensity.value =
                Mathf.Lerp(start, 0f, Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }

        _lensDistortion.intensity.value = 0f;
        _lensDistortion.active = false; // optional

        gameObject.SetActive(false);
    }
    public void FadeOutDistortion()
    {
        if (_lensDistortion == null)
            return;

        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float start = _lensDistortion.intensity.value;
        float time = 0f;

        _lensDistortion.active = true;

        while (time < _fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / _fadeDuration;
            _lensDistortion.intensity.value = Mathf.Lerp(start, _lensDistortionIntensity, t);
            yield return null;
        }

        _lensDistortion.intensity.value = _lensDistortionIntensity;
        
        FinishedCameraPan?.Invoke();
    }
    public void FadeInDistortion()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInRoutine());
    }

    public void TransitionToGame()
    {
        _cameraToDisable.SetActive(false);
        _gameTitle.SetActive(false);
        _pressAnyKey.SetActive(false);
        FadeOutDistortion();
    }

    private void OnDestroy()
    {
        _lensDistortion.active = true;
        _lensDistortion.intensity.value = _lensDistortionIntensity;
    }
}

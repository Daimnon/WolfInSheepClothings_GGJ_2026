using UnityEngine;
using UnityEngine.UI;

public class UI_Fill : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private ShepherdAI _shepherd;

    [Range(0f, 100f)]
    [SerializeField] private float _percentage;

    private void Update()
    {
        _percentage = _shepherd.AggroMeter;
    }
    private void LateUpdate()
    {
        SetPercentage(_percentage);
    }

    public void SetPercentage(float percentage)
    {
        float normalized = Mathf.Clamp01(percentage / 100f);
        _fillImage.fillAmount = normalized;
    }
}

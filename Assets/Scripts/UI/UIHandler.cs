using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject _mainMenuCanvas;
    [SerializeField] private GameObject _gameplayCanvas;
    [SerializeField] private GameObject _pauseMenuCanvas;
    [SerializeField] private GameObject _gameoverCanvase;
    [SerializeField] private GameObject _nextDayCanvas;

    private void OnEnable()
    {
        GameManager.OnGameTimerEnd += OnGameTimerEnd;
    }
    private void OnDisable()
    {
        GameManager.OnGameTimerEnd -= OnGameTimerEnd;
    }

    public void CloseAllCanvases()
    {
        _mainMenuCanvas.SetActive(false);
        _gameplayCanvas.SetActive(false);
        _pauseMenuCanvas.SetActive(false);
        _gameoverCanvase.SetActive(false);
        _nextDayCanvas.SetActive(false);
    }

    public void OpenPauseCanvas()
    {
        CloseAllCanvases();
        _pauseMenuCanvas.SetActive(true);
    }
    public void OpenGameoverCanvas()
    {
        CloseAllCanvases();
        _mainMenuCanvas.SetActive(true);
    }
    public void OpenNextDayCanvas()
    {
        CloseAllCanvases();
        _nextDayCanvas.SetActive(true);
    }

    #region Events
    private void OnGameTimerEnd()
    {
        _nextDayCanvas.SetActive(true);
    }
    #endregion
}

using UnityEngine;

public class NextDayUI : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void RestartLevel()
    {
        CustomSceneManager.ReloadFirstScene();
    }
}

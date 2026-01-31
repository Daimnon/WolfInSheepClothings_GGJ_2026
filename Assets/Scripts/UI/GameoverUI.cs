using UnityEngine;

public class GameoverUI : MonoBehaviour
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

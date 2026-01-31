using UnityEngine;

public class GameoverUI : MonoBehaviour
{
    public void RestartLevel()
    {
        CustomSceneManager.ReloadFirstScene();
    }
}

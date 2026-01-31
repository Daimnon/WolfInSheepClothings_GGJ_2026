using UnityEngine;
using UnityEngine.SceneManagement;

public static class CustomSceneManager
{
    public static void ReloadFirstScene()
    {
        SceneManager.LoadScene(0);
    }
}

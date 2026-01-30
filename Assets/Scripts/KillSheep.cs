using UnityEngine;

public class KillSheep : MonoBehaviour
{
    public Sheep[] sheep;

    public void KillRandomSheep()
    {
        Debug.Log("KillRandomSheep");
        int randomInt = Random.Range(0, sheep.Length);
        sheep[randomInt].GotShot();
    }
}

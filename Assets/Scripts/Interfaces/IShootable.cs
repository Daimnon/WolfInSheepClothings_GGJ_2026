using UnityEngine;

public interface IShootable
{
    public GameObject GetGameObj();
    public void GotShot();
    public ShootableType GetShootableType();
    public bool CanBeTargeted { get; }
}

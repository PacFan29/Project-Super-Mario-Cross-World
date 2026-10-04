using UnityEngine;

public class RollBloom : WeaponMovements
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        combo = 0;
    }

    void Update()
    {
        velocity = Vector3.zero;
    }
}

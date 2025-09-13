using System;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public Action<bool> OnGround;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            OnGround?.Invoke(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            OnGround?.Invoke(false);
        }
    }
}

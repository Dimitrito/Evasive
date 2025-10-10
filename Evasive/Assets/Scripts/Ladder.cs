using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Ladder : MonoBehaviour
{
    public float climbSpeed = 3f; // скорость подъема по лестнице

    private void Awake()
    {
        // ”бедимс€, что коллайдер Ч триггер
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerMovement>();
        if (player != null)
            player.SetOnLadder(true, climbSpeed);
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerMovement>();
        if (player != null)
            player.SetOnLadder(false, 0);
    }
}

using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Vector3 lastCheckpoint;

    void Start()
    {
        lastCheckpoint = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lastCheckpoint = transform.position;
            Debug.Log("Checkpoint Saved!");
        }
    }
}

using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public string npcKey;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is near");
        }
    }
}

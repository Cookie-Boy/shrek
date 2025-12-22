using UnityEngine;

public class Fiona_haunting_behaviour : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) 
            return;
        Transform root = other.transform.Find("ShrekModel");
        this.transform.SetParent(root);
    }
}

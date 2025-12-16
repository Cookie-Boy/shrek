using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MushroomJump : MonoBehaviour
{
    [Header("Food rotation speed")]
    [SerializeField] private int repelForce;

    private HashSet<GameObject> bouncedObjects = new HashSet<GameObject>();

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject go in bouncedObjects.ToList()) {
            if(go == null) {
                bouncedObjects.Remove(go);
                continue;
            }
            if (bouncedObjects.Contains(go) && intersect(go))
            {
                bouncedObjects.Remove(go);
                Debug.Log($"Сбросил флаг для: {go.name}");
            }
        }
    }

    bool intersect(GameObject go)
    {
        Collider goCol = go.GetComponent<Collider>();
        Collider thisCol = GetComponent<Collider>();
        
        return goCol.bounds.Intersects(thisCol.bounds);
    }

    void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !bouncedObjects.Contains(other.gameObject))
        {
            Vector3 force = Vector3.up * repelForce;
            Vector3 currentVelocity = rb.linearVelocity;
            currentVelocity.y = 0;
            rb.linearVelocity = currentVelocity;
            rb.AddForce(force, ForceMode.Impulse);
            bouncedObjects.Add(other.gameObject);
            Debug.Log($"Оттолкнул {other.name} с силой {repelForce}");
        } else Debug.Log("No rigit bod found");
    }
}

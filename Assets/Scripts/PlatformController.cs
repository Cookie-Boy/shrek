using System.Numerics;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [Header("Move settings")]

    [SerializeField] private int movementSpeed;

    [SerializeField] private Vector3Int axisMask = new Vector3Int(1, 0, 1);

    [SerializeField] private double travelDistance = 10;
    [SerializeField] private double destinationStop = 1;
    [SerializeField] private bool isStatic = false;

    private double traveled = 0;

    private double stopTime = 0;
    private bool pause = true;

    void Start()
    {
        axisMask.x = axisMask.x == 0? 0: axisMask.x < 0? -1 : 1;
        axisMask.y = axisMask.y == 0? 0: axisMask.y < 0? -1 : 1;
        axisMask.z = axisMask.z == 0? 0: axisMask.z < 0? -1 : 1;
    }

    void OnTriggerEnter(Collider other)
    {
        isStatic = false;
        Transform root = other.transform.root;
        root.transform.SetParent(transform);
    }
    
    void OnTriggerExit(Collider other)
    {
        foreach (Transform child in transform)
        {
            if (other.transform.IsChildOf(child))
            {
                child.SetParent(null);
                Debug.Log($"Убрали {child.name} из детей");
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isStatic)
        {
            return;
        }

        if (pause)
        {
            stopTime += Time.deltaTime;

            if(stopTime >= destinationStop)
            {
                stopTime = 0;
                pause = false;
            }

            return;
        }
        
        float speedNorm = movementSpeed * Time.deltaTime;
        traveled += speedNorm;
        if(traveled >= travelDistance)
        {
            traveled = 0;
            axisMask = axisMask * -1;
            pause = true;
        }
        UnityEngine.Vector3 vector = new UnityEngine.Vector3(axisMask.x * speedNorm, axisMask.y * speedNorm, axisMask.z * speedNorm);
        transform.Translate(vector, Space.Self);
    }
}

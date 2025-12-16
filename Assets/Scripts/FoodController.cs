using UnityEngine;
using System;
public class FoodController : MonoBehaviour
{
    [Header("Food rotation speed")]
    [SerializeField] private int rotationSpeed;

    private System.Random random = new System.Random();

    void Start()
    {
        transform.Rotate(0, 0, random.Next(0, 360));
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}

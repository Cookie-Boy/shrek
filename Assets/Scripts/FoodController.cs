using UnityEngine;
using System;
public class FoodController : MonoBehaviour
{
    [Header("Food rotation speed")]
    [SerializeField] private int rotationSpeed;

    private System.Random random = new System.Random();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.Rotate(0, 0, random.Next(0, 360));
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}

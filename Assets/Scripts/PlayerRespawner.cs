using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRespawner : MonoBehaviour
{
    [Header("Настройки телепортации")]
    [SerializeField] private Key teleportKey = Key.H;
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private float teleportCooldown = 1f;
    
    private float lastTeleportTime;
    
    void Start()
    {
        if (startPosition == Vector3.zero)
        {
            startPosition = transform.position;
        }
        
        lastTeleportTime = -teleportCooldown;
    }
    
    void Update()
    {
        HandleTeleportInput();
    }
    
    void HandleTeleportInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[teleportKey].wasPressedThisFrame)
        {
            float timeSinceLastTeleport = Time.time - lastTeleportTime;
            
            if (timeSinceLastTeleport >= teleportCooldown)
            {
                TeleportToStartPosition();
                lastTeleportTime = Time.time;
            }
        }
    }
    
    void TeleportToStartPosition()
    {
        transform.position = startPosition;
        Debug.Log($"Игрок телепортирован в позицию: {startPosition}");
    }
}
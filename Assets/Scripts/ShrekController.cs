using UnityEngine;
using UnityEngine.InputSystem;

public class ShrekController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 35f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float rotationSpeed = 4f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    private bool isRunning = false;
    private bool isJumping = false;
    private bool isOnGround = true;

    // Для хранения ввода движения
    private Vector2 moveInput = Vector2.zero;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
        }

        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found!");
        }

        // Если не задана точка проверки земли, используем центр объекта
        if (groundCheckPoint == null)
        {
            groundCheckPoint = transform;
        }
    }

    void Update()
    {
        // Проверка земли
        CheckGround();

        // Обработка ввода клавиатуры
        HandleKeyboardInput();

        // Обновление анимаций
        UpdateAnimations();

        // Движение
        MoveCharacter();

        Debug.Log("isRunning: " + isRunning + " isJumping: " + isJumping + " isOnGround: " + isOnGround);
    }

    void CheckGround()
    {
        // Используем Raycast для проверки земли под ногами
        RaycastHit hit;
        Vector3 rayStart = groundCheckPoint.position;
        Vector3 rayDirection = Vector3.down;

        // Учитываем размер персонажа
        float checkDistance = groundCheckDistance;
        if (GetComponent<Collider>() != null)
        {
            checkDistance += GetComponent<Collider>().bounds.extents.y;
        }

        // Бросаем луч вниз для проверки земли
        bool hitGround = Physics.Raycast(rayStart, rayDirection, out hit, checkDistance, groundLayer);

        // Визуализация луча в редакторе (только в Play mode)
        Debug.DrawRay(rayStart, rayDirection * checkDistance, hitGround ? Color.green : Color.red);

        isOnGround = hitGround;

        // Если мы падаем и касаемся земли, сбрасываем прыжок
        if (isOnGround && rb.linearVelocity.y <= 0.1f)
        {
            isJumping = false;
        }
    }

    void HandleKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            // Собираем ввод движения в вектор
            moveInput = Vector2.zero;

            // Стрелочки
            if (keyboard.upArrowKey.isPressed) moveInput.y += 1;
            if (keyboard.downArrowKey.isPressed) moveInput.y -= 1;
            if (keyboard.leftArrowKey.isPressed) moveInput.x -= 1;
            if (keyboard.rightArrowKey.isPressed) moveInput.x += 1;

            // WASD (имеет приоритет)
            if (keyboard.wKey.isPressed) moveInput.y += 1;
            if (keyboard.sKey.isPressed) moveInput.y -= 1;
            if (keyboard.aKey.isPressed) moveInput.x -= 1;
            if (keyboard.dKey.isPressed) moveInput.x += 1;

            // Нормализуем вектор, если длина > 1 (диагональное движение)
            if (moveInput.magnitude > 1f)
            {
                moveInput.Normalize();
            }

            // Проверяем, нужно ли бежать (есть ли ввод движения)
            bool shouldRun = moveInput.magnitude > 0.1f;

            // Обработка прыжка
            if (keyboard.spaceKey.wasPressedThisFrame && isOnGround && !isJumping)
            {
                Jump();
            }

            // Обновляем состояние бега
            if (shouldRun != isRunning)
            {
                isRunning = shouldRun;
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isJumping", isJumping);
        }
    }

void MoveCharacter()
{
    // 1. ПОВОРОТ (A/D) - работает всегда
    if (Mathf.Abs(moveInput.x) > 0.1f)
    {
        float turnAmount = moveInput.x * rotationSpeed * Time.deltaTime * 100f;
        transform.Rotate(0, turnAmount, 0);
    }
    
    // 2. ДВИЖЕНИЕ ВПЕРЁД/НАЗАД (W/S) - относительно взгляда
    if (Mathf.Abs(moveInput.y) > 0.1f)
    {
        // forward = куда смотрит, backward = против взгляда
        Vector3 moveDirection = transform.forward * -moveInput.y;
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
        
        transform.Translate(movement, Space.World);
    }
    
    // 3. isRunning уже установлен в HandleKeyboardInput()
    // Анимация обновится в UpdateAnimations()
}
    void Jump()
    {
        if (isOnGround && !isJumping)
        {
            // Применяем силу прыжка
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;

            // Обновляем анимацию немедленно
            if (animator != null)
            {
                animator.SetBool("isJumping", true);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isOnGround ? Color.green : Color.red;
            Gizmos.DrawSphere(groundCheckPoint.position, 0.1f);
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }
    }
}
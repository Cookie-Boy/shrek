using UnityEngine;
using UnityEngine.InputSystem;

public class ShrekController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.2f;

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
        if (moveInput.magnitude > 0.1f)
        {
            // Создаем вектор движения
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

            // Преобразуем локальное направление в мировые координаты
            movement = transform.TransformDirection(movement);

            // Двигаем Rigidbody
            Vector3 targetVelocity = movement * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y; // Сохраняем вертикальную скорость для прыжка/падения

            // Плавное изменение скорости
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * 10f);

            // Поворачиваем персонажа в сторону движения
            if (movement.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movement.x, 0, movement.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else
        {
            // Если нет ввода, постепенно останавливаем горизонтальное движение
            Vector3 currentVelocity = rb.linearVelocity;
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, 0, Time.deltaTime * 10f);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, 0, Time.deltaTime * 10f);
            rb.linearVelocity = currentVelocity;
        }
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

    // Опционально: для более точного контроля можно использовать FixedUpdate для физики
    void FixedUpdate()
    {
        // Дополнительная обработка физики может быть здесь
    }

    // Визуализация точки проверки земли в редакторе
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
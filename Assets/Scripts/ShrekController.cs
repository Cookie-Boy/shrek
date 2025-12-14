using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class ShrekController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI foodEatenText;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 35f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float rotationSpeed = 4f;

    [Header("Jump Settings - ДЛЯ РЕЗКОГО ПРЫЖКА")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float jumpCutMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("Item Collection - Система сбора")]
    [SerializeField] private float collectionRadius = 2f; // Радиус сбора
    [SerializeField] private string itemTag = "Food"; // Тег собираемых предметов
    [SerializeField] private AudioSource audioSource; // Для звуков
    [SerializeField] private AudioClip eatSound; // Звук поедания
    [SerializeField] private float eatSoundVolume = 0.7f; // Громкость звука

    private bool isRunning = false;
    private bool isJumping = false;
    private bool isOnGround = true;
    private bool isJumpKeyHeld = false;

    private int foodEaten = 0;

    // Для хранения ввода движения
    private Vector2 moveInput = Vector2.zero;

    // Список для избежания повторного сбора одного объекта
    private HashSet<GameObject> collectedItems = new HashSet<GameObject>();

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

        // Инициализация AudioSource если нет
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D звук
            audioSource.volume = eatSoundVolume;
        }

        // Настройка Rigidbody для резких движений
        SetupRigidbodyForSharpJump();

        // Если не задана точка проверки земли, используем центр объекта
        if (groundCheckPoint == null)
        {
            groundCheckPoint = transform;
        }

        // Обновляем UI
        UpdateUI();
    }

    void SetupRigidbodyForSharpJump()
    {
        rb.mass = 2f;               // Меньшая масса для более резкого отклика
        rb.linearDamping = 0.5f;             // Меньшее сопротивление воздуха
        rb.angularDamping = 0.5f;      // Меньшее сопротивление вращению
        rb.useGravity = true;       // Включаем гравитацию
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Проверка земли
        CheckGround();

        // Обработка ввода клавиатуры
        HandleKeyboardInput();

        // Проверка сбора предметов (непрерывно во время движения)
        CheckForItems();

        // Обновление анимаций
        UpdateAnimations();

        // Движение
        MoveCharacter();

        // Применяем дополнительную гравитацию для резкого падения
        ApplyJumpPhysics();
    }

    void FixedUpdate()
    {
        // Физика прыжка в FixedUpdate для стабильности
        ApplyJumpPhysics();
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
            animator.SetBool("isJumping", false);
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

            // Отслеживаем удержание кнопки прыжка
            isJumpKeyHeld = keyboard.spaceKey.isPressed;

            // Обработка прыжка (нажатие)
            if (keyboard.spaceKey.wasPressedThisFrame && isOnGround && !isJumping)
            {
                Jump();
            }

            Debug.Log($"Jump?? {isJumping}, ground? {isOnGround}, space??? {keyboard.spaceKey.wasPressedThisFrame}");

            // Обновляем состояние бега
            if (shouldRun != isRunning)
            {
                isRunning = shouldRun;
            }
        }
    }

    void ApplyJumpPhysics()
    {
        if (!isOnGround)
        {
            Vector3 velocity = rb.linearVelocity;

            // УСИЛЕННАЯ ГРАВИТАЦИЯ ПРИ ПАДЕНИИ - для быстрого спуска
            if (velocity.y < 0)
            {
                velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            // КОРОТКИЕ ПРЫЖКИ - если отпустили кнопку раньше
            else if (velocity.y > 0 && !isJumpKeyHeld)
            {
                velocity += Vector3.up * Physics.gravity.y * (jumpCutMultiplier - 1) * Time.deltaTime;
            }

            rb.linearVelocity = velocity;
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

        if (Mathf.Abs(moveInput.y) > 0.1f)
        {
            Vector3 moveDirection = transform.forward * -moveInput.y;
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x * 0.9f, 
                rb.linearVelocity.y, 
                rb.linearVelocity.z * 0.9f
            );
        }
    }

    void Jump()
    {
        if (isOnGround)
        {
            // ПЕРВЫЙ ВАРИАНТ: VelocityChange (самый резкий)
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0; // Сбрасываем вертикальную скорость
            rb.linearVelocity = velocity;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            isJumping = true;

            // Обновляем анимацию немедленно
            if (animator != null)
            {
                animator.SetBool("isJumping", true);
            }

            Debug.Log($"Jump! Force: {jumpForce}, Velocity: {rb.linearVelocity.y}");
        }
    }

    void CheckForItems()
    {
        // Ищем все предметы с нужным тегом в радиусе
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, collectionRadius);

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(itemTag))
            {
                GameObject item = collider.gameObject;

                // Проверяем, не собирали ли уже этот предмет
                if (!collectedItems.Contains(item))
                {
                    // Нашли крысу/предмет - мгновенно "съедаем"
                    collectedItems.Add(item);
                    EatItem(item);
                }
            }
        }
    }

    void EatItem(GameObject item)
    {
        // Мгновенное поедание без остановки движения

        // 1. Проигрываем звук (если есть)
        if (eatSound != null && audioSource != null)
        {
            // Воспроизводим звук без прерывания текущих действий
            audioSource.PlayOneShot(eatSound, eatSoundVolume);
        }

        // 2. Уничтожаем предмет (крысу) с небольшой задержкой для визуального эффекта
        StartCoroutine(DestroyWithDelay(item, 0.05f));

        // 3. Обновляем счёт
        foodEaten++;
        UpdateUI();

        // 4. Выводим в консоль (опционально)
        Debug.Log($"Ate some food! Total: {foodEaten}");
    }

    System.Collections.IEnumerator DestroyWithDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Убираем из списка собранных перед уничтожением
        if (collectedItems.Contains(obj))
        {
            collectedItems.Remove(obj);
        }

        Destroy(obj);
    }

    void UpdateUI()
    {
        if (foodEatenText != null)
        {
            foodEatenText.text = $"Eaten: {foodEaten}";
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Быстрая проверка приземления
        if (collision.contacts.Length > 0 && rb.linearVelocity.y <= 0.1f)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Альтернативный способ сбора через триггер (если нужно)
        if (other.CompareTag(itemTag) && !collectedItems.Contains(other.gameObject))
        {
            collectedItems.Add(other.gameObject);
            EatItem(other.gameObject);
        }
    }

    // Очищаем список собранных предметов при выходе за пределы сцены (на всякий случай)
    void OnDestroy()
    {
        collectedItems.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isOnGround ? Color.green : Color.red;
            Gizmos.DrawSphere(groundCheckPoint.position, 0.1f);
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }

        // Рисуем радиус сбора
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRadius);
    }
}
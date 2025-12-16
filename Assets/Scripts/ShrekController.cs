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

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float jumpCutMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;

    [Header("Item Collection")]
    [SerializeField] private float collectionRadius = 2f;
    [SerializeField] private string itemTag = "Food";
    private int maxItemCount = 20;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip eatSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private float eatSoundVolume = 0.7f;
    [SerializeField] private float jumpSoundVolume = 0.7f;

    private bool isRunning = false;
    private bool isJumping = false;
    private bool isOnGround = true;
    private bool isJumpKeyHeld = false;
    private bool isTeleportAvailable = false;

    public bool IsTeleportAvailable
    {
        get { return isTeleportAvailable; }
        set { isTeleportAvailable = value; }
    }

    private int foodEaten = 0;

    private Vector2 moveInput = Vector2.zero;

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

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D звук
            audioSource.volume = eatSoundVolume;
        }

        SetupRigidbodyForSharpJump();

        if (groundCheckPoint == null)
        {
            groundCheckPoint = transform;
        }

        maxItemCount = GetTotalFoodCount();
        UpdateUI();
    }

    void SetupRigidbodyForSharpJump()
    {
        rb.mass = 2f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        CheckGround();
        HandleKeyboardInput();
        CheckForItems();
        UpdateAnimations();
        MoveCharacter();
        ApplyJumpPhysics();
    }

    void FixedUpdate()
    {
        ApplyJumpPhysics();
    }

    void CheckGround()
    {
        RaycastHit hit;
        Vector3 rayStart = groundCheckPoint.position;
        Vector3 rayDirection = Vector3.down;

        float checkDistance = groundCheckDistance;
        if (GetComponent<Collider>() != null)
        {
            checkDistance += GetComponent<Collider>().bounds.extents.y;
        }

        bool hitGround = Physics.Raycast(rayStart, rayDirection, out hit, checkDistance, groundLayer);

        // Визуализация луча в редакторе (только в Play mode)
        Debug.DrawRay(rayStart, rayDirection * checkDistance, hitGround ? Color.green : Color.red);

        isOnGround = hitGround;

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
            moveInput = Vector2.zero;

            if (keyboard.upArrowKey.isPressed) moveInput.y += 1;
            if (keyboard.downArrowKey.isPressed) moveInput.y -= 1;
            if (keyboard.leftArrowKey.isPressed) moveInput.x -= 1;
            if (keyboard.rightArrowKey.isPressed) moveInput.x += 1;

            if (keyboard.wKey.isPressed) moveInput.y += 1;
            if (keyboard.sKey.isPressed) moveInput.y -= 1;
            if (keyboard.aKey.isPressed) moveInput.x -= 1;
            if (keyboard.dKey.isPressed) moveInput.x += 1;

            if (moveInput.magnitude > 1f)
            {
                moveInput.Normalize();
            }

            bool shouldRun = moveInput.magnitude > 0.1f;

            isJumpKeyHeld = keyboard.spaceKey.isPressed;

            if (keyboard.spaceKey.wasPressedThisFrame && isOnGround && !isJumping)
            {
                Jump();
            }

            //Debug.Log($"Jump?? {isJumping}, ground? {isOnGround}, space??? {keyboard.spaceKey.wasPressedThisFrame}");

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

            if (velocity.y < 0)
            {
                velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
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
            PlayJumpSound();

            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0; // Сбрасываем вертикальную скорость
            rb.linearVelocity = velocity;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            isJumping = true;

            if (animator != null)
            {
                animator.SetBool("isJumping", true);
            }

            Debug.Log($"Jump! Force: {jumpForce}, Velocity: {rb.linearVelocity.y}");
        }
    }

    void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpSoundVolume);
        }
        else if (jumpSound == null)
        {
            Debug.LogWarning("Jump sound is not assigned!");
        }
    }

    private int GetTotalFoodCount()
    {
        return GameObject.FindGameObjectsWithTag(itemTag).Length;
    }

    void CheckForItems()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, collectionRadius);

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(itemTag))
            {
                GameObject item = collider.gameObject;

                if (!collectedItems.Contains(item))
                {
                    collectedItems.Add(item);
                    EatItem(item);
                }
            }
        }
    }

    void EatItem(GameObject item)
    {
        PlayEatSound();
        StartCoroutine(DestroyWithDelay(item, 0.05f));
        foodEaten++;
        UpdateUI();
        Debug.Log($"Ate some food! Total: {foodEaten}/{maxItemCount}");

        if (foodEaten >= maxItemCount)
        {
            Debug.Log("All food collected!");
            isTeleportAvailable = true;
        }
    }

    void PlayEatSound()
    {
        if (eatSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(eatSound, eatSoundVolume);
        }
        else if (eatSound == null)
        {
            Debug.LogWarning("Eat sound is not assigned!");
        }
    }

    System.Collections.IEnumerator DestroyWithDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

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
            foodEatenText.text = $"Eaten: {foodEaten}/{maxItemCount}";
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0 && rb.linearVelocity.y <= 0.1f)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Альтернативный способ сбора через триггер
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRadius);
    }
}
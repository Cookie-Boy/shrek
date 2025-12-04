using UnityEngine;
using UnityEngine.InputSystem; // Добавьте эту строку

[RequireComponent(typeof(Rigidbody))]
public class ShrekController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    
    [Header("References")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Input Actions")]
    [SerializeField] private InputAction movementAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction runAction;
    
    private Rigidbody rb;
    private Vector3 movement;
    private bool isGrounded;
    public Animator animator;
    private float currentSpeed;
    private bool isRunning;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (modelTransform == null)
            modelTransform = transform.Find("Model") ?? transform;
            
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        animator.SetBool("isRunning", false);

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        groundLayer = LayerMask.GetMask("Ground");
        currentSpeed = walkSpeed;
        
        // Включаем Input Actions
        movementAction.Enable();
        jumpAction.Enable();
        runAction.Enable();
    }
    
    void OnDestroy()
    {
        // Отключаем Input Actions при уничтожении объекта
        movementAction.Disable();
        jumpAction.Disable();
        runAction.Disable();
    }
    
    void Update()
    {
        // Проверка земли
        isGrounded = Physics.CheckSphere(
            groundCheck.position, 
            groundCheckDistance, 
            groundLayer
        );
        
        // Получаем ввод из нового Input System
        Vector2 moveInput = movementAction.ReadValue<Vector2>();

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            animator.SetBool("isRunning", true);
            Debug.Log("Key down");
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            Debug.Log("Key up");
            animator.SetBool("isRunning", false);
        }

        //// Проверка бега
        //isRunning = runAction.IsPressed();
        //currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        //// Движение
        //movement = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        
        //// Прыжок
        //if (jumpAction.triggered && isGrounded)
        //{
        //    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        //    Debug.Log("Прыжок!");
        //}
        
        //// Обновление анимаций
        //UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        if (movement.magnitude >= 0.1f)
        {
            // Движение
            Vector3 moveDirection = transform.TransformDirection(movement);
            rb.MovePosition(rb.position + moveDirection * currentSpeed * Time.fixedDeltaTime);
            
            // Поворот модели в сторону движения
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            modelTransform.rotation = Quaternion.Slerp(
                modelTransform.rotation, 
                targetRotation, 
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        bool isMoving = movement.magnitude >= 0.1f;
        
        // Используем правильные имена параметров
        animator.SetBool("IsWalking", isMoving && !isRunning);
        animator.SetBool("isRunning", isMoving && isRunning);
        
        // Для отладки
        Debug.Log($"Anim - Moving: {isMoving}, Running: {isRunning}");
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

namespace SafetyTraining.Player
{
    /// <summary>
    /// Professional, robust First Person Controller optimized for Unity 6.
    /// Hooks directly into the project-wide InputSystem.actions for seamless WASD/Look.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 4.0f;
        [SerializeField] private float sprintSpeed = 7.0f;
        [SerializeField] private float gravity = -15.0f;
        [SerializeField] private float jumpHeight = 1.2f;

        [Header("Camera Settings")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float upperLookLimit = 80.0f;
        [SerializeField] private float lowerLookLimit = -80.0f;

        private CharacterController characterController;
        private Vector3 velocity;
        private float verticalRotation = 0f;
        private bool isGrounded;

        // Input System Actions (Cached)
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            // Lock cursor for FPS immersion
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Cache actions from the project-wide Input System
            if (InputSystem.actions != null)
            {
                moveAction = InputSystem.actions.FindAction("Move");
                lookAction = InputSystem.actions.FindAction("Look");
                jumpAction = InputSystem.actions.FindAction("Jump");
                sprintAction = InputSystem.actions.FindAction("Sprint");
            }
            else
            {
                Debug.LogError("[FirstPersonController] Project-wide Input Actions asset is missing!");
            }

            // Fallback camera attachment if not specified
            if (cameraTransform == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    cameraTransform = mainCam.transform;
                    // Position camera at head height if it's currently a child
                    if (cameraTransform.parent == transform)
                    {
                        cameraTransform.localPosition = new Vector3(0f, 0.8f, 0f);
                    }
                }
            }
        }

        private void Update()
        {
            HandleRotation();
            HandleMovement();
        }

        private void HandleRotation()
        {
            if (lookAction == null || cameraTransform == null) return;

            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            // Calculate rotation values
            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity;

            // Yaw (horizontal) rotation of player body
            transform.Rotate(Vector3.up * mouseX);

            // Pitch (vertical) rotation of head/camera (clamped)
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, lowerLookLimit, upperLookLimit);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        private void HandleMovement()
        {
            if (moveAction == null) return;

            // Ground check
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Slight downward force to snap to ground
            }

            // Read WASD movement input
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

            // Determine speed
            bool isSprinting = sprintAction != null && sprintAction.IsPressed();
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Apply movement
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

            // Handle jumping
            if (jumpAction != null && jumpAction.WasPressedThisFrame() && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void OnDisable()
        {
            // Restore cursor when disabled or leaving scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

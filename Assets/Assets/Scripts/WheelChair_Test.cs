using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;

public class WheelChair_Test : MonoBehaviour
{
    [Header("Input (drag these in)")]
    public InputActionReference leftWheelAction;
    public InputActionReference rightWheelAction;
    public InputActionReference scrollAction;
    public InputActionReference cameraAction;
    public InputActionReference interactAction;
    public InputActionReference pickupAction;

    [Header("Jumpscare")]
    public InputActionReference jumpscareAction;
    public AudioSource jumpscareAudio;

    [Header("Movement Settings")]
    public float wheelForce = 200f;
    public float turnForce = 5f;
    public float maxSpeed = 5f;
    public float maxTurnSpeed = 2f;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float cameraFollowSmooth = 5f;
    public Vector3 cameraOffset = new Vector3(0f, 1.5f, 0f);

    private Rigidbody rb;
    private bool leftWheelActive;
    private bool rightWheelActive;

    [Header("Camera Rotation Settings")]
    public FloatData mouseSensitivityData;
    public float mouseSensitivity = 10f;
    private float xRotation;
    private float yRotation;
    [SerializeField] private float minVertical = -30f;
    [SerializeField] private float maxVertical = 30f;
    [SerializeField] private float maxHorizontal = 60f;

    public Camera mainCamera;

    [Header("UI Prompt")]
    public TextMeshProUGUI tmpText;

    [Header("Raycast Settings")]
    public float detectionRange = 0f;

    private int soundCounter = 0; // ---------------- sound test only --------------
    public int numberoftimesToInvoke = 6; // ---------------- sound test only --------------
    public UnityEvent OnmoveEvent;

    [Header("Throwing / Holding Settings")]
    public float throwForce = 2f;
    public Transform holdPoint;
    public float holdDistance = 2f;
    public float followSpeed = 10f;

    private Rigidbody heldObject;
    private bool isHolding = false;

    public UnityEvent itemthrowon;
    public UnityEvent itemthrowoff;

    private GameObject lastLookAtObject;

    public void SetmouseSensitivity()
    {
        if (mouseSensitivityData != null)
            mouseSensitivity = mouseSensitivityData.value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (tmpText) tmpText.text = "";

        // Set once at start; call SetmouseSensitivity() from your Apply button/UI when needed.
        SetmouseSensitivity();
    }

    private void OnEnable()
    {
        if (leftWheelAction) leftWheelAction.action.Enable();
        if (rightWheelAction) rightWheelAction.action.Enable();
        if (scrollAction) scrollAction.action.Enable();
        if (cameraAction) cameraAction.action.Enable();
        if (interactAction) interactAction.action.Enable();
        if (pickupAction) pickupAction.action.Enable();
        if (jumpscareAction) jumpscareAction.action.Enable();

        if (leftWheelAction)
        {
            leftWheelAction.action.performed += OnLeftPressed;
            leftWheelAction.action.canceled += OnLeftReleased;
        }
        if (rightWheelAction)
        {
            rightWheelAction.action.performed += OnRightPressed;
            rightWheelAction.action.canceled += OnRightReleased;
        }
        if (scrollAction)
        {
            scrollAction.action.performed += OnScroll;
        }

        rb.linearDamping = 1.5f;
        rb.angularDamping = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnDisable()
    {
        if (leftWheelAction)
        {
            leftWheelAction.action.performed -= OnLeftPressed;
            leftWheelAction.action.canceled -= OnLeftReleased;
            leftWheelAction.action.Disable();
        }
        if (rightWheelAction)
        {
            rightWheelAction.action.performed -= OnRightPressed;
            rightWheelAction.action.canceled -= OnRightReleased;
            rightWheelAction.action.Disable();
        }
        if (scrollAction)
        {
            scrollAction.action.performed -= OnScroll;
            scrollAction.action.Disable();
        }
        if (cameraAction) cameraAction.action.Disable();
        if (interactAction) interactAction.action.Disable();
        if (pickupAction) pickupAction.action.Disable();
        if (jumpscareAction) jumpscareAction.action.Disable();
    }

    private void Update()
    {
        HandleCameraLook();
        FollowCameraToChair();
        CameraDetection(); // keep in Update for input-based interactions

        // Jumpscare: press ` (or whatever binding you set) to play the audio
        if (jumpscareAction && jumpscareAction.action.WasPressedThisFrame())
        {
            if (jumpscareAudio) jumpscareAudio.Play();
        }
    }

    private void FixedUpdate()
    {
        if (isHolding && heldObject != null)
            MoveHeldObject();
    }

    // ---------------- Movement Logic ----------------
    private void OnLeftPressed(InputAction.CallbackContext ctx) => leftWheelActive = true;
    private void OnLeftReleased(InputAction.CallbackContext ctx) => leftWheelActive = false;
    private void OnRightPressed(InputAction.CallbackContext ctx) => rightWheelActive = true;
    private void OnRightReleased(InputAction.CallbackContext ctx) => rightWheelActive = false;

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        Vector2 delta = ctx.ReadValue<Vector2>();
        float scrollY = delta.y;
        if (Mathf.Abs(scrollY) < 0.01f) return;

        ApplyWheelPush(scrollY);
    }

    private void ApplyWheelPush(float scroll)
    {
        soundCounter++;

        if (rb.linearVelocity.magnitude > maxSpeed && leftWheelActive && rightWheelActive)
            return;

        Vector3 fwd = transform.forward;

        if (leftWheelActive && rightWheelActive)
        {
            rb.AddForce(fwd * scroll * wheelForce, ForceMode.Force);
            TryInvokeMoveEvent();
        }
        else if (leftWheelActive)
        {
            if (Mathf.Abs(rb.angularVelocity.y) < maxTurnSpeed)
                rb.AddTorque(Vector3.up * scroll * turnForce, ForceMode.Force);

            TryInvokeMoveEvent();
        }
        else if (rightWheelActive)
        {
            if (Mathf.Abs(rb.angularVelocity.y) < maxTurnSpeed)
                rb.AddTorque(Vector3.up * -scroll * turnForce, ForceMode.Force);

            TryInvokeMoveEvent();
        }
    }

    private void TryInvokeMoveEvent()
    {
        if (soundCounter >= numberoftimesToInvoke)
        {
            OnmoveEvent.Invoke();
            soundCounter = 0;
        }
    }

    // ---------------- Camera Control ----------------
    private void HandleCameraLook()
    {
        if (!cameraAction || !mainCamera) return;

        Vector2 lookInput = cameraAction.action.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVertical, maxVertical);
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -maxHorizontal, maxHorizontal);

        Quaternion camRot = Quaternion.Euler(xRotation, transform.eulerAngles.y + yRotation, 0f);
        mainCamera.transform.rotation = camRot;
    }

    private void FollowCameraToChair()
    {
        if (!playerCamera) return;

        Vector3 targetPos = transform.position + cameraOffset;
        playerCamera.position = Vector3.Lerp(
            playerCamera.position, targetPos, Time.deltaTime * cameraFollowSmooth
        );
    }

    // ---------------- Detection (Raycast) ----------------
    private void CameraDetection()
    {
        if (!mainCamera) return;

        Vector3 fwd = mainCamera.transform.forward;

        // If player presses pickup while already holding, drop immediately (no need to be looking at the object)
        if (pickupAction && pickupAction.action.WasPressedThisFrame() && isHolding && heldObject != null)
        {
            DropHeldObject();
            return;
        }

        if (Physics.Raycast(mainCamera.transform.position, fwd, out RaycastHit hit, detectionRange))
        {
            Debug.DrawRay(mainCamera.transform.position, fwd * hit.distance, Color.red);

            Interactable interactable = hit.collider.GetComponent<Interactable>();
            LookAT lookAt = hit.collider.GetComponent<LookAT>();

            // ===================== UI prompt =====================
            if (tmpText)
                tmpText.text = (interactable != null) ? "Interact\n   (E)" : "";

            // ===================== interact =====================
            if (interactable && interactAction && interactAction.action.WasPressedThisFrame())
            {
                Debug.DrawRay(mainCamera.transform.position, fwd * detectionRange, Color.blue);
                Debug.Log("Interacting with: " + hit.collider.name);
                interactable.Interact();
            }

            // ===================== look at =====================
            if (lookAt && hit.collider.CompareTag("Look-at"))
            {
                if (hit.collider.gameObject != lastLookAtObject)
                {
                    lookAt.LookedAt();
                    lastLookAtObject = hit.collider.gameObject;
                }
            }
            else
            {
                lastLookAtObject = null;
            }

            // ===================== pick up =====================
            if (pickupAction && pickupAction.action.WasPressedThisFrame() && !isHolding && hit.collider.CompareTag("Pick-Up"))
            {
                Rigidbody rbhit = hit.collider.GetComponent<Rigidbody>();
                if (rbhit != null)
                    PickUpObject(rbhit);
            }
        }
        else
        {
            Debug.DrawRay(mainCamera.transform.position, fwd * detectionRange, Color.green);
            lastLookAtObject = null;

            if (tmpText) tmpText.text = "";
        }
    }

    private void PickUpObject(Rigidbody target)
    {
        heldObject = target;
        isHolding = true;

        heldObject.useGravity = false;
        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;

        itemthrowon.Invoke();
    }

    private void DropHeldObject()
    {
        if (heldObject == null) return;

        heldObject.useGravity = true;

        // throw forward
        if (mainCamera)
            heldObject.AddForce(mainCamera.transform.forward * throwForce, ForceMode.Impulse);

        heldObject = null;
        isHolding = false;

        itemthrowoff.Invoke();
    }

    private void MoveHeldObject()
    {
        if (!heldObject || !holdPoint) return;

        Vector3 targetPos = holdPoint.position + holdPoint.forward * holdDistance;

        heldObject.MovePosition(
            Vector3.Lerp(heldObject.position, targetPos, Time.fixedDeltaTime * followSpeed)
        );
    }
}
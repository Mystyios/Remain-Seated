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

    [Header("Movement Settings")]
    public float wheelForce = 200f;
    public float turnForce = 5f;
    public float maxSpeed = 5f;
    public float maxTurnSpeed = 2f;

    [Header("Camera Settings")]
    public Transform playerCamera;       // your camera transform (child or separate)
    public float cameraFollowSmooth = 5f;
    public Vector3 cameraOffset = new Vector3(0f, 1.5f, 0f); // camera height offset above chair

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

    private float baseYaw; // used for initial camera alignment
    private float basePitch; // used for initial camera alignment
    public Camera mainCamera;
    
    [Header("Highlight Settings")]
    public Material highlightMaterial;   // assign in Inspector
    private MeshRenderer lastHighlightedRenderer;
    private Material[] originalMaterials;
    Canvas lastCanvas;
    private GameObject lastLookAtObject;
    public TextMeshProUGUI tmpText;
    
    [Header("Raycast Settings")]
    public float detectionRange = 0f;

    private int soundCounter = 0; // ---------------- sound test only --------------
    public int numberoftimesToInvoke = 6; // ---------------- sound test only --------------
    
    public UnityEvent OnmoveEvent;

    private bool isToggled = false;
    
    [Header("Throwing Settings")]
    public float throwForce = 2f;
    public GameObject parentObject;
    public Transform holdPoint;
    public float holdDistance = 2f;
    public float followSpeed = 10f;
    private Rigidbody heldObject;
    
    public void SetmouseSensitivity()
    {
        // we can add a contumacious check in the update if we need this to contumacious check but this should work with an apply button
        
        mouseSensitivity = mouseSensitivityData.value;
        
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // not used yet but will be when camera and player interaction is fully implemented so camera does not jump on start
        baseYaw = transform.eulerAngles.y;
        basePitch = transform.eulerAngles.x;
        
        tmpText.text = "";
        
        mouseSensitivity = mouseSensitivityData.value;
        
    }

    private void OnApplicationQuit()
    {
        tmpText.text = "testing on exit";
    }

    private void OnEnable()
    {
        if (leftWheelAction) leftWheelAction.action.Enable();
        if (rightWheelAction) rightWheelAction.action.Enable();
        if (scrollAction) scrollAction.action.Enable();
        if (cameraAction) cameraAction.action.Enable();
        
        if(interactAction) interactAction.action.Enable();

        if (pickupAction) pickupAction.action.Enable(); 

        if (leftWheelAction)
        {
            leftWheelAction.action.performed += OnLeftPressed;
            leftWheelAction.action.canceled  += OnLeftReleased;
        }
        if (rightWheelAction)
        {
            rightWheelAction.action.performed += OnRightPressed;
            rightWheelAction.action.canceled  += OnRightReleased;
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
            leftWheelAction.action.canceled  -= OnLeftReleased;
            leftWheelAction.action.Disable();
        }
        if (rightWheelAction)
        {
            rightWheelAction.action.performed -= OnRightPressed;
            rightWheelAction.action.canceled  -= OnRightReleased;
            rightWheelAction.action.Disable();
        }
        if (scrollAction)
        {
            scrollAction.action.performed -= OnScroll;
            scrollAction.action.Disable();
        }
        if (cameraAction)
        {
            cameraAction.action.Disable();
        }
        
        if(interactAction) {interactAction.action.Disable();}
        
        
    }

    private void Update()
    {
        HandleCameraLook();
        FollowCameraToChair();
        CameraDetection(); // would like to handel in fixed update but player interactions will not work correctly then
        
        SetmouseSensitivity();
        
        if (isToggled && heldObject != null)
        {
            MoveHeldObject();
        }
        
    }

    // ---------------- Movement Logic ----------------
    private void OnLeftPressed(InputAction.CallbackContext ctx)  => leftWheelActive  = true;
    private void OnLeftReleased(InputAction.CallbackContext ctx) => leftWheelActive  = false;
    private void OnRightPressed(InputAction.CallbackContext ctx) => rightWheelActive = true;
    private void OnRightReleased(InputAction.CallbackContext ctx)=> rightWheelActive = false;

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
            if (soundCounter >= numberoftimesToInvoke) 
            {
                OnmoveEvent.Invoke();
                soundCounter = 0;
            }
        }
        else if (leftWheelActive)
        {
            if (Mathf.Abs(rb.angularVelocity.y) < maxTurnSpeed)
                rb.AddTorque(Vector3.up * scroll * turnForce, ForceMode.Force);
            if (soundCounter >= numberoftimesToInvoke) 
            { 
                OnmoveEvent.Invoke(); 
                soundCounter = 0; 
            }
        }
        else if (rightWheelActive)
        {
            if (Mathf.Abs(rb.angularVelocity.y) < maxTurnSpeed)
                rb.AddTorque(Vector3.up * -scroll * turnForce, ForceMode.Force);
            if (soundCounter >= numberoftimesToInvoke)
            {
                OnmoveEvent.Invoke();
                soundCounter = 0; 
            }
        }
    }

    // ---------------- Camera Control ----------------
    private void HandleCameraLook()
    {
        Vector2 lookInput = cameraAction.action.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVertical, maxVertical);
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -maxHorizontal, maxHorizontal);

        // camera rotation (independent of chair)
        Quaternion camRot = Quaternion.Euler(xRotation, transform.eulerAngles.y + yRotation, 0f);
        mainCamera.transform.rotation = camRot;
    }

    private void FollowCameraToChair()
    {
        // follow position smoothly
        Vector3 targetPos = transform.position + cameraOffset;
        playerCamera.position = Vector3.Lerp(
            playerCamera.position, targetPos, Time.deltaTime * cameraFollowSmooth
        );
    }

    // ---------------- Detection (Ray cast) ----------------
    private void CameraDetection()
    {
        if (!mainCamera) return;
        Vector3 fwd = mainCamera.transform.forward;

        if (Physics.Raycast(mainCamera.transform.position, fwd, out RaycastHit hit, detectionRange))
        {
            Debug.DrawRay(mainCamera.transform.position, fwd * hit.distance, Color.red);

            Interactable interactable = hit.collider.GetComponent<Interactable>();
            MeshRenderer meshRenderer = hit.collider.GetComponent<MeshRenderer>();
            LookAT lookAt = hit.collider.GetComponent<LookAT>();
            
            // ===================== highlight object =====================

            if (meshRenderer && interactable)
            {
                if (meshRenderer != lastHighlightedRenderer)
                {
                    // Remove outline from previous
                    if (lastHighlightedRenderer != null)
                    {
                        RemoveOutline(lastHighlightedRenderer);
                    }

                    // Add outline to new
                    AddOutline(meshRenderer);
                    lastHighlightedRenderer = meshRenderer;
                    
                    // Enable the new interactable's canvas
                    if (interactable)
                    {
                        tmpText.text = "Interact\n   (E)";
                    }
                }
            }

            // ===================== interact with object =====================
            
            if (interactable && interactAction.action.WasPressedThisFrame())
            {
                Debug.DrawRay(mainCamera.transform.position, fwd * detectionRange, Color.blue);
                Debug.Log("Interacting with: " + hit.collider.name);
                interactable.Interact();
            }

            // ===================== look at object =====================
            
            if (lookAt && hit.collider.CompareTag("Look-at"))
            {
                // only trigger once per new object
                if (hit.collider.gameObject != lastLookAtObject)
                {
                    lookAt.LookedAt();
                    lastLookAtObject = hit.collider.gameObject; // remember it
                }
            }
            else
            {
                lastLookAtObject = null;
            }
            
            // ===================== pick up object =====================
            
            if (pickupAction.action.WasPressedThisFrame() && hit.collider.CompareTag("Pick-Up")) // need to add a tag check here to only pick up certain objects
            {
                GameObject hitObject = hit.collider.gameObject;
                Rigidbody rbhit = hitObject.GetComponent<Rigidbody>();
                
                isToggled = !isToggled;
                
                if (isToggled)
                {
                    heldObject = rbhit;
                    //Debug.Log("pickup action pressed");
                    // make hit object a child of the camera and remove physics by setting isKinematic to true or turning off gravity
                    hit.transform.SetParent(parentObject.transform); // might need to remove this because I don't think its needed
                    rbhit.useGravity = false;
                    
                }
                else
                {
                    //Debug.Log("pickup action released");
                    // release object from camera and re-enable physics by setting isKinematic to false or turning on gravity and apply force if needed
                    rbhit.useGravity = true;
                    hitObject.transform.SetParent(null); // might need to remove this because I don't think its needed
                    rbhit.AddForce(mainCamera.transform.forward * throwForce, ForceMode.Impulse);
                    heldObject = null;
                }
            }
            
        }
        else
        {
            Debug.DrawRay(mainCamera.transform.position, fwd * detectionRange, Color.green);
            if (lastHighlightedRenderer != null)
            {
                RemoveOutline(lastHighlightedRenderer);
                lastHighlightedRenderer = null;
                
            }
            
            tmpText.text = "";
            
        }
    }

    private void MoveHeldObject()
    {
        // for moving the held object to follow the hold point in front of the camera smoothly
        Vector3 targetPos = holdPoint.position + holdPoint.forward * holdDistance;

        heldObject.MovePosition(
            Vector3.Lerp(heldObject.position, targetPos, Time.deltaTime * followSpeed)
        );
        
    }
    
    // Add outline material temporarily
    private void AddOutline(MeshRenderer renderer)
    {
        var mats = renderer.sharedMaterials;
        var newMats = new Material[mats.Length + 1];
        mats.CopyTo(newMats, 0);
        newMats[mats.Length] = highlightMaterial;
        renderer.materials = newMats;
    }

    // Remove outline material
    private void RemoveOutline(MeshRenderer renderer)
    {
        var mats = renderer.sharedMaterials;
        if (mats.Length > 1 && mats[mats.Length - 1] == highlightMaterial)
        {
            var newMats = new Material[mats.Length - 1];
            for (int i = 0; i < newMats.Length; i++)
                newMats[i] = mats[i];
            renderer.materials = newMats;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Control : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private bool useStamina = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 7.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter= new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultiplier = 5;
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5;
    [SerializeField] private float staminaValueIncrement = 2;
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    private float currentStamina;
    private Coroutine regeneratingStamina;
    public static Action<float> OnStaminaChange;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Footsteps Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] groundClips = default;
    [SerializeField] private AudioClip[] woodClips = default;
    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : IsSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 90.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 90.0f;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] LayerMask interactionLayer = default;
    private Interactuable currentInteractable;

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canCrouch)
                HandleCrouch();

            if (canUseHeadbob)
                HandleHeadbob();

            if (useFootsteps)
                Handle_Footsteps();

            if (canInteract)
            {
                HandleInteractionCheck();
                HandleInteractionInput();
            }

            if (useStamina)
                HandleStamina();

            ApplyFinalMovements(); 
        }
    }

    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal")); ;

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);

        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    private void HandleStamina()
    {
        if(IsSprinting && currentInput != Vector2.zero)
        {
            if(regeneratingStamina != null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }

            currentStamina -= staminaUseMultiplier * Time.deltaTime;

            if (currentStamina < 0)
                currentStamina = 0;

            OnStaminaChange?.Invoke(currentStamina); 

            if (currentStamina <= 0)
            {
                canSprint = false;
                print("Te cansaste");
            }
            
        }

        if(!IsSprinting && currentStamina < maxStamina && regeneratingStamina == null)
        {
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }

    private void HandleInteractionCheck()
    {
        if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            if(hit.collider.gameObject.layer == 6 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currentInteractable);

                //if (currentInteractable)
                    
            }
        }
        else if ( currentInteractable)
        {
            
        }
    }

    private void HandleInteractionInput()
    {
        if(Input.GetKeyDown(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            
        }
    }

    private void Handle_Footsteps()
    {
        if (!characterController.isGrounded) return;

        if (currentInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if(footstepTimer <= 0)
        {
            if(Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    case "Ground":
                        footstepAudioSource.PlayOneShot(groundClips[UnityEngine.Random.Range(0, groundClips.Length - 1)]);
                        break;

                    case "wood":
                        footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length - 1)]);
                        break;

                    default:
                        footstepAudioSource.PlayOneShot(groundClips[UnityEngine.Random.Range(0, groundClips.Length - 1)]);
                        break;
                }
            }

            footstepTimer = GetCurrentOffset;
        }

    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center; ;

        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while(currentStamina < maxStamina)
        {
            if (currentStamina > 0)
                canSprint = true;

            currentStamina += staminaValueIncrement;

            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
                print("Puedes correr");
            }
                

            OnStaminaChange?.Invoke(currentStamina);
           
            yield return timeToWait;
        }

        regeneratingStamina = null;
    }
}
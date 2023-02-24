using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private PlayerInput playerInput = null;
    private CharacterController controller;
    private Animator animator;
    InputMaster master;
    InputMaster.GameplayActions gameplayMovement;

    Vector2 horizontalInput;

    [SerializeField]
    private float speed = 4;

    [SerializeField]
    private float jumpHeight = 2f;
    bool jump;

    [SerializeField]
    private float gravity = -30.0f;

    [SerializeField]
    LayerMask groundMask;

    [SerializeField]
    private bool isGrounded = false;

    Vector3 verticalVelocity = Vector3.zero;

    private Vector3 currentInputVector;
    private Vector3 currentFallVector;
    private Vector3 smoothFallVelocity;
    private Vector3 smoothInputVeloctiy;
    [SerializeField] private float smoothInputSpeed = 0.1f;

    public PlayerInput PlayerInput => playerInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        master = new InputMaster();
        gameplayMovement = master.Gameplay;

        gameplayMovement.Movement.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>();
        gameplayMovement.Jump.performed += _ => JumpInput();
    }

    private void OnEnable()
    {
        master.Enable();
    }

    private void OnDisable()
    {
        master.Disable();
    }

    void Update()
    {
        if (IsOwner && IsClient)
        {
            isGrounded = Physics.CheckSphere(transform.position, 0.1f, groundMask);
            if (isGrounded)
            {
                verticalVelocity.y = 0;
            }

            Vector3 horizontalVelocity = speed * ((transform.right * horizontalInput.x) + (transform.forward * horizontalInput.y));
            currentInputVector = Vector3.SmoothDamp(currentInputVector, horizontalVelocity, ref smoothInputVeloctiy, smoothInputSpeed);
            controller.Move(currentInputVector * Time.deltaTime);

            if (jump)
            {
                if (isGrounded)
                {
                    verticalVelocity.y = Mathf.Sqrt(-2 * jumpHeight * gravity);
                }
                jump = false;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            currentFallVector = Vector3.SmoothDamp(currentFallVector, verticalVelocity, ref smoothFallVelocity, smoothInputSpeed);
            controller.Move(verticalVelocity * Time.deltaTime);
        }
    }

    private void JumpInput()
    {
        jump = true;
    }
}

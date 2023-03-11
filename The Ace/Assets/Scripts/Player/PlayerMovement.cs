using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController controller;
    private Animator animator;
    private InputMaster gameActions;

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
    public bool isGrounded = false;

    [SerializeField]
    private float groundOffset;

    Vector3 verticalVelocity = Vector3.zero;

    private Vector3 currentInputVector;
    private Vector3 currentFallVector;
    private Vector3 smoothFallVelocity;
    private Vector3 smoothInputVeloctiy;
    [SerializeField] private float smoothInputSpeed = 0.1f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        gameActions = KeybindManager.inputActions;
        gameActions.Gameplay.Jump.started += JumpInput;
        gameActions.Gameplay.Enable();
    }

    void Update()
    {
        if (IsOwner)
        {
            Vector2 input = gameActions.Gameplay.Movement.ReadValue<Vector2>();

            if (input.y > 0 || input.y > 0 && input.x != 0 || input.y > 0 && input.x == 0)
            {
                animator.SetInteger("WalkVertical", 1);
                AnimateServerRpc(1, 0);
            }
            else if (input.y < 0 || input.y < 0 && input.x != 0 || input.y > 0 && input.x == 0)
            {
                animator.SetInteger("WalkVertical", -1);
                AnimateServerRpc(-1, 0);
            }
            else if (input.y == 0 && input.x == 0)
            {
                animator.SetInteger("WalkVertical", 0);
                animator.SetInteger("WalkHorizontal", 0);
                AnimateServerRpc(0, 0);
            }
            else if (input.x > 0 && input.y == 0)
            {
                animator.SetInteger("WalkHorizontal", 1);
                AnimateServerRpc(0, 1);
            }
            else if (input.x < 0 && input.y == 0)
            {
                animator.SetInteger("WalkHorizontal", -1);
                AnimateServerRpc(0, -1);
            }


            isGrounded = Physics.Raycast(transform.position, -transform.up, groundOffset, groundMask);
            if (isGrounded)
            {
                verticalVelocity.y = 0;
            }

            Vector3 horizontalVelocity = speed * ((transform.right * input.x) + (transform.forward * input.y));
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

    private void JumpInput(InputAction.CallbackContext obj)
    {
        jump = true;
    }

    [ServerRpc] private void AnimateServerRpc(int verticalValue, int horizontalValue)
    {
        AnimateClientRpc(verticalValue, horizontalValue);
    }

    [ClientRpc] private void AnimateClientRpc(int verticalValue, int horizontalValue)
    {
        if (!IsOwner)
        {
            animator.SetInteger("WalkVertical", verticalValue);
            animator.SetInteger("WalkHorizontal", horizontalValue);
        }
    }
}

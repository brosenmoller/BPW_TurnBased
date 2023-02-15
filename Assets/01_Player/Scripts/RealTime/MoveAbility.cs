using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAbility : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;

    private Rigidbody2D rigidBody2D;
    private Vector2 movement;

    private Controls controls;

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        controls = new Controls();
        controls.Enable();
        controls.Default.Movement.performed += SetMovement;
        controls.Default.Movement.canceled += ResetMovement;
    }

    public void OnDisable()
    {
        controls.Default.Movement.performed -= SetMovement;
        controls.Default.Movement.canceled -= ResetMovement;
        controls.Disable();
    }

    public void SetMovement(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
        //animator.SetFloat("XMove", movement.x);
        //animator.SetFloat("YMove", movement.y);
        //animator.SetBool("IsWalking", true);
    }

    public void ResetMovement(InputAction.CallbackContext context)
    {
        //animator.SetBool("IsWalking", false);
        movement = Vector2.zero;
    }

    private void FixedUpdate()
    {
        rigidBody2D.velocity = speed * Time.deltaTime * movement.normalized;
    }
}


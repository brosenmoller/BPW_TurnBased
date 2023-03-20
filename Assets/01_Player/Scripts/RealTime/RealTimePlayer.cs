using UnityEngine;
using UnityEngine.InputSystem;

public class RealTimePlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;

    private Rigidbody2D rigidBody2D;
    private Vector2 movement;

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameManager.InputManager.controls.Default.Movement.performed += SetMovement;
        GameManager.InputManager.controls.Default.Movement.canceled += ResetMovement;
    }

    public void OnDisable()
    {
        GameManager.InputManager.controls.Default.Movement.performed -= SetMovement;
        GameManager.InputManager.controls.Default.Movement.canceled -= ResetMovement;
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


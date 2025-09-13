using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds Params")]
    public float runSpeed = 10f;
    public float walkSpeed = 5f;

    [Header("Params")]
    public float jumpForce = 5f;

    [Header("References")]
    public InputActionReference jump;
    public InputActionReference run;
    public InputActionReference move;
    public Ground isGround;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float speed;

    private bool _isRuning = false;
    private bool _isGround = false;

    void Awake()
    {
        speed = walkSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        move.action.Enable();

        run.action.started += RunHandler;
        run.action.canceled += RunHandler;

        jump.action.Enable();
        jump.action.started += JumpHandler;
    }

    void Start()
    {
        isGround.OnGround += UpdateGround;
    }

    void Update()
    {
        moveInput = move.action.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 targetPos = rb.position + transform.TransformDirection(moveDir) * speed * Time.fixedDeltaTime;

        rb.MovePosition(targetPos);
    }

    void JumpHandler(InputAction.CallbackContext ctx)
    {
        if (_isGround)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void RunHandler(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            speed = runSpeed;
            _isRuning = true;
        }
        else if (ctx.canceled)
        {
            speed = walkSpeed;
            _isRuning = false;
        }
    }

    void UpdateGround(bool isGround)
    {
        _isGround = isGround;
    }
}
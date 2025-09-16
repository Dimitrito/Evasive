using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds Params")]
    public float runSpeed = 10f;
    public float walkSpeed = 5f;
    public float croachSpeed = 2.5f;

    [Header("Params")]
    public float jumpForce = 5f;
    public float croachY = 0.7f;

    [Header("References")]
    public InputActionReference jump;
    public InputActionReference run;
    public InputActionReference move;
    public InputActionReference croach;
    public Ground isGround;

    private Rigidbody rb;
    private Vector2 moveInput;
    private float speed;

    private bool _isRuning = false;
    private bool _isCroach = false;
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

        croach.action.started += CroachHandler;
        croach.action.canceled += CroachHandler;

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
        if (ctx.started && !_isCroach)
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

    void CroachHandler(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _isRuning = false;
            speed = croachSpeed;

            float bottomY = transform.position.y - (transform.localScale.y / 2f);

            Vector3 scale = transform.localScale;
            scale.y = croachY;
            transform.localScale = scale;

            Vector3 pos = transform.position;
            pos.y = bottomY + (transform.localScale.y / 2f);
            transform.position = pos;

            _isCroach = true;
        }
        else if (ctx.canceled)
        {
            speed = walkSpeed;
            Vector3 scale = transform.localScale;
            scale.y = 1f;
            transform.localScale = scale;
            _isCroach = false;
        }
    }

    void UpdateGround(bool isGround)
    {
        _isGround = isGround;
    }
}
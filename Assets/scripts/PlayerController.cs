using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runSpeedMultiplier = 1.5f;

    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;

    [Header("Game")]
    [SerializeField] GameController gameController;

    Rigidbody2D rb;
    InputActionMap playerMap;
    InputAction moveAction;
    InputAction interactAction;
    InputAction sprintAction;
    Vector2 lastMoveDirection = Vector2.down;

    void Awake()
    {
        if (gameController == null)
            gameController = GameController.Instance;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        if (inputActions != null)
        {
            playerMap = inputActions.FindActionMap("Player");
            moveAction = playerMap.FindAction("Move");
            interactAction = playerMap.FindAction("Interact");
            sprintAction = playerMap.FindAction("Sprint");
        }
    }

    void OnEnable()
    {
        playerMap?.Enable();
    }

    void OnDisable()
    {
        playerMap?.Disable();
    }

    void Start()
    {
        if (rb != null)
            rb.gravityScale = 0f;
    }

    void Update()
    {
        if (!CanAct())
            return;
        if (interactAction != null && interactAction.triggered)
            OnInteract();
    }

    void FixedUpdate()
    {
        if (!CanAct() || moveAction == null || rb == null)
            return;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDirection = moveInput.normalized;

        float speed = moveSpeed;
        if (sprintAction != null && sprintAction.ReadValue<float>() > 0f)
            speed *= runSpeedMultiplier;

        Vector2 direction = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : Vector2.zero;
        rb.linearVelocity = direction * speed;

        UpdateFacing();
    }

    void UpdateFacing()
    {
        if (lastMoveDirection.sqrMagnitude < 0.01f)
            return;
        float scaleX = lastMoveDirection.x < 0 ? -1f : 1f;
        transform.localScale = new Vector3(scaleX, 1f, 1f);
    }

    bool CanAct()
    {
        if (gameController == null)
            return true;
        return gameController.CurrentState == GameController.GameState.Playing;
    }

    void OnInteract()
    {
        Debug.Log("Interact (hablar/examinar)");
    }
}

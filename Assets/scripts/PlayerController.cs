using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runSpeedMultiplier = 1.5f;

    [Header("Input")]
    [SerializeField] InputActionAsset inputActions;

    [Header("Game")]
    [SerializeField] GameController gameController;
    [SerializeField] float interactRadius = 1.5f;

    [Header("Audio")]
    [Tooltip("Sonido de pasos mientras el personaje camina.")]
    [SerializeField] AudioClip pasos;
    [Tooltip("Volumen del sonido de pasos (0-1).")]
    [SerializeField] [Range(0f, 1f)] float volumePasos = 1f;
    [Tooltip("AudioSource del jugador para los pasos. Si no se asigna, se usa el del mismo GameObject.")]
    [SerializeField] AudioSource audioPasos;

    Rigidbody2D rb;
    InputActionMap playerMap;
    InputAction moveAction;
    InputAction interactAction;
    InputAction sprintAction;
    Vector2 lastMoveDirection = Vector2.down;
    Vector3 baseScale;

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
        baseScale = transform.localScale;
        if (audioPasos == null)
            audioPasos = GetComponent<AudioSource>();
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
        // Con diálogo o secuencia pantalla completa (ventana/trompada), desactivar Move y Sprint
        if (gameController != null && (gameController.IsInDialogue || gameController.IsInVentanaSequence))
        {
            moveAction?.Disable();
            sprintAction?.Disable();
        }
        else
        {
            moveAction?.Enable();
            sprintAction?.Enable();
        }

        if (interactAction != null && interactAction.triggered)
            OnInteract();
        if (!CanAct())
            return;
    }

    void FixedUpdate()
    {
        if (!CanAct())
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            UpdatePasos(false);
            return;
        }
        if (gameController != null && gameController.IsInVentanaSequence)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            UpdatePasos(false);
            return;
        }
        if (moveAction == null || rb == null)
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
        UpdatePasos(direction.sqrMagnitude > 0.01f);
    }

    void UpdatePasos(bool walking)
    {
        if (audioPasos == null || pasos == null)
            return;
        if (walking)
        {
            if (!audioPasos.isPlaying)
            {
                audioPasos.clip = pasos;
                audioPasos.loop = true;
                audioPasos.volume = volumePasos;
                audioPasos.Play();
            }
        }
        else
        {
            audioPasos.Stop();
        }
    }

    void UpdateFacing()
    {
        if (lastMoveDirection.sqrMagnitude < 0.01f)
            return;
        float signX = lastMoveDirection.x < 0 ? -1f : 1f;
        transform.localScale = new Vector3(baseScale.x * signX, baseScale.y, baseScale.z);
    }

    bool CanAct()
    {
        if (gameController == null)
            return true;
        return gameController.CurrentState == GameController.GameState.Playing && !gameController.IsInDialogue;
    }

    void OnInteract()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue)
        {
            DialogueManager.Instance.Advance();
            return;
        }

        var hits = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        NpcController nearest = null;
        float nearestSq = float.MaxValue;
        foreach (var col in hits)
        {
            var npc = col.GetComponent<NpcController>();
            if (npc == null)
                continue;
            float sq = (npc.transform.position - transform.position).sqrMagnitude;
            if (sq < nearestSq)
            {
                nearestSq = sq;
                nearest = npc;
            }
        }
        if (nearest != null)
            nearest.StartDialogue();
    }
}

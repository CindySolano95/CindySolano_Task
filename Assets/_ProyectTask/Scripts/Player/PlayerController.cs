using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Movement")]
    [SerializeField] private float speed = 4.0f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header("Inventory")]
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private InventoryDatabase inventoryDatabase;

    [Header("Held Item Visual")]
    [SerializeField] private Transform itemAttachBone;
    [Tooltip("if true, the player will show the icon of the last picked item in their hand.")]
    [SerializeField] private bool autoHoldLastPickedItem = true;

    [Tooltip("Sorting order for the held icon SpriteRenderer.")]
    [SerializeField] private int heldSpriteSortingOrder = 10;

    // Animator parameter hashes (same names as your original script)
    private readonly int dirXHash = Animator.StringToHash("DirX");
    private readonly int dirYHash = Animator.StringToHash("DirY");
    private readonly int speedHash = Animator.StringToHash("Speed");

    private InputAction moveAction;
    private Vector2 currentLookDirection = Vector2.right;

    // Held visual runtime
    private SpriteRenderer heldSpriteRenderer;


    private void Awake()
    {
        // Safety checks 
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        EnsureHeldVisualExists();
        ClearHeldItemVisual();
    }

    private void OnEnable()
    {
        // The Move action is polled in FixedUpdate (same flow as the original script)
        moveAction = inputActions != null ? inputActions.FindAction("Gameplay/Move") : null;

        if (moveAction != null) moveAction.Enable();
        else Debug.LogWarning("[PlayerController] Gameplay/Move action not found in InputActionAsset.");
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.Disable();
    }

    private void FixedUpdate()
    {
        if (moveAction == null || rb == null) return;

        Vector2 move = moveAction.ReadValue<Vector2>();

        // Update look direction based on movement
        if (move != Vector2.zero)
            SetLookDirectionFrom(move);

        Vector2 movement = move * speed;
        float animSpeed = movement.sqrMagnitude;

        if (animator != null)
        {
            animator.SetFloat(dirXHash, currentLookDirection.x);
            animator.SetFloat(dirYHash, currentLookDirection.y);
            animator.SetFloat(speedHash, animSpeed);
        }

        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }

    private void SetLookDirectionFrom(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            currentLookDirection = direction.x > 0 ? Vector2.right : Vector2.left;
        else
            currentLookDirection = direction.y > 0 ? Vector2.up : Vector2.down;
    }

    // ---------------------------
    // PICKUP
    // ---------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        var pickup = other.GetComponent<WorldPickupItem>();
        if (pickup == null) return;

        TryPickup(pickup);
    }

    private void TryPickup(WorldPickupItem pickup)
    {
        if (pickup == null) return;

        // Add to your inventory system
        if (inventoryController != null)
            inventoryController.AddItem(pickup.ItemId, pickup.Amount);
        else
            Debug.LogWarning("[PlayerController] InventoryController is not assigned.");

        // Optionally show held icon (no equip system required)
        if (autoHoldLastPickedItem)
            SetHeldItemVisualFromDatabase(pickup.ItemId);

        pickup.Consume();
    }

    // ---------------------------
    // HELD ITEM VISUAL
    // ---------------------------

    private void EnsureHeldVisualExists()
    {
        if (itemAttachBone == null) return;

        // Reuse existing renderer if already created
        heldSpriteRenderer = itemAttachBone.GetComponentInChildren<SpriteRenderer>();
        if (heldSpriteRenderer != null) return;

        // Create a child object to render the held icon
        var go = new GameObject("HeldItemVisual");
        go.transform.SetParent(itemAttachBone, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        heldSpriteRenderer = go.AddComponent<SpriteRenderer>();
        heldSpriteRenderer.sortingOrder = heldSpriteSortingOrder;
    }

    public void SetHeldItemVisualFromDatabase(int itemId)
    {
        if (itemAttachBone == null)
        {
            Debug.LogWarning("[PlayerController] ItemAttachBone is not assigned.");
            return;
        }

        EnsureHeldVisualExists();

        if (inventoryDatabase == null)
        {
            Debug.LogWarning("[PlayerController] InventoryDatabase is not assigned.");
            return;
        }

        if (!inventoryDatabase.TryGet(itemId, out var def))
        {
            Debug.LogWarning($"[PlayerController] Cannot set held visual. Unknown item id: {itemId}");
            return;
        }

        heldSpriteRenderer.sprite = def.icon;
        heldSpriteRenderer.enabled = def.icon != null;
    }

    public void ClearHeldItemVisual()
    {
        if (heldSpriteRenderer != null)
        {
            heldSpriteRenderer.sprite = null;
            heldSpriteRenderer.enabled = false;
        }
    }
}

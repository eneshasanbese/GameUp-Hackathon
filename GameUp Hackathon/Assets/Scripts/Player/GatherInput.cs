using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-300)]
[DisallowMultipleComponent]
public sealed class GatherInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private InputActionAsset actions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private bool jumpRequested;
    private bool jumpReleaseRequested;

    public float horizontalInput => moveAction != null && moveAction.enabled
        ? Mathf.Clamp(moveAction.ReadValue<Vector2>().x, -1f, 1f)
        : 0f;

    private void Awake()
    {
        if (inputActions == null || inputActions.FindAction("Player/Move") == null ||
            inputActions.FindAction("Player/Jump") == null)
        {
            Debug.LogError("GatherInput needs an Input Action Asset with Player/Move (Vector2) and Player/Jump (Button).", this);
            enabled = false;
            return;
        }

        // Own the actions so disabling this player does not disable other consumers.
        actions = Instantiate(inputActions);
        moveAction = actions.FindAction("Player/Move", true);
        jumpAction = actions.FindAction("Player/Jump", true);
        jumpAction.performed += RequestJump;
        jumpAction.canceled += ReleaseJump;
    }

    private void OnEnable()
    {
        EnablePlayerMap();
    }

    private void OnDisable()
    {
        DisablePlayerMap();
    }

    private void OnDestroy()
    {
        if (actions == null)
            return;

        DisablePlayerMap();
        jumpAction.performed -= RequestJump;
        jumpAction.canceled -= ReleaseJump;
        Destroy(actions);
    }

    public void EnablePlayerMap()
    {
        if (!isActiveAndEnabled || actions == null)
            return;

        jumpRequested = false;
        jumpReleaseRequested = false;
        moveAction.Enable();
        jumpAction.Enable();
    }

    public void DisablePlayerMap()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        jumpRequested = false;
        jumpReleaseRequested = false;
    }

    public bool ConsumeJump()
    {
        // Preserve presses between physics ticks, then consume each press once.
        bool requested = jumpRequested;
        jumpRequested = false;
        return requested;
    }

    public bool ConsumeJumpRelease()
    {
        bool requested = jumpReleaseRequested;
        jumpReleaseRequested = false;
        return requested;
    }

    private void RequestJump(InputAction.CallbackContext context)
    {
        jumpRequested = true;
    }

    private void ReleaseJump(InputAction.CallbackContext context)
    {
        jumpReleaseRequested = true;
    }
}

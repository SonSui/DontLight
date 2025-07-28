using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public PlayerInput PlayerInput { get; private set; }
    public InputAction MoveAction => PlayerInput.actions["Move"];
    public InputAction LookAction => PlayerInput.actions["Look"];
    public InputAction ThrowAction => PlayerInput.actions["Throw"];
    public string CurrentControlScheme => PlayerInput.currentControlScheme;
    public bool IsUsingGamepad { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerInput = GetComponent<PlayerInput>();
        PlayerInput.onControlsChanged += OnControlsChanged;
        IsUsingGamepad = PlayerInput.currentControlScheme == "Gamepad";
    }

    private void OnControlsChanged(PlayerInput input)
    {
        IsUsingGamepad = input.currentControlScheme == "Gamepad";
        Debug.Log($"[InputManager] 控制方式变更为: {input.currentControlScheme}");
    }
}
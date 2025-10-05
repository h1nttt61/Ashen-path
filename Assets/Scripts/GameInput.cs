using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputAction playerInputAction;

    private void Awake()
    {
        Instance = this;
        playerInputAction = new PlayerInputAction();
        playerInputAction.Enable();
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputAction.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public bool IsJumpPressed() => playerInputAction.Player.Jump.ReadValue<float>() > 0.1f;

    public bool WasJumpPressedThisFrame() => playerInputAction.Player.Jump.triggered;

}

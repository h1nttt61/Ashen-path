using UnityEngine;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputAction playerInputAction;

    public event EventHandler OnPlayerDash;

    private void Awake()
    {
        Instance = this;
        playerInputAction = new PlayerInputAction();
        playerInputAction.Enable();
        playerInputAction.Player.Dash.performed +=Dash_performed;
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPlayerDash?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputAction.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public bool IsJumpPressed() => playerInputAction.Player.Jump.ReadValue<float>() > 0.1f;

    public bool WasJumpPressedThisFrame() => playerInputAction.Player.Jump.triggered;

}

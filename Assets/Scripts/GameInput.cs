using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputAction playerInputAction;

    public event EventHandler OnPlayerDash;

    public event EventHandler OnPlayerAttack;

    public event EventHandler OnPlayerHealHoldStarted;

    public event EventHandler OnPlayerHealHoldEnded;


    private void Awake()
    {
        Instance = this;
        playerInputAction = new PlayerInputAction();
        playerInputAction.Enable();

        playerInputAction.Combat.Attack.started += Attack_started;
        playerInputAction.Player.Dash.performed += Dash_performed;
    }


    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            OnPlayerHealHoldStarted?.Invoke(this, EventArgs.Empty);
        }
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            OnPlayerHealHoldEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Attack_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
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

using UnityEngine;
using UnityEngine.InputSystem;

public class Inputs : MonoBehaviour
{
    [SerializeField] protected InputActionAsset playerInputAsset;
    [SerializeField] protected string actionMap;
    [Space]
    [SerializeField] protected string grappleName;
    [SerializeField] protected string chargeName;
    [SerializeField] protected string primaryName;
    [SerializeField] protected string secondaryName;
    [SerializeField] protected string joystickName;

    protected InputAction gripAction, triggerAction, primaryAction, secondaryAction, joystickAction;
    private float gripValue, triggerValue, primaryValue, secondaryValue;
    private Vector2 joystickValue;

    protected void SetInputs()
    {
        var map = playerInputAsset.FindActionMap(actionMap);

        gripAction = map.FindAction(grappleName);
        gripAction.performed += OnGripChanged;
        gripAction.canceled += OnGripChanged;
        gripAction.Enable();

        triggerAction = map.FindAction(chargeName);
        triggerAction.performed += OnTriggerChanged;
        triggerAction.canceled += OnTriggerChanged;
        triggerAction.Enable();

        primaryAction = map.FindAction(primaryName);
        primaryAction.performed += OnPrimaryChanged;
        primaryAction.canceled += OnPrimaryChanged;
        primaryAction.Enable();

        secondaryAction = map.FindAction(secondaryName);
        secondaryAction.performed += OnSecondaryChanged;
        secondaryAction.canceled += OnSecondaryChanged;
        secondaryAction.Enable();

        joystickAction = map.FindAction(joystickName);
        joystickAction.performed += OnJoystickChanged;
        joystickAction.canceled += OnJoystickChanged;
        joystickAction.Enable();
    }

    private void OnGripChanged(InputAction.CallbackContext context)
    {
        gripValue = context.ReadValue<float>();
    }
    private void OnTriggerChanged(InputAction.CallbackContext context)
    {
        triggerValue = context.ReadValue<float>();
    }
    private void OnPrimaryChanged(InputAction.CallbackContext context)
    {
        primaryValue = context.ReadValue<float>();
    }
    private void OnSecondaryChanged(InputAction.CallbackContext context)
    {
        secondaryValue = context.ReadValue<float>();
    }

    private void OnJoystickChanged(InputAction.CallbackContext context)
    {
        joystickValue = context.ReadValue<Vector2>();
    }

    public float GetGrip()
    {
        return gripValue;
    }

    public float GetTrigger()
    {
        return triggerValue;
    }

    public float GetPrimary()
    {
        return primaryValue;
    }

    public float GetSecondary()
    {
        return secondaryValue;
    }

    public Vector2 GetJoystick()
    {
        return joystickValue;
    }
}
